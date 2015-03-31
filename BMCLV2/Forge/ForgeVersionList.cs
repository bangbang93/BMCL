using System;
using System.Collections.Generic;
using System.Drawing.Text;
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
        private const string FORGE_API_URL = "http://bmclapi2.bangbang93.com/forge/last";
        private string OldPageUrl = "http://bmclapi.bangbang93.com/forge/legacylist";  //这两个API允许对外开放，如果有人想要用的话就用吧。
        private string NewPageUrl = "http://bmclapi.bangbang93.com/forge/versionlist";
        public delegate void ForgePageReadyHandle();
        public event ForgePageReadyHandle ForgePageReadyEvent;
        private DataContractJsonSerializer ForgeVerJsonParse = new DataContractJsonSerializer(typeof(ForgeVersion[]));
        private ForgeVersion[] Forge;
        public Dictionary<string, string> ForgeDownloadUrl = new Dictionary<string, string>(), 
            ForgeChangeLogUrl = new Dictionary<string, string>();
        public void GetVersion()
        {
            WebClient wc = new WebClient();
            wc.DownloadStringAsync(new Uri("http://bmclapi2.bangbang93.com/forge/last"));
            wc.DownloadStringCompleted += wc_DownloadStringCompleted;
        }

        void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {

            }
            else
            {
                Forge = ForgeVerJsonParse.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(e.Result))) as ForgeVersion[];
                ForgePageReadyEvent();
            }
        }

        public TreeViewItem[] GetNew()
        {
            ArrayList r = new ArrayList(Forge.Length);
            TreeViewItem t = new TreeViewItem();
            foreach (ForgeVersion forge in Forge)
            {
                if (forge.downloads.installer.Length != 2)
                {
                    continue;
                }
                if (t.Header == null)
                {
                    t.Header = forge.minecraft;
                }
                if (t.Header.ToString() != forge.minecraft)
                {
                    r.Add(t);
                    t = new TreeViewItem();
                }
                if (ForgeDownloadUrl.ContainsKey(forge.version))
                    ForgeDownloadUrl[forge.version] = forge.downloads.installer[1].Replace("http://files.minecraftforge.net/", "http://bmclapi2.bangbang93.com/");
                else
                    ForgeDownloadUrl.Add(forge.version, forge.downloads.installer[1].Replace("http://files.minecraftforge.net/", "http://bmclapi2.bangbang93.com/"));
                if (ForgeChangeLogUrl.ContainsKey(forge.version))
                    ForgeChangeLogUrl[forge.version] = forge.downloads.installer[1];
                else
                    ForgeChangeLogUrl.Add(forge.version, forge.downloads.changelog);
                t.Items.Add(forge.version);
                Logger.log("获取Forge" + forge.version);
            }
            return r.ToArray(typeof(TreeViewItem)) as TreeViewItem[];
        }
    }
}
