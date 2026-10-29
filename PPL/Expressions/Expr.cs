namespace PPL.Expressions
{
    public abstract class Expr
    {
        abstract public T Accept<T>(IExprVisitor<T> visitor);
    }
}
