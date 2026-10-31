using PPL.Constants;
using PPL.Contracts;

namespace PPL.Interpretation.Callables
{
    public class PrintCallable : ICallable
    {
        public int? Arity => null;

        private string Stringify(object? value)
        {
            if (value == null)
            {
                return KeyWord.NULL;
            }

            if (value is double)
            {
                string text = value.ToString()!;

                return text;
            }

            if (value is bool)
            {
                return (bool)value ? KeyWord.TRUE : KeyWord.FALSE;
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
