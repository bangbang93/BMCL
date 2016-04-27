using BMCLV2.Mirrors.Interface;

namespace BMCLV2.Mirrors.Vanilla
{
    public class Vanilla : Mirror
    {
        public override Interface.Library Library { get; }
        public override Interface.Version Version { get; }
        public override string Name => "Vanilla";

        public Vanilla()
        {
            Library = new Library();
            Version = new Version();
        }
    }
}