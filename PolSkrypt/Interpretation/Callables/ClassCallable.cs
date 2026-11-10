using PolSkrypt.Constants;
using PolSkrypt.Contracts;

namespace PolSkrypt.Interpretation.Callables
{
    public class ClassCallable : ICallable
    {
        private readonly string _name;
        private readonly Dictionary<string, FunctionCallable> _methods;
        private readonly Dictionary<string, FunctionCallable> _staticMethods;
        private readonly ClassCallable? _superclass;


        public ClassCallable(string name, Dictionary<string, FunctionCallable> methods, Dictionary<string, FunctionCallable> staticMethods, ClassCallable? superclass)
        {
            _name = name;
            _methods = methods;
            _staticMethods = staticMethods;
            _superclass = superclass;
        }

        public string Name => _name;

        public int? Arity => FindMethod(Keywords.CONSTRUCTOR)?.Arity ?? 0;

        public FunctionCallable? FindStaticMethod(string name)
        {
            if (_staticMethods.ContainsKey(name))
            {
                return _staticMethods[name];
            }

            if (_superclass != null)
            {
                return _superclass.FindStaticMethod(name);
            }

            return null;
        }

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
            if (_methods.ContainsKey(name))
            {
                return _methods[name];
            }

            if (_superclass != null)
            {
                return _superclass.FindMethod(name);
            }

            return null;
        }
    }
}
