using PolSkrypt.Contracts;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing.Expressions;

namespace PolSkrypt.Parsing.Statements
{
    public class Class : Stmt
    {
        private readonly Token _name;
        private readonly List<Function> _methods;
        private readonly List<Function> _staticMethods;
        private readonly Variable? _superclass;

        public Class(Token name, List<Function> methods, List<Function> staticMethods, Variable? superclass)
        {
            _name = name;
            _methods = methods;
            _staticMethods = staticMethods;
            _superclass = superclass;
        }

        public Token Name => _name;
        public List<Function> Methods => _methods;
        public List<Function> StaticMethods => _staticMethods;
        public Variable? Superclass => _superclass;

        public override T Accept<T>(IStmtVisitor<T> visitor)
        {
            return visitor.VisitClassStmt(this);
        }
    }
}
