using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Collections;
using System.Threading;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;

using BMCLV2.JsonClass;

namespace BMCLV2.Forge
{
    class ForgeVersionList
    {
        private string OldPageUrl = "http://bmclapi.bangbang93.com/forge/legacylist";  //这两个API允许对外开放，如果有人想要用的话就用吧。
        private string NewPageUrl = "http://bmclapi.bangbang93.com/forge/versionlist";
        private string NewUrl = "http://bmclapi2.bangbang93.com/forge/last";
        public delegate void ForgePageReadyHandle();
        public event ForgePageReadyHandle ForgePageReadyEvent;
        private DataContractJsonSerializer ForgeVerJsonParse = new DataContractJsonSerializer(typeof(ForgeVersion[]));
        private ForgeVersion[] ForgeNew, ForgeLegacy;
        public Dictionary<string, string> ForgeDownloadUrl = new Dictionary<string, string>(), 
            ForgeChangeLogUrl = new Dictionary<string, string>();
        public void GetVersion()
        {
            var webClient = new WebClient();
            webClient.DownloadStringAsync(new Uri(NewUrl));
            webClient.DownloadStringCompleted += WebClient_DownloadStringCompleted;
        }

        private void WebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Logger.log("获取forge列表失败");
                Logger.error(e.Error);
            }
            else
            {
                ForgeNew =
                    ForgeVerJsonParse.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(e.Result))) as ForgeVersion[];
                Logger.log("获取Forge列表成功");
            }
            ForgePageReadyEvent?.Invoke();
        }

        public TreeViewItem[] GetNew()
        {
            ArrayList r = new ArrayList(ForgeNew.Length);
            TreeViewItem t = new TreeViewItem();
            r.Add(t);
            foreach (ForgeVersion Forge in ForgeNew)
            {
                Forge.minecraft = Forge.minecraft.Trim();
                Forge.version = Forge.version.Trim();
                if (Forge.downloads.installer == null || Forge.downloads.installer.Length == 0)
                {
                    continue;
                }
                if (t.Header == null)
                {
                    t.Header = Forge.minecraft;
                }
                if (t.Header.ToString() != Forge.minecraft)
                {
                    t = new TreeViewItem();
                    r.Add(t);
                }
                if (ForgeDownloadUrl.ContainsKey(Forge.version))
                    ForgeDownloadUrl[Forge.version] = Forge.downloads.installer[1];
                else
                    ForgeDownloadUrl.Add(Forge.version, Forge.downloads.installer[1]);
                if (ForgeChangeLogUrl.ContainsKey(Forge.version))
                    ForgeChangeLogUrl[Forge.version] = Forge.downloads.changelog;
                else
                    ForgeChangeLogUrl.Add(Forge.version, Forge.downloads.changelog);
                t.Items.Add(Forge.version);
                Logger.log("获取Forge"+Forge.version);
            }
            return r.ToArray(typeof(TreeViewItem)) as TreeViewItem[];
        }
    }
}
