using PolSkrypt.Constants;
using PolSkrypt.Contracts;
using PolSkrypt.Interpretation.NativeObjects;

namespace PolSkrypt.Interpretation.Callables.NativeCallables
{
    // Claude: generating class
    /// <summary>
    /// klasa_obiektu(x): returns the class object of an instance (like Python's type(x) for user classes).
    /// The result is the class itself, so it can be compared (==), called to create a new instance
    /// or used to call static methods. Open files (PFile) belong to the native class Plik.
    /// Returns null (wartość_pusta) for values that are not instances.
    /// </summary>
    public class ObjectClassCallable : ICallable
    {
        public int? Arity => 1;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            return arguments[0] switch
            {
                Instance instance => instance.ClassCallable,
                PFile => interpreter.FileClass,
                _ => null
            };
        }

        public override string ToString()
        {
            return FunctionConstants.NATIVE_FN_STRING;
        }
    }
}
