using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using BMCLV2.JsonClass;
using BMCLV2.Launcher;
using BMCLV2.Resource;
using BMCLV2.util;

namespace BMCLV2.Forge
{
  internal class ForgeTask
  {
    public delegate void OnProcessChange(string status);

    private const string ForgeUrl = "https://bmclapi2.bangbang93.com/forge/promos";

    public Dictionary<string, string> ForgeChangeLogUrl = new Dictionary<string, string>();

    public event OnProcessChange ProcessChange = status => { };

    public async Task<ForgeVersion[]> GetVersion()
    {
      var downloader = new Downloader.Downloader();
      var json = await downloader.DownloadStringTaskAsync(ForgeUrl);
      var versions = new JSON<ForgeVersion[]>().Parse(json);
      if (versions != null)
        Logger.Info($"获取到{versions.Length}个forge版本");
      else
        Logger.Fatal("获取到0个forge版本");
      return versions;
    }

    public async Task DownloadForge(ForgeVersion forgeVersion)
    {
      var vanillaPath = BmclCore.BaseDirectory + ".minecraft\\versions\\" + forgeVersion.GetMc();
      if (!Directory.Exists(vanillaPath))
      {
        MessageBox.Show("请先下载原版");
        return;
      }

      var vanillaInfo = BmclCore.GameManager.GetVersion(forgeVersion.GetMc());

      ProcessChange("DownloadingForge");
      var url = forgeVersion.GetDownloadUrl();
      var downer = new Downloader.Downloader();
      FileHelper.WriteFile(Path.Combine(BmclCore.MinecraftDirectory, "launcher_profiles.json"), NormalProfile.Profile);

      var installerPath = Path.Combine(BmclCore.TempDirectory, "forge.jar");
      await downer.DownloadFileTaskAsync(url, installerPath);

      ProcessChange("InstallingForge");
      var v = int.Parse(forgeVersion.build.version.Split('.')[0]);
      try
      {
        if (v >= 25)
        {
          var installer = new ForgeInstaller(Path.Combine(BmclCore.MinecraftDirectory), forgeVersion.GetMc());
          installer.ProgressChange += status => ProcessChange(status);
          await installer.Run(installerPath, vanillaInfo);
        }
        else
        {
          InstallForge(forgeVersion, installerPath);
        }
      }
      catch (Exception ex)
      {
        Logger.Fatal("内置forge安装器出错");
        Logger.Fatal(ex);
        Logger.Info("将使用传统forge安装器");
        await InstallForgeInOldWay(installerPath);
        Logger.Info("已使用传统forge安装器成功安装");
      }
    }

    private async Task InstallForgeInOldWay(string installerPath)
    {
      var cp = new ChildProcess(BmclCore.Config.Javaw, new[] { "-jar", installerPath });
      cp.Start();
      await cp.WaitForExitAsync();
    }

    private void InstallForge(ForgeVersion forgeVersion, string installerPath)
    {
      //将installer中的forge universal提取出来
      var tempDir = Path.Combine(Path.GetTempPath(), "BMCL\\ForgeInstaller");
      if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
      var archive = new ZipArchive(new FileStream(installerPath, FileMode.Open));
      archive.ExtractToDirectory(tempDir);

      //获得universal的完整名称
      var tempFolder = new DirectoryInfo(tempDir);
      if (!tempFolder.Exists) tempFolder.Create();
      var tempFiles = tempFolder.GetFiles("*.jar");
      if (tempFiles.Length == 0) //除非下载过来的内容错误，不然installer中一定包含universal
        throw new Exception("cannot find universal.jar");
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
    }
  }
}
