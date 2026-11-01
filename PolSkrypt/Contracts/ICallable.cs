using PolSkrypt.Interpretation;

namespace PolSkrypt.Contracts
{
    public interface ICallable
    {
        int? Arity { get; }
        object? Call(Interpreter interpreter, List<object?> arguments);
    }
}
