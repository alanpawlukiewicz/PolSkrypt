using PolSkrypt.Constants;
using PolSkrypt.Contracts;
using PolSkrypt.Helpers;

namespace PolSkrypt.Interpretation.Callables.NativeCallables
{
    public class PrintCallable : ICallable
    {
        public int? Arity => null;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            foreach (var argument in arguments)
            {
                Console.Write($"{StringHelper.Stringify(argument)} ");
            }
            return null;
        }

        public override string ToString()
        {
            return FunctionConstants.NATIVE_FN_STRING;
        }
    }
}
