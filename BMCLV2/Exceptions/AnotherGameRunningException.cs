using System;
using BMCLV2.I18N;

namespace BMCLV2.Exceptions
{
    public class AnotherGameRunningException : Exception
    {
        public AnotherGameRunningException(Launcher.Launcher launcher):base(LangManager.Transalte("AnotherGameRunningException", launcher.VersionInfo.Id))
        {
            Launcher = launcher;
            
        }

        public Launcher.Launcher Launcher { get; }
    }
}