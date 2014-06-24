using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BMCLV2.Plugin
{
    interface IBmclAuthPlugin
    {
        string GetName(string language = "zh-cn");
        long GetVer();

    }
}
