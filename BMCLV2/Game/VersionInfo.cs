using System.Runtime.Serialization;

namespace BMCLV2.Game
{
  [DataContract]
  public class VersionInfo
  {
    [DataMember(Name = "arguments")] public VersionArguments Arguments;
    [DataMember(Name = "assetIndex")] public VersionAssetIndex AssetIndex;
    [DataMember(Name = "assets")] public string Assets;
    [DataMember(Name = "downloads")] public VersionDownload Downloads;

    [DataMember(Name = "id")] public string Id;
    [DataMember(Name = "inheritsFrom")] public string InheritsFrom;
    [DataMember(Name = "jar")] public string Jar;
    [DataMember(Name = "libraries")] public LibraryInfo[] Libraries;
    [DataMember(Name = "mainClass")] public string MainClass;

    [DataMember(Name = "minecraftArguments")]
    public string MinecraftArguments;

    [DataMember(Name = "minimumLauncherVersion")]
    public string MinimumLauncherVersion;

    [DataMember(Name = "releaseTime")] public string ReleaseTime;
    [DataMember(Name = "time")] public string Time;
    [DataMember(Name = "type")] public string Type;

    [DataContract]
    public class VersionAssetIndex
    {
      [DataMember(Name = "id")] public string Id;
      [DataMember(Name = "sha1")] public string Sha1;
      [DataMember(Name = "size")] public string Size;
      [DataMember(Name = "totalSize")] public string TotalSize;
      [DataMember(Name = "url")] public string Url;
    }

    [DataContract]
    public class VersionDownload
    {
      [DataMember(Name = "client")] public File Client;
      [DataMember(Name = "server")] public File Server;

      [DataContract]
      public class File
      {
        [DataMember(Name = "sha1")] public string Sha1;
        [DataMember(Name = "size")] public string Size;
        [DataMember(Name = "url")] public string Url;
      }
    }

    [DataContract]
    public class VersionArguments
    {
      [DataMember(Name = "game")] public object[] Game;
      [DataMember(Name = "jvm")] public object[] Jvm;
    }
  }
}
