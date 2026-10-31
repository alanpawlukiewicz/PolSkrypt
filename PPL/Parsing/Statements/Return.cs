using PPL.Contracts;
using PPL.Lexing;
using PPL.Parsing.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PPL.Parsing.Statements
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
