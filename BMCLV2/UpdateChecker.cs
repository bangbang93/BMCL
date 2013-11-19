using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Collections;

namespace BMCLV2
{
    class UpdateChecker
    {
        private static string CheckUrl = @"http://www.bangbang93.com/bmcl/checkupdate.php";
        private static string Ver = FrmMain.ver;
        public bool HasUpdate {get; private set; }
        public string UpdateInfo {get; private set; }
        public string LastestDownloadUrl {get; private set; }
        public UpdateChecker()
        {
            try
            {
                int Build = Convert.ToInt32(Ver.Split('.')[3]);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(CheckUrl + "?ver=" + Build);
                req.Method = "GET";
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                DataContractJsonSerializer VerJsonSerializer = new DataContractJsonSerializer(typeof(VerList));
                VerList VerTable = (VerList)VerJsonSerializer.ReadObject(res.GetResponseStream());
                if (VerTable.Lastest.Build==0 )
                {
                    Logger.Log("解析返回的更新日志失败", Logger.LogType.Error);
                    HasUpdate = false;
                    return;
                }
                if (VerTable.Lastest.Build > Convert.ToInt32(Build))
                {
                    HasUpdate = true;
                    LastestDownloadUrl = VerTable.Lastest.DownloadUrl;
                    Logger.Log("需要更新，最新版本为" + VerTable.Lastest);
                    Logger.Log("下载地址为" + LastestDownloadUrl);
                }
                else
                {
                    HasUpdate = false;
                    Logger.Log("无需更新");
                }
                StringBuilder sb = new StringBuilder();
                foreach (UpdateInfo VerInfo in VerTable.Update)
                {
                    if ( VerInfo.Build > Convert.ToInt32(Build))
                    {
                        sb.AppendLine(VerInfo.Version as string);
                        sb.AppendLine(VerInfo.Info as string);
                    }
                }
                UpdateInfo = sb.ToString();

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                HasUpdate = false;
            }
        }
    }
    [DataContract]
    struct UpdateInfo
    {
        [DataMember]
        public string Version;
        [DataMember]
        public int Build;
        [DataMember]
        public string Info;
        [DataMember]
        public string DownloadUrl;
    }
    [DataContract]
    struct VerList
    {
        [DataMember]
        public UpdateInfo Lastest;
        [DataMember]
        public UpdateInfo[] Update;
    }
}
