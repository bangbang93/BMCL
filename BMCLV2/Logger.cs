using System;
using System.Globalization;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Collections;

namespace BMCLV2
{
    public static class Logger
    {
        public enum LogType
        {
            Error,Info,Crash,Exception,Game,Fml,
        }
        
        public static bool Debug = false;
        private static readonly FrmLog FrmLog = new FrmLog();
        private static StreamWriter _logFile;
        public static void Start(bool append = false)
        {
            var filemode = append ? FileMode.Append : FileMode.Create;
            _logFile = new StreamWriter(new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\bmcl.log", filemode, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
            _logFile.Flush();
            _logFile.AutoFlush = true;
            if (Debug) FrmLog.Show();
        }
        public static void Stop()
        {
            _logFile.Close();
            if (Debug) FrmLog.Close();
        }

        private static string WriteInfo(LogType type = LogType.Info)
        {
            switch (type)
            {
                case LogType.Error:
                    return (DateTime.Now.ToString(CultureInfo.InvariantCulture) + "错误:");
                case LogType.Info:
                    return (DateTime.Now.ToString(CultureInfo.InvariantCulture) + "信息:");
                case LogType.Crash:
                    return (DateTime.Now.ToString(CultureInfo.InvariantCulture) + "崩溃:");
                case LogType.Exception:
                    return (DateTime.Now.ToString(CultureInfo.InvariantCulture) + "异常:");
                case LogType.Game:
                    return (DateTime.Now.ToString(CultureInfo.InvariantCulture) + "游戏:");
                case LogType.Fml:
                    return (DateTime.Now.ToString(CultureInfo.InvariantCulture) + "FML :");
                default:
                    return (DateTime.Now.ToString(CultureInfo.InvariantCulture) + "信息:");
            }
        }
        private static void Write(string str, LogType type = LogType.Info)
        {
            _logFile.WriteLine(WriteInfo(type) + str);
            if (Debug) FrmLog.WriteLine(str, type);
        }
        private static void Write(Stream s, LogType type = LogType.Info)
        {
            StreamReader sr = new StreamReader(s);
            Write(sr.ReadToEnd(), type);
            if (Debug)
            {
                s.Position = 0;
                FrmLog.WriteLine(sr.ReadToEnd(),type);
            }
        }


        public static void Log(string str, LogType type = LogType.Info)
        {
            Write(str, type);
        }
        public static void Log(Config cfg, LogType type = LogType.Info)
        {
            Write(cfg.ToString(), type);
        }
        public static void Log(Stream s, LogType type = LogType.Info)
        {
            StreamReader sr = new StreamReader(s);
            Write(sr.ReadToEnd(), type);
        }
        public static void Log(Exception ex, LogType type = LogType.Exception)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine(ex.Source);
            message.AppendLine(ex.ToString());
            message.AppendLine(ex.Message);
            foreach (DictionaryEntry data in ex.Data)
                message.AppendLine($"Key:{data.Key}\nValue:{data.Value}");
            message.AppendLine(ex.StackTrace);
            var iex = ex;
            while (iex.InnerException != null)
            {
                message.AppendLine("------------------------");
                iex = iex.InnerException;
                message.AppendLine(iex.Source);
                message.AppendLine(iex.ToString());
                message.AppendLine(iex.Message);
                foreach (DictionaryEntry data in ex.Data)
                    message.AppendLine($"Key:{data.Key}\nValue:{data.Value}");
                message.AppendLine(iex.StackTrace);
            }
            Write(message.ToString(), type);
        }

        public static void Log(LogType type = LogType.Info, params string[] messages)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string str in messages)
            {
                sb.Append(str);
            }
            Write(sb.ToString(), type);
        }

        public static void Log(params string[] messages)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string str in messages)
            {
                sb.Append(str).Append(" ");
            }
            Write(sb.ToString());
        }

        public static void Info(string message)
        {
            Log(message);
        }

        public static void Fatal(string message)
        {
            Log(message, LogType.Error);
        }

        public static void Fatal(Exception ex)
        {
            Log(ex);
        }
        

    }
}
