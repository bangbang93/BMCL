using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BMCLV2.I18N;

namespace BMCLV2.Forge
{
    class ForgeListNotReadyException:Exception
    {
        public override string Message
        {
            get
            {
                return LangManager.GetLangFromResource("ForgeNotReady");
            }
        }
        public override string ToString()
        {
            return this.Message;
        }
    }
}
