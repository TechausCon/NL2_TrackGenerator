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
            WinRT.ComWrappersSupport.InitializeComWrappers();

            bool isPackaged = IsPackaged();
            if (!isPackaged)
            {
                // Initialize Windows App SDK for unpackaged app
                // 0x00010006 corresponds to 1.6
                Bootstrap.Initialize(0x00010006);
            }

            try
            {
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
            int length = 0;
            int result = GetCurrentPackageFullName(ref length, null);
            // APPMODEL_ERROR_NO_PACKAGE = 15700
            return result != 15700;
        }

        private static bool DecideRedirection()
        {
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
