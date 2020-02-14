using System;
using System.Threading.Tasks;
using BMCLV2.JsonClass;
using BMCLV2.Optifine;

namespace BMCLV2.Mirrors.MCBBS
{
  public class Optifine: Interface.Optifine
  {
    public override async Task<VersionInfo[]> GetVersionList()
    {
      var json = await Downloader.DownloadStringTaskAsync(new Uri("https://download.mcbbs.net/optifine/versionList"));
      return JSON<VersionInfo[]>.ParseOnce(json);
    }

    public override async Task Download(string mcversion, string type, string patch, string path)
    {
      await Downloader.DownloadFileTaskAsync(
        new Uri($"https://download.mcbbs.net/optifine/{mcversion}/{type}/{patch}"), path);
    }
  }
}
