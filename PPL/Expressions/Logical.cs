using PPL.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PPL.Expressions
{
    public class Logical : Expr
    {
        private readonly Expr _left;
        private readonly Token _operator;
        private readonly Expr _right;

        public Logical(Expr left, Token @operator, Expr right)
        {
            _left = left;
            _operator = @operator;
            _right = right;
        }

        public Expr Left { get { return _left; } }
        public Expr Right { get { return _right; } }
        public Token Operator { get { return _operator; } }

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitLogicalExpression(this);
        }
    }
}
