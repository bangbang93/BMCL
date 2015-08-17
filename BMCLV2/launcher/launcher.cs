using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using BMCLV2.Lang;
using BMCLV2.libraries;
using BMCLV2.Login;
using BMCLV2.util;
using ICSharpCode.SharpZipLib.Zip;

namespace BMCLV2.Launcher
{
    public sealed class Launcher
    {


        #region 属性
        private readonly Process _game = new Process();
        private readonly string _javaxmx = "";
        private readonly string _username = "";
        private readonly string _version;
        private readonly string _name;
        private readonly gameinfo _info;
        private readonly long _timestamp = DateTime.Now.Ticks;
        private readonly string _urlLib = BmclCore.UrlLibrariesBase;
        public int Downloading = 0;
        private readonly WebClient _downer = new WebClient();
        StreamReader _gameoutput;
        StreamReader _gameerror;
        Thread _thError;
        Thread _thOutput;
        private LoginInfo _li;
        public string[] Extarg;
        
        #endregion

        #region 委托

        public delegate void GameExitEvent();
        public delegate void StateChangeEventHandler(string state);
        public delegate void GameStartUpEventHandler(bool success);
        #endregion


        #region 事件
        public event GameExitEvent Gameexit;
        public event StateChangeEventHandler StateChangeEvent;
        public event GameStartUpEventHandler GameStartUp;

        private void OnGameStartUp(bool success)
        {
            GameStartUpEventHandler handler = GameStartUp;
            if (handler != null) BmclCore.Invoke(new Action(() => handler(success)));
        }

        private void OnStateChangeEvent(string state)
        {
            var handler = StateChangeEvent;
            if (handler != null) BmclCore.Invoke(new Action(() => handler(state)));
        }

        #endregion


        #region 方法

        /// <summary>
        /// 初始化启动器
        /// </summary>
        /// <param name="javaPath"></param>
        /// <param name="javaXmx"></param>
        /// <param name="userName"></param>
        /// <param name="name"></param>
        /// <param name="info"></param>
        /// <param name="extarg"></param>
        /// <param name="li"></param>
        public Launcher(string javaPath, string javaXmx, string userName, string name, gameinfo info, string[] extarg, LoginInfo li)
        {
            OnStateChangeEvent(LangManager.GetLangFromResource("LauncherCheckJava"));
            if (!File.Exists(javaPath))
            {
                BMCLV2.Logger.log("找不到java",BMCLV2.Logger.LogType.Error);
                throw new NoJavaException();
            }
            OnStateChangeEvent(LangManager.GetLangFromResource("LauncherCheckMem"));
            _javaxmx = javaXmx;
            _username = userName;
            _version = info.id;
            this._name = name;
            _game.StartInfo.FileName = javaPath;
            if (BMCLV2.Logger.debug)
            {
                _game.StartInfo.CreateNoWindow = true;
                _game.StartInfo.RedirectStandardOutput = true;
                _game.StartInfo.RedirectStandardError = true;
            }
            _info = info;
            this._li = li;
            this.Extarg = extarg;
            this._info = info;
        }



        /// <summary>
        /// 释放依赖并运行游戏
        /// </summary>
        /// <returns>true:成功运行；false:失败</returns>
        public void Start()
        {
            var thread = new Thread(Run);
            thread.Start();
        }

