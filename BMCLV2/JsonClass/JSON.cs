using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace BMCLV2.JsonClass
{
  public class JSON<T>
  {
    private readonly DataContractJsonSerializer _serializer;

    public JSON()
    {
      _serializer = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings
      {
        UseSimpleDictionaryFormat = true
      });
    }

    public static T ParseOnce(string json)
    {
      return new JSON<T>().Parse(json);
    }

    public static T ParseOnce(Stream stream)
    {
      return new JSON<T>().Parse(stream);
    }

    public T Parse(Stream stream)
    {
      return (T)_serializer.ReadObject(stream);
    }

    public T Parse(string json)
    {
      return Parse(new MemoryStream(Encoding.UTF8.GetBytes(json)));
    }

    public string Stringify(object obj)
    {
      var stream = new MemoryStream();
      _serializer.WriteObject(stream, obj);
      stream.Position = 0;
      var sr = new StreamReader(stream, Encoding.UTF8);
      return sr.ReadToEnd();
    }
  }
}
