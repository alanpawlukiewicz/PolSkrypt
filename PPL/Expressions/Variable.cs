using PPL.Contracts;

namespace PPL.Expressions
{
    public class Variable : Expr
    {
        private readonly Token _name;

        public Token Name { get { return _name; } }

        public Variable(Token name)
        {
            _name = name;
        }

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitVariableExpression(this);
        }
    }
}
