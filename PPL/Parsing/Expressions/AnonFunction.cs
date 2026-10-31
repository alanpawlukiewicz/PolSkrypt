using PPL.Contracts;
using PPL.Lexing;
using PPL.Parsing.Expressions;
using PPL.Parsing.Statements;

public class AnonFunction : Expr
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