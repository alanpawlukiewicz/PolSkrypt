using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Lexing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Interpretation
{
    // Claude: generating class
    public class PModule
    {
        private readonly string _name;
        private readonly string _path;
        private readonly Env _env;

        public string Name => _name;
        public string Path => _path;
        public Env Env => _env;

        public PModule(string name, string path, Env env)
        {
            _name = name;
            _path = path;
            _env = env;
        }

        public object? Get(Token name)
        {
            if (name.Lexeme.StartsWith('_'))
            {
                throw new RuntimeException(name,
                    $"'{name.Lexeme}' jest prywatne w module '{_name}'.");
            }

            if (_env.TryGetOwn(name.Lexeme, out var value))
            {
                return value;
            }

            throw new RuntimeException(name,
                $"Moduł '{_name}' nie zawiera '{name.Lexeme}'.");
        }

        public override string ToString()
        {
            return $"<moduł {_name}>";
        }
    }
}
