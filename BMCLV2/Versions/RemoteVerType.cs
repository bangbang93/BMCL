using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace BMCLV2.Versions
{
    [DataContract]
    class RemoteVerType
    {
        [DataMember(Order = 0, IsRequired = true)]
        public string id;
        [DataMember(Order = 1, IsRequired = true)]
        public string time;
        [DataMember(Order = 2, IsRequired = true)]
        public string releaseTime;
        [DataMember(Order = 3, IsRequired = true)]
        public string type;

        public RemoteVerType()
        {
            id = "";
            time = "";
            releaseTime = "";
            type = "";
        }
    }
}
