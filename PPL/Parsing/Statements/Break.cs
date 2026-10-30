using PPL.Contracts;

namespace PPL.Parsing.Statements
{
    public class Break : Stmt
    {
        public override T Accept<T>(IStmtVisitor<T> visitor)
        {
            return visitor.VisitBreakStmt(this);
        }
    }
}
