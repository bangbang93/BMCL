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
        [DataMember] public string minecraft, time, version;

        [DataMember] public Download downloads;
        public class Download
        {
            public string changelog;
            public string[] universal;
            public string[] src;
            public string[] javadoc;
            public string[] installer;
        }
    }
}
