using FluentAssertions;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing;
using PolSkrypt.Parsing.Expressions;
using PolSkrypt.Parsing.Statements;

namespace PolSkrypt.Tests
{
    public class ParserTests
    {
        private static Token T(TokenType type, string lexeme, object? literal = null, int line = 1)
                => new Token(type, lexeme, literal, line);

        public static IEnumerable<object[]> GetParserTestData()
        {
            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.VAR, "zmienna"), T(TokenType.IDENTIFIER, "x"), T(TokenType.EQUAL, "="),
                    T(TokenType.NUMBER, "5", 5), T(TokenType.SEMICOLON, ";"), T(TokenType.EOF, "")
                },
                new Stmt[]
                {
                    new Var(T(TokenType.IDENTIFIER, "x"), new Literal(5))
                }
            };

            // Claude: Generating test data for multiple test cases
            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.VAR, "zmienna"),
                    T(TokenType.IDENTIFIER, "a"), T(TokenType.EQUAL, "="), T(TokenType.NUMBER, "1", 1),
                    T(TokenType.COMMA, ","),
                    T(TokenType.IDENTIFIER, "b"),
                    T(TokenType.SEMICOLON, ";"), T(TokenType.EOF, "")
                },
                new Stmt[]
                {
                    new Var(T(TokenType.IDENTIFIER, "a"), new Literal(1)),
                    new Var(T(TokenType.IDENTIFIER, "b"), null)
                }
                    };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.VAR, "zmienna"), T(TokenType.IDENTIFIER, "x"), T(TokenType.EQUAL, "="),
                    T(TokenType.NUMBER, "1", 1), T(TokenType.PLUS, "+"),
                    T(TokenType.NUMBER, "2", 2), T(TokenType.STAR, "*"), T(TokenType.NUMBER, "3", 3),
                    T(TokenType.SEMICOLON, ";"), T(TokenType.EOF, "")
                },
                new Stmt[]
                {
                    new Var(
                        T(TokenType.IDENTIFIER, "x"),
                        new Binary(
                            new Literal(1),
                            T(TokenType.PLUS, "+"),
                            new Binary(new Literal(2), T(TokenType.STAR, "*"), new Literal(3))))
                }
    };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.VAR, "zmienna"), T(TokenType.IDENTIFIER, "x"), T(TokenType.EQUAL, "="),
                    T(TokenType.MINUS, "-"), T(TokenType.LEFT_PAREN, "("),
                    T(TokenType.NUMBER, "1", 1), T(TokenType.PLUS, "+"), T(TokenType.NUMBER, "2", 2),
                    T(TokenType.RIGHT_PAREN, ")"),
                    T(TokenType.SEMICOLON, ";"), T(TokenType.EOF, "")
                },
                new Stmt[]
                {
                    new Var(
                        T(TokenType.IDENTIFIER, "x"),
                        new Unary(
                            T(TokenType.MINUS, "-"),
                            new Grouping(new Binary(new Literal(1), T(TokenType.PLUS, "+"), new Literal(2)))))
                }
            };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.IDENTIFIER, "x"), T(TokenType.EQUAL, "="),
                    T(TokenType.NUMBER, "10", 10), T(TokenType.SEMICOLON, ";"), T(TokenType.EOF, "")
                },
                new Stmt[]
                {
                    new StmtExpression(new Assign(T(TokenType.IDENTIFIER, "x"), new Literal(10)))
                }
            };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.VAR, "zmienna"), T(TokenType.IDENTIFIER, "x"), T(TokenType.EQUAL, "="),
                    T(TokenType.IDENTIFIER, "a"), T(TokenType.OR, "lub"), T(TokenType.IDENTIFIER, "b"),
                    T(TokenType.QUESTION_MARK, "?"), T(TokenType.NUMBER, "1", 1),
                    T(TokenType.COLON, ":"), T(TokenType.NUMBER, "2", 2),
                    T(TokenType.SEMICOLON, ";"), T(TokenType.EOF, "")
                },
                new Stmt[]
                {
                    new Var(
                        T(TokenType.IDENTIFIER, "x"),
                        new Conditional(
                            new Logical(
                                new Variable(T(TokenType.IDENTIFIER, "a")),
                                T(TokenType.OR, "lub"),
                                new Variable(T(TokenType.IDENTIFIER, "b"))),
                            new Literal(1),
                            new Literal(2)))
                }
            };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.IF, "jeżeli"), T(TokenType.LEFT_PAREN, "("), T(TokenType.IDENTIFIER, "x"),
                    T(TokenType.RIGHT_PAREN, ")"),
                    T(TokenType.IDENTIFIER, "y"), T(TokenType.EQUAL, "="), T(TokenType.NUMBER, "1", 1),
                    T(TokenType.SEMICOLON, ";"),
                    T(TokenType.ELSE, "inaczej"),
                    T(TokenType.IDENTIFIER, "y"), T(TokenType.EQUAL, "="), T(TokenType.NUMBER, "2", 2),
                    T(TokenType.SEMICOLON, ";"), T(TokenType.EOF, "")
                },
                new Stmt[]
                {
                    new If(
                        new Variable(T(TokenType.IDENTIFIER, "x")),
                        new StmtExpression(new Assign(T(TokenType.IDENTIFIER, "y"), new Literal(1))),
                        new StmtExpression(new Assign(T(TokenType.IDENTIFIER, "y"), new Literal(2))))
                }
            };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.WHILE, "podczas_gdy"), T(TokenType.LEFT_PAREN, "("),
                    T(TokenType.IDENTIFIER, "x"), T(TokenType.LESS, "<"), T(TokenType.NUMBER, "10", 10),
                    T(TokenType.RIGHT_PAREN, ")"), T(TokenType.LEFT_BRACE, "{"),
                    T(TokenType.IDENTIFIER, "x"), T(TokenType.EQUAL, "="),
                    T(TokenType.IDENTIFIER, "x"), T(TokenType.PLUS, "+"), T(TokenType.NUMBER, "1", 1),
                    T(TokenType.SEMICOLON, ";"), T(TokenType.RIGHT_BRACE, "}"), T(TokenType.EOF, "")
                },
                new Stmt[]
                {
                    new While(
                        new Binary(new Variable(T(TokenType.IDENTIFIER, "x")), T(TokenType.LESS, "<"), new Literal(10)),
                        new Block(new List<Stmt>
                        {
                            new StmtExpression(new Assign(
                                T(TokenType.IDENTIFIER, "x"),
                                new Binary(new Variable(T(TokenType.IDENTIFIER, "x")), T(TokenType.PLUS, "+"), new Literal(1))))
                        }))
                }
            };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.FOR, "dla"), T(TokenType.LEFT_PAREN, "("),
                    T(TokenType.VAR, "zmienna"), T(TokenType.IDENTIFIER, "i"), T(TokenType.EQUAL, "="),
                    T(TokenType.NUMBER, "0", 0), T(TokenType.SEMICOLON, ";"),
                    T(TokenType.IDENTIFIER, "i"), T(TokenType.LESS, "<"), T(TokenType.NUMBER, "3", 3),
                    T(TokenType.SEMICOLON, ";"),
                    T(TokenType.IDENTIFIER, "i"), T(TokenType.EQUAL, "="),
                    T(TokenType.IDENTIFIER, "i"), T(TokenType.PLUS, "+"), T(TokenType.NUMBER, "1", 1),
                    T(TokenType.RIGHT_PAREN, ")"),
                    T(TokenType.BREAK, "przerwij"), T(TokenType.SEMICOLON, ";"),
                    T(TokenType.EOF, "")
                },
                new Stmt[]
                {
                    new Block(new List<Stmt>
                    {
                        new Var(T(TokenType.IDENTIFIER, "i"), new Literal(0)),
                        new While(
                            new Binary(new Variable(T(TokenType.IDENTIFIER, "i")), T(TokenType.LESS, "<"), new Literal(3)),
                            new Block(new List<Stmt>
                            {
                                new Break(T(TokenType.BREAK, "przerwij")),
                                new StmtExpression(new Assign(
                                    T(TokenType.IDENTIFIER, "i"),
                                    new Binary(new Variable(T(TokenType.IDENTIFIER, "i")), T(TokenType.PLUS, "+"), new Literal(1))))
                            }))
                    })
                }
            };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.FUN, "funkcja"), T(TokenType.IDENTIFIER, "dodaj"), T(TokenType.LEFT_PAREN, "("),
                    T(TokenType.IDENTIFIER, "a"), T(TokenType.COMMA, ","), T(TokenType.IDENTIFIER, "b"),
                    T(TokenType.RIGHT_PAREN, ")"), T(TokenType.LEFT_BRACE, "{"),
                    T(TokenType.RETURN, "zwróć"),
                    T(TokenType.IDENTIFIER, "a"), T(TokenType.PLUS, "+"), T(TokenType.IDENTIFIER, "b"),
                    T(TokenType.SEMICOLON, ";"), T(TokenType.RIGHT_BRACE, "}"), T(TokenType.EOF, "")
                },
                new Stmt[]
                {
                    new Function(
                        T(TokenType.IDENTIFIER, "dodaj"),
                        new List<Token> { T(TokenType.IDENTIFIER, "a"), T(TokenType.IDENTIFIER, "b") },
                        new List<Stmt>
                        {
                            new Return(
                                T(TokenType.RETURN, "zwróć"),
                                new Binary(
                                    new Variable(T(TokenType.IDENTIFIER, "a")),
                                    T(TokenType.PLUS, "+"),
                                    new Variable(T(TokenType.IDENTIFIER, "b"))))
                        })
                }
            };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.IDENTIFIER, "dodaj"), T(TokenType.LEFT_PAREN, "("),
                    T(TokenType.NUMBER, "1", 1), T(TokenType.COMMA, ","),
                    T(TokenType.STRING, "\"dwa\"", "dwa"),
                    T(TokenType.RIGHT_PAREN, ")"), T(TokenType.SEMICOLON, ";"), T(TokenType.EOF, "")
                },
                new Stmt[]
                {
                    new StmtExpression(new Call(
                        new Variable(T(TokenType.IDENTIFIER, "dodaj")),
                        T(TokenType.RIGHT_PAREN, ")"),
                        new List<Expr> { new Literal(1), new Literal("dwa") }))
                }
            };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.VAR, "zmienna"), T(TokenType.IDENTIFIER, "x"), T(TokenType.EQUAL, "="),
                    T(TokenType.BANG, "!"), T(TokenType.TRUE, "prawda"), T(TokenType.SEMICOLON, ";"),
                    T(TokenType.VAR, "zmienna"), T(TokenType.IDENTIFIER, "y"), T(TokenType.EQUAL, "="),
                    T(TokenType.NULL, "null"), T(TokenType.SEMICOLON, ";"),
                    T(TokenType.EOF, "")
                },
                new Stmt[]
                {
                    new Var(T(TokenType.IDENTIFIER, "x"), new Unary(T(TokenType.BANG, "!"), new Literal(true))),
                    new Var(T(TokenType.IDENTIFIER, "y"), new Literal(null))
                }
            };
        }

        [Theory]
        [MemberData(nameof(GetParserTestData))]
        public void Parse_ShouldReturnCorrectStmtTree(Token[] tokens, Stmt[] expected)
        {
            var parser = new Parser(tokens.ToList(), false);
            var statements = parser.Parse();
            statements.Count.Should().Be(expected.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                statements[i].Should().BeEquivalentTo(expected[i], o => o.PreferringRuntimeMemberTypes());
            }
        }
    }
}
