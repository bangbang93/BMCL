using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows.Media.Animation;
using System.Reflection;
using System.Diagnostics;
using System.Net;
using System.Data;
using System.Threading;
using System.Net.Sockets;


using BMCLV2.Versions;
using BMCLV2.util;
using BMCLV2.Lang;
using BMCLV2.Forge;

using HtmlAgilityPack;

namespace BMCLV2
{
    /// <summary>
    /// FrmMain.xaml 的交互逻辑
    /// </summary>
    public partial class FrmMain : Window
    {
        static private string cfgfile = "bmcl.xml";
        public static String URL_DOWNLOAD_BASE = BMCLV2.Resource.Url.URL_DOWNLOAD_BASE;
        public static String URL_RESOURCE_BASE = BMCLV2.Resource.Url.URL_RESOURCE_BASE;
        public static string URL_LIBRARIES_BASE = BMCLV2.Resource.Url.URL_LIBRARIES_BASE;
        private Dictionary<object, object> Auths = new Dictionary<object, object>();
        public static config cfg;
        static public gameinfo info;
        launcher game;
        bool inscreen;
        bool IsLaunchering;
        static public ListBox AuthList;
        static public string portinfo = "Port By BMCL";
        static public bool debug;
        public static System.Windows.Forms.NotifyIcon NIcon;
        public static string ver;
        private int ClientCrashReportCount;
        private FrmPrs starter;

        public FrmMain()
        {
            ver = Application.ResourceAssembly.FullName.Split('=')[1];
            ver = ver.Substring(0, ver.IndexOf(','));
            Logger.Log("BMCL V2 Ver." + ver + "正在启动");
            InitializeComponent();
            ReFlushlistver();
            #region 图标
            NIcon = new System.Windows.Forms.NotifyIcon();
            NIcon.Visible = true;
            System.Windows.Resources.StreamResourceInfo s = Application.GetResourceStream(new Uri("pack://application:,,,/screenLaunch.png"));
            NIcon.Icon = System.Drawing.Icon.FromHandle(new System.Drawing.Bitmap(s.Stream).GetHicon());
            System.Windows.Forms.ContextMenu NMenu = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem MenuItem = NMenu.MenuItems.Add(LangManager.GetLangFromResource("MenuShowMainWindow"));
            MenuItem.Name = "ShowMainWindow";
            MenuItem.DefaultItem = true;
            MenuItem.Click += NMenu_ShowMainWindows_Click;
            NIcon.DoubleClick += NIcon_DoubleClick;
            System.Windows.Forms.MenuItem DebugMode = NMenu.MenuItems.Add(LangManager.GetLangFromResource("MenuUseDebugMode"));
            DebugMode.Name = "DebugMode";
            DebugMode.Click += DebugMode_Click;
            NIcon.ContextMenu = NMenu;
            #endregion
            LoadLanguage();
            #region 加载配置
            if (File.Exists(cfgfile))
            {
                cfg = config.Load(cfgfile);
                Logger.Log(string.Format("加载{0}文件", cfgfile));
            }
            else
            {
                cfg = new config();
                Logger.Log("加载默认配置");
            }
            sliderJavaxmx.Maximum = config.getmem();
            if (cfg.javaw == "autosearch")
                txtJavaPath.Text = config.getjavadir();
            else
                txtJavaPath.Text = cfg.javaw;
            if (cfg.javaxmx == "autosearch")
                txtJavaXmx.Text = (config.getmem() / 4).ToString();
            else
                txtJavaXmx.Text = cfg.javaxmx;
            sliderJavaxmx.Value = int.Parse(txtJavaXmx.Text);
            txtUserName.Text = cfg.username;
            if (cfg.passwd != null)
                txtPwd.Password = Encoding.UTF8.GetString(cfg.passwd);
            txtExtJArg.Text = cfg.extraJVMArg;
            checkAutoStart.IsChecked = cfg.autostart;
            listVer.SelectedItem = cfg.lastPlayVer;
            if (listAuth.SelectedItem == null)
                listAuth.SelectedIndex = 0;
            sliderWindowTransparency.Value = cfg.WindowTransparency;
            checkReport.IsChecked = cfg.Report;
            txtInsPath.Text = AppDomain.CurrentDomain.BaseDirectory + "\\.minecraft";
            listDownSource.SelectedIndex = cfg.DownloadSource;
            LangManager.UseLanguage(cfg.Lang);
            comboLang.SelectedItem = LangManager.GetLangFromResource("DisplayName");
            #endregion
            LoadPlugin(LangManager.GetLangFromResource("LangName"));
            listAuth.SelectedItem = cfg.login;
            checkCheckUpdate.IsChecked = cfg.CheckUpdate;
            Logger.Log(cfg);
            
            this.Title = "BMCL V2 Ver." + ver;
            launcher.gameexit += launcher_gameexit;
#if DEBUG
#else
            if (cfg.Report)
            {
                Thread thReport = new Thread(new ThreadStart((new Report().Main)));
                thReport.Start();
            }
#endif
            if (cfg.CheckUpdate)
            {
                Thread thCheckUpdate = new Thread(new ThreadStart(funcCheckUpdate));
                thCheckUpdate.Start();
            }
        }

        void DebugMode_Click(object sender, EventArgs e)
        {
            Process.Start(Environment.CommandLine.Replace("\"", ""), "-Debug");
            NIcon.Visible = false;
            Environment.Exit(0);
        }

