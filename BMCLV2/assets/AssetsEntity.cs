using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace BMCLV2.assets
{
    [DataContract]
    public class AssetsEntity
    {
        [DataMember]
        public string hash;
        [DataMember]
        public int size;
    }
}
