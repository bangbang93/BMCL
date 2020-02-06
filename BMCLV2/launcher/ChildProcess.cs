using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BMCLV2.Exceptions;

namespace BMCLV2.Launcher
{
    public class ChildProcess
    {
        private ProcessStartInfo _processStartInfo;
        private readonly string _filename;
        private readonly string[] _arguments;
        private readonly string _workingDirectory;
        private Process _childProcess;
        private OnChildProcessExit _onExit;
        private OnLogEventHandler _onStdOut;
        private OnLogEventHandler _onStdErr;

        public DateTime StartTime;

        public int UpTime => (int)(DateTime.Now - StartTime).TotalSeconds;

        public event OnChildProcessExit OnExit
        {
            add { _onExit += value; }
            remove { _onExit -= value; }
        }

        public event OnLogEventHandler OnStdOut
        {
            add { _onStdOut += value; }
            remove { _onStdOut -= value; }
        }

        public event OnLogEventHandler OnStdErr
        {
            add { _onStdErr += value; }
            remove { _onStdErr -= value; }
        }

        public ChildProcess(ProcessStartInfo processStartInfo)
        {
            _processStartInfo = processStartInfo;
            _filename = processStartInfo.FileName;
            _arguments = processStartInfo.Arguments.Split();
        }

        public ChildProcess(string filename, string[] arguments)
        {
            _arguments = arguments;
            _filename = filename;
        }

        public ChildProcess(string filename, string workingDirectory = null, string[] arguments = null)
        {
            _filename = filename;
            _arguments = arguments;
            _workingDirectory = workingDirectory;
        }

        public static string JoinArguments(string[] arguments)
        {
            var sb = new StringBuilder();
            foreach (var argument in arguments)
            {
                sb.Append('"').Append(argument.Replace("\"", "\\\"")).Append("\" ");
            }
            return sb.ToString(0, Math.Max(sb.Length - 1, 0));
        }

        public bool Start()
        {
            Close();
            Logger.Info($"{_filename} {JoinArguments(_arguments)}");
            _processStartInfo = new ProcessStartInfo(_filename, JoinArguments(_arguments))
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = BmclCore.MinecraftDirectory
            };
            if (_workingDirectory != null)
            {
                _processStartInfo.WorkingDirectory = _workingDirectory;
            }
            _childProcess = new Process
            {
                StartInfo = _processStartInfo,
                EnableRaisingEvents = true
            };
            _childProcess.Exited += (sender, args) => _onExit?.Invoke(sender, _childProcess.ExitCode);
            _childProcess.OutputDataReceived += (sender, args) => _onStdOut?.Invoke(sender, args.Data);
            _childProcess.ErrorDataReceived += (sender, args) => _onStdErr?.Invoke(sender, args.Data);
            _childProcess.Start();
            _childProcess.BeginOutputReadLine();
            _childProcess.BeginErrorReadLine();
            StartTime = DateTime.Now;
            return true;
        }

        public StreamReader GetStdOut()
        {
            if (_childProcess == null || _childProcess.HasExited)
            {
                throw new ProcessNotStartException();
            }
            return _childProcess.StandardOutput;
        }

        public StreamReader GetStdErr()
        {
            if (_childProcess == null || _childProcess.HasExited)
            {
                throw new ProcessNotStartException();
            }
            return _childProcess.StandardError;
        }

        public StreamWriter GetStdIn()
        {
            if (_childProcess == null || _childProcess.HasExited)
            {
                throw new ProcessNotStartException();
            }
            return _childProcess.StandardInput;
        }

        public void Close()
        {
            if (_childProcess == null || _childProcess.HasExited) return;
            _childProcess.Close();
            _onExit(this, _childProcess.ExitCode);
            _childProcess = null;
        }

        public Task WaitForExitAsync(CancellationToken cancellationToken = default)
        {
          var tcs = new TaskCompletionSource<object>();
          _childProcess.EnableRaisingEvents = true;
          _childProcess.Exited += (sender, args) => tcs.TrySetResult(null);
          if (cancellationToken != default)
            cancellationToken.Register(tcs.SetCanceled);

          return tcs.Task;
        }

    public static List<string> SplitCommandLine(string commandLine)
        {
            var inQuote = false;
            var inEscape = false;
            var sb = new StringBuilder();
            var args = new List<string>();
            foreach (var ch in commandLine)
            {
                switch (ch)
                {
                    case '\\':
                        inEscape = true;
                        break;
                    case '"':
                        if (inEscape)
                        {
                            sb.Append("\\\"");
                            inEscape = false;
                        }
                        else
                        {
                            inQuote = !inQuote;
                            args.Add(sb.ToString());
                            sb.Clear();
                        }
                        break;
                    case ' ':
                        if (inQuote) break;
                        args.Add(sb.ToString());
                        sb.Clear();
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            if (sb.Length > 0) args.Add(sb.ToString());
            args.RemoveAll(str => str == "");
            return args;
        }

        public static ChildProcess Exec(string filename, string[] arguments = null)
        {
            var childProcess = new ChildProcess(filename, arguments);
            childProcess.Start();
            return childProcess;
        }
    }
}
