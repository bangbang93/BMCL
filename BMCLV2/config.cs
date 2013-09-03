using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.IO;
using System.Windows;

namespace BMCLV2
{
    [Serializable]
    public class config : ICloneable
    {
        public string javaw;
        public string username;
        public string javaxmx;
        public byte[] passwd;
        public string login;
        public bool autostart;
        public string lastPlayVer;
        public string extraJVMArg;
        public double WindowTransparency;
        public bool Report;
        public int DownloadSource;
        public string Lang;

        public config()
        {
            javaw = (getjavadir() != null) ? getjavadir() : "javaw.exe";
            username = "!!!";
            javaxmx = (getmem() / 4).ToString();
            passwd = null;
            login = "啥都没有";
            autostart = false;
            extraJVMArg = "";
            WindowTransparency = 1;
            Report = true;
            DownloadSource = 0;
            Lang = "zh-cn";
        }
        object ICloneable.Clone()
        {
            return this.clone();
        }
        public config clone()
        {
            return (config)this.MemberwiseClone();
        }
        public static config Load(string File)
        {
            FileStream fs = null;
            if (!System.IO.File.Exists(File))
                return new config();
            try
            {
                fs = new FileStream(File, FileMode.Open);
                DataContractSerializer ser = new DataContractSerializer(typeof(config));
                config cfg = ser.ReadObject(fs) as config;
                fs.Close();
                return cfg;
            }
            catch
            {
                MessageBox.Show("加载配置文件遇到错误，使用默认配置");
                return new config();
            }
        }
        public static void Save(config cfg,string File)
        {
            FileStream fs = new FileStream(File, FileMode.Create);
            DataContractSerializer ser = new DataContractSerializer(typeof(config));
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
    }
}
