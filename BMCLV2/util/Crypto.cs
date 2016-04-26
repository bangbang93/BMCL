using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BMCLV2.util
{
    class Crypto
    {
        public static string GetMd5HashFromFile(string filename)
        {
            var file = new FileStream(filename, FileMode.Open);
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var retVal = md5.ComputeHash(file);
            file.Close();

            var sb = new StringBuilder();
            foreach (var t in retVal)
            {
                sb.Append(t.ToString("x2"));
            }
            return sb.ToString();
        }

        public static string GetSha1HashFromFile(string filename)
        {
            var file = new FileStream(filename, FileMode.Open);
            var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            var retVal = sha1.ComputeHash(file);
            file.Close();

            var sb = new StringBuilder();
            foreach (var t in retVal)
            {
                sb.Append(t.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
