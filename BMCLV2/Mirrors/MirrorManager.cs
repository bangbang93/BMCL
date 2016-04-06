using System.Collections.Generic;
using System.Linq;
using BMCLV2.Mirrors.Interface;

namespace BMCLV2.Mirrors
{
    public class MirrorManager
    {
        private readonly List<Version> _version = new List<Version>
        {
            new BMCLAPI.Version(),
            new Vanilla.Version()
        };
        public Version CurrectMirror;

        public MirrorManager ()
        {
            CurrectMirror = _version[0];
        }

        public Version GetByName(string name)
        {
            return _version.FirstOrDefault(version => version.Name == name);
        }

        public Version this[int index] => _version[index];

        public Version this[string name] => GetByName(name);
    }
}