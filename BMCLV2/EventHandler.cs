using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMCLV2.Game;

namespace BMCLV2
{
    public delegate void OnLogEventHandler(object sender, string log);

    public delegate void OnChildProcessExit(object sender, int exitCode);

    public delegate void OnGameExit(object sender, VersionInfo versionInfo);
}
