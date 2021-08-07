using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using BMCLV2.util;
using BMCLV2.Util;

namespace BMCLV2.Game
{
  [DataContract]
  public class LibraryInfo
  {
    public enum Type
    {
      Library,
      Native
    }


    private Dictionary<string, List<Rule.OSInfo>> _rule;
    [DataMember(Name = "downloads")] public Download Downloads;
    [DataMember(Name = "extract")] public ExtractRule Extract;

    [DataMember(Name = "name")] public string Name;
    [DataMember(Name = "natives")] public NativesName Natives;
    [DataMember(Name = "rules")] public Rule[] Rules;

    public bool IsNative => Natives != null;

    [OnDeserialized]
    public void OnDeserialized(StreamingContext context)
    {
      if (Rules == null) return;
      _rule = new Dictionary<string, List<Rule.OSInfo>>
      {
        { "allow", new List<Rule.OSInfo>() },
        { "disallow", new List<Rule.OSInfo>() }
      };
      foreach (var rule in Rules)
      {
        var ruleList = _rule[rule.Action] ?? new List<Rule.OSInfo>();
        ruleList.Add(rule.OS);
        _rule[rule.Action] = ruleList;
      }
    }

    public bool HasLibrary()
    {
      if (Downloads == null) return true;
      return Downloads.Artifact != null;
    }

    public bool ShouldDeployOnOs(string os = "windows", string version = null)
    {
      if (Rules == null) return true;
      var disallow = _rule["disallow"];
      var allow = _rule["allow"];
      if (disallow.Count != 0 && allow.Count == 0) return disallow.All(osInfo => osInfo.Name != os);
      if (allow.Count != 0 && disallow.Count == 0) return allow.Any(osInfo => osInfo.Name == os);
      return true;
    }

    public bool IsVaildLibrary(string libraryPath)
    {
      var path = Path.Combine(libraryPath, GetLibraryPath());

      var fileInfo = new FileInfo(path);
      var library = GetLibrary();
      if (library == null) return fileInfo.Exists;
      if (GetLibrary().Size == 0 || GetLibrary().Sha1 == null)
        return fileInfo.Exists;
      return fileInfo.Exists
             && fileInfo.Length == GetLibrary().Size
             && Crypto.GetSha1HashFromFile(path) == GetLibrary().Sha1;
    }

    public bool IsValidNative(string libraryPath)
    {
      var path = Path.Combine(libraryPath, GetNativePath());

      var fileInfo = new FileInfo(path);
      if (GetNative().Size == 0 || GetNative().Sha1 == null)
        return fileInfo.Exists && fileInfo.Length > 0;
      return fileInfo.Exists
             && fileInfo.Length == GetNative().Size
             && Crypto.GetSha1HashFromFile(path) == GetNative().Sha1;
    }

    public string GetLibraryPath()
    {
      return Downloads?.Artifact.Path ?? PathHelper.ParseJavaLibraryNameToPath(Name);
    }

    public string GetNativePath()
    {
      if (Downloads?.Classifiers != null)
      {
        var classifiers = Downloads.Classifiers;
        if (Environment.Is64BitOperatingSystem && classifiers.Windowsx64 != null) return classifiers.Windowsx64.Path;
        if (classifiers.Windowsx32 != null) return classifiers.Windowsx32.Path;
        return classifiers.Windows.Path;
      }

      var libp = new StringBuilder();
      var split = Name.Split(':'); //0 包;1 名字；2 版本
      libp.Append(split[0].Replace('.', '\\'));
      libp.Append("\\");
      libp.Append(split[1]).Append("\\");
      libp.Append(split[2]).Append("\\");
      libp.Append(split[1]).Append("-").Append(split[2]).Append("-").Append(Natives.Windows);
      libp.Append(".jar");
      libp.Replace("${arch}", Environment.Is64BitOperatingSystem ? "64" : "32");
      return libp.ToString();
    }

    public Download.ArtifactInfo GetLibrary()
    {
      return Downloads?.Artifact;
    }

    public Download.ArtifactInfo GetNative()
    {
      Download.ArtifactInfo path = null;
      if (Downloads?.Classifiers != null)
        path = (Environment.Is64BitOperatingSystem
          ? Downloads.Classifiers.Windowsx64
          : Downloads.Classifiers.Windowsx32) ?? Downloads.Classifiers.Windows;
      if (path != null) return path;
      path = new Download.ArtifactInfo
      {
        Path = GetNativePath()
      };
      return path;
    }

    private Download.ArtifactInfo GetArtifact()
    {
      if (IsNative && Downloads.Classifiers.Windows == null)
        return Environment.Is64BitOperatingSystem
          ? Downloads.Classifiers.Windowsx64
          : Downloads.Classifiers.Windowsx32;
      return Downloads.Classifiers?.Windows ?? Downloads.Artifact;
    }

    [DataContract]
    public class ExtractRule
    {
      [DataMember(Name = "exclude")] public string[] Exclude;
    }

    [DataContract]
    public class Download
    {
      [DataMember(Name = "artifact")] public ArtifactInfo Artifact;
      [DataMember(Name = "classifiers")] public ClassifiersInfo Classifiers;

      [DataContract]
      public class ArtifactInfo
      {
        [DataMember(Name = "path")] public string Path;
        [DataMember(Name = "sha1")] public string Sha1;
        [DataMember(Name = "size")] public int Size;
        [DataMember(Name = "url")] public string Url;
      }

      [DataContract]
      public class ClassifiersInfo
      {
        [DataMember(Name = "natives-linux")] public ArtifactInfo Linux;
        [DataMember(Name = "natives-osx")] public ArtifactInfo OSX;
        [DataMember(Name = "natives-windows")] public ArtifactInfo Windows;

        [DataMember(Name = "natives-windows-32")]
        public ArtifactInfo Windowsx32;

        [DataMember(Name = "natives-windows-64")]
        public ArtifactInfo Windowsx64;
      }
    }

    [DataContract]
    public class Rule
    {
      [DataMember(Name = "action")] public string Action;
      [DataMember(Name = "os")] public OSInfo OS;

      [DataContract]
      public class OSInfo
      {
        [DataMember(Name = "name")] public string Name;
      }
    }

    [DataContract]
    public class NativesName
    {
      [DataMember(Name = "linux")] public string Linux;
      [DataMember(Name = "osx")] public string OSX;
      [DataMember(Name = "windows")] public string Windows;
    }
  }
}
