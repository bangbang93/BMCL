using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows.Forms.VisualStyles;

namespace BMCLV2.JsonClass
{
    [DataContract]
    public class ForgeVersion
    {
        [DataMember] public string time, minecraft, version;

        [DataMember]
        public Downloads downloads;
        public class Downloads
        {
            [DataMember]
            public string changelog;

            [DataMember] public string[] universal, src, javadoc, installer;
        }
    }
}
