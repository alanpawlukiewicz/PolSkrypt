using PPL.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PPL.Expressions
{
    public class Call : Expr
    {
        private readonly Expr _callee;
        private readonly Token _paren;
        private readonly List<Expr> _arguments;

        public Expr Callee { get { return _callee; } }
        public Token Paren { get { return _paren; } }
        public List<Expr> Arguments { get { return _arguments; } }

        public Call(Expr callee, Token paren, List<Expr> arguments)
        {
            _callee = callee;
            _paren = paren;
            _arguments = arguments;
        }

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitCallExpression(this);
        }
    }
}
