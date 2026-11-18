using PolSkrypt.Contracts;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Helpers;
using PolSkrypt.Interpretation.NativeObjects;

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
                _ => throw new RuntimeException("Nieprawidłowy tryb pliku. Dozwolone: r, w, a, rw.")
            };

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new RuntimeException("Nazwa pliku nie może być pusta.");
            }

            if (Directory.Exists(path))
            {
                throw new RuntimeException("Podana ścieżka wskazuje na katalog, a nie na plik.");
            }

            FileStream? stream = null;
            try
            {
                stream = File.Open(path, fileMode, fileAccess, FileShare.ReadWrite);
                return new PFile(stream);
            }
            catch (FileNotFoundException)
            {
                stream?.Dispose();
                throw new RuntimeException("Plik nie istnieje.");
            }
            catch (DirectoryNotFoundException)
            {
                stream?.Dispose();
                throw new RuntimeException("Nieprawidłowa ścieżka pliku.");
            }
            catch (PathTooLongException)
            {
                stream?.Dispose();
                throw new RuntimeException("Ścieżka pliku jest zbyt długa.");
            }
            catch (UnauthorizedAccessException)
            {
                stream?.Dispose();
                throw new RuntimeException("Prośba o dostęp do pliku została odrzucona.");
            }
            catch (ArgumentException)
            {
                stream?.Dispose();
                throw new RuntimeException("Ścieżka pliku zawiera niedozwolone znaki.");
            }
            catch (NotSupportedException)
            {
                stream?.Dispose();
                throw new RuntimeException("Nieprawidłowy format ścieżki pliku.");
            }
            catch (IOException)
            {
                stream?.Dispose();
                throw new RuntimeException("Nie można otworzyć pliku (może być używany przez inny proces).");
            }
        }

        public override string ToString()
        {
            return "<funkcja otwórz>";
        }
    }
}