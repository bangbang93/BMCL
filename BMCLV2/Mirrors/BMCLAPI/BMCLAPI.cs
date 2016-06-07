using BMCLV2.Mirrors.Interface;

namespace BMCLV2.Mirrors.BMCLAPI
{
    public class Bmclapi : Mirror
    {
        public override Interface.Library Library { get; }
        public override Interface.Version Version { get; }
        public override Interface.Asset Asset { get; }
        public override string Name => "BMCLAPI";

        public Bmclapi()
        {
            Library = new Library();
            Version = new Version();
            Asset = new Asset();
        }
    }
}