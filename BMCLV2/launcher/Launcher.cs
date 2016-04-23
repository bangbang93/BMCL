using System;
using System.Collections.Generic;
using System.Diagnostics;
using BMCLV2.Game;

namespace BMCLV2.Launcher
{
    public class Launcher
    {
        private ChildProcess _childProcess;
        private readonly Config _config;
        private readonly List<string> _arguments = new List<string>();
        private VersionInfo _versionInfo;

        public Launcher(VersionInfo versionInfo, Config config = null, bool disableXincgc = false)
        {
            _versionInfo = versionInfo;
            _config = config ?? Config.Load();
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

            Launch();
        }

        private void Launch()
        {
            _childProcess = new ChildProcess(_config.Javaw, _arguments.ToArray());
            if (_childProcess.Start())
            {
                _childProcess.OnStdOut += OnStdOut;
                _childProcess.OnStdErr += OnStdOut;
                _childProcess.OnExit += ChildProcessOnOnExit;
            }
        }

        private void ChildProcessOnOnExit(object sender, int exitCode)
        {
            Logger.log($"{_versionInfo.Id} has exited with exit code {exitCode}");
        }

        private void OnStdOut(object sender, string log)
        {
            Logger.log(log);
        }


    }
}