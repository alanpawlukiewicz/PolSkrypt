using PolSkrypt.Contracts;
using PolSkrypt.Helpers;

namespace PolSkrypt.Interpretation.Callables.FileCallables
{
    public class FileExistsCallable : ICallable
    {
        public int? Arity => 1;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            var filePath = StringHelper.Stringify(arguments[0]);

            return File.Exists(filePath);
        }

        public override string ToString()
        {
            return "<funkcja istnieje>";
        }
    }
}
