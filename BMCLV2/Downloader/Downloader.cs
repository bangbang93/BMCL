using System;
using System.Net;
using System.Threading.Tasks;
using BMCLV2.util;

namespace BMCLV2.Downloader
{
    public class Downloader:WebClient
    {
        public Downloader()
        {
            Headers.Add("User-Agent", "BMCLNG " + BmclCore.BmclVersion);
        }

        public static async Task<string> GetString(string url)
        {
            return await new Downloader().DownloadStringTaskAsync(new Uri(url));
        }

        public static async Task GetFile(string uri, string path)
        {
            FileHelper.CreateDirectoryForFile(path);
            await new Downloader().DownloadFileTaskAsync(uri, path);
        }

        public new Task<string> DownloadStringTaskAsync(Uri uri)
        {
            Logger.log(uri.ToString());
            return base.DownloadStringTaskAsync(uri);
        }

        public new Task<string> DownloadStringTaskAsync(string uri)
        {
            Logger.log(uri);
            return base.DownloadStringTaskAsync(uri);
        }

        public new Task DownloadFileTaskAsync(Uri uri, string path)
        {
            Logger.log(uri.ToString());
            return base.DownloadFileTaskAsync(uri, path);
        }

        public new Task DownloadFileTaskAsync(string uri, string path)
        {
            Logger.log(uri);
            return base.DownloadFileTaskAsync(uri, path);
        }
    }
}
