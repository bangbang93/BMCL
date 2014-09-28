using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BMCLV2.Forge;
using BMCLV2.Lang;

namespace BMCLV2.Windows.MainWindowTab
{
    /// <summary>
    /// GridForge.xaml 的交互逻辑
    /// </summary>
    public partial class GridForge
    {
        int _downedtime;
        int _downed;

        public GridForge()
        {
            InitializeComponent();
        }

        readonly ForgeVersionList _forgeVer = new ForgeVersionList();
        private void RefreshForgeVersionList()
        {
            treeForgeVer.Items.Add(LangManager.GetLangFromResource("ForgeListGetting"));
            _forgeVer.ForgePageReadyEvent += ForgeVer_ForgePageReadyEvent;
            _forgeVer.GetVersion();
        }

        void ForgeVer_ForgePageReadyEvent()
        {
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(() =>
            {
                treeForgeVer.Items.Clear();
                foreach (TreeViewItem t in _forgeVer.GetNew())
                {
                    treeForgeVer.Items.Add(t);
                }
                foreach (TreeViewItem t in _forgeVer.GetLegacy())
                {
                    treeForgeVer.Items.Add(t);
                }
                btnReForge.Content = LangManager.GetLangFromResource("btnReForge");
                btnReForge.IsEnabled = true;
                btnLastForge.IsEnabled = true;
            }));
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
        private void DownloadForge(string ver)
        {
            if (!_forgeVer.ForgeDownloadUrl.ContainsKey(ver))
            {
                MessageBox.Show(LangManager.GetLangFromResource("ForgeDoNotSupportInstaller"));
                return;
            }
            BmclCore.Invoke(new Action(() => BmclCore.MainWindow.SwitchDownloadGrid(Visibility.Visible)));
            var url = new Uri(_forgeVer.ForgeDownloadUrl[ver]);
            var downer = new WebClient();
            downer.Headers.Add("User-Agent", "BMCL" + BmclCore.BmclVersion);
            downer.DownloadProgressChanged += downer_DownloadProgressChanged;
            downer.DownloadFileCompleted += downer_DownloadForgeCompleted;
            _downedtime = Environment.TickCount - 1;
            _downed = 0;
            var w = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\.minecraft\\launcher_profiles.json");
            w.Write(Resource.NormalProfile.Profile);
            w.Close();
            downer.DownloadFileAsync(url, "forge.jar");
        }

        void downer_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            BmclCore.MainWindow.ChangeDownloadProgress(e.BytesReceived, e.TotalBytesToReceive);
            var info = new StringBuilder(LangManager.GetLangFromResource("DownloadSpeedInfo"));
            try
            {
                info.Append(((e.BytesReceived - _downed) / ((Environment.TickCount - _downedtime) / 1000.0) / 1024.0).ToString("F2")).Append("KB/s,");
            }
            catch (DivideByZeroException) { info.Append("0B/s,"); }
            info.Append(e.ProgressPercentage.ToString(CultureInfo.InvariantCulture)).Append("%");
            BmclCore.MainWindow.SetDownloadInfo(info.ToString());
        }

        void downer_DownloadForgeCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtInsPath.Text);
                MessageBox.Show(LangManager.GetLangFromResource("ForgeInstallInfo"));
            }
            catch
            {
                MessageBox.Show(LangManager.GetLangFromResource("ForgeCopyError"));
            }
            var forgeIns = new Process();
            if (!File.Exists(BmclCore.Config.Javaw))
            {
                MessageBox.Show(LangManager.GetLangFromResource("ForgeJavaError"));
                return;
            }
            forgeIns.StartInfo.FileName = BmclCore.Config.Javaw;
            forgeIns.StartInfo.Arguments = "-jar \"" + AppDomain.CurrentDomain.BaseDirectory + "\\forge.jar\"";
            Logger.log(forgeIns.StartInfo.Arguments);
            forgeIns.Start();
            forgeIns.WaitForExit();
            BmclCore.MainWindow.GridGame.ReFlushlistver();
            BmclCore.MainWindow.TabMain.SelectedIndex = 0;
            BmclCore.MainWindow.SwitchDownloadGrid(Visibility.Hidden);
        }
        private void treeForgeVer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.treeForgeVer.SelectedItem == null)
                return;
            if (this.treeForgeVer.SelectedItem is string)
            {
                DownloadForge(this.treeForgeVer.SelectedItem as string);
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
            if (this.treeForgeVer.SelectedItem == null)
                return;
            if (this.treeForgeVer.SelectedItem is string)
            {
                if (_forgeVer.ForgeChangeLogUrl.ContainsKey(this.treeForgeVer.SelectedItem as string))
                {
                    txtChangeLog.Text = LangManager.GetLangFromResource("FetchingForgeChangeLog");
                    var getLog = new WebClient();
                    getLog.DownloadStringCompleted += GetLog_DownloadStringCompleted;
                    getLog.DownloadStringAsync(new Uri(_forgeVer.ForgeChangeLogUrl[this.treeForgeVer.SelectedItem as string]));
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
