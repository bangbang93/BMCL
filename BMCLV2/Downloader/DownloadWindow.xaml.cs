using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using BMCLV2.Game;

namespace BMCLV2.Downloader
{
  /// <summary>
  /// DownloadWindow.xaml 的交互逻辑
  /// </summary>
  public partial class DownloadWindow : Window
  {
    private DownloadInfo[] _downloadInfos;
    private Task _downloadTask;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    public DownloadWindow(DownloadInfo[] downloads)
    {
      InitializeComponent();
      _downloadInfos = downloads;
      DownloadList.ItemsSource = downloads.ToList();

      var ct = new CancellationTokenSource().Token;
      _downloadTask = Task.Run(async () =>
      {
        foreach (var downloadInfo in _downloadInfos)
        {
          BmclCore.Dispatcher.Invoke(() => DownloadList.ScrollIntoView(downloadInfo));
          if (ct.IsCancellationRequested) return;
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
    }

    public async Task WaitForFinish()
    {
      await _downloadTask;
      Close();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
      _cancellationTokenSource.Cancel();
      Close();
    }
  }
}