        private void Run()
        {
            _game.StartInfo.UseShellExecute = false;
            OnStateChangeEvent(LangManager.GetLangFromResource("LauncherSettingupEnvoriement"));
            var arg = new StringBuilder("-Xincgc -Xmx");
            arg.Append(_javaxmx);
            arg.Append("M ");
            arg.Append(_solveArgs(Extarg));
            arg.Append(" ");
            arg.Append("-Djava.library.path=\"");
            arg.Append(Environment.CurrentDirectory).Append(@"\.minecraft\versions\");
            arg.Append(_name).Append("\\").Append(_version).Append("-natives-").Append(_timestamp.ToString(CultureInfo.InvariantCulture));
            arg.Append("\" -cp \"");
            foreach (libraryies lib in _info.libraries)
            {
                if (lib.natives != null)
                {
                    continue;
                }
                if (lib.rules != null)
                {
                    bool goflag = false;
                    foreach (rules rule in lib.rules)
                    {
                        if (rule.action == "disallow")
                        {
                            if (rule.os == null)
                            {
                                goflag = false;
                                break;
                            }
                            if (rule.os.name.ToLower().Trim() == "windows")
                            {
                                goflag = false;
                                break;
                            }
                        }
                        {
                            if (rule.os == null)
                            {
                                goflag = true;
                                break;
                            }
                            if (rule.os.name.ToLower().Trim() == "windows")
                            {
                                goflag = true;
                                break;
                            }
                        }
                    }
                    if (!goflag)
                    {
                        continue;
                    }
                }
                OnStateChangeEvent(LangManager.GetLangFromResource("LauncherSolveLib") + lib.name);
                string libp = BuildLibPath(lib);
                if (GetFileLength(libp) == 0)
                {
                    BMCLV2.Logger.log("未找到依赖" + lib.name + "开始下载", BMCLV2.Logger.LogType.Error);
                    try
                    {
                        if (lib.url == null)
                        {
                            OnStateChangeEvent(LangManager.GetLangFromResource("LauncherDownloadLib") + lib.name);
                            Downloading++;
                            if (!Directory.Exists(Path.GetDirectoryName(libp)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(libp));
                            }
#if DEBUG
                            System.Windows.MessageBox.Show(_urlLib + libp.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("\\", "/"));
#endif
                            BMCLV2.Logger.log(_urlLib + libp.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("\\", "/"));
                            _downer.DownloadFile(
                                _urlLib +
                                libp.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("/", "\\"), libp);
                            
                        }
                        else
                        {
                            string urlLib = lib.url;
                            OnStateChangeEvent(LangManager.GetLangFromResource("LauncherDownloadLib") + lib.name);
                            Downloading++;
                            /*
                            DownLib downer = new DownLib(lib);
                            downLibEvent(lib);
                            downer.DownFinEvent += downfin;
                            downer.startdownload();
                             */
                            if (!Directory.Exists(Path.GetDirectoryName(libp)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(libp));
                            }
#if DEBUG
                            System.Windows.MessageBox.Show(urlLib + libp.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("\\", "/"));
#endif
                            BMCLV2.Logger.log(urlLib + libp.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("\\", "/"));
                                _downer.DownloadFile(urlLib + libp.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("\\", "/"), libp);
                        }
                    }
                    catch (WebException ex)
                    {
                        BMCLV2.Logger.log(ex);
                        BMCLV2.Logger.log("原地址下载失败，尝试作者源" + lib.name);
                        try
                        {
                            _downer.DownloadFile(Resource.Url.URL_DOWNLOAD_bangbang93 + "libraries/" + libp.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("/", "\\"), libp);
                        }
                        catch (WebException exception)
                        {
                            BmclCore.Invoke(new Action(() => MessageBox.Show(BmclCore.MainWindow, "下载" + lib.name + "遇到错误：" + exception.Message)));
                            return;
                        }
                    }
                }
                arg.Append(BuildLibPath(lib) + ";");
            }
            OnStateChangeEvent(LangManager.GetLangFromResource("LauncherBuildMCArg"));
            var mcpath = new StringBuilder(Environment.CurrentDirectory + @"\.minecraft\versions\");
            if(_info.jar == "" || _info.jar == null)
                mcpath.Append(_name).Append("\\").Append(_version).Append(".jar\" ");
            else
                mcpath.Append(_info.jar).Append("\\").Append(_info.jar).Append(".jar\" ");
            mcpath.Append(_info.mainClass);
            arg.Append(mcpath);
            //" --username ${auth_player_name} --session ${auth_session} --version ${version_name} --gameDir ${game_directory} --assetsDir ${game_assets}"
            var mcarg = new StringBuilder(_info.minecraftArguments);
            mcarg.Replace("${auth_player_name}", _username);
            mcarg.Replace("${version_name}", _version);
            mcarg.Replace("${game_directory}", @".");
            mcarg.Replace("${game_assets}", @"assets");
            mcarg.Replace("${assets_root}", @"assets");
            mcarg.Replace("${user_type}", "Legacy");
            mcarg.Replace("${assets_index_name}", _info.assets);
            if (!string.IsNullOrEmpty(_li.OutInfo))
            {
                string[] replace = _li.OutInfo.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string str in replace)
                {
                    int sp = str.IndexOf(":", System.StringComparison.Ordinal);
                    mcarg.Replace(str.Substring(0, sp), str.Substring(sp + 1));
                    mcarg = new StringBuilder(Microsoft.VisualBasic.Strings.Replace(mcarg.ToString(), str.Split(':')[0], str.Split(':')[1], 1, -1, Microsoft.VisualBasic.CompareMethod.Text));
                }
            }
            else
            {
                mcarg.Replace("{auth_session}", _li.SID);
            }
            mcarg.Replace("${auth_uuid}", BmclCore.Config.GUID);
            mcarg.Replace("${auth_access_token}", BmclCore.Config.GUID);
            mcarg.Replace("${user_properties}", "{}");
            arg.Append(" ");
            arg.Append(mcarg);
            _game.StartInfo.Arguments = arg.ToString();
#if DEBUG
            System.Windows.MessageBox.Show(_game.StartInfo.Arguments);
#endif
            BMCLV2.Logger.log(_game.StartInfo.Arguments);
            OnStateChangeEvent(LangManager.GetLangFromResource("LauncherCreateNativeDir"));
            var nativePath = new StringBuilder(Environment.CurrentDirectory + @"\.minecraft\versions\");
            nativePath.Append(_name).Append("\\");
            var oldnative = new DirectoryInfo(nativePath.ToString());
            foreach (DirectoryInfo dir in oldnative.GetDirectories())
            {
                if (dir.FullName.Contains("-natives-"))
                {
                    Directory.Delete(dir.FullName, true);
                }
            }
            nativePath.Append(_version).Append("-natives-").Append(_timestamp);
            if (!Directory.Exists(nativePath.ToString()))
            {
                Directory.CreateDirectory(nativePath.ToString());
            }
            foreach (libraries.libraryies lib in _info.libraries)
            {
                if (lib.natives == null)
                    continue;
                if (lib.rules != null)
                {
                    bool goflag = false;
                    foreach (rules rule in lib.rules)
                    {
                        if (rule.action == "disallow")
                        {
                            if (rule.os == null)
                            {
                                goflag = false;
                                break;
                            }
                            if (rule.os.name.ToLower().Trim() == "windows")
                            {
                                goflag = false;
                                break;
                            }
                        }
                        {
                            if (rule.os == null)
                            {
                                goflag = true;
                                break;
                            }
                            if (rule.os.name.ToLower().Trim() == "windows")
                            {
                                goflag = true;
                                break;
                            }
                        }
                    }
                    if (!goflag)
                    {
                        continue;
                    }
                }
                OnStateChangeEvent(LangManager.GetLangFromResource("LauncherUnpackNative") + lib.name);
                string[] split = lib.name.Split(':');//0 包;1 名字；2 版本
                if (split.Count() != 3)
                {
                    throw new UnSupportVersionException();
                }
                string libp = BuildNativePath(lib);
                if (GetFileLength(libp) == 0)
                {
                    {
                        BMCLV2.Logger.log("未找到依赖" + lib.name + "开始下载", BMCLV2.Logger.LogType.Error);
                        if (lib.url == null)
                        {
                            try
                            {
                                OnStateChangeEvent(LangManager.GetLangFromResource("LauncherDownloadLib") + lib.name);
                                string nativep = BuildNativePath(lib);
                                if (!Directory.Exists(Path.GetDirectoryName(nativep)))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(nativep));
                                }
#if DEBUG
                                System.Windows.MessageBox.Show(_urlLib + nativep.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("\\", "/"));
                                BMCLV2.Logger.log(_urlLib + nativep.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("\\", "/"));
#endif
                                _downer.DownloadFile(_urlLib + nativep.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("\\", "/"), nativep);
                            }
                            catch (WebException ex)
                            {
                                BMCLV2.Logger.log(ex);
                                BMCLV2.Logger.log("原地址下载失败，尝试作者源" + lib.name);
                                string nativep = BuildLibPath(lib);
                                try
                                {
                                    _downer.DownloadFile(
                                        Resource.Url.URL_DOWNLOAD_bangbang93 + "libraries/" +
                                        nativep.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("/", "\\"),
                                        nativep);
                                }
                                catch (WebException exception)
                                {
                                    BmclCore.Invoke(new Action(() => MessageBox.Show(BmclCore.MainWindow, "下载" + lib.name + "遇到错误：" + exception.Message)));
                                    return;
                                }
                                
                            }
                        }
                        else
                        {
                            try
                            {
                                string urlLib = lib.url;
                                OnStateChangeEvent(LangManager.GetLangFromResource("LauncherDownloadLib") + lib.name);
                                /*
                                DownNative downer = new DownNative(lib);
                                downNativeEvent(lib);
                                downer.startdownload();
                                 */
                                string nativep = BuildNativePath(lib);
                                if (!Directory.Exists(Path.GetDirectoryName(nativep)))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(nativep));
                                }
#if DEBUG
                                System.Windows.MessageBox.Show(urlLib.Replace("\\", "/"));
                                BMCLV2.Logger.log(urlLib.Replace("\\", "/"));
#endif
                                _downer.DownloadFile(urlLib + nativep.Replace("/", "\\"), nativep);
                            }
                            catch (WebException ex)
                            {
                                BMCLV2.Logger.log(ex);
                                BMCLV2.Logger.log("原地址下载失败，尝试作者源" + lib.name);
                                string nativep = BuildLibPath(lib);
                                _downer.DownloadFile(Resource.Url.URL_DOWNLOAD_bangbang93 + "libraries/" + nativep.Replace("/", "\\"), nativep);
                            }
                        }
                    }
                }
                BMCLV2.Logger.log("解压native");
                var zipfile = new ZipInputStream(System.IO.File.OpenRead(libp));
                ZipEntry theEntry;
                while ((theEntry = zipfile.GetNextEntry()) != null)
                {
                    bool exc = false;
                    if (lib.extract.exclude != null)
                    {
                        if (lib.extract.exclude.Any(excfile => theEntry.Name.Contains(excfile)))
                        {
                            exc = true;
                        }
                    }
                    if (exc) continue;
                    var filepath = new StringBuilder(nativePath.ToString());
                    filepath.Append("\\").Append(theEntry.Name);
                    BMCLV2.Logger.log(filepath.ToString());
                    FileStream fileWriter = File.Create(filepath.ToString());
                    var data = new byte[2048];
                    while (true)
                    {
                        int size = zipfile.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            fileWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                    fileWriter.Close();

                }
            }
            OnStateChangeEvent(LangManager.GetLangFromResource("LauncherSolveMod"));
            BMCLV2.Logger.log("处理Mods");
            if (Directory.Exists(@".minecraft\versions\" + _name + @"\mods"))
            {
                if (Directory.Exists(@".minecraft\Config"))
                {
                    BMCLV2.Logger.log("找到旧的配置文件，备份并应用新配置文件");
                    Directory.Move(@".minecraft\Config", @".minecraft\Config" + _timestamp);
                    if (Directory.Exists(@".minecraft\versions\" + _name + @"\Config"))
                        FileHelper.dircopy(@".minecraft\versions\" + _name + @"\Config", @".minecraft\Config");
                }
                else
                {
                    BMCLV2.Logger.log("应用新配置文件");
                    if (Directory.Exists(@".minecraft\versions\" + _name + @"\Config"))
                        FileHelper.dircopy(@".minecraft\versions\" + _name + @"\Config", @".minecraft\Config");
                }
                if (Directory.Exists(@".minecraft\mods"))
                {
                    BMCLV2.Logger.log("找到旧的mod文件，备份并应用新mod文件");
                    Directory.Move(@".minecraft\mods", @".minecraft\mods" + _timestamp);
                    if (Directory.Exists(@".minecraft\versions\" + _name + @"\mods"))
                        FileHelper.dircopy(@".minecraft\versions\" + _name + @"\mods", @".minecraft\mods");
                }
                else
                {
                    BMCLV2.Logger.log("应用新mod文件");
                    if (Directory.Exists(@".minecraft\versions\" + _name + @"\mods"))
                        FileHelper.dircopy(@".minecraft\versions\" + _name + @"\mods", @".minecraft\mods");
                }
                if (Directory.Exists(@".minecraft\coremods"))
                {
                    BMCLV2.Logger.log("找到旧的coremod文件，备份并应用新coremod文件");
                    Directory.Move(@".minecraft\coremods", @".minecraft\coremods" + _timestamp);
                    if (Directory.Exists(@".minecraft\versions\" + _name + @"\coremods"))
                        FileHelper.dircopy(@".minecraft\versions\" + _name + @"\coremods", @".minecraft\coremods");
                }
                else
                {
                    BMCLV2.Logger.log("应用新coremod文件");
                    if (Directory.Exists(@".minecraft\versions\" + _name + @"\coremods"))
                        FileHelper.dircopy(@".minecraft\versions\" + _name + @"\coremods", @".minecraft\coremods");
                }
                if (Directory.Exists(@".minecraft\versions\" + _name + @"\moddir"))
                {
                    var moddirs = new DirectoryInfo(@".minecraft\versions\" + _name + @"\moddir");
                    foreach (DirectoryInfo moddir in moddirs.GetDirectories())
                    {
                        BMCLV2.Logger.log("复制ModDir " + moddir.Name);
                        FileHelper.dircopy(moddir.FullName, ".minecraft\\" + moddir.Name);
                    }
                    foreach (FileInfo modfile in moddirs.GetFiles())
                    {
                        BMCLV2.Logger.log("复制ModDir " + modfile.Name);
                        File.Copy(modfile.FullName, ".minecraft\\" + modfile.Name, true);
                    }
                }
            }

            OnStateChangeEvent(LangManager.GetLangFromResource("LauncherGo"));
            Environment.SetEnvironmentVariable("APPDATA", Environment.CurrentDirectory);
            _game.EnableRaisingEvents = true;
            _game.Exited += game_Exited;
            _game.StartInfo.WorkingDirectory = Environment.CurrentDirectory + "\\.minecraft";
            try
            {
                bool fin = _game.Start();
                if (BMCLV2.Logger.debug)
                {
                    _game.OutputDataReceived += _game_OutputDataReceived;
                    _game.ErrorDataReceived += _game_ErrorDataReceived;
                    _game.BeginOutputReadLine();
                    _game.BeginErrorReadLine();
                }
                OnGameStartUp(true);
            }
            catch
            {
                OnGameStartUp(false);
            }
        }

        void _game_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            BMCLV2.Logger.log(e.Data, BMCLV2.Logger.LogType.Fml);
        }

        void _game_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            BMCLV2.Logger.log(e.Data, BMCLV2.Logger.LogType.Game);
        }

        void game_Exited(object sender, EventArgs e)
        {
            if (_game.ExitCode != 0)
            {
#if DEBUG
                System.Windows.MessageBox.Show(_game.ExitCode.ToString(CultureInfo.InvariantCulture));
#endif
            }
            var nativePath = new StringBuilder(Environment.CurrentDirectory + @"\.minecraft\versions\");
            nativePath.Append(_name).Append("\\");
            var oldnative = new DirectoryInfo(nativePath.ToString());
            if (!BMCLV2.Logger.debug)
            {
                foreach (DirectoryInfo dir in oldnative.GetDirectories())
                {
                    if (dir.FullName.Contains("-natives-"))
                    {
                        Directory.Delete(dir.FullName, true);
                    }
                }
            }
            if (Directory.Exists(@".minecraft\versions\" + _name + @"\mods"))
            {
                Directory.Delete(@".minecraft\versions\" + _name + @"\mods", true);  
                FileHelper.dircopy(@".minecraft\mods", @".minecraft\versions\" + _name + @"\mods");
                Directory.Delete(@".minecraft\mods", true);
                Directory.Delete(@".minecraft\versions\" + _name + @"\coremods", true);
                FileHelper.dircopy(@".minecraft\coremods", @".minecraft\versions\" + _name + @"\coremods");
                Directory.Delete(@".minecraft\coremods", true);
                Directory.Delete(@".minecraft\versions\" + _name + @"\Config", true);
                FileHelper.dircopy(@".minecraft\Config", @".minecraft\versions\" + _name + @"\Config");
                Directory.Delete(@".minecraft\Config", true);
            }
            if (Directory.Exists(@".minecraft\versions\" + _name + @"\moddir"))
            {
                var moddirs = new DirectoryInfo(@".minecraft\versions\" + _name + @"\moddir");
                foreach (DirectoryInfo moddir in moddirs.GetDirectories())
                {
                    moddir.Delete(true);
                    FileHelper.dircopy(@".minecraft\" + moddir.Name, moddir.FullName);
                    Directory.Delete(@".minecraft\" + moddir.Name, true);
                }
            }
            if (BMCLV2.Logger.debug)
            {
//                _logthread.Abort();
//                _thError.Abort();
//                _thOutput.Abort();
//                _gameerror.Close();
//                _gameoutput.Close();
                _game.OutputDataReceived -= _game_OutputDataReceived;
                _game.ErrorDataReceived -= _game_ErrorDataReceived;
            }
            Gameexit();
        }

        /// <summary>
        /// 获取lib文件的绝对路径
        /// </summary>
        /// <param name="lib"></param>
        /// <returns></returns>
        public static string BuildLibPath(libraryies lib)
        {
            var libp = new StringBuilder(Environment.CurrentDirectory + @"\.minecraft\libraries\");
            string[] split = lib.name.Split(':');//0 包;1 名字；2 版本
            if (split.Count() != 3)
            {
                throw new UnSupportVersionException();
            }
            libp.Append(split[0].Replace('.', '\\'));
            libp.Append("\\");
            libp.Append(split[1]).Append("\\");
            libp.Append(split[2]).Append("\\");
            libp.Append(split[1]).Append("-");
            libp.Append(split[2]).Append(".jar");
            return libp.ToString();
        }

        /// <summary>
        /// 获取native文件的绝对路径
        /// </summary>
        /// <param name="lib"></param>
        /// <returns></returns>
        public static string BuildNativePath(libraryies lib)
        {
            var libp = new StringBuilder(Environment.CurrentDirectory + @"\.minecraft\libraries\");
            string[] split = lib.name.Split(':');//0 包;1 名字；2 版本
            libp.Append(split[0].Replace('.', '\\'));
            libp.Append("\\");
            libp.Append(split[1]).Append("\\");
            libp.Append(split[2]).Append("\\");
            libp.Append(split[1]).Append("-").Append(split[2]).Append("-").Append(lib.natives.windows);
            libp.Append(".jar");
            if (split[0] == "tv.twitch")
            {
                libp.Replace("${arch}", Environment.Is64BitOperatingSystem ? "64" : "32");
            }
            return libp.ToString();
        }

        public bool IsRunning()
        {
            return !_game.HasExited;
        }

        /// <summary>
        /// GetFileLength
        /// </summary>
        /// <param name="path"></param>
        /// <returns>FileLength,if file doesn't exist return 0</returns>
        private long GetFileLength(string path)
        {
            try
            {
                return (new FileInfo(path)).Length;
            }
            catch (IOException)
            {
                return 0;
            }
        }

        private string _solveArgs(string[] args)
        {
            var arg = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                if (i == 0 || args[i][0] == '"')
                {
                    arg.Append(args[i]).Append(' ');
                }
                else
                {
                    arg.Append('"').Append(args[i]).Append("\" ");
                }
            }
            return arg.ToString();
        }
        #endregion

    }
}
