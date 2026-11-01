using PolSkrypt.Constants;
using PolSkrypt.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Interpretation.Callables
{
    public class InputCallable : ICallable
    {
        public int? Arity => 0;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            return Console.ReadLine();
        }

        public override string ToString()
        {
            return FunctionConstants.NATIVE_FN_STRING;
        }
    }
}
