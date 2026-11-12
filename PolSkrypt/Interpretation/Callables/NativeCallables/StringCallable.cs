using PolSkrypt.Constants;
using PolSkrypt.Contracts;
using PolSkrypt.Helpers;

namespace PolSkrypt.Interpretation.Callables.NativeCallables
{
    public class StringCallable : ICallable
    {
        public int? Arity => 1;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            return StringHelper.Stringify(arguments[0]);
        }

        public override string ToString()
        {
            return FunctionConstants.NATIVE_FN_STRING;
        }
    }
}
