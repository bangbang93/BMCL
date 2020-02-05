using BMCLV2.Mirrors.Interface;

namespace BMCLV2.Mirrors.MCBBS
{
    public class MCBBS : Mirror
    {
        public override Interface.Library Library { get; }
        public override Interface.Version Version { get; }
        public override Interface.Asset Asset { get; }
        public override string Name => "MCBBS";

        public MCBBS()
        {
            Library = new Library();
            Version = new Version();
            Asset = new Asset();
        }
    }
}
