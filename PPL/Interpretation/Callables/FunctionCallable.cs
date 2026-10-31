using PPL.Contracts;
using PPL.Errors.Exceptions;
using PPL.Parsing.Statements;

namespace PPL.Interpretation.Callables
{
    public class FunctionCallable : ICallable
    {
        private readonly Function _declaration;
        private readonly Env _closure;

        public FunctionCallable(Function declaration, Env closure)
        {
            _declaration = declaration;
            _closure = closure;
        }

        public int? Arity => _declaration.Parameters.Count;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            Env environment = new Env(_closure);

            for (int i = 0; i < _declaration.Parameters.Count; i++)
            {
                environment.Define(_declaration.Parameters[i].Lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (ReturnException returnValue)
            {
                return returnValue.Value;
            }
            

            return null;
        }

        public override string ToString()
        {
            return $"<funkcja {_declaration.Name.Lexeme}>";
        }
    }
}
