using PolSkrypt.Constants;
using PolSkrypt.Contracts;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Interpretation.NativeObjects;
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
            if (arguments[1] is not IClassCallable target)
                throw new RuntimeException("Drugi argument 'jest_instancją' musi być klasą.");

            // Open files are instances of the native class Plik.
            if (arguments[0] is PFile)
                return target == interpreter.FileClass;

            if (arguments[0] is not Instance instance)
                return false;

            for (ClassCallable? k = instance.ClassCallable; k != null; k = k.Superclass)
                if (k == target) return true;

            return false;
        }

        public override string ToString()
        {
            return FunctionConstants.NATIVE_FN_STRING;
        }
    }
}
