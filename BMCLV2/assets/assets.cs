using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Web.Script.Serialization;
using BMCLV2.Lang;
using BMCLV2.util;

namespace BMCLV2.Assets
{
    public class Assets
    {
        private readonly WebClient _downloader = new Downloader.Downloader();
        bool _init = true;
        readonly gameinfo _gameInfo;
        private readonly string _urlDownloadBase;
        private readonly string _urlResourceBase;
        public Assets(gameinfo gameInfo, string urlDownloadBase = null, string urlResourceBase = null)
        {
            _gameInfo = gameInfo;
            _urlDownloadBase = urlDownloadBase ?? BmclCore.UrlDownloadBase;
            _urlResourceBase = urlResourceBase ?? BmclCore.UrlResourceBase;
            var thread = new Thread(Run);
            thread.Start();
        }

        private void Run()
        {
            string gameVersion = _gameInfo.assets;
            try
            {
                _downloader.DownloadStringAsync(new Uri(_urlDownloadBase + "indexes/" + gameVersion + ".json"));
                Logger.info(_urlDownloadBase + "indexes/" + gameVersion + ".json");
            }
            catch (WebException ex)
            {
                Logger.info("游戏版本" + gameVersion);
                Logger.error(ex);
            }
            _downloader.DownloadStringCompleted += Downloader_DownloadStringCompleted;
            _downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
        }
        void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Logger.error(e.UserState.ToString());
                Logger.error(e.Error);
            }
        }

        void Downloader_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            _downloader.DownloadStringCompleted -= Downloader_DownloadStringCompleted;
            if (e.Error != null)
            {
                var error = e.Error as WebException;
                if (error != null)
                {
                    var ex = error;
                    Logger.log(ex.Response.ResponseUri.ToString());
                }
                Logger.error(e.Error);
            }
            else
            {
                string gameVersion = _gameInfo.assets;
                FileHelper.CreateDirectoryForFile(AppDomain.CurrentDomain.BaseDirectory + ".minecraft/assets/indexes/" + gameVersion + ".json");
                var sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + ".minecraft/assets/indexes/" + gameVersion + ".json");
                sw.Write(e.Result);
                sw.Close();
                var jsSerializer = new JavaScriptSerializer();
                var assetsObject = jsSerializer.Deserialize<Dictionary<string, Dictionary<string, AssetsEntity>>>(e.Result);
                Dictionary<string, AssetsEntity> obj = assetsObject["objects"];
                Logger.log("共", obj.Count.ToString(CultureInfo.InvariantCulture), "项assets");
                int i = 0;
                foreach (KeyValuePair<string, AssetsEntity> entity in obj)
                {
                    i++;
                    string url = _urlResourceBase + entity.Value.hash.Substring(0, 2) + "/" + entity.Value.hash;
                    string file = AppDomain.CurrentDomain.BaseDirectory + @".minecraft\assets\objects\" + entity.Value.hash.Substring(0, 2) + "\\" + entity.Value.hash;
                    FileHelper.CreateDirectoryForFile(file);
                    try
                    {
                        if (FileHelper.IfFileVaild(file, entity.Value.size)) continue;
                        if (_init)
                        {
                            BmclCore.NIcon.ShowBalloonTip(3000, LangManager.GetLangFromResource("FoundAssetsModify"));
                            _init = false;
                        }
                        _downloader.DownloadFile(new Uri(url), file);
                        Logger.log(i.ToString(CultureInfo.InvariantCulture), "/", obj.Count.ToString(CultureInfo.InvariantCulture), file.Substring(AppDomain.CurrentDomain.BaseDirectory.Length), "下载完毕");
                        if (i == obj.Count)
                        {
                            Logger.log("assets下载完毕");
                            BmclCore.NIcon.ShowBalloonTip(3000, LangManager.GetLangFromResource("SyncAssetsFinish"));
                        }
                    }
                    catch (WebException ex)
                    {
                        Logger.log(ex.Response.ResponseUri.ToString());
                        Logger.error(ex);
                    }
                }
                if (_init)
                {
                    Logger.info("无需更新assets");
                }
            }
            
        }
    }
}
