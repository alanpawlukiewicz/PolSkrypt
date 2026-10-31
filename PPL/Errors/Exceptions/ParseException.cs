using PPL.Lexing;

namespace PPL.Errors.Exceptions
{
    public class ParseException : RuntimeException
    {
        public ParseException(Token token, string message) : base(token, message)
        {
            ErrorHandler.Error(token, message);
        }
    }
}
