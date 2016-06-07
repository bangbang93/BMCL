using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using BMCLV2.Launcher;
using BMCLV2.Util;

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



        private Dictionary<string, List<Rule.OSInfo>> _rule;

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (Rules == null) return;
            _rule = new Dictionary<string, List<Rule.OSInfo>>
            {
                {"allow", new List<Rule.OSInfo>()},
                {"disallow", new List<Rule.OSInfo>()}
            };
            foreach (var rule in Rules)
            {
                var ruleList = _rule[rule.Action] ?? new List<Rule.OSInfo>();
                ruleList.Add(rule.OS);
                _rule[rule.Action] = ruleList;
            }
        }

        public string Path
        {
            get
            {
                if (Downloads == null) return IsNative ? BuildNativePath() : BuildLibPath();
                return Downloads.Artifact != null ? Downloads.Artifact.Path : Downloads.Classifiers.Windows.Path;
            }
        }

        public string Url => Downloads == null ? null : Downloads.Artifact != null ? Downloads.Artifact.Url : Downloads.Classifiers.Windows.Url;

        public string Sha1 => Downloads == null ? null : Downloads.Artifact != null ? Downloads.Artifact.Sha1 : Downloads.Classifiers.Windows.Sha1;

        public int Size => Downloads == null ? -1 :  Downloads.Artifact?.Size ?? Downloads.Classifiers.Windows.Size;

        public bool IsNative => Natives != null;

        public bool ShouldDeployOnOs(string os = "windows", string version = null)
        {
            if (Rules == null) return true;
            var disallow = _rule["disallow"];
            var allow = _rule["allow"];
            if (disallow == null && allow != null)
            {
                return allow.Any(osInfo=>osInfo.Name == os);
            }
            if (allow == null && disallow != null)
            {
                return disallow.All(osInfo => osInfo.Name != os);
            }
            return true;
        }

        public bool IsVaild(string libraryPath)
        {
            var path = System.IO.Path.Combine(libraryPath, Path);
            var fileInfo = new FileInfo(path);
            if (Size == 0 || Sha1 == null)
                return fileInfo.Exists;
            return fileInfo.Exists
                   && fileInfo.Length == Size
                   && Crypto.GetSha1HashFromFile(path) == Sha1;
        }

        private string BuildLibPath()
        {
            var libp = new StringBuilder();
            var split = Name.Split(':');//0 包;1 名字；2 版本
            if (split.Length != 3)
            {
                throw new UnSupportVersionException();
            }
            libp.Append(split[0].Replace('.', '\\')).Append("\\");
            libp.Append(split[1]).Append("\\");
            libp.Append(split[2]).Append("\\");
            libp.Append(split[1]).Append("-");
            libp.Append(split[2]).Append(".jar");
            return libp.ToString();
        }

        private string BuildNativePath()
        {
            var libp = new StringBuilder();
            var split = Name.Split(':');//0 包;1 名字；2 版本
            libp.Append(split[0].Replace('.', '\\'));
            libp.Append("\\");
            libp.Append(split[1]).Append("\\");
            libp.Append(split[2]).Append("\\");
            libp.Append(split[1]).Append("-").Append(split[2]).Append("-").Append(Natives.Windows);
            libp.Append(".jar");
            libp.Replace("${arch}", Environment.Is64BitOperatingSystem ? "64" : "32");
            return libp.ToString();
        }
    }
}