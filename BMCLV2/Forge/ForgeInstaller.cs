using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BMCLV2.Game;
using BMCLV2.JsonClass;
using BMCLV2.Launcher;
using BMCLV2.Properties;
using BMCLV2.util;

namespace BMCLV2.Forge
{
  public class ForgeInstaller
  {
    public delegate void OnProgressChange(string status);

    private readonly string _mcVersion;

    private readonly string _path;

    public ForgeInstaller(string path, string mcVersion)
    {
      _path = path;
      _mcVersion = mcVersion;
    }

    public event OnProgressChange ProgressChange = status => { };

    public async Task Run(string installerPath, VersionInfo vanillaInfo)
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
        await BmclCore.MirrorManager.CurrentMirror.Library.DownloadLibrary(jsonLibrary,
          Path.Combine(libraryPath, jsonLibrary.GetLibraryPath()));
      }

      entry = archive.GetEntry("install_profile.json");
      if (entry == null) throw new Exception("cannot find install_profile.json");
      var profileJson = new JSON<InstallerProfileScheme>().Parse(entry.Open());

      foreach (var profileJsonLibrary in profileJson.Libraries)
      {
        if (profileJsonLibrary.IsVaildLibrary(libraryPath)) continue;
        ProgressChange("{DownloadingLibrary} " + profileJsonLibrary.Name);
        await BmclCore.MirrorManager.CurrentMirror.Library.DownloadLibrary(profileJsonLibrary,
          Path.Combine(libraryPath, profileJsonLibrary.GetLibraryPath()));
      }

      ProgressChange("{DownloadingLibrary} processor data");
      if (profileJson.Data.ContainsKey("MOJMAPS"))
      {
        var dataItem = profileJson.Data["MOJMAPS"];
        var regex = new Regex("^\\[(.*)]$");
        var match = regex.Match(dataItem.Client);
        if (dataItem.Client != null && match.Success)
        {
          var lib = match.Groups[1].Value;
          if (vanillaInfo.Downloads.ClientMappings != null)
          {
            var data = await BmclCore.MirrorManager.CurrentMirror.Version.DownloadJson(vanillaInfo.Downloads
              .ClientMappings.Url);
            FileHelper.WriteFile(Path.Combine(BmclCore.LibrariesDirectory,
              PathHelper.ParseJavaLibraryNameToPath(lib)), data);
          }
        }
      }

      var buffer = Resources.forge_install_bootstrapper;
      // var stream = Application.GetResourceStream(new Uri("pack://application:,,,/forge_installer"));
      var installerHelperPath = Path.Combine(BmclCore.TempDirectory, "forge-installer-helper.jar");
      File.WriteAllBytes(installerHelperPath, buffer);

      var arguments = new List<string>();
      arguments.AddRange(new[]
        { "-cp", $"{installerHelperPath};{installerPath}", "com.bangbang93.ForgeInstaller", _path });

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
