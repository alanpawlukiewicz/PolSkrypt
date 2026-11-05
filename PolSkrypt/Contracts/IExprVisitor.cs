using PolSkrypt.Parsing.Expressions;

namespace PolSkrypt.Contracts
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
        T VisitCompoundAssignmentExpression(CompoundAssignment expression);
        T VisitGetExpression(Get expression);
        T VisitSetExpression(Set expression);
        T VisitSelfExpression(Self expression);
    }
}
