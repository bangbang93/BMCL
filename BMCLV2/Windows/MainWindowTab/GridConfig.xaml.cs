using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BMCLV2.I18N;
using BMCLV2.Mirrors;

namespace BMCLV2.Windows.MainWindowTab
{
    /// <summary>
    /// GridConfig.xaml 的交互逻辑
    /// </summary>
    public partial class GridConfig
    {
        public GridConfig()
        {
            InitializeComponent();
            RefreshLangList();
            RefreshAuthList();
        }
        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            BmclCore.Config.Autostart = checkAutoStart.IsChecked != null && (bool)checkAutoStart.IsChecked;
            BmclCore.Config.ExtraJvmArg = txtExtJArg.Text;
            BmclCore.Config.Javaw = txtJavaPath.Text;
            BmclCore.Config.Javaxmx = txtJavaXmx.Text;
            BmclCore.Config.Login = listAuth.SelectedItem.ToString();
            BmclCore.Config.LastPlayVer = BmclCore.MainWindow.GridGame.GetSelectedVersion();
            BmclCore.Config.Passwd = Encoding.UTF8.GetBytes(txtPwd.Password);
            BmclCore.Config.Username = txtUserName.Text;
            BmclCore.Config.WindowTransparency = sliderWindowTransparency.Value;
            BmclCore.Config.Report = checkReport.IsChecked != null && checkReport.IsChecked.Value;
            BmclCore.Config.CheckUpdate = checkCheckUpdate.IsChecked != null && checkCheckUpdate.IsChecked.Value;
            BmclCore.Config.DownloadSource = listDownSource.SelectedIndex;
            BmclCore.Config.Lang = LangManager.GetLangFromResource("LangName");
            BmclCore.Config.Height = int.Parse(ScreenHeightTextBox.Text);
            BmclCore.Config.Width = int.Parse(ScreenWidthTextBox.Text);
            BmclCore.Config.FullScreen = FullScreenCheckBox.IsChecked??false;
            BmclCore.Config.Save(null);
            var dak = new DoubleAnimationUsingKeyFrames();
            dak.KeyFrames.Add(new LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(0)));
            dak.KeyFrames.Add(new LinearDoubleKeyFrame(30, TimeSpan.FromSeconds(0.3)));
            dak.KeyFrames.Add(new LinearDoubleKeyFrame(30, TimeSpan.FromSeconds(2.3)));
            dak.KeyFrames.Add(new LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(2.6)));
            popupSaveSuccess.BeginAnimation(FrameworkElement.HeightProperty, dak);
        }
        private void sliderJavaxmx_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtJavaXmx.Text = ((int)sliderJavaxmx.Value).ToString(CultureInfo.InvariantCulture);
        }
        private void txtUserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            tip.IsOpen = false;
        }
        private void btnSelectJava_Click(object sender, RoutedEventArgs e)
        {
            var ofJava = new System.Windows.Forms.OpenFileDialog
            {
                RestoreDirectory = true,
                Filter = @"Javaw.exe|Javaw.exe",
                Multiselect = false,
                CheckFileExists = true
            };
            if (ofJava.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtJavaPath.Text = ofJava.FileName;
            }
        }
        private void sliderWindowTransparency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (BmclCore.MainWindow.Container.Background != null)
                BmclCore.MainWindow.Container.Background.Opacity = e.NewValue;
        }

        private void listDownSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = listDownSource.SelectedIndex;
            BmclCore.MirrorManager.CurrectMirror = BmclCore.MirrorManager[index];
            BmclCore.Config.DownloadSource = index;
        }
        private void txtJavaXmx_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                sliderJavaxmx.Value = Convert.ToInt32(txtJavaXmx.Text);
            }
            catch (FormatException ex)
            {
                Logger.log(ex);
                MessageBox.Show("请输入一个有效数字");
                txtJavaXmx.Text = (Config.GetMemory()/4).ToString(CultureInfo.InvariantCulture);
                txtJavaXmx.SelectAll();
            }
            catch (XamlParseException ex)
            {
                Logger.log(ex);
                MessageBox.Show("请输入一个有效数字");
                txtJavaXmx.Text = (Config.GetMemory() / 4).ToString(CultureInfo.InvariantCulture);
                txtJavaXmx.SelectAll();
            }
        }

        private void txtUserName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtPwd.Focus();
                txtPwd.SelectAll();
            }
        }

        private void txtExtJArg_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtExtJArg.Text.IndexOf("-Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true", System.StringComparison.Ordinal) != -1)
            {
                checkOptifine.IsChecked = true;
            }
        }

        private void checkOptifine_Checked(object sender, RoutedEventArgs e)
        {
            if (txtExtJArg.Text.IndexOf("-Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true", System.StringComparison.Ordinal) != -1)
                return;
            txtExtJArg.Text += " -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true";
        }

        private void checkOptifine_Unchecked(object sender, RoutedEventArgs e)
        {
            txtExtJArg.Text = txtExtJArg.Text.Replace(" -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true", "");
        }

        private void checkCheckUpdate_Checked(object sender, RoutedEventArgs e)
        {
            BmclCore.Config.CheckUpdate = checkCheckUpdate.IsChecked == true;
        }

        private void comboLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!BmclCore.MainWindow.LoadOk)
                return;
            if (comboLang.SelectedItem as string != null)
                if (BmclCore.Language.ContainsKey(comboLang.SelectedItem as string))
                    LangManager.UseLanguage(BmclCore.Language[comboLang.SelectedItem as string] as string);
            BmclCore.MainWindow.ChangeLanguage();
            RefreshAuthList();
        }

        public void SaveConfig()
        {
            btnSaveConfig_Click(null, null);
        }

        public void RefreshLangList()
        {
            var langs = LangManager.ListLanuage();
            foreach (var lang in langs)
            {
                comboLang.Items.Add(lang);
            }
            comboLang.SelectedItem = LangManager.GetLangFromResource("LangName");
        }

        public void RefreshAuthList()
        {
            listAuth.Items.Clear();
            listAuth.Items.Add(LangManager.GetLangFromResource("NoneAuth"));
            foreach (var auth in BmclCore.Auths)
            {
                listAuth.Items.Add(auth.Key);
            }
            listAuth.SelectedItem = BmclCore.Config.Login;
            if (listAuth.SelectedItem == null)
            {
                listAuth.SelectedIndex = 0;
            }
        }

        private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (txtUserName.Text == "!!!" && (bool) e.NewValue)
            {
                tip.IsOpen = true;
                tip.Margin = txtUserName.Margin;
            }
            else
            {
                tip.IsOpen = false;
            }
        }
    }
}
