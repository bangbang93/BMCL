using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Collections;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;

using BMCLV2.JsonClass;

namespace BMCLV2.Forge
{
    class ForgeVersionList
    {
        private readonly string _newUrl = "http://bmclapi2.bangbang93.com/forge/last";
        public delegate void ForgePageReadyHandle();
        public event ForgePageReadyHandle ForgePageReadyEvent;
        private readonly DataContractJsonSerializer _forgeVerJsonParse = new DataContractJsonSerializer(typeof(ForgeVersion[]));
        private ForgeVersion[] _forgeNew;
        public Dictionary<string, string> ForgeDownloadUrl = new Dictionary<string, string>(), 
            ForgeChangeLogUrl = new Dictionary<string, string>();
        public void GetVersion()
        {
            var webClient = new WebClient();
            webClient.DownloadStringAsync(new Uri(_newUrl));
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
                _forgeNew =
                    _forgeVerJsonParse.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(e.Result))) as ForgeVersion[];
                Logger.log("获取Forge列表成功");
            }
            ForgePageReadyEvent?.Invoke();
        }

        public TreeViewItem[] GetNew()
        {
            ArrayList arrayList = new ArrayList(_forgeNew.Length);
            TreeViewItem treeViewItem = new TreeViewItem();
            arrayList.Add(treeViewItem);
            foreach (ForgeVersion forge in _forgeNew)
            {
                forge.minecraft = forge.minecraft.Trim();
                forge.version = forge.version.Trim();
                if (forge.downloads.installer == null || forge.downloads.installer.Length == 0)
                {
                    Logger.log(forge.version, "for", forge.minecraft, "does not have installer");
                    continue;
                }
                if (treeViewItem.Header == null)
                {
                    treeViewItem.Header = forge.minecraft;
                }
                else
                {
                    if (treeViewItem.Header.ToString() != forge.minecraft)
                    {
                        treeViewItem = new TreeViewItem();
                        arrayList.Add(treeViewItem);
                        treeViewItem.Header = forge.minecraft;
                    }
                }
                Logger.log(treeViewItem.Header.ToString());
                Logger.log(forge.minecraft);
                ForgeDownloadUrl[forge.version] = forge.downloads.installer[1];
                ForgeChangeLogUrl[forge.version] = forge.downloads.changelog;
                treeViewItem.Items.Add(forge.version);
                Logger.log("获取Forge", forge.version);
            }
            return arrayList.ToArray(typeof(TreeViewItem)) as TreeViewItem[];
        }
    }
}
