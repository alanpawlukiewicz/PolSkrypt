using PolSkrypt.Contracts;

namespace PolSkrypt.Parsing.Statements
{
    public abstract class Stmt
    {
        abstract public T Accept<T>(IStmtVisitor<T> visitor);
    }
}
