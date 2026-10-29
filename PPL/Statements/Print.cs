using PPL.Expressions;

namespace PPL.Statements
{
    public class Print : Stmt
    {
        private readonly Expr _expression;

        public Expr Expression { get { return _expression; } }

        public Print(Expr expression)
        {
            _expression = expression;
        }

        public override T Accept<T>(IStmtVisitor<T> visitor)
        {
            return visitor.VisitPrintStmt(this);
        }
    }
}
