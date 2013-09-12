using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BMCLV2
{
    /// <summary>
    /// FrmLog.xaml 的交互逻辑
    /// </summary>
    public partial class FrmLog : Window
    {
        long line = 0;
        public FrmLog()
        {
            InitializeComponent();
        }
        public void WriteLine(string Log, Logger.LogType Type = Logger.LogType.Info)
        {
            switch (Type)
            {
                case Logger.LogType.Error:
                    Log = DateTime.Now.ToString() + "错误:" + Log;
                    break;
                case Logger.LogType.Info:
                    Log = DateTime.Now.ToString() + "信息:" + Log;
                    break;
                case Logger.LogType.Crash:
                    Log = DateTime.Now.ToString() + "崩溃:" + Log;
                    break;
                case Logger.LogType.Exception:
                    Log = DateTime.Now.ToString() + "异常:" + Log;
                    break;
                case Logger.LogType.Game:
                    Log = DateTime.Now.ToString() + "游戏:" + Log;
                    break;
                case Logger.LogType.Fml:
                    Log = DateTime.Now.ToString() + "FML :" + Log;
                    break;
                default:
                    Log = DateTime.Now.ToString() + "信息:" + Log;
                    break;
            }
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate {listLog.Items.Add(Log); listLog.ScrollIntoView(listLog.Items[listLog.Items.Count - 1]); }));
        }

    }
}
