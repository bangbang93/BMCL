using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using BMCLV2.Forge;
using BMCLV2.Lang;
using BMCLV2.Login;
using BMCLV2.Mod;
using BMCLV2.util;
using BMCLV2.Versions;
using BMCLV2.Windows.MainWindowTab;

namespace BMCLV2.Windows
{
    /// <summary>
    /// FrmMain.xaml 的交互逻辑
    /// </summary>
    public partial class FrmMain
    {
        public static gameinfo Info;
        private bool _inscreen;
        private bool _isLaunchering;
        public bool Debug;
        
        private int _clientCrashReportCount;
        private FrmPrs _starter;

        public FrmMain()
        {
            InitializeComponent();
            BmclCore.NIcon.MainWindow = this;
            BmclCore.MainWindow = this;
            this.Title = "BMCL V2 Ver." + BmclCore.BmclVersion;
            this.LoadConfig();
            this.LoadLanguage();
            listAuth.Items.Add(LangManager.GetLangFromResource("NoneAuth"));
            foreach (var auth in BmclCore.Auths)
            {
                listAuth.Items.Add(auth.Key);
            }
            ReFlushlistver();
            listVer.SelectedItem = BmclCore.Config.LastPlayVer;
            listAuth.SelectedItem = BmclCore.Config.Login;
            checkCheckUpdate.IsChecked = BmclCore.Config.CheckUpdate;
        }

