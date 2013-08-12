using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BMCLV2.serverlist
{
    public class serverinfo
    {
        public string Name { get; set; }
        public bool IsHide { get; set; }
        public string Address { get; set; }
        public serverinfo(string Name, bool IsHide, string Address)
        {
            this.Name = Name;
            this.IsHide = IsHide;
            this.Address = Address;
        }
    }
}
