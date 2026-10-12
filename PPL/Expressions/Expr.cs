using System;
using System.Collections.Generic;
using System.Text;

namespace PPL.Expressions
{
    public abstract class Expr
    {
        abstract public T Accept<T>(IVisitor<T> visitor);
    }
}
