using System.Net;

namespace BMCLV2.Downloader
{
    public class Downloader:WebClient
    {
        public Downloader()
        {
            Headers.Add("User-Agent", "BMCLNG " + BmclCore.BmclVersion);
        }
    }
}
