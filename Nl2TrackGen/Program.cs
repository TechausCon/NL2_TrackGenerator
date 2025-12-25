using System;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;

namespace Nl2TrackGen
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();

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
