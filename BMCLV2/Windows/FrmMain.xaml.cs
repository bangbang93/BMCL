using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using BMCLV2.Exceptions;
using BMCLV2.Game;
using BMCLV2.I18N;
using BMCLV2.Plugin;
using BMCLV2.Themes;

namespace BMCLV2.Windows
{
    /// <summary>
    /// FrmMain.xaml 的交互逻辑
    /// </summary>
    public partial class FrmMain
    {
        private bool _inscreen;
        private bool _isLaunchering;

        private FrmPrs _frmPrs;

        private readonly Background _background = new Background();

        public FrmMain()
        {
            BmclCore.NIcon.MainWindow = this;
            BmclCore.MainWindow = this;
            InitializeComponent();
            Title = "BMCL Ver." + BmclCore.BmclVersion;
            LoadConfig();
            GridGame.ReFlushlistver();
            GridGame.listVer.SelectedItem = BmclCore.Config.LastPlayVer;
            BmclCore.SingleInstance(this);
        }

        private void LoadConfig()
        {
            GridConfig.txtUserName.Text = BmclCore.Config.Username;
            GridConfig.txtPwd.Password = Encoding.UTF8.GetString(BmclCore.Config.Passwd);
            GridConfig.txtJavaPath.Text = BmclCore.Config.Javaw;
            GridConfig.sliderJavaxmx.Maximum = Config.GetMemory();
            GridConfig.txtJavaXmx.Text = BmclCore.Config.Javaxmx;
            GridConfig.sliderJavaxmx.Value = int.Parse(BmclCore.Config.Javaxmx);
            GridConfig.txtExtJArg.Text = BmclCore.Config.ExtraJvmArg;
            GridConfig.checkAutoStart.IsChecked = BmclCore.Config.Autostart;
            if (GridConfig.listAuth.SelectedItem == null)
                GridConfig.listAuth.SelectedIndex = 0;
            GridConfig.sliderWindowTransparency.Value = BmclCore.Config.WindowTransparency;
            GridConfig.checkReport.IsChecked = BmclCore.Config.Report;
            GridForge.txtInsPath.Text = AppDomain.CurrentDomain.BaseDirectory + ".minecraft";
            GridConfig.listDownSource.SelectedIndex = BmclCore.Config.DownloadSource;
            GridConfig.comboLang.SelectedItem = LangManager.GetLangFromResource("DisplayName");
            GridConfig.ScreenHeightTextBox.Text = BmclCore.Config.Height.ToString();
            GridConfig.ScreenWidthTextBox.Text = BmclCore.Config.Width.ToString();
            GridConfig.FullScreenCheckBox.IsChecked = BmclCore.Config.FullScreen;
            GridConfig.checkCheckUpdate.IsChecked = BmclCore.Config.CheckUpdate;
        }

        public void SwitchStartButton(bool isenable)
        {
            btnStart.IsEnabled = isenable;
        }

        public void ClickStartButton()
        {
            if (btnStart.IsEnabled)
            {
                btnStart_Click(null, null);
            }
        }

        public void ChangeDownloadProgress(int value, int maxValue)
        {
            prsDown.Maximum = maxValue;
            prsDown.Value = value;
        }

        public void ChangeDownloadProgress(long value, long maxValue)
        {
            this.ChangeDownloadProgress((int)value, (int)maxValue);
        }

        public void SwitchDownloadGrid(Visibility visibility)
        {
            gridDown.Visibility = visibility;
        }

        public void SetDownloadInfo(string info)
        {
            labDownInfo.Content = info;
        }

