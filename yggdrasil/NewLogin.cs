using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections;
using System.IO;
using System.Web;
using System.Runtime.Serialization.Json;
using BMCLV2.Plugin;

namespace yggdrasil
{
    public class NewLogin : IBmclAuthPlugin
    {
// ReSharper disable UnusedMember.Local
        private const string BaseUrl = "https://authserver.mojang.com/";
        private const string RouteAuthenticate = "https://authserver.mojang.com/authenticate";
        private const string RouteRefresh = "https://authserver.mojang.com/refresh";
        private const string RouteValidate = "https://authserver.mojang.com/validate";
        private const string RouteInvalidate = "https://authserver.mojang.com/invalidate";
        private const string RouteSignout = "https://authserver.mojang.com/signout";
        // ReSharper restore UnusedMember.Local
        private static string _clientToken;

        public LoginInfo Login(string userName, string password, string clientIdentifier = "", string language = "zh-cn")
        {
            var li = new LoginInfo();
            _clientToken = clientIdentifier;
            try
            {
                var auth = (HttpWebRequest)WebRequest.Create(RouteAuthenticate);
                auth.Method = "POST";
                var ag = new AuthenticationRequest(userName, password);
                var agJsonSerialiaer = new DataContractJsonSerializer(typeof(AuthenticationRequest));
                var agJsonStream = new MemoryStream();
                agJsonSerialiaer.WriteObject(agJsonStream, ag);
                agJsonStream.Position = 0;
                string logindata = (new StreamReader(agJsonStream)).ReadToEnd();
                byte[] postdata = Encoding.UTF8.GetBytes(logindata);
                auth.ContentLength = postdata.LongLength;
                Stream poststream = auth.GetRequestStream();
                poststream.Write(postdata, 0, postdata.Length);
                poststream.Close();
                var authans = (HttpWebResponse)auth.GetResponse();
                var responseJsonSerializer = new DataContractJsonSerializer(typeof(AuthenticationResponse));
                var res = authans.GetResponseStream();
                if (res == null)
                {
                    li.Suc = false;
                    li.Errinfo = "服务器无响应";
                    return li;
                }
                var responseStream = new StreamReader(res);
                string responseJson = responseStream.ReadToEnd();
                var responseJsonStream = new MemoryStream(Encoding.UTF8.GetBytes(responseJson)) {Position = 0};
                var response = responseJsonSerializer.ReadObject(responseJsonStream) as AuthenticationResponse;
                if (response == null)
                {
                    li.Suc = false;
                    li.Errinfo = "服务器无响应";
                    return li;
                }
                if (response.getClientToken() != NewLogin._clientToken)
                {
                    li.Suc = false;
                    li.Errinfo = "客户端标识和服务器返回不符，这是个不常见的错误，就算是正版启动器这里也没做任何处理，只是报了这么个错。";
                    return li;
                }
                li.Suc = true;
                li.UN = response.getSelectedProfile().getName();
                li.Client_identifier = NewLogin._clientToken;
                li.SID = response.getAccessToken();
                var otherInfoSerializer = new DataContractJsonSerializer(typeof(SortedList));
                var otherInfoList = new SortedList
                {
                    {"${auth_uuid}", response.getSelectedProfile().getId()},
                    {"${auth_access_token}", response.getAccessToken()}
                };
                var otherInfoStream = new MemoryStream();
                otherInfoSerializer.WriteObject(otherInfoStream, otherInfoList);
                otherInfoStream.Position = 0;
                //LI.OtherInfo = (new StreamReader(OtherInfoStream)).ReadToEnd();
                var outInfo = new StringBuilder();
                outInfo.Append("${auth_session}:").AppendLine(response.getAccessToken());
                outInfo.Append("${auth_uuid}:").AppendLine(response.getSelectedProfile().getId());
                outInfo.Append("${auth_access_token}:").AppendLine(response.getAccessToken());
                if (response.getUser() != null)
                {
                    if (response.getUser().getId()!=null)
                    {
                        AuthenticationResponse.Properties[] properties = response.getUser().getProperties();
                        if (properties != null)
                        {
                            var propertiesObj = properties.ToDictionary(p => p.getName(), p => new[] {p.getValue()});
                            var pJsonSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                            string pJsonStr = pJsonSerializer.Serialize(propertiesObj);
                            outInfo.Append("${user_properties}:").AppendLine(pJsonStr);
                        }
                    }
                }
                li.OtherInfo = li.SID;
                li.OutInfo = outInfo.ToString();
                return li;
            }
            catch (TimeoutException ex)
            {
                li.Suc = false;
                li.Errinfo = ex.Message;
                return li;
            }
        }
        public long GetVer()
        {
            return 1;
            //代表为第一代标准化登陆插件
        }
        public string GetName(string language = "zh-cn")
        {
            return "新正版登录";
        }
        public static string GetClientToken()
        {
            return _clientToken;
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
            public string OutInfo;
        }

        public long GetVersion(int version)
        {
            switch (version)
            {
                case 1:
                    return 1;
                default:
                    return version;
            }
        }
    }
}
