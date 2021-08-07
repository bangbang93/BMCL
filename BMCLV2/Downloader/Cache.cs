using System;
using System.IO;
using System.Text;
using BMCLV2.util;

namespace BMCLV2.Downloader
{
  public class Cache
  {
    public byte[] Get(string url)
    {
      var uri = new Uri(url);
      var path = Path.Combine(BmclCore.CacheDirectory, uri.LocalPath.Substring(1));
      return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }

    public void Set(string url, byte[] content)
    {
      var uri = new Uri(url);
      var path = Path.Combine(BmclCore.CacheDirectory, uri.LocalPath.Substring(1));
      FileHelper.CreateDirectoryForFile(path);
      File.WriteAllBytes(path, content);
    }

    public void Set(string url, string content)
    {
      Set(url, Encoding.Default.GetBytes(content));
    }
  }
}
