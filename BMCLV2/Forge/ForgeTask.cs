using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using BMCLV2.I18N;
using BMCLV2.JsonClass;
using ICSharpCode.SharpZipLib.Zip;
using System.Text.RegularExpressions;
using BMCLV2.util;
using System.Windows;

namespace BMCLV2.Forge
{
    class ForgeTask
    {
        private readonly string forgeUrl = "http://bmclapi2.bangbang93.com/forge/promos";
        public delegate void ForgePageReadyHandle();
        public event ForgePageReadyHandle ForgePageReadyEvent;
        private ForgeVersion[] _forgeNew;
        public Dictionary<string, string> ForgeDownloadUrl = new Dictionary<string, string>(), 
            ForgeChangeLogUrl = new Dictionary<string, string>();
        public async Task<ForgeVersion[]> GetVersion()
        {
            var downloder = new Downloader.Downloader();
            var json = await downloder.DownloadStringTaskAsync(forgeUrl);
            var versions = new JSON(typeof (ForgeVersion[])).Parse(json) as ForgeVersion[];
            if (versions != null)
                Logger.Info($"获取到{versions.Length}个forge版本");
            else 
                Logger.Fatal("获取到0个forge版本");
            return versions;
        }

        public async Task DownloadForge(ForgeVersion forgeVersion)
        {
            if (!Directory.Exists(BmclCore.BaseDirectory + ".minecraft\\versions\\" + forgeVersion.GetMc())) {
                MessageBox.Show("请先下载原版");
                return;
            }
            var url = forgeVersion.GetDownloadUrl();
            var downer = new Downloader.Downloader();
            var w = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\.minecraft\\launcher_profiles.json");
            w.Write(Resource.NormalProfile.Profile);
            w.Close();
            await downer.DownloadFileTaskAsync(url, "forge.jar");

            var stat = false;
            try {
                stat = InstallForge(forgeVersion);
            } catch (Exception ex) {
                Logger.Fatal("内置forge安装器出错："+ex.Message);
            }
            if (!stat) {
                Logger.Info("将使用传统forge安装器");
                InstallForgeInOldWay();
            } else {
                Logger.Info("已使用内置forge安装器成功安装");
            }
        }

        public void InstallForgeInOldWay()
        {
            var forgeIns = new Process
            {
                StartInfo =
                {
                    FileName = BmclCore.Config.Javaw,
                    Arguments = "-jar \"" + BmclCore.BaseDirectory + "\\forge.jar\""
                }
            };
            Logger.Log(forgeIns.StartInfo.Arguments);
            forgeIns.Start();
            forgeIns.WaitForExit();
        }
 
        public bool InstallForge(ForgeVersion forgeVersion)
        {
            //将installer中的forge universal提取出来
            string tempDir = Path.Combine(BmclCore.BaseDirectory, "temp");
            new FastZip().ExtractZip(Path.Combine(BmclCore.BaseDirectory, "forge.jar"), tempDir, "\\w*\\.jar");

            //获得universal的完整名称
            DirectoryInfo tempFolder = new DirectoryInfo(tempDir);
            FileInfo[] tempFiles = tempFolder.GetFiles("*.jar");
            if (tempFiles.Length == 0)//除非下载过来的内容错误，不然installer中一定包含universal
                return false;
            string forge = tempFiles[0].Name;

            //再从universal中提出version.json
            new FastZip().ExtractZip(tempDir + "\\"+forge, tempDir, "version\\.json");

            //从version.json中获得目标游戏版本名，并在versions文件夹中创建
            string forge0 = gameinfo.Read(tempDir + "\\version.json").id;
            string versionFolder = BmclCore.BaseDirectory + ".minecraft\\versions\\" + forge0;
            Directory.CreateDirectory(versionFolder);

            //复制json与核心文件
            File.Copy(tempDir + "\\version.json", versionFolder + "\\"+ forgeVersion.GetMc() + "-" + forge0 +".json");
            File.Copy(versionFolder + "\\..\\" + forgeVersion.GetMc() + "\\" + forgeVersion.GetMc() + ".jar", versionFolder + "\\" +  forge0 + ".jar");

            //复制forge到libraries中
            forge0 = Regex.Replace(forge0.ToLower(), forgeVersion.GetMc() + "-forge", "");
            string forgeFolder = BmclCore.BaseDirectory + ".minecraft\\libraries\\net\\minecraftforge\\forge\\" + forge0;
            Directory.CreateDirectory(forgeFolder);
            File.Copy(tempDir + "\\"+forge, forgeFolder + "\\forge-" + forge0 + ".jar");

            return true;
        }
    }
}
