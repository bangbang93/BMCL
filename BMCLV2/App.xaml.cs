﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BMCLV2.Util;
using BMCLV2.Windows;

namespace BMCLV2
{
    //分支测试
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App
    {
        public static EventWaitHandle ProgramStarted;

        public static bool SkipPlugin { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
          //            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, Process.GetCurrentProcess().ProcessName, out createNew);
            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, $"BMCLNG:{Process.GetCurrentProcess().MainModule.FileName.Replace('\\', '_')}", out var createNew);
            if (!createNew)
            {
                ProgramStarted.Set();
                Environment.Exit(3);
                return;
            }
            if (e.Args.Length == 0)   // 判断debug模式
                Logger.Debug = false;
            else
                if (Array.IndexOf(e.Args, "-Debug") != -1)
                    Logger.Debug = true;
            Logger.Start();
#if DEBUG
#else
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
            if (Array.IndexOf(e.Args, "-Update") != -1)
            {
                var index = Array.IndexOf(e.Args, "-Update");
                if (index < e.Args.Length - 1)
                {
                    if (!e.Args[index + 1].StartsWith("-"))
                    {
                        DoUpdate(e.Args[index + 1]);
                    }
                    else
                    {
                        DoUpdate();
                    }
                }
            }
            if (Array.IndexOf(e.Args, "-SkipPlugin") != -1)
            {
                SkipPlugin = true;
            }
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Logger.Stop();
        }

        public static void AboutToExit()
        {
            Logger.Stop();
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var resolved = new ErrorHandler(e.ExceptionObject as Exception).Resolve();
            if (resolved) return;
            var crash = new CrashHandle(e.ExceptionObject as Exception);
            crash.Show();
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            var resolved = new ErrorHandler(e.Exception).Resolve();
            if (resolved) return;
            var crash = new CrashHandle(e.Exception);
            crash.Show();
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            var resolved = new ErrorHandler(e.Exception).Resolve();
            if (resolved) return;
            var crash = new CrashHandle(e.Exception);
            crash.Show();
        }

        private void DoUpdate()
        {
            var processName = Process.GetCurrentProcess().ProcessName;
            var time = 0;
            while (time < 10)
            {
                try
                {
                    File.Copy(processName, "BMCL.exe", true);
                    Process.Start("BMCL.exe", "-Update " + processName);
                    Current.Shutdown(0);
                    return;
                }
                catch (Exception e)
                {
                    Logger.Fatal(e);
                }
                finally
                {
                    time ++;
                }
            }
            MessageBox.Show("自动升级失败，请手动使用" + processName + "替代旧版文件");
            MessageBox.Show("自动升级失败，请手动使用" + processName + "替代旧版文件");
        }

        private void DoUpdate(string fileName)
        {
            File.Delete(fileName);
        }
    }
}
