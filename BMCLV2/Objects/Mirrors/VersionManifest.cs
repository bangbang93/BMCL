using System.Runtime.Serialization;
// ReSharper disable InconsistentNaming

namespace BMCLV2.Objects.Mirrors
{
    public class VersionManifest
    {
        [DataMember] public Latest latest;
        [DataMember] public Version[] versions;

        public class Latest
        {
            [DataMember] public string snapshot;
            [DataMember] public string release;
        }

        public class Version
        {
            [DataMember]
            public string id;
            [DataMember]
            public string type;
            [DataMember]
            public string time;
            [DataMember]
            public string releaseTime;
            [DataMember]
            public string url;
        }
    }
}