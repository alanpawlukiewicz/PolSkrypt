namespace PPL.Exceptions
{
    public class RuntimeException : Exception
    {
        private readonly Token _token;

        public Token Token { get { return _token; } }
        public RuntimeException(Token token, string message) : base(message)
        {
            _token = token;
        }
    }
}
