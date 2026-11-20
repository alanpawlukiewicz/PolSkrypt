using PolSkrypt.Constants;
using PolSkrypt.Contracts;
using PolSkrypt.Errors.Exceptions;
using System.Globalization;

namespace PolSkrypt.Interpretation.Callables.NativeCallables
{
    public class NumberCallable : ICallable
    {
        public int? Arity => 1;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            var argument = arguments[0];
            if (argument is string s &&
                double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }

            throw new RuntimeException("Argument musi być liczbą.");
        }

        public override string ToString()
        {
            return FunctionConstants.NATIVE_FN_STRING;
        }
    }
}
