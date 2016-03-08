using System.Runtime.Serialization;
using System.Security.Policy;

// ReSharper disable InconsistentNaming

namespace BMCLV2.Objects.Mirrors
{
    public struct LibraryInfo
    {
        public struct Extract
        {
            [DataMember] public string[] exclude;
        }

        public struct Downloads
        {
            public struct Artifcat
            {
                [DataMember] public int size;
                [DataMember] public string sha1;
                [DataMember] public string path;
                [DataMember] public string url;
            }

            public struct Classifiers
            {
                [DataMember(Name = "natives-linux")] private Artifcat linux;
                [DataMember(Name = "natives-osx")] private Artifcat osx;
                [DataMember(Name = "natives-windows")] private Artifcat windows;
            }

            [DataMember] public Artifcat artifact;
            [DataMember] public Classifiers classifiers;
        }

        public struct Rule
        {
            public struct OS
            {
                [DataMember] public string name;
            }

            [DataMember] public string action;
            [DataMember] public OS os;
        }

        [DataMember] public string name;
        [DataMember] public Downloads downloads;
        [DataMember] public Rule[] rules;
        [DataMember] public Extract extract;
    }
}