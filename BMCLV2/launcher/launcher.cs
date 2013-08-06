using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net;

using BMCLV2.download;
using BMCLV2.libraries;
using BMCLV2.util;

using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace BMCLV2
{
    class launcher
    {

        #region 异常
        public static Exception NoJava = new Exception("找不到java");
        public static Exception NoRam = new Exception("没有足够物理内存");
        public static Exception NoMoreRam = new Exception("没有足够的可用内存");
        public static Exception UnSupportVer = new Exception("启动器不支持这个版本");
        public static Exception FailInLib = new Exception("无法获得所需的依赖");
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
        StreamWriter bmclgamelog;
        Thread logthread;
        
        #endregion

        #region 委托
        delegate void downThread();
        public delegate void downLibEventHandler(libraryies lib);
        public delegate void downNativeEventHalder(libraryies lib);
        public delegate void changeHandel(string status);
        public delegate void gameExitEvent();
        #endregion


        #region 事件
        public static event changeHandel changeEvent;
        public static event gameExitEvent gameexit;
        public static event downLibEventHandler downLibEvent;
        public static event downNativeEventHalder downNativeEvent;
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
        public launcher(string JavaPath, string JavaXmx, string UserName, string name, gameinfo info, string extarg, string session = "no")
        {
            changeEvent("检查Java");
            if (!File.Exists(JavaPath))
            {
                throw NoJava;
            }
            changeEvent("检查内存");
            if (Convert.ToUInt64(JavaXmx) < 512M)
            {
                throw NoRam;
            }
            java = JavaPath;
            javaxmx = JavaXmx;
            username = UserName;
            version = info.id;
            this.name = name;
            game.StartInfo.FileName = java;
            if (FrmMain.debug)
            {
                game.StartInfo.CreateNoWindow = true;
                game.StartInfo.RedirectStandardOutput = true;
                game.StartInfo.RedirectStandardError = true;
                bmclgamelog = new StreamWriter("bmclgame.log");
            }
            game.StartInfo.UseShellExecute = false;
            Info = info;
            changeEvent("设置环境变量");
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
                changeEvent("处理依赖" + lib.name);
                if (!File.Exists(buildLibPath(lib)))
                {
                    if (lib.url == null)
                    {
                        changeEvent("下载依赖" + lib.name);
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
                        downer.DownloadFile(urlLib + libp.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("/", "\\"), libp);
                    }
                    else
                    {
                        string urlLib = lib.url;
                        changeEvent("下载依赖" + lib.name);
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
                        downer.DownloadFile(urlLib + libp.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("/", "\\"), libp);
                    }
                }
                arg.Append(buildLibPath(lib) + ";");
            }
            changeEvent("传递MC参数");
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
            downLibEvent += launcher_downLibEvent;
        }

        void launcher_downLibEvent(libraryies lib)
        {
            DownLib downer = new DownLib(lib);
            downloading++;
        }

        /// <summary>
        /// 释放依赖并运行游戏
        /// </summary>
        /// <returns>true:成功运行；false:失败</returns>
        public bool start()
        {
            changeEvent("创建依赖文件夹");
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
                changeEvent("解压" + lib.name);
                string[] split = lib.name.Split(':');//0 包;1 名字；2 版本
                if (split.Count() != 3)
                {
                    throw UnSupportVer;
                }
                string libp = buildNativePath(lib);
                if (!File.Exists(libp))
                {
                    if (lib.url == null)
                    {
                        changeEvent("下载依赖" + lib.name);
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
                        System.Windows.MessageBox.Show(urlLib + nativep.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("\\", "/"));
#endif
                        downer.DownloadFile(urlLib + nativep.Remove(0, Environment.CurrentDirectory.Length + 22).Replace("/", "\\"), nativep);
                    }
                    else
                    {
                        string urlLib = lib.url;
                        changeEvent("下载依赖" + lib.name);
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
#endif
                        downer.DownloadFile(urlLib + nativep.Replace("/", "\\"), nativep);
                    }
                }
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
            changeEvent("处理mods");
            if (Directory.Exists(@".minecraft\versions\" + name + @"\mods"))
            {
                if (Directory.Exists(@".minecraft\config"))
                {
                    Directory.Move(@".minecraft\config", @".minecraft\config" + timestamp);
                    Dir.dircopy(@".minecraft\versions\" + name + @"\config", @".minecraft\config");
                }
                else
                    Dir.dircopy(@".minecraft\versions\" + name + @"\config", @".minecraft\config");
                if (Directory.Exists(@".minecraft\mods"))
                {

                    Directory.Move(@".minecraft\mods", @".minecraft\mods" + timestamp);
                    Dir.dircopy(@".minecraft\versions\" + name + @"\mods", @".minecraft\mods");
                }
                else
                    Dir.dircopy(@".minecraft\versions\" + name + @"\mods", @".minecraft\mods");
                if (Directory.Exists(@".minecraft\coremods"))
                {
                    Directory.Move(@".minecraft\coremods", @".minecraft\coremods" + timestamp);
                    Dir.dircopy(@".minecraft\versions\" + name + @"\coremods", @".minecraft\coremods");
                }
                else
                    Dir.dircopy(@".minecraft\versions\" + name + @"\coremods", @".minecraft\coremods");
                if (Directory.Exists(@".minecraft\versions\" + name + @"\moddir"))
                {
                    DirectoryInfo moddirs = new DirectoryInfo(@".minecraft\versions\" + name + @"\moddir");
                    foreach (DirectoryInfo moddir in moddirs.GetDirectories())
                    {
                        Dir.dircopy(moddir.FullName, ".minecraft\\" + moddir.Name);
                    }
                }
            }

            changeEvent("走你");
            //game.StartInfo.WorkingDirectory = Environment.CurrentDirectory + "\\.minecraft\\versions\\" + version;
            Environment.SetEnvironmentVariable("APPDATA", Environment.CurrentDirectory);
            game.EnableRaisingEvents = true;
            game.Exited += game_Exited;
            //System.Windows.Forms.MessageBox.Show(System.IO.File.Exists(game.StartInfo.FileName).ToString());
            //while (downloading > 0)
            {
                //Thread.Sleep(0);
            }
            try
            {
                bool fin = game.Start();
                gameoutput = game.StandardOutput;
                gameerror = game.StandardError;
                logthread = new Thread(new ThreadStart(logger));
                logthread.Start();
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
                Directory.Delete(@".minecraft\mods", true);
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
            if (FrmMain.debug)
            {
                bmclgamelog.Close();
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

        private void downfin()
        {
            downloading--;
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
            while (gameoutput.EndOfStream && gameerror.EndOfStream)
            {
                if (!gameoutput.EndOfStream)
                {
                    string line = gameoutput.ReadLine();
                    bmclgamelog.WriteLine(DateTime.Now.ToShortTimeString() + line);
                }
                if (!gameerror.EndOfStream)
                {
                    string line = gameerror.ReadLine();
                    bmclgamelog.WriteLine(DateTime.Now.ToShortTimeString() + line);
                }
            }
            bmclgamelog.Close();
            gameerror.Close();
            gameoutput.Close();
        }
        #endregion

    }
}
