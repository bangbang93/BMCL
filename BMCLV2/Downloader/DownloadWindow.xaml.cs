using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BMCLV2.Util;

namespace BMCLV2.Downloader
{
  /// <summary>
  /// DownloadWindow.xaml 的交互逻辑
  /// </summary>
  public partial class DownloadWindow
  {
    private readonly DownloadInfo[] _downloadInfos;
    private Task _downloadTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public DownloadWindow(DownloadInfo[] downloads)
    {
      InitializeComponent();
      _downloadInfos = downloads;
      DownloadList.ItemsSource = downloads.ToList();
    }

    public async Task StartDownload()
    {
      var ct = _cancellationTokenSource.Token;
      _downloadTask = Task.Run(async () =>
      {
        foreach (var downloadInfo in _downloadInfos)
        {
          BmclCore.Dispatcher.Invoke(() => DownloadList.ScrollIntoView(downloadInfo));

          ct.ThrowIfCancellationRequested();

          if (File.Exists(downloadInfo.SavePath))
          {
            var sha1 = Crypto.GetSha1HashFromFile(downloadInfo.SavePath);
            if (sha1 == downloadInfo.Sha1)
            {
              downloadInfo.Complete = downloadInfo.Size;
              continue;
            }
          }

          var downloader = new Downloader();
          downloader.DownloadProgressChanged += (sender, args) =>
          {
            ct.ThrowIfCancellationRequested();
            downloadInfo.Complete = args.BytesReceived;
            downloadInfo.Size = args.TotalBytesToReceive;
          };
          await downloader.DownloadFileTaskAsync(downloadInfo.Uri, downloadInfo.SavePath);
        }
      }, ct);
      await _downloadTask;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
      _cancellationTokenSource.Cancel();
      Close();
    }
  }
}
