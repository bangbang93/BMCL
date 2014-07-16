using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using BMCLV2.Lang;
using BMCLV2.Resource;
using BMCLV2.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace BMCLV2
{
    public static class BmclCore
    {
        public static String BmclVersion;
        private const string Cfgfile = "bmcl.xml";
        public static Config Config;
        public static Dictionary<string, object> Auths = new Dictionary<string, object>();
        public static Launcher.Launcher Game;
        public static bool GameRunning = false;
        public static String UrlDownloadBase = Url.URL_DOWNLOAD_bangbang93;
        public static String UrlResourceBase = Url.URL_RESOURCE_bangbang93;
        public static string UrlLibrariesBase = Url.URL_LIBRARIES_bangbang93;
        public static NotiIcon NIcon = new NotiIcon();
        public static FrmMain MainWindow = null;
        public static Dispatcher Dispatcher = Dispatcher.CurrentDispatcher;
        public static gameinfo GameInfo;
        public static Dictionary<string, object> Language = new Dictionary<string, object>();
        public static string BaseDirectory = Environment.CurrentDirectory + '\\';

        static BmclCore()
        {
            BmclVersion = Application.ResourceAssembly.FullName.Split('=')[1];
            BmclVersion = BmclVersion.Substring(0, BmclVersion.IndexOf(','));
            Logger.log("BMCL V3 Ver." + BmclVersion + "正在启动");
            if (File.Exists(Cfgfile))
            {
                Config = Config.Load(Cfgfile);
                if (Config.Passwd == null)
                {
                    Config.Passwd = new byte[0];   //V2的密码存储兼容
                }
                Logger.log(String.Format("加载{0}文件", Cfgfile));
                Logger.log(Config);
                LoadLanguage();
                ChangeLanguage(Config.Lang);
            }
            else
            {
                Config = new Config();
                Logger.log("加载默认配置");
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
                LoadPlugin(LangManager.GetLangFromResource("LangName"));
            }
#if DEBUG
#else
            ReleaseCheck();
#endif
        }

// ReSharper disable once UnusedMember.Local
        private static void ReleaseCheck()
        {
            if (BmclCore.Config.Report)
            {
// ReSharper disable once ObjectCreationAsStatement
                new Report();
            }
            if (BmclCore.Config.CheckUpdate)
            {
                var updateChecker = new UpdateChecker();
                updateChecker.CheckUpdateFinishEvent += UpdateCheckerOnCheckUpdateFinishEvent;
            }
        }

        private static void UpdateCheckerOnCheckUpdateFinishEvent(bool hasUpdate, string updateAddr, string updateInfo, int updateBuild)
        {
            if (hasUpdate)
            {
                var a = MessageBox.Show(BmclCore.MainWindow, updateInfo, "更新", MessageBoxButton.OKCancel,
                    MessageBoxImage.Information);
                if (a == MessageBoxResult.OK)
                {
                    var updater = new FrmUpdater(updateBuild, updateAddr);
                    updater.ShowDialog();
                }
                if (a == MessageBoxResult.No || a == MessageBoxResult.None) //若窗口直接消失
                {
                    if (MessageBox.Show(BmclCore.MainWindow, updateInfo, "更新", MessageBoxButton.OKCancel,
                    MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        var updater = new FrmUpdater(updateBuild, updateAddr);
                        updater.ShowDialog();
                    }
                }
            }
        }

        public static void ChangeLanguage(string lang)
        {
            LangManager.UseLanguage(lang);
        }

        public static void LoadPlugin(string language)
        {
            Auths.Clear();
            if (!Directory.Exists("auths")) return;
            var authplugins = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\auths");
            foreach (string auth in authplugins.Where(auth => auth.ToLower().EndsWith(".dll")))
            {
                Logger.log("尝试加载" + auth);
                try
                {
                    var authMethod = Assembly.LoadFrom(auth);
                    var types = authMethod.GetTypes();
                    foreach (var t in types)
                    {
                        if (t.GetInterface("IBmclAuthPlugin") != null)
                        {
                            try
                            {
                                var authInstance = authMethod.CreateInstance(t.FullName);
                                if (authInstance == null) continue;
                                var T = authInstance.GetType();
                                var authVer = T.GetMethod("GetVer");
                                if (authVer == null)
                                {
                                    Logger.log(String.Format("未找到{0}的GetVer方法，放弃加载", authInstance));
                                    continue;
                                }
                                if ((long)authVer.Invoke(authInstance, null) != 1)
                                {
                                    Logger.log(String.Format("{0}的版本不为1，放弃加载", authInstance));
                                    continue;
                                }
                                var authVersion = T.GetMethod("GetVersion");
                                if (authVersion == null)
                                {
                                    Logger.log(String.Format("{0}为第一代插件", authInstance));
                                }
                                else if ((long)authVersion.Invoke(authInstance, new object[] { 2 }) != 2)
                                {
                                    Logger.log(String.Format("{0}版本高于启动器，放弃加载", authInstance));
                                }
                                var mAuthName = T.GetMethod("GetName");
                                var authName =
                                    mAuthName.Invoke(authInstance, new object[] { language }).ToString();
                                Auths.Add(authName, authInstance);
                                Logger.log(String.Format("{0}加载成功，名称为{1}", authInstance, authName),
                                    Logger.LogType.Error);
                            }
                            catch (MissingMethodException ex)
                            {
                                Logger.log(String.Format("加载{0}的{1}失败", auth, t), Logger.LogType.Error);
                                Logger.log(ex);
                            }
                            catch (ArgumentException ex)
                            {
                                Logger.log(String.Format("加载{0}的{1}失败", auth, t), Logger.LogType.Error);
                                Logger.log(ex);
                            }
                            catch (NotSupportedException ex)
                            {
                                if (ex.Message.IndexOf("0x80131515", System.StringComparison.Ordinal) != -1)
                                {
                                    MessageBox.Show(LangManager.GetLangFromResource("LoadPluginLockErrorInfo"), LangManager.GetLangFromResource("LoadPluginLockErrorTitle"));
                                }
                                else throw;
                            }
                        } else if (t.GetInterface("IBmclPlugin") != null)
                        {

                        }
                        else
                        {
                            try
                            {
                                var authInstance = authMethod.CreateInstance(t.FullName);
                                if (authInstance == null) continue;
                                var T = authInstance.GetType();
                                var authVer = T.GetMethod("GetVer");
                                if (authVer == null)
                                {
                                    if (authInstance.ToString().IndexOf("My.MyApplication", System.StringComparison.Ordinal) == -1 &&
                                        authInstance.ToString().IndexOf("My.MyComputer", System.StringComparison.Ordinal) == -1 &&
                                        authInstance.ToString().IndexOf("My.MyProject+MyWebServices", System.StringComparison.Ordinal) == -1 &&
                                        authInstance.ToString().IndexOf("My.MySettings", System.StringComparison.Ordinal) == -1)
                                    {
                                        Logger.log(String.Format("未找到{0}的GetVer方法，放弃加载", authInstance));
                                    }
                                    continue;
                                }
                                if ((long)authVer.Invoke(authInstance, null) != 1)
                                {
                                    Logger.log(String.Format("{0}的版本不为1，放弃加载", authInstance));
                                    continue;
                                }
                                var authVersion = T.GetMethod("GetVersion");
                                if (authVersion == null)
                                {
                                    Logger.log(String.Format("{0}为第一代插件", authInstance));
                                }
                                else if ((long)authVersion.Invoke(authInstance, new object[] { 2 }) != 2)
                                {
                                    Logger.log(String.Format("{0}版本高于启动器，放弃加载", authInstance));
                                }
                                var mAuthName = T.GetMethod("GetName");
                                var authName =
                                    mAuthName.Invoke(authInstance, new object[] { language }).ToString();
                                Auths.Add(authName, authInstance);
                                Logger.log(String.Format("{0}加载成功，名称为{1}", authInstance, authName),
                                    Logger.LogType.Error);
                            }
                            catch (MissingMethodException ex)
                            {
                                if (t.ToString().IndexOf("My.MyProject", System.StringComparison.Ordinal) == -1 &&
                                    t.ToString().IndexOf("My.Resources.Resources", System.StringComparison.Ordinal) == -1 &&
                                    t.ToString().IndexOf("My.MySettingsProperty", System.StringComparison.Ordinal) == -1)
                                {
                                    Logger.log(String.Format("加载{0}的{1}失败", auth, t), Logger.LogType.Error);
                                    Logger.log(ex);
                                }
                            }
                            catch (ArgumentException ex)
                            {
                                if (t.ToString().IndexOf("My.MyProject+MyWebServices", System.StringComparison.Ordinal) == -1 &&
                                    t.ToString().IndexOf("My.MyProject+ThreadSafeObjectProvider`1[T]", System.StringComparison.Ordinal) == -1)
                                {
                                    Logger.log(String.Format("加载{0}的{1}失败", auth, t), Logger.LogType.Error);
                                    Logger.log(ex);
                                }
                            }
                            catch (NotSupportedException ex)
                            {
                                if (ex.Message.IndexOf("0x80131515", System.StringComparison.Ordinal) != -1)
                                {
                                    MessageBox.Show(LangManager.GetLangFromResource("LoadPluginLockErrorInfo"), LangManager.GetLangFromResource("LoadPluginLockErrorTitle"));
                                }
                                else throw;
                            }
                        }
                    }
                }
                catch (NotSupportedException ex)
                {
                    if (ex.Message.IndexOf("0x80131515", System.StringComparison.Ordinal) != -1)
                    {
                        MessageBox.Show(LangManager.GetLangFromResource("LoadPluginLockErrorInfo"), LangManager.GetLangFromResource("LoadPluginLockErrorTitle"));
                    }
                }
            }
        }

        public static void Invoke(Delegate invoke, object[] argObjects = null)
        {
            BmclCore.Dispatcher.Invoke(invoke, argObjects);
        }


        private static void LoadLanguage()
        {
            ResourceDictionary lang = LangManager.LoadLangFromResource("pack://application:,,,/Lang/zh-cn.xaml");
            BmclCore.Language.Add((string)lang["DisplayName"], lang["LangName"]);
            LangManager.Add(lang["LangName"] as string, "pack://application:,,,/Lang/zh-cn.xaml");

            lang = LangManager.LoadLangFromResource("pack://application:,,,/Lang/zh-tw.xaml");
            BmclCore.Language.Add((string)lang["DisplayName"], lang["LangName"]);
            LangManager.Add(lang["LangName"] as string, "pack://application:,,,/Lang/zh-tw.xaml");
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Lang"))
            {
                foreach (string langFile in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\Lang", "*.xaml", SearchOption.TopDirectoryOnly))
                {
                    lang = LangManager.LoadLangFromResource(langFile);
                    BmclCore.Language.Add((string)lang["DisplayName"], lang["LangName"]);
                    LangManager.Add(lang["LangName"] as string, langFile);
                }
            }
            else
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Lang");
            }
        }
        
    }
}
