using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace BMCLV2.Auth
{
    public interface IAuth
    {
        Task<AuthResult> Login(string username, string password);
    }
}