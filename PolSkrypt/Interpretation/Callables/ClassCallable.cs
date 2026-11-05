using PolSkrypt.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Interpretation.Callables
{
    public class ClassCallable : ICallable
    {
        private readonly string _name;
        private readonly Dictionary<string, FunctionCallable> _methods;

        public ClassCallable(string name, Dictionary<string, FunctionCallable> methods)
        {
            _name = name;
            _methods = methods;
        }

        public string Name => _name;

        public int? Arity => FindMethod("konstruktor")?.Arity ?? 0;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            Instance instance = new Instance(this);
            FunctionCallable? ctor = FindMethod("konstruktor");
            if (ctor != null)
            {
                ctor.Bind(instance).Call(interpreter, arguments);
            }
            return instance;
        }

        public override string ToString()
        {
            return $"<klasa {_name}>";
        }

        public FunctionCallable? FindMethod(string name)
        {
            return _methods.ContainsKey(name) ? _methods[name] : null;
        }
    }
}
