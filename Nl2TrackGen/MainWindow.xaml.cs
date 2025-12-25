using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Nl2TrackGen.ViewModels;

namespace Nl2TrackGen
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            this.InitializeComponent();
            ViewModel = new MainViewModel(this);

            // Listen to VM message request
            ViewModel.WebViewMessageRequested += ViewModel_WebViewMessageRequested;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await PreviewWebView.EnsureCoreWebView2Async();

            // HTML with Three.js via UNPKG (Module)
            string html = @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <style>body { margin: 0; overflow: hidden; background-color: #111; color: #fff; font-family: sans-serif; }</style>
    <script type='importmap'>
      {
        ""imports"": {
          ""three"": ""https://unpkg.com/three@0.160.0/build/three.module.js"",
          ""three/addons/"": ""https://unpkg.com/three@0.160.0/examples/jsm/""
        }
      }
    </script>
</head>
<body>
    <script type='module'>
        import * as THREE from 'three';
        import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

        let scene, camera, renderer, line;

        init();
        animate();

        function init() {
            scene = new THREE.Scene();
            scene.background = new THREE.Color(0x222222);

            camera = new THREE.PerspectiveCamera(60, window.innerWidth / window.innerHeight, 0.1, 5000);
            camera.position.set(50, 50, 50);

            renderer = new THREE.WebGLRenderer({ antialias: true });
            renderer.setSize(window.innerWidth, window.innerHeight);
            document.body.appendChild(renderer.domElement);

            const controls = new OrbitControls(camera, renderer.domElement);

            // Grid
            const gridHelper = new THREE.GridHelper(200, 20);
            scene.add(gridHelper);

            // Axes
            const axesHelper = new THREE.AxesHelper(10);
            scene.add(axesHelper);

            // Light
            const ambientLight = new THREE.AmbientLight(0xffffff, 0.6);
            scene.add(ambientLight);

            window.addEventListener('resize', onWindowResize);

            // Listen for messages
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.addEventListener('message', event => {
                    const msg = JSON.parse(event.data);
                    if (msg.type === 'track') {
                        updateTrack(msg.points);
                    }
                });
            }
        }

        function updateTrack(points) {
            if (line) scene.remove(line);

            const geometry = new THREE.BufferGeometry();
            const positions = [];

            points.forEach(p => {
                positions.push(p.x, p.y, p.z);
            });

            geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));

            const material = new THREE.LineBasicMaterial({ color: 0x00ff00 });
            line = new THREE.Line(geometry, material);
            scene.add(line);

            // Center camera roughly
            if (points.length > 0) {
                // Focus on middle point?
                // Or just don't move camera to let user control it
            }
        }

        function onWindowResize() {
            camera.aspect = window.innerWidth / window.innerHeight;
            camera.updateProjectionMatrix();
            renderer.setSize(window.innerWidth, window.innerHeight);
        }

        function animate() {
            requestAnimationFrame(animate);
            renderer.render(scene, camera);
        }
    </script>
</body>
</html>";
            PreviewWebView.NavigateToString(html);
        }

        private void ViewModel_WebViewMessageRequested(object sender, string json)
        {
            if (PreviewWebView.CoreWebView2 != null)
            {
                PreviewWebView.CoreWebView2.PostWebMessageAsString(json);
            }
        }
    }
}
