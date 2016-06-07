namespace BMCLV2.Exceptions
{
    class NoJavaException : System.Exception
    {
        public override string Message { get; }

        public NoJavaException(string javaw)
        {
            Message = $"{javaw} No Such File";
        }
    }
}
