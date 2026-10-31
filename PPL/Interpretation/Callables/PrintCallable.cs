using PPL.Constants;
using PPL.Contracts;

namespace PPL.Interpretation.Callables
{
    public class PrintCallable : ICallable
    {
        public int Arity => 1;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            var value = arguments[0];
            if (value == null)
            {
                value = KeyWord.NULL;
            }

            if (value is double)
            {
                value = value.ToString()!;
            }

            if (value is bool)
            {
                value = (bool)value ? KeyWord.TRUE : KeyWord.FALSE;
            }

            Console.WriteLine(value.ToString() ?? string.Empty);
            return null;
        }
    }
}
