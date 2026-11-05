using PolSkrypt.Contracts;
using PolSkrypt.Lexing;

namespace PolSkrypt.Parsing.Statements
{
    public class Class : Stmt
    {
        private readonly Token _name;
        private readonly List<Function> _methods;

        public Class(Token name, List<Function> methods)
        {
            _name = name;
            _methods = methods;
        }

        public Token Name => _name;
        public List<Function> Methods => _methods;

        public override T Accept<T>(IStmtVisitor<T> visitor)
        {
            return visitor.VisitClassStmt(this);
        }
    }
}
