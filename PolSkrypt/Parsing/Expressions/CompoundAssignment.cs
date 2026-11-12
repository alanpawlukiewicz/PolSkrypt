using PolSkrypt.Contracts;
using PolSkrypt.Lexing;

namespace PolSkrypt.Parsing.Expressions
{
    public class CompoundAssignment : Expr
    {
        private readonly Token _op;
        private readonly Expr _target;
        private readonly Expr _value;

        public CompoundAssignment(Token op, Expr target, Expr value)
        {
            _op = op;
            _target = target;
            _value = value;
        }

        public Token Operator => _op;

        public Expr Target => _target;

        public Expr Value => _value;

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitCompoundAssignmentExpression(this);
        }
    }
}
