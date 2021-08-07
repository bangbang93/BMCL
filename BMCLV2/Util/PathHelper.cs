using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BMCLV2.util
{
  public class PathHelper
  {
    public static string VersionFile(string id, string suffix)
    {
      return Path.Combine(BmclCore.BaseDirectory, ".minecraft", "versions", id, $"{id}.{suffix}");
    }

    public static string GetDirectoryName(string file)
    {
      return Path.GetFileName(Path.GetDirectoryName(file));
    }

    public static string ParseJavaLibraryNameToPath(string name)
    {
      var regex =
        new Regex("(?<groupId>[^:]+):(?<artifactId>[^:]+):(?<version>[^:]+):?(?<packaging>[^:@]+)?@?(?<ext>\\w+)?");
      var match = regex.Match(name);
      var libp = new StringBuilder();
      libp.Append(match.Groups["groupId"].Value.Replace('.', '\\')).Append("\\");
      libp.Append(match.Groups["artifactId"].Value).Append("\\");
      libp.Append(match.Groups["version"].Value).Append("\\");
      libp.Append(match.Groups["artifactId"].Value).Append("-");
      libp.Append(match.Groups["version"].Value);

      if (match.Groups["packaging"].Success) libp.Append("-").Append(match.Groups["packaging"].Value);

      libp.Append(".").Append(match.Groups["ext"]?.Value ?? "jar");
      return libp.ToString();
    }
  }
}
