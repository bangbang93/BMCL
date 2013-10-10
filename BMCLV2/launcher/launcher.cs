using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net;

using BMCLV2.libraries;
using BMCLV2.util;
using BMCLV2.Lang;

using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace BMCLV2
{
    class launcher
    {

        #region 异常
        public static Exception NoJava = new Exception(LangManager.GetLangFromResource("NoJavaException"));
        public static Exception NoRam = new Exception(LangManager.GetLangFromResource("NoEnoughMemException"));
        public static Exception UnSupportVer = new Exception(LangManager.GetLangFromResource("UnSupportVersionExcepton"));
        public static Exception FailInLib = new Exception(LangManager.GetLangFromResource("FailInLibException"));
        #endregion


        #region 属性
        private Process game = new Process();
        private string java = "";
        private string javaxmx = "";
        private string username = "";
        private string version;
        private string name;
        private gameinfo Info;
        long timestamp = DateTime.Now.Ticks;
        private string urlLib = FrmMain.URL_DOWNLOAD_BASE + "libraries/";
        public int downloading = 0;
        WebClient downer = new WebClient();
        StreamReader gameoutput;
        StreamReader gameerror;
        Thread logthread;
        Thread thError;
        Thread thOutput;
        FrmPrs prs;
        
        #endregion

        #region 委托
        delegate void downThread();
        public delegate void gameExitEvent();
        #endregion


        #region 事件
        public static event gameExitEvent gameexit;
        #endregion


        #region 方法
        /// <summary>
        /// 初始化启动器
        /// </summary>
        /// <param name="JavaPath"></param>
        /// <param name="JavaXmx"></param>
        /// <param name="UserName"></param>
        /// <param name="ver"></param>
        /// <param name="info"></param>
        /// <param name="session"></param>
        public launcher(string JavaPath, string JavaXmx, string UserName, string name, gameinfo info, string extarg, ref FrmPrs prs, string session = "no")
        {
            this.prs = prs;
            prs.changeEventH(LangManager.GetLangFromResource("LauncherCheckJava"));
            if (!File.Exists(JavaPath))
            {
                Logger.Log("找不到java",Logger.LogType.Error);
                throw NoJava;
            }
            prs.changeEventH(LangManager.GetLangFromResource("LauncherCheckMem"));
            if (Convert.ToUInt64(JavaXmx) < 0)
            {
                Logger.Log("可用内存过小" + JavaXmx, Logger.LogType.Error);
                throw NoRam;
            }
            java = JavaPath;
            javaxmx = JavaXmx;
            username = UserName;
            version = info.id;
            this.name = name;
            game.StartInfo.FileName = java;
            if (Logger.Debug)
            {
                game.StartInfo.CreateNoWindow = true;
                game.StartInfo.RedirectStandardOutput = true;
                game.StartInfo.RedirectStandardError = true;
            }
            game.StartInfo.UseShellExecute = false;
            Info = info;
            prs.changeEventH(LangManager.GetLangFromResource("LauncherSettingupEnvoriement"));
            StringBuilder arg = new StringBuilder("-Xincgc -Xmx");
            arg.Append(javaxmx);
            arg.Append("M ");
            arg.Append(extarg);
            arg.Append(" ");
            arg.Append("-Djava.library.path=\"");
            arg.Append(Environment.CurrentDirectory).Append(@"\.minecraft\versions\");
            arg.Append(name).Append("\\").Append(version).Append("-natives-").Append(timestamp.ToString());
            arg.Append("\" -cp \"");
            foreach (libraries.libraryies lib in info.libraries)
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
                prs.changeEventH(LangManager.GetLangFromResource("LauncherSolveLib") + lib.name);
                if (!File.Exists(buildLibPath(lib)))
                {
                    Logger.Log("未找到依赖" + lib.name + "开始下载", Logger.LogType.Error);
                    if (lib.url == null)
                    {
                        prs.changeEventH(LangManager.GetLangFromResource("LauncherDownloadLib") + lib.name);
                        downloading++;
                        string libp = buildLibPath(lib);
                        if (!Directory.Exists(Path.GetDirectoryName(libp)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(libp));
                        }
#if DEBUG
                        System.Windows.MessageBox.Show(urlLib + libp.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("\\", "/"));
#endif
                        Logger.Log(urlLib + libp.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("\\", "/"), Logger.LogType.Info);
                        downer.DownloadFile(urlLib + libp.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("/", "\\"), libp);
                    }
                    else
                    {
                        string urlLib = lib.url;
                        prs.changeEventH(LangManager.GetLangFromResource("LauncherDownloadLib") + lib.name);
                        downloading++;
                        /*
                        DownLib downer = new DownLib(lib);
                        downLibEvent(lib);
                        downer.DownFinEvent += downfin;
                        downer.startdownload();
                         */
                        string libp = buildLibPath(lib);
                        if (!Directory.Exists(Path.GetDirectoryName(libp)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(libp));
                        }
#if DEBUG
                        System.Windows.MessageBox.Show(urlLib + libp.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("\\", "/"));
#endif
                        Logger.Log(urlLib + libp.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("\\", "/"), Logger.LogType.Info);
                        downer.DownloadFile(urlLib + libp.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("/", "\\"), libp);
                    }
                }
                arg.Append(buildLibPath(lib) + ";");
            }
            prs.changeEventH(LangManager.GetLangFromResource("LauncherBuildMCArg"));
            StringBuilder mcpath = new StringBuilder(Environment.CurrentDirectory + @"\.minecraft\versions\");
            mcpath.Append(name).Append("\\").Append(version).Append(".jar\" ");
            mcpath.Append(info.mainClass);
            arg.Append(mcpath);
            StringBuilder mcarg;
            //" --username ${auth_player_name} --session ${auth_session} --version ${version_name} --gameDir ${game_directory} --assetsDir ${game_assets}"
            mcarg = new StringBuilder(info.minecraftArguments);
            mcarg.Replace("${auth_player_name}", username);
            mcarg.Replace("${auth_session}", session);
            mcarg.Replace("${version_name}", version);
            mcarg.Replace("${game_directory}", ".minecraft");
            mcarg.Replace("${game_assets}", @".minecraft\assets");
            arg.Append(" ");
            arg.Append(mcarg);
            game.StartInfo.Arguments = arg.ToString();
#if DEBUG
            System.Windows.MessageBox.Show(game.StartInfo.Arguments);
#endif
            Logger.Log(game.StartInfo.Arguments, Logger.LogType.Info);
        }


        /// <summary>
        /// 释放依赖并运行游戏
        /// </summary>
        /// <returns>true:成功运行；false:失败</returns>
        public bool start()
        {
            prs.changeEventH(LangManager.GetLangFromResource("LauncherCreateNativeDir"));
            StringBuilder NativePath = new StringBuilder(Environment.CurrentDirectory + @"\.minecraft\versions\");
            NativePath.Append(name).Append("\\");
            DirectoryInfo oldnative = new DirectoryInfo(NativePath.ToString());
            foreach (DirectoryInfo dir in oldnative.GetDirectories())
            {
                if (dir.FullName.Contains("-natives-"))
                {
                    try
                    {
                        Directory.Delete(dir.FullName, true);
                    }
                    catch { }
                }
            }
            NativePath.Append(version).Append("-natives-").Append(timestamp);
            if (!Directory.Exists(NativePath.ToString()))
            {
                Directory.CreateDirectory(NativePath.ToString());
            }
            foreach (libraries.libraryies lib in Info.libraries)
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
                prs.changeEventH(LangManager.GetLangFromResource("LauncherUnpackNative") + lib.name);
                string[] split = lib.name.Split(':');//0 包;1 名字；2 版本
                if (split.Count() != 3)
                {
                    throw UnSupportVer;
                }
                string libp = buildNativePath(lib);
                if (!File.Exists(libp) && File.OpenRead(libp).Length!=0)
                {
                    Logger.Log("未找到依赖" + lib.name + "开始下载", Logger.LogType.Error);
                    if (lib.url == null)
                    {
                        prs.changeEventH(LangManager.GetLangFromResource("LauncherDownloadLib") + lib.name);
                        string nativep = buildNativePath(lib);
                        if (!Directory.Exists(Path.GetDirectoryName(nativep)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(nativep));
                        }
#if DEBUG
                        System.Windows.MessageBox.Show(urlLib + nativep.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("\\", "/"));
                        Logger.Log(urlLib + nativep.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("\\", "/"), Logger.LogType.Info);
#endif
                        downer.DownloadFile(urlLib + nativep.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("/", "\\"), nativep);
                    }
                    else
                    {
                        string urlLib = lib.url;
                        prs.changeEventH(LangManager.GetLangFromResource("LauncherDownloadLib") + lib.name);
                        /*
                        DownNative downer = new DownNative(lib);
                        downNativeEvent(lib);
                        downer.startdownload();
                         */
                        string nativep = buildNativePath(lib);
                        if (!Directory.Exists(Path.GetDirectoryName(nativep)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(nativep));
                        }
#if DEBUG
                        System.Windows.MessageBox.Show(urlLib.Replace("\\", "/"));
                        Logger.Log(urlLib.Replace("\\", "/"), Logger.LogType.Info);
#endif
                        downer.DownloadFile(urlLib + nativep.Replace("/", "\\"), nativep);
                    }
                }
                Logger.Log("解压native", Logger.LogType.Info);
                ZipInputStream zipfile = new ZipInputStream(System.IO.File.OpenRead(libp.ToString()));
                ZipEntry theEntry;
                while ((theEntry = zipfile.GetNextEntry()) != null)
                {
                    bool exc = false;
                    if (lib.extract.exclude != null)
                    {
                        foreach (string excfile in lib.extract.exclude)
                        {
                            if (theEntry.Name.Contains(excfile))
                            {
                                exc = true;
                                break;
                            }
                        }
                    }
                    if (exc) continue;
                    StringBuilder filepath = new StringBuilder(NativePath.ToString());
                    filepath.Append("\\").Append(theEntry.Name);
                    FileStream fileWriter = File.Create(filepath.ToString());
                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = zipfile.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            fileWriter.Write(data, 0, data.Length);
                        }
                        else
                        {
                            break;
                        }
                    }
                    fileWriter.Close();

                }
            }
            prs.changeEventH(LangManager.GetLangFromResource("LauncherSolveMod"));
            Logger.Log("处理Mods", Logger.LogType.Info);
            if (Directory.Exists(@".minecraft\versions\" + name + @"\mods"))
            {
                if (Directory.Exists(@".minecraft\config"))
                {
                    Logger.Log("找到旧的配置文件，备份并应用新配置文件", Logger.LogType.Info);
                    Directory.Move(@".minecraft\config", @".minecraft\config" + timestamp);
                    Dir.dircopy(@".minecraft\versions\" + name + @"\config", @".minecraft\config");
                }
                else
                {
                    Logger.Log("应用新配置文件", Logger.LogType.Info);
                    Dir.dircopy(@".minecraft\versions\" + name + @"\config", @".minecraft\config");
                }
                if (Directory.Exists(@".minecraft\mods"))
                {
                    Logger.Log("找到旧的mod文件，备份并应用新mod文件", Logger.LogType.Info);
                    Directory.Move(@".minecraft\mods", @".minecraft\mods" + timestamp);
                    Dir.dircopy(@".minecraft\versions\" + name + @"\mods", @".minecraft\mods");
                }
                else
                {
                    Logger.Log("应用新mod文件", Logger.LogType.Info);
                    Dir.dircopy(@".minecraft\versions\" + name + @"\mods", @".minecraft\mods");
                }
                if (Directory.Exists(@".minecraft\coremods"))
                {
                    Logger.Log("找到旧的coremod文件，备份并应用新coremod文件", Logger.LogType.Info);
                    Directory.Move(@".minecraft\coremods", @".minecraft\coremods" + timestamp);
                    Dir.dircopy(@".minecraft\versions\" + name + @"\coremods", @".minecraft\coremods");
                }
                else
                {
                    Logger.Log("应用新coremod文件", Logger.LogType.Info);
                    Dir.dircopy(@".minecraft\versions\" + name + @"\coremods", @".minecraft\coremods");
                }
                if (Directory.Exists(@".minecraft\versions\" + name + @"\moddir"))
                {
                    DirectoryInfo moddirs = new DirectoryInfo(@".minecraft\versions\" + name + @"\moddir");
                    foreach (DirectoryInfo moddir in moddirs.GetDirectories())
                    {
                        Logger.Log("复制ModDir " + moddir.Name, Logger.LogType.Info);
                        Dir.dircopy(moddir.FullName, ".minecraft\\" + moddir.Name);
                    }
                    foreach (FileInfo modfile in moddirs.GetFiles())
                    {
                        Logger.Log("复制ModDir " + modfile.Name, Logger.LogType.Info);
                        File.Copy(modfile.FullName, ".minecraft\\" + modfile.Name, true);
                    }
                }
            }

            prs.changeEventH(LangManager.GetLangFromResource("LauncherGo"));
            //game.StartInfo.WorkingDirectory = Environment.CurrentDirectory + "\\.minecraft\\versions\\" + version;
            Environment.SetEnvironmentVariable("APPDATA", Environment.CurrentDirectory);
            game.EnableRaisingEvents = true;
            game.Exited += game_Exited;
            try
            {
                bool fin = game.Start();
                if (Logger.Debug)
                {
                    gameoutput = game.StandardOutput;
                    gameerror = game.StandardError;
                    logthread = new Thread(new ThreadStart(logger));
                    logthread.Start();
                }
                return fin;
            }
            catch
            {
                return false;
            }
        }

        void game_Exited(object sender, EventArgs e)
        {
            if (game.ExitCode != 0)
            {
#if DEBUG
                System.Windows.MessageBox.Show(game.ExitCode.ToString());
#endif
            }
            StringBuilder NativePath = new StringBuilder(Environment.CurrentDirectory + @"\.minecraft\versions\");
            NativePath.Append(name).Append("\\");
            DirectoryInfo oldnative = new DirectoryInfo(NativePath.ToString());
            foreach (DirectoryInfo dir in oldnative.GetDirectories())
            {
                if (dir.FullName.Contains("-natives-"))
                {
                    try
                    {
                        Directory.Delete(dir.FullName, true);
                    }
                    catch { }
                }
            }
            if (Directory.Exists(@".minecraft\versions\" + name + @"\mods"))
            {
                Directory.Delete(@".minecraft\versions\" + name + @"\mods", true);  
                Dir.dircopy(@".minecraft\mods", @".minecraft\versions\" + name + @"\mods");
                Directory.Delete(@".minecraft\mods", true);
                Directory.Delete(@".minecraft\versions\" + name + @"\coremods", true);
                Dir.dircopy(@".minecraft\coremods", @".minecraft\versions\" + name + @"\coremods");
                Directory.Delete(@".minecraft\coremods", true);
                Directory.Delete(@".minecraft\versions\" + name + @"\config", true);
                Dir.dircopy(@".minecraft\config", @".minecraft\versions\" + name + @"\config");
                Directory.Delete(@".minecraft\config", true);
            }
            if (Directory.Exists(@".minecraft\versions\" + name + @"\moddir"))
            {
                DirectoryInfo moddirs = new DirectoryInfo(@".minecraft\versions\" + name + @"\moddir");
                foreach (DirectoryInfo moddir in moddirs.GetDirectories())
                {
                    moddir.Delete(true);
                    Dir.dircopy(@".minecraft\" + moddir.Name, moddir.FullName);
                    Directory.Delete(@".minecraft\" + moddir.Name, true);
                }
            }
            if (Logger.Debug)
            {
                logthread.Abort();
                thError.Abort();
                thOutput.Abort();
                gameerror.Close();
                gameoutput.Close();
            }
            gameexit();
        }

        /// <summary>
        /// 获取lib文件的绝对路径
        /// </summary>
        /// <param name="lib"></param>
        /// <returns></returns>
        public static string buildLibPath(libraryies lib)
        {
            StringBuilder libp = new StringBuilder(Environment.CurrentDirectory + @"\.minecraft\libraries\");
            string[] split = lib.name.Split(':');//0 包;1 名字；2 版本
            if (split.Count() != 3)
            {
                throw UnSupportVer;
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
        public static string buildNativePath(libraryies lib)
        {
            StringBuilder libp = new StringBuilder(Environment.CurrentDirectory + @"\.minecraft\libraries\");
            string[] split = lib.name.Split(':');//0 包;1 名字；2 版本
            libp.Append(split[0].Replace('.', '\\'));
            libp.Append("\\");
            libp.Append(split[1]).Append("\\");
            libp.Append(split[2]).Append("\\");
            libp.Append(split[1]).Append("-").Append(split[2]).Append("-").Append(lib.natives.windows);
            libp.Append(".jar");
            return libp.ToString();
        }

        public bool IsRunning()
        {
            try
            {
                if (game.Id != -1)
                    return true;
                else
                    return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private void logger()
        {
            thOutput = new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(delegate
            {
                while (true)
                {
                    try
                    {
                        if (!gameoutput.EndOfStream)
                        {
                            string line = gameoutput.ReadLine();
                            Logger.Log(line, Logger.LogType.Game);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("获取游戏输出失败:" + ex.Message, Logger.LogType.Error);
                    }
                }
            })));
            thError = new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(delegate
            {
                while (true)
                {
                    try
                    {
                        if (!gameerror.EndOfStream)
                        {
                            string line = gameerror.ReadLine();
                            Logger.Log(line, Logger.LogType.Fml);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("获取FML输出失败:" + ex.Message, Logger.LogType.Error);
                    }
                }
            })));
            thOutput.IsBackground = true;
            thError.IsBackground = true;
            thOutput.Start();
            thError.Start();

        }
        #endregion

    }
}
