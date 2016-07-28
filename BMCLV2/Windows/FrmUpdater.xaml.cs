using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Windows;

namespace BMCLV2.Windows
{
    /// <summary>
    /// FrmUpdater.xaml 的交互逻辑
    /// </summary>
    public partial class FrmUpdater
    {
        private readonly int _build;
        private readonly string _url;
        private readonly WebClient _client = new WebClient();
        public FrmUpdater(int build, string url)
        {
            _build = build;
            _url = url;
            InitializeComponent();
            _client.DownloadProgressChanged += ClientOnDownloadProgressChanged;
            _client.DownloadFileCompleted += ClientOnDownloadFileCompleted;
            _client.DownloadFileAsync(new Uri(_url), "BMCL." + _build + ".exe");
        }

        private void ClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            Close();
            _client.Dispose();
            Process.Start("BMCL." + _build + ".exe", "-Update");
            Thread.Sleep(1000);
            Logger.Log($"BMCLNG Ver.{BmclCore.BmclVersion} 由于更新正在退出");
            Environment.Exit(0);
        }

        private void ClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
        {
            PrsBar.Maximum = downloadProgressChangedEventArgs.TotalBytesToReceive;
            PrsBar.Value = downloadProgressChangedEventArgs.BytesReceived;
            LabInfo.Content = "BMCL更新中   " + downloadProgressChangedEventArgs.ProgressPercentage.ToString(CultureInfo.InvariantCulture) + "%";
//            var left = (this.Width - LabInfo.Width)/2;
//            LabInfo.Margin = new Thickness(left, LabInfo.Margin.Top, left, LabInfo.Margin.Bottom);
        }
    }
}
