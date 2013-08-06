using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BMCLV2.libraries;
using System.IO;

namespace BMCLV2.download
{
    class DownNative
    {
        libraryies lib;
        private string urlLib = FrmMain.URL_DOWNLOAD_BASE + "/libraries/";

        public delegate void changeHandel(string status);
        private delegate void downThread();

        public static Exception NoJava = new Exception("找不到java");
        public static Exception NoRam = new Exception("没有足够物理内存");
        public static Exception NoMoreRam = new Exception("没有足够的可用内存");
        public static Exception UnSupportVer = new Exception("启动器不支持这个版本");
        public static Exception FailInLib = new Exception("无法获得所需的依赖");

        public DownNative(libraryies Lib)
        {
            lib = Lib;
            launcher.downNativeEvent += launcher_downNativeEvent;
        }

        void launcher_downNativeEvent(libraryies lib)
        {
            startdownload();
        }
        public void startdownload()
        {
            string libp = buildNativePath(lib);
            downloader downer = new downloader(urlLib + buildNativePath(lib).Remove(0, Environment.CurrentDirectory.Length + 1));
            downer.Filename = buildNativePath(lib);
            downer.Start();
        }

        /// <summary>
        /// 获取native文件相对路径
        /// </summary>
        /// <param name="lib"></param>
        /// <returns></returns>
        public string buildNativePath(libraryies lib)
        {
            StringBuilder libp = new StringBuilder(@".minecraft\libraries\");
            string[] split = lib.name.Split(':');//0 包;1 名字；2 版本
            libp.Append(split[0].Replace('.', '\\'));
            libp.Append("\\");
            libp.Append(split[1]).Append("\\");
            libp.Append(split[2]).Append("\\");
            libp.Append(split[1]).Append("-").Append(split[2]).Append("-natives-windows");
            libp.Append(".jar");
            return libp.ToString();
        }
    }
}
