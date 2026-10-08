using System;
using System.Collections.Generic;
using System.Text;

namespace PPL
{
    public static class ErrorHandler
    {
        private static bool _hadError = false;
        public static bool HadError
        {
            get { return _hadError; }
            set { _hadError = value; }
        }

        public static void Error(int line, string message)
        {
            Report(line, string.Empty, message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[Linia {line}] Błąd {where}: {message}");
            _hadError = true;
        }
    }
}
