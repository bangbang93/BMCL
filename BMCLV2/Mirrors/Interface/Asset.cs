using System.Threading.Tasks;
using BMCLV2.Game;
using BMCLV2.JsonClass;

namespace BMCLV2.Mirrors.Interface
{
    public abstract class Asset
    {
        protected readonly Downloader.Downloader Downloader = new Downloader.Downloader();
        public abstract Task<AssetsIndex> GetAssetsIndex(VersionInfo versionInfo, string savePath);
        public abstract Task GetAssetsObject(AssetsIndex.Assets assets, string savePath);
    }
}