using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using BMCLV2.JsonClass;
using BMCLV2.Resource;
using Version = BMCLV2.Downloader.Version;

namespace BMCLV2.Forge
{
  internal class ForgeTask
  {
    public delegate void OnProcessChange(string status);

    private const string ForgeUrl = "http://bmclapi2.bangbang93.com/forge/promos";

    public Dictionary<string, string> ForgeChangeLogUrl = new Dictionary<string, string>();

    public event OnProcessChange ProcessChange = status => { };

    public async Task<ForgeVersion[]> GetVersion()
    {
      var downloder = new Downloader.Downloader();
      var json = await downloder.DownloadStringTaskAsync(ForgeUrl);
      var versions = new JSON<ForgeVersion[]>().Parse(json);
      if (versions != null)
        Logger.Info($"获取到{versions.Length}个forge版本");
      else
        Logger.Fatal("获取到0个forge版本");
      return versions;
    }

    public async Task DownloadForge(ForgeVersion forgeVersion)
    {
      if (!Directory.Exists(BmclCore.BaseDirectory + ".minecraft\\versions\\" + forgeVersion.GetMc()))
      {
        MessageBox.Show("请先下载原版");
        return;
      }

      ProcessChange("DownloadingForge");
      var url = forgeVersion.GetDownloadUrl();
      var downer = new Downloader.Downloader();
      var w = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\.minecraft\\launcher_profiles.json");
      w.Write(NormalProfile.Profile);
      w.Close();
      var installerPath = Path.Combine(BmclCore.TempDirectory, "forge.jar");
      await downer.DownloadFileTaskAsync(url, installerPath);

      ProcessChange("InstallingForge");
      var v = int.Parse(forgeVersion.build.version.Split('.')[0]);
      if (v >= 25)
      {
        var installer = new ForgeInstaller(Path.Combine(BmclCore.MinecraftDirectory));
        installer.ProgressChange += status => ProcessChange(status);
        await installer.Run(installerPath);
      }
      else
      {
        var stat = false;
        try
        {
          stat = InstallForge(forgeVersion, installerPath);
        }
        catch (Exception ex)
        {
          Logger.Fatal("内置forge安装器出错");
          Logger.Fatal(ex);
        }

        if (!stat)
        {
          Logger.Info("将使用传统forge安装器");
          InstallForgeInOldWay();
        }
        else
        {
          Logger.Info("已使用内置forge安装器成功安装");
        }
      }
    }

    public void InstallForgeInOldWay()
    {
      var forgeIns = new Process
      {
        StartInfo =
        {
          FileName = BmclCore.Config.Javaw,
          Arguments = "-jar \"" + BmclCore.TempDirectory + "\\forge.jar\""
        }
      };
      Logger.Log(forgeIns.StartInfo.Arguments);
      forgeIns.Start();
      forgeIns.WaitForExit();
    }

    public bool InstallForge(ForgeVersion forgeVersion, string installerPath)
    {
      //将installer中的forge universal提取出来
      var tempDir = Path.Combine(Path.GetTempPath(), "BMCL\\ForgeInstaller");
      if (Directory.Exists(tempDir))
      {
        Directory.Delete(tempDir, true);
      }
      var archive = new ZipArchive(new FileStream(installerPath, FileMode.Open));
      archive.ExtractToDirectory(tempDir);

      //获得universal的完整名称
      var tempFolder = new DirectoryInfo(tempDir);
      if (!tempFolder.Exists) tempFolder.Create();
      var tempFiles = tempFolder.GetFiles("*.jar");
      if (tempFiles.Length == 0) //除非下载过来的内容错误，不然installer中一定包含universal
        return false;
      var forge = tempFiles[0].Name;

      archive.Dispose();
      //再从universal中提出version.json
      archive = new ZipArchive(new FileStream(tempDir + "\\" + forge, FileMode.Open));
      archive.GetEntry("version.json").ExtractToFile(Path.Combine(tempDir, "version.json"));

      //从version.json中获得目标游戏版本名，并在versions文件夹中创建
      var forge0 = gameinfo.Read(tempDir + "\\version.json").id;
      var versionFolder = BmclCore.BaseDirectory + ".minecraft\\versions\\" + forge0;
      Directory.CreateDirectory(versionFolder);

      //复制json与核心文件
      File.Copy(tempDir + "\\version.json", versionFolder + "\\" + forgeVersion.GetMc() + "-" + forge0 + ".json");
      File.Copy(versionFolder + "\\..\\" + forgeVersion.GetMc() + "\\" + forgeVersion.GetMc() + ".jar",
        versionFolder + "\\" + forge0 + ".jar");

      //复制forge到libraries中
      forge0 = Regex.Replace(forge0.ToLower(), forgeVersion.GetMc() + "-forge", "");
      var forgeFolder = BmclCore.BaseDirectory + ".minecraft\\libraries\\net\\minecraftforge\\forge\\" + forge0;
      Directory.CreateDirectory(forgeFolder);
      File.Copy(tempDir + "\\" + forge, forgeFolder + "\\forge-" + forge0 + ".jar");

      archive.Dispose();
      Directory.Delete(tempDir, true);

      return true;
    }
  }
}
