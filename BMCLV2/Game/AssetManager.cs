using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BMCLV2.JsonClass;
using BMCLV2.Util;

namespace BMCLV2.Game
{
  public class AssetManager
  {
    private readonly string _assetsDirectory = Path.Combine(BmclCore.MinecraftDirectory, "assets");
    private readonly string _indexesDirectory = Path.Combine(BmclCore.MinecraftDirectory, "assets/indexes");
    private readonly string _objectsDirectory = Path.Combine(BmclCore.MinecraftDirectory, "assets/objects");
    private readonly VersionInfo _versionInfo;
    private AssetsIndex _assetsIndex;

    private OnAssetsDownload _onAssetsDownload;

    public AssetManager(VersionInfo versionInfo)
    {
      _versionInfo = versionInfo;
    }

    public event OnAssetsDownload OnAssetsDownload
    {
      add => _onAssetsDownload += value;
      remove => _onAssetsDownload -= value;
    }

    public async Task Sync()
    {
      try
      {
        _assetsIndex =
          await BmclCore.MirrorManager.CurrentMirror.Asset.GetAssetsIndex(_versionInfo, _indexesDirectory);
      }
      catch (WebException exception)
      {
        Logger.Fatal(exception);
        return;
      }

      Logger.Log($"assets count: {_assetsIndex.Objects.Count}");
      var index = 0;
      var set = new List<string>();
      var semi = new SemaphoreSlim(20, 20);

      await Task.WhenAll(_assetsIndex.Objects.Values.Select(obj => Task.Run(async () =>
      {
        await semi.WaitAsync();

        try
        {
          index++;
          if (set.Contains(obj.Path)) return;
          set.Add(obj.Path);
          _onAssetsDownload(_assetsIndex.Objects.Count, index, obj.Path);
          var path = Path.Combine(_objectsDirectory, obj.Path);
          var hash = Crypto.GetSha1HashFromFile(path);
          if (hash == obj.Hash) return;
          Logger.Log($"{index}/{_assetsIndex.Objects.Count} Sync {obj.Path}");
          await BmclCore.MirrorManager.CurrentMirror.Asset.GetAssetsObject(obj, _objectsDirectory);
        }
        catch (WebException exception)
        {
          Logger.Log($"{index}/{_assetsIndex.Objects.Count} SyncFailed {obj.Path}");
          Logger.Fatal(exception);
        }
        finally
        {
          semi.Release();
        }
      })).ToArray());

      Logger.Log("Sync assets finish");
    }
  }
}
