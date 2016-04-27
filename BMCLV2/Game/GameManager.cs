using System.Collections.Generic;
using System.IO;
using System.Linq;
using BMCLV2.Exceptions;
using BMCLV2.JsonClass;

namespace BMCLV2.Game
{
    public class GameManager
    {
        public static readonly string VersionDirectory = Path.Combine(BmclCore.BaseDirectory, @".minecraft\versions");
        private Dictionary<string, VersionInfo> _versions = new Dictionary<string, VersionInfo>();
        private Launcher.Launcher _launcher;
        private readonly string[] _inheritFields = new[] {"Type", "MinecraftArguments", "MainClass", "Assets", "Jar"};

        public bool IsGameRunning => _launcher == null;

        public GameManager()
        {
            ReloadList();
        }

        public void ReloadList()
        {
            _versions.Clear();
            var dirs = new List<string>(Directory.GetDirectories(VersionDirectory));
            var jsonFiles = new List<string>();
            dirs.ForEach(dir => jsonFiles.AddRange(Directory.GetFiles(dir).Where(file => file.EndsWith(".json"))));
            var jsonParser = new JSON(typeof(VersionInfo));
            foreach (var jsonFile in jsonFiles)
            {
                using (var jsonStream = new FileStream(jsonFile, FileMode.Open))
                {
                    var info = jsonParser.Parse(jsonStream) as VersionInfo;
                    if (info != null)
                    {
                        _versions.Add(info.Id, info);
                    }
                    jsonStream.Close();
                }
            }
            foreach (var version in _versions.Values)
            {
                if (version.InheritsFrom == null) continue;
                var inherbits = _versions[version.InheritsFrom];
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
            return _versions;
        }

        public VersionInfo GetVersion(string id)
        {
            return _versions[id];
        }

        public bool LaunchGame(string id)
        {
            if (_launcher != null) throw new AnotherGameRunningException(_launcher);
            var game = GetVersion(id);
            if (game == null) throw new NoSuchVersionException(id);
            _launcher = new Launcher.Launcher(game, BmclCore.Config);
            _launcher.Start();
            _launcher.OnGameExit += (sender, info, exitcode) => _launcher = null;
            return true;
        }
    }
}