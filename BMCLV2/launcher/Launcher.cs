using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BMCLV2.Auth;
using BMCLV2.Cfg;
using BMCLV2.Exceptions;
using BMCLV2.Game;
using BMCLV2.I18N;
using BMCLV2.util;

namespace BMCLV2.Launcher
{
    public class Launcher
    {
        private ChildProcess _childProcess;
        private readonly Config _config;
        private readonly List<string> _arguments = new List<string>();
        public VersionInfo VersionInfo { get; }
        private readonly string _versionDirectory;
        private readonly string _libraryDirectory;
        private readonly string _nativesDirectory;
        private Dictionary<string, int> _errorCount = new Dictionary<string, int>();
        private readonly AuthResult _authResult;

        private OnGameExit _onGameExit;
        private OnGameStart _onGameStart;
        private OnGameLaunch _onGameLaunch;
        private OnLaunchError _onLaunchError;


        public LauncherState State { get; }

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

        public Launcher(VersionInfo versionInfo, AuthResult authResult, Config config = null)
        {
            _authResult = authResult;
            VersionInfo = versionInfo;
            State = LauncherState.Initializing;
            _config = config ?? Config.Load();
            _versionDirectory = Path.Combine(BmclCore.BaseDirectory, ".minecraft\\versions", VersionInfo.Id);
            _libraryDirectory = Path.Combine(BmclCore.MinecraftDirectory, "libraries");
            _nativesDirectory = Path.Combine(_versionDirectory, $"{VersionInfo.Id}-natives-{TimeHelper.TimeStamp()}");
            if (!string.IsNullOrEmpty(_config.ExtraJvmArg))
            {
                _arguments.AddRange(ChildProcess.SplitCommandLine(_config.ExtraJvmArg));
            }
        }

