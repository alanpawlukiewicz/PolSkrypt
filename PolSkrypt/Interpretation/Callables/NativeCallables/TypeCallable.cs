using PolSkrypt.Constants;
using PolSkrypt.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Interpretation.Callables.NativeCallables
{
    public class TypeCallable : ICallable
    {
        public int? Arity => 1;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            var argument = arguments[0];
            if (argument is double)
            {
                return Keywords.NUMBER;
            }
            else if (argument is string)
            {
                return Keywords.STRING;
            }
            else if (argument is bool)
            {
                return Keywords.BOOLEAN;
            }
            else if (argument is null)
            {
                return Keywords.NULL;
            }
            else
            {
                return argument.ToString();
            }
        }
    }
}
