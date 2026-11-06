using PolSkrypt.Errors;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Lexing;

namespace PolSkrypt.Interpretation
{
    public class Env
    {
        private readonly Env? _enclosing;
        private readonly Dictionary<string, object?> _values = new();

        public Env()
        {
            _enclosing = null;
        }

        public Env(Env enclosing)
        {
            _enclosing = enclosing;
        }

        public void Define(string name, object? value)
        {
            if (_values.ContainsKey(name))
            {
                ErrorHandler.Error(name, "Zmienna o tej nazwie już istnieje w tym zakresie.");
                return;
            }
            _values.Add(name, value);
        }

        public object? Get(Token name)
        {
            if (_values.TryGetValue(name.Lexeme, out var match))
            {
                return match;
            }

            if (_enclosing != null)
            {
                return _enclosing.Get(name);
            }

            throw new RuntimeException(name, $"Niezidentyfikowana zmienna '{name.Lexeme}'.");
        }

        public void Assign(Token name, object? value)
        {
            if (_values.ContainsKey(name.Lexeme))
            {
                _values[name.Lexeme] = value;
                return;
            }

            if (_enclosing != null)
            {
                _enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeException(name, $"Niezidentyfikowana zmienna '{name.Lexeme}'.");
        }

        public object? GetAt(int distance, Token name)
        {
            return Ancestor(distance, name)._values[name.Lexeme];
        }

        public Env Ancestor(int distance, Token name)
        {
            Env environment = this;
            for (int i = 0; i < distance; i++)
            {
                environment = environment._enclosing
                    ?? throw new RuntimeException(name, $"Niezidentyfikowana zmienna '{name.Lexeme}'.");
            }

            return environment;
        }

        public void AssignAt(int distance, Token name, object? value)
        {
            Ancestor(distance, name)._values[name.Lexeme] = value;
        }
    }
}
