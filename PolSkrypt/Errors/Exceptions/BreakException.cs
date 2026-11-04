using PolSkrypt.Lexing;

namespace PolSkrypt.Errors.Exceptions
{
    public class BreakException : RuntimeException
    {
        public BreakException(Token token, string message) : base(token, message)
        {
        }
    }
}
