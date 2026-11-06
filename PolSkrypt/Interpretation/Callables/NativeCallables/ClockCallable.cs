using PolSkrypt.Constants;
using PolSkrypt.Contracts;

namespace PolSkrypt.Interpretation.Callables.NativeCallables
{
    public class ClockCallable : ICallable
    {
        public int? Arity => 0;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            return (double)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public override string ToString()
        {
            return FunctionConstants.NATIVE_FN_STRING;
        }
    }
}
