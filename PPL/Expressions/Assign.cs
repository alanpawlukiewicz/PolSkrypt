using System;
using System.Collections.Generic;
using System.Text;

namespace PPL.Expressions
{
    public class Assign : Expr
    {
        private readonly Token _name;
        private readonly Expr _value;
        public Expr Value { get { return _value; } }
        public Token Name { get { return _name; } }

        public Assign(Token name, Expr value)
        {
            _name = name;
            _value = value;
        }

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitAssignExpression(this);
        }
    }
}
