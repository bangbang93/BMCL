using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMCLV2.Exceptions;
using BMCLV2.Mirrors;
using BMCLV2.Objects.Mirrors;
using BMCLV2.util;
using ICSharpCode.SharpZipLib.Zip;
using VersionInfo = BMCLV2.Game.VersionInfo;

namespace BMCLV2.Launcher
{
    public class Launcher
    {
        private ChildProcess _childProcess;
        private readonly Config _config;
        private readonly List<string> _arguments = new List<string>();
        private readonly VersionInfo _versionInfo;
        private readonly string _versionDirectory;
        private readonly string _libraryDirectory;
        private readonly string _nativesDirectory;
        private Dictionary<string ,int> _errorCount = new Dictionary<string, int>(); 

        private OnGameExit _onGameExit;

        public event OnGameExit OnGameExit
        {
            add { _onGameExit += value; }
            remove { _onGameExit -= value; }
        }

        public Launcher(VersionInfo versionInfo, Config config = null, bool disableXincgc = false)
        {
            _versionInfo = versionInfo;
            _config = config ?? Config.Load();
            _versionDirectory = Path.Combine(BmclCore.BaseDirectory, ".minecraft\\versions", _versionInfo.Id);
            _libraryDirectory = Path.Combine(BmclCore.BaseDirectory, "libraries");
            _nativesDirectory = Path.Combine(_versionDirectory, $"{_versionInfo.Id}-natives-{TimeHelper.TimeStamp()}");

            if (!disableXincgc)
            {
                _arguments.AddRange(new[] { "-Xincgc" });
            }
            if (!string.IsNullOrEmpty(_config.ExtraJvmArg))
            {
                _arguments.AddRange(ChildProcess.SplitCommandLine(_config.ExtraJvmArg));
            }
        }

        public async void Start()
        {
            if (!SetupJava()) return;
            _arguments.Add($"-Djava.library.path={_nativesDirectory}");
            if (!await SetupLibraries()) return;
            if (!await SetupNatives()) return;
            _arguments.Add(_versionInfo.MainClass);
            _arguments.AddRange(McArguments());
            if (!Launch()) return;
            Logger.Log(ChildProcess.JoinArguments(_arguments.ToArray()));
        }

        private bool Launch()
        {
            _childProcess = new ChildProcess(_config.Javaw, _arguments.ToArray());
            if (_childProcess.Start())
            {
                _childProcess.OnStdOut += OnStdOut;
                _childProcess.OnStdErr += OnStdOut;
                _childProcess.OnExit += ChildProcessOnExit;
                _errorCount = CountError();
                return true;
            }
            return false;
        }

        private Dictionary<string, int> CountError()
        {
            var values = new Dictionary<string, int>();
            var crashReportDir = Path.Combine(BmclCore.MinecraftDirectory, "crash-reports");
            values["crashReports"] = Directory.Exists(crashReportDir) ? Directory.GetFiles(crashReportDir).Length : 0;
            var hsErrorDir = BmclCore.MinecraftDirectory;
            values["hsError"] = Directory.Exists(hsErrorDir) ? Directory.GetFiles(hsErrorDir).Count(s => s.StartsWith("hs_err")) : 0;
            return values;
        }

        private void ChildProcessOnExit(object sender, int exitCode)
        {
            _onGameExit(sender, _versionInfo, exitCode);
            Logger.Log(
                $"{_versionInfo.Id} has exited with exit code {exitCode}, Running for {new TimeSpan(0, 0, 0, _childProcess.UpTime)}");
            if (_childProcess.UpTime < 10)
            {
                //TODO maybe startup problem
            }
            else
            {
                var newValue = CountError();
                HandleCrashReport(newValue);
                HandleHsError(newValue);
            }
        }

        private void OnStdOut(object sender, string log)
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
            var libraries = _versionInfo.Libraries;
            foreach (var libraryInfo in libraries)
            {
                // skip natives
                if (libraryInfo.IsNative) continue;
                var filePath = Path.Combine(_libraryDirectory, libraryInfo.Path);
                if (!libraryInfo.IsVaild(_libraryDirectory))
                {
                    await BmclCore.MirrorManager.CurrectMirror.Library.DownloadLibrary(libraryInfo);
                }
                libraryPath.Append(filePath).Append(";");
            }
            libraryPath.Append(Path.Combine(_versionDirectory, $"{_versionInfo.Id}.jar"));
            _arguments.Add("-cp");
            _arguments.Add(libraryPath.ToString());
            return true;
        }

        private async Task<bool> SetupNatives()
        {
            foreach (var libraryInfo in _versionInfo.Libraries)
            {
                //skip non-natives
                if (!libraryInfo.IsNative) continue;
                if (!libraryInfo.ShouldDeployOnOs()) continue;
                var filePath = Path.Combine(_libraryDirectory, libraryInfo.Path);
                if (!libraryInfo.IsVaild(_libraryDirectory))
                {
                    await BmclCore.MirrorManager.CurrectMirror.Library.DownloadLibrary(libraryInfo);
                }
                await UnzipNative(filePath, libraryInfo.Extract);
            }
            return true;
        }

        private async Task UnzipNative(string filename, LibraryInfo.ExtractRule extractRules)
        {
            var zipFile = new ZipFile(filename);
            foreach (ZipEntry entry in zipFile)
            {
                if (extractRules != null && extractRules.Exclude.Any(entryName => entry.Name.Contains(entryName))) continue;
                var filePath = Path.Combine(_nativesDirectory, entry.Name);
                FileHelper.CreateDirectoryForFile(filePath);
                if (entry.IsDirectory) continue;
                var file = File.Create(filePath);
                var stream = zipFile.GetInputStream(entry);
                await stream.CopyToAsync(file);
                file.Close();
                stream.Close();
            }
            zipFile.Close();
        }

        private string[] McArguments()
        {
            var values = new Dictionary<string ,string>();
            values.Add("${auth_player_name}", _config.Username);
            values.Add("${version_name}", _versionInfo.Id);
            values.Add("${game_directory}", BmclCore.MinecraftDirectory);
            values.Add("${assets_root}", "assets");
            values.Add("${assets_index_name}", _versionInfo.Assets);
            values.Add("${auth_uuid}", "0000");
            values.Add("${auth_access_token}", "0000");
            values.Add("${user_type}", "Legacy");
            values.Add("${version_type}", "Legacy");
            var arguments = new StringBuilder(_versionInfo.MinecraftArguments);
            arguments = values.Aggregate(arguments, (current, value) => current.Replace(value.Key, value.Value));
            return arguments.ToString().Split(' ');
        }

        private void HandleCrashReport(Dictionary<string, int> nowValue)
        {
            var crashReportsPath = Path.Combine(BmclCore.MinecraftDirectory, "crash-reports");
            if (nowValue["crashReport"] != _errorCount["crashReport"] && Directory.Exists(crashReportsPath))
            {
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
        }

        private void HandleHsError(Dictionary<string, int> nowValue)
        {
            var hsErrorPath = BmclCore.MinecraftDirectory;
            if (nowValue["hsError"] != _errorCount["hsError"])
            {
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
        }
    }
}