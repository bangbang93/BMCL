using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections;
using System.IO;
using System.Web;

namespace Genuine
{
    public class _Login
    {   
        public LoginInfo Login(string UserName, string Password, string Client_identifier = "", string Language = "zh-cn")
        {
            LoginInfo LI = new LoginInfo();
            try
            {
                HttpWebRequest auth = (HttpWebRequest)WebRequest.Create("https://login.minecraft.net");
                auth.Method = "POST";
                StringBuilder PostData = new StringBuilder();
                PostData.Append("user=");
                PostData.Append(UserName);
                PostData.Append("&password=");
                PostData.Append(Password);
                PostData.Append("&version=14");
                auth.ContentType = "text/html";
                Hashtable logindata = new Hashtable();
                logindata.Add("user", UserName);
                logindata.Add("password", Password);
                logindata.Add("version", 14);
                StreamReader AuthAnsStream;
                //byte[] postdata = Encoding.UTF8.GetBytes(HttpUtility.UrlEncode(PostData.ToString()));
                byte[] postdata = Encoding.UTF8.GetBytes(ParsToString(logindata));
                auth.ContentLength = postdata.LongLength;
                Stream poststream = auth.GetRequestStream();
                poststream.Write(postdata, 0, postdata.Length);
                poststream.Close();
                try
                {
                    HttpWebResponse authans = (HttpWebResponse)auth.GetResponse();
                    AuthAnsStream = new StreamReader(authans.GetResponseStream());
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                string AuthAnsString = AuthAnsStream.ReadToEnd();
                string[] split = AuthAnsString.Split(':');
                if (split.Length == 5)
                {
                    LI.Client_identifier = split[4];
                    LI.UN = split[2];
                    LI.SID = split[3];
                    if (String.IsNullOrWhiteSpace(LI.Client_identifier) || String.IsNullOrWhiteSpace(LI.UN) || String.IsNullOrWhiteSpace(LI.SID))
                    {
                        LI.Suc = false;
                        LI.Errinfo = "服务器响应无法解析";
                        return LI;
                    }
                    else
                    {
                        LI.Suc = true;
                        return LI;
                    }
                }
                else
                {
                    LI.Suc = false;
                    LI.Errinfo = "用户名或密码错误";
                    return LI;
                }
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
            return "正版登录";
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
