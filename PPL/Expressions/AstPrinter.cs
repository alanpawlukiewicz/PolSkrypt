using System;
using System.Collections.Generic;
using System.Text;

namespace PPL.Expressions
{
    public class AstPrinter : IVisitor<string>
    {
        public string Print(Expression expression)
        {
            return expression.Accept(this);
        }

        private string Parenthesize(string name, List<Expression> exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expression expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }

        public string VisitBinaryExpression(Binary expression)
        {
            return Parenthesize(expression.Operator.Lexeme, new List<Expression> { expression.Left, expression.Right });
        }

        public string VisitGroupingExpression(Grouping expression)
        {
            return Parenthesize("group", new List<Expression> { expression.Expression });
        }

        public string VisitLiteralExpression(Literal expression)
        {
            return expression.Value?.ToString() ?? "null";
        }

        public string VisitUnaryExpression(Unary expression)
        {
            return Parenthesize(expression.Operator.Lexeme, new List<Expression> { expression.Right });
        }
    }
}
