using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace BMCLV2
{
    static class Logger
    {
        public enum LogType
        {
            Error,Info,Crash,Exception,Game,Fml,
        }
        static StreamWriter sw;
        static public bool Debug = false;
        static public void Start()
        {
            if (Debug)
            {
                FileStream fs = new FileStream(Environment.CurrentDirectory + "\\bmcl.log", FileMode.Create, FileAccess.ReadWrite,FileShare.Read);
                sw = new StreamWriter(fs,Encoding.UTF8);
            }
        }
        static public void Stop()
        {
            if (Debug)
            {
                sw.Close();
            }
        }

        static private void WriteInfo(LogType Type = LogType.Info)
        {
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
        }
        static private void Write(string str, LogType Type = LogType.Info)
        {
            if (Debug)
            {
                WriteInfo(Type);
                sw.WriteLine(str);
            }
        }
        static private void Write(Stream s, LogType Type = LogType.Info)
        {
            if (Debug)
            {
                WriteInfo(Type);
                StreamReader sr = new StreamReader(s);
                Write(sr.ReadToEnd(), Type);
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
    }
}
