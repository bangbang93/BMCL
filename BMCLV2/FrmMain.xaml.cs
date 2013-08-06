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


using BMCLV2.Versions;

using HtmlAgilityPack;

namespace BMCLV2
{
    /// <summary>
    /// FrmMain.xaml 的交互逻辑
    /// </summary>
    public partial class FrmMain : Window
    {
        static private string cfgfile = "bmcl.xml";
        public static String URL_DOWNLOAD_BASE = "https://s3.amazonaws.com/Minecraft.Download/";
        public static String URL_RESOURCE_BASE = "https://s3.amazonaws.com/Minecraft.Resources/";
        private ArrayList Auths = new ArrayList();
        public static config cfg;
        static DataContractSerializer Cfg = new DataContractSerializer(typeof(config));
        static public gameinfo info;
        string session;
        int startup = 4;
        launcher game;
        bool inscreen;
        static public ListBox AuthList;
        static public string portinfo = "Port By BMCL";
        static public bool debug;
        public delegate void statuschange(string status);
        public static event statuschange changeEvent;
        System.Windows.Forms.NotifyIcon NIcon;
        public static string ver;

        public FrmMain()
        {
            InitializeComponent();
            ReFlushlistver();
            this.Icon = new BitmapImage(new Uri("pack://application:,,,/Resource/tofu slime.jpg"));
            NIcon = new System.Windows.Forms.NotifyIcon();
            NIcon.Visible = true;
            System.Windows.Resources.StreamResourceInfo s = Application.GetResourceStream(new Uri("pack://application:,,,/Resource/tofu slime.jpg"));
            NIcon.Icon = System.Drawing.Icon.FromHandle(new System.Drawing.Bitmap(s.Stream).GetHicon());
            System.Windows.Forms.ContextMenu NMenu = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem MenuItem = NMenu.MenuItems.Add("显示主窗口");
            MenuItem.Name = "ShowMainWindow";
            MenuItem.Click += NMenu_ShowMainWindows_Click;
            NIcon.DoubleClick += NIcon_DoubleClick;
            NIcon.ContextMenu = NMenu;

            #region 加载插件
            listAuth.Items.Add("啥都没有");
            if (Directory.Exists("auths"))
            {
                string[] authplugins = Directory.GetFiles(Environment.CurrentDirectory + @"\auths");
                foreach (string auth in authplugins)
                {
                    if (auth.ToLower().EndsWith(".dll"))
                    {
                        try
                        {
                            Assembly AuthMothed = Assembly.LoadFrom(auth);
                            Type[] types = AuthMothed.GetTypes();
                            foreach (Type t in types)
                            {
                                if (t.GetInterface("auth") != null)
                                {
                                    Auths.Add(AuthMothed.CreateInstance(t.FullName));
                                    object Auth = Auths[Auths.Count - 1];
                                    Type T = Auth.GetType();
                                    MethodInfo AuthName = T.GetMethod("getname");
                                    listAuth.Items.Add(AuthName.Invoke(Auth, null).ToString());

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }

                }
            }
            AuthList = listAuth;
            #endregion
            #region 加载配置
            cfg = config.Load(cfgfile);
            if (cfg.javaw == "autosearch")
                txtJavaPath.Text = config.getjavadir();
            else
                txtJavaPath.Text = cfg.javaw;
            if (cfg.javaxmx == "autosearch")
                txtJavaXmx.Text = (config.getmem() / 4).ToString();
            else
                txtJavaXmx.Text = cfg.javaxmx;
            sliderJavaxmx.Maximum = config.getmem();
            sliderJavaxmx.Value = int.Parse(txtJavaXmx.Text);
            txtUserName.Text = cfg.username;
            if (cfg.passwd != null)
                txtPwd.Password = Encoding.UTF8.GetString(cfg.passwd);
            txtExtJArg.Text = cfg.extraJVMArg;
            listVer.SelectedItem = cfg.lastPlayVer;
            listAuth.SelectedItem = cfg.login;
            sliderWindowTransparency.Value = cfg.WindowTransparency;
            checkReport.IsChecked = cfg.Report;
            txtInsPath.Text = Environment.CurrentDirectory + "\\.minecraft";
            #endregion
            ver=Application.ResourceAssembly.FullName.Split('=')[1];
            ver = ver.Substring(0, ver.IndexOf(','));
            this.Title = "BMCL V2 Ver." + ver;
#if DEBUG
#else
            if (cfg.Report)
            {
                Thread thReport = new Thread(new ThreadStart((new Report().Main)));
                thReport.Start();
            }
#endif
        }


        #region 公共按钮
        private void btnChangeBg_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Environment.CurrentDirectory + "\\bg"))
            {
                Random rand = new Random();
                ArrayList pics = new ArrayList();
                foreach (string str in Directory.GetFiles(Environment.CurrentDirectory + "\\bg", "*.jpg", SearchOption.AllDirectories))
                {
                    pics.Add(str);
                }
                foreach (string str in Directory.GetFiles(Environment.CurrentDirectory + "\\bg", "*.png", SearchOption.AllDirectories))
                {
                    pics.Add(str);
                }
                foreach (string str in Directory.GetFiles(Environment.CurrentDirectory + "\\bg", "*.bmp", SearchOption.AllDirectories))
                {
                    pics.Add(str);
                }
                int imgTotal = pics.Count; 
                if (imgTotal == 0)
                {
                    MessageBox.Show("没有可用的背景图");
                    return;
                }
                if (imgTotal == 1)
                {
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
                MessageBox.Show("请在启动启动其目录下新建bg文件夹，并放入图片文件，支持jpg,bmp,png等格式，比例请尽量接近16:9，否则会被拉伸");
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\bg");
                Process explorer = new Process();
                explorer.StartInfo.FileName = "explorer.exe";
                explorer.StartInfo.Arguments = Environment.CurrentDirectory + "\\bg";
                explorer.Start();
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (txtUserName.Text == "!!!")
            {
                MessageBox.Show("请先修改用户名");
                tabMain.SelectedIndex = 1;
                txtUserName.Focus();
                return;
            }
            FrmPrs starter = new FrmPrs("正在准备游戏环境及启动游戏");
            starter.Show();
            changeEvent("正在登陆");
            try
            {
                if (listAuth.SelectedIndex != 0)
                {
                    object Auth = Auths[Auths.Count - 1];
                    Type T = Auth.GetType();
                    MethodInfo Login = T.GetMethod("login");
                    string loginans;
                    try
                    {
                        loginans = Login.Invoke(Auth, new object[] { txtUserName.Text, txtPwd.Password }).ToString();
                    }
                    catch (Exception ex)
                    {
                        Exception exc = ex;
                        while (exc.InnerException != null)
                        {
                            exc = exc.InnerException;
                        }
                        MessageBox.Show(exc.Message);
                        return;
                    }
                    if (loginans == "True")
                    {
                        cfg.username = txtUserName.Text;
                        cfg.passwd = Encoding.UTF8.GetBytes(txtPwd.Password);
                        cfg.javaxmx = txtJavaXmx.Text;
                        cfg.javaw = txtJavaPath.Text;
                        cfg.login = listAuth.SelectedItem.ToString();
                        cfg.lastPlayVer = listVer.SelectedItem.ToString();
                        cfg.autostart = checkAutoStart.IsChecked.Value;
                        cfg.extraJVMArg = txtExtJArg.Text;
                        MethodInfo getSession = T.GetMethod("getsession");
                        session = getSession.Invoke(Auth, null).ToString();
                        config.Save(cfg, cfgfile);
                        MethodInfo getPname = T.GetMethod("getPname");
                        string username = getPname.Invoke(Auth, null).ToString();
                        try
                        {
                            game = new launcher(txtJavaPath.Text, txtJavaXmx.Text, username, listVer.SelectedItem.ToString(), info, txtExtJArg.Text, session);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    else
                    {
                        MessageBox.Show("登录失败，用户名或密码错误");
                        return;
                    }
                }
                else
                {
                    try
                    {
                        cfg.username = txtUserName.Text;
                        cfg.passwd = null;
                        cfg.javaxmx = txtJavaXmx.Text;
                        cfg.javaw = txtJavaPath.Text;
                        cfg.login = listAuth.SelectedItem.ToString();
                        cfg.lastPlayVer = listVer.SelectedItem.ToString();
                        cfg.autostart = checkAutoStart.IsChecked.Value;
                        cfg.extraJVMArg = txtExtJArg.Text;
                        config.Save(cfg, cfgfile);
                        game = new launcher(txtJavaPath.Text, txtJavaXmx.Text, txtUserName.Text, listVer.SelectedItem.ToString(), info, txtExtJArg.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }

                try
                {
                    bool start = game.start();
                    launcher.gameexit += launcher_gameexit;
                    this.Hide();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            finally
            {
                NIcon.Visible = true;
                NIcon.ShowBalloonTip(10000, "BMCL", "启动" + cfg.lastPlayVer + "成功", System.Windows.Forms.ToolTipIcon.Info);
                starter.Close();
            }

        }

        void launcher_gameexit()
        {
            if (!inscreen)
            {
                Environment.Exit(0);
            }
        }
        void NIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
        }
        void NMenu_ShowMainWindows_Click(object sender, EventArgs e)
        {
            this.Show();
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
            StringBuilder JsonFilePath = new StringBuilder();
            JsonFilePath.Append(@".minecraft\versions\");
            JsonFilePath.Append(listVer.SelectedItem.ToString());
            JsonFilePath.Append(@"\");
            JsonFilePath.Append(listVer.SelectedItem.ToString());
            JsonFilePath.Append(".json");
            if (!File.Exists(JsonFilePath.ToString()))
            {
                DirectoryInfo mcpath = new DirectoryInfo(System.IO.Path.GetDirectoryName(JsonFilePath.ToString()));
                bool find = false;
                foreach (FileInfo js in mcpath.GetFiles())
                {
                    if (js.FullName.Contains("json"))
                    {
                        JsonFilePath = new StringBuilder(js.FullName);
                        find = true;
                    }
                }
                if (!find)
                {
                    MessageBox.Show("找不到版本所需的json文件");
                    btnStart.IsEnabled = false;
                    return;
                }
                else
                {
                    btnStart.IsEnabled = true;
                }
            }
            StreamReader JsonFile = new StreamReader(JsonFilePath.ToString());
            DataContractJsonSerializer InfoReader = new DataContractJsonSerializer(typeof(gameinfo));
            info = InfoReader.ReadObject(JsonFile.BaseStream) as gameinfo;
            JsonFile.Close();
            labVer.Content = info.id;
            labTime.Content = info.time;
            labRelTime.Content = info.releaseTime;
            labType.Content = info.type;
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("你确定要删除当前版本？这个操作不能恢复，1.5.1之后的版本可去版本管理里重新下载", "删除确认", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
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
                    MessageBox.Show("删除失败，请检查该客户端是否正处于运行状态");
                }
                catch (IOException)
                {
                    MessageBox.Show("删除失败，请检查该客户端是否正处于运行状态");
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
                string rname = Microsoft.VisualBasic.Interaction.InputBox("新名字", "重命名", listVer.SelectedItem.ToString());
                if (rname == "") return;
                if (rname == listVer.SelectedItem.ToString()) return;
                if (listVer.Items.IndexOf(rname) != -1) throw new Exception("这个名字已经存在");
                Directory.Move(".minecraft\\versions\\" + listVer.SelectedItem.ToString(), ".minecraft\\versions\\" + rname);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("重命名失败，请检查客户端是否开启");
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
            FrmPrs prs = new FrmPrs("正在导入Minecraft");
            if (folderImportOldVer.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string ImportFrom = folderImportOldVer.SelectedPath;
                if (!File.Exists(ImportFrom + "\\bin\\minecraft.jar"))
                {
                    MessageBox.Show("未在该目录内发现有效的旧版Minecraft");
                    return;
                }
                string ImportName;
                bool F1 = false, F2 = false;
                ImportName = Microsoft.VisualBasic.Interaction.InputBox("输入导入后的名称", "导入旧版MC", "OldMinecraft");
                do
                {
                    F1 = false;
                    F2 = false;
                    if (ImportName.Length <= 0 || ImportName.IndexOf('.') != -1)
                        ImportName = Microsoft.VisualBasic.Interaction.InputBox("输入导入后的名称", "输入无效，请不要带\".\"符号", "OldMinecraft");
                    else
                        F1 = true;
                    if (Directory.Exists(".minecraft\\versions\\" + ImportName))
                        ImportName = Microsoft.VisualBasic.Interaction.InputBox("输入导入后的名称", "版本已存在", "OldMinecraft");
                    else
                        F2 = true;

                } while (!(F1 && F2));
                prs.Show();
                changeEvent("导入主程序");
                Directory.CreateDirectory(".minecraft\\versions\\" + ImportName);
                File.Copy(ImportFrom + "\\bin\\minecraft.jar", ".minecraft\\versions\\" + ImportName + "\\" + ImportName + ".jar");
                changeEvent("创建Json");
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
                changeEvent("处理native");
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
                changeEvent("写入Json");
                FileStream wcfg = new FileStream(".minecraft\\versions\\" + ImportName + "\\" + ImportName + ".json", FileMode.Create);
                DataContractJsonSerializer infojson = new DataContractJsonSerializer(typeof(gameinfo));
                infojson.WriteObject(wcfg, info);
                wcfg.Close();
                changeEvent("处理lib");
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
                changeEvent("处理mods");
                if (Directory.Exists(ImportFrom + "\\mods"))
                    util.Dir.dircopy(ImportFrom + "\\mods", ".minecraft\\versions\\" + ImportName + "\\mods");
                else
                    Directory.CreateDirectory(".minecraft\\versions\\" + ImportName + "\\mods");
                if (Directory.Exists(ImportFrom + "\\coremods"))
                    util.Dir.dircopy(ImportFrom + "\\coremods", ".minecraft\\versions\\" + ImportName + "\\coremods");
                else
                    Directory.CreateDirectory(".minecraft\\versions\\" + ImportName + "\\coremods");
                if (Directory.Exists(ImportFrom + "\\config"))
                    util.Dir.dircopy(ImportFrom + "\\config", ".minecraft\\versions\\" + ImportName + "\\config");
                else
                    Directory.CreateDirectory(".minecraft\\versions\\" + ImportName + "\\configmods");
                prs.Close();
                MessageBox.Show("导入成功，如果这个版本的MC还有MOD在.minecraft下创建了文件夹（例如Flan's mod,Custom NPC等），请点击MOD独立文件夹按钮进行管理");
                this.ReFlushlistver();
            }
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
            explorer.StartInfo.Arguments = moddirpath.ToString();
            explorer.Start();
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
            cfg.passwd = Encoding.UTF8.GetBytes(txtPwd.Password);
            cfg.username = txtUserName.Text;
            cfg.WindowTransparency = sliderWindowTransparency.Value;
            cfg.Report = checkReport.IsChecked.Value;
            config.Save(cfg, cfgfile);
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

        #endregion


        #region tabRemoteVer
        private void btnRefreshRemoteVer_Click(object sender, RoutedEventArgs e)
        {
            listRemoteVer.Items.Clear();
            DataContractJsonSerializer RawJson = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(RawVersionListType));
            HttpWebRequest GetJson = (HttpWebRequest)WebRequest.Create("https://s3.amazonaws.com/Minecraft.Download/versions/versions.json");
            GetJson.Timeout = 10000;
            GetJson.ReadWriteTimeout = 10000;
            Thread thGet = new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(delegate
            {
                try
                {
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { btnRefreshRemoteVer.Content = "正在获取，请稍候"; }));
                HttpWebResponse GetJsonAns = (HttpWebResponse)GetJson.GetResponse();
                RawVersionListType RemoteVersion = RawJson.ReadObject(GetJsonAns.GetResponseStream()) as RawVersionListType;
                DataTable dt = new DataTable();
                dt.Columns.Add("版本");
                dt.Columns.Add("发布时间");
                dt.Columns.Add("发布类型");
                foreach (RemoteVerType RV in RemoteVersion.getVersions())
                {
                    dt.Rows.Add(new string[] { RV.id, RV.releaseTime, RV.type });
                }
                Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                {
                    btnRefreshRemoteVer.Content = "刷新版本";
                    listRemoteVer.DataContext = dt;
                    listRemoteVer.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("版本", System.ComponentModel.ListSortDirection.Ascending));
                }));
                }
                catch (WebException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                catch (TimeoutException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            })));
            thGet.Start();
        }
        private void btnDownloadVer_Click(object sender, RoutedEventArgs e)
        {
            if (listRemoteVer.SelectedItem == null)
            {
                MessageBox.Show("请先选择一个版本");
                return;
            }
            DataRowView SelectVer = listRemoteVer.SelectedItem as DataRowView;
            string selectver = SelectVer[0] as string;
            StringBuilder downpath = new StringBuilder(Environment.CurrentDirectory + @"\.minecraft\versions\");
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
            btnDownloadVer.Content = "下载中请稍候";
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
                downer.DownloadFile(new Uri(downjsonfile), downjsonpath);
                downer.DownloadFileAsync(new Uri(downurl.ToString()), downpath.ToString());
                downedtime = Environment.TickCount - 1;
                downed = 0;
                gridDown.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message+"\n"+ex.InnerException.Message);
            }

        }
        int downedtime;
        int downed;
        void downer_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            prsDown.Maximum = (int)e.TotalBytesToReceive;
            prsDown.Value = (int)e.BytesReceived;
            //            TaskbarManager.Instance.SetProgressValue((int)e.BytesReceived, (int)e.TotalBytesToReceive);
            StringBuilder info = new StringBuilder("速度：");
            try
            {
                info.Append(((double)(e.BytesReceived - downed) / (double)((Environment.TickCount - downedtime) / 1000) / 1024.0).ToString("F2")).AppendLine("KB/s");
            }
            catch (DivideByZeroException) { info.AppendLine("0B/s"); }
            info.Append(e.ProgressPercentage.ToString()).AppendLine("%");
            labDownInfo.Content = info.ToString();
        }

        void downer_DownloadClientFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            MessageBox.Show("下载成功");
            btnDownloadVer.Content = "下载";
            ReFlushlistver();
            //            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            gridDown.Visibility = Visibility.Hidden;
            tabMain.SelectedIndex = 0;
        }
        private void btnCheckRes_Click(object sender, RoutedEventArgs e)
        {
            FrmCheckRes checkres = new FrmCheckRes();
            checkres.ShowDialog();
        }

        #endregion


        #region tabForge
        Hashtable DownloadUrl = new Hashtable();
        Thread thGet;
        private void btnLastForge_Click(object sender, RoutedEventArgs e)
        {
            if (DownloadUrl.Count == 0)
            {
                HtmlDocument ForgePage;
                treeForgeVer.Items.Add("正在获取列表，视网络情况可能需要数秒到数分钟，请耐心等待");
                thGet = new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(delegate
                {
                    HtmlWeb ForgePageGet = new HtmlWeb();
                    ForgePage = ForgePageGet.Load("http://files.minecraftforge.net/");
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { treeForgeVer.Items.Clear(); }));
                    GetForgeFinishDel GetForgePageFin = new GetForgeFinishDel(GetForgeFinish);
                    Dispatcher.Invoke(GetForgePageFin, ForgePage);
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { DownloadForge("Latest"); }));
                })));
                thGet.Start();
                return;
            }
            DownloadForge("Latest");
        }
        delegate void GetForgeFinishDel(HtmlDocument ForgePage);
        private void btnReForge_Click(object sender, RoutedEventArgs e)
        {
            HtmlDocument ForgePage;
            treeForgeVer.Items.Add("正在获取列表，视网络情况可能需要数秒到数分钟，请耐心等待");
            thGet=new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(delegate{
            HtmlWeb ForgePageGet = new HtmlWeb();
            ForgePage= ForgePageGet.Load("http://files.minecraftforge.net/");
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { treeForgeVer.Items.Clear(); }));
            GetForgeFinishDel GetForgePageFin = new GetForgeFinishDel(GetForgeFinish);
            Dispatcher.Invoke(GetForgePageFin, ForgePage);
            })));
            thGet.Start();
        }
        private void GetForgeFinish(HtmlDocument ForgePage)
        {
            HtmlNode promtions = ForgePage.GetElementbyId("promotions_table");
            HtmlNodeCollection shortcuts = promtions.SelectNodes("tr");
            TreeViewItem tree = new TreeViewItem();
            for (int i = 1; i < shortcuts.Count; i++)
            {
                HtmlNode shortcut = shortcuts[i];
                string ver = shortcut.SelectNodes("td")[0].InnerText;
                HtmlNodeCollection urls = shortcut.SelectNodes("td")[4].SelectNodes("a");
                string url = "none"; ;
                foreach (HtmlNode maybeurl in urls)
                {
                    string murl = maybeurl.GetAttributeValue("href","");
                    if (murl.IndexOf("adf.ly") == -1 && murl.IndexOf("installer")!=-1)
                    {
                        url = murl;
                        continue;
                    }
                }
                if (url == "none")
                {
                    continue;
                }
                DownloadUrl.Add(ver, url);
                tree = new TreeViewItem();
                tree.Header = ver;
                tree.Items.Add(shortcut.SelectNodes("td")[1].InnerText);
                treeForgeVer.Items.Add(tree);
            }
            HtmlNode all_builds = ForgePage.GetElementbyId("all_builds");
            HtmlNode Alltable = all_builds.SelectSingleNode("table");
            HtmlNodeCollection All = Alltable.SelectNodes("tr");
            tree = new TreeViewItem();
            tree.Header = All[1].SelectNodes("td")[1].InnerText;
            for (int i = 1; i < All.Count; i++)
            {
                HtmlNode shortcut = All[i];
                if (shortcut.SelectSingleNode("th") != null)
                {
                    treeForgeVer.Items.Add(tree);
                    tree = new TreeViewItem();
                    tree.Header = All[i + 1].SelectNodes("td")[1].InnerText;
                    continue;
                }
                string ver = shortcut.SelectNodes("td")[0].InnerText;
                HtmlNodeCollection urls = shortcut.SelectNodes("td")[3].SelectNodes("a");
                string url = "none"; ;
                foreach (HtmlNode maybeurl in urls)
                {
                    string murl = maybeurl.GetAttributeValue("href", "");
                    if (murl.IndexOf("adf.ly") == -1 && murl.IndexOf("installer") != -1)
                    {
                        url = murl;
                        continue;
                    }
                }
                if (url == "none")
                {
                    continue;
                }
                DownloadUrl.Add(ver, url);
                tree.Items.Add(ver);
            }
            treeForgeVer.Items.Add(tree);
        }
        private void DownloadForge(string ver)
        {
            if (!DownloadUrl.ContainsKey(ver))
            {
                MessageBox.Show("该版本不支持自动安装");
                return;
            }
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { gridDown.Visibility = Visibility.Visible; }));
            Uri url = new Uri(DownloadUrl[ver].ToString());
            WebClient downer = new WebClient();
            downer.DownloadProgressChanged+=downer_DownloadProgressChanged;
            downer.DownloadFileCompleted += downer_DownloadForgeCompleted;
            downedtime = Environment.TickCount - 1;
            downed = 0;
            StreamWriter w = new StreamWriter(Environment.CurrentDirectory + "\\.minecraft\\launcher_profiles.json");
            w.Write(Resource.NormalProfile.Profile);
            w.Close();
            downer.DownloadFileAsync(url, "forge.jar");
        }

        void downer_DownloadForgeCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtInsPath.Text);
                MessageBox.Show("接下来弹出来的窗口里请选择路径为启动器这里的.minecraft目录。程序已经将目录复制到了剪贴板，直接在窗口里选择浏览，粘贴路径，确定即可");
            }
            catch
            {
                MessageBox.Show("自动复制失败，请手动复制窗口里的安装路径，然后在安装窗口里粘贴路径即可");
            }
            Process ForgeIns = new Process();
            if (!File.Exists(cfg.javaw))
            {
                MessageBox.Show("请先去启动设置设置好java相关信息并保存");
                return;
            }
            ForgeIns.StartInfo.FileName = cfg.javaw;
            ForgeIns.StartInfo.Arguments = "-jar " + Environment.CurrentDirectory + "\\forge.jar";
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
                MessageBox.Show("复制安装路径成功");
            }
            catch
            {
                MessageBox.Show("自动复制失败，请手动复制窗口里的安装路径，然后在安装窗口里粘贴路径即可");
            }
        }
        #endregion


        private void ReFlushlistver()
        {
            listVer.Items.Clear();

                if (!Directory.Exists(".minecraft"))
                {
                    {
                        MessageBox.Show("无法找到游戏文件夹");
                        btnStart.IsEnabled = false;
                        btnDelete.IsEnabled = false;
                        btnModCfgMrg.IsEnabled = false;
                        btnModdirMrg.IsEnabled = false;
                        btnModMrg.IsEnabled = false;
                        btnCoreModMrg.IsEnabled = false;
                        return;
                    }
                }
                if (!Directory.Exists(@".minecraft\versions\"))
                {
                    MessageBox.Show("无法找到版本文件夹，本启动器只支持1.6以后的目录结构");
                    btnStart.IsEnabled      = false;
                    btnDelete.IsEnabled     = false;
                    btnModCfgMrg.IsEnabled  = false;
                    btnModdirMrg.IsEnabled  = false;
                    btnModMrg.IsEnabled     = false;
                    btnCoreModMrg.IsEnabled = false;
                    return;
                }
                DirectoryInfo mcdirinfo = new DirectoryInfo(".minecraft");
                DirectoryInfo[] versions = new DirectoryInfo(@".minecraft\versions").GetDirectories();
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
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 1;
            da.Duration = TimeSpan.FromSeconds(0.8);
            this.FrmMainWindow.BeginAnimation(Window.OpacityProperty, da);
            try
            {
                Random rand = new Random();
                int img = rand.Next(Directory.GetFiles(Environment.CurrentDirectory + "\\bg").Length);
                ImageBrush b = new ImageBrush();
                b.ImageSource = new BitmapImage(new Uri((Directory.GetFiles(Environment.CurrentDirectory + "\\bg")[img])));
                b.Stretch = Stretch.Fill;
                this.Top.Background = b;
            }
            catch
            {
                SolidColorBrush b=new SolidColorBrush(Color.FromRgb(255,255,255));
                this.Top.Background = b;
            }
            if (cfg.username == "!!!")
            {
                tabMain.SelectedIndex = 1;
                tip.IsOpen = true;
                txtUserName.Focus();
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
                case 2: gridRemoteVer.BeginAnimation(Grid.WidthProperty, da1); gridRemoteVer.BeginAnimation(Grid.HeightProperty, da2); break;
                case 3: gridForge.BeginAnimation(Grid.WidthProperty, da1); gridForge.BeginAnimation(Grid.HeightProperty, da2); break;
            }
        }



        private void FrmMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NIcon.Visible = false;
        }











    }
}
