using FluentAssertions;
using PolSkrypt.Errors;
using PolSkrypt.Lexing;
using System.Text.RegularExpressions;

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

        // ---------------------------------------------------------------
        // Non-ASCII digits (the scanner explicitly accepts only '0'-'9' as digits)
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("٣")]   // Arabic-Indic digit three
        [InlineData("５")]   // fullwidth digit five
        [InlineData("३")]   // Devanagari digit three
        public void ScanTokens_ShouldReportNonAsciiDigitAsUnexpectedCharacterWithoutThrowing(string digit)
        {
            List<Token>? tokens = null;
            Action act = () => tokens = new Scanner(digit).ScanTokens();

            act.Should().NotThrow();
            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain($"Nieoczekiwany znak: '{digit}'");
            tokens!.Select(t => t.Type).Should().Equal(TokenType.EOF);
        }

        [Theory]
        [InlineData("5٣", new[] { TokenType.NUMBER, TokenType.EOF })]
        [InlineData("٣5", new[] { TokenType.NUMBER, TokenType.EOF })]
        [InlineData("1٣2", new[] { TokenType.NUMBER, TokenType.NUMBER, TokenType.EOF })]
        [InlineData("1.٣", new[] { TokenType.NUMBER, TokenType.DOT, TokenType.EOF })]
        public void ScanTokens_ShouldStopNumberAtNonAsciiDigitAndReportError(string source, TokenType[] expected)
        {
            var tokens = new Scanner(source).ScanTokens();

            ErrorHandler.HadError.Should().BeTrue();
            tokens.Select(t => t.Type).Should().Equal(expected);
        }

        [Fact]
        public void ScanTokens_ShouldKeepAsciiPartOfNumberFollowedByNonAsciiDigit()
        {
            var tokens = new Scanner("12٣").ScanTokens();

            tokens[0].Type.Should().Be(TokenType.NUMBER);
            tokens[0].Literal.Should().Be(12.0);
            tokens[0].Lexeme.Should().Be("12");
        }

        // ---------------------------------------------------------------
        // Other unexpected characters (typical copy-paste artifacts)
        // ---------------------------------------------------------------

        [Fact]
        public void ScanTokens_ShouldReportTypographicQuotesAsUnexpectedCharacters()
        {
            var tokens = new Scanner("“tekst”").ScanTokens();

            var output = _errorOutput.ToString();
            output.Should().Contain("Nieoczekiwany znak: '“'");
            output.Should().Contain("Nieoczekiwany znak: '”'");
            Regex.Matches(output, "Nieoczekiwany znak").Count.Should().Be(2);
            tokens.Select(t => t.Type).Should().Equal(TokenType.IDENTIFIER, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldReportEnDashAsUnexpectedCharacter()
        {
            var tokens = new Scanner("5 – 3").ScanTokens();

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("Nieoczekiwany znak: '–'");
            tokens.Select(t => t.Type).Should().Equal(TokenType.NUMBER, TokenType.NUMBER, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldSplitIdentifierOnUnexpectedCharacter()
        {
            var tokens = new Scanner("ab@cd").ScanTokens();

            ErrorHandler.HadError.Should().BeTrue();
            tokens.Select(t => t.Type).Should().Equal(
                TokenType.IDENTIFIER, TokenType.IDENTIFIER, TokenType.EOF);
            tokens[0].Lexeme.Should().Be("ab");
            tokens[1].Lexeme.Should().Be("cd");
        }

        [Fact]
        public void ScanTokens_ShouldReportEachErrorWithItsOwnLine()
        {
            new Scanner("@\n#").ScanTokens();

            var output = _errorOutput.ToString();
            output.Should().Contain("[Linia 1]");
            output.Should().Contain("[Linia 2]");
        }

        [Fact]
        public void ScanTokens_ShouldReportCorrectLineForUnexpectedCharacterAfterBlockComment()
        {
            new Scanner("/* a\nb */ @").ScanTokens();

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("[Linia 2]");
        }

        [Fact]
        public void ScanTokens_ShouldReportUnexpectedCharacterAfterMultilineString()
        {
            new Scanner("\"a\nb\" @").ScanTokens();

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("[Linia 2]");
        }

        [Theory]
        [InlineData("// @ # $\n5")]
        [InlineData("/* @ # $ */ 5")]
        [InlineData("\"@ # $ ^ ~ |\"")]
        [InlineData("\"٣ ５ ३\"")]
        [InlineData("// “cytat” – myślnik")]
        public void ScanTokens_ShouldNotReportInvalidCharactersInsideCommentsAndStrings(string source)
        {
            new Scanner(source).ScanTokens();

            ErrorHandler.HadError.Should().BeFalse();
            _errorOutput.ToString().Should().BeEmpty();
        }

        // ---------------------------------------------------------------
        // Unterminated strings - additional cases
        // ---------------------------------------------------------------

        [Fact]
        public void ScanTokens_ShouldNotProduceStringTokenForUnterminatedString()
        {
            var tokens = new Scanner("\"abc").ScanTokens();

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("[Linia 1]");
            Regex.Matches(_errorOutput.ToString(), "Nieoczekiwany koniec pliku w stringu\\.").Count.Should().Be(1);
            tokens.Select(t => t.Type).Should().Equal(TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldSwallowRestOfSourceInUnterminatedString()
        {
            var tokens = new Scanner("\"abc zmienna x = 5;").ScanTokens();

            ErrorHandler.HadError.Should().BeTrue();
            tokens.Select(t => t.Type).Should().Equal(TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldKeepTerminatedStringBeforeUnterminatedOne()
        {
            var tokens = new Scanner("\"a\" \"b").ScanTokens();

            ErrorHandler.HadError.Should().BeTrue();
            tokens.Select(t => t.Type).Should().Equal(TokenType.STRING, TokenType.EOF);
            tokens[0].Literal.Should().Be("a");
        }

        [Fact]
        public void ScanTokens_ShouldNotReportUnterminatedStringErrorForQuoteInsideComment()
        {
            new Scanner("// \"nie zamknięty\n5").ScanTokens();

            ErrorHandler.HadError.Should().BeFalse();
            _errorOutput.ToString().Should().BeEmpty();
        }

        // ---------------------------------------------------------------
        // Unterminated block comment
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("/*")]
        [InlineData("5 /* nie zamknięty")]
        [InlineData("/* a\nb\nc")]
        public void ScanTokens_ShouldNotReportErrorForUnterminatedBlockComment(string source)
        {
            // Documents current behavior: an unterminated block comment is silently ignored.
            // If an error is added for it later, this test should be updated.
            new Scanner(source).ScanTokens();

            ErrorHandler.HadError.Should().BeFalse();
            _errorOutput.ToString().Should().BeEmpty();
        }

        // ---------------------------------------------------------------
        // Valid sources using new syntax
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("[]")]
        [InlineData("a[0] = [1, 2, 3];")]
        [InlineData("zm i = 0; pgdy (i < 3) { i += 1; }")]
        [InlineData("jeżeli (a) { } wpw { }")]
        [InlineData("klasa Pies dziedziczy_po Zwierze { }")]
        [InlineData("x **= 2")]
        public void ScanTokens_ShouldNotReportErrorForValidSourceWithBracketsAliasesAndInheritance(string source)
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