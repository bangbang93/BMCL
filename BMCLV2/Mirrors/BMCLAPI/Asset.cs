using System.IO;
using System.Threading.Tasks;
using BMCLV2.JsonClass;
using BMCLV2.util;

namespace BMCLV2.Mirrors.BMCLAPI
{
  public class Asset : Interface.Asset
  {
    private const string Server = "http://bmclapi2.bangbang93.com/";

    public override async Task GetAssetsObject(AssetsIndex.Assets assets, string savePath)
    {
      var uri = $"{Server}objects/{assets.Path}".Replace('\\', '/');
      savePath = Path.Combine(savePath, assets.Path);
      FileHelper.CreateDirectoryForFile(savePath);
      await Downloader.DownloadFileTaskAsync(uri, savePath);
    }
  }
}