        private void LoadConfig()
        {
            txtUserName.Text = BmclCore.Config.Username;
            txtPwd.Password = Encoding.UTF8.GetString(BmclCore.Config.Passwd);
            txtJavaPath.Text = BmclCore.Config.Javaw;
            sliderJavaxmx.Maximum = Config.GetMemory();
            txtJavaXmx.Text = BmclCore.Config.Javaxmx;
            sliderJavaxmx.Value = int.Parse(BmclCore.Config.Javaxmx);
            txtExtJArg.Text = BmclCore.Config.ExtraJvmArg;
            checkAutoStart.IsChecked = BmclCore.Config.Autostart;
            if (listAuth.SelectedItem == null)
                listAuth.SelectedIndex = 0;
            sliderWindowTransparency.Value = BmclCore.Config.WindowTransparency;
            checkReport.IsChecked = BmclCore.Config.Report;
            txtInsPath.Text = AppDomain.CurrentDomain.BaseDirectory + ".minecraft";
            listDownSource.SelectedIndex = BmclCore.Config.DownloadSource;
            comboLang.SelectedItem = LangManager.GetLangFromResource("DisplayName");
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
                this.top.Background = b;
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
            }
            BmclCore.GameRunning = true;
            if (txtUserName.Text == "!!!")
            {
                MessageBox.Show(this, "请先修改用户名");
                tabMain.SelectedIndex = 1;
                txtUserName.Focus();
                return;
            }
            _clientCrashReportCount = Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\crash-reports") ? Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\crash-reports").Count() : 0;
            _starter = new FrmPrs("正在准备游戏环境及启动游戏");
            Logger.info(string.Format("正在启动{0},使用的登陆方式为{1}", listVer.SelectedItem, listAuth.SelectedItem));
            _starter.ShowInTaskbar = false;
            _starter.Show();
            _starter.Activate();
            _starter.Focus();
            _starter.ChangeEventH("正在登陆");
            var loginThread = new LoginThread(this.txtUserName.Text, this.txtPwd.Password, this.listAuth.SelectedItem.ToString(), this.listAuth.SelectedIndex);
            loginThread.LoginFinishEvent += LoginThreadOnLoginFinishEvent;
            loginThread.Start();
        }

        private void LoginThreadOnLoginFinishEvent(LoginInfo loginInfo)
        {
            if (loginInfo.Suc)
            {
                btnSaveConfig_Click(null, null);
                var username = loginInfo.UN;
                try
                {
                    var javaPath = txtJavaPath.Text;
                    var javaXmx = txtJavaXmx.Text;
                    var selectVer = listVer.SelectedItem.ToString();
                    var extArg = txtExtJArg.Text;
                    BmclCore.Game = new Launcher.Launcher(javaPath, javaXmx, username, selectVer, Info, extArg, loginInfo);
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
            if (Info.assets != null)
            {
// ReSharper disable once ObjectCreationAsStatement
                new Assets.Assets(Info);
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
            this.top.Background = b;
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
            this.top.Background = b;
            btnStart.Background = button;
            da = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25));
            this.BeginAnimation(UIElement.OpacityProperty, da);
        }
        #endregion


        #region tabVer
        private void listVer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listVer.SelectedIndex == -1)
            {
                listVer.SelectedIndex = 0;
                return;
            }
            this.listVer.ScrollIntoView(listVer.SelectedItem);
            string jsonFilePath = gameinfo.GetGameInfoJsonPath(listVer.SelectedItem.ToString());
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                MessageBox.Show(LangManager.GetLangFromResource("ErrorNoGameJson"));
                btnStart.IsEnabled = false;
                return;
            }
            btnStart.IsEnabled = true;
            Info = gameinfo.Read(jsonFilePath);
            BmclCore.GameInfo = Info;
            if (Info == null)
            {
                MessageBox.Show(LangManager.GetLangFromResource("ErrorJsonEncoding"));
                return;
            }
            labVer.Content = Info.id;
            labTime.Content = Info.time;
            labRelTime.Content = Info.releaseTime;
            labType.Content = Info.type;
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(LangManager.GetLangFromResource("DeleteMessageBoxInfo"), LangManager.GetLangFromResource("DeleteMessageBoxTitle"), MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                try
                {
                    FileStream isused = File.OpenWrite(".minecraft\\versions\\" + listVer.SelectedItem + "\\" + Info.id + ".jar");
                    isused.Close();
                    Directory.Delete(".minecraft\\versions\\" + listVer.SelectedItem, true);
                    if (Directory.Exists(".minecraft\\libraries\\" + listVer.SelectedItem))
                    {
                        Directory.Delete(".minecraft\\libraries\\" + listVer.SelectedItem, true);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(LangManager.GetLangFromResource("DeleteFailedMessageInfo"));
                }
                catch (IOException)
                {
                    MessageBox.Show(LangManager.GetLangFromResource("DeleteFailedMessageInfo"));
                }
                finally
                {
                    this.ReFlushlistver();
                }
            }
        }
        private void btnReName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string rname = Microsoft.VisualBasic.Interaction.InputBox(LangManager.GetLangFromResource("RenameNewName"), LangManager.GetLangFromResource("RenameTitle"), listVer.SelectedItem.ToString());
                if (rname == "") return;
                if (rname == listVer.SelectedItem.ToString()) return;
                if (listVer.Items.IndexOf(rname) != -1) throw new Exception(LangManager.GetLangFromResource("RenameFailedExist"));
                Directory.Move(".minecraft\\versions\\" + listVer.SelectedItem, ".minecraft\\versions\\" + rname);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(LangManager.GetLangFromResource("RenameFailedError"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.ReFlushlistver();
            }
        }
        private void btnModMrg_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments =
                    ModHelper.SetupModPath(listVer.SelectedItem.ToString()) + "mods"
            });
        }
        private void btnImportOldMc_Click(object sender, RoutedEventArgs e)
        {
            var folderImportOldVer = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = LangManager.GetLangFromResource("ImportDirInfo")
            };
            var prs = new FrmPrs(LangManager.GetLangFromResource("ImportPrsTitle"));
            prs.Show();
            if (folderImportOldVer.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string importFrom = folderImportOldVer.SelectedPath;
                if (!File.Exists(importFrom + "\\bin\\minecraft.jar"))
                {
                    MessageBox.Show(LangManager.GetLangFromResource("ImportFailedNoMinecraftFound"));
                    return;
                }
                bool f1, f2;
                string importName = Microsoft.VisualBasic.Interaction.InputBox(LangManager.GetLangFromResource("ImportNameInfo"), LangManager.GetLangFromResource("ImportOldMcInfo"), "OldMinecraft");
                do
                {
                    f1 = false;
                    f2 = false;
                    if (importName.Length <= 0 || importName.IndexOf('.') != -1)
                        importName = Microsoft.VisualBasic.Interaction.InputBox(LangManager.GetLangFromResource("ImportNameInfo"), LangManager.GetLangFromResource("ImportInvildName"), "OldMinecraft");
                    else
                        f1 = true;
                    if (Directory.Exists(".minecraft\\versions\\" + importName))
                        importName = Microsoft.VisualBasic.Interaction.InputBox(LangManager.GetLangFromResource("ImportNameInfo"), LangManager.GetLangFromResource("ImportFailedExist"), "OldMinecraft");
                    else
                        f2 = true;

                } while (!(f1 && f2));
                var thImport = new Thread(() => ImportOldMC(importName, importFrom, prs));
                thImport.Start();
            }
            else prs.Close();
        }
        private void btnCoreModMrg_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments =
                    ModHelper.SetupModPath(listVer.SelectedItem.ToString()) + "coremods"
            });
        }
        private void btnModdirMrg_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments =
                    ModHelper.SetupModPath(listVer.SelectedItem.ToString()) + "moddir"
            });
        }
        private void btnModCfgMrg_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments =
                    ModHelper.SetupModPath(listVer.SelectedItem.ToString()) + "configs"
            });
        }
        private void listVer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (btnStart.IsEnabled)
                btnStart_Click(null, null);
        }
        private void btnLibraries_Click(object sender, RoutedEventArgs e)
        {
            var f = new FrmLibraries(Info.libraries);
            if (f.ShowDialog() == true)
            {
                Info.libraries = f.GetChange();
                string jsonFile = gameinfo.GetGameInfoJsonPath(listVer.SelectedItem.ToString());
                File.Delete(jsonFile + ".bak");
                File.Move(jsonFile, jsonFile + ".bak");
                gameinfo.Write(Info, jsonFile);
                this.listVer_SelectionChanged(null, null);
            }
        }
        #endregion


        #region tabLauncherCfg
        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            BmclCore.Config.Autostart = checkAutoStart.IsChecked != null && (bool) checkAutoStart.IsChecked;
            BmclCore.Config.ExtraJvmArg = txtExtJArg.Text;
            BmclCore.Config.Javaw = txtJavaPath.Text;
            BmclCore.Config.Javaxmx = txtJavaXmx.Text;
            BmclCore.Config.Login = listAuth.SelectedItem.ToString();
            BmclCore.Config.LastPlayVer = listVer.SelectedItem as string;
            BmclCore.Config.Passwd = Encoding.UTF8.GetBytes(txtPwd.Password);
            BmclCore.Config.Username = txtUserName.Text;
            BmclCore.Config.WindowTransparency = sliderWindowTransparency.Value;
            BmclCore.Config.Report = checkReport.IsChecked != null && checkReport.IsChecked.Value;
            BmclCore.Config.DownloadSource = listDownSource.SelectedIndex;
            BmclCore.Config.Lang = LangManager.GetLangFromResource("LangName");
            Config.Save();
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
            if (top.Background != null)
                top.Background.Opacity = e.NewValue;
        }

        private void listDownSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (listDownSource.SelectedIndex)
            {
                case 0:
                    BmclCore.UrlDownloadBase = Resource.Url.URL_DOWNLOAD_BASE;
                    BmclCore.UrlResourceBase = Resource.Url.URL_RESOURCE_BASE;
                    BmclCore.UrlLibrariesBase = Resource.Url.URL_LIBRARIES_BASE;
                    break;
                case 1:
                    BmclCore.UrlDownloadBase = Resource.Url.URL_DOWNLOAD_bangbang93;
                    BmclCore.UrlResourceBase = Resource.Url.URL_RESOURCE_bangbang93;
                    BmclCore.UrlLibrariesBase = Resource.Url.URL_LIBRARIES_bangbang93;
                    break;
                default:
                    goto case 0;
            }
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
        #endregion


        #region tabRemoteVer
        private void btnRefreshRemoteVer_Click(object sender, RoutedEventArgs e)
        {
            if (btnReflushServer.Content.ToString() == LangManager.GetLangFromResource("RemoteVerGetting"))
                return;
            listRemoteVer.DataContext = null;
            var rawJson = new DataContractJsonSerializer(typeof(RawVersionListType));
            var getJson = (HttpWebRequest)WebRequest.Create(BmclCore.UrlDownloadBase + "versions/versions.json");
            getJson.Timeout = 10000;
            getJson.ReadWriteTimeout = 10000;
            var thGet = new Thread(new ThreadStart(delegate
            {
                try
                {
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { btnRefreshRemoteVer.Content = LangManager.GetLangFromResource("RemoteVerGetting"); btnRefreshRemoteVer.IsEnabled = false; }));
                    var getJsonAns = (HttpWebResponse)getJson.GetResponse();
// ReSharper disable once AssignNullToNotNullAttribute
                    var remoteVersion = rawJson.ReadObject(getJsonAns.GetResponseStream()) as RawVersionListType;
                    var dt = new DataTable();
                    dt.Columns.Add("Ver");
                    dt.Columns.Add("RelTime");
                    dt.Columns.Add("Type");
                    if (remoteVersion != null)
                        foreach (RemoteVerType rv in remoteVersion.getVersions())
                        {
                            dt.Rows.Add(new object[] { rv.id, rv.releaseTime, rv.type });
                        }
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                    {
                        btnRefreshRemoteVer.Content = LangManager.GetLangFromResource("btnRefreshRemoteVer");
                        btnRefreshRemoteVer.IsEnabled = true;
                        listRemoteVer.DataContext = dt;
                        listRemoteVer.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("RelTime", System.ComponentModel.ListSortDirection.Descending));
                    }));
                }
                catch (WebException ex)
                {
                    MessageBox.Show(LangManager.GetLangFromResource("RemoteVerFailedTimeout")+"\n"+ex.Message);
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                    {
                        btnRefreshRemoteVer.Content = LangManager.GetLangFromResource("btnRefreshRemoteVer");
                        btnRefreshRemoteVer.IsEnabled = true;
                    }));
                }
                catch (TimeoutException ex)
                {
                    MessageBox.Show(LangManager.GetLangFromResource("RemoteVerFailedTimeout")+"\n"+ex.Message);
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                    {
                        btnRefreshRemoteVer.Content = LangManager.GetLangFromResource("btnRefreshRemoteVer");
                        btnRefreshRemoteVer.IsEnabled = true;
                    }));
                }
            }));
            thGet.Start();
        }
        private void btnDownloadVer_Click(object sender, RoutedEventArgs e)
        {
            if (listRemoteVer.SelectedItem == null)
            {
                MessageBox.Show(LangManager.GetLangFromResource("RemoteVerErrorNoVersionSelect"));
                return;
            }
            var selectVer = listRemoteVer.SelectedItem as DataRowView;
            if (selectVer != null)
            {
                var selectver = selectVer[0] as string;
                var downpath = new StringBuilder(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\versions\");
                downpath.Append(selectver).Append("\\");
                downpath.Append(selectver).Append(".jar");
                var downer = new WebClient();
                var downurl = new StringBuilder(BmclCore.UrlDownloadBase);
                downurl.Append(@"versions\");
                downurl.Append(selectver).Append("\\");
                downurl.Append(selectver).Append(".jar");
#if DEBUG
            MessageBox.Show(downpath.ToString()+"\n"+downurl.ToString());
#endif
                btnDownloadVer.Content = LangManager.GetLangFromResource("RemoteVerDownloading");
                btnDownloadVer.IsEnabled = false;
// ReSharper disable once AssignNullToNotNullAttribute
                if (!Directory.Exists(Path.GetDirectoryName(downpath.ToString())))
                {
// ReSharper disable once AssignNullToNotNullAttribute
                    Directory.CreateDirectory(Path.GetDirectoryName(downpath.ToString()));
                }
                string downjsonfile = downurl.ToString().Substring(0, downurl.Length - 4) + ".json";
                string downjsonpath = downpath.ToString().Substring(0, downpath.Length - 4) + ".json";
                try
                {
                    downer.DownloadFileCompleted += downer_DownloadClientFileCompleted;
                    downer.DownloadProgressChanged += downer_DownloadProgressChanged;
                    Logger.log("下载:" + downjsonfile);
                    downer.DownloadFile(new Uri(downjsonfile), downjsonpath);
                    Logger.log("下载:" + downurl);
                    downer.DownloadFileAsync(new Uri(downurl.ToString()), downpath.ToString());
                    _downedtime = Environment.TickCount - 1;
                    _downed = 0;
                    gridDown.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message+"\n");
                    btnDownloadVer.Content = LangManager.GetLangFromResource("btnDownloadVer");
                    btnDownloadVer.IsEnabled = true;
                }
            }
        }
        int _downedtime;
        int _downed;
        void downer_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            prsDown.Maximum = (int)e.TotalBytesToReceive;
            prsDown.Value = (int)e.BytesReceived;
            //            TaskbarManager.Instance.SetProgressValue((int)e.BytesReceived, (int)e.TotalBytesToReceive);
            StringBuilder info = new StringBuilder(LangManager.GetLangFromResource("DownloadSpeedInfo"));
            try
            {
                info.Append(((double)(e.BytesReceived - _downed) / (double)((Environment.TickCount - _downedtime) / 1000) / 1024.0).ToString("F2")).Append("KB/s,");
            }
            catch (DivideByZeroException) { info.Append("0B/s,"); }
            info.Append(e.ProgressPercentage.ToString(CultureInfo.InvariantCulture)).Append("%");
            labDownInfo.Content = info.ToString();
        }

        void downer_DownloadClientFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Logger.log("下载客户端文件成功", Logger.LogType.Info);
            MessageBox.Show(LangManager.GetLangFromResource("RemoteVerDownloadSuccess"));
            btnDownloadVer.Content = LangManager.GetLangFromResource("btnDownloadVer");
            btnDownloadVer.IsEnabled = true;
            ReFlushlistver();
            //            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            gridDown.Visibility = Visibility.Hidden;
            tabMain.SelectedIndex = 0;
        }
        private void btnCheckRes_Click(object sender, RoutedEventArgs e)
        {
            var checkres = new FrmCheckRes();
            checkres.Show();
        }
        private void listRemoteVer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnDownloadVer_Click(null, null);
        }

        #endregion


        #region tabForge

        readonly ForgeVersionList _forgeVer = new ForgeVersionList();
        private void RefreshForgeVersionList()
        {
            treeForgeVer.Items.Add(LangManager.GetLangFromResource("ForgeListGetting"));
            _forgeVer.ForgePageReadyEvent += ForgeVer_ForgePageReadyEvent;
            _forgeVer.GetVersion();
        }

        void ForgeVer_ForgePageReadyEvent()
        {
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(() =>
            {
                treeForgeVer.Items.Clear();
                foreach (TreeViewItem t in _forgeVer.GetNew())
                {
                    treeForgeVer.Items.Add(t);
                }
                foreach (TreeViewItem t in _forgeVer.GetLegacy())
                {
                    treeForgeVer.Items.Add(t);
                }
                btnReForge.Content = LangManager.GetLangFromResource("btnReForge");
                btnReForge.IsEnabled = true;
                btnLastForge.IsEnabled = true;
            }));
        }
        private void btnLastForge_Click(object sender, RoutedEventArgs e)
        {
            DownloadForge("Latest");
        }
        private void btnReForge_Click(object sender, RoutedEventArgs e)
        {
            if (btnReForge.Content.ToString() == LangManager.GetLangFromResource("btnReForgeGetting"))
                return;
            btnReForge.Content = LangManager.GetLangFromResource("btnReForgeGetting");
            btnReForge.IsEnabled = false;
            btnLastForge.IsEnabled = false;
            RefreshForgeVersionList();
        }
        private void DownloadForge(string ver)
        {
            if (!_forgeVer.ForgeDownloadUrl.ContainsKey(ver))
            {
                MessageBox.Show(LangManager.GetLangFromResource("ForgeDoNotSupportInstaller"));
                return;
            }
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { gridDown.Visibility = Visibility.Visible; }));
            var url = new Uri(_forgeVer.ForgeDownloadUrl[ver]);
            var downer = new WebClient();
            downer.DownloadProgressChanged+=downer_DownloadProgressChanged;
            downer.DownloadFileCompleted += downer_DownloadForgeCompleted;
            _downedtime = Environment.TickCount - 1;
            _downed = 0;
            var w = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\.minecraft\\launcher_profiles.json");
            w.Write(Resource.NormalProfile.Profile);
            w.Close();
            downer.DownloadFileAsync(url, "forge.jar");
        }

        void downer_DownloadForgeCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtInsPath.Text);
                MessageBox.Show(LangManager.GetLangFromResource("ForgeInstallInfo"));
            }
            catch
            {
                MessageBox.Show(LangManager.GetLangFromResource("ForgeCopyError"));
            }
            var forgeIns = new Process();
            if (!File.Exists(BmclCore.Config.Javaw))
            {
                MessageBox.Show(LangManager.GetLangFromResource("ForgeJavaError"));
                return;
            }
            forgeIns.StartInfo.FileName = BmclCore.Config.Javaw;
            forgeIns.StartInfo.Arguments = "-jar \"" + AppDomain.CurrentDomain.BaseDirectory + "\\forge.jar\"";
            Logger.log(forgeIns.StartInfo.Arguments);
            forgeIns.Start();
            forgeIns.WaitForExit();
            ReFlushlistver();
            tabMain.SelectedIndex = 0;
            gridDown.Visibility = Visibility.Hidden;
        }
        private void treeForgeVer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.treeForgeVer.SelectedItem == null)
                return;
            if (this.treeForgeVer.SelectedItem is string)
            {
                DownloadForge(this.treeForgeVer.SelectedItem as string);
            }
        }
        private void txtInsPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtInsPath.Text);
                MessageBox.Show(LangManager.GetLangFromResource("ForgeCopySuccess"));
            }
            catch
            {
                MessageBox.Show(LangManager.GetLangFromResource("ForgeCopyError"));
            }
        }

        private void treeForgeVer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.treeForgeVer.SelectedItem == null)
                return;
            if (this.treeForgeVer.SelectedItem is string)
            {
                if (!_forgeVer.ForgeChangeLogUrl.ContainsKey(this.treeForgeVer.SelectedItem as string))
                    if(_forgeVer.ForgeChangeLogUrl[this.treeForgeVer.SelectedItem as string] !=null)
                    {
                        MessageBox.Show(LangManager.GetLangFromResource("ForgeDoNotHaveChangeLog"));
                        return;
                    }
                txtChangeLog.Text = LangManager.GetLangFromResource("FetchingForgeChangeLog");
                var getLog = new WebClient();
                getLog.DownloadStringCompleted += GetLog_DownloadStringCompleted;
                getLog.DownloadStringAsync(new Uri(_forgeVer.ForgeChangeLogUrl[this.treeForgeVer.SelectedItem as string]));
            }
        }

        void GetLog_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            txtChangeLog.Text = e.Result;
        }
        #endregion


        #region tabServerList

        readonly DataTable _serverListDataTable = new DataTable();
        private serverlist.serverlist _sl;
        private void btnReflushServer_Click(object sender, RoutedEventArgs e)
        {
            _serverListDataTable.Clear();
            _serverListDataTable.Columns.Clear();
            _serverListDataTable.Rows.Clear();
            _serverListDataTable.Columns.Add("ServerName");
            _serverListDataTable.Columns.Add("HiddenAddress");
            _serverListDataTable.Columns.Add("ServerAddress");
            _serverListDataTable.Columns.Add("ServerMotd");
            _serverListDataTable.Columns.Add("ServerVer");
            _serverListDataTable.Columns.Add("ServerOnline");
            _serverListDataTable.Columns.Add("ServerDelay");
            this.listServer.DataContext = _serverListDataTable;
            this.btnReflushServer.IsEnabled = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(GetServerInfo));
        }

        private void GetServerInfo(object obj)
        {
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { btnReflushServer.Content = LangManager.GetLangFromResource("ServerListGetting"); }));
            if (File.Exists(@".minecraft\servers.dat"))
            {
                _sl = new serverlist.serverlist();
                foreach (serverlist.serverinfo info in _sl.info)
                {
                    DateTime start = DateTime.Now;
                    string[] server = new string[7];
                    server[0] = info.Name;
                    server[1] = info.IsHide ? LangManager.GetLangFromResource("ServerListYes") : LangManager.GetLangFromResource("ServerListNo");
                    if (info.IsHide)
                        server[2] = string.Empty;
                    else
                        server[2] = info.Address;
                    try
                    {
                        Socket con = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        con.ReceiveTimeout = 3000;
                        con.SendTimeout = 3000;
                        if (info.Address.Split(':').Length == 1)
                            con.Connect(Dns.GetHostAddresses(info.Address.Split(':')[0]), 25565);
                        else
                            con.Connect(Dns.GetHostAddresses(info.Address.Split(':')[0]), int.Parse(info.Address.Split(':')[1]));
                        con.Send(new byte[1] { 254 });
                        con.Send(new byte[1] { 1 });
                        byte[] recive = new byte[512];
                        int bytes = con.Receive(recive);
                        if (recive[0] != 255)
                        {
                            throw new Exception(LangManager.GetLangFromResource("ServerListInvildReply"));
                        }
                        string message = Encoding.UTF8.GetString(recive, 4, bytes - 4);
                        StringBuilder remessage = new StringBuilder(30);
                        for (int i = 0; i <= message.Length; i += 2)
                        {
                            remessage.Append(message[i]);
                        }
                        message = remessage.ToString();
                        con.Shutdown(SocketShutdown.Both);
                        con.Close();
                        DateTime end = DateTime.Now;
                        char[] achar = message.ToCharArray();

                        for (int i = 0; i < achar.Length; ++i)
                        {
                            if (achar[i] != 167 && achar[i] != 0 && char.IsControl(achar[i]))
                            {
                                achar[i] = (char)63;
                            }
                        }
                        message = new String(achar);
                        if (message[0] == (char)253 || message[0] == (char)65533)
                        {
                            message = (char)167 + message.Substring(1);
                        }
                        string[] astring;
                        if (message.StartsWith("\u00a7") && message.Length > 1)
                        {
                            astring = message.Substring(1).Split('\0');
                            if (MathHelper.parseIntWithDefault(astring[0], 0) == 1)
                            {
                                server[3] = astring[3];
                                server[4] = astring[2];
                                int online = MathHelper.parseIntWithDefault(astring[4], 0);
                                int maxplayer = MathHelper.parseIntWithDefault(astring[5], 0);
                                server[5] = online + "/" + maxplayer;
                            }
                        }
                        else
                        {
                            server[3] = " ";
                            server[4] = " ";
                            server[5] = " ";
                        }
                        server[6] = (end - start).Milliseconds.ToString() + " ms";
                    }
                    catch (SocketException ex)
                    {
                        server[3] = " ";
                        server[4] = " ";
                        server[5] = " ";
                        server[6] = LangManager.GetLangFromResource("ServerListSocketException") + ex.Message;
                        //server.SubItems[0].ForeColor = Color.Red;
                    }
                    catch (Exception ex)
                    {
                        server[3] = " ";
                        server[4] = " ";
                        server[5] = " ";
                        server[6] = LangManager.GetLangFromResource("ServerListUnknowServer") + ex.Message;
                        //server.SubItems[0].ForeColor = Color.Red;
                    }
                    finally
                    {
                        StringBuilder logger = new StringBuilder();
                        foreach (string str in server)
                        {
                            logger.Append(str + ",");
                        }
                        Logger.log(logger.ToString());
                        lock (_serverListDataTable)
                        {
                            _serverListDataTable.Rows.Add(server);
                            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                            {
                                listServer.DataContext = null;
                                listServer.DataContext = _serverListDataTable;
                            }));
                        }
                    }
                }
                Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { btnReflushServer.Content = LangManager.GetLangFromResource("btnReflushServer"); btnReflushServer.IsEnabled = true; }));
            }
            else
            {
                if (MessageBox.Show(LangManager.GetLangFromResource("ServerListNotFound"), "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    if (!Directory.Exists(".minecraft"))
                    {
                        Directory.CreateDirectory(".minecraft");
                    }
                    FileStream serverdat = new FileStream(@".minecraft\servers.dat", FileMode.Create);
                    serverdat.Write(Convert.FromBase64String(Resource.ServerDat.Header), 0, Convert.FromBase64String(Resource.ServerDat.Header).Length);
                    serverdat.WriteByte(0);
                    serverdat.Close();
                    _sl = new serverlist.serverlist();
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                    {
                        btnAddServer.IsEnabled = true;
                        btnDeleteServer.IsEnabled = true;
                        btnEditServer.IsEnabled = true;
                        btnReflushServer.IsEnabled = true;
                    }));
                }
                else
                {
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                    {
                        btnAddServer.IsEnabled = false;
                        btnDeleteServer.IsEnabled = false;
                        btnEditServer.IsEnabled = false;
                        btnReflushServer.IsEnabled = false;
                    }));
                }
            }
        }

        private void btnAddServer_Click(object sender, RoutedEventArgs e)
        {
            serverlist.AddServer FrmAdd = new serverlist.AddServer(ref _sl);
            if (FrmAdd.ShowDialog() == true)
            {
                _sl.Write();
                btnReflushServer_Click(null, null);
            }
        }

        private void btnDeleteServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _sl.Delete(listServer.SelectedIndex);
                _sl.Write();
                btnReflushServer_Click(null, null);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show(LangManager.GetLangFromResource("ServerListNoServerSelect"));
            }
        }

        private void btnEditServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int selected = this.listServer.SelectedIndex;
                serverlist.AddServer FrmEdit = new serverlist.AddServer(ref _sl, selected);
                if (FrmEdit.ShowDialog() == true)
                {
                    serverlist.serverinfo info = FrmEdit.getEdit();
                    _sl.Edit(selected, info.Name, info.Address, info.IsHide);
                    _sl.Write();
                    btnReflushServer_Click(null, null);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show(LangManager.GetLangFromResource("ServerListNoServerSelect"));
            }
        }


        #endregion 


        private void ReFlushlistver()
        {
            listVer.Items.Clear();

                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\.minecraft"))
                {
                    {
                        MessageBox.Show(LangManager.GetLangFromResource("NoClientFound"));
                        btnStart.IsEnabled = false;
                        btnDelete.IsEnabled = false;
                        btnModCfgMrg.IsEnabled = false;
                        btnModdirMrg.IsEnabled = false;
                        btnModMrg.IsEnabled = false;
                        btnCoreModMrg.IsEnabled = false;
                        return;
                    }
                }
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\versions\"))
                {
                    MessageBox.Show(LangManager.GetLangFromResource("InvidMinecratDir"));
                    btnStart.IsEnabled      = false;
                    btnDelete.IsEnabled     = false;
                    btnModCfgMrg.IsEnabled  = false;
                    btnModdirMrg.IsEnabled  = false;
                    btnModMrg.IsEnabled     = false;
                    btnCoreModMrg.IsEnabled = false;
                    return;
                }
                DirectoryInfo mcdirinfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\.minecraft");
                DirectoryInfo[] versions = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\versions").GetDirectories();
                foreach (DirectoryInfo version in versions)
                {
                    listVer.Items.Add(version.Name);
                }
                if (listVer.Items.Count != 0)
                {
                    listVer.SelectedIndex = 0;
                    btnStart.IsEnabled      = true;
                    btnDelete.IsEnabled     = true;
                    btnModCfgMrg.IsEnabled  = true;
                    btnModdirMrg.IsEnabled  = true;
                    btnModMrg.IsEnabled     = true;
                    btnCoreModMrg.IsEnabled = true;
                }
                else
                {
                    btnStart.IsEnabled = false;
                    btnDelete.IsEnabled = false;
                    btnModCfgMrg.IsEnabled = false;
                    btnModdirMrg.IsEnabled = false;
                    btnModMrg.IsEnabled = false;
                    btnCoreModMrg.IsEnabled = false;
                    return;
                }
        }


        public bool loadOk = false;
        private void FrmMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!loadOk)
            {
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\assets"))
                    if (MessageBox.Show(LangManager.GetLangFromResource("ResourceTipForFirstLauncher"), LangManager.GetLangFromResource("ResourceTipTitleForFirstLauncher"), MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        FrmCheckRes frmCheckRes = new FrmCheckRes();
                        frmCheckRes.Show();
                    }
            }
            if (BmclCore.Config.Username == "!!!")
            {
                tabMain.SelectedIndex = 1;
                tip.IsOpen = true;
                txtUserName.Focus();
            }
            else
            {
                if (BmclCore.Config.Autostart == true)
                {
                    btnStart_Click(null, null);
                    loadOk = true;
                    this.Hide();
                    return;
                }
            }
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 1;
            da.Duration = TimeSpan.FromSeconds(0.8);
            this.FrmMainWindow.BeginAnimation(Window.OpacityProperty, da);
            try
            {
                Random rand = new Random();
                int img = rand.Next(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\bg").Length);
                ImageBrush b = new ImageBrush();
                b.ImageSource = new BitmapImage(new Uri((Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\bg")[img])));
                b.Stretch = Stretch.Fill;
                this.top.Background = b;
            }
            catch
            {
                SolidColorBrush b=new SolidColorBrush(Color.FromRgb(255,255,255));
                this.top.Background = b;
            }
            loadOk = true;
        }
        private void FrmMainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            }
            catch { }
        }
        int LasttabMainSelectIndex = -1;
        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LasttabMainSelectIndex == tabMain.SelectedIndex)
                return;
            LasttabMainSelectIndex = tabMain.SelectedIndex;
            if (!loadOk)
                return;
            DoubleAnimation da1 = new DoubleAnimation(0, tabMain.ActualWidth, new Duration(new TimeSpan(0, 0, 0, 0, 100)));
            DoubleAnimation da2 = new DoubleAnimation(0, tabMain.ActualHeight, new Duration(new TimeSpan(0, 0, 0, 0, 100)));
            switch (tabMain.SelectedIndex)
            {
                case 0: gridGame.BeginAnimation(Grid.WidthProperty, da1); gridGame.BeginAnimation(Grid.HeightProperty, da2); break;
                case 1: gridLaunchCfg.BeginAnimation(Grid.WidthProperty, da1); gridLaunchCfg.BeginAnimation(Grid.HeightProperty, da2); break;
                case 2: gridRemoteVer.BeginAnimation(Grid.WidthProperty, da1); gridRemoteVer.BeginAnimation(Grid.HeightProperty, da2); if (btnRefreshRemoteVer.IsEnabled && listRemoteVer.HasItems == false) btnRefreshRemoteVer_Click(null, null); break;
                case 3: gridForge.BeginAnimation(Grid.WidthProperty, da1); gridForge.BeginAnimation(Grid.HeightProperty, da2); if (btnReForge.IsEnabled && treeForgeVer.HasItems == false) btnReForge_Click(null, null); break;
                case 4: gridServerList.BeginAnimation(Grid.WidthProperty, da1); gridServerList.BeginAnimation(Grid.HeightProperty, da2); if(btnReflushServer.IsEnabled && listServer.HasItems == false) btnReflushServer_Click(null, null); break;
                case 5:
                    gridUpdateInfo.BeginAnimation(Grid.WidthProperty, da1); 
                    gridUpdateInfo.BeginAnimation(Grid.HeightProperty, da2); 
                    break;
            }
        }

        private void FrmMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BmclCore.NIcon.Hide();
        }

        private void FrmMainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible == true)
            {
                _inscreen = true;
                btnChangeBg_Click(null, null);
            }
            else
                _inscreen = false;
            
        }


        private void ImportOldMC(string ImportName,string ImportFrom,FrmPrs prs)
        {
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { prs.ChangeEventH(LangManager.GetLangFromResource("ImportMain")); }));
            Directory.CreateDirectory(".minecraft\\versions\\" + ImportName);
            File.Copy(ImportFrom + "\\bin\\minecraft.jar", ".minecraft\\versions\\" + ImportName + "\\" + ImportName + ".jar");
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { prs.ChangeEventH(LangManager.GetLangFromResource("ImportCreateJson")); }));
            gameinfo info = new gameinfo();
            info.id = ImportName;
            string timezone = DateTimeOffset.Now.Offset.ToString();
            if (timezone[0] != '-')
            {
                timezone = "+" + timezone;
            }
            info.time = DateTime.Now.GetDateTimeFormats('s')[0].ToString() + timezone;
            info.releaseTime = DateTime.Now.GetDateTimeFormats('s')[0].ToString() + timezone;
            info.type = "Port By BMCL";
            info.minecraftArguments = "${auth_player_name}";
            info.mainClass = "net.minecraft.client.Minecraft";
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { prs.ChangeEventH(LangManager.GetLangFromResource("ImportSolveNative")); }));
            ArrayList libs = new ArrayList();
            DirectoryInfo bin = new DirectoryInfo(ImportFrom + "\\bin");
            foreach (FileInfo file in bin.GetFiles("*.jar"))
            {
                libraries.libraryies libfile = new libraries.libraryies();
                if (file.Name == "minecraft.jar")
                    continue;
                if (!Directory.Exists(".minecraft\\libraries\\" + ImportName + "\\" + file.Name.Substring(0, file.Name.Length - 4) + "\\BMCL\\"))
                {
                    Directory.CreateDirectory(".minecraft\\libraries\\" + ImportName + "\\" + file.Name.Substring(0, file.Name.Length - 4) + "\\BMCL\\");
                }
                File.Copy(file.FullName, ".minecraft\\libraries\\" + ImportName + "\\" + file.Name.Substring(0, file.Name.Length - 4) + "\\BMCL\\" + file.Name.Substring(0, file.Name.Length - 4) + "-BMCL.jar");
                libfile.name = ImportName + ":" + file.Name.Substring(0, file.Name.Length - 4) + ":BMCL";
                libs.Add(libfile);
            }
            ICSharpCode.SharpZipLib.Zip.FastZip nativejar = new ICSharpCode.SharpZipLib.Zip.FastZip();
            if (!Directory.Exists(".minecraft\\libraries\\" + ImportName + "\\BMCL\\"))
            {
                Directory.CreateDirectory(".minecraft\\libraries\\" + ImportName + "\\native\\BMCL\\");
            }
            nativejar.CreateZip(".minecraft\\libraries\\" + ImportName + "\\native\\BMCL\\native-BMCL-natives-windows.jar", ImportFrom + "\\bin\\natives", false, @"\.dll");
            libraries.libraryies nativefile = new libraries.libraryies();
            nativefile.name = ImportName + ":native:BMCL";
            libraries.OS nativeos = new libraries.OS();
            nativeos.windows = "natives-windows";
            nativefile.natives = nativeos;
            nativefile.extract = new libraries.extract();
            libs.Add(nativefile);
            info.libraries = (libraries.libraryies[])libs.ToArray(typeof(libraries.libraryies));
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { prs.ChangeEventH(LangManager.GetLangFromResource("ImportWriteJson")); }));
            FileStream wcfg = new FileStream(".minecraft\\versions\\" + ImportName + "\\" + ImportName + ".json", FileMode.Create);
            DataContractJsonSerializer infojson = new DataContractJsonSerializer(typeof(gameinfo));
            infojson.WriteObject(wcfg, info);
            wcfg.Close();
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { prs.ChangeEventH(LangManager.GetLangFromResource("ImportSolveLib")); }));
            if (Directory.Exists(ImportFrom + "\\lib"))
            {
                if (!Directory.Exists(".minecraft\\lib"))
                {
                    Directory.CreateDirectory(".minecraft\\lib");
                }
                foreach (string libfile in Directory.GetFiles(ImportFrom + "\\lib", "*", SearchOption.AllDirectories))
                {
                    if (!File.Exists(".minecraft\\lib\\" + System.IO.Path.GetFileName(libfile)))
                    {
                        File.Copy(libfile, ".minecraft\\lib\\" + System.IO.Path.GetFileName(libfile));
                    }
                }
            }
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { prs.ChangeEventH(LangManager.GetLangFromResource("ImportSolveMod")); }));
            if (Directory.Exists(ImportFrom + "\\mods"))
                util.FileHelper.dircopy(ImportFrom + "\\mods", ".minecraft\\versions\\" + ImportName + "\\mods");
            else
                Directory.CreateDirectory(".minecraft\\versions\\" + ImportName + "\\mods");
            if (Directory.Exists(ImportFrom + "\\coremods"))
                util.FileHelper.dircopy(ImportFrom + "\\coremods", ".minecraft\\versions\\" + ImportName + "\\coremods");
            else
                Directory.CreateDirectory(".minecraft\\versions\\" + ImportName + "\\coremods");
            if (Directory.Exists(ImportFrom + "\\config"))
                util.FileHelper.dircopy(ImportFrom + "\\config", ".minecraft\\versions\\" + ImportName + "\\config");
            else
                Directory.CreateDirectory(".minecraft\\versions\\" + ImportName + "\\configmods");
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
            {
                prs.Close();
                MessageBox.Show(LangManager.GetLangFromResource("ImportOldMCInfo"));
                this.ReFlushlistver();
            }));
        }



        private void comboLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!loadOk)
                return;
            switch (comboLang.SelectedIndex)
            {
                case 0:
                    LangManager.UseLanguage("zh-cn");break;
                case 1:
                    LangManager.UseLanguage("zh-tw");break;
                default:
                    if (comboLang.SelectedItem as string != null)
                    if (BmclCore.Language.ContainsKey(comboLang.SelectedItem as string))
                        LangManager.UseLanguage(BmclCore.Language[comboLang.SelectedItem as string]as string);
                    break;
            }
            changeLanguage();
        }

        private void LoadLanguage()
        {
            ResourceDictionary Lang;
            Lang = LangManager.LoadLangFromResource("pack://application:,,,/Lang/zh-cn.xaml");
            BmclCore.Language.Add((string)Lang["DisplayName"], Lang["LangName"]);
            comboLang.Items.Add(Lang["DisplayName"]);
            LangManager.Add(Lang["LangName"] as string, "pack://application:,,,/Lang/zh-cn.xaml");

            Lang = LangManager.LoadLangFromResource("pack://application:,,,/Lang/zh-tw.xaml");
            BmclCore.Language.Add((string)Lang["DisplayName"], Lang["LangName"]);
            comboLang.Items.Add(Lang["DisplayName"]);
            LangManager.Add(Lang["LangName"] as string, "pack://application:,,,/Lang/zh-tw.xaml");
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Lang"))
            {
                foreach (string LangFile in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\Lang", "*.xaml", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        Lang = LangManager.LoadLangFromResource(LangFile);
                        BmclCore.Language.Add((string)Lang["DisplayName"], Lang["LangName"]);
                        comboLang.Items.Add(Lang["DisplayName"]);
                        LangManager.Add(Lang["LangName"] as string, LangFile);
                    }
                    catch { }
                }
            }
            else
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Lang");
            }
        }

        public void changeLanguage()
        {
            listDownSource.Items[0] = LangManager.GetLangFromResource("listOfficalSource");
            listDownSource.Items[1] = LangManager.GetLangFromResource("listAuthorSource");
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
