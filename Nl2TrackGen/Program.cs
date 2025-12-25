using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.ApplicationModel.DynamicDependency;

namespace Nl2TrackGen
{
    public static class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, System.Text.StringBuilder? packageFullName);

        [STAThread]
        static void Main(string[] args)
        {
            // Ensure ComWrappers are initialized for WinUI 3
            WinRT.ComWrappersSupport.InitializeComWrappers();

            // Check if running as unpackaged (e.g. from output directory or debugger)
            bool isPackaged = IsPackaged();
            if (!isPackaged)
            {
                // Initialize Windows App SDK for unpackaged app
                // Version 1.6 -> 0x00010006
                // This is required to use AppInstance and other WinAppSDK APIs
                try
                {
                    Bootstrap.Initialize(0x00010006);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Bootstrap.Initialize failed: {ex.Message}");
                    // Continue, it might be packaged but detection failed?
                }
            }

            try
            {
                // DecideRedirection uses AppInstance which requires Bootstrapper (if unpackaged)
                bool isRedirect = DecideRedirection();
                if (!isRedirect)
                {
                    Microsoft.UI.Xaml.Application.Start((p) =>
                    {
                        var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                        SynchronizationContext.SetSynchronizationContext(context);
                        new App();
                    });
                }
            }
            finally
            {
                if (!isPackaged)
                {
                    Bootstrap.Shutdown();
                }
            }
        }

        private static bool IsPackaged()
        {
            try
            {
                int length = 0;
                // Check if we have a package identity
                int result = GetCurrentPackageFullName(ref length, null);
                // APPMODEL_ERROR_NO_PACKAGE = 15700
                return result != 15700;
            }
            catch
            {
                return false;
            }
        }

        private static bool DecideRedirection()
        {
            // This line will fail with REGDB_E_CLASSNOTREG if Bootstrapper is not initialized in unpackaged app
            var args = AppInstance.GetCurrent().GetActivatedEventArgs();
            var key = AppInstance.FindOrRegisterForKey("main");
            if (key.IsCurrent)
            {
                return false;
            }
            else
            {
                key.RedirectActivationToAsync(args).GetAwaiter().GetResult();
                return true;
            }
        }
    }
}
