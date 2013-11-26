using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Collections;
using System.Threading;

using HtmlAgilityPack;

namespace BMCLV2.Forge
{
    class ForgeVersionList
    {
        private HtmlDocument ForgeOldPage, ForgeNewPage;
        private HtmlWeb ForgeOldPageGet = new HtmlWeb(), ForgeNewPageGet = new HtmlWeb();
        private string OldPageUrl = "http://files.minecraftforge.net/minecraftforge/index_legacy.html";
        private string NewPageUrl = "http://files.minecraftforge.net/";
        public delegate void ForgePageReadyHandle();
        public event ForgePageReadyHandle ForgePageReadyEvent;
        public Dictionary<string, string> ForgeDownloadUrl = new Dictionary<string, string>(), ForgeChangeLogUrl = new Dictionary<string, string>();
        public void GetVersion()
        {
            Thread thOldPage = new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(() => ForgeOldPage = ForgeOldPageGet.Load(OldPageUrl))));
            Thread thNewPage = new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(() => ForgeNewPage = ForgeNewPageGet.Load(NewPageUrl))));
            Thread thWaiting = new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(() =>
            {
                while (ForgeOldPage == null || ForgeNewPage == null) ;
                if (ForgePageReadyEvent != null)
                    ForgePageReadyEvent();
            })));
            thWaiting.Start();
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
                ForgeDownloadUrl.Add(ver, url);
                ForgeChangeLogUrl.Add(ver, changelogurl);
                tree.Items.Add(ver);
            }
            trees.Add(tree);
            #endregion


            return trees.ToArray(typeof(TreeViewItem)) as TreeViewItem[];
        }
    }
}
