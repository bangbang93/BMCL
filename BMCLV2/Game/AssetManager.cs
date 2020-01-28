using System.IO;
using System.Net;
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

        private OnAssetsDownload _onAssetsDownload;

        public event OnAssetsDownload OnAssetsDownload
        {
          add => _onAssetsDownload += value;
          remove => _onAssetsDownload -= value;
        }

    public AssetManager(VersionInfo versionInfo)
        {
            _versionInfo = versionInfo;
        }

        public async Task Sync()
        {
          try
          {
            _assetsIndex =
              await BmclCore.MirrorManager.CurrectMirror.Asset.GetAssetsIndex(_versionInfo, _indexesDirectory);
          }
          catch (WebException exception)
          {
            Logger.Fatal(exception);
            return;
          }
          Logger.Log($"assets count: {_assetsIndex.Objects.Count}");
          var index = 0;
          foreach (var obj in _assetsIndex.Objects.Values)
          {
            _onAssetsDownload(_assetsIndex.Objects.Count, index, obj.Path);
            index++;
            var path = Path.Combine(_objectsDirectory, obj.Path);
            var hash = Crypto.GetSha1HashFromFile(path);
            if (hash == obj.Hash) continue;
            Logger.Log($"{index}/{_assetsIndex.Objects.Count} Sync {obj.Path}");
            try
            {
              await BmclCore.MirrorManager.CurrectMirror.Asset.GetAssetsObject(obj, _objectsDirectory);
            }
            catch (WebException exception)
            {
              Logger.Log($"{index}/{_assetsIndex.Objects.Count} SyncFailed {obj.Path}");
              Logger.Fatal(exception);
            }
          }
          Logger.Log("Sync assets finish");
        }
    }
}
