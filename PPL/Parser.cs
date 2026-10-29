using PPL.Enums;
using PPL.Exceptions;
using PPL.Expressions;
using PPL.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace PPL
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private readonly bool _promptMode;
        private int _current = 0;

        public Parser(List<Token> tokens, bool promptMode)
        {
            _tokens = tokens;
            _promptMode = promptMode;
        }

        public List<Stmt> Parse()
        {
            List<Stmt> statements = new();
            while (!IsAtEnd())
            {
                statements.AddRange(Declaration());
            }

            return statements;
        }

        private List<Stmt> Declaration()
        {
            try
            {
                if (Match(TokenType.VAR))
                {
                    return VarDeclaration();
                }

                return new List<Stmt> { Statement() };
            }
            catch (ParseException)
            {
                Synchronize();
                return new();
            }
        }

        private List<Stmt> VarDeclaration()
        {
            var declarations = new List<Stmt>() { };
            do
            {
                var name = Consume(TokenType.IDENTIFIER, "Oczekiwano nazwy zmiennej.");

                Expr? initializer = null;
                if (Match(TokenType.EQUAL))
                {
                    initializer = Assignment();
                }

                declarations.Add(new Var(name, initializer));
            }
            while (Match(TokenType.COMMA));

            Consume(TokenType.SEMICOLON, "Oczekiwano ';' po deklaracji zmiennej.");

            return declarations;
        }

        private Stmt Statement()
        {
            if (Match(TokenType.IF))
            {
                return IfStatement();
            }
            if (Match(TokenType.PRINT))
            {
                return PrintStatement();
            }
            if (Match(TokenType.LEFT_BRACE))
            {
                return new Block(Block());
            }

            if (_promptMode)
            {
                return PrintWithoutKeywordStatement();
            }

            return ExpressionStatement();
        }

        private List<Stmt> Block()
        {
            List<Stmt> statements = new();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.AddRange(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Oczekiwano '}' po bloku.");
            return statements;
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Oczekiwano ';' po wartości.");
            return new Print(value);
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Oczekiwano znaku '(' po 'jeżeli'");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Oczekiwano znaku ')' po warunku jeżeli");

            Stmt thenBranch = Statement();
            Stmt? elseBranch = null;
            if (Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new If(condition, thenBranch, elseBranch);
        }

        private Stmt PrintWithoutKeywordStatement()
        {
            Expr value = Expression();
            if (Check(TokenType.SEMICOLON))
            {
                Advance();
            }
            return new Print(value);
        }

        private Stmt ExpressionStatement()
        {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Oczekiwano ';' po wartości.");
            return new StmtExpression(value);
        }

        private Expr Expression()
        {
            return Comma();
        }

        private Expr Comma()
        {
            Expr expr = Assignment();
            if (Match(TokenType.COMMA))
            {
                Token op = Previous();
                Expr right = Assignment();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Assignment()
        {
            Expr expr = Ternary();
            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr is Variable)
                {
                    Token name = ((Variable)expr).Name;
                    return new Assign(name, value);
                }

                ErrorHandler.Error(equals, "Nieprawidłowy cel przypisania.");
            }
            return expr;
        }

        private Expr Ternary()
        {
            Expr expr = Equality();
            while (Match(TokenType.QUESTION_MARK))
            {
                Expr thenStatement = Expression();
                Consume(TokenType.COLON, "Oczekiwano ':' po gałęzi.");
                Expr elseStatement = Ternary();
                expr = new Conditional(expr, thenStatement, elseStatement);
            }
            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token op = Previous();
                Expr right = Comparison();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd())
            {
                _current++;
            }
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        private Expr Comparison()
        {
            Expr expr = Term();
            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token op = Previous();
                Expr right = Term();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Term()
        {
            Expr expr = Factor();

            while (Match(TokenType.MINUS, TokenType.PLUS, TokenType.DOT))
            {
                Token op = Previous();
                Expr right = Factor();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Factor()
        {
            Expr expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                Token op = Previous();
                Expr right = Unary();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Unary()
        {
            while (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token op = Previous();
                Expr right = Unary();
                return new Unary(op, right);
            }
            return Primary();
        }

        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Literal(false);
            if (Match(TokenType.TRUE)) return new Literal(true);
            if (Match(TokenType.NULL)) return new Literal(null);
            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Literal(Previous().Literal);
            }
            if (Match(TokenType.IDENTIFIER))
            {
                return new Variable(Previous());
            }
            if (Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Oczekiwano ')' po wyrażeniu.");
                return new Grouping(expr);
            }
            throw new ParseException(Peek(), "Oczekiwano wyrażenia.");
        }

        private Token Consume(TokenType type, string msg)
        {
            if (Check(type))
            {
                return Advance();
            }
            throw new ParseException(Peek(), msg);
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Type == TokenType.SEMICOLON) return;
                switch (Peek().Type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }
                Advance();
            }
        }
    }
}
