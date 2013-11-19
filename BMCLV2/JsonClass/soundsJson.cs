using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace BMCLV2.JsonClass
{
    [DataContract]
    class soundsEntity
    {
        [DataMember]
        public string name;
        [DataMember]
        public bool stream;
    }
}
