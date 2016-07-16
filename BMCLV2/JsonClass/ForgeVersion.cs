using System.Linq;
using System.Runtime.Serialization;

namespace BMCLV2.JsonClass
{
    public class ForgeVersion
    {
        public class ForgeBuild
        {
            public class ForgeFile
            {
                [DataMember] public string format;
                [DataMember] public string category;
                [DataMember] public string hash;
            }

            [DataMember] public string branch;
            [DataMember] public int build;
            [DataMember] public string mcversion;
            [DataMember] public string modified;
            [DataMember] public string version;
            [DataMember] public ForgeFile[] files;

        }
        [DataMember] public string name;
        [DataMember] public ForgeBuild build;

        public ForgeBuild.ForgeFile GetInstaller()
        {
            return build.files.First(file => file.category == "installer");
        }

        public string GetMc()
        {
            return build?.mcversion;
        }

        public string GetDownloadUrl()
        {
            var installer = GetInstaller();
            return
                $"http://bmclapi2.bangbang93.com/maven/net/minecraftforge/forge/{build.mcversion}-{build.version}" + (build.branch != null ? $"-{build.branch}" : "") + 
                $"/forge-{build.mcversion}-{build.version}" + (build.branch != null ? $"-{build.branch}" : "") + $"-{installer.category}.{installer.format}";
        }
    }
}
