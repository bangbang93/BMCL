using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using BMCLV2.Auth;
using BMCLV2.Exceptions;
using BMCLV2.JsonClass;
using BMCLV2.Plugin;
using BMCLV2.util;

namespace BMCLV2.Game
{
    public class GameManager
    {
        public static readonly string VersionDirectory = Path.Combine(BmclCore.BaseDirectory, @".minecraft\versions");
        private readonly Dictionary<string, VersionInfo> _versions = new Dictionary<string, VersionInfo>();
        private Launcher.Launcher _launcher;
        private readonly string[] _inheritFields = {"Type", "MinecraftArguments", "MainClass", "Assets", "Jar", "AssetIndex"};
        private AssetManager _assetManager;

        public bool IsGameRunning => _launcher == null;

        public GameManager()
        {
            ReloadList();
        }

        public void ReloadList()
        {
            _versions.Clear();
            FileHelper.CreateDirectoryIfNotExist(VersionDirectory);
            var dirs = new List<string>(Directory.GetDirectories(VersionDirectory));
            var jsonFiles = new List<string>();
            dirs.ForEach(dir => jsonFiles.AddRange(Directory.GetFiles(dir).Where(file => file.EndsWith(".json"))));
            var jsonParser = new JSON<VersionInfo>();
            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    using (var jsonStream = new FileStream(jsonFile, FileMode.Open))
                    {
                        var info = jsonParser.Parse(jsonStream);
                        if (info != null)
                        {
                            var id = PathHelper.GetDirectoryName(jsonFile);
                            _versions.Add(_versions.ContainsKey(id) ? $"{id}({jsonFile})" : id, info);
                        }
                        jsonStream.Close();
                    }
                }
                catch (SerializationException ex)
                {
                    Logger.Log($"无法加载{jsonFile}");
                    Logger.Log(ex);
                }
            }
            // I miss js
            foreach (var version in _versions.Values)
            {
                if (version.InheritsFrom == null) continue;
                var inherbits = GetVersion(version.InheritsFrom);
                if (inherbits == null) continue;
                foreach (var field in _inheritFields.Where(field => version.GetType().GetField(field).GetValue(version) == null))
                {
                    version.GetType().GetField(field).SetValue(version, inherbits.GetType().GetField(field).GetValue(inherbits));
                }
                version.Libraries = version.Libraries.Concat(inherbits.Libraries).ToArray();
            }
        }

        public Dictionary<string, VersionInfo> GetVersions()
        {
            return _versions.Where(pair => pair.Value.Type != "hidden")
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public VersionInfo GetVersion(string id)
        {
          return _versions.ContainsKey(id) ? _versions[id] : null;
        }

        public async Task<Launcher.Launcher> LaunchGame(string id, bool offline = true)
        {
            AuthResult authResult;

            if (offline)
                authResult = new AuthResult(BmclCore.Config);
            else
            {
                authResult = await BmclCore.AuthManager.Login(BmclCore.Config.Username, BmclCore.Config.GetPassword());
                if (!authResult.IsSuccess)
                {
                    var authname = "BMCL";
                    if (BmclCore.AuthManager.GetCurrectAuth() is IBmclAuthPlugin)
                    {
                        var plugin = BmclCore.AuthManager.GetCurrectAuth() as IBmclAuthPlugin;
                        authname = plugin?.GetName();
                    }
                    MessageBox.Show(BmclCore.MainWindow, authResult.Message, authname, MessageBoxButton.OK);
                    return null;
                }
            }
            if (_launcher != null) throw new AnotherGameRunningException(_launcher);
            var game = GetVersion(id);
            if (game == null) throw new NoSuchVersionException(id);
            _launcher = new Launcher.Launcher(game, authResult, BmclCore.Config);
            _launcher.OnLaunchError += (launcher, exception) => _launcher = null;
            _launcher.OnGameExit += (sender, info, exitcode) => _launcher = null;
            return _launcher;
        }

        private async void LauncherOnGameStart(object sender, VersionInfo versionInfo)
        {
            _assetManager = new AssetManager(versionInfo);
            await _assetManager.Sync();
            //TODO 弹窗
            BmclCore.Notify("资源文件同步完成");
        }

        private void registerWatcher()
        {
            
        }
    }
}
