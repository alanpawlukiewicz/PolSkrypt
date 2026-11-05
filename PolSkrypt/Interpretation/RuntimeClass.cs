using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Interpretation
{
    public class RuntimeClass
    {
        private readonly string _name;

        public RuntimeClass(string name)
        {
            _name = name;
        }

        public string Name => _name;

        public override string ToString()
        {
            return $"<klasa {_name}>";
        }
    }
}
