using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Net;
using System.Web.Script.Serialization;
using System.Collections;
using System.IO;

using BMCLV2.util;

namespace BMCLV2.Assets
{
    public class Assets
    {
        private readonly WebClient _downloader = new WebClient();
        bool _init = true;
        gameinfo GameInfo;
        Dictionary<string, string> _downloadUrlPathPair = new Dictionary<string, string>();
        private string _urlDownloadBase;
        private string _urlResourceBase;
        public Assets(gameinfo GameInfo, string urlDownloadBase = null, string urlResourceBase = null)
        {
            this.GameInfo = GameInfo;
            string gameVersion = GameInfo.assets;
            this._urlDownloadBase = urlDownloadBase ?? BmclCore.UrlDownloadBase;
            this._urlResourceBase = urlResourceBase ?? BmclCore.UrlResourceBase;
            try
            {
                _downloader.DownloadStringAsync(new Uri(BmclCore.UrlDownloadBase + "indexes/" + gameVersion + ".json"));
                Logger.info(BmclCore.UrlDownloadBase + "indexes/" + gameVersion + ".json");
            }
            catch (WebException ex)
            {
                Logger.info("游戏版本" + gameVersion);
                Logger.error(ex);
            }
            _downloader.DownloadStringCompleted += Downloader_DownloadStringCompleted;
            _downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
        }

        void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
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
                Logger.error(e.Error);
            }
            else
            {
                string gameVersion = GameInfo.assets;
                FileHelper.CreateDirectoryForFile(AppDomain.CurrentDomain.BaseDirectory + ".minecraft/assets/indexes/" + gameVersion + ".json");
                StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + ".minecraft/assets/indexes/" + gameVersion + ".json");
                sw.Write(e.Result);
                sw.Close();
                JavaScriptSerializer JSSerializer = new JavaScriptSerializer();
                Dictionary<string, Dictionary<string, AssetsEntity>> AssetsObject = JSSerializer.Deserialize<Dictionary<string, Dictionary<string, AssetsEntity>>>(e.Result);
                Dictionary<string, AssetsEntity> obj = AssetsObject["objects"];
                Logger.log("共", obj.Count.ToString(), "项assets");
                int i = 0;
                foreach (KeyValuePair<string, AssetsEntity> entity in obj)
                {
                    i++;
                    string Url = BmclCore.UrlResourceBase + entity.Value.hash.Substring(0, 2) + "/" + entity.Value.hash;
                    string File = AppDomain.CurrentDomain.BaseDirectory + @".minecraft\assets\objects\" + entity.Value.hash.Substring(0, 2) + "\\" + entity.Value.hash;
                    FileHelper.CreateDirectoryForFile(File);
                    try
                    {
                        if (FileHelper.IfFileVaild(File, entity.Value.size)) continue;
                        if (_init)
                        {
                            BmclCore.NIcon.ShowBalloonTip(3000, Lang.LangManager.GetLangFromResource("FoundAssetsModify"), System.Windows.Forms.ToolTipIcon.Info);
                            _init = false;
                        }
                        //Downloader.DownloadFileAsync(new Uri(Url), File,Url);
                        _downloader.DownloadFile(new Uri(Url), File);
                        Logger.log(i.ToString(), "/", obj.Count.ToString(), File.Substring(AppDomain.CurrentDomain.BaseDirectory.Length), "下载完毕");
                        if (i == obj.Count)
                        {
                            Logger.log("assets下载完毕");
                            BmclCore.NIcon.ShowBalloonTip(3000, Lang.LangManager.GetLangFromResource("SyncAssetsFinish"), System.Windows.Forms.ToolTipIcon.Info);
                        }
                    }
                    catch (WebException ex)
                    {
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
