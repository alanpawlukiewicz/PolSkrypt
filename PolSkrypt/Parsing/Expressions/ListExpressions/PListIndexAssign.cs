using PolSkrypt.Contracts;
using PolSkrypt.Lexing;

namespace PolSkrypt.Parsing.Expressions.ListExpressions
{
    public class PListIndexAssign : Expr
    {
        private readonly Token _bracket;
        private readonly Expr _target;
        private readonly Expr _index;
        private readonly Expr _value;

        public PListIndexAssign(Token bracket, Expr target, Expr index, Expr value)
        {
            _bracket = bracket;
            _target = target;
            _index = index;
            _value = value;
        }

        public Token Bracket => _bracket;

        public Expr Target => _target;

        public Expr Index => _index;

        public Expr Value => _value;

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitPListIndexAssignExpression(this);
        }
    }
}
