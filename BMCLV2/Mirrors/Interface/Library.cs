using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BMCLV2.Game;

namespace BMCLV2.Mirrors.Interface
{
  public abstract class Library
  {
    public List<Regex> Replaces = new List<Regex>
    {
      new Regex(@"http[s]*://libraries\.minecraft\.net/"),
      new Regex(@"http[s]*://files\.minecraftforge\.net/maven/"),
      new Regex(@"http[s]*://maven\.minecraftforge\.net/")
    };

    protected Downloader.Downloader Downloader => new Downloader.Downloader();

    public abstract Task DownloadLibrary(LibraryInfo library, string savePath);
  }
}
