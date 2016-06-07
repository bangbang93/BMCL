using System.Threading.Tasks;

namespace BMCLV2.Mirrors.Vanilla
{
    public class Version : Interface.Version
    {
        public override string Name { get;} = "Vanilla";
        public override async Task<string> DownloadJson(string url)
        {
            return await Downloader.DownloadStringTaskAsync(url);
        }

        public override async Task DownloadJar(string url, string savePath)
        {
            await Downloader.DownloadFileTaskAsync(url, savePath);
        }

        public Version()
        {
            Url = "http://launchermeta.mojang.com/mc/game/version_manifest.json";
        }
    }
}