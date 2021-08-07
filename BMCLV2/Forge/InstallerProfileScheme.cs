using System.Collections.Generic;
using System.Runtime.Serialization;
using BMCLV2.Game;

namespace BMCLV2.Forge
{
  [DataContract]
  public class InstallerProfileScheme
  {
    [DataMember(Name = "data")] public Dictionary<string, DataItem> Data;
    [DataMember(Name = "libraries")] public LibraryInfo[] Libraries;

    [DataContract]
    public class DataItem
    {
      [DataMember(Name = "client")] public string Client;
      [DataMember(Name = "server")] public string Server;
    }
  }
}
