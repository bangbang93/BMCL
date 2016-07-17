using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using BMCLV2.Auth;

namespace BMCLV2.Plugin
{
    interface IBmclAuthPlugin : IBmclPlugin ,IAuth
    {
        string GetName(string language = "zh-cn");
        long GetVer();

    }
}
