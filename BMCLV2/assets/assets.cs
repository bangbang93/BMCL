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

namespace BMCLV2.assets
{
    public class assets
    {
        WebClient Downloader = new WebClient();
        bool init = true;
        gameinfo GameInfo;
        Dictionary<string, string> DownloadUrlPathPair = new Dictionary<string, string>();
        public assets(gameinfo GameInfo)
        {
            this.GameInfo = GameInfo;
            string GameVersion = GameInfo.assets;
            try
            {
                Downloader.DownloadStringAsync(new Uri(FrmMain.URL_DOWNLOAD_BASE + "indexes/" + GameVersion + ".json"));
                Logger.Log(FrmMain.URL_DOWNLOAD_BASE + "indexes/" + GameVersion + ".json");
            }
            catch (WebException ex)
            {
                Logger.Log("游戏版本" + GameVersion);
                Logger.Log(ex);
            }
            Downloader.DownloadStringCompleted += Downloader_DownloadStringCompleted;
            Downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
        }

        void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Logger.Log(Logger.LogType.Error, e.UserState.ToString());
                Logger.Log(e.Error);
            }
        }

        void Downloader_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Downloader.DownloadStringCompleted -= Downloader_DownloadStringCompleted;
            if (e.Error != null)
            {
                Logger.Log(e.Error);
            }
            else
            {
                string GameVersion = GameInfo.assets;
                FileHelper.CreateDirectoryForFile(AppDomain.CurrentDomain.BaseDirectory + ".minecraft/assets/indexes/" + GameVersion + ".json");
                StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + ".minecraft/assets/indexes/" + GameVersion + ".json");
                sw.Write(e.Result);
                sw.Close();
                JavaScriptSerializer JSSerializer = new JavaScriptSerializer();
                Dictionary<string, Dictionary<string, AssetsEntity>> AssetsObject = JSSerializer.Deserialize<Dictionary<string, Dictionary<string, AssetsEntity>>>(e.Result);
                Dictionary<string, AssetsEntity> obj = AssetsObject["objects"];
                Logger.Log("共", obj.Count.ToString(), "项assets");
                int i = 0;
                foreach (KeyValuePair<string, AssetsEntity> entity in obj)
                {
                    i++;
                    string Url = FrmMain.URL_RESOURCE_BASE + entity.Value.hash.Substring(0, 2) + "/" + entity.Value.hash;
                    string File = AppDomain.CurrentDomain.BaseDirectory + @".minecraft\assets\objects\" + entity.Value.hash.Substring(0, 2) + "\\" + entity.Value.hash;
                    FileHelper.CreateDirectoryForFile(File);
                    try
                    {
                        if (FileHelper.IfFileVaild(File, entity.Value.size)) continue;
                        if (init)
                        {
                            FrmMain.NIcon.ShowBalloonTip(3000, "BMCL", Lang.LangManager.GetLangFromResource("FoundAssetsModify"), System.Windows.Forms.ToolTipIcon.Info);
                            init = false;
                        }
                        //Downloader.DownloadFileAsync(new Uri(Url), File,Url);
                        Downloader.DownloadFile(new Uri(Url), File);
                        Logger.Log(i.ToString(), obj.Count.ToString(), File.Substring(AppDomain.CurrentDomain.BaseDirectory.Length), "下载完毕");
                    }
                    catch (WebException ex)
                    {
                        Logger.Log(ex);
                    }
                }
                if (init)
                {
                    Logger.Log("无需更新assets");
                }
            }
            
        }
    }
}
