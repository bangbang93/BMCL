using BMCLV2.Game;

namespace BMCLV2
{
    public delegate void OnLogEventHandler(object sender, string log);

    public delegate void OnChildProcessExit(object sender, int exitCode);

    public delegate void OnGameExit(object sender, VersionInfo versionInfo, int exitCode);

    public delegate void OnGameStart(object sender, VersionInfo versionInfo);
}
