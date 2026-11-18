using PolSkrypt.Contracts;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Interpretation.Callables.FileCallables
{
    public class DeleteFileCallable : ICallable
    {
        public int? Arity => 1;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            var filePath = StringHelper.Stringify(arguments[0]);

            if (Directory.Exists(filePath))
            {
                throw new RuntimeException("Podana ścieżka wskazuje na katalog, a nie na plik.");
            }

            try
            {
                File.Delete(filePath);
            }
            catch (ArgumentException)
            {
                throw new RuntimeException("Ścieżka pliku jest pusta lub zawiera niedozwolone znaki.");
            }
            catch (NotSupportedException)
            {
                throw new RuntimeException("Nieprawidłowy format ścieżki pliku.");
            }
            catch (PathTooLongException)
            {
                throw new RuntimeException("Ścieżka pliku jest zbyt długa.");
            }
            catch (DirectoryNotFoundException)
            {
                throw new RuntimeException("Nieprawidłowa ścieżka pliku.");
            }
            catch (UnauthorizedAccessException)
            {
                throw new RuntimeException("Odmowa dostępu do pliku (brak uprawnień lub plik jest tylko do odczytu).");
            }
            catch (IOException)
            {
                throw new RuntimeException("Nie można usunąć pliku (może być używany przez inny proces).");
            }

            return null;
        }

        public override string ToString()
        {
            return "<funkcja usuń>";
        }
    }
}
