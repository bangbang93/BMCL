using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

using BMCLV2.Lang;

namespace BMCLV2
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        FileStream AppLock;
        protected override void OnStartup(StartupEventArgs e)
        {
            
            try
            {
                AppLock = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "BMCL.lck", FileMode.Create);
            }
            catch (IOException)
            {
                MessageBox.Show(LangManager.GetLangFromResource("StartupDuplicate"));
                MessageBox.Show(LangManager.GetLangFromResource("StartupDuplicate"));
                Environment.Exit(3);
            }
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
            AppLock.Close();
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
