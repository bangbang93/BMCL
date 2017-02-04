using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using BMCLV2.I18N;
using BMCLV2.Launcher;

namespace BMCLV2.Util
{
    public class ErrorHandler
    {
        private readonly Exception _exception;

        public ErrorHandler(Exception exception)
        {
            _exception = exception;
            Logger.Fatal(exception);
        }

        public bool Resolve()
        {
            return _match();
        }

        private bool _match()
        {
            if (_exception is XamlParseException)
            {
                if (_exception.InnerException is FileLoadException)
                {
                    //TODO 资源加载
                }
            }
            if (_exception is TypeLoadException)
            {
                if (_exception.Message.Contains("System.Runtime.CompilerServices.IAsyncStateMachine"))
                {
                    MessageBox.Show(LangManager.Transalte("ExceptionDotNet4.5"), "BMCL", MessageBoxButton.OK);
                    ChildProcess.Exec("https://www.bangbang93.com/topic/4/bmclng-%E9%9C%80%E8%A6%81-net-4-5");
                    return true;
                }
            }
            return false;
        }
    }
}