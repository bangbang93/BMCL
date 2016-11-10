using System.Runtime.Serialization;

namespace BMCLV2.Plugin.InnerPlugin.Yggdrasil
{
    [DataContract]
    internal class AuthenticationRequest
    {
        [DataMember(Name = "agent")]
        public Agent Agent { get; private set; }
        [DataMember(Name = "username")]
        public string Username { get; private set; }
        [DataMember(Name = "password")]
        public string Password { get; private set; }
        [DataMember(Name = "clientToken")]
        public string ClientToken { get; private set; }
        [DataMember(Name = "requestUser")]
        public bool RequestUser { get; private set; }

        public AuthenticationRequest(string username, string password)
        {
            RequestUser = true;
            Agent = Agent.Minecraft;
            Username = username;
            ClientToken = Yggdrasil.ClientToken;
            Password = password;
        }
    }
}
