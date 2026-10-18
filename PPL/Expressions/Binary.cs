namespace PPL.Expressions
{
    public class Binary : Expr
    {
        private readonly Expr _left;
        private readonly Token _operator;
        private readonly Expr _right;

        public Expr Left { get { return _left; } }
        public Expr Right { get { return _right; } }
        public Token Operator { get { return _operator; } }
        public Binary(Expr left, Token op, Expr right)
        {
            _left = left;
            _operator = op;
            _right = right;
        }

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }
    }
}
