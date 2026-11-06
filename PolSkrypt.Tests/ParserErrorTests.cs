using FluentAssertions;
using PolSkrypt.Errors;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing;

namespace PolSkrypt.Tests
{

    [Collection("ErrorHandler")]
    public class ParserErrorTests : IDisposable
    {
        private readonly TextWriter _originalError;
        private readonly StringWriter _errorOutput;

        public ParserErrorTests()
        {
            _originalError = Console.Error;
            _errorOutput = new StringWriter();
            Console.SetError(_errorOutput);
            ErrorHandler.HadError = false;
            ErrorHandler.HadRuntimeError = false;
        }

        public void Dispose()
        {
            Console.SetError(_originalError);
            _errorOutput.Dispose();
        }

        private static Token T(TokenType type, string lexeme, object? literal = null, int line = 1)
                => new Token(type, lexeme, literal, line);

        public static IEnumerable<object[]> GetFunctionInvalidSyntaxTestData()
        {
            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.FUN, "funkcja"), T(TokenType.IDENTIFIER, "fn"), T(TokenType.LEFT_PAREN, "("),
                    T(TokenType.RIGHT_PAREN, ")"), T(TokenType.LEFT_BRACE, "{"), T(TokenType.EOF, "")
                },
                "Oczekiwano '}' po bloku."
            };
            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.FUN, "funkcja"), T(TokenType.IDENTIFIER, "fn"), T(TokenType.LEFT_PAREN, "("),
                    T(TokenType.LEFT_BRACE, "{"), T(TokenType.RIGHT_BRACE, "}"), T(TokenType.EOF, "")
                },
                "Oczekiwano nazwy parametru."
            };

            // Claude: generate test cases
            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.FUN, "funkcja"), T(TokenType.LEFT_PAREN, "("), T(TokenType.RIGHT_PAREN, ")"),
                    T(TokenType.LEFT_BRACE, "{"), T(TokenType.RIGHT_BRACE, "}"), T(TokenType.EOF, "")
                },
                "Oczekiwano nazwy dla 'funkcja'."
            };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.FUN, "funkcja"), T(TokenType.IDENTIFIER, "fn"),
                    T(TokenType.LEFT_BRACE, "{"), T(TokenType.RIGHT_BRACE, "}"), T(TokenType.EOF, "")
                },
                "Oczekiwano '(' po nazwie dla 'funkcja'."
            };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.FUN, "funkcja"), T(TokenType.IDENTIFIER, "fn"), T(TokenType.LEFT_PAREN, "("),
                    T(TokenType.IDENTIFIER, "a"), T(TokenType.COMMA, ","), T(TokenType.IDENTIFIER, "b"),
                    T(TokenType.LEFT_BRACE, "{"), T(TokenType.RIGHT_BRACE, "}"), T(TokenType.EOF, "")
                },
                "Oczekiwano ')' po parametrach."
            };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.FUN, "funkcja"), T(TokenType.IDENTIFIER, "fn"), T(TokenType.LEFT_PAREN, "("),
                    T(TokenType.RIGHT_PAREN, ")"), T(TokenType.RIGHT_BRACE, "}"), T(TokenType.EOF, "")
                },
                "Oczekiwano '{' przed ciałem 'funkcja'"
            };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.FUN, "funkcja"), T(TokenType.IDENTIFIER, "fn"), T(TokenType.LEFT_PAREN, "("),
                    T(TokenType.IDENTIFIER, "a"), T(TokenType.COMMA, ","), T(TokenType.RIGHT_PAREN, ")"),
                    T(TokenType.LEFT_BRACE, "{"), T(TokenType.RIGHT_BRACE, "}"), T(TokenType.EOF, "")
                },
                "Oczekiwano nazwy parametru."
            };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.FUN, "funkcja"), T(TokenType.IDENTIFIER, "fn"), T(TokenType.LEFT_PAREN, "("),
                    T(TokenType.NUMBER, "1", 1.0), T(TokenType.RIGHT_PAREN, ")"),
                    T(TokenType.LEFT_BRACE, "{"), T(TokenType.RIGHT_BRACE, "}"), T(TokenType.EOF, "")
                },
                "Oczekiwano nazwy parametru."
            };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.FUN, "funkcja"), T(TokenType.IDENTIFIER, "fn"), T(TokenType.LEFT_PAREN, "("),
                    T(TokenType.IDENTIFIER, "a"), T(TokenType.IDENTIFIER, "b"), T(TokenType.RIGHT_PAREN, ")"),
                    T(TokenType.LEFT_BRACE, "{"), T(TokenType.RIGHT_BRACE, "}"), T(TokenType.EOF, "")
                },
                "Oczekiwano ')' po parametrach."
            };

            yield return new object[]
            {
                new Token[]
                {
                    T(TokenType.FUN, "funkcja"), T(TokenType.NUMBER, "123", 123.0), T(TokenType.LEFT_PAREN, "("),
                    T(TokenType.RIGHT_PAREN, ")"), T(TokenType.LEFT_BRACE, "{"), T(TokenType.RIGHT_BRACE, "}"),
                    T(TokenType.EOF, "")
                },
                "Oczekiwano nazwy dla 'funkcja'."
            };
        }

        [Theory]
        [MemberData(nameof(GetFunctionInvalidSyntaxTestData))]
        public void Function_GivenInvalidSyntax_ShouldThrowError(Token[] tokens, string errorMessage)
        {
            // Arrange
            var parser = new Parser(tokens.ToList());
            // Act & Assert
            parser.Parse();
            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("[Linia 1]");
            _errorOutput.ToString().Should().Contain(errorMessage);
        }
    }
}
