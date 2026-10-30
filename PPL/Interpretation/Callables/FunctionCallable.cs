using PPL.Contracts;
using PPL.Interpretation;
using PPL.Parsing.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace PPL.Interpretation.Callables
{
    public class FunctionCallable : ICallable
    {
        private readonly Function _declaration;

        public FunctionCallable(Function declaration)
        {
            _declaration = declaration;
        }

        public int Arity => _declaration.Parameters.Count;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            Env environment = new Env(interpreter.Globals);

            for (int i = 0; i < _declaration.Parameters.Count; i++)
            {
                environment.Define(_declaration.Parameters[i].Lexeme, arguments[i]);
            }

            interpreter.ExecuteBlock(_declaration.Body, environment);

            return null;
        }

        public override string ToString()
        {
            return $"<funkcja {_declaration.Name.Lexeme}>";
        }
    }
}
