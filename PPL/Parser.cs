using PPL.Constants;
using PPL.Enums;
using PPL.Exceptions;
using PPL.Expressions;
using PPL.Statements;

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
                if (Match(TokenType.FUN))
                {
                    return new List<Stmt> { Function("funkcja") };
                }
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

        private Function Function(string kind)
        {
            Token name = Consume(TokenType.IDENTIFIER,
                $"Oczekiwano nazwy dla '{kind}'.");
            Consume(TokenType.LEFT_PAREN, $"Oczekiwano '(' po nazwie dla '{kind}'");
            List<Token> parameters = new List<Token> { };
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= FunctionConstants.MAX_PARAMS_COUNT)
                    {
                        ErrorHandler.Error(Peek(), $"Funkcja nie może mieć więcej niż {FunctionConstants.MAX_PARAMS_COUNT} argumentów.");
                    }

                    parameters.Add(Consume(TokenType.IDENTIFIER, "Oczekiwano nazwy parametru."));
                } while (Match(TokenType.COMMA));
            }

            Consume(TokenType.RIGHT_PAREN, "Oczekiwano ')' po parametrach.");
            Consume(TokenType.LEFT_BRACE, $"Oczekiwano '{{' przed ciałem '{kind}'");
            List<Stmt> body = Block();
            return new Function(name, parameters, body);
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
            if (Match(TokenType.WHILE))
            {
                return WhileStatement();
            }
            if (Match(TokenType.FOR))
            {
                return ForStatement();
            }
            if (Match(TokenType.PRINT))
            {
                return PrintStatement();
            }
            if (Match(TokenType.LEFT_BRACE))
            {
                return new Block(Block());
            }
            if (Match(TokenType.BREAK))
            {
                return Break();
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

        private Stmt WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Oczekiwano '(' po 'podczas_gdy'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Oczekiwano ')' po warunku podczas_gdy.");
            Stmt body = Statement();

            return new While(condition, body);
        }

        private Stmt Break()
        {
            Consume(TokenType.SEMICOLON, "Oczekiwano ';' po 'przerwij'.");

            return new Break();
        }

        private Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Oczekiwano '(' po 'dla'.");
            List<Stmt>? initializer = Match(TokenType.SEMICOLON) 
                ? null
                : Match(TokenType.VAR) 
                    ? VarDeclaration() 
                    : new List<Stmt>{ ExpressionStatement() };

            Expr? condition = Check(TokenType.SEMICOLON) ? null : Expression();
            Consume(TokenType.SEMICOLON, "Oczekiwano ';' po warunku pętli.");

            Expr? increment = Check(TokenType.RIGHT_PAREN) ? null : Expression();
            Consume(TokenType.RIGHT_PAREN, "Oczekiwano ')'.");

            Stmt body = Statement();

            if (increment != null)
            {
                body = new Block(new List<Stmt> { body, new StmtExpression(increment)});
            }

            if (condition == null)
            {
                condition = new Literal(true);
            }

            body = new While(condition, body);

            if (initializer != null)
            {
                var newBlock = new Block();
                foreach (var init in initializer)
                {
                    newBlock.AddStatement(init);
                }
                newBlock.AddStatement(body);

                body = newBlock;
            }

            return body;
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
                return new StmtExpression(value);
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
            return Assignment();
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
            Expr expr = Or();
            while (Match(TokenType.QUESTION_MARK))
            {
                Expr thenStatement = Expression();
                Consume(TokenType.COLON, "Oczekiwano ':' po gałęzi.");
                Expr elseStatement = Ternary();
                expr = new Conditional(expr, thenStatement, elseStatement);
            }
            return expr;
        }

        private Expr Or()
        {
            Expr expr = And();

            while (Match(TokenType.OR))
            {
                Token op = Previous();
                Expr right = And();
                expr = new Logical(expr, op, right);
            }

            return expr;
        }

        private Expr And()
        {
            Expr expr = Equality();

            while (Match(TokenType.AND))
            {
                Token op = Previous();
                Expr right = Equality();
                expr = new Logical(expr, op, right);
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
            return Call();
        }

        private Expr Call()
        {
            Expr expr = Primary();

            while (true)
            {
                if (Match(TokenType.LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            List<Expr> arguments = new() { };
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count >= FunctionConstants.MAX_PARAMS_COUNT)
                    {
                        ErrorHandler.Error(Peek(), $"Funkcja nie może mieć więcej niż {FunctionConstants.MAX_PARAMS_COUNT} argumentów.");
                    }
                    arguments.Add(Expression());
                } while (Match(TokenType.COMMA));
            }

            Token paren = Consume(TokenType.RIGHT_PAREN, "Oczekiwano ')' po liscie argumentów.");

            return new Call(callee, paren, arguments);
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
