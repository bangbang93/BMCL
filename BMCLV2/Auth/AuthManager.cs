using System.Threading.Tasks;

namespace BMCLV2.Auth
{
    public class AuthManager
    {
        public IAuth GetAuth(string name)
        {
            var authPlugin = BmclCore.PluginManager.GetAuth(name);
            return authPlugin switch
            {
              null => null,
              IAuth auth => auth,
              _ => WrapOldAuth(authPlugin)
            };
        }

        private static IAuth WrapOldAuth(object oldAuth)
        {
            return new OldPluginLogin(oldAuth);
        }

        public IAuth GetCurrentAuth()
        {
            var currentAuthName = BmclCore.Config.Login;
            return GetAuth(currentAuthName);
        }

        public async Task<AuthResult> Login(string username, string password = null)
        {
            var auth = GetCurrentAuth();
            if (auth == null) return new AuthResult(username)
            {
                IsSuccess = true
            };
            return await auth.Login(username, password);
        }
    }
}
