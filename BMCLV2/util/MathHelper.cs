using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BMCLV2.util
{
    class MathHelper
    {
        private static Random _rand = new Random();
        public static int parseIntWithDefault(String par0Str, int par1)
        {
            int j = par1;

            try
            {
                j = int.Parse(par0Str);
            }
            catch 
            {
                
            }

            return j;
        }

        public static int Rand(int max = 1000)
        {
            return _rand.Next(max);
        }
    }
}
