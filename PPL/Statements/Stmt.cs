using PPL.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PPL.Statements
{
    public abstract class Stmt
    {
        abstract public T Accept<T>(IStmtVisitor<T> visitor);
    }
}
