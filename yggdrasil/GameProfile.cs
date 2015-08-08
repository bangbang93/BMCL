using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace yggdrasil
{
    [DataContract]
    public class GameProfile
    {
        [DataMember]
        private String id;
        [DataMember]
        private String name;

        public GameProfile(String id, String name)
        {
            if (String.IsNullOrEmpty(id) && String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name and ID cannot both be blank");
            }
            else
            {
                this.id = id;
                this.name = name;
                return;
            }
        }

        public String getId()
        {
            return id;
        }

        public String getName()
        {
            return name;
        }

        public bool isComplete()
        {
            return !string.IsNullOrEmpty(getId()) && !String.IsNullOrEmpty(getName());
        }

        public int hashCode()
        {
            int result = id.GetHashCode();
            result = 31 * result + name.GetHashCode();
            return result;
        }

        public String toString()
        {
            return (new StringBuilder()).Append("GameProfile{id='").Append(id).Append('\'').Append(", name='").Append(name).Append('\'').Append('}').ToString();
        }
    }
}
