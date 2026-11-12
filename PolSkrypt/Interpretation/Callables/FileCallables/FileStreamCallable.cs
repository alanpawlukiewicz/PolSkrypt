using PolSkrypt.Contracts;
using PolSkrypt.Entities;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Helpers;
using PolSkrypt.Lexing;
using System.Text;

namespace PolSkrypt.Interpretation.Callables.FileCallables
{
    public class FileStreamCallable : ICallable
    {
        private enum FileCallType
        {
            READ, READ_LINE, APPEND, APPEND_LINE, 
            CLOSE, SEEK
        }

        private static readonly Dictionary<string, FileCallType> _fileFunctions = new Dictionary<string, FileCallType>{
            { "czytaj", FileCallType.READ },
            { "czytaj_linie", FileCallType.READ_LINE },
            { "wpisz", FileCallType.APPEND },
            { "wpisz_linie", FileCallType.APPEND_LINE },
            { "zamknij", FileCallType.CLOSE },
            { "ustaw", FileCallType.SEEK }
        };

        private readonly PFile _file;
        private readonly Token _name;
        private readonly FileCallType _callType;

        public FileStreamCallable(PFile file, Token name)
        {
            if (_fileFunctions.ContainsKey(name.Lexeme))
            {
                _callType = _fileFunctions[name.Lexeme];
            }
            else
            {
                throw new RuntimeException(name, "Podano nieprawidłową funkcję pliku.");
            }

            _file = file;
            _name = name;
        }

        public int? Arity { get 
            { 
                if (_callType == FileCallType.READ || _callType == FileCallType.READ_LINE
                    || _callType == FileCallType.CLOSE)
                {
                    return 0;
                }
                else if (_callType == FileCallType.APPEND || _callType == FileCallType.APPEND_LINE
                    || _callType == FileCallType.SEEK)
                {
                    return 1;
                }
                return null;
            }
        }

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            switch (_callType)
            {
                case FileCallType.READ:
                    return ReadFile();
                case FileCallType.READ_LINE:
                    return ReadLine();
                case FileCallType.APPEND:
                    AppendToFile(arguments[0]);
                    break;
                case FileCallType.APPEND_LINE:
                    AppendLine(arguments[0]);
                    break;
                case FileCallType.CLOSE:
                    CloseFile();
                    return null;
                case FileCallType.SEEK:
                    Seek(arguments[0]);
                    break;
            }

            return _file;
        }

        private string ReadFile()
        {
            try
            {
                string text = string.Empty;
                string? line;
                while ((line = _file.ReadLine()) != null)
                {
                    text += line + "\n";
                }

                return text.Length == 0 ? text : text[..^1];
            }
            catch (ArgumentException)
            {
                throw new RuntimeException("Plik nie jest otworzony w trybie czytania.");
            }
        }

        private string? ReadLine()
        {
            try
            {
                return _file.ReadLine();
            }
            catch (ArgumentException)
            {
                throw new RuntimeException("Plik nie jest otworzony w trybie czytania.");
            }
            
        }

        private void AppendToFile(object? appendedText)
        {
            try
            {
                _file.Write(StringHelper.Stringify(appendedText));
            }
            catch (ArgumentException)
            {
                throw new RuntimeException("Plik nie jest otworzony w trybie wpisywania.");
            }
        }

        private void AppendLine(object? appendedText)
        {
            try
            {
                _file.Write(StringHelper.Stringify(appendedText) + "\n");
            }
            catch (ArgumentException)
            {
                throw new RuntimeException("Plik nie jest otworzony w trybie wpisywania.");
            }
        }

        private void CloseFile()
        {
            _file.Close();
        }

        private void Seek(object? index)
        {
            if (index is not double parsedIdx || parsedIdx % 1 != 0)
            {
                throw new RuntimeException("Indeks musi być dodatnią liczbą całkowitą.");
            }
            else
            {
                _file.Seek((long)parsedIdx);
            }
        }
    }
}
