using PPL.Enums;
using PPL.Exceptions;

namespace PPL
{
    public static class ErrorHandler
    {
        private static bool _hadError = false;
        private static bool _hadRuntimeError = false;
        public static bool HadError
        {
            get { return _hadError; }
            set { _hadError = value; }
        }

        public static bool HadRuntimeError
        {
            get { return _hadRuntimeError; }
            set { _hadRuntimeError = value; }
        }

        public static void Error(int line, string message)
        {
            Report(line, string.Empty, message);
        }

        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Report(token.Line, " na końcu", message);
            }
            else
            {
                Report(token.Line, $" na '{token.Lexeme}'", message);
            }

        }

        public static void RuntimeError(RuntimeException ex)
        {
            Console.Error.WriteLine($"[Linia ${ex.Token.Line}] Błąd: {ex.Message}");
            _hadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[Linia {line}] Błąd {where}: {message}");
            _hadError = true;
        }
    }
}
