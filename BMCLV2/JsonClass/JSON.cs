using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace BMCLV2.JsonClass
{
    public class JSON
    {
        private readonly DataContractJsonSerializer _serialzier;

        public JSON(Type T)
        {
            _serialzier = new DataContractJsonSerializer(T);
        } 
        public object Parse(string json)
        {
            return _serialzier.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        }

        public string Stringify(object obj)
        {
            var stream = new MemoryStream();
            _serialzier.WriteObject(stream, obj);
            var sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }
    }
}