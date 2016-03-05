using System.IO;
using System.Threading.Tasks;
using BMCLV2.JsonClass;
using BMCLV2.Objects.Mirrors;
using BMCLV2.util;

namespace BMCLV2.Downloader
{
    public class Version
    {
        private readonly Downloader _downloader = new Downloader();
        private string _url;
        private string _path;

        public Version(string url, string path)
        {
            _url = url;
            _path = path;
        }

        public async Task Start()
        {
            var json = await _downloader.DownloadStringTaskAsync(_url);
            var versionInfo = (VersionInfo) new JSON(typeof(VersionInfo)).Parse(json);
            var clientUrl = versionInfo.downloads.client.url;
            await _downloader.DownloadFileTaskAsync(clientUrl, PathHelper.VersionFile(versionInfo.id, "jar"));
            FileHelper.WriteFile(PathHelper.VersionFile(versionInfo.id, "json"), json);
        }
    }
}