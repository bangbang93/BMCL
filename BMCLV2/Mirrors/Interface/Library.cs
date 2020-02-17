using System.Threading.Tasks;
using BMCLV2.Game;

namespace BMCLV2.Mirrors.Interface
{
  public abstract class Library
  {
    protected Downloader.Downloader Downloader => new Downloader.Downloader();

    public abstract Task DownloadLibrary(LibraryInfo library, string savePath);
  }
}
