using System.IO;
using System.Net;
using System.Runtime.Serialization;
using BMCLV2.util;

namespace BMCLV2.Objects.Mirrors
{
    public class LibraryInfo
    {
        public class ExtractRule
        {
            [DataMember (Name = "exclude")] public string[] Exclude;
        }

        public class Download
        {
            public class ArtifactInfo
            {
                [DataMember(Name= "size")] public int Size;
                [DataMember(Name = "sha1")] public string Sha1;
                [DataMember(Name = "path")] public string Path;
                [DataMember(Name= "url")] public string Url;
            }

            public class ClassifiersInfo
            {
                [DataMember(Name = "natives-linux")] public ArtifactInfo Linux;
                [DataMember(Name = "natives-osx")] public ArtifactInfo OSX;
                [DataMember(Name = "natives-windows")] public ArtifactInfo Windows;
            }

            [DataMember] public ArtifactInfo Artifact;
            [DataMember] public ClassifiersInfo Classifiers;
        }

        public class Rule
        {
            public class OSInfo
            {
                [DataMember(Name = "name")] public string Name;
            }

            [DataMember(Name="action")] public string Action;
            [DataMember(Name="os")] public OSInfo OS;
        }

        [DataMember(Name = "name")] public string Name;
        [DataMember(Name = "downloads")] public Download Downloads;
        [DataMember(Name = "rules")] public Rule[] Rules;
        [DataMember(Name= "extract")] public ExtractRule Extract;

        public string Path => Rules == null ? Downloads.Artifact.Path : Downloads.Classifiers.Windows.Path;

        public string Url => Rules == null ? Downloads.Artifact.Url : Downloads.Classifiers.Windows.Url;

        public string Sha1 => Rules == null ? Downloads.Artifact.Sha1 : Downloads.Classifiers.Windows.Sha1;

        public int Size => Rules == null ? Downloads.Artifact.Size : Downloads.Classifiers.Windows.Size;

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