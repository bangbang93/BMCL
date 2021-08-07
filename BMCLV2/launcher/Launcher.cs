using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BMCLV2.Auth;
using BMCLV2.Cfg;
using BMCLV2.Downloader;
using BMCLV2.Exceptions;
using BMCLV2.Game;
using BMCLV2.Mojang.Runtime;
using BMCLV2.util;

namespace BMCLV2.Launcher
{
  public class Launcher
  {
    private readonly List<string> _arguments = new List<string>
    {
      "-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump",
    };

    private readonly string _libraryDirectory;
    private readonly string _nativesDirectory;
    private readonly string _versionDirectory;
    private ChildProcess _childProcess;
    private Dictionary<string, int> _errorCount = new Dictionary<string, int>();

    private OnGameExit _onGameExit;
    private OnGameLaunch _onGameLaunch;
    private OnGameStart _onGameStart;
    private OnLaunchError _onLaunchError;

    private string _java;
    private readonly LaunchMode _launchMode;
    private readonly string _xmx;
    private readonly List<string> _jvmArgs = new List<string>();
    private readonly Dictionary<string, string> _values;

    public Launcher(VersionInfo versionInfo, AuthResult authResult, Config config)
    {
      VersionInfo = versionInfo;
      _java = config.Javaw;
      _launchMode = config.LaunchMode;
      _xmx = config.Javaxmx;
      _versionDirectory = Path.Combine(BmclCore.BaseDirectory, ".minecraft\\versions", VersionInfo.Id);
      _libraryDirectory = Path.Combine(BmclCore.MinecraftDirectory, "libraries");
      _nativesDirectory = Path.Combine(_versionDirectory, $"{VersionInfo.Id}-natives-{TimeHelper.TimeStamp()}");
      if (!string.IsNullOrEmpty(config.ExtraJvmArg))
      {
        _jvmArgs.AddRange(ChildProcess.SplitCommandLine(config.ExtraJvmArg));
      }

      _values = new Dictionary<string, string>
      {
        { "${auth_player_name}", authResult.Username },
        { "${version_name}", VersionInfo.Id },
        { "${game_directory}", BmclCore.MinecraftDirectory },
        { "${assets_root}", Path.Combine(BmclCore.MinecraftDirectory, "assets") },
        { "${assets_index_name}", VersionInfo.Assets },
        { "${user_type}", "Legacy" },
        { "${version_type}", VersionInfo.Type ?? "Legacy" },
        { "${user_properties}", "{}" },
        { "${launcher_name}", "BMCL" },
        { "${launcher_version}", BmclCore.BmclVersion },
        { "${natives_directory}", _nativesDirectory },
        { "${library_directory}", _libraryDirectory },
        { "${classpath_separator}", BmclCore.OS == "windows" ? ";" : ":" }
      };

      if (versionInfo.Arguments?.Jvm != null)
      {
        _jvmArgs.AddRange(versionInfo.Arguments.Jvm.OfType<string>());
      }

      if (_launchMode == LaunchMode.Standalone)
      {
        var gameDirectory = Path.Combine(_versionDirectory, ".minecraft");
        FileHelper.CreateDirectoryIfNotExist(gameDirectory);
        _values["${game_directory}"] = gameDirectory;
      }

      if (authResult.OutInfo != null)
        foreach (var info in authResult.OutInfo)
          _values[info.Key] = info.Value;
    }

    public VersionInfo VersionInfo { get; }

    public event OnGameExit OnGameExit
    {
      add => _onGameExit += value;
      remove => _onGameExit -= value;
    }

    public event OnGameStart OnGameStart
    {
      add => _onGameStart += value;
      remove => _onGameStart -= value;
    }

    public event OnGameLaunch OnGameLaunch
    {
      add => _onGameLaunch += value;
      remove => _onGameLaunch -= value;
    }

    public event OnLaunchError OnLaunchError
    {
      add => _onLaunchError += value;
      remove => _onLaunchError -= value;
    }

    public async Task Start()
    {
      try
      {
        _onGameLaunch(this, "LauncherCheckJava", VersionInfo);
        await SetupJava();
        if (!CleanNatives()) return;
        _onGameLaunch(this, "LauncherSolveLib", VersionInfo);
        if (!await SetupLibraries()) return;
        if (!await SetupNatives()) return;
        _onGameLaunch(this, "LauncherBuildMCArg", VersionInfo);
        _arguments.Add(VersionInfo.MainClass);
        _arguments.AddRange(McArguments());
        Logger.Log(ChildProcess.JoinArguments(_arguments.ToArray()));
        _onGameLaunch(this, "LauncherGo", VersionInfo);
        if (!Launch()) return;
        _onGameStart(this, VersionInfo);
      }
      catch (Exception e)
      {
        _onLaunchError(this, e);
        throw;
      }
    }

