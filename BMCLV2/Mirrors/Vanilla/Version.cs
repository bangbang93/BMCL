namespace BMCLV2.Mirrors.Vanilla
{
    public class Version : Interface.Version
    {
        public override string Name { get;} = "Vanilla";

        public Version()
        {
            Url = "http://launchermeta.mojang.com/mc/game/version_manifest.json";
        }
    }
}