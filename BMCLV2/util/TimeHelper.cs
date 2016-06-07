using System;

namespace BMCLV2.util
{
    public class TimeHelper
    {
        public static int TimeStamp()
        {
            return (int) (DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
        } 
    }
}