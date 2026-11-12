using PolSkrypt.Contracts;
using PolSkrypt.Entities;
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

            (FileMode fileMode, FileAccess fileAccess) = mode switch
            {
                "r" => (FileMode.Open, FileAccess.Read),
                "w" => (FileMode.Create, FileAccess.Write),
                "a" => (FileMode.Append, FileAccess.Write),
                "rw" => (FileMode.OpenOrCreate, FileAccess.ReadWrite),
                _ => throw new RuntimeException("Nieprawidłowy tryb pliku.")
            };

            try
            {
                return new PFile(File.Open(path, fileMode, fileAccess, FileShare.ReadWrite));
            }
            catch (FileNotFoundException)
            {
                throw new RuntimeException("Plik nie istnieje.");
            }
            catch (DirectoryNotFoundException)
            {
                throw new RuntimeException("Nieprawidłowa ścieżka pliku.");
            }
            catch (UnauthorizedAccessException)
            {
                throw new RuntimeException("Prośba o dostęp do pliku została odrzucona.");
            }
            catch (ArgumentException)
            {
                throw new RuntimeException("Nazwa pliku nie może być pusta.");
            }
            catch (IOException)
            {
                throw new RuntimeException("Nie można otworzyć pliku (może być używany przez inny proces).");
            }
        }

        public override string ToString()
        {
            return "<funkcja otwórz>";
        }
    }
}
