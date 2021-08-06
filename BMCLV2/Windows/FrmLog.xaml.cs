using System;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;

namespace BMCLV2
{
  /// <summary>
  ///   FrmLog.xaml 的交互逻辑
  /// </summary>
  public partial class FrmLog : Window
  {
    public FrmLog()
    {
      InitializeComponent();
    }

    public void WriteLine(string Log, Logger.LogType Type = Logger.LogType.Info)
    {
      switch (Type)
      {
        case Logger.LogType.Error:
          Log = DateTime.Now + "错误:" + Log;
          break;
        case Logger.LogType.Info:
          Log = DateTime.Now + "信息:" + Log;
          break;
        case Logger.LogType.Crash:
          Log = DateTime.Now + "崩溃:" + Log;
          break;
        case Logger.LogType.Exception:
          Log = DateTime.Now + "异常:" + Log;
          break;
        case Logger.LogType.Game:
          Log = DateTime.Now + "游戏:" + Log;
          break;
        case Logger.LogType.Fml:
          Log = DateTime.Now + "FML :" + Log;
          break;
        default:
          Log = DateTime.Now + "信息:" + Log;
          break;
      }

      Dispatcher.Invoke(new MethodInvoker(delegate
      {
        listLog.Items.Add(Log);
        listLog.ScrollIntoView(listLog.Items[listLog.Items.Count - 1]);
        while (listLog.Items.Count > 1000) listLog.Items.RemoveAt(0);
      }));
    }

    private void listLog_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
      try
      {
        var log = "";
        if (listLog.SelectedItems.Count <= 0) return;
        var sb = new StringBuilder();
        foreach (var selectedItem in listLog.SelectedItems) sb.AppendLine(selectedItem as string);
        log = sb.ToString();
        Clipboard.SetDataObject(log);
        MessageBox.Show(this, "复制成功");
      }
      catch (Exception)
      {
        // ignored
      }
    }
  }
}
