using Microsoft.VisualBasic.FileIO;
using PolSkrypt.Contracts;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Helpers;

namespace PolSkrypt.Interpretation.Callables.FileCallables
{
    public class ChangeFileNameCallable : ICallable
    {
        public int? Arity => 2;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            var oldFilePath = StringHelper.Stringify(arguments[0]);
            var newFilePath = StringHelper.Stringify(arguments[1]);

            try { File.Move(oldFilePath, newFilePath); }
            catch (FileNotFoundException) { throw new RuntimeException("Plik nie istnieje."); }
            catch (DirectoryNotFoundException) { throw new RuntimeException("Nieprawidłowa ścieżka pliku."); }
            catch (UnauthorizedAccessException) { throw new RuntimeException("Odmowa dostępu do pliku."); }
            catch (ArgumentException) { throw new RuntimeException("Nieprawidłowe argumenty."); }
            catch (IOException) { throw new RuntimeException("Nie można przemianować pliku (docelowy już istnieje lub plik jest używany)."); }

            return null;
        }

        public override string ToString()
        {
            return "<funkcja przemianuj>";
        }
    }
}
