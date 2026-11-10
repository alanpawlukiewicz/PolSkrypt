using PolSkrypt.Contracts;
using PolSkrypt.Lexing;

namespace PolSkrypt.Parsing.Expressions
{
    public class PListIndex : Expr
    {
        private readonly Token _keyword;
        private readonly Expr _value;
        private readonly Token _identifier;

        public PListIndex(Token keyword, Expr value, Token identifier)
        {
            _keyword = keyword;
            _value = value;
            _identifier = identifier;
        }

        public Token Keyword => _keyword;

        public Expr Value => _value;

        public Token Identifier => _identifier;

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitListIndexExpression(this);
        }
    }
}
