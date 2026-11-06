using FluentAssertions;
using PolSkrypt.Errors;
using PolSkrypt.Lexing;

namespace PolSkrypt.Tests
{
    // Claude: generating new tests for Scanner error handling, especially for unexpected characters and unterminated strings.
    /// <summary>
    /// Tests verifying error messages. They must be in the "ErrorHandler" collection,
    /// because ErrorHandler holds static state.
    /// </summary>
    [Collection("ErrorHandler")]
    public class ScannerErrorTests : IDisposable
    {
        private readonly TextWriter _originalError;
        private readonly StringWriter _errorOutput;

        public ScannerErrorTests()
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

        [Theory]
        [InlineData("@")]
        [InlineData("#")]
        [InlineData("$")]
        [InlineData("^")]
        [InlineData("~")]
        [InlineData("|")]
        public void ScanTokens_ShouldReportUnexpectedCharacter(string character)
        {
            var tokens = new Scanner(character).ScanTokens();

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("[Linia 1]");
            _errorOutput.ToString().Should().Contain($"Nieoczekiwany znak: '{character}'");
            tokens.Should().HaveCount(1);
            tokens[0].Type.Should().Be(TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldReportCorrectLineForUnexpectedCharacter()
        {
            new Scanner("5\n\n@").ScanTokens();

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("[Linia 3]");
        }

        [Fact]
        public void ScanTokens_ShouldContinueScanningAfterUnexpectedCharacter()
        {
            var tokens = new Scanner("5 @ 6").ScanTokens();

            ErrorHandler.HadError.Should().BeTrue();
            tokens.Select(t => t.Type).Should().Equal(TokenType.NUMBER, TokenType.NUMBER, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldReportEachUnexpectedCharacterSeparately()
        {
            new Scanner("@ #").ScanTokens();

            var output = _errorOutput.ToString();
            output.Should().Contain("Nieoczekiwany znak: '@'");
            output.Should().Contain("Nieoczekiwany znak: '#'");
        }

        [Fact]
        public void ScanTokens_ShouldReportLineOfEndOfFileForUnterminatedMultilineString()
        {
            new Scanner("\"a\nb\nc").ScanTokens();

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("[Linia 3]");
            _errorOutput.ToString().Should().Contain("Nieoczekiwany koniec pliku w stringu.");
        }

        [Fact]
        public void ScanTokens_ShouldKeepTokensScannedBeforeUnterminatedString()
        {
            var tokens = new Scanner("zmienna x = \"abc").ScanTokens();

            ErrorHandler.HadError.Should().BeTrue();
            tokens.Select(t => t.Type).Should().Equal(
                TokenType.VAR, TokenType.IDENTIFIER, TokenType.EQUAL, TokenType.EOF);
        }

        [Theory]
        [InlineData("zmienna x = 5;")]
        [InlineData("\"tekst\"")]
        [InlineData("// komentarz")]
        [InlineData("a >= b oraz c != d")]
        [InlineData("")]
        public void ScanTokens_ShouldNotReportErrorForValidSource(string source)
        {
            new Scanner(source).ScanTokens();

            ErrorHandler.HadError.Should().BeFalse();
            _errorOutput.ToString().Should().BeEmpty();
        }

        [Fact]
        public void ScanTokens_ShouldNotSetRuntimeErrorFlag()
        {
            new Scanner("@").ScanTokens();

            ErrorHandler.HadRuntimeError.Should().BeFalse();
        }
    }
}
