using System;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Threading;

namespace BMCLV2
{
    class UpdateChecker
    {
        public delegate void CheckUpdateFinishEventHandler(bool hasUpdate, string updateAddr, string updateInfo, int updateBuild);

        public event CheckUpdateFinishEventHandler CheckUpdateFinishEvent;

        protected virtual void OnCheckUpdateFinishEvent(bool hasupdate, string updateaddr, string updateinfo, int updateBuild)
        {
            CheckUpdateFinishEventHandler handler = CheckUpdateFinishEvent;
            if (handler != null) BmclCore.Invoke(new Action(() => handler(hasupdate, updateaddr, updateinfo, updateBuild)));
        }


        private const string CheckUrl = @"http://www.bangbang93.com/bmcl/checkupdate.php";
        public bool HasUpdate {get; private set; }
        public string UpdateInfo {get; private set; }
        public string LastestDownloadUrl {get; private set; }
        public UpdateChecker()
        {
            var thread = new Thread(Run);
            thread.Start();
        }

        private void Run()
        {
            try
            {
                int build = Convert.ToInt32(BmclCore.BmclVersion.Split('.')[3]);
                var req = (HttpWebRequest)WebRequest.Create(CheckUrl + "?ver=" + build);
                req.Method = "GET";
                var res = (HttpWebResponse)req.GetResponse();
                var verJsonSerializer = new DataContractJsonSerializer(typeof(VerList));
                var verTable = (VerList)verJsonSerializer.ReadObject(res.GetResponseStream());
                if (verTable.Lastest.Build == 0)
                {
                    Logger.Log("解析返回的更新日志失败", Logger.LogType.Error);
                    HasUpdate = false;
                    return;
                }
                if (verTable.Lastest.Build > Convert.ToInt32(build))
                {
                    HasUpdate = true;
                    LastestDownloadUrl = verTable.Lastest.DownloadUrl;
                    Logger.Log("需要更新，最新版本为" + verTable.Lastest);
                    Logger.Log("下载地址为" + LastestDownloadUrl);
                }
                else
                {
                    HasUpdate = false;
                    Logger.Log("无需更新");
                }
                var sb = new StringBuilder();
                foreach (UpdateInfo verInfo in verTable.Update)
                {
                    if (verInfo.Build > Convert.ToInt32(build))
                    {
                        sb.AppendLine(verInfo.Version);
                        sb.AppendLine(verInfo.Info);
                    }
                }
                UpdateInfo = sb.ToString();
                OnCheckUpdateFinishEvent(HasUpdate, verTable.Lastest.DownloadUrl, UpdateInfo, verTable.Lastest.Build);
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
