using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BMCLV2.Mod
{
    static class ModHelper
    {
        static public string SetupModPath(string version)
        {
            var versionPath = Path.Combine(BmclCore.BaseDirectory, @".minecraft\versions\", version);
            var modpath = Path.Combine(versionPath, "mods");
            var configpath = Path.Combine(versionPath, "config");
            var coremodpath = Path.Combine(versionPath, "coremods");
            var moddirpath = Path.Combine(versionPath, "moddir");
            if (!Directory.Exists(modpath))
            {
                Directory.CreateDirectory(modpath);
            }
            if (!Directory.Exists(configpath))
            {
                Directory.CreateDirectory(configpath);
            }
            if (!Directory.Exists(coremodpath))
            {
                Directory.CreateDirectory(coremodpath);
            }
            if (!Directory.Exists(moddirpath))
            {
                Directory.CreateDirectory(moddirpath);
            }
            return versionPath;
        }
    }
}
