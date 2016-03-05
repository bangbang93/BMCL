using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using BMCLV2.I18N;
using BMCLV2.Mirrors;
using BMCLV2.Versions;
using MessageBox = System.Windows.MessageBox;
using Version = BMCLV2.Mirrors.Interface.Version;

namespace BMCLV2.Windows.MainWindowTab
{
    /// <summary>
    /// GridVersion.xaml 的交互逻辑
    /// </summary>
    public partial class GridVersion
    {

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
        private void btnDownloadVer_Click(object sender, RoutedEventArgs e)
        {
            if (listRemoteVer.SelectedItem == null)
            {
                MessageBox.Show(LangManager.GetLangFromResource("RemoteVerErrorNoVersionSelect"));
                return;
            }
            var selectVer = listRemoteVer.SelectedItem as DataRowView;
            if (selectVer != null)
            {
                var selectver = selectVer[0] as string;
                var downpath = new StringBuilder(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\versions\");
                downpath.Append(selectver).Append("\\");
                downpath.Append(selectver).Append(".jar");
            }
        }
        int _downedtime;
        int _downed;
        void downer_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            BmclCore.MainWindow.ChangeDownloadProgress((int)e.BytesReceived, (int)e.TotalBytesToReceive);
            var info = new StringBuilder(LangManager.GetLangFromResource("DownloadSpeedInfo"));
            try
            {
                info.Append(((e.BytesReceived - _downed) / ((Environment.TickCount - _downedtime) / 1000.0) / 1024.0).ToString("F2")).Append("KB/s,");
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
            //            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
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
