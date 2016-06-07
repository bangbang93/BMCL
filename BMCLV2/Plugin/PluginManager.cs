using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using BMCLV2.I18N;

namespace BMCLV2.Plugin
{
    public class PluginManager
    {

        private readonly Dictionary<string, object> _oldAuthPlugins = new Dictionary<string, object>();
        public void LoadOldAuthPlugin(string language)
        {
            _oldAuthPlugins.Clear();
            if (!Directory.Exists("auths")) return;
            var authplugins = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\auths");
            foreach (var auth in authplugins.Where(auth => auth.ToLower().EndsWith(".dll")))
            {
                Logger.Log("尝试加载" + auth);
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
                                    Logger.Log($"未找到{authInstance}的GetVer方法，放弃加载");
                                    continue;
                                }
                                if ((long)authVer.Invoke(authInstance, null) > 2)
                                {
                                    Logger.Log($"{authInstance}的版本高于2，放弃加载");
                                    continue;
                                }
                                var authVersion = T.GetMethod("GetVersion");
                                if (authVersion == null)
                                {
                                    Logger.Log($"{authInstance}为第一代插件");
                                }
                                else if ((long)authVersion.Invoke(authInstance, new object[] { 2 }) != 2)
                                {
                                    Logger.Log($"{authInstance}版本高于启动器，放弃加载");
                                }
                                var mAuthName = T.GetMethod("GetName");
                                var authName =
                                    mAuthName.Invoke(authInstance, new object[] { language }).ToString();
                                _oldAuthPlugins.Add(authName, authInstance);
                                Logger.Log($"{authInstance}加载成功，名称为{authName}",
                                    Logger.LogType.Error);
                            }
                            catch (MissingMethodException ex)
                            {
                                Logger.Log($"加载{auth}的{t}失败", Logger.LogType.Error);
                                Logger.Log(ex);
                            }
                            catch (ArgumentException ex)
                            {
                                Logger.Log($"加载{auth}的{t}失败", Logger.LogType.Error);
                                Logger.Log(ex);
                            }
                            catch (NotSupportedException ex)
                            {
                                if (ex.Message.IndexOf("0x80131515", StringComparison.Ordinal) != -1)
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
                                    if (authInstance.ToString().IndexOf("My.MyApplication", StringComparison.Ordinal) == -1 &&
                                        authInstance.ToString().IndexOf("My.MyComputer", StringComparison.Ordinal) == -1 &&
                                        authInstance.ToString().IndexOf("My.MyProject+MyWebServices", StringComparison.Ordinal) == -1 &&
                                        authInstance.ToString().IndexOf("My.MySettings", StringComparison.Ordinal) == -1)
                                    {
                                        Logger.Log($"未找到{authInstance}的GetVer方法，放弃加载");
                                    }
                                    continue;
                                }
                                if ((long)authVer.Invoke(authInstance, null) != 1)
                                {
                                    Logger.Log($"{authInstance}的版本不为1，放弃加载");
                                    continue;
                                }
                                var authVersion = T.GetMethod("GetVersion");
                                if (authVersion == null)
                                {
                                    Logger.Log($"{authInstance}为第一代插件");
                                }
                                else if ((long)authVersion.Invoke(authInstance, new object[] { 2 }) != 2)
                                {
                                    Logger.Log($"{authInstance}版本高于启动器，放弃加载");
                                }
                                var mAuthName = T.GetMethod("GetName");
                                var authName =
                                    mAuthName.Invoke(authInstance, new object[] { language }).ToString();
                                _oldAuthPlugins.Add(authName, authInstance);
                                Logger.Log($"{authInstance}加载成功，名称为{authName}",
                                    Logger.LogType.Error);
                            }
                            catch (MissingMethodException ex)
                            {
                                if (t.ToString().IndexOf("My.MyProject", StringComparison.Ordinal) == -1 &&
                                    t.ToString().IndexOf("My.Resources.Resources", StringComparison.Ordinal) == -1 &&
                                    t.ToString().IndexOf("My.MySettingsProperty", StringComparison.Ordinal) == -1)
                                {
                                    Logger.Log($"加载{auth}的{t}失败", Logger.LogType.Error);
                                    Logger.Log(ex);
                                }
                            }
                            catch (ArgumentException ex)
                            {
                                if (t.ToString().IndexOf("My.MyProject+MyWebServices", StringComparison.Ordinal) == -1 &&
                                    t.ToString().IndexOf("My.MyProject+ThreadSafeObjectProvider`1[T]", StringComparison.Ordinal) == -1)
                                {
                                    Logger.Log($"加载{auth}的{t}失败", Logger.LogType.Error);
                                    Logger.Log(ex);
                                }
                            }
                            catch (NotSupportedException ex)
                            {
                                if (ex.Message.IndexOf("0x80131515", StringComparison.Ordinal) != -1)
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
                    if (ex.Message.IndexOf("0x80131515", StringComparison.Ordinal) != -1)
                    {
                        MessageBox.Show(LangManager.GetLangFromResource("LoadPluginLockErrorInfo"), LangManager.GetLangFromResource("LoadPluginLockErrorTitle"));
                    }
                }
            }
        }

        public object GetAuth(string name)
        {
            return _oldAuthPlugins.ContainsKey(name) ? _oldAuthPlugins[name] : null;
        }

        public string[] GetAuthNames()
        {
            return _oldAuthPlugins.Keys.ToArray();
        }
    }
}