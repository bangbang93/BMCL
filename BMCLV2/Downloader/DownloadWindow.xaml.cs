using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BMCLV2.Annotations;
using BMCLV2.Util;

namespace BMCLV2.Downloader
{
  /// <summary>
  /// DownloadWindow.xaml 的交互逻辑
  /// </summary>
  public partial class DownloadWindow: INotifyPropertyChanged
  {
    public string ProgressStatus => $"{ProgressValue}/{_downloadInfos.Length}";
    public int ProgressValue { get; private set; }

    public int ProgressMax => _downloadInfos.Length;

    private readonly DownloadInfo[] _downloadInfos;
    private Task _downloadTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public event PropertyChangedEventHandler PropertyChanged;

    public DownloadWindow(DownloadInfo[] downloads)
    {
      InitializeComponent();
      _downloadInfos = downloads;
      DownloadList.ItemsSource = downloads.ToList();
      Container.DataContext = this;
    }

    public async Task StartDownload()
    {
      var ct = _cancellationTokenSource.Token;
      _downloadTask = Task.Run(async () =>
      {
        foreach (var downloadInfo in _downloadInfos)
        {
          ProgressValue++;
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressValue"));
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressStatus"));
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

    private void DownloadWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed)
      {
        DragMove();
      }
    }
  }
}
