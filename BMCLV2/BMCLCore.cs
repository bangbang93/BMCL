using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BMCLV2.Lang;
using BMCLV2.launcher;
using BMCLV2.Resource;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace BMCLV2
{
    public static class BmclCore
    {
        public static String bmclVersion;
        private const string Cfgfile = "bmcl.xml";
        public static Config config;
        public static Dictionary<string, object> Auths = new Dictionary<string, object>();
        public static Launcher game;
        public static bool gameRunning = false;
        public static String urlDownloadBase = Url.URL_DOWNLOAD_BASE;
        public static String urlResourceBase = Url.URL_RESOURCE_BASE;
        public static string urlLibrariesBase = Url.URL_LIBRARIES_BASE;
        public static System.Windows.Forms.NotifyIcon nIcon;

        static BmclCore()
        {
            bmclVersion = Application.ResourceAssembly.FullName.Split('=')[1];
            bmclVersion = bmclVersion.Substring(0, bmclVersion.IndexOf(','));
            Logger.log("BMCL V3 Ver." + bmclVersion + "正在启动");
            if (File.Exists(Cfgfile))
            {
                config = Config.Load(Cfgfile);
                if (config.passwd == null)
                {
                    config.passwd = new byte[0];   //V2的密码存储兼容
                }
                Logger.log(String.Format("加载{0}文件", Cfgfile));
                Logger.log(config);
            }
            else
            {
                config = new Config();
                Logger.log("加载默认配置");
            }
            if (config.javaw == "autosearch")
            {
                config.javaw = Config.getjavadir();
            }
            if (config.javaxmx == "autosearch")
            {
                config.javaxmx = (Config.getmem() / 4).ToString(CultureInfo.InvariantCulture);
            }
            LangManager.UseLanguage(config.lang);
            loadPlugin(LangManager.GetLangFromResource("LangName"));
        }

        public static void changeLanguate(string lang)
        {
            LangManager.UseLanguage(lang);
        }

        public static void loadPlugin(string language)
        {
            Auths.Clear();
            #region 加载新插件

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
                            if ((long) authVer.Invoke(authInstance, null) != 1)
                            {
                                Logger.log(String.Format("{0}的版本不为1，放弃加载", authInstance));
                                continue;
                            }
                            var authVersion = T.GetMethod("GetVersion");
                            if (authVersion == null)
                            {
                                Logger.log(String.Format("{0}为第一代插件", authInstance));
                            }
                            else if ((long) authVersion.Invoke(authInstance, new object[] {2}) != 2)
                            {
                                Logger.log(String.Format("{0}版本高于启动器，放弃加载", authInstance));
                            }
                            var mAuthName = T.GetMethod("GetName");
                            var authName =
                                mAuthName.Invoke(authInstance, new object[] {language}).ToString();
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

            #endregion
        }
    }
}
