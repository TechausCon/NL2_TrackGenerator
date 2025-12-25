using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Windows.Storage.Pickers;
using Nl2TrackGen.Models;

namespace Nl2TrackGen.Services
{
    public class FileSaveService
    {
        private readonly Window _window;

        public FileSaveService(Window window)
        {
            _window = window;
        }

        public async Task SaveCsvAsync(List<TrackPoint> points, string defaultFileName = "track.csv")
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("NoLimits 2 Exchange Format", new List<string>() { ".csv", ".txt" });
            savePicker.SuggestedFileName = defaultFileName;

            // Initialize the picker with the window handle (WinUI 3 requirement)
            savePicker.InitializeWithWindow(_window);

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                var content = Nl2CsvExporter.Export(points);
                // Use WriteBytesAsync with UTF8Encoding(false) to ensure no BOM
                var bytes = new System.Text.UTF8Encoding(false).GetBytes(content);
                await Windows.Storage.FileIO.WriteBytesAsync(file, bytes);
            }
        }
    }
}
