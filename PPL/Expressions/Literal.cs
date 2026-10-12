using PPL.Expressions;

public class Literal : Expr
{
    private readonly object? _value;
    public object? Value { get { return _value; } }
    public Literal(object? value)
    {
        _value = value;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitLiteralExpression(this);
    }
}
