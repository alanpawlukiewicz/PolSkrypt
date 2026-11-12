using PolSkrypt.Contracts;
using PolSkrypt.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Interpretation.Callables.FileCallables
{
    public class DeleteFileCallable : ICallable
    {
        public int? Arity => 1;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            var filePath = StringHelper.Stringify(arguments[0]);

            File.Delete(filePath);
            return null;
        }

        public override string ToString()
        {
            return "<funkcja usuń>";
        }
    }
}
