using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace yggdrasil
{
    [DataContract]
    public class AuthenticationResponse : Response
    {
        [DataContract]
        public class User
        {
            [DataMember]
            private String id;
            [DataMember]
            private Properties[] properties;
            public String getId() { return id; }
            public Properties[] getProperties() { return this.properties; }
        }
        [DataContract]
        public class Properties
        {
            [DataMember]
            private string name, value;
            public string getName() { return this.name; }
            public string getValue() { return this.value; }

        }

        [DataMember]
        private String accessToken;
        [DataMember]
        private String clientToken;
        [DataMember]
        private GameProfile selectedProfile;
        [DataMember]
        private GameProfile[] availableProfiles;
        [DataMember]
        private User user;

        public AuthenticationResponse()
        {
        }

        public String getAccessToken()
        {
            return accessToken;
        }

        public String getClientToken()
        {
            return clientToken;
        }

        public GameProfile[] getAvailableProfiles()
        {
            return availableProfiles;
        }

        public GameProfile getSelectedProfile()
        {
            return selectedProfile;
        }

        public User getUser()
        {
            return user;
        }
    }
}
