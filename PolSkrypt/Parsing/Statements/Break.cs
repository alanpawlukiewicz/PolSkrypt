using PolSkrypt.Contracts;
using PolSkrypt.Lexing;

namespace PolSkrypt.Parsing.Statements
{
    public class Break : Stmt
    {
        private readonly Token _keyword;

        public Break(Token keyword)
        {
            _keyword = keyword;
        }

        public Token KeyWord => _keyword;

        public override T Accept<T>(IStmtVisitor<T> visitor)
        {
            return visitor.VisitBreakStmt(this);
        }
    }
}
