using System;
using System.IO;
using System.Threading.Tasks;
using BMCLV2.Game;
using BMCLV2.JsonClass;
using BMCLV2.util;

namespace BMCLV2.Mirrors.BMCLAPI
{
    public class Asset : Interface.Asset
    {
        private const string Server = "http://bmclapi2.bangbang93.com/indexes/";

        public override async Task<AssetsIndex> GetAssetsIndex(VersionInfo versionInfo, string savePath)
        {
            string assetIndexString;
            var assetIndex = versionInfo.AssetIndex;
            if (assetIndex == null)
                assetIndexString = await Downloader.DownloadStringTaskAsync($"{Server}{versionInfo.Id}.json");
            else
                assetIndexString = await Downloader.DownloadStringTaskAsync(versionInfo.AssetIndex.Url);
            savePath = Path.Combine(savePath, $"{assetIndex?.Id ?? versionInfo.Assets}.json");
            FileHelper.WriteFile(savePath, assetIndexString);
            return new JSON<AssetsIndex>().Parse(assetIndexString);
        }
    }
}