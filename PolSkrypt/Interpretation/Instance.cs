using PolSkrypt.Interpretation.Callables;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Interpretation
{
    public class Instance
    {
        private readonly ClassCallable _classCallable;

        public Instance(ClassCallable classCallable)
        {
            _classCallable = classCallable;
        }

        public ClassCallable ClassCallable => _classCallable;

        public override string ToString()
        {
            return $"<instancja klasy {_classCallable.Name}>";
        }
    }
}
