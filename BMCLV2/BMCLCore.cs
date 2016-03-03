using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using BMCLV2.I18N;
using BMCLV2.Plugin;
using BMCLV2.Resource;
using BMCLV2.Windows;

namespace BMCLV2
{
    public static class BmclCore
    {
        public static string BmclVersion;
        public static Config Config;
        public static Dictionary<string, object> Auths = new Dictionary<string, object>();
        public static Launcher.Launcher Game;
        public static bool GameRunning = false;
        public static string UrlResourceBase = Url.URL_RESOURCE_bangbang93;
        public static string UrlLibrariesBase = Url.URL_LIBRARIES_bangbang93;
        public static NotiIcon NIcon = new NotiIcon();
        public static FrmMain MainWindow = null;
        public static Dispatcher Dispatcher = Dispatcher.CurrentDispatcher;
        public static gameinfo GameInfo;
        public static Dictionary<string, object> Language = new Dictionary<string, object>();
        public static string BaseDirectory = Environment.CurrentDirectory + '\\';
        private static readonly Application ThisApplication = Application.Current;
        private readonly static string Cfgfile = BaseDirectory + "bmcl.xml";
        private static Report _reporter;

        static BmclCore()
        {
            BmclVersion = Application.ResourceAssembly.FullName.Split('=')[1];
            BmclVersion = BmclVersion.Substring(0, BmclVersion.IndexOf(','));
            Logger.log("BMCL V3 Ver." + BmclVersion + "正在启动");
            Config = Config.Load(Cfgfile);
            if (Config.Passwd == null)
            {
                Config.Passwd = new byte[0];   //V2的密码存储兼容
            }
            Logger.log($"加载{Cfgfile}文件");
            Logger.log(Config);
            LangManager.LoadLanguage();
            LangManager.ChangeLanguage(Config.Lang);
            Logger.log("加载默认配置");
            if (!Directory.Exists(BaseDirectory + ".minecraft"))
            {
                Directory.CreateDirectory(BaseDirectory + ".minecraft");
            }
            if (Config.Javaw == "autosearch")
            {
                Config.Javaw = Config.GetJavaDir();
            }
            if (Config.Javaxmx == "autosearch")
            {
                Config.Javaxmx = (Config.GetMemory() / 4).ToString(CultureInfo.InvariantCulture);
            }
            LangManager.UseLanguage(Config.Lang);
            if (!App.SkipPlugin)
            {
                PluginManager.LoadPlugin(LangManager.GetLangFromResource("LangName"));
            }
#if DEBUG
#else
            ReleaseCheck();
#endif
        }
        
        private static void ReleaseCheck()
        {
            if (Config.Report)
            {
                _reporter = new Report();
            }
            if (Config.CheckUpdate)
            {
                var updateChecker = new UpdateChecker();
                updateChecker.CheckUpdateFinishEvent += UpdateCheckerOnCheckUpdateFinishEvent;
            }
        }

        private static void UpdateCheckerOnCheckUpdateFinishEvent(bool hasUpdate, string updateAddr, string updateInfo, int updateBuild)
        {
            if (hasUpdate)
            {
                var a = MessageBox.Show(MainWindow, updateInfo, "更新", MessageBoxButton.OKCancel,
                    MessageBoxImage.Information);
                if (a == MessageBoxResult.OK)
                {
                    var updater = new FrmUpdater(updateBuild, updateAddr);
                    updater.ShowDialog();
                }
                if (a == MessageBoxResult.No || a == MessageBoxResult.None) //若窗口直接消失
                {
                    if (MessageBox.Show(MainWindow, updateInfo, "更新", MessageBoxButton.OKCancel,
                    MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        var updater = new FrmUpdater(updateBuild, updateAddr);
                        updater.ShowDialog();
                    }
                }
            }
        }

        public static void Invoke(Delegate invoke, object[] argObjects = null)
        {
            Dispatcher.Invoke(invoke, argObjects);
        }


        public static void Halt(int code = 0)
        {
            ThisApplication.Shutdown(code);
        }

        public static void SingleInstance(Window window)
        {
            ThreadPool.RegisterWaitForSingleObject(App.ProgramStarted, OnAnotherProgramStarted, window, -1, false);
        }

        private static void OnAnotherProgramStarted(object state, bool timedout)
        {
            var window = MainWindow;
            NIcon.ShowBalloonTip(2000, LangManager.GetLangFromResource("BMCLHiddenInfo"));
            if (window != null)
            {
                Dispatcher.Invoke(window.Show);
            }
        }
    }
}
