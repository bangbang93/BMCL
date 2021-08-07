using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using BMCLV2.Game;
using BMCLV2.JsonClass;
using BMCLV2.util;

namespace BMCLV2.Mirrors.MCBBS
{
    public class Asset : Interface.Asset
    {
        private const string Server = "https://download.mcbbs.net/";
        private readonly Regex _originServer = new Regex(@"https://launchermeta\.mojang\.com/");

        public override async Task GetAssetsObject(AssetsIndex.Assets assets, string savePath)
        {
            var uri = $"{Server}objects/{assets.Path}".Replace('\\', '/');
            savePath = Path.Combine(savePath, assets.Path);
            FileHelper.CreateDirectoryForFile(savePath);
            await Downloader.DownloadFileTaskAsync(uri, savePath);
        }
    }
}
