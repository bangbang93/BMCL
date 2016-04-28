using System.Threading.Tasks;
using BMCLV2.Objects.Mirrors;

namespace BMCLV2.Mirrors.Interface
{
    public abstract class Library
    {
        protected readonly Downloader.Downloader Downloader = new Downloader.Downloader();

        public abstract Task DownloadLibrary(LibraryInfo library, string savePath);
    }
}