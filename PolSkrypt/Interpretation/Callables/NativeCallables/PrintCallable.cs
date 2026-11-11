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
            if (arguments.Count == 1)
            {
                Console.Write(StringHelper.Stringify(arguments[0])
                    .Replace("\\n", "\n")
                    .Replace("\\t", "\t")
                    .Replace("\\r", "\r"));
                return null;
            }

            foreach (var argument in arguments)
            {
                Console.Write($"{StringHelper.Stringify(argument)
                    .Replace("\\n", "\n")
                    .Replace("\\t", "\t")
                    .Replace("\\r", "\r")} ");
            }
            return null;
        }

        public override string ToString()
        {
            return FunctionConstants.NATIVE_FN_STRING;
        }
    }
}
