using System.Linq;
using System.Threading.Tasks;
using BMCLV2.Game;

namespace BMCLV2.Mirrors.MCBBS
{
    public class Version : Interface.Version
    {
        public override string Name { get; } = "BMCLAPI";

        private const string Server = "https://download.mcbbs.net/";

        private readonly string[] _originServers = new []{"https://launchermeta.mojang.com/", "https://launcher.mojang.com/" };

    public Version()
        {
            Url = "https://download.mcbbs.net/mc/game/version_manifest.json";
        }

        public override async Task<string> DownloadJson(string url)
        {
            url = _originServers.Aggregate(url, (current, originServer) => current.Replace(originServer, Server));
            return await Downloader.DownloadStringTaskAsync(url);
        }

        public override async Task DownloadJar(string url, string savePath)
        {
            url = _originServers.Aggregate(url, (current, originServer) => current.Replace(originServer, Server));
            await Downloader.DownloadFileTaskAsync(url, savePath);
        }
    }
}
