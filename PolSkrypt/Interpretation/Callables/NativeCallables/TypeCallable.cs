using PolSkrypt.Constants;
using PolSkrypt.Contracts;
using PolSkrypt.Interpretation.NativeObjects;

namespace PolSkrypt.Interpretation.Callables.NativeCallables
{
    public class TypeCallable : ICallable
    {
        public int? Arity => 1;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            return arguments[0] switch
            {
                null => Keywords.NULL,
                double => Keywords.NUMBER,
                string => Keywords.STRING,
                bool => Keywords.BOOLEAN,
                List<object?> => Keywords.LIST,
                IClassCallable => Keywords.CLASS,
                ICallable => Keywords.FUNCTION,
                Instance => Keywords.INSTANCE,
                PModule => Keywords.MODULE,
                PFile => Keywords.FILE,
                var other => throw new InvalidOperationException(
                    $"typ(): nieobsłużony typ wartości {other.GetType().Name}")
            };
        }

    }
}
