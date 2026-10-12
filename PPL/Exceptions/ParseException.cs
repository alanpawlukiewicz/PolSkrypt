using System;
using System.Collections.Generic;
using System.Text;

namespace PPL.Exceptions
{
    public class ParseException : Exception
    {
        public ParseException(Token token, string message) : base(message)
        {
            ErrorHandler.Error(token, message);
        }
    }
}
