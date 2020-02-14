using System.Collections.Generic;
using System.Linq;
using BMCLV2.Mirrors.Interface;

namespace BMCLV2.Mirrors
{
    public class MirrorManager
    {
        private readonly List<Mirror> _version = new List<Mirror>
        {
            new BMCLAPI.Bmclapi(),
            new Vanilla.Vanilla(),
            new MCBBS.MCBBS(),
        };
        public Mirror CurrentMirror;

        public MirrorManager()
        {
            CurrentMirror = _version[0];
        }

        public Mirror GetByName(string name)
        {
            return _version.FirstOrDefault(version => version.Name == name);
        }

        public Mirror this[int index] => _version[index];

        public Mirror this[string name] => GetByName(name);
    }
}
