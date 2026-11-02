using PolSkrypt.Contracts;
using PolSkrypt.Lexing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Parsing.Expressions
{
    public class CompoundAssignment : Expr
    {
        private readonly Token _name;
        private readonly Token _op;
        private readonly Expr _value;
        public Expr Value => _value;
        public Token Name => _name;
        public Token Operator => _op;

        public CompoundAssignment(Token name, Token op, Expr value)
        {
            _name = name;
            _op = op;
            _value = value;
        }

        public override T Accept<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitCompoundAssignmentExpression(this);
        }
    }
}
