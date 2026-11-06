using PolSkrypt.Constants;
using PolSkrypt.Contracts;

namespace PolSkrypt.Interpretation.Callables.NativeCallables
{
    public class PrintCallable : ICallable
    {
        public int? Arity => null;

        private string Stringify(object? value)
        {
            if (value == null)
            {
                return Keywords.NULL;
            }

            if (value is double)
            {
                string text = value.ToString()!;

                return text;
            }

            if (value is bool)
            {
                return (bool)value ? Keywords.TRUE : Keywords.FALSE;
            }

            return value.ToString() ?? string.Empty;
        }

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            foreach(var argument in arguments)
            {
                Console.Write($"{Stringify(argument)} ");
            }
            Console.WriteLine();
            return null;
        }

        public override string ToString()
        {
            return FunctionConstants.NATIVE_FN_STRING;
        }
    }
}
