using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Interpretation.Callables;
using PolSkrypt.Lexing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Interpretation
{
    public class Instance
    {
        private readonly ClassCallable _classCallable;
        private readonly Dictionary<string, object?> _fields = new Dictionary<string, object?>();

        public Instance(ClassCallable classCallable)
        {
            _classCallable = classCallable;
        }

        public ClassCallable ClassCallable => _classCallable;

        public override string ToString()
        {
            return $"<instancja klasy {_classCallable.Name}>";
        }

        public object? Get(Token name)
        {
            if (_fields.ContainsKey(name.Lexeme))
            {
                return _fields[name.Lexeme];
            }

            throw new RuntimeException(name, $"Nie znaleziono właściwości ani metody o nazwie '{name.Lexeme}'.");
        }

        public void Set(Token name, object? value)
        {
            _fields[name.Lexeme] = value;
        }
    }
}
