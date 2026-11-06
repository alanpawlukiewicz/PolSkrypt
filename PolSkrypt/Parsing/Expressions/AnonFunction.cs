using PolSkrypt.Contracts;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing.Expressions;
using PolSkrypt.Parsing.Statements;

public class AnonFunction : Expr, IFunction
{
    public List<Token> Parameters { get; }
    public List<Stmt> Body { get; }

    public AnonFunction(List<Token> parameters, List<Stmt> body)
    {
        Parameters = parameters;
        Body = body;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitAnonFunctionExpression(this);
    }
}