    private bool Launch()
    {
      PatchOldJvmArgs();
      try
      {
        var arguments = new List<string>(_jvmArgs).Concat(_arguments).ToArray();
        arguments = this.ReplaceArguments(arguments).ToArray();
        _childProcess =
          _launchMode == LaunchMode.Normal
            ? new ChildProcess(_java, arguments)
            : new ChildProcess(_java, _versionDirectory, arguments);
        if (!_childProcess.Start()) return false;
        _childProcess.OnStdOut += OnStdOut;
        _childProcess.OnStdErr += OnStdOut;
        _childProcess.OnExit += ChildProcessOnExit;
        _errorCount = CountError();
      }
      catch (Exception exception)
      {
        _onLaunchError(this, exception);
      }

      return true;
    }

    private void PatchOldJvmArgs()
    {
      var library = _jvmArgs.FirstOrDefault(arg => arg.StartsWith("-Djava.library.path"));
      if (library == null) _jvmArgs.Add($"-Djava.library.path={_nativesDirectory}");
      var cp = _jvmArgs.Contains("-cp");
      if (cp) return;
      _jvmArgs.Add("-cp");
      _jvmArgs.Add(_values["${classpath}"]);
    }

    private static Dictionary<string, int> CountError()
    {
      var values = new Dictionary<string, int>();
      var crashReportDir = Path.Combine(BmclCore.MinecraftDirectory, "crash-reports");
      values["crashReport"] = Directory.Exists(crashReportDir) ? Directory.GetFiles(crashReportDir).Length : 0;
      var hsErrorDir = BmclCore.MinecraftDirectory;
      values["hsError"] = Directory.Exists(hsErrorDir)
        ? Directory.GetFiles(hsErrorDir).Count(s => s.StartsWith("hs_err"))
        : 0;
      return values;
    }

    private void ChildProcessOnExit(object sender, int exitCode)
    {
      Logger.Log(
        $"{VersionInfo.Id} has exited with exit code {exitCode}, Running for {new TimeSpan(0, 0, 0, _childProcess.UpTime)}");
      CleanNatives();
      _onGameExit(sender, VersionInfo, exitCode);
      if (_childProcess.UpTime < 10)
      {
        //TODO maybe startup problem
      }

      var newValue = CountError();
      HandleCrashReport(newValue);
      HandleHsError(newValue);
    }

    private static void OnStdOut(object sender, string log)
    {
      Logger.Log(log);
    }

    private async Task SetupJava()
    {
      if (VersionInfo.JavaVersion == null)
      {
        if (!File.Exists(_java)) throw new NoJavaException(_java);
        _arguments.Add($"-Xmx{_xmx}M");
      }
      else
      {
        var javaManager = new JavaManager(VersionInfo.JavaVersion.Component);
        var downloads = await javaManager.EnsureJava();
        var downloadWindow = new DownloadWindow(downloads);
        downloadWindow.Show();
        await downloadWindow.StartDownload();
        downloadWindow.Close();
        _java = javaManager.ExecutablePath;
      }
    }

    private async Task<bool> SetupLibraries()
    {
      var libraryPath = new List<string>();
      var libraries = VersionInfo.Libraries.Where(e => !e.IsNative && e.ShouldDeployOnOs());
      var set = new List<string>();
      var semi = new SemaphoreSlim(BmclCore.Config.DownloadThread, BmclCore.Config.DownloadThread);
      await Task.WhenAll(libraries.Select(libraryInfo => Task.Run(async () =>
      {
        await semi.WaitAsync();

        try
        {
          if (set.Contains(libraryInfo.Name)) return;
          set.Add(libraryInfo.Name);
          var filePath = Path.Combine(_libraryDirectory, libraryInfo.GetLibraryPath());
          if (!libraryInfo.IsVaildLibrary(_libraryDirectory))
            await BmclCore.MirrorManager.CurrentMirror.Library.DownloadLibrary(libraryInfo, filePath);

          libraryPath.Add(filePath.Replace('/', '\\'));
        }
        catch (WebException exception)
        {
          throw new DownloadLibException(libraryInfo, exception);
        }
        finally
        {
          semi.Release();
        }
      })));

      if (VersionInfo.MainClass != "cpw.mods.bootstraplauncher.BootstrapLauncher")
      {
        libraryPath.Add(Path.Combine(BmclCore.BaseDirectory, ".minecraft", "versions",
          VersionInfo.Jar ?? VersionInfo.InheritsFrom ?? VersionInfo.Id,
          $"{VersionInfo.Jar ?? VersionInfo.InheritsFrom ?? VersionInfo.Id}.jar"));
      }

      _values["${classpath}"] = string.Join(_values["${classpath_separator}"], libraryPath);
      return true;
    }

