using PPL.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

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
