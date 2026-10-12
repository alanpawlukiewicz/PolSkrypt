using System;
using System.Collections.Generic;
using System.Text;

namespace PPL.Expressions
{
    public interface IVisitor<T>
    {
        T VisitBinaryExpression(Binary expression);
        T VisitGroupingExpression(Grouping expression);
        T VisitLiteralExpression(Literal expression);
        T VisitUnaryExpression(Unary expression);
        T VisitConditionalExpression(Conditional expression);
    }
}
