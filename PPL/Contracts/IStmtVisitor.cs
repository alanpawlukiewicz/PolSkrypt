using PPL.Parsing.Statements;

namespace PPL.Contracts
{
    public interface IStmtVisitor<T>
    {
        T VisitExpressionStmt(StmtExpression statement);
        T VisitPrintStmt(Print statement);
        T VisitVarStmt(Var statement);
        T VisitBlockStmt(Block statement);
        T VisitIfStmt(If statement);
        T VisitWhileStmt(While statement);
        T VisitBreakStmt(Break statement);
        T VisitFunctionStmt(Function statement);
    }
}
