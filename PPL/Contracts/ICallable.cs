using PPL.Interpretation;

namespace PPL.Contracts
{
    public interface ICallable
    {
        int Arity { get; }
        object? Call(Interpreter interpreter, List<object?> arguments);
    }
}
