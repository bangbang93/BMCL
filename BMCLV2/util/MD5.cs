using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BMCLV2.util
{
    class MD5
    {
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
    }
}
