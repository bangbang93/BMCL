using System.Windows.Input;
using BMCLV2.Model;

namespace BMCLV2.Components
{
  public partial class InputDialog
  {

    public readonly InputDialogModel InputDialogModel = new();
    public InputDialog()
    {
      InitializeComponent();
      DataContext = InputDialogModel;
    }


    private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter) return;
      BtnOk.Focus();
      BtnOk.Command.Execute(BtnOk.CommandParameter);
      e.Handled = true;
    }
  }
}

