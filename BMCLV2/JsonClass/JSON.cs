using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace BMCLV2.JsonClass
{
    public class JSON<T>
    {
        private readonly DataContractJsonSerializer _serialzier;

        public JSON()
        {
            _serialzier = new DataContractJsonSerializer(typeof(T));
        }

        public T Parse(Stream stream)
        {
            return (T)_serialzier.ReadObject(stream);
        }

        public T Parse(string json)
        {
            return Parse(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        }

        public string Stringify(object obj)
        {
            var stream = new MemoryStream();
            _serialzier.WriteObject(stream, obj);
            stream.Position = 0;
            var sr = new StreamReader(stream, Encoding.UTF8);
            return sr.ReadToEnd();
        }
    }
}