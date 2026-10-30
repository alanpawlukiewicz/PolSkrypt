using PPL.Contracts;

namespace PPL.Parsing.Expressions
{
    public class Conditional : Expr
    {
        private readonly Expr _condition;
        private readonly Expr _thenBranch;
        private readonly Expr _elseBranch;

        public Expr Condition { get { return _condition; } }
        public Expr ThenBranch { get { return _thenBranch; } }
        public Expr ElseBranch { get { return _elseBranch; } }
        public Conditional(Expr condition, Expr thenBranch, Expr elseBranch)
        {
            _condition = condition;
            _thenBranch = thenBranch;
            _elseBranch = elseBranch;
        }

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitConditionalExpression(this);
        }
    }
}
