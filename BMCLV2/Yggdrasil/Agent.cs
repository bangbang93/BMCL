using System.Runtime.Serialization;
using System.Text;

namespace BMCLV2.Yggdrasil
{
    [DataContract]
    public class Agent
    {
        public static Agent Minecraft = new Agent("Minecraft", 1);
        public static Agent Scrolls = new Agent("Scrolls", 1);
        [DataMember(Name = "name")]
        public string Name { get; private set; }
        [DataMember(Name = "version")]
        public int Version { get; private set; }

        public Agent(string name, int version)
        {
            Name = name;
            Version = version;
        }

        public override string ToString()
        {
            return (new StringBuilder()).Append("Agent{name='").Append(Name).Append('\'').Append(", version=").Append(Version).Append('}').ToString();
        }

    }
}
