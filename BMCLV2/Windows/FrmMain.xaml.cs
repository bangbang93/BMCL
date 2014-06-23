using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using BMCLV2.Lang;
using BMCLV2.Login;

namespace BMCLV2.Windows
{
    /// <summary>
    /// FrmMain.xaml 的交互逻辑
    /// </summary>
    public partial class FrmMain
    {
        private bool _inscreen;
        private bool _isLaunchering;
        public bool Debug;
        
        private int _clientCrashReportCount;
        private FrmPrs _starter;

        public FrmMain()
        {
            BmclCore.NIcon.MainWindow = this;
            BmclCore.MainWindow = this;
            InitializeComponent();
            this.Title = "BMCL V2 Ver." + BmclCore.BmclVersion;
            this.LoadConfig();
            this.LoadLanguage();
            GridConfig.listAuth.Items.Add(LangManager.GetLangFromResource("NoneAuth"));
            foreach (var auth in BmclCore.Auths)
            {
                GridConfig.listAuth.Items.Add(auth.Key);
            }
            GridGame.ReFlushlistver();
            GridGame.listVer.SelectedItem = BmclCore.Config.LastPlayVer;
            GridConfig.listAuth.SelectedItem = BmclCore.Config.Login;
            GridConfig.checkCheckUpdate.IsChecked = BmclCore.Config.CheckUpdate;
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
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\bg"))
            {
                var rand = new Random();
                var pics = new ArrayList();
                foreach (string str in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\bg", "*.jpg", SearchOption.AllDirectories))
                {
                    pics.Add(str);
                }
                foreach (string str in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\bg", "*.png", SearchOption.AllDirectories))
                {
                    pics.Add(str);
                }
                foreach (string str in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\bg", "*.bmp", SearchOption.AllDirectories))
                {
                    pics.Add(str);
                }
                int imgTotal = pics.Count; 
                if (imgTotal == 0)
                {
                    if (e != null)
                        MessageBox.Show("没有可用的背景图");
                    return;
                }
                if (imgTotal == 1)
                {
                    if (e != null)
                        MessageBox.Show("只有一张可用的背景图哦");
                    return;
                }
                int img = rand.Next(imgTotal);
                var b = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(((string) pics[img]))),
                    Stretch = Stretch.Fill
                };
                var da = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.25));
                this.BeginAnimation(UIElement.OpacityProperty, da);
                this.Container.Background = b;
                da = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25));
                this.BeginAnimation(UIElement.OpacityProperty, da);
            }
            else
            {
                if (e == null)
                    return;
                MessageBox.Show("请在启动启动其目录下新建bg文件夹，并放入图片文件，支持jpg,bmp,png等格式，比例请尽量接近16:9，否则会被拉伸");
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\bg");
                var explorer = new Process
                {
                    StartInfo = {FileName = "explorer.exe", Arguments = AppDomain.CurrentDomain.BaseDirectory + "\\bg"}
                };
                explorer.Start();
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
            Logger.log(string.Format("BMCL V2 Ver.{0} 正在退出", BmclCore.BmclVersion));
            this.Close();
            if (!Logger.debug)
            {
                Application.Current.Shutdown(0);
            }
        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (BmclCore.GameRunning)
            {
                MessageBox.Show(this, "同时只能运行一个客户端", "运行冲突", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            BmclCore.GameRunning = true;
            if (GridConfig.txtUserName.Text == "!!!")
            {
                MessageBox.Show(this, "请先修改用户名");
                TabMain.SelectedIndex = 1;
                GridConfig.txtUserName.Focus();
                return;
            }
            _clientCrashReportCount = Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\crash-reports") ? Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\crash-reports").Count() : 0;
            _starter = new FrmPrs("正在准备游戏环境及启动游戏");
            Logger.info(string.Format("正在启动{0},使用的登陆方式为{1}", GridGame.listVer.SelectedItem, GridConfig.listAuth.SelectedItem));
            _starter.ShowInTaskbar = false;
            _starter.Show();
            _starter.Activate();
            _starter.Focus();
            _starter.ChangeEventH("正在登陆");
            var loginThread = new LoginThread(GridConfig.txtUserName.Text, GridConfig.txtPwd.Password, GridConfig.listAuth.SelectedItem.ToString(), GridConfig.listAuth.SelectedIndex);
            loginThread.LoginFinishEvent += LoginThreadOnLoginFinishEvent;
            loginThread.Start();
        }

        private void LoginThreadOnLoginFinishEvent(LoginInfo loginInfo)
        {
            if (loginInfo.Suc)
            {
                GridConfig.SaveConfig();
                var username = loginInfo.UN;
                try
                {
                    var javaPath = GridConfig.txtJavaPath.Text;
                    var javaXmx = GridConfig.txtJavaXmx.Text;
                    var selectVer = GridGame.listVer.SelectedItem.ToString();
                    var extArg = GridConfig.txtExtJArg.Text;
                    BmclCore.Game = new Launcher.Launcher(javaPath, javaXmx, username, selectVer, BmclCore.GameInfo, extArg, loginInfo);
                    BmclCore.Game.StateChangeEvent += Game_StateChangeEvent;
                    BmclCore.Game.Gameexit += launcher_gameexit;
                    BmclCore.Game.GameStartUp += Game_GameStartUp;
                }
                catch (Exception ex)
                {
                    _starter.Topmost = false;
                    _starter.Close();
                    MessageBox.Show("启动失败：" + ex.Message);
                    Logger.log(ex);
                    return;
                }
            }
            else
            {
                _starter.Topmost = false;
                MessageBox.Show("登录失败:" + loginInfo.Errinfo);
                Logger.log("登录失败" + loginInfo.Errinfo, Logger.LogType.Error);
                BmclCore.GameRunning = false;
            }
            if (BmclCore.Game == null)
            {
                _starter.Topmost = false;
                Logger.log("启动器初始化失败，放弃启动", Logger.LogType.Crash);
                BmclCore.GameRunning = false;
            }
            else
            {
                BmclCore.Game.Start();
                this.Hide();
            }
            
        }

        void Game_StateChangeEvent(string state)
        {
            _starter.ChangeEventH(state);
        }

        void Game_GameStartUp(bool success)
        {
            BmclCore.NIcon.NIcon.Visible = true;
            if (BmclCore.Game == null || !success)
            {
                BmclCore.NIcon.ShowBalloonTip(10000, "启动失败" + BmclCore.Config.LastPlayVer, System.Windows.Forms.ToolTipIcon.Error);
                BmclCore.GameRunning = false;
            }
            else
            {
                BmclCore.NIcon.ShowBalloonTip(10000, "已启动" + BmclCore.Config.LastPlayVer);
            }
            _starter.Close();
            _starter = null;
            _isLaunchering = false;
            if (BmclCore.GameInfo.assets != null)
            {
// ReSharper disable once ObjectCreationAsStatement
                new Assets.Assets(BmclCore.GameInfo);
            }
        }

        private void launcher_gameexit()
        {
            BmclCore.GameRunning = false;
            BmclCore.Game.Gameexit -= launcher_gameexit;
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\crash-reports"))
            {
                if (_clientCrashReportCount != Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\crash-reports").Count())
                {
                    Logger.log("发现新的错误报告");
                    var clientCrashReportDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\crash-reports");
                    var lastClientCrashReportPath = "";
                    var lastClientCrashReportModifyTime=DateTime.MinValue;
                    foreach (var clientCrashReport in clientCrashReportDir.GetFiles())
                    {
                        if (lastClientCrashReportModifyTime < clientCrashReport.LastWriteTime)
                        {
                            lastClientCrashReportPath = clientCrashReport.FullName;
                        }
                    }
                    var crashReportReader = new StreamReader(lastClientCrashReportPath);
                    Logger.log(crashReportReader.ReadToEnd(),Logger.LogType.Crash);
                    crashReportReader.Close();
                    if (MessageBox.Show("客户端好像崩溃了，是否查看崩溃报告？", "客户端崩溃", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        Process.Start(lastClientCrashReportPath);
                    }
                }
            }
            if (Logger.debug)
            {
                Logger.log("游戏退出，Debug模式保留Log信息窗口，程序不退出");
                Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(this.Show));
                return;
            }
            if (!_inscreen)
            {
                Logger.log("BMCL V2 Ver" + BmclCore.BmclVersion + DateTime.Now + "由于游戏退出而退出");
                Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(() => Application.Current.Shutdown(0)));
            }
        }

        private void btnMiniSize_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
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
            if (!LoadOk)
            {
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\assets"))
                    if (MessageBox.Show(LangManager.GetLangFromResource("ResourceTipForFirstLauncher"), LangManager.GetLangFromResource("ResourceTipTitleForFirstLauncher"), MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        var frmCheckRes = new FrmCheckRes();
                        frmCheckRes.Show();
                    }
            }
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
                    GridServer.BeginAnimation(FrameworkElement.WidthProperty, da1); GridServer.BeginAnimation(FrameworkElement.HeightProperty, da2); 
                    if(GridServer.btnReflushServer.IsEnabled && GridServer.listServer.HasItems == false) GridServer.ReflushSever();
                    break;
                case 5:
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


        private void LoadLanguage()
        {
            ResourceDictionary lang = LangManager.LoadLangFromResource("pack://application:,,,/Lang/zh-cn.xaml");
            BmclCore.Language.Add((string)lang["DisplayName"], lang["LangName"]);
            GridConfig.comboLang.Items.Add(lang["DisplayName"]);
            LangManager.Add(lang["LangName"] as string, "pack://application:,,,/Lang/zh-cn.xaml");

            lang = LangManager.LoadLangFromResource("pack://application:,,,/Lang/zh-tw.xaml");
            BmclCore.Language.Add((string)lang["DisplayName"], lang["LangName"]);
            GridConfig.comboLang.Items.Add(lang["DisplayName"]);
            LangManager.Add(lang["LangName"] as string, "pack://application:,,,/Lang/zh-tw.xaml");
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Lang"))
            {
                foreach (string langFile in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\Lang", "*.xaml", SearchOption.TopDirectoryOnly))
                {
                    lang = LangManager.LoadLangFromResource(langFile);
                    BmclCore.Language.Add((string)lang["DisplayName"], lang["LangName"]);
                    GridConfig.comboLang.Items.Add(lang["DisplayName"]);
                    LangManager.Add(lang["LangName"] as string, langFile);
                }
            }
            else
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Lang");
            }
        }

        public void ChangeLanguage()
        {
            GridConfig.listDownSource.Items[1] = LangManager.GetLangFromResource("listOfficalSource");
            GridConfig.listDownSource.Items[0] = LangManager.GetLangFromResource("listAuthorSource");
            BmclCore.LoadPlugin(LangManager.GetLangFromResource("LangName"));
        }

        private void MenuStartDebug_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(LangManager.GetLangFromResource("MenuDebugHint"));
            Logger.debug = true;
            btnStart_Click(null, null);
        }

        private void FrmMainWindow_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this._isLaunchering && _starter != null)
            {
                _starter.Activate();
            }
        }

        private void FrmMainWindow_Activated(object sender, EventArgs e)
        {
            if (this._isLaunchering && _starter != null)
            {
                _starter.Activate();
            }
        }



        




    }
}
