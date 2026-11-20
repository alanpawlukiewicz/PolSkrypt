using PolSkrypt.Contracts;
using PolSkrypt.Lexing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Parsing.Statements
{
    public class Import : Stmt
    {
        private readonly Token _keyword;
        private readonly Token _path;
        private readonly List<Token>? _imports;
        private readonly Token? _alias;

        public Import(Token path, List<Token>? imports, Token? alias, Token keyword)
        {
            _path = path;
            _imports = imports;
            _alias = alias;
            _keyword = keyword;
        }

        public Token? Alias => _alias;

        public List<Token>? Imports => _imports;

        public Token Path => _path;

        public Token Keyword => _keyword;

        public override T Accept<T>(IStmtVisitor<T> visitor)
        {
            return visitor.VisitImportStmt(this);
        }
    }
}
