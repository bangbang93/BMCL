using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMCLV2.JsonClass
{
    [DataContract]
    public class AssetsIndex
    {
        [DataContract]
        public class Assets
        {
            [DataMember(Name = "hash")] public string Hash;
            [DataMember(Name = "size")] public int Size;

            public string Path => $"{Hash.Substring(0, 2)}\\{Hash}";
        }

        [DataMember(Name = "objects")] public Dictionary<string, Assets> Objects;
    }
}