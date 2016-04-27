namespace BMCLV2.Mirrors.Interface
{
    public abstract class Mirror
    {
        public abstract Library Library { get; }
        public abstract Version Version { get; }

        public abstract string Name { get; }
    }
}