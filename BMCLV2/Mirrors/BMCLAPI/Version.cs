using System.Linq;
using System.Threading.Tasks;

namespace BMCLV2.Mirrors.BMCLAPI
{
  public class Version : Interface.Version
  {
    private const string Server = "http://bmclapi2.bangbang93.com/";

    private readonly string[] _originServers =
      { "https://launchermeta.mojang.com/", "https://launcher.mojang.com/", "https://piston-data.mojang.com" };

    public Version()
    {
      Url = "http://bmclapi2.bangbang93.com/mc/game/version_manifest.json";
    }

    public override string Name { get; } = "BMCLAPI";

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

    public override string GetUrl(string url)
    {
      return _originServers.Aggregate(url, (current, originServer) => current.Replace(originServer, Server));
    }
  }
}
