using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Collections;

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
            FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\bmcl.log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
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

        static private string WriteInfo(LogType Type = LogType.Info)
        {
            switch (Type)
            {
                case LogType.Error:
                    return (DateTime.Now.ToString() + "错误:");
                case LogType.Info:
                    return (DateTime.Now.ToString() + "信息:");
                case LogType.Crash:
                    return (DateTime.Now.ToString() + "崩溃:");
                case LogType.Exception:
                    return (DateTime.Now.ToString() + "异常:");
                case LogType.Game:
                    return (DateTime.Now.ToString() + "游戏:");
                case LogType.Fml:
                    return (DateTime.Now.ToString() + "FML :");
                default:
                    return (DateTime.Now.ToString() + "信息:");
            }
        }
        static private void Write(string str, LogType Type = LogType.Info)
        {
            FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\bmcl.log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            sw.WriteLine(WriteInfo(Type) + str);
            sw.Close();
            fs.Close();
            if (Debug)
            {
                frmLog.WriteLine(WriteInfo(Type) + str, Type);
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
            StringBuilder Message = new StringBuilder();
            Message.AppendLine(ex.Source);
            Message.AppendLine(ex.ToString());
            Message.AppendLine(ex.Message);
            foreach (DictionaryEntry data in ex.Data)
                Message.AppendLine(string.Format("Key:{0}\nValue:{1}", data.Key, data.Value));
            Message.AppendLine(ex.StackTrace);
            var iex = ex;
            while (iex.InnerException != null)
            {
                Message.AppendLine("------------------------");
                iex = iex.InnerException;
                Message.AppendLine(iex.Source);
                Message.AppendLine(iex.ToString());
                Message.AppendLine(iex.Message);
                foreach (DictionaryEntry data in ex.Data)
                    Message.AppendLine(string.Format("Key:{0}\nValue:{1}", data.Key, data.Value));
                Message.AppendLine(iex.StackTrace);
            }
            Write(Message.ToString(), Type);
        }

        static public void Log(LogType Type = LogType.Info, params string[] Messages)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string str in Messages)
            {
                sb.Append(str);
            }
            Write(sb.ToString(), Type);
        }

        static public void Log(params string[] Messages)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string str in Messages)
            {
                sb.Append(str);
            }
            Write(sb.ToString(), LogType.Info);
        }
    }
}
