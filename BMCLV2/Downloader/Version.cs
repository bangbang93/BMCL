using System.IO;
using System.Threading.Tasks;
using BMCLV2.Game;
using BMCLV2.JsonClass;
using BMCLV2.util;

namespace BMCLV2.Downloader
{
    public class Version
    {
        private readonly Downloader _downloader = new Downloader();
        private string _url;

        public delegate void OnProcessChange(string status);

        public event OnProcessChange ProcessChange = status => { };

        public Version(string url)
        {
            _url = url;
        }

        public async Task Start()
        {
            ProcessChange("VersionDownloadingJSON");
            var json = await _downloader.DownloadStringTaskAsync(_url);
            var versionInfo = (VersionInfo) new JSON(typeof(VersionInfo)).Parse(json);
            ProcessChange("VersionProcessingJSON");
            var clientUrl = versionInfo.Downloads.Client.Url;
            ProcessChange("VersionDownloadingJar");
            FileHelper.CreateDirectoryForFile(PathHelper.VersionFile(versionInfo.Id, "jar"));
            await _downloader.DownloadFileTaskAsync(clientUrl, PathHelper.VersionFile(versionInfo.Id, "jar"));
            FileHelper.WriteFile(PathHelper.VersionFile(versionInfo.Id, "json"), json);
        }
    }
}