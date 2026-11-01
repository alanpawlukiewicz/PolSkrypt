using FluentAssertions;
using PolSkrypt.Lexing;

namespace PolSkrypt.Tests
{
    public class ScannerTests
    {
        [Theory]
        [InlineData("zmienna x = 5;", new[] { TokenType.VAR, TokenType.IDENTIFIER, TokenType.EQUAL, TokenType.NUMBER, TokenType.SEMICOLON, TokenType.EOF })]
        [InlineData("funkcja dodaj(a, b) { zwróć a + b; }", 
            new[] { TokenType.FUN, TokenType.IDENTIFIER, TokenType.LEFT_PAREN, TokenType.IDENTIFIER, TokenType.COMMA, TokenType.IDENTIFIER, 
                TokenType.RIGHT_PAREN, TokenType.LEFT_BRACE, TokenType.RETURN, TokenType.IDENTIFIER, TokenType.PLUS, TokenType.IDENTIFIER, 
                TokenType.SEMICOLON, TokenType.RIGHT_BRACE, TokenType.EOF })]
        [InlineData("jeżeli (x > 10) { wypisz(\"Większe niż 10\"); }", 
            new[] { TokenType.IF, TokenType.LEFT_PAREN, TokenType.IDENTIFIER, TokenType.GREATER, TokenType.NUMBER, TokenType.RIGHT_PAREN, 
                TokenType.LEFT_BRACE, TokenType.IDENTIFIER, TokenType.LEFT_PAREN, TokenType.STRING, TokenType.RIGHT_PAREN, TokenType.SEMICOLON, 
                TokenType.RIGHT_BRACE, TokenType.EOF })]
        [InlineData("podczas_gdy (i < 10) { i = i + 1; }", 
            new[] { TokenType.WHILE, TokenType.LEFT_PAREN, TokenType.IDENTIFIER, TokenType.LESS, TokenType.NUMBER, TokenType.RIGHT_PAREN, 
                TokenType.LEFT_BRACE, TokenType.IDENTIFIER, TokenType.EQUAL, TokenType.IDENTIFIER, TokenType.PLUS, TokenType.NUMBER, 
                TokenType.SEMICOLON, TokenType.RIGHT_BRACE, TokenType.EOF })]
        [InlineData("dla (zmienna i = 0; i < 10; i = i + 1) { wypisz(i); }",
            new[] { TokenType.FOR, TokenType.LEFT_PAREN, TokenType.VAR, TokenType.IDENTIFIER, TokenType.EQUAL, TokenType.NUMBER, TokenType.SEMICOLON,
                TokenType.IDENTIFIER, TokenType.LESS, TokenType.NUMBER, TokenType.SEMICOLON, TokenType.IDENTIFIER, TokenType.EQUAL,
                TokenType.IDENTIFIER, TokenType.PLUS, TokenType.NUMBER, TokenType.RIGHT_PAREN, TokenType.LEFT_BRACE,
                TokenType.IDENTIFIER, TokenType.LEFT_PAREN, TokenType.IDENTIFIER, TokenType.RIGHT_PAREN, TokenType.SEMICOLON,
                TokenType.RIGHT_BRACE, TokenType.EOF })]
        public void ScanTokens_ShouldReturnCorrectTokens(string source, TokenType[] expectedTokenTypes)
        {
            // Arrange
            var scanner = new Scanner(source);
            // Act
            var tokens = scanner.ScanTokens();
            // Assert
            expectedTokenTypes.Length.Should().Be(tokens.Count);
            for (int i = 0; i < expectedTokenTypes.Length; i++)
            {
                tokens[i].Type.Should().Be(expectedTokenTypes[i]);
            }
        }

        [Fact]
        public void ScanTokens_ShouldHandleEmptySource()
        {
            // Arrange
            var scanner = new Scanner(string.Empty);
            // Act
            var tokens = scanner.ScanTokens();
            // Assert
            tokens.Count.Should().Be(1);
            tokens[0].Type.Should().Be(TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldHandleOnlyWhitespace()
        {
            // Arrange
            var scanner = new Scanner("   \n\t  ");
            // Act
            var tokens = scanner.ScanTokens();
            // Assert
            tokens.Count.Should().Be(1);
            tokens[0].Type.Should().Be(TokenType.EOF);
        }

        [Theory]
        [InlineData("5", 5.0)]
        [InlineData("3.14", 3.14)]
        public void ScanTokens_ShouldReturnCorrectTokenNumericLiterals(string literal, double expectedValue)
        {
            // Arrange
            var scanner = new Scanner(literal);
            // Act
            var tokens = scanner.ScanTokens();
            // Assert
            tokens.Count.Should().Be(2); // One token for the number and one for EOF
            tokens[0].Type.Should().Be(TokenType.NUMBER);
            tokens[0].Literal.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("\"hello\"", "hello")]
        [InlineData("\"world\"", "world")]
        public void ScanTokens_ShouldReturnCorrectTokenStringLiterals(string literal, string expectedValue)
        {
            // Arrange
            var scanner = new Scanner(literal);
            // Act
            var tokens = scanner.ScanTokens();
            // Assert
            tokens.Count.Should().Be(2); // One token for the string and one for EOF
            tokens[0].Type.Should().Be(TokenType.STRING);
            tokens[0].Literal.Should().Be(expectedValue);
        }
    }
}