        #region 公共按钮
        private void btnChangeBg_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\bg"))
            {
                Random rand = new Random();
                ArrayList pics = new ArrayList();
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
                ImageBrush b;
                int img = rand.Next(imgTotal);
                b = new ImageBrush();
                b.ImageSource = new BitmapImage(new Uri((pics[img] as string)));
                b.Stretch = Stretch.Fill;
                DoubleAnimation da = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.25));
                this.BeginAnimation(FrmMain.OpacityProperty, da);
                this.Top.Background = b;
                da = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25));
                this.BeginAnimation(FrmMain.OpacityProperty, da);
            }
            else
            {
                if (e == null)
                    return;
                MessageBox.Show("请在启动启动其目录下新建bg文件夹，并放入图片文件，支持jpg,bmp,png等格式，比例请尽量接近16:9，否则会被拉伸");
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\bg");
                Process explorer = new Process();
                explorer.StartInfo.FileName = "explorer.exe";
                explorer.StartInfo.Arguments = AppDomain.CurrentDomain.BaseDirectory + "\\bg";
                explorer.Start();
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (game!=null)
                if (game.IsRunning())
                {
                    this.btnMiniSize_Click(null, null);
                    return;
                }
            Logger.Log(string.Format("BMCL V2 Ver.{0} 正在退出", ver));
            this.Close();
            if (!Logger.Debug)
            {
                Application.Current.Shutdown(0);
            }
        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsLaunchering)
                return;
            IsLaunchering = true;
            if (txtUserName.Text == "!!!")
            {
                MessageBox.Show("请先修改用户名");
                tabMain.SelectedIndex = 1;
                txtUserName.Focus();
                return;
            }
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\crash-reports"))
            {
                ClientCrashReportCount = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\crash-reports").Count();
            }
            else
            {
                ClientCrashReportCount = 0;
            }
            starter = new FrmPrs("正在准备游戏环境及启动游戏");
            int SelectedIndex = listAuth.SelectedIndex; ;
            Logger.Log(string.Format("正在启动{0},使用的登陆方式为{1}", listVer.SelectedItem.ToString(), AuthList.SelectedItem.ToString()));
            object Auth;
            if (SelectedIndex != 0)
                Auth = Auths[AuthList.SelectedItem.ToString()];
            else
                Auth = null;
            Thread thGO = new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(delegate
            {
                string tSelectVer = "";
                Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                {
                    tSelectVer = listVer.SelectedItem.ToString();
                    starter.ShowInTaskbar = false;
                    starter.Show();
                    starter.Activate();
                    starter.Focus();
                    starter.changeEventH("正在登陆");
                }));
                LoginInfo loginans = new LoginInfo();
                try
                {
                    if (SelectedIndex != 0)
                    {
                        Type T = Auth.GetType();
                        MethodInfo Login = T.GetMethod("Login");
                        try
                        {
                            object loginansobj;
                            Type Li;
                            string username = "", pwd = "";
                            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                            {
                                username = txtUserName.Text;
                                pwd = txtPwd.Password;
                            }));
                                loginansobj= Login.Invoke(Auth, new object[] { username, pwd, System.Guid.NewGuid().ToString(), "zh-cn" });
                                Li = loginansobj.GetType();
                                loginans.Suc = (bool)Li.GetField("Suc").GetValue(loginansobj);
                                if (loginans.Suc == true)
                                {
                                    loginans.UN = Li.GetField("UN").GetValue(loginansobj) as string;
                                    loginans.SID = Li.GetField("SID").GetValue(loginansobj)as string;
                                    loginans.Client_identifier = Li.GetField("Client_identifier").GetValue(loginansobj) as string;
                                    loginans.UID = Li.GetField("UID").GetValue(loginansobj) as string;
                                    loginans.OtherInfo = Li.GetField("OtherInfo").GetValue(loginansobj) as string;
                                    if (Li.GetField("OutInfo") != null)
                                    {
                                        loginans.OutInfo = Li.GetField("OutInfo").GetValue(loginansobj) as string;
                                    }
                                    Logger.Log(string.Format("登陆成功，使用用户名{0},sid{1},Client_identifier{2},uid{3}", loginans.UN != null ? loginans.UN : "", loginans.SID != null ? loginans.SID : "", loginans.Client_identifier != null ? loginans.Client_identifier : "", loginans.UID != null ? loginans.UID : ""));
                                }
                                else
                                {
                                    loginans.Errinfo = Li.GetField("Errinfo").GetValue(loginansobj)as string;
                                    loginans.OtherInfo = Li.GetField("OtherInfo").GetValue(loginansobj)as string;
                                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { starter.Topmost = false; }));
                                    Logger.Log(string.Format("登陆失败，错误信息:{0}，其他信息:{1}", loginans.Errinfo != null ? loginans.Errinfo : "", loginans.OtherInfo != null ? loginans.OtherInfo : ""));
                                }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                            loginans.Suc = false;
                            loginans.Errinfo = ex.Message;
                            while (ex.InnerException != null)
                            {
                                ex = ex.InnerException;
                                loginans.Errinfo += "\n" + ex.Message;
                            }
                            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { starter.Topmost = false; }));
                            MessageBox.Show("登录失败:" + loginans.Errinfo);
                        }
                    }
                    else
                    {
                        loginans.Suc = true;
                        loginans.SID = "no";
                        Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { loginans.UN = txtUserName.Text; }));
                    }
                    if (loginans.Suc == true)
                    {
                        Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { btnSaveConfig_Click(null, null); }));
                        string username = loginans.UN;
                        try
                        {
                            string javaPath = "", javaXmx = "", selectVer = "", extArg = "";
                            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                            {
                                javaPath = txtJavaPath.Text;
                                javaXmx = txtJavaXmx.Text;
                                selectVer = tSelectVer;
                                extArg = txtExtJArg.Text;
                            }));
                            game = new launcher(javaPath, javaXmx, username, selectVer, info, extArg, ref starter, loginans);
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { starter.Topmost = false; }));
                            MessageBox.Show("启动失败：" + ex.Message);
                            Logger.Log(ex);
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { starter.Topmost = false; }));
                        MessageBox.Show("登录失败:" + loginans.Errinfo);
                        Logger.Log("登录失败" + loginans.Errinfo, Logger.LogType.Error);
                        return;
                    }
                    try
                    {
                        if (game == null)
                        {
                            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { starter.Topmost = false; }));
                            Logger.Log("启动器初始化失败，放弃启动", Logger.LogType.Crash);
                        }
                        else
                        {
                            game.start();
                            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { this.Hide(); }));
                        }
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                        {
                            throw ex;
                        }));
                    }
                }
                finally
                {
                    NIcon.Visible = true;
                    if ((loginans.Suc == false && SelectedIndex != 0) || game == null)
                        NIcon.ShowBalloonTip(10000, "BMCL", "启动失败" + cfg.lastPlayVer, System.Windows.Forms.ToolTipIcon.Error);
                    else
                        NIcon.ShowBalloonTip(10000, "BMCL", "已启动" + cfg.lastPlayVer, System.Windows.Forms.ToolTipIcon.Info);
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { starter.Close(); }));
                    IsLaunchering = false;
                    if (info.assets != null)
                    {
                        Thread thAssets = new Thread(new ThreadStart(() => 
                        {
                            new assets.assets(info);
                        }));
                        thAssets.IsBackground = true;
                        thAssets.Start();
                    }
                }
            })));
            thGO.Start();

        }
        private void launcher_gameexit()
        {
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\crash-reports"))
            {
                if (ClientCrashReportCount != Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\crash-reports").Count())
                {
                    Logger.Log("发现新的错误报告");
                    DirectoryInfo ClientCrashReportDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\crash-reports");
                    string LastClientCrashReportPath = "";
                    DateTime LastClientCrashReportModifyTime=DateTime.MinValue;
                    foreach (FileInfo ClientCrashReport in ClientCrashReportDir.GetFiles())
                    {
                        if (LastClientCrashReportModifyTime < ClientCrashReport.LastWriteTime)
                        {
                            LastClientCrashReportPath = ClientCrashReport.FullName;
                        }
                    }
                    StreamReader CrashReportReader = new StreamReader(LastClientCrashReportPath);
                    Logger.Log(CrashReportReader.ReadToEnd(),Logger.LogType.Crash);
                    CrashReportReader.Close();
                    if (MessageBox.Show("客户端好像崩溃了，是否查看崩溃报告？", "客户端崩溃", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        Process.Start(LastClientCrashReportPath);
                    }
                }
            }
            if (Logger.Debug)
            {
                Logger.Log("游戏退出，Debug模式保留Log信息窗口，程序不退出");
                Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { this.Show(); }));
                return;
            }
            if (!inscreen)
            {
                Logger.Log("BMCL V2 Ver" + ver + DateTime.Now.ToString() + "由于游戏退出而退出");
                Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { Application.Current.Shutdown(0); }));
            }
        }
        private void NIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
        }
        private void NMenu_ShowMainWindows_Click(object sender, EventArgs e)
        {
            this.Show();
        }
        private void btnMiniSize_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            NIcon.ShowBalloonTip(2000, "BMCL", LangManager.GetLangFromResource("BMCLHiddenInfo"), System.Windows.Forms.ToolTipIcon.Info);
        }
        private void MenuSelectFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofbg = new System.Windows.Forms.OpenFileDialog();
            ofbg.CheckFileExists = true;
            ofbg.Filter = "支持的图片|*.jpg;*.png;*.bmp";
            ofbg.Multiselect = false;
            string pic;
            if (ofbg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                pic = ofbg.FileName;
            else
                return;
            ImageBrush b = new ImageBrush();
            b.ImageSource = new BitmapImage(new Uri((pic)));
            b.Stretch = Stretch.Fill;
            DoubleAnimation da = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.25));
            this.BeginAnimation(FrmMain.OpacityProperty, da);
            this.Top.Background = b;
            da = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25));
            this.BeginAnimation(FrmMain.OpacityProperty, da);
        }
        private void MenuSelectTexturePack_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("这是个正在试验的功能，请不要报告有关任何该功能的bug");
            FrmTexturepack frmTexturepack = new FrmTexturepack();
            frmTexturepack.ShowDialog();
            Texturepack.TexturePackEntity Texture = frmTexturepack.GetSelected();
            ImageBrush b = new ImageBrush();
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = Texture.GuiBackground;
            bitmap.EndInit();
            b.ImageSource = bitmap;
            b.ViewportUnits = BrushMappingMode.Absolute;
            b.Viewport = new Rect(0, 0, bitmap.Width, bitmap.Height);
            b.Stretch = Stretch.None;
            b.TileMode = TileMode.Tile;
            ImageBrush button = new ImageBrush();
            button.ImageSource = Texture.GuiButton.Source;

            DoubleAnimation da = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.25));
            this.BeginAnimation(FrmMain.OpacityProperty, da);
            this.Top.Background = b;
            btnStart.Background = button;
            da = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25));
            this.BeginAnimation(FrmMain.OpacityProperty, da);
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
            string JsonFilePath = gameinfo.GetGameInfoJsonPath(listVer.SelectedItem.ToString());
            if (string.IsNullOrEmpty(JsonFilePath))
            {
                MessageBox.Show(LangManager.GetLangFromResource("ErrorNoGameJson"));
                btnStart.IsEnabled = false;
                return;
            }
            else
            {
                btnStart.IsEnabled = true;
            }
            info = gameinfo.Read(JsonFilePath);
            labVer.Content = info.id;
            labTime.Content = info.time;
            labRelTime.Content = info.releaseTime;
            labType.Content = info.type;
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(LangManager.GetLangFromResource("DeleteMessageBoxInfo"), LangManager.GetLangFromResource("DeleteMessageBoxTitle"), MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                try
                {
                    FileStream Isused = File.OpenWrite(".minecraft\\versions\\" + listVer.SelectedItem.ToString() + "\\" + info.id + ".jar");
                    Isused.Close();
                    Directory.Delete(".minecraft\\versions\\" + listVer.SelectedItem.ToString(), true);
                    if (Directory.Exists(".minecraft\\libraries\\" + listVer.SelectedItem.ToString()))
                    {
                        Directory.Delete(".minecraft\\libraries\\" + listVer.SelectedItem.ToString(), true);
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
                Directory.Move(".minecraft\\versions\\" + listVer.SelectedItem.ToString(), ".minecraft\\versions\\" + rname);
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
            StringBuilder modpath = new StringBuilder(@".minecraft\versions\");
            modpath.Append(listVer.SelectedItem.ToString()).Append("\\");
            StringBuilder configpath = new StringBuilder(modpath.ToString());
            StringBuilder coremodpath = new StringBuilder(modpath.ToString());
            StringBuilder moddirpath = new StringBuilder(modpath.ToString());
            modpath.Append("mods");
            configpath.Append("config");
            coremodpath.Append("coremods");
            moddirpath.Append("moddir");
            if (!Directory.Exists(modpath.ToString()))
            {
                Directory.CreateDirectory(modpath.ToString());
            }
            if (!Directory.Exists(configpath.ToString()))
            {
                Directory.CreateDirectory(configpath.ToString());
            }
            if (!Directory.Exists(coremodpath.ToString()))
            {
                Directory.CreateDirectory(coremodpath.ToString());
            }
            if (!Directory.Exists(moddirpath.ToString()))
            {
                Directory.CreateDirectory(moddirpath.ToString());
            }
            Process explorer = new Process();
            explorer.StartInfo.FileName = "explorer.exe";
            explorer.StartInfo.Arguments = modpath.ToString();
            explorer.Start();
        }
        private void btnImportOldMc_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderImportOldVer = new System.Windows.Forms.FolderBrowserDialog();
            folderImportOldVer.Description = LangManager.GetLangFromResource("ImportDirInfo");
            FrmPrs prs = new FrmPrs(LangManager.GetLangFromResource("ImportPrsTitle"));
            prs.Show();
            if (folderImportOldVer.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string ImportFrom = folderImportOldVer.SelectedPath;
                if (!File.Exists(ImportFrom + "\\bin\\minecraft.jar"))
                {
                    MessageBox.Show(LangManager.GetLangFromResource("ImportFailedNoMinecraftFound"));
                    return;
                }
                string ImportName;
                bool F1 = false, F2 = false;
                ImportName = Microsoft.VisualBasic.Interaction.InputBox(LangManager.GetLangFromResource("ImportNameInfo"), LangManager.GetLangFromResource("ImportOldMcInfo"), "OldMinecraft");
                do
                {
                    F1 = false;
                    F2 = false;
                    if (ImportName.Length <= 0 || ImportName.IndexOf('.') != -1)
                        ImportName = Microsoft.VisualBasic.Interaction.InputBox(LangManager.GetLangFromResource("ImportNameInfo"), LangManager.GetLangFromResource("ImportInvildName"), "OldMinecraft");
                    else
                        F1 = true;
                    if (Directory.Exists(".minecraft\\versions\\" + ImportName))
                        ImportName = Microsoft.VisualBasic.Interaction.InputBox(LangManager.GetLangFromResource("ImportNameInfo"), LangManager.GetLangFromResource("ImportFailedExist"), "OldMinecraft");
                    else
                        F2 = true;

                } while (!(F1 && F2));
                Thread thImport = new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(delegate { ImportOldMC(ImportName, ImportFrom, prs); })));
                thImport.Start();
            }
            else prs.Close();
        }
        private void btnCoreModMrg_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder modpath = new StringBuilder(@".minecraft\versions\");
            modpath.Append(listVer.SelectedItem.ToString()).Append("\\");
            StringBuilder configpath = new StringBuilder(modpath.ToString());
            StringBuilder coremodpath = new StringBuilder(modpath.ToString());
            StringBuilder moddirpath = new StringBuilder(modpath.ToString());
            modpath.Append("mods");
            configpath.Append("config");
            coremodpath.Append("coremods");
            moddirpath.Append("moddir");
            if (!Directory.Exists(modpath.ToString()))
            {
                Directory.CreateDirectory(modpath.ToString());
            }
            if (!Directory.Exists(configpath.ToString()))
            {
                Directory.CreateDirectory(configpath.ToString());
            }
            if (!Directory.Exists(coremodpath.ToString()))
            {
                Directory.CreateDirectory(coremodpath.ToString());
            }
            if (!Directory.Exists(moddirpath.ToString()))
            {
                Directory.CreateDirectory(moddirpath.ToString());
            }
            Process explorer = new Process();
            explorer.StartInfo.FileName = "explorer.exe";
            explorer.StartInfo.Arguments = coremodpath.ToString();
            explorer.Start();
        }
        private void btnModdirMrg_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder modpath = new StringBuilder(@".minecraft\versions\");
            modpath.Append(listVer.SelectedItem.ToString()).Append("\\");
            StringBuilder configpath = new StringBuilder(modpath.ToString());
            StringBuilder coremodpath = new StringBuilder(modpath.ToString());
            StringBuilder moddirpath = new StringBuilder(modpath.ToString());
            modpath.Append("mods");
            configpath.Append("config");
            coremodpath.Append("coremods");
            moddirpath.Append("moddir");
            if (!Directory.Exists(modpath.ToString()))
            {
                Directory.CreateDirectory(modpath.ToString());
            }
            if (!Directory.Exists(configpath.ToString()))
            {
                Directory.CreateDirectory(configpath.ToString());
            }
            if (!Directory.Exists(coremodpath.ToString()))
            {
                Directory.CreateDirectory(coremodpath.ToString());
            }
            if (!Directory.Exists(moddirpath.ToString()))
            {
                Directory.CreateDirectory(moddirpath.ToString());
            }
            Process explorer = new Process();
            explorer.StartInfo.FileName = "explorer.exe";
            explorer.StartInfo.Arguments = moddirpath.ToString();
            explorer.Start();
        }
        private void btnModCfgMrg_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder modpath = new StringBuilder(@".minecraft\versions\");
            modpath.Append(listVer.SelectedItem.ToString()).Append("\\");
            StringBuilder configpath = new StringBuilder(modpath.ToString());
            StringBuilder coremodpath = new StringBuilder(modpath.ToString());
            StringBuilder moddirpath = new StringBuilder(modpath.ToString());
            modpath.Append("mods");
            configpath.Append("config");
            coremodpath.Append("coremods");
            moddirpath.Append("moddir");
            if (!Directory.Exists(modpath.ToString()))
            {
                Directory.CreateDirectory(modpath.ToString());
            }
            if (!Directory.Exists(configpath.ToString()))
            {
                Directory.CreateDirectory(configpath.ToString());
            }
            if (!Directory.Exists(coremodpath.ToString()))
            {
                Directory.CreateDirectory(coremodpath.ToString());
            }
            if (!Directory.Exists(moddirpath.ToString()))
            {
                Directory.CreateDirectory(moddirpath.ToString());
            }
            Process explorer = new Process();
            explorer.StartInfo.FileName = "explorer.exe";
            explorer.StartInfo.Arguments = configpath.ToString();
            explorer.Start();
        }
        private void listVer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (btnStart.IsEnabled)
                btnStart_Click(null, null);
        }
        private void btnLibraries_Click(object sender, RoutedEventArgs e)
        {
            FrmLibraries f = new FrmLibraries(info.libraries);
            if (f.ShowDialog() == true)
            {
                info.libraries = f.GetChange();
                string JsonFile = gameinfo.GetGameInfoJsonPath(listVer.SelectedItem.ToString());
                File.Delete(JsonFile + ".bak");
                File.Move(JsonFile, JsonFile + ".bak");
                gameinfo.Write(info, JsonFile);
                this.listVer_SelectionChanged(null, null);
            }
        }
        #endregion


        #region tabLauncherCfg
        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            cfg.autostart = (bool)checkAutoStart.IsChecked;
            cfg.extraJVMArg = txtExtJArg.Text;
            cfg.javaw = txtJavaPath.Text;
            cfg.javaxmx = txtJavaXmx.Text;
            cfg.login = listAuth.SelectedItem.ToString();
            cfg.lastPlayVer = listVer.SelectedItem as string;
            cfg.passwd = Encoding.UTF8.GetBytes(txtPwd.Password);
            cfg.username = txtUserName.Text;
            cfg.WindowTransparency = sliderWindowTransparency.Value;
            cfg.Report = checkReport.IsChecked.Value;
            cfg.DownloadSource = listDownSource.SelectedIndex;
            cfg.Lang = LangManager.GetLangFromResource("LangName");
            config.Save(cfg, cfgfile);
            DoubleAnimationUsingKeyFrames dak = new DoubleAnimationUsingKeyFrames();
            dak.KeyFrames.Add(new LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(0)));
            dak.KeyFrames.Add(new LinearDoubleKeyFrame(30, TimeSpan.FromSeconds(0.3)));
            dak.KeyFrames.Add(new LinearDoubleKeyFrame(30, TimeSpan.FromSeconds(2.3)));
            dak.KeyFrames.Add(new LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(2.6)));
            popupSaveSuccess.BeginAnimation(Grid.HeightProperty, dak);
        }
        private void sliderJavaxmx_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtJavaXmx.Text = ((int)sliderJavaxmx.Value).ToString();
        }
        private void txtUserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            tip.IsOpen = false;
        }
        private void btnSelectJava_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofJava = new System.Windows.Forms.OpenFileDialog();
            ofJava.RestoreDirectory = true;
            ofJava.Filter = "Javaw.exe|Javaw.exe";
            ofJava.Multiselect = false;
            ofJava.CheckFileExists = true;
            if (ofJava.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtJavaPath.Text = ofJava.FileName;
            }
        }
        private void sliderWindowTransparency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Top.Background != null)
                Top.Background.Opacity = e.NewValue;
        }

        private void listDownSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (listDownSource.SelectedIndex)
            {
                case 0:
                    FrmMain.URL_DOWNLOAD_BASE = Resource.Url.URL_DOWNLOAD_BASE;
                    FrmMain.URL_RESOURCE_BASE = Resource.Url.URL_RESOURCE_BASE;
                    FrmMain.URL_LIBRARIES_BASE = Resource.Url.URL_LIBRARIES_BASE;
                    break;
                case 1:
                    FrmMain.URL_DOWNLOAD_BASE = Resource.Url.URL_DOWNLOAD_bangbang93;
                    FrmMain.URL_RESOURCE_BASE = Resource.Url.URL_RESOURCE_bangbang93;
                    FrmMain.URL_LIBRARIES_BASE = Resource.Url.URL_LIBRARIES_bangbang93;
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
                Logger.Log(ex);
                MessageBox.Show("请输入一个有效数字");
                txtJavaXmx.Text = (config.getmem()/4).ToString();
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
            if (txtExtJArg.Text.IndexOf("-Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true") != -1)
            {
                checkOptifine.IsChecked = true;
            }
        }

        private void checkOptifine_Checked(object sender, RoutedEventArgs e)
        {
            if (txtExtJArg.Text.IndexOf("-Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true") != -1)
                return;
            txtExtJArg.Text += " -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true";
        }

        private void checkOptifine_Unchecked(object sender, RoutedEventArgs e)
        {
            int t = txtExtJArg.Text.IndexOf(" -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true");
            txtExtJArg.Text = txtExtJArg.Text.Replace(" -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true", "");
        }

        private void checkCheckUpdate_Checked(object sender, RoutedEventArgs e)
        {
            cfg.CheckUpdate = checkCheckUpdate.IsChecked == true ? true : false;
        }
        #endregion


        #region tabRemoteVer
        private void btnRefreshRemoteVer_Click(object sender, RoutedEventArgs e)
        {
            if (btnReflushServer.Content.ToString() == LangManager.GetLangFromResource("RemoteVerGetting"))
                return;
            listRemoteVer.DataContext = null;
            DataContractJsonSerializer RawJson = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(RawVersionListType));
            HttpWebRequest GetJson = (HttpWebRequest)WebRequest.Create(URL_DOWNLOAD_BASE + "versions/versions.json");
            GetJson.Timeout = 10000;
            GetJson.ReadWriteTimeout = 10000;
            Thread thGet = new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(delegate
            {
                try
                {
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { btnRefreshRemoteVer.Content = LangManager.GetLangFromResource("RemoteVerGetting"); btnRefreshRemoteVer.IsEnabled = false; }));
                HttpWebResponse GetJsonAns = (HttpWebResponse)GetJson.GetResponse();
                RawVersionListType RemoteVersion = RawJson.ReadObject(GetJsonAns.GetResponseStream()) as RawVersionListType;
                DataTable dt = new DataTable();
                dt.Columns.Add("Ver");
                dt.Columns.Add("RelTime");
                dt.Columns.Add("Type");
                foreach (RemoteVerType RV in RemoteVersion.getVersions())
                {
                    dt.Rows.Add(new string[] { RV.id, RV.releaseTime, RV.type });
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
            })));
            thGet.Start();
        }
        private void btnDownloadVer_Click(object sender, RoutedEventArgs e)
        {
            if (listRemoteVer.SelectedItem == null)
            {
                MessageBox.Show(LangManager.GetLangFromResource("RemoteVerErrorNoVersionSelect"));
                return;
            }
            DataRowView SelectVer = listRemoteVer.SelectedItem as DataRowView;
            string selectver = SelectVer[0] as string;
            StringBuilder downpath = new StringBuilder(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\versions\");
            downpath.Append(selectver).Append("\\");
            downpath.Append(selectver).Append(".jar");
            WebClient downer = new WebClient();
            StringBuilder downurl = new StringBuilder(URL_DOWNLOAD_BASE);
            downurl.Append(@"versions\");
            downurl.Append(selectver).Append("\\");
            downurl.Append(selectver).Append(".jar");
#if DEBUG
            MessageBox.Show(downpath.ToString()+"\n"+downurl.ToString());
#endif
            btnDownloadVer.Content = LangManager.GetLangFromResource("RemoteVerDownloading");
            btnDownloadVer.IsEnabled = false;
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(downpath.ToString())))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(downpath.ToString()));
            }
            string downjsonfile = downurl.ToString().Substring(0, downurl.Length - 4) + ".json";
            string downjsonpath = downpath.ToString().Substring(0, downpath.Length - 4) + ".json";
            try
            {
                downer.DownloadFileCompleted += downer_DownloadClientFileCompleted;
                downer.DownloadProgressChanged += downer_DownloadProgressChanged;
                Logger.Log("下载:" + downjsonfile, Logger.LogType.Info);
                downer.DownloadFile(new Uri(downjsonfile), downjsonpath);
                Logger.Log("下载:" + downurl, Logger.LogType.Info);
                downer.DownloadFileAsync(new Uri(downurl.ToString()), downpath.ToString());
                downedtime = Environment.TickCount - 1;
                downed = 0;
                gridDown.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message+"\n");
                btnDownloadVer.Content = LangManager.GetLangFromResource("btnDownloadVer");
                btnDownloadVer.IsEnabled = true;
            }

        }
        int downedtime;
        int downed;
        void downer_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            prsDown.Maximum = (int)e.TotalBytesToReceive;
            prsDown.Value = (int)e.BytesReceived;
            //            TaskbarManager.Instance.SetProgressValue((int)e.BytesReceived, (int)e.TotalBytesToReceive);
            StringBuilder info = new StringBuilder(LangManager.GetLangFromResource("DownloadSpeedInfo"));
            try
            {
                info.Append(((double)(e.BytesReceived - downed) / (double)((Environment.TickCount - downedtime) / 1000) / 1024.0).ToString("F2")).Append("KB/s,");
            }
            catch (DivideByZeroException) { info.Append("0B/s,"); }
            info.Append(e.ProgressPercentage.ToString()).Append("%");
            labDownInfo.Content = info.ToString();
        }

        void downer_DownloadClientFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Logger.Log("下载客户端文件成功", Logger.LogType.Info);
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
            FrmCheckRes checkres = new FrmCheckRes();
            checkres.Show();
        }
        private void listRemoteVer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnDownloadVer_Click(null, null);
        }

        #endregion


        #region tabForge
        ForgeVersionList ForgeVer = new ForgeVersionList();
        private void RefreshForgeVersionList()
        {
            treeForgeVer.Items.Add(LangManager.GetLangFromResource("ForgeListGetting"));
            ForgeVer.ForgePageReadyEvent += ForgeVer_ForgePageReadyEvent;
            ForgeVer.GetVersion();
        }

        void ForgeVer_ForgePageReadyEvent()
        {
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(() =>
            {
                treeForgeVer.Items.Clear();
                treeForgeVer.Items.Add(ForgeVer.GetLastForge());
                foreach (TreeViewItem t in ForgeVer.GetAllBuild())
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
            if (!ForgeVer.ForgeDownloadUrl.ContainsKey(ver))
            {
                MessageBox.Show(LangManager.GetLangFromResource("ForgeDoNotSupportInstaller"));
                return;
            }
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { gridDown.Visibility = Visibility.Visible; }));
            Uri url = new Uri(ForgeVer.ForgeDownloadUrl[ver].ToString());
            WebClient downer = new WebClient();
            downer.DownloadProgressChanged+=downer_DownloadProgressChanged;
            downer.DownloadFileCompleted += downer_DownloadForgeCompleted;
            downedtime = Environment.TickCount - 1;
            downed = 0;
            StreamWriter w = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\.minecraft\\launcher_profiles.json");
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
            Process ForgeIns = new Process();
            if (!File.Exists(cfg.javaw))
            {
                MessageBox.Show(LangManager.GetLangFromResource("ForgeJavaError"));
                return;
            }
            ForgeIns.StartInfo.FileName = cfg.javaw;
            ForgeIns.StartInfo.Arguments = "-jar " + AppDomain.CurrentDomain.BaseDirectory + "\\forge.jar";
            ForgeIns.Start();
            ForgeIns.WaitForExit();
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
                if (!ForgeVer.ForgeChangeLogUrl.ContainsKey(this.treeForgeVer.SelectedItem as string))
                {
                    MessageBox.Show(LangManager.GetLangFromResource("ForgeDoNotHaveChangeLog"));
                    return;
                }
                txtChangeLog.Text = LangManager.GetLangFromResource("FetchingForgeChangeLog");
                WebClient GetLog = new WebClient();
                GetLog.DownloadStringCompleted += GetLog_DownloadStringCompleted;
                GetLog.DownloadStringAsync(new Uri(ForgeVer.ForgeChangeLogUrl[this.treeForgeVer.SelectedItem as string] as string));
            }
        }

        void GetLog_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            txtChangeLog.Text = e.Result;
        }
        #endregion


        #region tabServerList

        DataTable ServerListDataTable = new DataTable();
        private serverlist.serverlist sl;
        private void btnReflushServer_Click(object sender, RoutedEventArgs e)
        {
            ServerListDataTable.Clear();
            ServerListDataTable.Columns.Clear();
            ServerListDataTable.Rows.Clear();
            ServerListDataTable.Columns.Add("ServerName");
            ServerListDataTable.Columns.Add("HiddenAddress");
            ServerListDataTable.Columns.Add("ServerAddress");
            ServerListDataTable.Columns.Add("ServerMotd");
            ServerListDataTable.Columns.Add("ServerVer");
            ServerListDataTable.Columns.Add("ServerOnline");
            ServerListDataTable.Columns.Add("ServerDelay");
            this.listServer.DataContext = ServerListDataTable;
            this.btnReflushServer.IsEnabled = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(GetServerInfo));
        }

        private void GetServerInfo(object obj)
        {
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { btnReflushServer.Content = LangManager.GetLangFromResource("ServerListGetting"); }));
            if (File.Exists(@".minecraft\servers.dat"))
            {
                sl = new serverlist.serverlist();
                foreach (serverlist.serverinfo info in sl.info)
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
                        Logger.Log(logger.ToString());
                        lock (ServerListDataTable)
                        {
                            ServerListDataTable.Rows.Add(server);
                            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                            {
                                listServer.DataContext = null;
                                listServer.DataContext = ServerListDataTable;
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
                    sl = new serverlist.serverlist();
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
            serverlist.AddServer FrmAdd = new serverlist.AddServer(ref sl);
            if (FrmAdd.ShowDialog() == true)
            {
                sl.Write();
                btnReflushServer_Click(null, null);
            }
        }

        private void btnDeleteServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sl.Delete(listServer.SelectedIndex);
                sl.Write();
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
                serverlist.AddServer FrmEdit = new serverlist.AddServer(ref sl, selected);
                if (FrmEdit.ShowDialog() == true)
                {
                    serverlist.serverinfo info = FrmEdit.getEdit();
                    sl.Edit(selected, info.Name, info.Address, info.IsHide);
                    sl.Write();
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


        bool loadOk = false;
        private void FrmMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!loadOk)
            {
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\assets"))
                    if (MessageBox.Show("可能是第一次启动，未找到资源文件，是否下载？", "未找到资源文件", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        FrmCheckRes frmCheckRes = new FrmCheckRes();
                        frmCheckRes.Show();
                    }
            }
            if (cfg.username == "!!!")
            {
                tabMain.SelectedIndex = 1;
                tip.IsOpen = true;
                txtUserName.Focus();
            }
            else
            {
                if (cfg.autostart == true)
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
                this.Top.Background = b;
            }
            catch
            {
                SolidColorBrush b=new SolidColorBrush(Color.FromRgb(255,255,255));
                this.Top.Background = b;
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
            else
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
            NIcon.Visible = false;
        }

        private void FrmMainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible == true)
            {
                inscreen = true;
                btnChangeBg_Click(null, null);
            }
            else
                inscreen = false;
            
        }


        private void ImportOldMC(string ImportName,string ImportFrom,FrmPrs prs)
        {
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { prs.changeEventH(LangManager.GetLangFromResource("ImportMain")); }));
            Directory.CreateDirectory(".minecraft\\versions\\" + ImportName);
            File.Copy(ImportFrom + "\\bin\\minecraft.jar", ".minecraft\\versions\\" + ImportName + "\\" + ImportName + ".jar");
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { prs.changeEventH(LangManager.GetLangFromResource("ImportCreateJson")); }));
            gameinfo info = new gameinfo();
            info.id = ImportName;
            string timezone = DateTimeOffset.Now.Offset.ToString();
            if (timezone[0] != '-')
            {
                timezone = "+" + timezone;
            }
            info.time = DateTime.Now.GetDateTimeFormats('s')[0].ToString() + timezone;
            info.releaseTime = DateTime.Now.GetDateTimeFormats('s')[0].ToString() + timezone;
            info.type = portinfo;
            info.minecraftArguments = "${auth_player_name}";
            info.mainClass = "net.minecraft.client.Minecraft";
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { prs.changeEventH(LangManager.GetLangFromResource("ImportSolveNative")); }));
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
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { prs.changeEventH(LangManager.GetLangFromResource("ImportWriteJson")); }));
            FileStream wcfg = new FileStream(".minecraft\\versions\\" + ImportName + "\\" + ImportName + ".json", FileMode.Create);
            DataContractJsonSerializer infojson = new DataContractJsonSerializer(typeof(gameinfo));
            infojson.WriteObject(wcfg, info);
            wcfg.Close();
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { prs.changeEventH(LangManager.GetLangFromResource("ImportSolveLib")); }));
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
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { prs.changeEventH(LangManager.GetLangFromResource("ImportSolveMod")); }));
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

        public struct LoginInfo
        {
            public string UN;
            public string UID;
            public string SID;
            public bool Suc;
            public string Errinfo;
            public string OtherInfo;
            public string Client_identifier;
            public string OutInfo;
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
                    LangManager.UseLanguage(Language[comboLang.SelectedItem as string]as string);break;
            }
            ChangeLanguage();
        }

        new Hashtable Language = new Hashtable();
        private void LoadLanguage()
        {
            ResourceDictionary Lang;
            Lang = LangManager.LoadLangFromResource("pack://application:,,,/Lang/zh-cn.xaml");
            Language.Add(Lang["DisplayName"], Lang["LangName"]);
            comboLang.Items.Add(Lang["DisplayName"]);
            LangManager.Add(Lang["LangName"] as string, "pack://application:,,,/Lang/zh-cn.xaml");

            Lang = LangManager.LoadLangFromResource("pack://application:,,,/Lang/zh-tw.xaml");
            Language.Add(Lang["DisplayName"], Lang["LangName"]);
            comboLang.Items.Add(Lang["DisplayName"]);
            LangManager.Add(Lang["LangName"] as string, "pack://application:,,,/Lang/zh-tw.xaml");
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Lang"))
            {
                foreach (string LangFile in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\Lang", "*.xaml", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        Lang = LangManager.LoadLangFromResource(LangFile);
                        Language.Add(Lang["DisplayName"], Lang["LangName"]);
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

        private void ChangeLanguage()
        {
            listDownSource.Items[0] = LangManager.GetLangFromResource("listOfficalSource");
            listDownSource.Items[1] = LangManager.GetLangFromResource("listAuthorSource");
            LoadPlugin(LangManager.GetLangFromResource("LangName"));
        }

        private void LoadPlugin(string Language)
        {
            listAuth.Items.Clear();
            Auths.Clear();
            #region 加载新插件
            listAuth.Items.Add(LangManager.GetLangFromResource("NoneAuth"));
            if (Directory.Exists("auths"))
            {
                string[] authplugins = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\auths");
                foreach (string auth in authplugins)
                {
                    if (auth.ToLower().EndsWith(".dll"))
                    {
                        Logger.Log("尝试加载" + auth);
                        try
                        {
                            Assembly AuthMethod = Assembly.LoadFrom(auth);
                            Type[] types = AuthMethod.GetTypes();
                            foreach (Type t in types)
                            {
                                try
                                {
                                    object Auth = AuthMethod.CreateInstance(t.FullName);
                                    Type T = Auth.GetType();
                                    MethodInfo AuthVer = T.GetMethod("GetVer");
                                    if (AuthVer == null)
                                    {
                                        Logger.Log(string.Format("未找到{0}的GetVer方法，放弃加载", auth));
                                        continue;
                                    }
                                    if ((long)AuthVer.Invoke(Auth, null) != 1)
                                    {
                                        Logger.Log(string.Format("{0}的版本不为1，放弃加载", auth));
                                        continue;
                                    }
                                    MethodInfo AuthVersion = T.GetMethod("GetVersion");
                                    if (AuthVersion == null)
                                    {
                                        Logger.Log(string.Format("{0}为第一代插件", auth));
                                    }
                                    else if ((long)AuthVersion.Invoke(Auth, new object[]{2}) != 2)
                                        {
                                            Logger.Log(string.Format("{0}版本高于启动器，放弃加载", auth));
                                        }
                                    MethodInfo MAuthName = T.GetMethod("GetName");
                                    string AuthName = MAuthName.Invoke(Auth, new object[] { Language }).ToString();
                                    Auths.Add(AuthName, Auth);
                                    listAuth.Items.Add(AuthName);
                                    Logger.Log(string.Format("{0}加载成功，名称为{1}", auth, AuthName), Logger.LogType.Error);
                                }
                                catch (MissingMethodException ex) 
                                {
                                    Logger.Log(string.Format("加载{0}的{1}失败", auth, t.ToString()), Logger.LogType.Error);
                                    Logger.Log(ex, Logger.LogType.Exception);
                                }
                                catch (ArgumentException ex) 
                                {
                                    Logger.Log(string.Format("加载{0}的{1}失败", auth, t.ToString()), Logger.LogType.Error);
                                    Logger.Log(ex, Logger.LogType.Exception);
                                }
                                catch (NotSupportedException ex)
                                {
                                    if (ex.Message.IndexOf("0x80131515") != -1)
                                    {
                                        MessageBox.Show(LangManager.GetLangFromResource("LoadPluginLockErrorInfo"), LangManager.GetLangFromResource("LoadPluginLockErrorTitle"));
                                    }
                                    else throw ex;
                                }
                            }
                        }
                        catch (NotSupportedException ex)
                        {
                            if (ex.Message.IndexOf("0x80131515") != -1)
                            {
                                MessageBox.Show(LangManager.GetLangFromResource("LoadPluginLockErrorInfo"), LangManager.GetLangFromResource("LoadPluginLockErrorTitle"));
                            }
                        }
                    }
                }
            }
            AuthList = listAuth;
            #endregion
            if (listAuth.SelectedIndex == -1)
                listAuth.SelectedIndex = 0;
        }

        private void MenuStartDebug_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(LangManager.GetLangFromResource("MenuDebugHint"));
            Logger.Debug = true;
            btnStart_Click(null, null);
        }

        private void FrmMainWindow_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.IsLaunchering && starter != null)
            {
                starter.Activate();
            }
        }

        private void FrmMainWindow_Activated(object sender, EventArgs e)
        {
            if (this.IsLaunchering && starter != null)
            {
                starter.Activate();
            }
        }

        private void funcCheckUpdate()
        {
            UpdateChecker check = new UpdateChecker();
            if (check.HasUpdate)
            {
                if (MessageBox.Show(check.UpdateInfo, "更新", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    Process.Start(check.LastestDownloadUrl);
                }
            }
        }



        




    }
}
