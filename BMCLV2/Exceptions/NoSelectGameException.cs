using System;
using BMCLV2.I18N;

namespace BMCLV2.Exceptions
{
    public class NoSelectGameException :Exception
    {
        public NoSelectGameException() : base(LangManager.Translate("NoSelectGameException"))
        {
        }
    }
}