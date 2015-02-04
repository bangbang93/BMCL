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
using BMCLV2.Lang;
using BMCLV2.Versions;
using MessageBox = System.Windows.MessageBox;

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
        private void btnRefreshRemoteVer_Click(object sender, RoutedEventArgs e)
        {
            btnRefreshRemoteVer.IsEnabled = false;
            listRemoteVer.DataContext = null;
            var rawJson = new DataContractJsonSerializer(typeof(RawVersionListType));
            var getJson = (HttpWebRequest)WebRequest.Create(BmclCore.UrlDownloadBase + "versions/versions.json");
            getJson.Timeout = 10000;
            getJson.ReadWriteTimeout = 10000;
            getJson.UserAgent = "BMCL" + BmclCore.BmclVersion;
            var thGet = new Thread(new ThreadStart(delegate
            {
                try
                {
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { btnRefreshRemoteVer.Content = LangManager.GetLangFromResource("RemoteVerGetting");}));
                    var getJsonAns = (HttpWebResponse)getJson.GetResponse();
                    // ReSharper disable once AssignNullToNotNullAttribute
                    var remoteVersion = rawJson.ReadObject(getJsonAns.GetResponseStream()) as RawVersionListType;
                    var dt = new DataTable();
                    dt.Columns.Add("Ver");
                    dt.Columns.Add("RelTime");
                    dt.Columns.Add("Type");
                    if (remoteVersion != null)
                        foreach (RemoteVerType rv in remoteVersion.getVersions())
                        {
                            dt.Rows.Add(new object[] { rv.id, rv.releaseTime, rv.type });
                        }
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                    {
                        btnRefreshRemoteVer.Content = LangManager.GetLangFromResource("btnRefreshRemoteVer");
                        btnRefreshRemoteVer.IsEnabled = true;
                        listRemoteVer.DataContext = dt;
                        listRemoteVer.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("RelTime", System.ComponentModel.ListSortDirection.Descending));
                    }));
                }
                catch (WebException ex)
                {
                    MessageBox.Show(LangManager.GetLangFromResource("RemoteVerFailedTimeout") + "\n" + ex.Message);
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                    {
                        btnRefreshRemoteVer.Content = LangManager.GetLangFromResource("btnRefreshRemoteVer");
                        btnRefreshRemoteVer.IsEnabled = true;
                    }));
                }
                catch (TimeoutException ex)
                {
                    MessageBox.Show(LangManager.GetLangFromResource("RemoteVerFailedTimeout") + "\n" + ex.Message);
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                    {
                        btnRefreshRemoteVer.Content = LangManager.GetLangFromResource("btnRefreshRemoteVer");
                        btnRefreshRemoteVer.IsEnabled = true;
                    }));
                }
            }));
            thGet.Start();
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
                var downer = new WebClient();
                downer.Headers.Add("User-Agent", "BMCL" + BmclCore.BmclVersion);
                var downurl = new StringBuilder(BmclCore.UrlDownloadBase);
                downurl.Append(@"versions\");
                downurl.Append(selectver).Append("\\");
                downurl.Append(selectver).Append(".jar");
#if DEBUG
                MessageBox.Show(downpath + "\n" + downurl);
#endif
                btnDownloadVer.Content = LangManager.GetLangFromResource("RemoteVerDownloading");
                btnDownloadVer.IsEnabled = false;
                // ReSharper disable once AssignNullToNotNullAttribute
                if (!Directory.Exists(Path.GetDirectoryName(downpath.ToString())))
                {
// ReSharper disable AssignNullToNotNullAttribute
                    Directory.CreateDirectory(Path.GetDirectoryName(downpath.ToString()));
// ReSharper restore AssignNullToNotNullAttribute
                }
                string downjsonfile = downurl.ToString().Substring(0, downurl.Length - 4) + ".json";
                string downjsonpath = downpath.ToString().Substring(0, downpath.Length - 4) + ".json";
                try
                {
                    downer.DownloadFileCompleted += downer_DownloadClientFileCompleted;
                    downer.DownloadProgressChanged += downer_DownloadProgressChanged;
                    Logger.log("下载:" + downjsonfile);
                    downer.DownloadFile(new Uri(downjsonfile), downjsonpath);
                    Logger.log("下载:" + downurl);
                    downer.DownloadFileAsync(new Uri(downurl.ToString()), downpath.ToString());
                    _downedtime = Environment.TickCount - 1;
                    _downed = 0;
                    BmclCore.MainWindow.SwitchDownloadGrid(Visibility.Visible);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n");
                    btnDownloadVer.Content = LangManager.GetLangFromResource("btnDownloadVer");
                    btnDownloadVer.IsEnabled = true;
                }
            }
        }
        int _downedtime;
        int _downed;
        void downer_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            BmclCore.MainWindow.ChangeDownloadProgress((int)e.BytesReceived, (int)e.TotalBytesToReceive);
            //            TaskbarManager.Instance.SetProgressValue((int)e.BytesReceived, (int)e.TotalBytesToReceive);
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
        private void btnCheckRes_Click(object sender, RoutedEventArgs e)
        {
            if (
                MessageBox.Show(LangManager.GetLangFromResource("ResourceDeprecatedWarning"), "BMCL", MessageBoxButton.OKCancel) ==
                MessageBoxResult.OK)
            {
                var checkres = new FrmCheckRes();
                checkres.Show();
            }
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
