using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;

namespace yggdrasil
{
  public class NewLogin
  {
    private static string ClientToken;
    private string BaseUrl = "https://authserver.mojang.com/";
    private readonly string RouteAuthenticate = "https://authserver.mojang.com/authenticate";
    private string RouteInvalidate = "https://authserver.mojang.com/invalidate";
    private string RouteRefresh = "https://authserver.mojang.com/refresh";
    private string RouteSignout = "https://authserver.mojang.com/signout";
    private string RouteValidate = "https://authserver.mojang.com/validate";

    public LoginInfo Login(string UserName, string Password, string Client_identifier = "", string Language = "zh-cn")
    {
      var LI = new LoginInfo();
      ClientToken = Client_identifier;
      try
      {
        var auth = (HttpWebRequest)WebRequest.Create(RouteAuthenticate);
        auth.Method = "POST";
        var ag = new AuthenticationRequest(UserName, Password);
        var agJsonSerialiaer = new DataContractJsonSerializer(typeof(AuthenticationRequest));
        var agJsonStream = new MemoryStream();
        agJsonSerialiaer.WriteObject(agJsonStream, ag);
        agJsonStream.Position = 0;
        var logindata = new StreamReader(agJsonStream).ReadToEnd();
        var postdata = Encoding.UTF8.GetBytes(logindata);
        auth.ContentLength = postdata.LongLength;
        var poststream = auth.GetRequestStream();
        poststream.Write(postdata, 0, postdata.Length);
        poststream.Close();
        var authans = (HttpWebResponse)auth.GetResponse();
        var ResponseJsonSerializer = new DataContractJsonSerializer(typeof(AuthenticationResponse));
        var ResponseStream = new StreamReader(authans.GetResponseStream());
        var ResponseJson = ResponseStream.ReadToEnd();
        var ResponseJsonStream = new MemoryStream(Encoding.UTF8.GetBytes(ResponseJson));
        ResponseJsonStream.Position = 0;
        var Response = ResponseJsonSerializer.ReadObject(ResponseJsonStream) as AuthenticationResponse;
        if (Response.getClientToken() != ClientToken)
        {
          LI.Suc = false;
          LI.Errinfo = "客户端标识和服务器返回不符，这是个不常见的错误，就算是正版启动器这里也没做任何处理，只是报了这么个错。";
          return LI;
        }

        LI.Suc = true;
        LI.UN = Response.getSelectedProfile().getName();
        LI.Client_identifier = ClientToken;
        var OtherInfoSerializer = new DataContractSerializer(typeof(SortedList));
        var OtherInfoList = new SortedList();
        OtherInfoList.Add("${auth_uuid}", Response.getSelectedProfile().getId());
        OtherInfoList.Add("${auth_access_token}", Response.getAccessToken());
        var OtherInfoStream = new MemoryStream();
        OtherInfoSerializer.WriteObject(OtherInfoStream, OtherInfoList);
        OtherInfoStream.Position = 0;
        LI.OtherInfo = new StreamReader(OtherInfoStream).ReadToEnd();
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

    private static string ParsToString(Hashtable Pars)
    {
      var sb = new StringBuilder();
      foreach (string k in Pars.Keys)
      {
        if (sb.Length > 0) sb.Append("&");
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
