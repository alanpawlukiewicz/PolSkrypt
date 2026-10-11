using PPL;
using PPL.Expressions;

public class Unary : Expression
{
    private readonly Token _operator;
    private readonly Expression _right;
    public Expression Right { get { return _right; } }
    public Token Operator { get { return _operator; } }
    public Unary(Token op, Expression right)
    {
        _operator = op;
        _right = right;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitUnaryExpression(this);
    }
}
