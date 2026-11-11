using PolSkrypt.Contracts;
using PolSkrypt.Lexing;

namespace PolSkrypt.Parsing.Expressions.ListExpressions
{
    public class PListIndexAssign : Expr
    {
        private readonly Expr _value;
        private readonly Expr _index;
        private readonly Token _identifier;

        public PListIndexAssign(Expr value, Token identifier, Expr index)
        {
            _value = value;
            _identifier = identifier;
            _index = index;
        }

        public Expr Value => _value;
        public Token Identifier => _identifier;

        public Expr Index => _index;

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitPListIndexAssignExpression(this);
        }
    }
}
