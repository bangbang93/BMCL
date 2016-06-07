using System.Threading.Tasks;
using BMCLV2.Objects.Mirrors;

namespace BMCLV2.Mirrors.Vanilla
{
    public class Library : Interface.Library
    {
        private const string Server = "https://libraries.minecraft.net/";

        public override async Task DownloadLibrary(LibraryInfo library, string savePath)
        {
            var path = library.Path;
            var url = library.Url ?? Server + library.Path;
            await Downloader.DownloadFileTaskAsync(path, url);
        }
    }
}