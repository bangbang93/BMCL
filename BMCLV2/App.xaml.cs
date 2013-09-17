using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BMCLV2
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            if (e.Args.Length == 0)
                Logger.Debug = false;
            else
                if (Array.IndexOf(e.Args, "-Debug") != -1)
                    Logger.Debug = true;
            Logger.Start();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Logger.Stop();
        }

        void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            var crash = new CrashHandle(e.Exception);
            crash.Show();
        }
    }
}