    private async Task<bool> SetupNatives()
    {
      FileHelper.CreateDirectoryIfNotExist(_nativesDirectory);

      var natives = VersionInfo.Libraries.Where(e => e.IsNative && e.ShouldDeployOnOs());

      var set = new List<string>();
      var semi = new SemaphoreSlim(BmclCore.Config.DownloadThread, BmclCore.Config.DownloadThread);
      await Task.WhenAll(natives.Select(nativeInfo => Task.Run(async () =>
      {
        await semi.WaitAsync();
        try
        {
          if (set.Contains(nativeInfo.Name)) return;
          set.Add(nativeInfo.Name);
          //skip non-natives
          Logger.Info(nativeInfo.Name);
          Logger.Info("unarchive");
          var filePath = Path.Combine(_libraryDirectory, nativeInfo.GetNativePath());

          if (!nativeInfo.IsValidNative(_libraryDirectory))
            await BmclCore.MirrorManager.CurrentMirror.Library.DownloadLibrary(nativeInfo, filePath);

          UnzipNative(filePath, nativeInfo.Extract);
        }
        catch (WebException exception)
        {
          throw new DownloadLibException(nativeInfo, exception);
        }
        catch (InvalidDataException exception)
        {
          throw new DownloadLibException(nativeInfo, exception);
        }
        finally
        {
          semi.Release();
        }
      })));
      return true;
    }

    private void UnzipNative(string filename, LibraryInfo.ExtractRule extractRules)
    {
      using (var zipFile = new FileStream(filename, FileMode.Open))
      {
        var zipArchive = new ZipArchive(zipFile);
        foreach (var entry in zipArchive.Entries)
        {
          if (entry.FullName.Contains("META-INF/")) continue; //skip META-INF
          if (extractRules != null &&
              extractRules.Exclude.Any(entryName => entry.FullName.Contains(entryName))) continue;
          var filePath = Path.Combine(_nativesDirectory, entry.FullName);
          Logger.Log($"extract ${filePath}");
          entry.ExtractToFile(filePath, true);
        }
      }
    }

    private IEnumerable<string> McArguments()
    {
      if (VersionInfo.Arguments == null)
      {
        var arguments = VersionInfo.MinecraftArguments.Split(' ');
        for (var i = 0; i < arguments.Length; i++)
          if (_values.ContainsKey(arguments[i]))
            arguments[i] = _values[arguments[i]];
        return arguments;
      }
      else
      {
        var arguments = new List<string>(20);
        var gameArgs = VersionInfo.Arguments.Game;
        arguments.AddRange(gameArgs.OfType<string>().Select(s => _values.ContainsKey(s) ? _values[s] : s));
        return arguments;
      }
    }

    private List<string> ReplaceArguments(IEnumerable<string> arguments)
    {
      return arguments.Select(argument =>
        _values.Aggregate(argument, (current, value) =>
          current.Replace(value.Key, value.Value))
      ).ToList();
    }

    private void HandleCrashReport(IReadOnlyDictionary<string, int> nowValue)
    {
      var crashReportsPath = Path.Combine(BmclCore.MinecraftDirectory, "crash-reports");
      if (nowValue["crashReport"] == _errorCount["crashReport"] || !Directory.Exists(crashReportsPath)) return;
      Logger.Log("发现新的错误报告");
      var clientCrashReportDir = new DirectoryInfo(crashReportsPath);
      var clientReports = clientCrashReportDir.GetFiles();
      Array.Sort(clientReports,
        (info1, info2) => (int)(info1.LastWriteTime - info2.LastWriteTime).TotalSeconds);
      var crashReportReader = new StreamReader(clientReports[0].FullName);
      Logger.Log(crashReportReader.ReadToEnd(), Logger.LogType.Crash);
      crashReportReader.Close();
      ChildProcess.Exec(clientReports[0].FullName);
    }

    private void HandleHsError(IReadOnlyDictionary<string, int> nowValue)
    {
      var hsErrorPath = BmclCore.MinecraftDirectory;
      if (nowValue["hsError"] == _errorCount["hsError"]) return;
      Logger.Log("发现新的JVM错误报告");
      var hsErrorDir = new DirectoryInfo(hsErrorPath);
      var hsErrors = hsErrorDir.GetFiles().Where(s => s.FullName.StartsWith("hs_err")).ToArray();
      Array.Sort(hsErrors,
        (info1, info2) => (int)(info1.LastWriteTime - info2.LastWriteTime).TotalSeconds);
      var crashReportReader = new StreamReader(hsErrors[0].FullName);
      Logger.Log(crashReportReader.ReadToEnd(), Logger.LogType.Crash);
      crashReportReader.Close();
      ChildProcess.Exec(hsErrors[0].FullName);
    }

    private bool CleanNatives()
    {
      var dir = _versionDirectory;
      var dirInfo = new DirectoryInfo(dir);
      var nativeDir = dirInfo.GetDirectories($"{VersionInfo.Id}-natives-*");
      foreach (var directoryInfo in nativeDir) directoryInfo.Delete(true);
      return true;
    }
  }
}
