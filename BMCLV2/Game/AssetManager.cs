using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BMCLV2.JsonClass;
using BMCLV2.Util;

namespace BMCLV2.Game
{
  public class AssetManager
  {
    private static readonly string AssetsDirectory = Path.Combine(BmclCore.MinecraftDirectory, "assets");
    private static readonly string IndexesDirectory = Path.Combine(AssetsDirectory, "indexes");
    private static readonly string ObjectsDirectory = Path.Combine(AssetsDirectory, "objects");
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
        var assetIndex = _versionInfo.AssetIndex;
        string url;
        if (assetIndex == null)
          url = $"http://resources.download.minecraft.net/indexes/{_versionInfo.Assets}.json";
        else
          url = _versionInfo.AssetIndex.Url;
        var cache = BmclCore.FileCache.Get(url);
        string assetIndexString;
        if (cache != null)
        {
          assetIndexString = Encoding.Default.GetString(cache);
        }
        else
        {
          var savePath = Path.Combine(IndexesDirectory, $"{assetIndex?.Id ?? _versionInfo.Assets}.json");
          assetIndexString = await BmclCore.MirrorManager.CurrentMirror.Version.DownloadJson(url);
          File.WriteAllText(savePath, assetIndexString);
          BmclCore.FileCache.Set(url, assetIndexString);
        }
        // assets的json比较奇葩，不能直接通过反序列化得到
        var assetsObject = new JSON<Dictionary<string, Dictionary<string, AssetsIndex.Assets>>>().Parse(assetIndexString);
        _assetsIndex=  new AssetsIndex
        {
          Objects = assetsObject["objects"]
        };
      }
      catch (WebException exception)
      {
        Logger.Fatal(exception);
        return;
      }

      Logger.Log($"assets count: {_assetsIndex.Objects.Count}");
      var index = 0;
      var set = new List<string>();
      var semi = new SemaphoreSlim(BmclCore.Config.DownloadThread, BmclCore.Config.DownloadThread);

      await Task.WhenAll(_assetsIndex.Objects.Values.Select(obj => Task.Run(async () =>
      {
        await semi.WaitAsync();

        try
        {
          index++;
          if (set.Contains(obj.Path)) return;
          set.Add(obj.Path);
          var path = Path.Combine(ObjectsDirectory, obj.Path);
          var hash = Crypto.GetSha1HashFromFile(path);
          if (hash == obj.Hash) return;
          Logger.Log($"{index}/{_assetsIndex.Objects.Count} Sync {obj.Path}");
          await BmclCore.MirrorManager.CurrentMirror.Asset.GetAssetsObject(obj, ObjectsDirectory);
          _onAssetsDownload(_assetsIndex.Objects.Count, index, obj.Path);
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
