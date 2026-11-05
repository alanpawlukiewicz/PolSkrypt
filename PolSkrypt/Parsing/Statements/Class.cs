using PolSkrypt.Contracts;
using PolSkrypt.Lexing;

namespace PolSkrypt.Parsing.Statements
{
    public class Class : Stmt
    {
        private readonly Token _name;
        private readonly List<Function> _methods;
        private readonly List<Function> _staticMethods;

        public Class(Token name, List<Function> methods, List<Function> staticMethods)
        {
            _name = name;
            _methods = methods;
            _staticMethods = staticMethods;
        }

        public Token Name => _name;
        public List<Function> Methods => _methods;
        public List<Function> StaticMethods => _staticMethods;

        public override T Accept<T>(IStmtVisitor<T> visitor)
        {
            return visitor.VisitClassStmt(this);
        }
    }
}
