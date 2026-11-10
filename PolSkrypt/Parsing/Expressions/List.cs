using PolSkrypt.Contracts;
using PolSkrypt.Lexing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Parsing.Expressions
{
    public class List : Expr
    {
        private readonly Token _keyword;
        private readonly List<Expr> _items;

        public List(Token keyword, List<Expr> items)
        {
            _keyword = keyword;
            _items = items;
        }

        public Token Keyword => _keyword;

        public List<Expr> Items => _items;

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitListExpression(this);
        }
    }
}
