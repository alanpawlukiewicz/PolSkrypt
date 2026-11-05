using PolSkrypt.Contracts;
using PolSkrypt.Lexing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Parsing.Expressions
{
    public class Get : Expr
    {
        private readonly Expr _object;
        private readonly Token _name;

        public Get(Expr obj, Token name)
        {
            _object = obj;
            _name = name;
        }

        public Expr Object => _object;
        public Token Name => _name;

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitGetExpression(this);
        }
    }
}
