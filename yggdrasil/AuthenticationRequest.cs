using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace yggdrasil
{
    [DataContract]
    class AuthenticationRequest
    {
        [DataMember]
        private Agent agent;
        [DataMember]
        private String username;
        [DataMember]
        private String password;
        [DataMember]
        private String clientToken;
        [DataMember]
        private bool requestUser;

        public AuthenticationRequest(String username, String password)
        {
            requestUser = true;
            agent = Agent.MINECRAFT;
            this.username = username;
            clientToken = NewLogin.GetClientToken();
            this.password = password;
        }
    }
}
