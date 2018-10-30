using System.Runtime.Serialization;

namespace BMCLV2.Game
{
  [DataContract]
  public class VersionInfo
  {
    [DataContract]
    public class VersionAssetIndex
    {
      [DataMember(Name = "id")] public string Id;
      [DataMember(Name = "sha1")] public string Sha1;
      [DataMember(Name = "size")] public string Size;
      [DataMember(Name = "url")] public string Url;
      [DataMember(Name = "totalSize")] public string TotalSize;
    }

    [DataContract]
    public class VersionDownload
    {
      [DataContract]
      public class File
      {
        [DataMember(Name = "sha1")] public string Sha1;
        [DataMember(Name = "size")] public string Size;
        [DataMember(Name = "url")] public string Url;
      }

      [DataMember(Name = "client")] public File Client;
      [DataMember(Name = "server")] public File Server;
    }

    [DataContract]
    public class VersionArguments
    {
      [DataMember(Name = "game")] public object[] Game;
      [DataMember(Name = "jvm")] public object[] Jvm;
    }

    [DataMember(Name = "id")] public string Id;
    [DataMember(Name = "time")] public string Time;
    [DataMember(Name = "releaseTime")] public string ReleaseTime;
    [DataMember(Name = "type")] public string Type;
    [DataMember(Name = "minecraftArguments")] public string MinecraftArguments;
    [DataMember(Name = "mainClass")] public string MainClass;
    [DataMember(Name = "minimumLauncherVersion")] public string MinimumLauncherVersion;
    [DataMember(Name = "inheritsFrom")] public string InheritsFrom;
    [DataMember(Name = "assetIndex")] public VersionAssetIndex AssetIndex;
    [DataMember(Name = "libraries")] public LibraryInfo[] Libraries;
    [DataMember(Name = "downloads")] public VersionDownload Downloads;
    [DataMember(Name = "assets")] public string Assets;
    [DataMember(Name = "jar")] public string Jar;
    [DataMember(Name = "arguments")] public VersionArguments Arguments;

  }
}
