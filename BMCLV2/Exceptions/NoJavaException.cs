namespace BMCLV2.Exceptions
{
    internal class NoJavaException : System.Exception
    {
        public override string Message { get; }

        public string Javaw { get; }

        public NoJavaException(string javaw)
        {
            Javaw = javaw;
            Message = $"{javaw} No Such File";
        }
    }
}
