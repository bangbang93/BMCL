using System.Runtime.Serialization;

namespace BMCLV2.JsonClass
{
  [DataContract]
  public class FileSchema
  {
    [DataMember(Name = "sha1")] public string Sha1;
    [DataMember(Name = "size")] public int Size;
    [DataMember(Name = "url")] public string Url;
  }
}
