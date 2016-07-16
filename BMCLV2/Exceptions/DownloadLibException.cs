using System;
using BMCLV2.Game;

namespace BMCLV2.Exceptions
{
    public class DownloadLibException : Exception
    {
        public LibraryInfo Library { get; }

        public DownloadLibException(LibraryInfo library, Exception innerException):base($"{library.Name} download failed", innerException)
        {
            Library = library;
        }
    }
}