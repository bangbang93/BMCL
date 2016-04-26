using System.IO;
using System.Net;
using System.Runtime.Serialization;
using BMCLV2.util;

namespace BMCLV2.Objects.Mirrors
{
    [DataContract]
    public class LibraryInfo
    {
        [DataContract]
        public class ExtractRule
        {
            [DataMember (Name = "exclude")] public string[] Exclude;
        }
        [DataContract]
        public class Download
        {
            [DataContract]
            public class ArtifactInfo
            {
                [DataMember(Name= "size")] public int Size;
                [DataMember(Name = "sha1")] public string Sha1;
                [DataMember(Name = "path")] public string Path;
                [DataMember(Name= "url")] public string Url;
            }
            [DataContract]
            public class ClassifiersInfo
            {
                [DataMember(Name = "natives-linux")] public ArtifactInfo Linux;
                [DataMember(Name = "natives-osx")] public ArtifactInfo OSX;
                [DataMember(Name = "natives-windows")] public ArtifactInfo Windows;
            }

            [DataMember(Name = "artifact")] public ArtifactInfo Artifact;
            [DataMember(Name = "classifiers")] public ClassifiersInfo Classifiers;
        }
        [DataContract]
        public class Rule
        {
            [DataContract]
            public class OSInfo
            {
                [DataMember(Name = "name")] public string Name;
            }

            [DataMember(Name="action")] public string Action;
            [DataMember(Name="os")] public OSInfo OS;
        }

        [DataContract]
        public class NativesName
        {
            [DataMember(Name = "linux")] public string Linux;
            [DataMember(Name = "osx")] public string OSX;
            [DataMember(Name = "windows")] public string Windows;
        }

        [DataMember(Name = "name")] public string Name;
        [DataMember(Name = "downloads")] public Download Downloads;
        [DataMember(Name = "rules")] public Rule[] Rules;
        [DataMember(Name= "extract")] public ExtractRule Extract;
        [DataMember(Name = "natives")] public NativesName Natives;

        public string Path => Downloads.Artifact != null ? Downloads.Artifact.Path : Downloads.Classifiers.Windows.Path;

        public string Url => Downloads.Artifact != null ? Downloads.Artifact.Url : Downloads.Classifiers.Windows.Url;

        public string Sha1 => Downloads.Artifact != null ? Downloads.Artifact.Sha1 : Downloads.Classifiers.Windows.Sha1;

        public int Size => Downloads.Artifact?.Size ?? Downloads.Classifiers.Windows.Size;

        public bool IsNative => Natives != null;

        public bool ShouldDeployOnOs(string os = "windows", string version = null)
        {
            if (Rules == null) return true;
            foreach (var rule in Rules)
            {
                if (rule.Action == "allow")
                {
                    return rule.OS.Name == os;
                }
                return rule.OS.Name != os;
            }
            return true;
        }

        public bool IsVaild(string libraryPath)
        {
            var path = System.IO.Path.Combine(libraryPath, Path);
            var fileInfo = new FileInfo(path);
            return fileInfo.Exists 
                && fileInfo.Length == Size 
                && Crypto.GetSha1HashFromFile(path) == Sha1;
        }
    }
}