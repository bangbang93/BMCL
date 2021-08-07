using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using BMCLV2.util;

namespace BMCLV2.Downloader
{
  public class Downloader : WebClient
  {
    public Downloader()
    {
      Headers.Add("User-Agent", "BMCLNG/" + BmclCore.BmclVersion);
    }

    public new Task<string> DownloadStringTaskAsync(Uri uri)
    {
      Logger.Log(uri.ToString());
      return base.DownloadStringTaskAsync(uri);
    }

    public new Task<string> DownloadStringTaskAsync(string uri)
    {
      Logger.Log(uri);
      return base.DownloadStringTaskAsync(uri);
    }

    public new async Task DownloadFileTaskAsync(Uri uri, string path)
    {
      Logger.Log($"url: {uri}, path: {path}");
      FileHelper.CreateDirectoryForFile(path);
      var buffer = await DownloadDataTaskAsync(uri);
      File.WriteAllBytes(path, buffer);
    }

    public new async Task DownloadFileTaskAsync(string uri, string path)
    {
      await DownloadFileTaskAsync(new Uri(uri), path);
    }
  }
}
