using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BMCLV2.Mod
{
    static class ModHelper
    {
        public static string SetupModPath(string version)
        {
            var versionPath = Path.Combine(BmclCore.BaseDirectory, @".minecraft\versions\", version);
            var modpath = Path.Combine(versionPath, "mods");
            var configpath = Path.Combine(versionPath, "config");
            if (!Directory.Exists(modpath))
            {
                Directory.CreateDirectory(modpath);
            }
            if (!Directory.Exists(configpath))
            {
                Directory.CreateDirectory(configpath);
            }
            return versionPath;
        }
    }
}