        public async Task Start()
        {
          try
          {
            _onGameLaunch(this, "LauncherCheckJava", VersionInfo);
            if (!SetupJava()) return;
            if (!CleanNatives()) return;
            _arguments.Add($"-Djava.library.path={_nativesDirectory}");
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
            try
            {
                _childProcess =
                    _config.LaunchMode == LaunchMode.Normal
                        ? new ChildProcess(_config.Javaw, _arguments.ToArray())
                        : new ChildProcess(_config.Javaw, _versionDirectory, _arguments.ToArray());
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

        private static Dictionary<string, int> CountError()
        {
            var values = new Dictionary<string, int>();
            var crashReportDir = Path.Combine(BmclCore.MinecraftDirectory, "crash-reports");
            values["crashReport"] = Directory.Exists(crashReportDir) ? Directory.GetFiles(crashReportDir).Length : 0;
            var hsErrorDir = BmclCore.MinecraftDirectory;
            values["hsError"] = Directory.Exists(hsErrorDir) ? Directory.GetFiles(hsErrorDir).Count(s => s.StartsWith("hs_err")) : 0;
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

        private bool SetupJava()
        {
            if (!File.Exists(_config.Javaw)) throw new NoJavaException(_config.Javaw);
            _arguments.Add($"-Xmx{_config.Javaxmx}M");
            return true;
        }

        private async Task<bool> SetupLibraries()
        {
            var libraryPath = new StringBuilder();
            var libraries = VersionInfo.Libraries;
            foreach (var libraryInfo in libraries)
            {
                if (libraryInfo.IsNative) continue;
                var filePath = Path.Combine(_libraryDirectory, libraryInfo.GetLibraryPath());
                if (!libraryInfo.IsVaildLibrary(_libraryDirectory))
                {
                    try
                    {
                        await BmclCore.MirrorManager.CurrectMirror.Library.DownloadLibrary(libraryInfo, filePath);
                    }
                    catch (WebException exception)
                    {
                        throw new DownloadLibException(libraryInfo, exception);
                    }
                }
                libraryPath.Append(filePath).Append(";");
            }
            if (VersionInfo.InheritsFrom == null)
            {
                libraryPath.Append(Path.Combine(_versionDirectory, $"{VersionInfo.Jar ?? VersionInfo.Id}.jar"));
            }
            else
            {
                libraryPath.Append(Path.Combine(BmclCore.BaseDirectory, ".minecraft\\versions", VersionInfo.InheritsFrom, $"{VersionInfo.Jar ?? VersionInfo.Id}.jar"));
            }
            
            _arguments.Add("-cp");
            _arguments.Add(libraryPath.ToString());
            return true;
        }

        private async Task<bool> SetupNatives()
        {
            FileHelper.CreateDirectoryIfNotExist(_nativesDirectory);
            foreach (var libraryInfo in VersionInfo.Libraries)
            {
                //skip non-natives
                Logger.Info(libraryInfo.Name);
                if (!libraryInfo.IsNative) continue;
                if (!libraryInfo.ShouldDeployOnOs()) continue;
                Logger.Info("unarchive");
                var filePath = Path.Combine(_libraryDirectory, libraryInfo.GetNativePath());
                try
                {
                    if (!libraryInfo.IsVaildNative(_libraryDirectory))
                    {
                        await BmclCore.MirrorManager.CurrectMirror.Library.DownloadLibrary(libraryInfo, filePath);
                    }
                    UnzipNative(filePath, libraryInfo.Extract);
                }
                catch (WebException exception)
                {
                    throw new DownloadLibException(libraryInfo, exception);
                }
                catch (InvalidDataException exception)
                {
                    throw new DownloadLibException(libraryInfo, exception);
                }
            }
            return true;
        }

        private void UnzipNative(string filename, LibraryInfo.ExtractRule extractRules)
        {
            using (var zipFile = new FileStream(filename, FileMode.Open))
            {
                var zipArchive = new ZipArchive(zipFile);
                foreach (var entry in zipArchive.Entries)
                {
                    if (entry.FullName.Contains("META-INF/")) continue;//skip META-INF
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
            var values = new Dictionary<string, string>
            {
                {"${auth_player_name}", _authResult.Username},
                {"${version_name}", VersionInfo.Id},
                {"${game_directory}", BmclCore.MinecraftDirectory},
                {"${assets_root}", Path.Combine(BmclCore.MinecraftDirectory, "assets")},
                {"${assets_index_name}", VersionInfo.Assets},
                {"${user_type}", "Legacy"},
                {"${version_type}", "Legacy"},
                {"${user_properties}", "{}"}
            };
            if (_config.LaunchMode == LaunchMode.Standalone)
            {
                var gameDirectory = Path.Combine(_versionDirectory, ".minecraft");
                FileHelper.CreateDirectoryIfNotExist(gameDirectory);
                values["${game_directory}"] = gameDirectory;
            }
            if (_authResult.OutInfo != null)
            {
                foreach (var info in _authResult.OutInfo)
                {
                    values.Add(info.Key, info.Value);
                }
            }
          if (VersionInfo.Arguments == null)
          {
            var arguments = VersionInfo.MinecraftArguments.Split(' ');
            for (var i = 0; i < arguments.Length; i++)
            {
              if (values.ContainsKey(arguments[i]))
              {
                arguments[i] = values[arguments[i]];
              }
            }
            return arguments;
          }
          else
          {
            var arguments = new List<string>(20);
            var gameArgs = VersionInfo.Arguments.Game;
            arguments.AddRange(gameArgs.OfType<string>().Select(s => values.ContainsKey(s) ? values[s] : s));
            return arguments;
          }
        }

        private void HandleCrashReport(IReadOnlyDictionary<string, int> nowValue)
        {
          var crashReportsPath = Path.Combine(BmclCore.MinecraftDirectory, "crash-reports");
          if (nowValue["crashReport"] == _errorCount["crashReport"] || !Directory.Exists(crashReportsPath)) return;
          Logger.Log("发现新的错误报告");
          var clientCrashReportDir = new DirectoryInfo(crashReportsPath);
          var clientReports = clientCrashReportDir.GetFiles();
          Array.Sort(clientReports,
            (info1, info2) => (int) (info1.LastWriteTime - info2.LastWriteTime).TotalSeconds);
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
            foreach (var directoryInfo in nativeDir)
            {
                directoryInfo.Delete(true);
            }
            return true;
        }
    }
}
