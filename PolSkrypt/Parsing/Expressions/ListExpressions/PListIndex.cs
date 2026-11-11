using PolSkrypt.Contracts;
using PolSkrypt.Lexing;

namespace PolSkrypt.Parsing.Expressions.ListExpressions
{
    public class PListIndex : Expr
    {
        private readonly Token _keyword;
        private readonly Expr _index;
        private readonly Token _identifier;

        public PListIndex(Token keyword, Expr index, Token identifier)
        {
            _keyword = keyword;
            _index = index;
            _identifier = identifier;
        }

        public Token Keyword => _keyword;

        public Expr Index => _index;

        public Token Identifier => _identifier;

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitListIndexExpression(this);
        }
    }
}
