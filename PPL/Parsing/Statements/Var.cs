using PPL.Contracts;
using PPL.Lexing;
using PPL.Parsing.Expressions;

namespace PPL.Parsing.Statements
{
    public class Var : Stmt
    {
        private readonly Token _name;
        private readonly Expr? _initializer;

        public Token Name { get { return _name; } }
        public Expr? Initializer { get { return _initializer; } }

        public Var(Token name, Expr? initializer)
        {
            _name = name;
            _initializer = initializer;
        }

        public override T Accept<T>(IStmtVisitor<T> visitor)
        {
            return visitor.VisitVarStmt(this);
        }
    }
}
