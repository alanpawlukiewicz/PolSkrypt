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
            string str = string.Empty;

            foreach (var argument in arguments)
            {
                str += $"{StringHelper.Stringify(argument)
                    .Replace("\\n", "\n")
                    .Replace("\\t", "\t")
                    .Replace("\\r", "\r")} ";
            }
            str = str.Remove(str.Length - 1);
            Console.Write(str);
            return null;
        }

        public override string ToString()
        {
            return FunctionConstants.NATIVE_FN_STRING;
        }
    }
}
