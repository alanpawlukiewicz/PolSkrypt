using PPL.Contracts;
using PPL.Parsing.Expressions;

namespace PPL.Parsing.Statements
{
    public class StmtExpression : Stmt
    {
        private readonly Expr _expression;

        public Expr Expression { get { return _expression; } }

        public StmtExpression(Expr expression)
        {
            _expression = expression;
        }

        public override T Accept<T>(IStmtVisitor<T> visitor)
        {
            return visitor.VisitExpressionStmt(this);
        }
    }
}
