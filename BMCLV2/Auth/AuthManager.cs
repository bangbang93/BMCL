using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BMCLV2.Auth
{
    public class AuthManager
    {
        public IAuth GetAuth(string name)
        {
            var authPlugin = BmclCore.PluginManager.GetAuth(name);
            if (authPlugin == null) return null;
            if (authPlugin is IAuth)
            {
                return authPlugin as IAuth;
            }
            return WrapOldAuth(authPlugin);
        }

        private static IAuth WrapOldAuth(object oldAuth)
        {
            return new OldPluginLogin(oldAuth);
        }

        public IAuth GetCurrectAuth()
        {
            var currectAuthName = BmclCore.Config.Login;
            return GetAuth(currectAuthName);
        }

        public async Task<AuthResult> Login(string username, string password = null)
        {
            var auth = GetCurrectAuth();
            if (auth == null) return new AuthResult(username)
            {
                IsSuccess = true
            };
            return await auth.Login(username, password);
        }
    }
}