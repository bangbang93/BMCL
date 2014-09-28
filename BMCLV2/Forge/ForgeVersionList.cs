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
        public delegate void ForgePageReadyHandle();
        public event ForgePageReadyHandle ForgePageReadyEvent;
        private DataContractJsonSerializer ForgeVerJsonParse = new DataContractJsonSerializer(typeof(ForgeVersion[]));
        private ForgeVersion[] ForgeNew, ForgeLegacy;
        public Dictionary<string, string> ForgeDownloadUrl = new Dictionary<string, string>(), 
            ForgeChangeLogUrl = new Dictionary<string, string>();
        public void GetVersion()
        {
            
            bool NewPageReady = false, OldPageReady = false;
            Thread thOldPage = new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(() => 
                {
                    WebClient wc = new WebClient();
                    wc.Headers.Add("User-Agent", "BMCL" + BmclCore.BmclVersion);
                    byte[] buffer = wc.DownloadData(OldPageUrl);
                    MemoryStream ms = new MemoryStream(buffer);
                    ForgeLegacy = ForgeVerJsonParse.ReadObject(ms) as ForgeVersion[];
                    OldPageReady = true;
                    Logger.log("获取Legcy Forge列表成功");
                    if (OldPageReady && NewPageReady)
                    {
                        Logger.log("开始解析Forge");
                        if (ForgePageReadyEvent != null)
                            ForgePageReadyEvent();
                    }
                })));
            Thread thNewPage = new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(() =>
                {
                    WebClient wc = new WebClient();
                    wc.Headers.Add("User-Agent", "BMCL" + BmclCore.BmclVersion);
                    byte[] buffer = wc.DownloadData(NewPageUrl);
                    MemoryStream ms = new MemoryStream(buffer);
                    ForgeNew = ForgeVerJsonParse.ReadObject(ms) as ForgeVersion[];
                    NewPageReady = true;
                    Logger.log("获取new Forge列表成功");
                    if (OldPageReady && NewPageReady)
                    {
                        Logger.log("开始解析Forge");
                        if (ForgePageReadyEvent != null)
                            ForgePageReadyEvent();
                    }
                })));
            thOldPage.Start();
            thNewPage.Start();
        }

        public TreeViewItem[] GetNew()
        {
            ArrayList r = new ArrayList(ForgeNew.Length);
            TreeViewItem t = new TreeViewItem();
            foreach (ForgeVersion Forge in ForgeNew)
            {
                if (Forge.installer == null)
                {
                    continue;
                }
                if (t.Header == null)
                {
                    t.Header = Forge.mcver;
                }
                if (t.Header.ToString() != Forge.mcver)
                {
                    t = new TreeViewItem();
                    r.Add(t);
                }
                if (ForgeDownloadUrl.ContainsKey(Forge.vername))
                    ForgeDownloadUrl[Forge.vername] = Forge.installer[1];
                else
                    ForgeDownloadUrl.Add(Forge.vername, Forge.installer[1]);
                if (ForgeChangeLogUrl.ContainsKey(Forge.vername))
                    ForgeChangeLogUrl[Forge.vername] = Forge.installer[1];
                else
                    ForgeChangeLogUrl.Add(Forge.vername, Forge.changelog);
                t.Items.Add(Forge.vername);
                Logger.log("获取Forge"+Forge.vername);
            }
            return r.ToArray(typeof(TreeViewItem)) as TreeViewItem[];
        }

        public TreeViewItem[] GetLegacy()
        {
            ArrayList r = new ArrayList(ForgeLegacy.Length);
            TreeViewItem t = new TreeViewItem();
            foreach (ForgeVersion Forge in ForgeLegacy)
            {
                if (Forge.installer == null)
                {
                    continue;
                }
                if (t.Header == null)
                {
                    t.Header = Forge.mcver;
                }
                if (t.Header.ToString() != Forge.mcver)
                {
                    t = new TreeViewItem();
                    r.Add(t);
                }
                if (ForgeDownloadUrl.ContainsKey(Forge.vername))
                    ForgeDownloadUrl[Forge.vername] = Forge.installer[1];
                else
                    ForgeDownloadUrl.Add(Forge.vername, Forge.installer[1]);
                if (ForgeChangeLogUrl.ContainsKey(Forge.vername))
                    ForgeChangeLogUrl[Forge.vername] = Forge.installer[1];
                else
                    ForgeChangeLogUrl.Add(Forge.vername, Forge.changelog);
                t.Items.Add(Forge.vername);
//                Logger.log("获取Forge" + Forge.vername);
            }
            return r.ToArray(typeof(TreeViewItem)) as TreeViewItem[];
        }
    }
}
