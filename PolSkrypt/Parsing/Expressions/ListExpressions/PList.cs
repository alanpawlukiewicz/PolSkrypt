using PolSkrypt.Contracts;
using PolSkrypt.Lexing;

namespace PolSkrypt.Parsing.Expressions.ListExpressions
{
    public class PList : Expr
    {
        private readonly Token _keyword;
        private readonly List<Expr> _items;

        public PList(Token keyword, List<Expr> items)
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
