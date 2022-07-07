using System.IO;

namespace BMCLV2.util
{
  internal static class FileHelper
  {
    public static void CopyDir(string from, string to)
    {
      var dir = new DirectoryInfo(from);
      if (!Directory.Exists(to)) Directory.CreateDirectory(to);
      foreach (var sondir in dir.GetDirectories()) CopyDir(sondir.FullName, to + "\\" + sondir.Name);
      foreach (var file in dir.GetFiles()) File.Copy(file.FullName, to + "\\" + file.Name, true);
    }

    public static bool IfFileVaild(string path, long length = -1)
    {
      if (!File.Exists(path)) return false;
      if (new FileInfo(path).Length == 0) return false;
      if (length != -1)
        if (new FileInfo(path).Length != length)
          return false;
      return true;
    }

    public static void CreateDirectoryIfNotExist(string dir)
    {
      if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
    }

    public static void CreateDirectoryForFile(string file)
    {
      CreateDirectoryIfNotExist(Path.GetDirectoryName(file));
    }

    public static void WriteFile(string path, string content)
    {
      CreateDirectoryForFile(path);
      File.WriteAllText(path, content);
    }

    public static void WriteFile(string path, byte[] content)
    {
      CreateDirectoryForFile(path);
      File.WriteAllBytes(path, content);
    }
  }
}
