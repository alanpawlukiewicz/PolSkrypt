using PPL.Contracts;

namespace PPL.Callables
{
    public class ClockCallable : ICallable
    {
        public int Arity => 0;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public override string ToString()
        {
            return "<natywna funkcja>";
        }
    }
}
