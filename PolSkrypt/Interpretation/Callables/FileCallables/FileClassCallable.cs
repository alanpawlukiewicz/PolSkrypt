using PolSkrypt.Constants;
using PolSkrypt.Contracts;

namespace PolSkrypt.Interpretation.Callables.FileCallables
{
    public class FileClassCallable : IClassCallable
    {
        private readonly Dictionary<string, ICallable> _methods = new Dictionary<string, ICallable>{ 
            { "otwórz", new OpenFileCallable() }
        };

        public int? Arity => 0;

        public string Name => Keywords.FILE;

        public ICallable? FindStaticMethod(string name)
        {
            if (_methods.ContainsKey(name))
            {
                return _methods[name];
            }

            return null;
        }

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            return this;
        }

        public override string ToString()
        {
            return $"<klasa natywna {Name}>";
        }
    }
}
