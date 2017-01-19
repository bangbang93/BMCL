using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using BMCLV2.Auth;
using BMCLV2.I18N;
using BMCLV2.JsonClass;

namespace BMCLV2.Plugin.InnerPlugin.Yggdrasil
{
    public class Yggdrasil : IBmclAuthPlugin
    {
        private const string _baseUrl = "https://authserver.mojang.com/";
        private const string _routeAuthenticate = "https://authserver.mojang.com/authenticate";
        private const string _routeRefresh = "https://authserver.mojang.com/refresh";
        private const string _routeValidate = "https://authserver.mojang.com/validate";
        private const string _routeInvalidate = "https://authserver.mojang.com/invalidate";
        private const string _routeSignout = "https://authserver.mojang.com/signout";
        public static string ClientToken { get; private set; }

        public PluginType GetPluginType()
        {
            return PluginType.Auth;
        }

        public string GetName(string language = "zh-cn")
        {
            return "正版登录";
        }

        public long GetVer()
        {
            return 2;
        }

        public async Task<AuthResult> Login(string username, string password)
        {
            var authResult = new AuthResult(username);
            ClientToken = Guid.NewGuid().ToString();
            try
            {
                var auth = new WebClient();
                var ag = new AuthenticationRequest(username, password);
                var logindata = new JSON<AuthenticationRequest>().Stringify(ag);
                var authans = await auth.UploadStringTaskAsync(_routeAuthenticate, logindata);
                var response = new JSON<AuthenticationResponse>().Parse(authans);
                if (response.ClientToken != ClientToken)
                {
                    authResult.IsSuccess = false;
                    authResult.ErrCode = "客户端标识和服务器返回不符，这是个不常见的错误，就算是正版启动器这里也没做任何处理，只是报了这么个错。";
                    return authResult;
                }
                authResult.IsSuccess = true;
                authResult.Username = response.SelectedProfile.Name;
                authResult.ClientIdentifier = ClientToken;
                var otherInfoList = new Dictionary<string, string>()
                {
                    {"${auth_uuid}", response.SelectedProfile.Id},
                    {"${auth_access_token}", response.AccessToken}
                };
                authResult.OutInfo = otherInfoList;
                return authResult;
            }
            catch (TimeoutException exception)
            {
                authResult.IsSuccess = false;
                authResult.ErrCode = exception.Message;
                return authResult;
            }
            catch (WebException exception)
            {
                var res = (HttpWebResponse)exception.Response;
                if (res.StatusCode == HttpStatusCode.Forbidden)
                {
                    authResult.IsSuccess = false;
                    authResult.ErrCode = LangManager.Transalte("UsernameOrPasswordError");
                    return authResult;
                }
                throw;
            }
        }
    }
}
