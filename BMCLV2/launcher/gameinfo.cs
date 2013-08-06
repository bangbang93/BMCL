using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections;

using BMCLV2.libraries;

namespace BMCLV2
{
    [DataContract]
    public class gameinfo : ICloneable
    {
        [DataMember(Order = 0, IsRequired = true)]
        public string id = "";
        [DataMember(Order = 1, IsRequired = false)]
        public string time = "";
        [DataMember(Order = 2, IsRequired = false)]
        public string releaseTime = "";
        [DataMember(Order = 3, IsRequired = false)]
        public string type = "";
        [DataMember(Order = 4, IsRequired = true)]
        public string minecraftArguments = "";
        [DataMember(Order = 5, IsRequired = true)]
        public string mainClass = "";
        [DataMember(Order = 6, IsRequired = true)]
        public libraryies[] libraries = null;
        [DataMember(Order = 7, IsRequired = false)]
        public int minimumLauncherVersion = 0;
        object ICloneable.Clone()
        {
            return this.clone();
        }
        public gameinfo clone()
        {
            return (gameinfo)this.MemberwiseClone();
        }
    }
}
