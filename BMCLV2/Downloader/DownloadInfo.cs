using System;
using System.ComponentModel;
using System.IO;
using System.Security.Policy;
using BMCLV2.JsonClass;

namespace BMCLV2.Downloader
{
  public class DownloadInfo: INotifyPropertyChanged
  {
    public string Name { get; }

    public readonly Uri Uri;
    public readonly string SavePath;
    public long Size;
    public string Sha1;

    public long Complete {
      get => _complete;
      set
      {
        _complete = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Progress"));
      }
    }

    public string Progress => Size == 0 ? "..." : $"{Math.Round((double)(Complete / Size * 100))}%";

    private long _complete;

    public DownloadInfo(string name, string uri, string savePath, long size = 0)
    {
      Name = name;
      Uri = new Uri(uri);
      SavePath = savePath;
      Size = size;
    }

    public DownloadInfo(string url, string savePath, long size = 0) : this(Path.GetFileName(url), url, savePath, size)
    { }

    public DownloadInfo(string savePath, FileSchema fileSchema) : this(Path.GetFileName(fileSchema.Url), fileSchema.Url, savePath, fileSchema.Size)
    {
      Sha1 = fileSchema.Sha1;
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
