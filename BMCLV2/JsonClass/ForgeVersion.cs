using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace BMCLV2.JsonClass
{
    [DataContract]
    public class ForgeVersion
    {
        [DataMember]
        public string vername, ver, mcver, releasetime, changelog;
        [DataMember]
        public string[] installer, javadoc, src, universal;
    }
}
