using PPL.Contracts;
using PPL.Lexing;
using PPL.Parsing.Expressions;

public class Unary : Expr
{
    private readonly Token _operator;
    private readonly Expr _right;
    public Expr Right { get { return _right; } }
    public Token Operator { get { return _operator; } }
    public Unary(Token op, Expr right)
    {
        _operator = op;
        _right = right;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitUnaryExpression(this);
    }
}
