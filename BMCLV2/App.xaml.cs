using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Net;

using BMCLV2.Lang;
using BMCLV2.Windows;

namespace BMCLV2
{
    //分支测试
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App
    {
        private FileStream _appLock;
        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
#else
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            
#endif
            try
            {
                _appLock = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "BMCL.lck", FileMode.Create);
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture));
                _appLock.Write(buffer, 0, buffer.Length);
                _appLock.Close();
                _appLock = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "BMCL.lck", FileMode.Open);
            }
            catch (IOException)
            {
                MessageBox.Show(LangManager.GetLangFromResource("StartupDuplicate"));
                MessageBox.Show(LangManager.GetLangFromResource("StartupDuplicate"));
                Environment.Exit(3);
            }
            WebRequest.DefaultWebProxy = null;  //禁用默认代理
            if (e.Args.Length == 0)   // 判断debug模式
                Logger.debug = false;
            else
                if (Array.IndexOf(e.Args, "-Debug") != -1)
                    Logger.debug = true;
            Logger.start();

            base.OnStartup(e);
        }



        protected override void OnExit(ExitEventArgs e)
        {
            _appLock.Close();
            base.OnExit(e);
            Logger.stop();
        }

// ReSharper disable once UnusedMember.Local
// ReSharper disable once UnusedParameter.Local
        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            var crash = new CrashHandle(e.Exception);
            crash.Show();
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            var crash = new CrashHandle(e.Exception);
            crash.Show();
        }
    }
}
