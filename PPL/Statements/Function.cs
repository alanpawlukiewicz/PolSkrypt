using PPL.Contracts;

namespace PPL.Statements
{
    public class Function : Stmt
    {
        private readonly Token _name;
        private readonly List<Token> _parameters;
        private readonly List<Stmt> _body;

        public Function(Token name, List<Token> parameters, List<Stmt> body)
        {
            _name = name;
            _parameters = parameters;
            _body = body;
        }

        public Token Name => _name;
        public List<Token> Parameters => _parameters;
        public List<Stmt> Body => _body;

        public override T Accept<T>(IStmtVisitor<T> visitor)
        {
            return visitor.VisitFunctionStmt(this);
        }
    }
}
