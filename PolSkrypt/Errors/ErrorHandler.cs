using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Helpers;
using PolSkrypt.Lexing;

namespace PolSkrypt.Errors
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

        private static void WriteError(string message)
        {
            LineTrackingWriter.EnsureNewLine();
            Console.Error.WriteLine(message);
        }

        public static void Error(int line, string message)
        {
            Report(line, null, string.Empty, message);
        }

        public static void Error(int line, string? file, string message)
        {
            Report(line, file, string.Empty, message);
        }

        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Report(token.Line, token.File, " na końcu", message);
            }
            else
            {
                Report(token.Line, token.File, $" na '{token.Lexeme}'", message);
            }

        }

        public static void Error(string name, string message)
        {
            WriteError($"Błąd przy {name}: {message}");
            _hadError = true;
        }

        public static void RuntimeError(RuntimeException ex)
        {
            if (ex.Token == null)
            {
                WriteError($"Błąd: {ex.Message}");
                _hadRuntimeError = true;
                return;
            }
            WriteError($"{Location(ex.Token.Line, ex.Token.File)} Błąd: {ex.Message}");
            _hadRuntimeError = true;
        }

        private static void Report(int line, string? file, string where, string message)
        {
            WriteError($"{Location(line, file)} Błąd {where}: {message}");
            _hadError = true;
        }

        // "[Linia 3]" albo, gdy znany jest plik (np. importowany moduł), "[mat.pols, Linia 3]".
        private static string Location(int line, string? file)
        {
            return file == null ? $"[Linia {line}]" : $"[{file}, Linia {line}]";
        }
    }
}
