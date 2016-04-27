using System;
using BMCLV2.Launcher;

namespace BMCLV2.Exceptions
{
    public class NoSuchVersionException : Exception
    {
        public string Id { get; }
        public override string Message { get; }

        public NoSuchVersionException(string id)
        {
            Id = id;
            Message = $"No Version Named {Id}";
        }
    }
}