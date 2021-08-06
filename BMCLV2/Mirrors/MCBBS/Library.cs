using System.Threading.Tasks;
using BMCLV2.Game;

namespace BMCLV2.Mirrors.MCBBS
{
  public class Library : Interface.Library
  {
    private const string Server = "https://download.mcbbs.net/maven/";

    public override async Task DownloadLibrary(LibraryInfo library, string savePath)
    {
      if (library.HasLibrary())
      {
        var url = library.GetLibrary()?.Url;
        if (string.IsNullOrEmpty(url)) url = $"{Server}{library.GetLibraryPath().Replace('\\', '/')}";
        foreach (var replace in Replaces) url = replace.Replace(url, Server);
        Logger.Info(url);
        await Downloader.DownloadFileTaskAsync(url, savePath);
      }

      if (library.IsNative)
      {
        var url = library.GetNative().Url;
        if (string.IsNullOrEmpty(url)) url = $"{Server}{library.GetNativePath().Replace('\\', '/')}";
        foreach (var replace in Replaces) url = replace.Replace(url, Server);
        await Downloader.DownloadFileTaskAsync(url, savePath);
      }
    }
  }
}
