using System;
using System.Data.Common;
using System.Runtime.Serialization;

namespace BMCLV2.Game
{
    [DataContract]
    public class VersionInfo
    {
        [DataContract]
        public class VersionLibrary
        {
            [DataContract]
            public class LibraryDownload
            {
                [DataContract]
                public class File
                {
                    [DataMember(Name = "size")] public string Size;
                    [DataMember(Name = "sha1")] public string Sha1;
                    [DataMember(Name = "path")] public string Path;
                    [DataMember(Name = "url")] public string Url;
                }
                [DataContract]
                public class Classifier
                {
                    [DataMember(Name = "natives-linux")] public File NativesLinux;
                    [DataMember(Name = "natives-osx")] public File NativesOsx;
                    [DataMember(Name = "natives-windows")] public File NativesWindows;
                }

                [DataMember(Name = "artifact")] public File Atrifact;
                [DataMember(Name = "classifiers")] public Classifier Classifiers;

            }
            [DataContract]
            public class LibraryRule
            {
                [DataContract]
                public class RuleOs
                {
                    [DataMember(Name = "name")]
                    public string Name;
                }
                [DataMember(Name = "action")]
                public string Action;
                [DataMember(Name = "os")]
                public RuleOs Os;
            }
            [DataContract]
            public class LibraryExtract
            {
                [DataMember(Name = "exclude")] public string[] Exclude;
                [DataMember(Name = "include")] public string[] Include;
            }
            [DataContract]
            public class LibraryNative
            {
                [DataMember(Name = "linux")] public string Linux;
                [DataMember(Name = "osx")] public string Osx;
                [DataMember(Name = "windows")] public string Windows;
            }


            [DataMember(Name = "name")] public string Name;
            [DataMember(Name = "url")] public string Url;
            [DataMember(Name = "comment")] public string Comment;
            [DataMember(Name = "serverreq")] public bool ServerReq;
            [DataMember(Name = "clientreq")] public bool ClientReq;
            [DataMember(Name = "checksums")] public string[] Checksums;
            [DataMember(Name = "downloads")] public LibraryDownload Download;
            [DataMember(Name = "rules")] public LibraryRule[] Rules;
            [DataMember(Name = "extract")] public LibraryExtract Extract;

        }
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
        [DataMember(Name = "id")] public string Id;
        [DataMember(Name = "time")] public string Time;
        [DataMember(Name = "releaseTime")] public string ReleaseTime;
        [DataMember(Name = "type")] public string Type;
        [DataMember(Name = "minecraftArguments")] public string MinecraftArguments;
        [DataMember(Name = "mainClass")] public string MainClass;
        [DataMember(Name = "minimumLauncherVersion")] public string MinimumLauncherVersion;
        [DataMember(Name = "inheritsFrom")] public string InheritsFrom;
        [DataMember(Name = "assetIndex")] public VersionAssetIndex AssetIndex;
        [DataMember(Name = "libraries")] public VersionLibrary[] Libraries;
        [DataMember(Name = "assets")] public string Assets;
    }
}