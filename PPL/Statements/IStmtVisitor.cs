using PPL.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PPL.Statements
{
    public interface IStmtVisitor<T>
    {
        T VisitExpressionStmt(StmtExpression statement);
        T VisitPrintStmt(Print statement);
        T VisitVarStmt(Var statement);
        T VisitBlockStmt(Block statement);
        T VisitIfStmt(If statement);
    }
}
