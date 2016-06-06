using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BMCLV2.Login;

namespace BMCLV2.Auth
{
    public class OldPluginLogin : IAuth
    {
        private object _plugin;

        public OldPluginLogin(object plugin)
        {
            _plugin = plugin;
        }

        public async Task<AuthResult> Login(string username, string password)
        {
            var authInfo = new AuthResult(username);
            var T = _plugin.GetType();
            var login = T.GetMethod("Login");
            try
            {
                var loginansobj = await Task.Run(() => login.Invoke(_plugin, new object[] {username, password, Guid.NewGuid().ToString(), "zh-cn"}));
                var li = loginansobj.GetType();
                authInfo.IsSuccess = (bool)li.GetField("Suc").GetValue(loginansobj);
                if (authInfo.IsSuccess)
                {
                    authInfo.Username = li.GetField("UN").GetValue(loginansobj) as string;
                    authInfo.SID = li.GetField("SID").GetValue(loginansobj) as string;
                    authInfo.ClientIdentifier =
                        li.GetField("Client_identifier").GetValue(loginansobj) as string;
                    authInfo.Uid = li.GetField("UID").GetValue(loginansobj) as string;
                    authInfo.OtherInfo = li.GetField("OtherInfo").GetValue(loginansobj) as string;
                    if (li.GetField("OutInfo") != null)
                    {
                        authInfo.OutInfo = li.GetField("OutInfo").GetValue(loginansobj) as string;
                    }
                    Logger.Log(
                        $"登陆成功，使用用户名{authInfo.Username},sid{authInfo.SID},Client_identifier{authInfo.ClientIdentifier},uid{authInfo.Uid}");
                    return authInfo;
                }
                authInfo.Message = li.GetField("Errinfo").GetValue(loginansobj) as string;
                authInfo.OtherInfo = li.GetField("OtherInfo").GetValue(loginansobj) as string;
                Logger.Log($"登陆失败，错误信息:{authInfo.Message}，其他信息:{authInfo.OtherInfo}");
                return authInfo;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                authInfo.IsSuccess = false;
                authInfo.Message = ex.Message;
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    authInfo.Message += "\n" + ex.Message;
                }
                return authInfo;
            }
        }
    }
}