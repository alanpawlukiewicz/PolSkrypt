using PPL.Contracts;

namespace PPL.Parsing.Expressions
{
    public abstract class Expr
    {
        abstract public T Accept<T>(IExprVisitor<T> visitor);
    }
}
