using PolSkrypt.Contracts;
using PolSkrypt.Lexing;

namespace PolSkrypt.Parsing.Expressions
{
    public class Set : Expr
    {
        private readonly Expr _object;
        private readonly Token _name;
        private readonly Expr _value;

        public Set(Expr obj, Token name, Expr value)
        {
            _object = obj;
            _name = name;
            _value = value;
        }

        public Expr Object => _object;
        public Token Name => _name;
        public Expr Value => _value;

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitSetExpression(this);
        }
    }
}
