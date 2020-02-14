using System.Threading.Tasks;
using BMCLV2.Optifine;

namespace BMCLV2.Mirrors.Interface
{
  public abstract class Optifine
  {
    protected Downloader.Downloader Downloader => new Downloader.Downloader();

    public abstract Task<VersionInfo[]> GetVersionList();

    public abstract Task Download(string mcversion, string type, string patch, string path);
  }
}
