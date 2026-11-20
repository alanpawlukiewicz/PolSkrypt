using PolSkrypt.Contracts;
using PolSkrypt.Errors.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Interpretation.Callables.NativeCallables
{
    public class IsInstanceCallable : ICallable
    {
        public int? Arity => 2;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            if (arguments[1] is not ClassCallable target)
                throw new RuntimeException("Drugi argument 'jest_instancją' musi być klasą.");

            if (arguments[0] is not Instance instance)
                return false;

            for (ClassCallable? k = instance.ClassCallable; k != null; k = k.Superclass)
                if (k == target) return true;

            return false;
        }

    }
}
