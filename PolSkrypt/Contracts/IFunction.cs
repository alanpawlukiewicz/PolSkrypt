using PolSkrypt.Lexing;
using PolSkrypt.Parsing.Statements;

namespace PolSkrypt.Contracts
{
    public interface IFunction
    {
        List<Token> Parameters { get; }
        List<Stmt> Body { get; }
    }
}
