using PolSkrypt.Contracts;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Helpers;
using System.Text;

namespace PolSkrypt.Interpretation.Callables.FileCallables
{
    public class OpenFileCallable : ICallable
    {
        public int? Arity => 2;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            string path = StringHelper.Stringify(arguments[0]);
            string mode = StringHelper.Stringify(arguments[1]);

            if (!File.Exists(path))
            {
                try
                {
                    return File.Create(path);
                }
                catch (ArgumentException)
                {
                    throw new RuntimeException("Nazwa pliku nie może być pusta.");
                }
                catch(UnauthorizedAccessException)
                {
                    throw new RuntimeException("Prośba o dostęp do pliku została odrzucona.");
                }
            }

            FileAccess? fileAccess = null;
            switch (mode)
            {
                case "r":
                    fileAccess = FileAccess.Read;
                    break;
                case "w":
                    fileAccess = FileAccess.Write;
                    break;
                case "rw":
                    fileAccess = FileAccess.ReadWrite;
                    break;
            }

            if (fileAccess == null)
            {
                throw new RuntimeException("Nieprawidłowy tryb pliku.");
            }

            return File.Open(path, FileMode.Open, (FileAccess)fileAccess, FileShare.None);
        }

    }
}
