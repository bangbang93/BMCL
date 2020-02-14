using System.Runtime.Serialization;

namespace BMCLV2.Optifine
{
  [DataContract]
  public class VersionInfo
  {
    [DataMember(Name = "mcversion")] public string McVersion;
    [DataMember(Name = "type")] public string Type;
    [DataMember(Name = "patch")] public string Patch;
    [DataMember(Name = "_id")] public string Id;
  }
}
