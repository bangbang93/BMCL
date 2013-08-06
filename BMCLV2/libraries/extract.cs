using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace BMCLV2.libraries
{
    [DataContract]
    public class extract
    {
        [DataMember(Order = 0, IsRequired = false)]
        public string[] exclude;
    }
}
