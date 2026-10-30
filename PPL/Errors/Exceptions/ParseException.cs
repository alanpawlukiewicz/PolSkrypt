using PPL.Lexing;

namespace PPL.Errors.Exceptions
{
    public class ParseException : Exception
    {
        public ParseException(Token token, string message) : base(message)
        {
            ErrorHandler.Error(token, message);
        }
    }
}
