using PPL.Constants;
using System.Text;

namespace PPL.Expressions
{
    public class AstPrinter : IExprVisitor<string>
    {
        public string Print(Expr expression)
        {
            return expression.Accept(this);
        }

        private string Parenthesize(string name, List<Expr> exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expr expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }

        public string VisitBinaryExpression(Binary expression)
        {
            return Parenthesize(expression.Operator.Lexeme, new List<Expr> { expression.Left, expression.Right });
        }

        public string VisitGroupingExpression(Grouping expression)
        {
            return Parenthesize("group", new List<Expr> { expression.Expression });
        }

        public string VisitLiteralExpression(Literal expression)
        {
            return expression.Value?.ToString() ?? KeyWord.NULL;
        }

        public string VisitUnaryExpression(Unary expression)
        {
            return Parenthesize(expression.Operator.Lexeme, new List<Expr> { expression.Right });
        }

        public string VisitConditionalExpression(Conditional expression)
        {
            throw new NotImplementedException();
        }

        public string VisitVariableExpression(Variable expression)
        {
            throw new NotImplementedException();
        }

        public string VisitAssignExpression(Assign expression)
        {
            throw new NotImplementedException();
        }

        public string VisitLogicalExpression(Logical expression)
        {
            throw new NotImplementedException();
        }

        public string VisitCallExpression(Call expression)
        {
            throw new NotImplementedException();
        }
    }
}
