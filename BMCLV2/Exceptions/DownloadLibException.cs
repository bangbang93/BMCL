using System;
using BMCLV2.Game;
using BMCLV2.I18N;

namespace BMCLV2.Exceptions
{
    public class DownloadLibException : Exception
    {
        public LibraryInfo Library { get; }

        public DownloadLibException(LibraryInfo library, Exception innerException):base(LangManager.Transalte("DownloadLibException", library.Name), innerException)
        {
            Library = library;
        }
    }
}