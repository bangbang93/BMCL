using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace BMCLV2
{
  /// <summary>
  ///   FrmPrs.xaml 的交互逻辑
  /// </summary>
  public partial class FrmPrs : Window
  {
    public FrmPrs(string name)
    {
      InitializeComponent();
      labName.Content = name;
    }

    public void ChangeStatus(string status)
    {
      Dispatcher.Invoke(new MethodInvoker(delegate
      {
        labStatus.Content = status;
        Logger.Log(status);
      }));
    }

    private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
    {
      try
      {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
      }
      catch
      {
      }
    }
  }
}
