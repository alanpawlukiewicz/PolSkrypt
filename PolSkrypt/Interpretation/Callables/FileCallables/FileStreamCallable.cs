using PolSkrypt.Contracts;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Lexing;
using System.Text;

namespace PolSkrypt.Interpretation.Callables.FileCallables
{
    public class FileStreamCallable : ICallable
    {
        private enum FileCallType
        {
            READ
        }

        private static readonly Dictionary<string, FileCallType> _fileFunctions = new Dictionary<string, FileCallType>{
            { "czytaj", FileCallType.READ }
        };

        private readonly FileStream _fileStream;
        private readonly Token _name;
        private readonly FileCallType _callType;

        public FileStreamCallable(FileStream fileStream, Token name)
        {
            if (_fileFunctions.ContainsKey(name.Lexeme))
            {
                _callType = _fileFunctions[name.Lexeme];
            }
            else
            {
                throw new RuntimeException(name, "Podano niepoprawną funkcję pliku.");
            }

            _fileStream = fileStream;
            _name = name;
        }

        public int? Arity { get 
            { 
                if (_callType == FileCallType.READ)
                {
                    return 0;
                }
                return null;
            } 
        }

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            using (StreamReader reader = new StreamReader(_fileStream, Encoding.UTF8))
            {
                string text = "";
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    text += line + "\n";
                }

                return text[..^1];
            }
        }
    }
}
