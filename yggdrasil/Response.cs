using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace yggdrasil
{
    [DataContract]
    public class Response
    {
        [DataMember]
        private String error;
        [DataMember]
        private String errorMessage;
        [DataMember]
        private String cause;

        public Response()
        {
        }

        public String getError()
        {
            return error;
        }

        public String getCause()
        {
            return cause;
        }

        public String getErrorMessage()
        {
            return errorMessage;
        }
    }
}
