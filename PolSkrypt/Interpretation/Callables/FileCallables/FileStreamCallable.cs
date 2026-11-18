using PolSkrypt.Contracts;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Helpers;
using PolSkrypt.Interpretation.NativeObjects;
using PolSkrypt.Lexing;

namespace PolSkrypt.Interpretation.Callables.FileCallables
{
    public class FileStreamCallable : ICallable
    {
        private enum FileCallType
        {
            READ, READ_LINE, APPEND, APPEND_LINE,
            CLOSE, SEEK, EOF, REWIND, POSITION
        }

        private static readonly Dictionary<string, FileCallType> _fileFunctions = new()
        {
            { "czytaj", FileCallType.READ },
            { "czytaj_linie", FileCallType.READ_LINE },
            { "wpisz", FileCallType.APPEND },
            { "wpisz_linie", FileCallType.APPEND_LINE },
            { "zamknij", FileCallType.CLOSE },
            { "ustaw", FileCallType.SEEK },
            { "koniec", FileCallType.EOF },
            { "przewiń", FileCallType.REWIND },
            { "pozycja", FileCallType.POSITION }
        };

        private readonly PFile _file;
        private readonly Token _name;
        private readonly FileCallType _callType;

        public FileStreamCallable(PFile file, Token name)
        {
            if (!_fileFunctions.TryGetValue(name.Lexeme, out _callType))
            {
                throw new RuntimeException(name, "Podano nieprawidłową funkcję pliku.");
            }

            _file = file;
            _name = name;
        }

        public int? Arity => _callType switch
        {
            FileCallType.APPEND or FileCallType.APPEND_LINE or FileCallType.SEEK => 1,
            _ => 0
        };

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            try
            {
                switch (_callType)
                {
                    case FileCallType.READ:
                        return _file.ReadAll();

                    case FileCallType.READ_LINE:
                        return _file.ReadLine();

                    case FileCallType.EOF:
                        return _file.Eof();

                    case FileCallType.CLOSE:
                        _file.Close();
                        return null;

                    case FileCallType.APPEND:
                        _file.Write(StringHelper.Stringify(arguments[0]));
                        return _file;

                    case FileCallType.APPEND_LINE:
                        _file.Write(StringHelper.Stringify(arguments[0]) + "\n");
                        return _file;

                    case FileCallType.SEEK:
                        _file.Seek(ParseIndex(arguments[0]));
                        return _file;

                    case FileCallType.REWIND:
                        _file.Rewind();
                        return _file;

                    case FileCallType.POSITION:
                        return _file.Tell();

                    default:
                        throw new RuntimeException(_name, "Podano nieprawidłową funkcję pliku.");
                }
            }
            catch (NotSupportedException)
            {
                throw new RuntimeException(_name, "Ta operacja nie jest obsługiwana dla tego pliku.");
            }
            catch (ObjectDisposedException)
            {
                throw new RuntimeException(_name, "Plik jest zamknięty.");
            }
            catch (IOException)
            {
                throw new RuntimeException(_name,
                    "Błąd operacji na pliku (np. ustawienie pozycji przed końcem w trybie dopisywania \"a\", " +
                    "lub brak miejsca na dysku).");
            }
        }

        private long ParseIndex(object? index)
        {
            if (index is not double value
                || double.IsNaN(value)
                || value % 1 != 0
                || value < 0
                || value >= long.MaxValue)
            {
                throw new RuntimeException(_name, "Indeks musi być nieujemną liczbą całkowitą.");
            }

            return (long)value;
        }

        public override string ToString()
        {
            return $"<funkcja pliku {_name.Lexeme}>";
        }
    }
}