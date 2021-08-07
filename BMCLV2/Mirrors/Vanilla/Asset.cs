using System.IO;
using System.Threading.Tasks;
using BMCLV2.JsonClass;

namespace BMCLV2.Mirrors.Vanilla
{
    public class Asset : Interface.Asset
    {
        private const string Server = "http://resources.download.minecraft.net/";

        public override async Task GetAssetsObject(AssetsIndex.Assets assets, string savePath)
        {
            var uri = $"{Server}/{assets.Path}";
            savePath = Path.Combine(savePath, assets.Path);
            await Downloader.DownloadFileTaskAsync(uri, savePath);
        }
    }
}
