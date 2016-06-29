using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BMCLV2.Forge;
using BMCLV2.I18N;
using BMCLV2.JsonClass;

namespace BMCLV2.Windows.MainWindowTab
{
    /// <summary>
    /// GridForge.xaml 的交互逻辑
    /// </summary>
    public partial class GridForge
    {
        public GridForge()
        {
            InitializeComponent();
        }

        private readonly ForgeTask _forgeTask = new ForgeTask();
        private ForgeVersion[] _forgeVersions;
        private async void RefreshForgeVersionList()
        {
            treeForgeVer.Items.Add(LangManager.GetLangFromResource("ForgeListGetting"));
            _forgeVersions = await _forgeTask.GetVersion();
            treeForgeVer.Items.Clear();
            var versionList = new SortedList<string, TreeViewItem>();
            foreach (var version in _forgeVersions)
            {
                var mc = version.GetMc();
                if (mc == null) continue;
                if (!versionList.ContainsKey(mc))
                {
                    versionList[version.GetMc()] = new TreeViewItem()
                    {
                        Header = version.GetMc()
                    };
                }
                versionList[version.GetMc()].Items.Add(version.name);
            }
            foreach (var treeViewItem in versionList)
            {
                treeForgeVer.Items.Add(treeViewItem.Value);
            }
            btnReForge.Content = LangManager.GetLangFromResource("btnReForge");
            btnReForge.IsEnabled = true;
            btnLastForge.IsEnabled = true;
        }

        private void btnLastForge_Click(object sender, RoutedEventArgs e)
        {
            DownloadForge("Latest");
        }
        private void btnReForge_Click(object sender, RoutedEventArgs e)
        {
            if (btnReForge.Content.ToString() == LangManager.GetLangFromResource("btnReForgeGetting"))
                return;
            btnReForge.Content = LangManager.GetLangFromResource("btnReForgeGetting");
            btnReForge.IsEnabled = false;
            btnLastForge.IsEnabled = false;
            RefreshForgeVersionList();
        }
        private async void DownloadForge(string ver)
        {
            var forgeVersion = _forgeVersions.First(version => version.name == ver);
            await _forgeTask.DownloadForge(forgeVersion);
            BmclCore.MainWindow.GridGame.ReFlushlistver();
            BmclCore.MainWindow.TabMain.SelectedIndex = 0;
        }
        private void treeForgeVer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = treeForgeVer.SelectedItem as string;
            if (selectedItem != null)
            {
                DownloadForge(selectedItem);
            }
        }

        private void txtInsPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtInsPath.Text);
                MessageBox.Show(LangManager.GetLangFromResource("ForgeCopySuccess"));
            }
            catch
            {
                MessageBox.Show(LangManager.GetLangFromResource("ForgeCopyError"));
            }
        }

        private void treeForgeVer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = treeForgeVer.SelectedItem as string;
            if (selectedItem != null)
            {
                if (_forgeTask.ForgeChangeLogUrl.ContainsKey(selectedItem))
                {
                    txtChangeLog.Text = LangManager.GetLangFromResource("FetchingForgeChangeLog");
                    var getLog = new WebClient();
                    getLog.DownloadStringCompleted += GetLog_DownloadStringCompleted;
                    getLog.DownloadStringAsync(new Uri(_forgeTask.ForgeChangeLogUrl[(string) treeForgeVer.SelectedItem]));
                } 
                else
                {
                    MessageBox.Show(LangManager.GetLangFromResource("ForgeDoNotHaveChangeLog"));
                }
            }
        }

        void GetLog_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            txtChangeLog.Text = e.Result;
        }

        public void RefreshForge()
        {
            this.btnReForge_Click(null, null);
        }
    }
}
