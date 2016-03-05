using System;
using System.Data;
using System.Globalization;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Input;
using BMCLV2.I18N;
using MessageBox = System.Windows.MessageBox;

namespace BMCLV2.Windows.MainWindowTab
{
    /// <summary>
    /// GridVersion.xaml 的交互逻辑
    /// </summary>
    public partial class GridVersion
    {

        private int _downloadStartTime;
        public GridVersion()
        {
            InitializeComponent();
        }

        private async void btnRefreshRemoteVer_Click(object sender, RoutedEventArgs e)
        {
            btnRefreshRemoteVer.IsEnabled = false;
            await BmclCore.MirrorManager.CurrectMirror.Refresh();
            listRemoteVer.DataContext = BmclCore.MirrorManager.CurrectMirror.GetDataTable();
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
            if (selectVer != null)
            {
                var url = selectVer[3] as string;
                var downloader = new Downloader.Downloader();
                downloader.DownloadProgressChanged += downer_DownloadProgressChanged;
                downloader.DownloadFileCompleted += downer_DownloadClientFileCompleted;
                _downloadStartTime = Environment.TickCount;
            }
        }
        void downer_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            BmclCore.MainWindow.ChangeDownloadProgress((int)e.BytesReceived, (int)e.TotalBytesToReceive);
            var info = new StringBuilder(LangManager.GetLangFromResource("DownloadSpeedInfo"));
            try
            {
                long escapeTime = (Environment.TickCount - _downloadStartTime)/1000;
                info.Append(((double)e.BytesReceived / escapeTime / 1024.0).ToString("F2")).Append("KB/s,");
            }
            catch (DivideByZeroException) { info.Append("0B/s,"); }
            info.Append(e.ProgressPercentage.ToString(CultureInfo.InvariantCulture)).Append("%");
            BmclCore.MainWindow.SetDownloadInfo(info.ToString());
        }

        void downer_DownloadClientFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Logger.log("下载客户端文件成功");
            MessageBox.Show(LangManager.GetLangFromResource("RemoteVerDownloadSuccess"));
            btnDownloadVer.Content = LangManager.GetLangFromResource("btnDownloadVer");
            btnDownloadVer.IsEnabled = true;
            BmclCore.MainWindow.GridGame.ReFlushlistver();
            BmclCore.MainWindow.SwitchDownloadGrid(Visibility.Hidden);
            BmclCore.MainWindow.TabMain.SelectedIndex = 0;
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
