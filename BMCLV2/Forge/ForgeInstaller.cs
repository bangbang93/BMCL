using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using BMCLV2.Game;
using BMCLV2.JsonClass;
using BMCLV2.Launcher;
using BMCLV2.Properties;

namespace BMCLV2.Forge
{
  public class ForgeInstaller
  {
    private string _path;

    public delegate void OnProgressChange(string status);
    public event OnProgressChange ProgressChange = status => { };

    public ForgeInstaller(string path)
    {
      _path = path;
    }

    public async Task Run(string installerPath)
    {
      var libraryPath = Path.Combine(BmclCore.MinecraftDirectory, "libraries");
      var archive = new ZipArchive(new FileStream(installerPath, FileMode.Open));

      var entry = archive.GetEntry("version.json");
      if (entry == null) throw new Exception("cannot find version.json");

      var versionJson = new JSON<VersionInfo>().Parse(entry.Open());

      foreach (var jsonLibrary in versionJson.Libraries)
      {
        if (jsonLibrary.Name.StartsWith("net.minecraftforge:forge:")) continue;
        if (jsonLibrary.IsVaildLibrary(libraryPath)) continue;
        ProgressChange("{DownloadingLibrary} " + jsonLibrary.Name);
        await BmclCore.MirrorManager.CurrectMirror.Library.DownloadLibrary(jsonLibrary, Path.Combine(libraryPath, jsonLibrary.GetLibraryPath()));
      }

      entry = archive.GetEntry("install_profile.json");
      if (entry == null) throw new Exception("cannot find install_profile.json");
      var profileJson = new JSON<InstallerProfileScheme>().Parse(entry.Open());

      foreach (var profileJsonLibrary in profileJson.Libraries)
      {
        if (profileJsonLibrary.IsVaildLibrary(libraryPath)) continue;
        ProgressChange("{DownloadingLibrary} " + profileJsonLibrary.Name);
        await BmclCore.MirrorManager.CurrectMirror.Library.DownloadLibrary(profileJsonLibrary, Path.Combine(libraryPath, profileJsonLibrary.GetLibraryPath()));
      }

      var buffer = Resources.forge_installer;
      // var stream = Application.GetResourceStream(new Uri("pack://application:,,,/forge_installer"));
      var installerHelperPath = Path.Combine(BmclCore.TempDirectory, "forge-installer-helper.jar");
      var fs = new FileStream(installerHelperPath, FileMode.Create);
      await fs.WriteAsync(buffer, 0, buffer.Length);
      fs.Close();

      var arguments = new List<string>();
      arguments.AddRange(new[]
        {"-cp", $"{installerHelperPath};{installerPath}", "com.bangbang93.ForgeInstaller", _path});

      var cp = new ChildProcess(BmclCore.Config.Javaw, arguments.ToArray());
      cp.Start();

      cp.OnStdOut += (sender, log) =>
      {
        ProgressChange(log);
        Logger.Info(log);
      };
      cp.OnStdErr += (sender, log) => Logger.Fatal(log);

      await cp.WaitForExitAsync();
    }
  }
}
