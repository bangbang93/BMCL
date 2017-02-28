using System.Threading.Tasks;
using BMCLV2.Game;

namespace BMCLV2.Mirrors.Vanilla
{
    public class Library : Interface.Library
    {
        private const string Server = "https://libraries.minecraft.net/";

        public override async Task DownloadLibrary(LibraryInfo library, string savePath)
        {
            var url = library.Url ?? Server + library.Path;
            await Downloader.DownloadFileTaskAsync(url, savePath);
        }
    }
}