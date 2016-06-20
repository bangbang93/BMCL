using System;
using System.Collections.Generic;
using BMCLV2.Util;

namespace BMCLV2.Auth
{
    public class AuthResult
    {
        public string Username;
        public string Uuid;
        public string AccessToken;
        public string SID;
        public bool IsSuccess;
        public string Message;
        public string ErrCode;
        public string OtherInfo;
        public string ClientIdentifier;
        public Dictionary<string, string> OutInfo;

        public AuthResult(string username, string uid = null, string clientIdentifier = null)
        {
            Username = username;
            Uuid = AccessToken = uid ?? Guid.Parse(Crypto.Md5("OfflinePlayer:" + username)).ToString();
            ClientIdentifier = clientIdentifier ?? Guid.NewGuid().ToString();
        }

        public AuthResult(Config config) : this(config.Username)
        {
        }
    }
}