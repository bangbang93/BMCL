using System;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BMCLV2.JsonClass;

namespace BMCLV2
{
    internal class UpdateChecker
    {
        private const string CheckUrl = @"http://bbs.bangbang93.com/bmcl/checkupdate.php";

        public async Task<UpdateDescription> Run()
        {
            try
            {
                var build = Convert.ToInt32(BmclCore.BmclVersion.Split('.')[3]);
                var res = await new WebClient().DownloadStringTaskAsync(new Uri($"{CheckUrl}?ver={build}"));
                var verTable = new JSON<VersionList>().Parse(res);
                if (verTable.Lastest.Build == 0)
                {
                    Logger.Log("解析返回的更新日志失败", Logger.LogType.Error);
                    return null;
                }
                if (verTable.Lastest.Build > Convert.ToInt32(build))
                {
                    Logger.Log("需要更新，最新版本为" + verTable.Lastest.Version);
                    Logger.Log($"下载地址为 {verTable.Lastest.DownloadUrl}");
                    var sb = new StringBuilder();
                    foreach (var verInfo in verTable.Update)
                    {
                        if (verInfo.Build > Convert.ToInt32(build))
                        {
                            sb.AppendLine(verInfo.Version);
                            sb.AppendLine(verInfo.Info);
                        }
                    }
                    return new UpdateDescription
                    {
                        Description = sb.ToString(),
                        LastBuild = verTable.Lastest.Build,
                        Url = verTable.Lastest.DownloadUrl,
                    };
                }
                Logger.Log("无需更新");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }
    }
    
    internal class UpdateDescription
    {
        public int LastBuild;
        public string Description;
        public string Url;
    }

    [DataContract]
    internal class UpdateInfo
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
    internal class VersionList
    {
        [DataMember]
        public UpdateInfo Lastest;
        [DataMember]
        public UpdateInfo[] Update;
    }
}
