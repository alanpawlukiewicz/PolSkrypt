using PolSkrypt.Contracts;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Parsing.Statements
{
    public class Return : Stmt
    {
        private readonly Token _keyword;
        private readonly Expr? _value;

        public Return(Token keyword, Expr? value)
        {
            _keyword = keyword;
            _value = value;
        }

        public Token KeyWord => _keyword;
        public Expr? Value => _value;

        public override T Accept<T>(IStmtVisitor<T> visitor)
        {
            return visitor.VisitReturnStmt(this);
        }
    }
}
