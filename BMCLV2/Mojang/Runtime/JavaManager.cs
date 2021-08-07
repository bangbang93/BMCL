using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BMCLV2.Downloader;
using BMCLV2.Launcher;

namespace BMCLV2.Mojang.Runtime
{
  public class JavaManager
  {
    public string RuntimePath { private set; get; }
    public string ExecutablePath { private set; get; }

    private readonly string _requiredVersion;

    public JavaManager(string requiredVersion)
    {
      _requiredVersion = requiredVersion;
    }

    public async Task<DownloadInfo[]> EnsureJava()
    {
      Directory.CreateDirectory(BmclCore.RuntimeDirectory);
      var meta = await new JavaMeta().FetchMeta();
      var java = meta[BmclCore.Platform];
      if (java == null) throw new UnSupportVersionException($"{BmclCore.Platform} unsupported");

      var javaMeta = java[_requiredVersion];
      if (javaMeta == null) throw new UnauthorizedAccessException($"{_requiredVersion} not found");

      RuntimePath = Path.Combine(BmclCore.RuntimeDirectory, _requiredVersion, BmclCore.Platform, _requiredVersion);
      Directory.CreateDirectory(RuntimePath);

      var manifest = await new JavaManifest(javaMeta[0].Manifest.Url).FetchManifest();
      var downloadInfos = new List<DownloadInfo>();
      foreach (var file in manifest.Files)
      {
        var path = Path.Combine(RuntimePath, file.Key);
        if (file.Value.Type == "directory")
        {
          Directory.CreateDirectory(path);
        }
        else
        {
          var download = file.Value.Downloads["raw"];
          download.Url = BmclCore.MirrorManager.CurrentMirror.Version.GetUrl(download.Url);
          downloadInfos.Add(new DownloadInfo(path, download));
        }
      }

      var executableName = BmclCore.OS == "windows" ? "javaw.exe" : "java";

      ExecutablePath = Path.Combine(RuntimePath, "bin", executableName);

      return downloadInfos.ToArray();
    }
  }
}
