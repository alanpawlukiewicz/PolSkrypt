using PolSkrypt.Lexing;
using PolSkrypt.Parsing.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Contracts
{
    public interface IFunction
    {
        List<Token> Parameters { get; }
        List<Stmt> Body { get; }
    }
}
