namespace BMCLV2.Launcher
{
    public enum LauncherState
    {
        Initializing,
        Checking,
        CleaningUp,
        SolvingLibrary,
        SolvingNative,
        ConstractingArguments,
        PostLaunch,
        StartProcess,
        Running,
        Stop,
        Error
    }
}