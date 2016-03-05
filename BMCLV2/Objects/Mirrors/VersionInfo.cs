using System.Net;
using System.Runtime.Serialization;
// ReSharper disable InconsistentNaming

namespace BMCLV2.Objects.Mirrors
{
    public struct VersionInfo
    {
        public struct Downloads
        {
            public struct DownlaodFile
            {
                [DataMember] public string sha1;
                [DataMember] public int size;
                [DataMember] public string url;
            }

            [DataMember] public DownlaodFile client;
            [DataMember] public DownlaodFile server;
        }

        public struct AssetIndex
        {
            [DataMember] public string id;
            [DataMember] public string sha1;
            [DataMember] public int size;
            [DataMember] public string url;
            [DataMember] public int totalSize;
        }

        [DataMember] public AssetIndex assetIndex;
        [DataMember] public string assets;
        [DataMember] public Downloads downloads;
        [DataMember] public string id;
        [DataMember] public LibraryInfo[] Library;
        [DataMember] public string mainClass;
        [DataMember] public string minecraftArguments;
        [DataMember] public int minimumLauncherVersion;
        [DataMember] public string releaseTime;
        [DataMember] public string time;
        [DataMember] public string type;
    }
}