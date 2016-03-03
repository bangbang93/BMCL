using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using BMCLV2.Objects.Mirrors;

namespace BMCLV2.Mirrors.Interface
{
    public abstract class Version
    {
        protected readonly Downloader.Downloader Downloader = new Downloader.Downloader();
        protected VersionManifest VersionManifest;
        protected string Url = "http://bmclapi2.bangbang93.com/mc/game/version_manifest.json";

            
        public VersionManifest.Latest GetLatest()
        {
            return VersionManifest.latest;
        }

        public VersionManifest.Version[] GetVersions()
        {
            return VersionManifest.versions;
        }

        public VersionManifest.Version GetVersion(string id)
        {
            return VersionManifest.versions.FirstOrDefault(version => version.id == id);
        }

        public async Task Refresh()
        {
            var json =
                await Downloader.DownloadStringTaskAsync(
                    new Uri(Url));
            var serializer = new JavaScriptSerializer();
            VersionManifest = serializer.DeserializeObject(json) as VersionManifest;
        }
    }
}