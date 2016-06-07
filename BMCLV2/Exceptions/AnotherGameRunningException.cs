using System;

namespace BMCLV2.Exceptions
{
    public class AnotherGameRunningException : Exception
    {
        public AnotherGameRunningException(Launcher.Launcher launcher)
        {
            Launcher = launcher;
        }

        public Launcher.Launcher Launcher { get; }
    }
}