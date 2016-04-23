using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
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
        private Process _childProcess;
        private OnChildProcessExit _onExit;
        private OnLogEventHandler _onStdOut;
        private OnLogEventHandler _onStdErr;

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

        public ChildProcess(string filename, string[] arguments = null)
        {
            if (arguments != null)
            {
                _processStartInfo = new ProcessStartInfo(filename, JoinArguments(arguments));
            }
            _filename = filename;
            _arguments = arguments;
        }

        public static string JoinArguments(string[] arguments)
        {
            var sb = new StringBuilder();
            foreach (var argument in arguments)
            {
                sb.Append(@"\" + Regex.Replace(argument, @"(\\+)$", @"$1$1") + @"\").Append(" ");
            }
            return sb.ToString(0, Math.Max(sb.Length - 1, 0));
        }

        public bool Start()
        {
            Close();
            _processStartInfo = new ProcessStartInfo(_filename, JoinArguments(_arguments));
            _childProcess = new Process {StartInfo = _processStartInfo};
            var result = _childProcess.Start();
            if (!result) return false;
            _childProcess.Exited += (sender, args) => _onExit?.Invoke(sender, args);
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
            if (_childProcess.HasExited)
                return;
            _childProcess.Close();
            _onExit(this, _childProcess.ExitCode);
            _childProcess = null;
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
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return args;
        }
    }
}