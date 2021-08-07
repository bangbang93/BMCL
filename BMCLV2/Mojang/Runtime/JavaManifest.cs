using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMCLV2.JsonClass;

namespace BMCLV2.Mojang.Runtime
{
  public class JavaManifest
  {
    private readonly string _url;

    public JavaManifest(string url)
    {
      _url = url;
    }

    public async Task<ManifestSchema> FetchManifest()
    {
      var cache = BmclCore.Cache.Get(_url);
      string json;
      if (cache != null)
      {
        json = Encoding.Default.GetString(cache);
      }
      else
      {
        json = await BmclCore.MirrorManager.CurrentMirror.Version.DownloadJson(_url);
        BmclCore.Cache.Set(_url, json);
      }
      return new JSON<ManifestSchema>().Parse(json);
    }

    [DataContract]
    public class ManifestSchema
    {
      [DataMember(Name = "files")] public Dictionary<string, FilesSchema> Files;

      [DataContract]
        public class FilesSchema
        {
          [DataMember(Name = "type")] public string Type;
          [DataMember(Name = "executable")] public bool Executable;
          [DataMember(Name = "downloads")] public Dictionary<string, FileSchema> Downloads;
        }
    }
  }
}