        #region 公共按钮
        private void btnChangeBg_Click(object sender, RoutedEventArgs e)
        {
            var background = _background.GetRadnomImageBrush();
            if (background != null)
            {
                var da = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.25));
                BeginAnimation(OpacityProperty, da);
                Container.Background = background;
                da = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25));
                BeginAnimation(OpacityProperty, da);
            }
            else
            {
                btnChangeBg.IsEnabled = false;
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (BmclCore.Game!=null)
                if (BmclCore.Game.IsRunning())
                {
                    this.btnMiniSize_Click(null, null);
                    return;
                }
            Logger.Log($"BMCL V2 Ver.{BmclCore.BmclVersion} 正在退出");
            BmclCore.Halt();
        }
        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //TODO 分离launcher方法
            if (GridConfig.txtUserName.Text == "!!!")
            {
                MessageBox.Show(this, "请先修改用户名");
                TabMain.SelectedIndex = 1;
                GridConfig.txtUserName.Focus();
                return;
            }
            GridConfig.SaveConfig();
            var somethingBad = false;
            try
            {
                var selectedVersion = GridGame.GetSelectedVersion();
                Logger.Info($"正在启动{selectedVersion},使用的登陆方式为{GridConfig.listAuth.SelectedItem}");
                _frmPrs = new FrmPrs(LangManager.GetLangFromResource(selectedVersion));
                _frmPrs.Show();
                _frmPrs.ChangeStatus(LangManager.GetLangFromResource("LauncherAuth"));
                var launcher = await BmclCore.GameManager.LaunchGame(selectedVersion, false);
                if (launcher == null)
                {
                    _frmPrs.Close();
                    _frmPrs = null;
                    return;
                }
                launcher.OnGameLaunch += Launcher_OnGameLaunch;
                launcher.OnGameStart += Game_GameStartUp;
                launcher.OnGameExit += launcher_gameexit;
                await launcher.Start();
            }
            catch (NoSelectGameException exception)
            {
                Logger.Fatal(exception);
                somethingBad = true;
                MessageBox.Show(this, exception.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (NoJavaException exception)
            {
                Logger.Fatal(exception);
                somethingBad = true;
                MessageBox.Show(this, exception.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (AnotherGameRunningException exception)
            {
                Logger.Fatal(exception);
                somethingBad = true;
                MessageBox.Show(this, exception.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DownloadLibException exception)
            {
                Logger.Fatal(exception);
                somethingBad = true;
                MessageBox.Show(this, exception.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (somethingBad)
                {
                    _frmPrs?.Close();
                    _frmPrs = null;
                }
            }
            
        }

        private void Launcher_OnGameLaunch(object sender, string status, VersionInfo versionInfo)
        {
            _frmPrs.ChangeStatus(LangManager.GetLangFromResource(status));
        }

        private void Game_GameStartUp(object sender, VersionInfo versionInfo)
        {
            BmclCore.NIcon.NIcon.Visible = true;
            BmclCore.NIcon.ShowBalloonTip(10000, "启动成功" + versionInfo.Id);
            _frmPrs.Close();
            _frmPrs = null;
            _isLaunchering = false;
            Hide();
        }

        private void launcher_gameexit(object sender, VersionInfo versionInfo, int exitCode)
        {
            if (Logger.Debug)
            {
                Logger.Log("游戏退出，Debug模式保留Log信息窗口，程序不退出");
                Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(this.Show));
                return;
            }
            if (_inscreen) return;
            Logger.Log("BMCL V2 Ver" + BmclCore.BmclVersion + DateTime.Now + "由于游戏退出而退出");
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(() => Application.Current.Shutdown(0)));
        }

        private void btnMiniSize_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            BmclCore.NIcon.NIcon.ShowBalloonTip(2000, "BMCL", LangManager.GetLangFromResource("BMCLHiddenInfo"), System.Windows.Forms.ToolTipIcon.Info);
        }
        private void MenuSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var ofbg = new System.Windows.Forms.OpenFileDialog
            {
                CheckFileExists = true,
                Filter = @"支持的图片|*.jpg;*.png;*.bmp",
                Multiselect = false
            };
            string pic;
            if (ofbg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                pic = ofbg.FileName;
            else
                return;
            var b = new ImageBrush {ImageSource = new BitmapImage(new Uri((pic))), Stretch = Stretch.Fill};
            var da = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.25));
            this.BeginAnimation(OpacityProperty, da);
            this.Container.Background = b;
            da = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25));
            this.BeginAnimation(OpacityProperty, da);
        }
        private void MenuSelectTexturePack_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("这是个正在试验的功能，请不要报告有关任何该功能的bug");
            var frmTexturepack = new FrmTexturepack();
            frmTexturepack.ShowDialog();
            Texturepack.TexturePackEntity texture = frmTexturepack.GetSelected();
            var b = new ImageBrush();
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = texture.GuiBackground;
            bitmap.EndInit();
            b.ImageSource = bitmap;
            b.ViewportUnits = BrushMappingMode.Absolute;
            b.Viewport = new Rect(0, 0, bitmap.Width, bitmap.Height);
            b.Stretch = Stretch.None;
            b.TileMode = TileMode.Tile;
            var button = new ImageBrush {ImageSource = texture.GuiButton.Source};

            var da = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.25));
            this.BeginAnimation(UIElement.OpacityProperty, da);
            this.Container.Background = b;
            btnStart.Background = button;
            da = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25));
            this.BeginAnimation(UIElement.OpacityProperty, da);
        }
        #endregion

        public bool LoadOk = false;
        private void FrmMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (BmclCore.Config.Username == "!!!")
            {
                TabMain.SelectedIndex = 1;
                GridConfig.tip.IsOpen = true;
                GridConfig.txtUserName.Focus();
            }
            else
            {
                if (BmclCore.Config.Autostart)
                {
                    btnStart_Click(null, null);
                    LoadOk = true;
                    this.Hide();
                    return;
                }
            }
            var da = new DoubleAnimation {From = 0, To = 1, Duration = TimeSpan.FromSeconds(0.8)};
            this.FrmMainWindow.BeginAnimation(UIElement.OpacityProperty, da);
            try
            {
                var rand = new Random();
                int img = rand.Next(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\bg").Length);
                var b = new ImageBrush
                {
                    ImageSource =
                        new BitmapImage(
                            new Uri((Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\bg")[img]))),
                    Stretch = Stretch.Fill
                };
                this.Container.Background = b;
            }
            catch
            {
                var b=new SolidColorBrush(Color.FromRgb(255,255,255));
                this.Container.Background = b;
            }
            LoadOk = true;
        }
        private void FrmMainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        int _lasttabMainSelectIndex = -1;
        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_lasttabMainSelectIndex == TabMain.SelectedIndex)
                return;
            _lasttabMainSelectIndex = TabMain.SelectedIndex;
            if (!LoadOk)
                return;
            var da1 = new DoubleAnimation(0, TabMain.ActualWidth, new Duration(new TimeSpan(0, 0, 0, 0, 100)));
            var da2 = new DoubleAnimation(0, TabMain.ActualHeight, new Duration(new TimeSpan(0, 0, 0, 0, 100)));
            switch (TabMain.SelectedIndex)
            {
                case 0: 
                    GridGame.BeginAnimation(FrameworkElement.WidthProperty, da1); GridGame.BeginAnimation(FrameworkElement.HeightProperty, da2); 
                    break;
                case 1: 
                    GridConfig.BeginAnimation(FrameworkElement.WidthProperty, da1); GridConfig.BeginAnimation(FrameworkElement.HeightProperty, da2); 
                    break;
                case 2: 
                    GridVersion.BeginAnimation(FrameworkElement.WidthProperty, da1); GridVersion.BeginAnimation(FrameworkElement.HeightProperty, da2); 
                    if (GridVersion.btnRefreshRemoteVer.IsEnabled && GridVersion.listRemoteVer.HasItems == false) GridVersion.RefreshVersion(); 
                    break;
                case 3: 
                    GridForge.BeginAnimation(FrameworkElement.WidthProperty, da1); GridForge.BeginAnimation(FrameworkElement.HeightProperty, da2); 
                    if (GridForge.btnReForge.IsEnabled && GridForge.treeForgeVer.HasItems == false) GridForge.RefreshForge(); 
                    break;
                case 4:
                    gridUpdateInfo.BeginAnimation(FrameworkElement.WidthProperty, da1); 
                    gridUpdateInfo.BeginAnimation(FrameworkElement.HeightProperty, da2); 
                    break;
            }
        }

        private void FrmMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BmclCore.NIcon.Hide();
        }

        private void FrmMainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                _inscreen = true;
                btnChangeBg_Click(null, null);
            }
            else
                _inscreen = false;
            
        }

        public void ChangeLanguage()
        {
            GridConfig.listDownSource.Items[1] = LangManager.GetLangFromResource("listOfficalSource");
            GridConfig.listDownSource.Items[0] = LangManager.GetLangFromResource("listAuthorSource");
            BmclCore.PluginManager.LoadOldAuthPlugin(LangManager.GetLangFromResource("LangName"));
        }

        private void MenuStartDebug_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(LangManager.GetLangFromResource("MenuDebugHint").Replace("\\n", "\n"));
            Logger.Debug = true;
            btnStart_Click(null, null);
        }

        private void FrmMainWindow_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_isLaunchering)
            {
                _frmPrs?.Activate();
            }
        }

        private void FrmMainWindow_Activated(object sender, EventArgs e)
        {
            if (_isLaunchering)
            {
                _frmPrs?.Activate();
            }
        }



        




    }
}
