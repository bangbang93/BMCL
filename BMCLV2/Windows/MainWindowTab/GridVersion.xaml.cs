using System;
using System.Data;
using System.Globalization;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Input;
using BMCLV2.I18N;
using MessageBox = System.Windows.MessageBox;
using Version = BMCLV2.Downloader.Version;

namespace BMCLV2.Windows.MainWindowTab
{
    /// <summary>
    /// GridVersion.xaml 的交互逻辑
    /// </summary>
    public partial class GridVersion
    {
        private FrmPrs _prs;

        public GridVersion()
        {
            InitializeComponent();
        }

        private async void btnRefreshRemoteVer_Click(object sender, RoutedEventArgs e)
        {
            btnRefreshRemoteVer.IsEnabled = false;
            var versionMirror = BmclCore.MirrorManager.CurrectMirror.Version;
            await versionMirror.Refresh();
            listRemoteVer.DataContext = versionMirror.GetDataTable();
            btnRefreshRemoteVer.IsEnabled = true;
        }
        private async void btnDownloadVer_Click(object sender, RoutedEventArgs e)
        {
            if (listRemoteVer.SelectedItem == null)
            {
                MessageBox.Show(LangManager.GetLangFromResource("RemoteVerErrorNoVersionSelect"));
                return;
            }
            var selectVer = listRemoteVer.SelectedItem as DataRowView;
            if (selectVer == null) return;
            var url = selectVer[3] as string;
            var versionDownloader = new Version(url);
            _prs = new FrmPrs(LangManager.GetLangFromResource("btnDownloadVer"));
            _prs.Show();
            versionDownloader.ProcessChange += VersionDownloader_ProcessChange;
            await versionDownloader.Start();
            Logger.Log("下载客户端文件成功");
            MessageBox.Show(LangManager.GetLangFromResource("RemoteVerDownloadSuccess"));
            btnDownloadVer.Content = LangManager.GetLangFromResource("btnDownloadVer");
            btnDownloadVer.IsEnabled = true;
            BmclCore.MainWindow.GridGame.ReFlushlistver();
            BmclCore.MainWindow.SwitchDownloadGrid(Visibility.Hidden);
            BmclCore.MainWindow.TabMain.SelectedIndex = 0;
            _prs.Close();
            _prs = null;
        }

        private void VersionDownloader_ProcessChange(string status)
        {
            _prs.ChangeStatus(status);
        }
        private void listRemoteVer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnDownloadVer_Click(null, null);
        }

        public void RefreshVersion()
        {
            this.btnRefreshRemoteVer_Click(null, null);
        }

    }
}
