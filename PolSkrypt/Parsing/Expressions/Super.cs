using PolSkrypt.Contracts;
using PolSkrypt.Lexing;

namespace PolSkrypt.Parsing.Expressions
{
    public class Super : Expr
    {
        private readonly Token _keyword;
        private readonly Token _method;

        public Super(Token keyword, Token method)
        {
            _keyword = keyword;
            _method = method;
        }

        public Token Keyword => _keyword;

        public Token Method => _method;

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitSuperExpression(this);
        }
    }
}
