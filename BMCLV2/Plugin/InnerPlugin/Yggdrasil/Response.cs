using System.Runtime.Serialization;

namespace BMCLV2.Plugin.InnerPlugin.Yggdrasil
{
    [DataContract]
    public class Response
    {
        [DataMember(Name = "error")]
        public string Error { get; private set; }
        [DataMember(Name = "errorMessage")]
        public string ErrorMessage { get; private set; }
        [DataMember(Name = "cause")]
        public string Cause { get; private set; }
    }
}
