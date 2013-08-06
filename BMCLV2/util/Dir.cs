using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BMCLV2.util
{
    static class Dir
    {
        static public void dircopy(string from, string to)
        {
            DirectoryInfo dir = new DirectoryInfo(from);
            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }
            foreach (DirectoryInfo sondir in dir.GetDirectories())
            {
                dircopy(sondir.FullName, to + "\\" + sondir.Name);
            }
            foreach (FileInfo file in dir.GetFiles())
            {
                File.Delete(to + "\\" + file.Name);
                File.Copy(file.FullName, to + "\\" + file.Name);
            }
        }
    }
}
