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
        public class User
        {

            private String id;

            public String getId()
            {
                return id;
            }
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
