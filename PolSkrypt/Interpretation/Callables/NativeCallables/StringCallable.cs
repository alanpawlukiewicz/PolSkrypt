using PolSkrypt.Constants;
using PolSkrypt.Contracts;

namespace PolSkrypt.Interpretation.Callables.NativeCallables
{
    public class StringCallable : ICallable
    {
        public int? Arity => 1;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            return arguments[0]?.ToString();
        }

        public override string ToString()
        {
            return FunctionConstants.NATIVE_FN_STRING;
        }
    }
}
