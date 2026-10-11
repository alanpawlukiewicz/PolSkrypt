using PPL.Expressions;

public class Grouping : Expression
{
    private readonly Expression _expression;
    public Expression Expression { get { return _expression; } }
    public Grouping(Expression expression)
    {
        _expression = expression;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitGroupingExpression(this);
    }
}
