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

            try
            {
                FileSystem.RenameFile(oldFilePath, newFilePath);
            }
            catch(ArgumentException)
            {
                throw new RuntimeException("Nieprawidłowe argumenty.");
            }
            
            return null;
        }

        public override string ToString()
        {
            return "<funkcja przemianuj>";
        }
    }
}
