using System;
using System.Collections.Generic;
using System.Text;

namespace PPL.Contracts
{
    public interface ICallable
    {
        int Arity { get; }
        object? Call(Interpreter interpreter, List<object?> arguments);
    }
}
