using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace PolSkrypt.Contracts
{
    public interface IClassCallable : ICallable
    {
        string Name { get; }
        ICallable? FindStaticMethod(string name);
    }
}
