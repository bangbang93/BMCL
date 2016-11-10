using System.Runtime.Serialization;

namespace BMCLV2.Plugin.InnerPlugin.Yggdrasil
{
    [DataContract]
    public class AuthenticationResponse : Response
    {
        public class UserType
        {
            public string id { get; private set; }
        }

        [DataMember(Name = "accessToken")]
        public string AccessToken { get; private set; }
        [DataMember(Name = "clientToken")]
        public string ClientToken { get; private set; }
        [DataMember(Name = "selectedProfile")]
        public GameProfile SelectedProfile { get; private set; }
        [DataMember(Name = "availableProfiles")]
        public GameProfile[] AvailableProfiles { get; private set; }
        [DataMember(Name = "user")]
        public UserType User { get; private set; }
    }
}
