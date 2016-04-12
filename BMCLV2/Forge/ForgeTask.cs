using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using BMCLV2.I18N;
using BMCLV2.JsonClass;

namespace BMCLV2.Forge
{
    class ForgeTask
    {
        private readonly string forgeUrl = "http://bmclapi2.bangbang93.com/forge/promos";
        public delegate void ForgePageReadyHandle();
        public event ForgePageReadyHandle ForgePageReadyEvent;
        private ForgeVersion[] _forgeNew;
        public Dictionary<string, string> ForgeDownloadUrl = new Dictionary<string, string>(), 
            ForgeChangeLogUrl = new Dictionary<string, string>();
        public async Task<ForgeVersion[]> GetVersion()
        {
            var downloder = new Downloader.Downloader();
            var json = await downloder.DownloadStringTaskAsync(forgeUrl);
            var versions = new JSON(typeof (ForgeVersion[])).Parse(json) as ForgeVersion[];
            if (versions != null)
                Logger.info($"获取到{versions.Length}个forge版本");
            else 
                Logger.error("获取到0个forge版本");
            return versions;
        }

        public async Task DownloadForge(ForgeVersion forgeVersion)
        {
            var url = forgeVersion.GetDownloadUrl();
            var downer = new Downloader.Downloader();
            var w = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\.minecraft\\launcher_profiles.json");
            w.Write(Resource.NormalProfile.Profile);
            w.Close();
            await downer.DownloadFileTaskAsync(url, "forge.jar");
            var forgeIns = new Process
            {
                StartInfo =
                {
                    FileName = BmclCore.Config.Javaw,
                    Arguments = "-jar \"" + BmclCore.BaseDirectory + "\\forge.jar\""
                }
            };
            Logger.log(forgeIns.StartInfo.Arguments);
            forgeIns.Start();
            forgeIns.WaitForExit();
        }
    }
}
