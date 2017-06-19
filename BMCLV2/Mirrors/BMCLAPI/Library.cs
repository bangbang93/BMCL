using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BMCLV2.Game;

namespace BMCLV2.Mirrors.BMCLAPI
{
  public class Library : Interface.Library
  {
    private const string Server = "https://bmclapi2.bangbang93.com/maven/";
    private readonly Regex _vanillaServer = new Regex(@"http[s]*://libraries\.minecraft\.net/");

    public override async Task DownloadLibrary(LibraryInfo library, string savePath)
    {
      if (library.HasLibrary())
      {
        var url = library.GetLibrary()?.Url ?? Server + library.GetLibraryPath();
        url = _vanillaServer.Replace(url, Server);
        await Downloader.DownloadFileTaskAsync(url, savePath);
      }
      if (library.IsNative)
      {
        var url = library.GetNative().Url ?? Server + library.GetNativePath();
        url = _vanillaServer.Replace(url, Server);
        await Downloader.DownloadFileTaskAsync(url, savePath);
      }
    }
  }
}
