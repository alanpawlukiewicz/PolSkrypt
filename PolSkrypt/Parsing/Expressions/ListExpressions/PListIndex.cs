using PolSkrypt.Contracts;
using PolSkrypt.Lexing;

namespace PolSkrypt.Parsing.Expressions.ListExpressions
{
    public class PListIndex : Expr
    {
        private readonly Token _bracket;
        private readonly Expr _target;
        private readonly Expr _index;

        public PListIndex(Token bracket, Expr target, Expr index)
        {
            _bracket = bracket;
            _target = target;
            _index = index;
        }

        public Token Bracket => _bracket;

        public Expr Target => _target;

        public Expr Index => _index;

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitListIndexExpression(this);
        }
    }
}
