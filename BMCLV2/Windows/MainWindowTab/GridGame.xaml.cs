using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BMCLV2.Components;
using BMCLV2.Exceptions;
using BMCLV2.I18N;
using BMCLV2.Model;
using MaterialDesignThemes.Wpf;

namespace BMCLV2.Windows.MainWindowTab
{
  /// <summary>
  ///   GridGame.xaml 的交互逻辑
  /// </summary>
  public partial class GridGame
  {

    public GridGame()
    {
      InitializeComponent();
    }

    private void listVer_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (listVer.SelectedIndex == -1)
      {
        listVer.SelectedIndex = 0;
        return;
      }

      listVer.ScrollIntoView(listVer.SelectedItem);
      var id = GetSelectedVersion();
      var game = BmclCore.GameManager.GetVersion(id);
      labVer.Content = game.Id;
      labTime.Content = DateTime.Parse(game.Time).ToString(CultureInfo.CurrentCulture);
      labRelTime.Content = DateTime.Parse(game.ReleaseTime).ToString(CultureInfo.CurrentCulture);
      labType.Content = game.Type;

      ChangeButtonEnable(true); //Enable button after choosing version
    }

    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
      if (
        MessageBox.Show(LangManager.GetLangFromResource("DeleteMessageBoxInfo"),
          LangManager.GetLangFromResource("DeleteMessageBoxTitle"), MessageBoxButton.OKCancel,
          MessageBoxImage.Question) != MessageBoxResult.OK) return;
      try
      {
        if (BmclCore.GameInfo != null)
        {
          var isused = File.OpenWrite(".minecraft\\versions\\" + listVer.SelectedItem + "\\" + BmclCore.GameInfo.id +
                                      ".jar");
          isused.Close();
        }

        Directory.Delete(".minecraft\\versions\\" + listVer.SelectedItem, true);
        if (Directory.Exists(".minecraft\\libraries\\" + listVer.SelectedItem))
          Directory.Delete(".minecraft\\libraries\\" + listVer.SelectedItem, true);
      }
      catch (SystemException)
      {
        MessageBox.Show(LangManager.GetLangFromResource("DeleteFailedMessageInfo"));
      }

      ReFlushlistver();

      if (BmclCore.GameManager.GetVersions().Count == 0) //No version exists
      {
        labVer.Content = ""; //Clear label
        labTime.Content = "";
        labRelTime.Content = "";
        labType.Content = "";
        ChangeButtonEnable(false); //Disable button
      }
    }

    private void btnModMrg_Click(object sender, RoutedEventArgs e)
    {
      var version = listVer.SelectedItems.ToString();
      BmclCore.GameManager.SetupModPath(version);
      Process.Start(new ProcessStartInfo
      {
        FileName = "explorer.exe",
        Arguments =
          Path.Combine(BmclCore.GameManager.GetVersionPath(version), "mods")
      });
    }

    private void btnModCfgMrg_Click(object sender, RoutedEventArgs e)
    {
      var version = listVer.SelectedItems.ToString();
      BmclCore.GameManager.SetupModPath(version);
      Process.Start(new ProcessStartInfo
      {
        FileName = "explorer.exe",
        Arguments =
          Path.Combine(BmclCore.GameManager.GetVersionPath(version), "config")
      });
    }

    private void listVer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      BmclCore.MainWindow.ClickStartButton();
    }

    public void ReFlushlistver()
    {
      BmclCore.GameManager.ReloadList();
      BmclCore.Invoke(new Action(() => listVer.ItemsSource = BmclCore.GameManager.GetVersions().Keys.ToList()));
    }

    public string GetSelectedVersion()
    {
      var version = listVer.SelectedItem as string;
      if (version != null) return version;
      if (listVer.Items.Count == 1) return listVer.Items[0] as string;
      throw new NoSelectGameException();
    }

    private void ChangeButtonEnable(bool isEnable)
    {
      btnDelete.IsEnabled = isEnable;
      btnReName.IsEnabled = isEnable;
      btnModMrg.IsEnabled = isEnable;
      btnModCfgMrg.IsEnabled = isEnable;
    }

    private void BtnReName_OnClick(object sender, RoutedEventArgs e)
    {
      var inputDialog = new InputDialog();
      DialogHost.Show(inputDialog, (object o, DialogClosingEventArgs args) =>
      {
        if (!Equals(args.Parameter, "true")) return;
        try
        {
          var name = inputDialog.InputDialogModel.Text;
          if (name == listVer.SelectedItem.ToString()) return;
          if (listVer.Items.IndexOf(name) != -1)
            throw new Exception(LangManager.GetLangFromResource("RenameFailedExist"));
          Directory.Move(".minecraft\\versions\\" + listVer.SelectedItem, ".minecraft\\versions\\" + name);
        }
        catch (UnauthorizedAccessException)
        {
          MessageBox.Show(LangManager.GetLangFromResource("RenameFailedError"));
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);
        }

        ReFlushlistver();
      });
    }
  }
}
