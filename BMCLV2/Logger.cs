using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace BMCLV2
{
    static public class Logger
    {
        public enum LogType
        {
            Error,Info,Crash,Exception,Game,Fml,
        }
        
        static public bool Debug = false;
        static FrmLog frmLog = new FrmLog();
        static public void Start()
        {
            FileStream fs = new FileStream(Environment.CurrentDirectory + "\\bmcl.log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            fs.Close();
            if (Debug)
            {
                frmLog.Show();
            }
        }
        static public void Stop()
        {
            if (Debug)
            {
                frmLog.Close();
            }
        }

        static private void WriteInfo(LogType Type = LogType.Info)
        {
            FileStream fs = new FileStream(Environment.CurrentDirectory + "\\bmcl.log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            switch (Type)
            {
                case LogType.Error:
                    sw.Write(DateTime.Now.ToString() + "错误:");
                    break;
                case LogType.Info:
                    sw.Write(DateTime.Now.ToString() + "信息:");
                    break;
                case LogType.Crash:
                    sw.Write(DateTime.Now.ToString() + "崩溃:");
                    break;
                case LogType.Exception:
                    sw.Write(DateTime.Now.ToString() + "异常:");
                    break;
                case LogType.Game:
                    sw.Write(DateTime.Now.ToString() + "游戏:");
                    break;
                case LogType.Fml:
                    sw.Write(DateTime.Now.ToString() + "FML :");
                    break;
                default:
                    sw.Write(DateTime.Now.ToString() + "信息:");
                    break;
            }
            sw.Close();
            fs.Close();
        }
        static private void Write(string str, LogType Type = LogType.Info)
        {
            WriteInfo(Type);
            FileStream fs = new FileStream(Environment.CurrentDirectory + "\\bmcl.log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            sw.WriteLine(str);
            sw.Close();
            fs.Close();
            if (Debug)
            {
                frmLog.WriteLine(str,Type);
            }
        }
        static private void Write(Stream s, LogType Type = LogType.Info)
        {
            StreamReader sr = new StreamReader(s);
            Write(sr.ReadToEnd(), Type);
            if (Debug)
            {
                s.Position = 0;
                frmLog.WriteLine(sr.ReadToEnd(),Type);
            }
        }


        static public void Log(string str, LogType Type = LogType.Info)
        {
            Write(str, Type);
        }
        static public void Log(config cfg, LogType Type = LogType.Info)
        {
            DataContractSerializer cfgSerializer = new DataContractSerializer(typeof(config));
            MemoryStream ms=new MemoryStream();
            cfgSerializer.WriteObject(ms, cfg);
            ms.Position = 0;
            Write(ms, Type);
        }
        static public void Log(Stream s, LogType Type = LogType.Info)
        {
            StreamReader sr = new StreamReader(s);
            Write(sr.ReadToEnd(), Type);
        }
        static public void Log(Exception ex, LogType Type = LogType.Exception)
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine(ex.Message);
            str.AppendLine(ex.StackTrace);
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                str.AppendLine(ex.Message);
                str.AppendLine(ex.StackTrace);
            }
            Write(str.ToString(), Type);
        }
    }
}
