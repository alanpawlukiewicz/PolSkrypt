namespace PPL.Statements
{
    public class Block : Stmt
    {
        private readonly List<Stmt> _statements;

        public List<Stmt> Statements { get { return _statements; } }

        public Block(List<Stmt> statements)
        {
            _statements = statements;
        }

        public Block()
        {
            _statements = new() { };
        }

        public void AddStatement(Stmt statement)
        {
            _statements.Add(statement);
        }

        public override T Accept<T>(IStmtVisitor<T> visitor)
        {
            return visitor.VisitBlockStmt(this);
        }
    }
}
