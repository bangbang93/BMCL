using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using BMCLV2.JsonClass;

namespace BMCLV2.Mojang.Runtime
{
  public class JavaMeta
  {
    private static Dictionary<string, Dictionary<string, MetaSchema[]>> _meta;

    public async Task<Dictionary<string, Dictionary<string, MetaSchema[]>>> FetchMeta()
    {
      if (_meta != null) return _meta;
      var json = await BmclCore.MirrorManager.CurrentMirror.Version.DownloadJson(
        "https://launchermeta.mojang.com/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json");
      _meta = new JSON<Dictionary<string, Dictionary<string, MetaSchema[]>>>().Parse(json);
      return _meta;
    }

    [DataContract]
    public class MetaSchema
    {
      [DataMember(Name = "availability")] public AvailabilitySchema Availability;
      [DataMember(Name = "manifest")] public FileSchema Manifest;
      [DataMember(Name = "version")] public VersionSchema Version;

      [DataContract]
      public class AvailabilitySchema
      {
        [DataMember(Name = "group")] public int Group;
        [DataMember(Name = "progress")] public int Progress;
      }

      [DataContract]
      public class VersionSchema
      {
        [DataMember(Name = "name")] public string Name;
        [DataMember(Name = "released")] public string Released;
      }
    }
  }
}
