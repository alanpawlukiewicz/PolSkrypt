namespace PPL.Expressions
{
    public class Binary : Expression
    {
        private readonly Expression _left;
        private readonly Token _operator;
        private readonly Expression _right;

        public Expression Left { get { return _left; } }
        public Expression Right { get { return _right; } }
        public Token Operator { get { return _operator; } }
        public Binary(Expression left, Token op, Expression right)
        {
            _left = left;
            _operator = op;
            _right = right;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }
    }
}
