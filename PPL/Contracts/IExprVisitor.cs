using PPL.Parsing.Expressions;

namespace PPL.Contracts
{
    public interface IExprVisitor<T>
    {
        T VisitBinaryExpression(Binary expression);
        T VisitGroupingExpression(Grouping expression);
        T VisitLiteralExpression(Literal expression);
        T VisitUnaryExpression(Unary expression);
        T VisitConditionalExpression(Conditional expression);
        T VisitVariableExpression(Variable expression);
        T VisitAssignExpression(Assign expression);
        T VisitLogicalExpression(Logical expression);
        T VisitCallExpression(Call expression);
        T VisitAnonFunctionExpression(AnonFunction expression);
    }
}
