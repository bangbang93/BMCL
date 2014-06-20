using System;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace BMCLV2
{
    class UpdateChecker
    {
        private const string CheckUrl = @"http://www.bangbang93.com/bmcl/checkupdate.php";
        public bool HasUpdate {get; private set; }
        public string UpdateInfo {get; private set; }
        public string LastestDownloadUrl {get; private set; }
        public UpdateChecker()
        {
            try
            {
                int build = Convert.ToInt32(BmclCore.bmclVersion.Split('.')[3]);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(CheckUrl + "?ver=" + build);
                req.Method = "GET";
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                DataContractJsonSerializer verJsonSerializer = new DataContractJsonSerializer(typeof(VerList));
                VerList verTable = (VerList)verJsonSerializer.ReadObject(res.GetResponseStream());
                if (verTable.Lastest.Build==0 )
                {
                    Logger.log("解析返回的更新日志失败", Logger.LogType.Error);
                    HasUpdate = false;
                    return;
                }
                if (verTable.Lastest.Build > Convert.ToInt32(build))
                {
                    HasUpdate = true;
                    LastestDownloadUrl = verTable.Lastest.DownloadUrl;
                    Logger.log("需要更新，最新版本为" + verTable.Lastest);
                    Logger.log("下载地址为" + LastestDownloadUrl);
                }
                else
                {
                    HasUpdate = false;
                    Logger.log("无需更新");
                }
                StringBuilder sb = new StringBuilder();
                foreach (UpdateInfo verInfo in verTable.Update)
                {
                    if ( verInfo.Build > Convert.ToInt32(build))
                    {
                        sb.AppendLine(verInfo.Version);
                        sb.AppendLine(verInfo.Info);
                    }
                }
                UpdateInfo = sb.ToString();

            }
            catch (Exception ex)
            {
                Logger.log(ex);
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
