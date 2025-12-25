using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using WinRT;

namespace Nl2TrackGen.Services
{
    public static class WinUIWindowExtensions
    {
        [ComImport, Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IInitializeWithWindow
        {
            void Initialize(IntPtr hwnd);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        public static void InitializeWithWindow(this object target, Window window)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var initializeWithWindow = target.As<IInitializeWithWindow>();
            initializeWithWindow.Initialize(hwnd);
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            return WinRT.Interop.WindowNative.GetWindowHandle(window);
        }
    }
}
