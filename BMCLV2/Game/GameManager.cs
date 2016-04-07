using System.Collections.Generic;
using System.IO;
using System.Linq;
using BMCLV2.JsonClass;

namespace BMCLV2.Game
{
    public class GameManager
    {
        public static readonly string VersionDirectory = Path.Combine(BmclCore.BaseDirectory, @".minecraft\versions");
        private Dictionary<string, VersionInfo> _versions = new Dictionary<string, VersionInfo>();

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
            jsonFiles.ForEach((jsonFile) =>
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
            });
        }

        public Dictionary<string, VersionInfo> GetVersions()
        {
            return _versions;
        } 
    }
}