using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace yggdrasil
{
    [DataContract]
    public class Agent
    {
        public static Agent MINECRAFT = new Agent("Minecraft", 1);
        public static Agent SCROLLS = new Agent("Scrolls", 1);
        [DataMember]
        private String name;
        [DataMember]
        private int version;

        public Agent(String name, int version)
        {
            this.name = name;
            this.version = version;
        }

        public String getName()
        {
            return name;
        }

        public int getVersion()
        {
            return version;
        }

        public String toString()
        {
            return (new StringBuilder()).Append("Agent{name='").Append(name).Append('\'').Append(", version=").Append(version).Append('}').ToString();
        }

    }
}
