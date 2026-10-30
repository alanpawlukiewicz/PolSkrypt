using PPL.Contracts;
using PPL.Parsing.Expressions;

namespace PPL.Parsing.Statements
{
    public class If : Stmt
    {
        private readonly Expr _condition;
        private readonly Stmt _thenBranch;
        private readonly Stmt? _elseBranch;

        public If(Expr condition, Stmt thenBranch, Stmt? elseBranch)
        {
            _condition = condition;
            _thenBranch = thenBranch;
            _elseBranch = elseBranch;
        }

        public Expr Condition { get { return _condition; } }
        public Stmt ThenBranch { get { return _thenBranch; } }
        public Stmt? ElseBranch { get { return _elseBranch; } }

        public override T Accept<T>(IStmtVisitor<T> visitor)
        {
            return visitor.VisitIfStmt(this);
        }
    }
}
