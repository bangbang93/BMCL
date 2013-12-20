using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BMCLV2.Texturepack
{
    class NoResourcePackDirException:Exception
    {
        public override string Message
        {
            get
            {
                return "未找到资源包路径";
            }
        }
    }
}
