using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace BMCLV2.libraries
{
    [DataContract]
    public class ros
    {
        [DataMember(Order = 0, IsRequired = true)]
        public string name;
        [DataMember(Order = 1, IsRequired = false)]
        public string version;
    }
}
