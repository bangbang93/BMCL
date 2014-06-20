using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.Runtime.Serialization.Json;
using System.Collections;
using System.IO;

using System.Management;

namespace BMCLV2
{
    class Report
    {
        public void Main()
        {
            try
            {
                DataContractJsonSerializer SysinfoJsonSerializer = new DataContractJsonSerializer(typeof(sysinfo));
                Stream SysinfoJsonStream = new MemoryStream();
                sysinfo systeminfo = new sysinfo();
                SysinfoJsonSerializer.WriteObject(SysinfoJsonStream, systeminfo);
                SysinfoJsonStream.Position = 0;
                StreamReader SysinfoJsonReader = new StreamReader(SysinfoJsonStream);
                string SysinfoJson = SysinfoJsonReader.ReadToEnd();
                Hashtable ht = new Hashtable();
                ht.Add("id", BmclCore.config.username);
                ht.Add("sysinfo", SysinfoJson);
                ht.Add("version", BmclCore.bmclVersion);
                string postdata = ParsToString(ht);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://www.bangbang93.com/bmcl/bmcllog.php");
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                byte[] buffer = Encoding.UTF8.GetBytes(postdata);
                req.ContentLength = buffer.Length;
                Stream s = req.GetRequestStream();
                s.Write(buffer, 0, buffer.Length);
                s.Close();
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();


            }
            catch (Exception ex)
            {
                Logger.log(ex);
            }
        }
        public static String ParsToString(Hashtable Pars)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string k in Pars.Keys)
            {
                if (sb.Length > 0)
                {
                    sb.Append("&");
                }
                sb.Append(HttpUtility.UrlEncode(k) + "=" + HttpUtility.UrlEncode(Pars[k].ToString()));
            }
            return sb.ToString();
        }
    }

    [Serializable]
    class sysinfo
    {
        string memory;
        string cpu;
        string bit;
        string video;
        string system;
        public sysinfo()
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
            memory = capacity.ToString("f0")+"MB";

            try  //系统位数，系统名称
            {
                ManagementClass searcher = new ManagementClass("WIN32_Processor");
                ManagementObjectCollection moc = searcher.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    cpu = mo["Name"].ToString().Trim();
                    bit = mo["AddressWidth"].ToString().Trim() + "Bit";
                }
            }
            catch { }

            try  //显卡， 支持多显卡
            {
                ManagementClass searcher = new ManagementClass("Win32_VideoController");
                ManagementObjectCollection moc = searcher.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    video += (mo["Name"].ToString().Trim()) + "\n";
                }
            }
            catch { }

            try  //系统版本
            {
                //ManagementClass searcher = new ManagementClass("Win32_OperatingSystem");
                //ManagementObjectCollection moc = searcher.GetInstances();
                //foreach (ManagementObject mo in moc)
                //{
                //    system += (mo["Name"].ToString().Trim()) + "\n";
                //    system += (mo["CSDVersion"].ToString().Trim()) + "\n";
                //    system += (mo["Version"].ToString().Trim()) + "\n";
                //}
                system = Environment.OSVersion.VersionString;
            }
            catch { }
        }
    }
}
