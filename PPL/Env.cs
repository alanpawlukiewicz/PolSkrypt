using PPL.Exceptions;

namespace PPL
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
    }
}
