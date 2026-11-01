using PolSkrypt.Contracts;
using PolSkrypt.Parsing.Expressions;

namespace PolSkrypt.Parsing.Statements
{
    public class While : Stmt
    {
        private readonly Expr _condition;
        private readonly Stmt _body;

        public While(Expr condition, Stmt body)
        {
            _condition = condition;
            _body = body;
        }

        public Expr Condition { get { return _condition; } }
        public Stmt Body { get { return _body; } }

        public override T Accept<T>(IStmtVisitor<T> visitor)
        {
            return visitor.VisitWhileStmt(this);
        }
    }
}
