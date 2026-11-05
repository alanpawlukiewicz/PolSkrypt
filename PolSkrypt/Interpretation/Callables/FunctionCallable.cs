using PolSkrypt.Contracts;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing.Statements;

namespace PolSkrypt.Interpretation.Callables
{
    public class FunctionCallable : ICallable
    {
        private readonly Function _declaration;
        private readonly Env _closure;
        private readonly bool _isInitializer;

        public FunctionCallable(Function declaration, Env closure, bool isInitializer = false)
        {
            _declaration = declaration;
            _closure = closure;
            _isInitializer = isInitializer;
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
                if (_isInitializer)
                {
                    return _closure.GetAt(0, new Token(TokenType.SELF, "ja", null, 0));
                }

                return returnValue.Value;
            }

            if (_isInitializer)
            {
                return _closure.GetAt(0, new Token(TokenType.SELF, "ja", null, 0));
            }


            return null;
        }

        public override string ToString()
        {
            return $"<funkcja {_declaration.Name.Lexeme}>";
        }

        public FunctionCallable Bind(Instance instance)
        {
            Env environment = new Env(_closure);
            environment.Define("ja", instance);
            return new FunctionCallable(_declaration, environment, _isInitializer);
        }
    }
}
