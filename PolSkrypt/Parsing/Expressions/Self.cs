using PolSkrypt.Contracts;
using PolSkrypt.Lexing;

namespace PolSkrypt.Parsing.Expressions
{
    public class Self : Expr
    {
        private readonly Token _keyword;

        public Self(Token keyword)
        {
            _keyword = keyword;
        }

        public Token Keyword => _keyword;

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitSelfExpression(this);
        }
    }
}
