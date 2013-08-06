using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace BMCLV2.libraries
{
    [DataContract]
    public class rules
    {
        [DataMember(Order = 0, IsRequired = true)]
        public string action;
        [DataMember(Order = 1, IsRequired = false)]
        public ros os;
        [DataMember(Order = 2, IsRequired = false)]
        public string version;
    }
}
