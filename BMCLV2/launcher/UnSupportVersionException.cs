using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BMCLV2.Lang;

namespace BMCLV2.Launcher
{
    class UnSupportVersionException : Exception
    {
        private readonly string _message;

        public override string Message
        {
            get { return _message ?? LangManager.GetLangFromResource("UnSupportVersionExcepton"); }
        }

        public UnSupportVersionException(){}

        public UnSupportVersionException(string message)
        {
            _message = message;
        }
    }
}
