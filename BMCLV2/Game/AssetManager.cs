using System.IO;
using System.Threading.Tasks;
using BMCLV2.JsonClass;
using BMCLV2.Util;

namespace BMCLV2.Game
{
    public class AssetManager
    {
        private AssetsIndex _assetsIndex;
        private readonly  VersionInfo _versionInfo;
        private readonly string _assetsDirectory = Path.Combine(BmclCore.MinecraftDirectory, "assets");
        private readonly string _indexesDirectory = Path.Combine(BmclCore.MinecraftDirectory, "assets/indexes");
        private readonly string _objectsDirectory = Path.Combine(BmclCore.MinecraftDirectory, "assets/objects");

        public AssetManager(VersionInfo versionInfo)
        {
            _versionInfo = versionInfo;
        }

        public async Task Sync()
        {
            _assetsIndex =
                await BmclCore.MirrorManager.CurrectMirror.Asset.GetAssetsIndex(_versionInfo, _indexesDirectory);
            foreach (var obj in _assetsIndex.Objects.Values)
            {
                var path = Path.Combine(_objectsDirectory, obj.Path);
                var hash = Crypto.GetSha1HashFromFile(path);
                if (hash != obj.Hash)
                {
                    await BmclCore.MirrorManager.CurrectMirror.Asset.GetAssetsObject(obj, _objectsDirectory);
                }
            }
        }
    }
}