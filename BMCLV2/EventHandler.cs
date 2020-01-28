using System;
using BMCLV2.Game;

namespace BMCLV2
{
    public delegate void OnLogEventHandler(object sender, string log);

    public delegate void OnChildProcessExit(object sender, int exitCode);

    public delegate void OnGameExit(object sender, VersionInfo versionInfo, int exitCode);

    public delegate void OnGameStart(object sender, VersionInfo versionInfo);

    public delegate void OnGameLaunch(object sender, string status, VersionInfo versionInfo);

    public delegate void OnLaunchError(Launcher.Launcher launcher, Exception exception);

    public delegate void OnAssetsDownload(int total, int cur, string name);
}
