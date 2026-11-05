using PolSkrypt.Constants;
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
        private readonly Dictionary<string, FunctionCallable> _staticMethods;


        public ClassCallable(string name, Dictionary<string, FunctionCallable> methods, Dictionary<string, FunctionCallable> staticMethods)
        {
            _name = name;
            _methods = methods;
            _staticMethods = staticMethods;
        }

        public string Name => _name;

        public int? Arity => FindMethod(Keywords.CONSTRUCTOR)?.Arity ?? 0;

        public FunctionCallable? FindStaticMethod(string name) => _staticMethods.TryGetValue(name, out var m) ? m : null;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            Instance instance = new Instance(this);
            FunctionCallable? ctor = FindMethod(Keywords.CONSTRUCTOR);
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
