using PPL.Constants;
using PPL.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PPL.Interpretation.Callables
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
