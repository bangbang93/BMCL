using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using BMCLV2.Lang;

namespace BMCLV2
{
// ReSharper disable once InconsistentNaming
    public static class BmclCore
    {
        public static String bmclVersion;
        private const string Cfgfile = "bmcl.xml";
        public static Config config;
        public static Dictionary<object, object> Auths = new Dictionary<object, object>();
        public static launcher _game;

        static BmclCore()
        {
            BmclCore.bmclVersion = Application.ResourceAssembly.FullName.Split('=')[1];
            BmclCore.bmclVersion = BmclCore.bmclVersion.Substring(0, BmclCore.bmclVersion.IndexOf(','));
            Logger.log("BMCL V3 Ver." + BmclCore.bmclVersion + "正在启动");
            if (File.Exists(Cfgfile))
            {
                config = Config.Load(Cfgfile);
                if (config.passwd == null)
                {
                    config.passwd = new byte[0];   //V2的密码存储兼容
                }
                Logger.log(string.Format("加载{0}文件", Cfgfile));
                Logger.log(BmclCore.config);
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
            LangManager.UseLanguage(BmclCore.config.lang);
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
            if (Directory.Exists("auths"))
            {
                string[] authplugins = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\auths");
                foreach (string auth in authplugins)
                {
                    if (auth.ToLower().EndsWith(".dll"))
                    {
                        Logger.log("尝试加载" + auth);
                        try
                        {
                            Assembly authMethod = Assembly.LoadFrom(auth);
                            Type[] types = authMethod.GetTypes();
                            foreach (Type t in types)
                            {
                                try
                                {
                                    object authInstance = authMethod.CreateInstance(t.FullName);
                                    if (authInstance != null)
                                    {
                                        Type T = authInstance.GetType();
                                        MethodInfo AuthVer = T.GetMethod("GetVer");
                                        if (AuthVer == null)
                                        {
                                            Logger.log(string.Format("未找到{0}的GetVer方法，放弃加载", authInstance));
                                            continue;
                                        }
                                        if ((long) AuthVer.Invoke(authInstance, null) != 1)
                                        {
                                            Logger.log(string.Format("{0}的版本不为1，放弃加载", authInstance));
                                            continue;
                                        }
                                        MethodInfo AuthVersion = T.GetMethod("GetVersion");
                                        if (AuthVersion == null)
                                        {
                                            Logger.log(string.Format("{0}为第一代插件", authInstance));
                                        }
                                        else if ((long) AuthVersion.Invoke(authInstance, new object[] {2}) != 2)
                                        {
                                            Logger.log(string.Format("{0}版本高于启动器，放弃加载", authInstance));
                                        }
                                        MethodInfo MAuthName = T.GetMethod("GetName");
                                        string AuthName =
                                            MAuthName.Invoke(authInstance, new object[] {language}).ToString();
                                        Auths.Add(AuthName, authInstance);
                                        Logger.log(string.Format("{0}加载成功，名称为{1}", authInstance, AuthName),
                                            Logger.LogType.Error);
                                    }
                                    else
                                    {
                                        
                                    }
                                }
                                catch (MissingMethodException ex)
                                {
                                    Logger.log(string.Format("加载{0}的{1}失败", auth, t.ToString()), Logger.LogType.Error);
                                    Logger.log(ex, Logger.LogType.Exception);
                                }
                                catch (ArgumentException ex)
                                {
                                    Logger.log(string.Format("加载{0}的{1}失败", auth, t.ToString()), Logger.LogType.Error);
                                    Logger.log(ex, Logger.LogType.Exception);
                                }
                                catch (NotSupportedException ex)
                                {
                                    if (ex.Message.IndexOf("0x80131515") != -1)
                                    {
                                        MessageBox.Show(LangManager.GetLangFromResource("LoadPluginLockErrorInfo"), LangManager.GetLangFromResource("LoadPluginLockErrorTitle"));
                                    }
                                    else throw ex;
                                }
                            }
                        }
                        catch (NotSupportedException ex)
                        {
                            if (ex.Message.IndexOf("0x80131515") != -1)
                            {
                                MessageBox.Show(LangManager.GetLangFromResource("LoadPluginLockErrorInfo"), LangManager.GetLangFromResource("LoadPluginLockErrorTitle"));
                            }
                        }
                    }
                }
            }
            #endregion
        }
    }
}
