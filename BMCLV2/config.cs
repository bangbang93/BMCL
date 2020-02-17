using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management;
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using BMCLV2.Cfg;

namespace BMCLV2
{
    [DataContract]
    public class Config
    {
        [DataMember] public string Javaw;
        [DataMember] public string Username;
        [DataMember] public string Javaxmx;
        [DataMember] public string Login;
        [DataMember] public string LastPlayVer;
        [DataMember] public string ExtraJvmArg;
        [DataMember] public string Lang;
        [DataMember] public byte[] Passwd;
        [DataMember] public bool Autostart, Report,CheckUpdate;
        [DataMember] public double WindowTransparency;
        [DataMember] public int DownloadSource;
        [DataMember] public Dictionary<string, object> PluginConfig;
        [DataMember] public int Height;
        [DataMember] public int Width;
        [DataMember] public bool FullScreen;
        [DataMember] public LaunchMode LaunchMode;
        [DataMember] public int DownloadThread = 20;

        public Config()
        {
            Javaw = GetJavaDir() ?? "javaw.exe";
            Username = "!!!";
            Javaxmx = (GetMemory() / 4).ToString();
            Passwd = new byte[0];
            Login = "啥都没有";
            Autostart = false;
            ExtraJvmArg = " -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true";
            WindowTransparency = 1;
            Report = true;
            DownloadSource = 0;
            Lang = "zh-cn";
            CheckUpdate = true;
            PluginConfig = new Dictionary<string, object>();
            Height = -1;
            Width = -1;
            FullScreen = false;
            LaunchMode = LaunchMode.Normal;
        }

        public override string ToString()
        {
            return $"Javaw: {Javaw}, Username: {Username}, Javaxmx: {Javaxmx}, Login: {Login}, LastPlayVer: {LastPlayVer}, ExtraJvmArg: {ExtraJvmArg}, Lang: {Lang}, Passwd: {Passwd}, Autostart: {Autostart}, Report: {Report}, CheckUpdate: {CheckUpdate}, WindowTransparency: {WindowTransparency}, DownloadSource: {DownloadSource}, PluginConfig: {PluginConfig}, Height: {Height}, Width: {Width}, FullScreen: {FullScreen}, LaunchMode: {LaunchMode}";
        }

        public object GetPluginConfig(string key)
        {
            return PluginConfig.ContainsKey(key) ? PluginConfig[key] : null;
        }

        public void SetPluginConfig(string key, object value)
        {
            if (PluginConfig.ContainsKey(key))
            {
                PluginConfig[key] = value;
            }
            else
            {
                PluginConfig.Add(key, value);
            }
        }

        public static Config Load(string file = "bmcl.xml")
        {
            if (!File.Exists(file))
                return new Config();
            try
            {
                var fs = new FileStream(file, FileMode.Open);
                var ser = new DataContractSerializer(typeof(Config));
                var cfg = (Config)ser.ReadObject(fs);
                fs.Close();
                return cfg;
            }
            catch
            {
                MessageBox.Show("加载配置文件遇到错误，使用默认配置");
                return new Config();
            }
        }
        public static void Save(Config cfg = null ,string file = null)
        {
            if (cfg == null)
            {
                cfg = BmclCore.Config;
            }
            if (file == null)
            {
                file = BmclCore.BaseDirectory + "bmcl.xml";
            }
            var fs = new FileStream(file, FileMode.Create);
            var ser = new DataContractSerializer(typeof(Config));
            ser.WriteObject(fs, cfg);
            fs.Close();
        }

        public void Save(string file = null)
        {
            Save(this, file);
        }

        /// <summary>
        /// 读取注册表，寻找安装的java路径
        /// </summary>
        /// <returns>javaw.exe路径</returns>
        public static string GetJavaDir()
        {
            try
            {
                var reg = Registry.LocalMachine;
                var openSubKey = reg.OpenSubKey("SOFTWARE");
                var registryKey = openSubKey?.OpenSubKey("JavaSoft");
                var jre = registryKey?.OpenSubKey("Java Runtime Environment");
                if (jre == null) return null;
                var javaList = new List<string>();
                foreach (var ver in jre.GetSubKeyNames())
                {
                    try
                    {
                        var command = jre.OpenSubKey(ver);
                        if (command == null) continue;
                        var str = command.GetValue("JavaHome").ToString();
                        if (str != "")
                            javaList.Add(str + @"\bin\javaw.exe");
                    }
                    catch { return null; }
                }
                //优先java8
                foreach (var java in javaList)
                {
                    if(java.ToLower().Contains("jre8")||java.ToLower().Contains("jdk1.8")||java.ToLower().Contains("jre1.8")){
                        return java;
                    }
                }
                return javaList[0];
            }
            catch { return null; }

        }
        /// <summary>
        /// 获取系统物理内存大小
        /// </summary>
        /// <returns>系统物理内存大小，支持64bit,单位MB</returns>
        public static ulong GetMemory()
        {
            try
            {
                var cimobject1 = new ManagementClass("Win32_PhysicalMemory");
                var moc1 = cimobject1.GetInstances();
                var capacity = moc1.Cast<ManagementObject>().Sum(mo1 => Math.Round(long.Parse(mo1.Properties["Capacity"].Value.ToString())/1024.0/1024.0, 1));
                moc1.Dispose();
                cimobject1.Dispose();
                var qmem = Convert.ToUInt64(capacity.ToString(CultureInfo.InvariantCulture));
                return qmem;
            }
            catch (Exception ex)
            {
                Logger.Fatal("获取内存失败");
                Logger.Fatal(ex);
                return ulong.MaxValue;
            }
        }

        public static string GetGuid()
        {
            return Guid.NewGuid().ToString();
        }

        public string GetPassword()
        {
            return Encoding.UTF8.GetString(Passwd);
        }
    }
}
