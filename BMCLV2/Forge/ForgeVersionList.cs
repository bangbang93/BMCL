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

using HtmlAgilityPack;

using BMCLV2.JsonClass;

namespace BMCLV2.Forge
{
    class ForgeVersionList
    {
        private HtmlDocument ForgeOldPage = new HtmlDocument(), ForgeNewPage = new HtmlDocument();
        private string OldPageUrl = "http://bmclapi.bangbang93.com/forge/legacylist";  //这两个API允许对外开放，如果有人想要用的话就用吧。
        private string NewPageUrl = "http://bmclapi.bangbang93.com/forge/versionlist";
        public delegate void ForgePageReadyHandle();
        public event ForgePageReadyHandle ForgePageReadyEvent;
        private DataContractJsonSerializer ForgeVerJsonParse = new DataContractJsonSerializer(typeof(ForgeVersion[]));
        private ForgeVersion[] ForgeNew, ForgeLegacy;
        public Dictionary<string, string> ForgeDownloadUrl = new Dictionary<string, string>(), ForgeChangeLogUrl = new Dictionary<string, string>();
        public void GetVersion()
        {
            
            bool NewPageReady = false, OldPageReady = false;
            Thread thOldPage = new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(() => 
                {
                    WebClient wc = new WebClient();
                    wc.Proxy = null;
                    byte[] buffer = wc.DownloadData(OldPageUrl);
                    MemoryStream ms = new MemoryStream(buffer);
                    ForgeLegacy = ForgeVerJsonParse.ReadObject(ms) as ForgeVersion[];
                    OldPageReady = true;
                    Logger.Log("获取Legcy Forge列表成功");
                    if (OldPageReady && NewPageReady)
                    {
                        Logger.Log("开始解析Forge");
                        if (ForgePageReadyEvent != null)
                            ForgePageReadyEvent();
                    }
                })));
            Thread thNewPage = new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(() =>
                {
                    WebClient wc = new WebClient();
                    byte[] buffer = wc.DownloadData(NewPageUrl);
                    MemoryStream ms = new MemoryStream(buffer);
                    ForgeNew = ForgeVerJsonParse.ReadObject(ms) as ForgeVersion[];
                    NewPageReady = true;
                    Logger.Log("获取new Forge列表成功");
                    if (OldPageReady && NewPageReady)
                    {
                        Logger.Log("开始解析Forge");
                        if (ForgePageReadyEvent != null)
                            ForgePageReadyEvent();
                    }
                })));
            thOldPage.Start();
            thNewPage.Start();
        }
        public TreeViewItem GetLastForge()
        {
            if (ForgeNewPage == null)
            {
                throw new ForgeListNotReadyException();
            }
            TreeViewItem tree = new TreeViewItem();
            HtmlNode promtions = ForgeNewPage.GetElementbyId("promotions_table");
            HtmlNodeCollection shortcuts = promtions.SelectNodes("tr");
            for (int i = 1; i < shortcuts.Count; i++)
            {
                HtmlNode shortcut = shortcuts[i];
                string ver = shortcut.SelectNodes("td")[0].InnerText;
                HtmlNodeCollection urls = shortcut.SelectNodes("td")[4].SelectNodes("a");
                string url = "nothing";
                string changelogurl = "nothing";
                foreach (HtmlNode maybeurl in urls)
                {
                    string murl = maybeurl.GetAttributeValue("href", "");
                    if (murl.IndexOf("adf.ly") == -1 && murl.IndexOf("installer") != -1)
                    {
                        url = murl;
                        continue;
                    }
                    if (murl.IndexOf("changelog") != -1)
                    {
                        changelogurl = murl;
                        continue;
                    }
                }
                if (url == "nothing")
                {
                    continue;
                }
                ForgeDownloadUrl.Add(ver, url);
                ForgeChangeLogUrl.Add(ver, changelogurl);
                tree.Header = ver;
                tree.Items.Add(shortcut.SelectNodes("td")[1].InnerText);
            }
            return tree;
        }
        public TreeViewItem[] GetAllBuild()
        {
            if (ForgeOldPage == null || ForgeNewPage == null)
            {
                throw new ForgeListNotReadyException();
            }
            ArrayList trees = new ArrayList();
            TreeViewItem tree = new TreeViewItem();
            #region 新地址解析
            tree = new TreeViewItem();
            HtmlNode VersionList = ForgeNewPage.GetElementbyId("view_version");
            HtmlNodeCollection Versions = VersionList.SelectNodes("option");
            ArrayList VersionStrBuilder = new ArrayList();
            foreach (HtmlNode Version in Versions)
            {
                if (Version.Attributes["Value"].Value == "all")
                    continue;
                VersionStrBuilder.Add(Version.Attributes["Value"].Value);
            }
            string[] VersionStr = VersionStrBuilder.ToArray(typeof(string)) as string[];
            foreach (string Version in VersionStr)
            {
                HtmlNode build = ForgeNewPage.GetElementbyId(Version + "_builds");
                HtmlNode table = build.SelectSingleNode("table");
                HtmlNodeCollection line = table.SelectNodes("tr");
                tree = new TreeViewItem();
                tree.Header = line[1].SelectNodes("td")[1].InnerText;
                for (int i = 1; i < line.Count; i++)
                {
                    HtmlNode shortcut = line[i];
                    if (shortcut.SelectSingleNode("th") != null)
                    {
                        trees.Add(tree);
                        tree = new TreeViewItem();
                        tree.Header = line[i + 1].SelectNodes("td")[1].InnerText;
                        continue;
                    }
                    string ver = shortcut.SelectNodes("td")[0].InnerText;
                    HtmlNodeCollection urls = shortcut.SelectNodes("td")[3].SelectNodes("a");
                    string url = "none";
                    string changelogurl = "none";
                    foreach (HtmlNode maybeurl in urls)
                    {
                        string murl = maybeurl.GetAttributeValue("href", "");
                        if (murl.IndexOf("adf.ly") == -1 && murl.IndexOf("installer.jar") != -1)
                        {
                            url = murl;
                            continue;
                        }
                        if (murl.IndexOf("changelog") != -1)
                        {
                            changelogurl = murl;
                            continue;
                        }
                    }
                    if (url == "none")
                    {
                        continue;
                    }
                    ForgeDownloadUrl.Add(ver, url);
                    ForgeChangeLogUrl.Add(ver, changelogurl);
                    tree.Items.Add(ver);
                }
                trees.Add(tree);
            }
            #endregion
            #region 旧地址解析
            HtmlNode promtions = ForgeOldPage.GetElementbyId("promotions_table");
            HtmlNodeCollection shortcuts = promtions.SelectNodes("tr");
            tree = new TreeViewItem();
            for (int i = 1; i < shortcuts.Count; i++)
            {
                HtmlNode shortcut = shortcuts[i];
                string ver = shortcut.SelectNodes("td")[0].InnerText;
                HtmlNodeCollection urls = shortcut.SelectNodes("td")[4].SelectNodes("a");
                string url = "none";
                string changelogurl = "none";
                foreach (HtmlNode maybeurl in urls)
                {
                    string murl = maybeurl.GetAttributeValue("href", "");
                    if (murl.IndexOf("adf.ly") == -1 && murl.IndexOf("installer") != -1)
                    {
                        url = murl;
                        continue;
                    }
                    if (murl.IndexOf("changelog") != -1)
                    {
                        changelogurl = murl;
                        continue;
                    }
                }
                if (url == "none")
                {
                    continue;
                }
                if (!ForgeDownloadUrl.ContainsKey(ver))
                {
                    ForgeDownloadUrl.Add(ver, url);
                    ForgeChangeLogUrl.Add(ver, changelogurl);
                }
                tree = new TreeViewItem();
                tree.Header = ver;
                tree.Items.Add(shortcut.SelectNodes("td")[1].InnerText);
                trees.Add(tree);
            }
            HtmlNode all_builds = ForgeOldPage.GetElementbyId("all_builds");
            HtmlNode Alltable = all_builds.SelectSingleNode("table");
            HtmlNodeCollection All = Alltable.SelectNodes("tr");
            tree = new TreeViewItem();
            tree.Header = All[1].SelectNodes("td")[1].InnerText;
            for (int i = 1; i < All.Count; i++)
            {
                HtmlNode shortcut = All[i];
                if (shortcut.SelectSingleNode("th") != null)
                {
                    trees.Add(tree);
                    tree = new TreeViewItem();
                    tree.Header = All[i + 1].SelectNodes("td")[1].InnerText;
                    continue;
                }
                string ver = shortcut.SelectNodes("td")[0].InnerText;
                HtmlNodeCollection urls = shortcut.SelectNodes("td")[3].SelectNodes("a");
                string url = "none";
                string changelogurl = "none";
                foreach (HtmlNode maybeurl in urls)
                {
                    string murl = maybeurl.GetAttributeValue("href", "");
                    if (murl.IndexOf("adf.ly") == -1 && murl.IndexOf("installer") != -1)
                    {
                        url = murl;
                        continue;
                    }
                    if (murl.IndexOf("changelog") != -1)
                    {
                        changelogurl = murl;
                        continue;
                    }
                }
                if (url == "none")
                {
                    continue;
                }
                if (!ForgeDownloadUrl.ContainsKey(ver))
                {
                    ForgeDownloadUrl.Add(ver, url);
                    ForgeChangeLogUrl.Add(ver, changelogurl);
                }
                tree.Items.Add(ver);
            }
            trees.Add(tree);
            #endregion


            return trees.ToArray(typeof(TreeViewItem)) as TreeViewItem[];
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
                    ForgeChangeLogUrl.Add(Forge.vername, Forge.changlog);
                t.Items.Add(Forge.vername);
                Logger.Log("获取Forge"+Forge.vername);
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
                    ForgeChangeLogUrl.Add(Forge.vername, Forge.changlog);
                t.Items.Add(Forge.vername);
                Logger.Log("获取Forge" + Forge.vername);
            }
            return r.ToArray(typeof(TreeViewItem)) as TreeViewItem[];
        }
    }
}
