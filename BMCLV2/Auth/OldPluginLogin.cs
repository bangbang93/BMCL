using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
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
                    authInfo.Uuid = li.GetField("UID").GetValue(loginansobj) as string;
                    authInfo.OtherInfo = DecodeOtherInfo(li.GetField("OtherInfo").GetValue(loginansobj) as string);
                    if (li.GetField("OutInfo") != null)
                    {
                        authInfo.OutInfo = li.GetField("OutInfo").GetValue(loginansobj) as string;
                    }
                    authInfo.Uuid = authInfo.Uuid ?? authInfo.OtherInfo["${auth_uuid}"];
                    authInfo.AccessToken = authInfo.AccessToken ?? authInfo.OtherInfo["${auth_access_token}"];
                    Logger.Log(
                        $"登陆成功，使用用户名{authInfo.Username},sid{authInfo.SID},Client_identifier{authInfo.ClientIdentifier},uid{authInfo.Uuid}");
                    return authInfo;
                }
                var otherInfoString = li.GetField("OtherInfo").GetValue(loginansobj) as string;
                authInfo.OtherInfo = DecodeOtherInfo(otherInfoString);
                Logger.Log($"登陆失败，错误信息:{authInfo.Message}，其他信息:{otherInfoString}");
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

        private static Dictionary<string, string> DecodeOtherInfo(string otherInfoString)
        {
            var deserialzer = new DataContractSerializer(typeof(SortedList));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(otherInfoString));
            var list = deserialzer.ReadObject(stream) as SortedList;
            if (list == null) return new Dictionary<string, string>();
            var dic = new Dictionary<string, string>(list.Count);
            foreach (DictionaryEntry entry in list)
            {
                dic.Add((string)entry.Key, (string)entry.Value);
            }
            return dic;
        }
    }
}