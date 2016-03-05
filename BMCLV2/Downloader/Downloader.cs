using System;
using System.Net;
using System.Threading.Tasks;

namespace BMCLV2.Downloader
{
    public class Downloader:WebClient
    {
        public Downloader()
        {
            Headers.Add("User-Agent", "BMCLNG " + BmclCore.BmclVersion);
        }

        public async static Task<string> GetString(string url)
        {
            return await new Downloader().DownloadStringTaskAsync(new Uri(url));
        }
    }
}
