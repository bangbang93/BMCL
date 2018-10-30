using System.IO;

namespace BMCLV2.util
{
    public class PathHelper
    {
        public static string VersionFile(string id, string suffix)
        {
            return System.IO.Path.Combine(BmclCore.BaseDirectory, ".minecraft", "versions", id, $"{id}.{suffix}");
        }

        public static string GetDirectoryName(string file)
        {
            return Path.GetFileName(Path.GetDirectoryName(file));
        }
    }
}