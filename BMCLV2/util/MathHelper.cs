using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BMCLV2.util
{
    class MathHelper
    {
        public static int parseIntWithDefault(String par0Str, int par1)
        {
            int j = par1;

            try
            {
                j = int.Parse(par0Str);
            }
            catch (Exception ex)
            {
                ;
            }

            return j;
        }
    }
}
