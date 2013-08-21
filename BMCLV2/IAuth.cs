using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCAuth
{
    public interface IAuth
    {
        string GetName(string Language = "zh-cn");
        long GetVer();
        LoginInfo Login(string UserName, string Password, string Client_identifier = "", string Language = "zh-cn");
    }
    public struct LoginInfo
    {
        public string UN;
        public string UID;
        public string SID;
        public bool Suc;
        public string Errinfo;
        public string OtherInfo;
        public string Client_identifier;
    }
}
