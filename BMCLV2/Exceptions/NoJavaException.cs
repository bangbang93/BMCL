using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using BMCLV2.I18N;

namespace BMCLV2.Launcher
{
    class NoJavaException : System.Exception
    {
        private readonly string _message;

        public override string Message
        {
            get { return _message??LangManager.GetLangFromResource("NoJavaException"); }
        }

        public NoJavaException()
        {
        }

        public NoJavaException(string message)
        {
            _message = message;
        }
    }
}
