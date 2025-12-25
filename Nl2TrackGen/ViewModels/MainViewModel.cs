using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.Web.WebView2.Core;
using Nl2TrackGen.Models;
using Nl2TrackGen.Services;

namespace Nl2TrackGen.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly TrackGenerator _generator;
        private readonly FileSaveService _fileService;

        // Properties
        private CoasterType _selectedCoasterType;
        public CoasterType SelectedCoasterType
        {
            get => _selectedCoasterType;
            set { _selectedCoasterType = value; OnPropertyChanged(); }
        }

        private bool _isNl2Preset = true; // Default true
        public bool IsNl2Preset
        {
            get => _isNl2Preset;
            set { _isNl2Preset = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsSampleStepEnabled)); }
        }

        public bool IsSampleStepEnabled => !IsNl2Preset;

        private int _seed;
        public int Seed
        {
            get => _seed;
            set { _seed = value; OnPropertyChanged(); }
        }

        private double _targetLength = 800;
        public double TargetLength
        {
            get => _targetLength;
            set { _targetLength = value; OnPropertyChanged(); }
        }

        private double _sampleStep = 1.0;
        public double SampleStep
        {
            get => _sampleStep;
            set { _sampleStep = value; OnPropertyChanged(); }
        }

        private double _intensity = 0.5;
        public double Intensity
        {
            get => _intensity;
            set { _intensity = value; OnPropertyChanged(); }
        }

        private string _statusText = "Ready";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        private string _statsText = "";
        public string StatsText
        {
            get => _statsText;
            set { _statsText = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ElementSelection> ElementOptions { get; } = new();

        public ICommand GenerateCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand RandomSeedCommand { get; }

        private List<TrackPoint> _currentPoints = new();

        // WebView Interaction
        public event EventHandler<string>? WebViewMessageRequested;

        public MainViewModel(Window window)
        {
            _generator = new TrackGenerator();
            _fileService = new FileSaveService(window);

            GenerateCommand = new RelayCommand(Generate);
            ExportCommand = new RelayCommand(Export);
            RandomSeedCommand = new RelayCommand(() => Seed = new Random().Next());

            // Init Element Options
            foreach (TrackElementType t in Enum.GetValues(typeof(TrackElementType)))
            {
                ElementOptions.Add(new ElementSelection { Type = t, IsSelected = true });
            }
            Seed = new Random().Next();
        }

        private void Generate()
        {
            try
            {
                StatusText = "Generating...";

                var selectedElements = ElementOptions.Where(e => e.IsSelected).Select(e => e.Type).ToList();
                if (!selectedElements.Any()) selectedElements.Add(TrackElementType.Straight);

                float step = (float)SampleStep;
                var (points, actualStep) = _generator.Generate(
                    Seed,
                    (float)TargetLength,
                    SelectedCoasterType,
                    selectedElements,
                    (float)Intensity,
                    IsNl2Preset,
                    step);

                _currentPoints = points;

                // Stats
                float totalLen = 0;
                float minY = float.MaxValue, maxY = float.MinValue;
                if (points.Any())
                {
                    minY = points.Min(p => p.Position.Y);
                    maxY = points.Max(p => p.Position.Y);
                    // Approximate length
                    totalLen = points.Count * actualStep; // Rough approximation
                }

                StatsText = $"Points: {points.Count}\nEst. Length: {totalLen:F1}m\nHeight Range: {minY:F1}m .. {maxY:F1}m\nStep Used: {actualStep:F3}m\nPreset Active: {IsNl2Preset}";
                StatusText = "Generated successfully.";

                // Send to WebView
                SendTrackToView(points);
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
        }

        private async void Export()
        {
            if (_currentPoints == null || !_currentPoints.Any())
            {
                StatusText = "No track generated yet.";
                return;
            }

            try
            {
                StatusText = "Exporting...";
                await _fileService.SaveCsvAsync(_currentPoints);
                StatusText = "Export complete.";
            }
            catch (Exception ex)
            {
                StatusText = $"Export failed: {ex.Message}";
            }
        }

        private void SendTrackToView(List<TrackPoint> points)
        {
            // Simple JSON serialization for the points
            var simplePoints = points.Select(p => new { x = p.Position.X, y = p.Position.Y, z = -p.Position.Z }).ToList(); // Invert Z for ThreeJS if needed, or keep standard. NL2 Y is up. ThreeJS Y is up. Z is depth.
            // NL2: +Z is forward?
            // Let's just pass raw.

            var payload = new
            {
                type = "track",
                points = simplePoints
            };

            string json = JsonSerializer.Serialize(payload);
            WebViewMessageRequested?.Invoke(this, json);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class ElementSelection : INotifyPropertyChanged
    {
        public TrackElementType Type { get; set; }
        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected))); } }
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
    }
}
