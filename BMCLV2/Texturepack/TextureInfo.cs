using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

namespace BMCLV2.Texturepack
{
    [DataContract]
    public class TextureInfo
    {
        [DataMember]
        public Pack pack;
        [DataMember]
        public MemoryStream Logo;
        public TextureInfo()
        {
            pack = new Pack();
            Logo = new MemoryStream();
        }
    }

    [DataContract]
    public class Pack
    {
        [DataMember]
        public int pack_format;
        [DataMember]
        public string description;
        public Pack()
        {
            pack_format = 1;
            description = "";
        }
    }
}
