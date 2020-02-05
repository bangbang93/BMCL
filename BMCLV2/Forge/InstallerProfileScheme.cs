using System.Runtime.Serialization;
using BMCLV2.Game;

namespace BMCLV2.Forge
{
  [DataContract]
  public class InstallerProfileScheme
  {
    [DataMember(Name = "libraries")] public LibraryInfo[] Libraries;
  }
}
