using System;
using System.Globalization;
using System.Management;
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.IO;
using System.Windows;

namespace BMCLV2
{
    [DataContract]
    public class Config : ICloneable
    {
        [DataMember]
        public string javaw;

        [DataMember]
        public string username;

        [DataMember]
        public string javaxmx;

        [DataMember]
        public string login;

        [DataMember]
        public string lastPlayVer;

        [DataMember]
        public string extraJvmArg;

        [DataMember]
        public string lang;

        [DataMember]
        public byte[] passwd;
        [DataMember]
        public bool autostart, report,checkUpdate;
        [DataMember]
        public double windowTransparency;
        [DataMember]
        public int downloadSource;

        public Config()
        {
            javaw = getjavadir() ?? "javaw.exe";
            username = "!!!";
            javaxmx = (getmem() / 4).ToString(CultureInfo.InvariantCulture);
            passwd = new byte[0];
            login = "啥都没有";
            autostart = false;
            extraJvmArg = " -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true";
            windowTransparency = 1;
            report = true;
            downloadSource = 0;
            lang = "zh-cn";
            checkUpdate = true;
        }
        object ICloneable.Clone()
        {
            return this.clone();
        }
        public Config clone()
        {
            return (Config)this.MemberwiseClone();
        }
        public static Config Load(string File)
        {
            FileStream fs = null;
            if (!System.IO.File.Exists(File))
                return new Config();
            try
            {
                fs = new FileStream(File, FileMode.Open);
                DataContractSerializer ser = new DataContractSerializer(typeof(Config));
                Config cfg = ser.ReadObject(fs) as Config;
                fs.Close();
                return cfg;
            }
            catch
            {
                MessageBox.Show("加载配置文件遇到错误，使用默认配置");
                return new Config();
            }
        }
        public static void Save(Config cfg = null ,string File = "bmcl.xml")
        {
            if (cfg == null)
            {
                cfg = BmclCore.config;
            }
            FileStream fs = new FileStream(File, FileMode.Create);
            DataContractSerializer ser = new DataContractSerializer(typeof(Config));
            ser.WriteObject(fs, cfg);
            fs.Close();
        }
        /// <summary>
        /// 读取注册表，寻找安装的java路径
        /// </summary>
        /// <returns>javaw.exe路径</returns>
        public static string getjavadir()
        {
            try
            {
                RegistryKey reg = Registry.LocalMachine;
                reg = reg.OpenSubKey("SOFTWARE").OpenSubKey("JavaSoft").OpenSubKey("Java Runtime Environment");
                foreach (string ver in reg.GetSubKeyNames())
                {
                    try
                    {
                        RegistryKey command = reg.OpenSubKey(ver);
                        string str = command.GetValue("JavaHome").ToString();
                        if (str != "")
                            return str + @"\bin\javaw.exe";
                    }
                    catch { return null; }
                }
                return null;
            }
            catch { return null; }

        }
        /// <summary>
        /// 获取系统物理内存大小
        /// </summary>
        /// <returns>系统物理内存大小，支持64bit,单位MB</returns>
        public static ulong getmem()
        {
            try
            {
                double capacity = 0.0;
                ManagementClass cimobject1 = new ManagementClass("Win32_PhysicalMemory");
                ManagementObjectCollection moc1 = cimobject1.GetInstances();
                foreach (ManagementObject mo1 in moc1)
                {
                    capacity += ((Math.Round(Int64.Parse(mo1.Properties["Capacity"].Value.ToString()) / 1024 / 1024.0, 1)));
                }
                moc1.Dispose();
                cimobject1.Dispose();
                UInt64 qmem = Convert.ToUInt64(capacity.ToString());
                return qmem;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Logger.error("获取内存失败");
                Logger.error(ex);
                return ulong.MaxValue;

            }
        }
    }
}
