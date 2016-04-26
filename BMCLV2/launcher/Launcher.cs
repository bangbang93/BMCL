using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BMCLV2.Exceptions;
using BMCLV2.Objects.Mirrors;
using BMCLV2.util;
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

        public void Start()
        {
            if (!SetupJava()) return;
            _arguments.Add($"-Djava.library.path =\"{_versionDirectory}\"");
            if (!await SetupLibraries()) return;
            if (!Launch()) return;
        }

        private bool Launch()
        {
            _childProcess = new ChildProcess(_config.Javaw, _arguments.ToArray());
            if (_childProcess.Start())
            {
                _childProcess.OnStdOut += OnStdOut;
                _childProcess.OnStdErr += OnStdOut;
                _childProcess.OnExit += ChildProcessOnOnExit;
                return true;
            }
            return false;
        }

        private void ChildProcessOnOnExit(object sender, int exitCode)
        {
            Logger.log(
                $"{_versionInfo.Id} has exited with exit code {exitCode}, Running for {new TimeSpan(0, 0, 0, _childProcess.UpTime)}");
            if (_childProcess.UpTime < 10)
            {
                //TODO maybe startup problem
            }
        }

        private void OnStdOut(object sender, string log)
        {
            Logger.log(log);
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
                var filePath = Path.Combine(_libraryDirectory, libraryInfo.Path);
                if (libraryInfo.Rules != null) continue;
                if (!libraryInfo.IsVaild(_libraryDirectory))
                {
                    await new Downloader.Downloader().DownloadFileTaskAsync(libraryInfo.Url, filePath);
                }
                libraryPath.Append(filePath).Append(";");
            }
            libraryPath.Append(Path.Combine(_versionDirectory, $"{_versionInfo.Id}.jar"));
            _arguments.Add("-cp");
            _arguments.Add(libraryPath.ToString());
            return true;
        }
    }
}