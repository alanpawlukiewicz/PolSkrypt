using PPL.Expressions;

public class Grouping : Expr
{
    private readonly Expr _expression;
    public Expr Expression { get { return _expression; } }
    public Grouping(Expr expression)
    {
        _expression = expression;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitGroupingExpression(this);
    }
}
