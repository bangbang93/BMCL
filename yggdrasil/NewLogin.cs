using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections;
using System.IO;
using System.Web;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace yggdrasil
{
    public class NewLogin
    {
        private string BaseUrl = "https://authserver.mojang.com/";
        private string RouteAuthenticate = "https://authserver.mojang.com/authenticate";
        private string RouteRefresh = "https://authserver.mojang.com/refresh";
        private string RouteValidate = "https://authserver.mojang.com/validate";
        private string RouteInvalidate = "https://authserver.mojang.com/invalidate";
        private string RouteSignout = "https://authserver.mojang.com/signout";
        private static string ClientToken;

        public LoginInfo Login(string UserName, string Password, string Client_identifier = "", string Language = "zh-cn")
        {
            LoginInfo LI = new LoginInfo();
            ClientToken = Client_identifier;
            try
            {
                HttpWebRequest auth = (HttpWebRequest)WebRequest.Create(RouteAuthenticate);
                auth.Method = "POST";
                AuthenticationRequest ag = new AuthenticationRequest(UserName, Password);
                DataContractJsonSerializer agJsonSerialiaer = new DataContractJsonSerializer(typeof(AuthenticationRequest));
                MemoryStream agJsonStream = new MemoryStream();
                agJsonSerialiaer.WriteObject(agJsonStream, ag);
                agJsonStream.Position = 0;
                string logindata = (new StreamReader(agJsonStream)).ReadToEnd();
                byte[] postdata = Encoding.UTF8.GetBytes(logindata);
                auth.ContentLength = postdata.LongLength;
                Stream poststream = auth.GetRequestStream();
                poststream.Write(postdata, 0, postdata.Length);
                poststream.Close();
                HttpWebResponse authans = (HttpWebResponse)auth.GetResponse();
                DataContractJsonSerializer ResponseJsonSerializer = new DataContractJsonSerializer(typeof(AuthenticationResponse));
                StreamReader ResponseStream = new StreamReader(authans.GetResponseStream());
                string ResponseJson = ResponseStream.ReadToEnd();
                MemoryStream ResponseJsonStream = new MemoryStream(Encoding.UTF8.GetBytes(ResponseJson));
                ResponseJsonStream.Position = 0;
                AuthenticationResponse Response = ResponseJsonSerializer.ReadObject(ResponseJsonStream) as AuthenticationResponse;
                if (Response.getClientToken() != NewLogin.ClientToken)
                {
                    LI.Suc = false;
                    LI.Errinfo = "客户端标识和服务器返回不符，这是个不常见的错误，就算是正版启动器这里也没做任何处理，只是报了这么个错。";
                    return LI;
                }
                LI.Suc = true;
                LI.UN = Response.getSelectedProfile().getName();
                LI.Client_identifier = NewLogin.ClientToken;
                DataContractSerializer OtherInfoSerializer = new DataContractSerializer(typeof(SortedList));
                SortedList OtherInfoList = new SortedList();
                OtherInfoList.Add("${auth_uuid}",Response.getSelectedProfile().getId());
                OtherInfoList.Add("${auth_access_token}", Response.getAccessToken());
                MemoryStream OtherInfoStream = new MemoryStream();
                OtherInfoSerializer.WriteObject(OtherInfoStream, OtherInfoList);
                OtherInfoStream.Position = 0;
                LI.OtherInfo = (new StreamReader(OtherInfoStream)).ReadToEnd();
                return LI;
            }
            catch (TimeoutException ex)
            {
                LI.Suc = false;
                LI.Errinfo = ex.Message;
                return LI;
            }
        }
        public long GetVer()
        {
            return 1;
            //代表为第一代标准化登陆插件
        }
        public string GetName(string Language = "zh-cn")
        {
            return "1.7新正版";
        }
        public static string GetClientToken()
        {
            return ClientToken;
        }

        private static String ParsToString(Hashtable Pars)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string k in Pars.Keys)
            {
                if (sb.Length > 0)
                {
                    sb.Append("&");
                }
                sb.Append(HttpUtility.UrlEncode(k) + "=" + HttpUtility.UrlEncode(Pars[k].ToString()));
            }
            return sb.ToString();
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
}
