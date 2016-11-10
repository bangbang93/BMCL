using System;
using System.Runtime.Serialization;
using System.Text;

namespace BMCLV2.Plugin.InnerPlugin.Yggdrasil
{
    [DataContract]
    public class GameProfile
    {
        [DataMember(Name = "id")]
        public string Id { get; private set; }

        [DataMember(Name = "name")]
        public string Name { get; private set; }

        public GameProfile(string id, string name)
        {
            if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name and ID cannot both be blank");
            }
            Id = id;
            Name = name;
        }

        public bool IsComplete()
        {
            return !string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(Name);
        }

        public int HashCode()
        {
            var result = Id.GetHashCode();
            result = 31 * result + Name.GetHashCode();
            return result;
        }

        public override string ToString()
        {
            return new StringBuilder().Append("GameProfile{id='").Append(Id).Append('\'').Append(", name='").Append(Name).Append('\'').Append('}').ToString();
        }
    }
}
