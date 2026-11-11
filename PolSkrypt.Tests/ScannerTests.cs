using FluentAssertions;
using PolSkrypt.Lexing;

namespace PolSkrypt.Tests
{
    // Claude: generating new tests for Scanner class
    /// <summary>
    /// Tests that do not touch ErrorHandler (valid sources).
    /// </summary>
    public class ScannerTests
    {
        private static List<Token> Scan(string source) => new Scanner(source).ScanTokens();

        private static TokenType[] Types(string source) => Scan(source).Select(t => t.Type).ToArray();

        // ---------------------------------------------------------------
        // Keywords
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("oraz", TokenType.AND)]
        [InlineData("klasa", TokenType.CLASS)]
        [InlineData("w_przeciwnym_wypadku", TokenType.ELSE)]
        [InlineData("fałsz", TokenType.FALSE)]
        [InlineData("funkcja", TokenType.FUN)]
        [InlineData("dla", TokenType.FOR)]
        [InlineData("jeżeli", TokenType.IF)]
        [InlineData("wartość_pusta", TokenType.NULL)]
        [InlineData("lub", TokenType.OR)]
        [InlineData("zwróć", TokenType.RETURN)]
        [InlineData("super", TokenType.SUPER)]
        [InlineData("ja", TokenType.SELF)]
        [InlineData("prawda", TokenType.TRUE)]
        [InlineData("zmienna", TokenType.VAR)]
        [InlineData("podczas_gdy", TokenType.WHILE)]
        [InlineData("przerwij", TokenType.BREAK)]
        [InlineData("metoda", TokenType.METHOD)]
        [InlineData("statyczna", TokenType.STATIC)]
        public void ScanTokens_ShouldRecognizeEveryKeyword(string keyword, TokenType expected)
        {
            var tokens = Scan(keyword);

            tokens.Should().HaveCount(2);
            tokens[0].Type.Should().Be(expected);
            tokens[0].Lexeme.Should().Be(keyword);
            tokens[0].Literal.Should().BeNull();
        }

        [Theory]
        [InlineData("zmienna1")]
        [InlineData("zmienna_")]
        [InlineData("_zmienna")]
        [InlineData("zmiennaa")]
        [InlineData("dla_każdego")]
        [InlineData("jaa")]
        [InlineData("Zmienna")]   // keywords are case-sensitive
        [InlineData("PRAWDA")]
        [InlineData("JEŻELI")]
        public void ScanTokens_ShouldTreatKeywordLookalikesAsIdentifiers(string text)
        {
            var tokens = Scan(text);

            tokens.Should().HaveCount(2);
            tokens[0].Type.Should().Be(TokenType.IDENTIFIER);
            tokens[0].Lexeme.Should().Be(text);
        }

        [Theory]
        [InlineData("x")]
        [InlineData("_")]
        [InlineData("__init__")]
        [InlineData("licznik2")]
        [InlineData("żółć")]
        [InlineData("zażółć_gęślą_jaźń")]
        [InlineData("ŁÓDŹ")]
        public void ScanTokens_ShouldScanIdentifiersWithPolishLettersAndUnderscores(string identifier)
        {
            var tokens = Scan(identifier);

            tokens.Should().HaveCount(2);
            tokens[0].Type.Should().Be(TokenType.IDENTIFIER);
            tokens[0].Lexeme.Should().Be(identifier);
            tokens[0].Literal.Should().BeNull();
        }

        [Fact]
        public void ScanTokens_ShouldSplitIdentifierOnNonIdentifierCharacter()
        {
            Types("abc.def").Should().Equal(
                TokenType.IDENTIFIER, TokenType.DOT, TokenType.IDENTIFIER, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldRecognizeSelfAndSuperInMethodCall()
        {
            Types("ja.pole = super.metoda();").Should().Equal(
                TokenType.SELF, TokenType.DOT, TokenType.IDENTIFIER, TokenType.EQUAL,
                TokenType.SUPER, TokenType.DOT, TokenType.METHOD, TokenType.LEFT_PAREN,
                TokenType.RIGHT_PAREN, TokenType.SEMICOLON, TokenType.EOF);
        }

        // ---------------------------------------------------------------
        // Operators and single-character tokens
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("(", TokenType.LEFT_PAREN)]
        [InlineData(")", TokenType.RIGHT_PAREN)]
        [InlineData("{", TokenType.LEFT_BRACE)]
        [InlineData("}", TokenType.RIGHT_BRACE)]
        [InlineData(",", TokenType.COMMA)]
        [InlineData(".", TokenType.DOT)]
        [InlineData(";", TokenType.SEMICOLON)]
        [InlineData("?", TokenType.QUESTION_MARK)]
        [InlineData(":", TokenType.COLON)]
        [InlineData("+", TokenType.PLUS)]
        [InlineData("-", TokenType.MINUS)]
        [InlineData("*", TokenType.STAR)]
        [InlineData("/", TokenType.SLASH)]
        [InlineData("%", TokenType.PERCENT)]
        [InlineData("&", TokenType.AMPERSAND)]
        [InlineData("!", TokenType.BANG)]
        [InlineData("=", TokenType.EQUAL)]
        [InlineData("<", TokenType.LESS)]
        [InlineData(">", TokenType.GREATER)]
        public void ScanTokens_ShouldScanSingleCharacterTokens(string source, TokenType expected)
        {
            var tokens = Scan(source);

            tokens.Should().HaveCount(2);
            tokens[0].Type.Should().Be(expected);
            tokens[0].Lexeme.Should().Be(source);
        }

        [Theory]
        [InlineData("!=", TokenType.BANG_EQUAL)]
        [InlineData("==", TokenType.EQUAL_EQUAL)]
        [InlineData("<=", TokenType.LESS_EQUAL)]
        [InlineData(">=", TokenType.GREATER_EQUAL)]
        [InlineData("+=", TokenType.PLUS_EQUAL)]
        [InlineData("-=", TokenType.MINUS_EQUAL)]
        [InlineData("*=", TokenType.STAR_EQUAL)]
        [InlineData("/=", TokenType.SLASH_EQUAL)]
        [InlineData("%=", TokenType.PERCENT_EQUAL)]
        [InlineData("&=", TokenType.AMPERSAND_EQUAL)]
        [InlineData("**", TokenType.STAR_STAR)]
        public void ScanTokens_ShouldScanTwoCharacterTokens(string source, TokenType expected)
        {
            var tokens = Scan(source);

            tokens.Should().HaveCount(2);
            tokens[0].Type.Should().Be(expected);
            tokens[0].Lexeme.Should().Be(source);
        }

        [Theory]
        [InlineData("===", new[] { TokenType.EQUAL_EQUAL, TokenType.EQUAL, TokenType.EOF })]
        [InlineData("!==", new[] { TokenType.BANG_EQUAL, TokenType.EQUAL, TokenType.EOF })]
        [InlineData("<==", new[] { TokenType.LESS_EQUAL, TokenType.EQUAL, TokenType.EOF })]
        [InlineData("**=", new[] { TokenType.STAR_STAR, TokenType.EQUAL, TokenType.EOF })]
        [InlineData("***", new[] { TokenType.STAR_STAR, TokenType.STAR, TokenType.EOF })]
        [InlineData("&&", new[] { TokenType.AMPERSAND, TokenType.AMPERSAND, TokenType.EOF })]
        [InlineData("+++", new[] { TokenType.PLUS, TokenType.PLUS, TokenType.PLUS, TokenType.EOF })]
        [InlineData("=!", new[] { TokenType.EQUAL, TokenType.BANG, TokenType.EOF })]
        [InlineData("x+=1", new[] { TokenType.IDENTIFIER, TokenType.PLUS_EQUAL, TokenType.NUMBER, TokenType.EOF })]
        [InlineData("a**b", new[] { TokenType.IDENTIFIER, TokenType.STAR_STAR, TokenType.IDENTIFIER, TokenType.EOF })]
        public void ScanTokens_ShouldUseLongestMatchForOperators(string source, TokenType[] expected)
        {
            Types(source).Should().Equal(expected);
        }

        [Fact]
        public void ScanTokens_ShouldNotMergeOperatorsSeparatedBySpace()
        {
            Types("= =").Should().Equal(TokenType.EQUAL, TokenType.EQUAL, TokenType.EOF);
            Types("! =").Should().Equal(TokenType.BANG, TokenType.EQUAL, TokenType.EOF);
            Types("* *").Should().Equal(TokenType.STAR, TokenType.STAR, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldScanTernaryExpression()
        {
            Types("x > 0 ? 1 : 2").Should().Equal(
                TokenType.IDENTIFIER, TokenType.GREATER, TokenType.NUMBER, TokenType.QUESTION_MARK,
                TokenType.NUMBER, TokenType.COLON, TokenType.NUMBER, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldScanLogicalKeywordsInExpression()
        {
            Types("a oraz b lub !c").Should().Equal(
                TokenType.IDENTIFIER, TokenType.AND, TokenType.IDENTIFIER, TokenType.OR,
                TokenType.BANG, TokenType.IDENTIFIER, TokenType.EOF);
        }

        // ---------------------------------------------------------------
        // Numbers
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("0", 0.0)]
        [InlineData("007", 7.0)]
        [InlineData("123456", 123456.0)]
        [InlineData("0.5", 0.5)]
        [InlineData("10.25", 10.25)]
        [InlineData("123456789.123", 123456789.123)]
        public void ScanTokens_ShouldParseNumbersAsDouble(string source, double expected)
        {
            var tokens = Scan(source);

            tokens.Should().HaveCount(2);
            tokens[0].Type.Should().Be(TokenType.NUMBER);
            tokens[0].Literal.Should().BeOfType<double>();
            tokens[0].Literal.Should().Be(expected);
            tokens[0].Lexeme.Should().Be(source);
        }

        [Fact]
        public void ScanTokens_ShouldTreatTrailingDotAsSeparateDotToken()
        {
            var tokens = Scan("3.");

            tokens.Select(t => t.Type).Should().Equal(TokenType.NUMBER, TokenType.DOT, TokenType.EOF);
            tokens[0].Literal.Should().Be(3.0);
        }

        [Fact]
        public void ScanTokens_ShouldTreatLeadingDotAsSeparateDotToken()
        {
            var tokens = Scan(".5");

            tokens.Select(t => t.Type).Should().Equal(TokenType.DOT, TokenType.NUMBER, TokenType.EOF);
            tokens[1].Literal.Should().Be(5.0);
        }

        [Fact]
        public void ScanTokens_ShouldSplitNumberWithTwoDots()
        {
            var tokens = Scan("1.2.3");

            tokens.Select(t => t.Type).Should().Equal(
                TokenType.NUMBER, TokenType.DOT, TokenType.NUMBER, TokenType.EOF);
            tokens[0].Literal.Should().Be(1.2);
            tokens[2].Literal.Should().Be(3.0);
        }

        [Fact]
        public void ScanTokens_ShouldNotTreatDotAsDecimalSeparatorBeforeIdentifier()
        {
            Types("5.abc").Should().Equal(
                TokenType.NUMBER, TokenType.DOT, TokenType.IDENTIFIER, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldSplitNumberFollowedByLetters()
        {
            Types("5abc").Should().Equal(TokenType.NUMBER, TokenType.IDENTIFIER, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldTreatMinusBeforeNumberAsSeparateToken()
        {
            var tokens = Scan("-5");

            tokens.Select(t => t.Type).Should().Equal(TokenType.MINUS, TokenType.NUMBER, TokenType.EOF);
            tokens[1].Literal.Should().Be(5.0);
        }

        // The test for non-ASCII digits as a standalone character lives in ScannerErrorTests,
        // because such a character is now reported through ErrorHandler (static state).

        // ---------------------------------------------------------------
        // Strings
        // ---------------------------------------------------------------

        [Fact]
        public void ScanTokens_ShouldScanEmptyString()
        {
            var tokens = Scan("\"\"");

            tokens.Should().HaveCount(2);
            tokens[0].Type.Should().Be(TokenType.STRING);
            tokens[0].Literal.Should().Be(string.Empty);
            tokens[0].Lexeme.Should().Be("\"\"");
        }

        [Fact]
        public void ScanTokens_ShouldKeepQuotesInLexemeButNotInLiteral()
        {
            var tokens = Scan("\"abc\"");

            tokens[0].Lexeme.Should().Be("\"abc\"");
            tokens[0].Literal.Should().Be("abc");
        }

        [Theory]
        [InlineData("\"zażółć gęślą jaźń\"", "zażółć gęślą jaźń")]
        [InlineData("\"  spacje  \"", "  spacje  ")]
        [InlineData("\"// to nie komentarz\"", "// to nie komentarz")]
        [InlineData("\"/* ani to */\"", "/* ani to */")]
        [InlineData("\"a + b == c;\"", "a + b == c;")]
        [InlineData("\"zmienna\"", "zmienna")]
        public void ScanTokens_ShouldPreserveStringContent(string source, string expected)
        {
            var tokens = Scan(source);

            tokens.Should().HaveCount(2);
            tokens[0].Type.Should().Be(TokenType.STRING);
            tokens[0].Literal.Should().Be(expected);
        }

        [Fact]
        public void ScanTokens_ShouldScanMultipleStringsSeparately()
        {
            var tokens = Scan("\"a\" \"b\"");

            tokens.Select(t => t.Type).Should().Equal(TokenType.STRING, TokenType.STRING, TokenType.EOF);
            tokens[0].Literal.Should().Be("a");
            tokens[1].Literal.Should().Be("b");
        }

        [Fact]
        public void ScanTokens_ShouldSupportMultilineStringsAndCountLines()
        {
            var tokens = Scan("\"a\nb\" 5");

            tokens.Select(t => t.Type).Should().Equal(TokenType.STRING, TokenType.NUMBER, TokenType.EOF);
            tokens[0].Literal.Should().Be("a\nb");
            tokens[1].Line.Should().Be(2);
        }

        // ---------------------------------------------------------------
        // Comments
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("// komentarz", new[] { TokenType.EOF })]
        [InlineData("//", new[] { TokenType.EOF })]
        [InlineData("5 // komentarz", new[] { TokenType.NUMBER, TokenType.EOF })]
        [InlineData("5 // komentarz\n6", new[] { TokenType.NUMBER, TokenType.NUMBER, TokenType.EOF })]
        [InlineData("// a\n// b\n7", new[] { TokenType.NUMBER, TokenType.EOF })]
        [InlineData("// zmienna x = 5;\n", new[] { TokenType.EOF })]
        public void ScanTokens_ShouldIgnoreLineComments(string source, TokenType[] expected)
        {
            Types(source).Should().Equal(expected);
        }

        [Theory]
        [InlineData("/* komentarz */ 5", new[] { TokenType.NUMBER, TokenType.EOF })]
        [InlineData("5 /* x */ 6", new[] { TokenType.NUMBER, TokenType.NUMBER, TokenType.EOF })]
        [InlineData("/**/ 5", new[] { TokenType.NUMBER, TokenType.EOF })]
        [InlineData("/* a * b */ 5", new[] { TokenType.NUMBER, TokenType.EOF })]
        [InlineData("/* a / b */ 5", new[] { TokenType.NUMBER, TokenType.EOF })]
        [InlineData("/* // */ 5", new[] { TokenType.NUMBER, TokenType.EOF })]
        [InlineData("/* a */ /* b */ 5", new[] { TokenType.NUMBER, TokenType.EOF })]
        [InlineData("1 /* a\nb\nc */ 2", new[] { TokenType.NUMBER, TokenType.NUMBER, TokenType.EOF })]
        [InlineData("/* a */", new[] { TokenType.EOF })]
        public void ScanTokens_ShouldEndBlockCommentAtClosingDelimiter(string source, TokenType[] expected)
        {
            Types(source).Should().Equal(expected);
        }

        [Fact]
        public void ScanTokens_ShouldCountLinesInsideBlockComment()
        {
            var tokens = Scan("/* a\nb\nc */ 5");

            tokens.Select(t => t.Type).Should().Equal(TokenType.NUMBER, TokenType.EOF);
            tokens[0].Line.Should().Be(3);
        }

        [Fact]
        public void ScanTokens_ShouldNotThrowOnUnterminatedBlockComment()
        {
            Action act = () => Scan("5 /* nie zamknięty");

            act.Should().NotThrow();
            Types("5 /* nie zamknięty").Should().Equal(TokenType.NUMBER, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldCountLinesAfterLineComment()
        {
            var tokens = Scan("// a\n5");

            tokens[0].Type.Should().Be(TokenType.NUMBER);
            tokens[0].Line.Should().Be(2);
        }

        [Fact]
        public void ScanTokens_ShouldTreatSingleSlashAsDivision()
        {
            Types("a / b").Should().Equal(
                TokenType.IDENTIFIER, TokenType.SLASH, TokenType.IDENTIFIER, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldScanCompoundDivisionAssignment()
        {
            Types("a /= 2").Should().Equal(
                TokenType.IDENTIFIER, TokenType.SLASH_EQUAL, TokenType.NUMBER, TokenType.EOF);
        }

        // ---------------------------------------------------------------
        // Whitespace and line numbers
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("5\r\n3\r\n4", new[] { 1, 2, 3 })]
        [InlineData("5\n\n\n3", new[] { 1, 4 })]
        [InlineData("\n\n5", new[] { 3 })]
        [InlineData("\t5\t\t6", new[] { 1, 1 })]
        [InlineData("a\n// komentarz\nb", new[] { 1, 3 })]
        [InlineData("a /* x\ny */ b", new[] { 1, 2 })]
        public void ScanTokens_ShouldTrackLineNumbers(string source, int[] expectedLines)
        {
            Scan(source)
                .Where(t => t.Type != TokenType.EOF)
                .Select(t => t.Line)
                .Should().Equal(expectedLines);
        }

        [Fact]
        public void ScanTokens_ShouldAssignLineOfClosingQuoteToMultilineString()
        {
            // Documents current behavior: AddToken uses _line after the string has been scanned.
            var tokens = Scan("\"a\nb\nc\"");

            tokens[0].Type.Should().Be(TokenType.STRING);
            tokens[0].Line.Should().Be(3);
        }

        [Theory]
        [InlineData("5 6", 2)]
        [InlineData("5      6", 2)]
        [InlineData("5\t6", 2)]
        [InlineData("5\r6", 2)]
        public void ScanTokens_ShouldSeparateTokensByWhitespace(string source, int expectedTokenCount)
        {
            Scan(source).Where(t => t.Type == TokenType.NUMBER).Should().HaveCount(expectedTokenCount);
        }

        // ---------------------------------------------------------------
        // General behavior
        // ---------------------------------------------------------------

        [Fact]
        public void ScanTokens_ShouldAlwaysEndWithSingleEofToken()
        {
            var tokens = Scan("zmienna x = 1;");

            tokens.Last().Type.Should().Be(TokenType.EOF);
            tokens.Count(t => t.Type == TokenType.EOF).Should().Be(1);
            tokens.Last().Lexeme.Should().BeEmpty();
            tokens.Last().Literal.Should().BeNull();
        }

        [Fact]
        public void ScanTokens_ShouldReturnLexemesMatchingSourceText()
        {
            var lexemes = Scan("zmienna wynik = 3.14 * (a + b);")
                .Where(t => t.Type != TokenType.EOF)
                .Select(t => t.Lexeme);

            lexemes.Should().Equal("zmienna", "wynik", "=", "3.14", "*", "(", "a", "+", "b", ")", ";");
        }

        [Fact]
        public void ScanTokens_ShouldReturnNullLiteralForNonLiteralTokens()
        {
            var tokens = Scan("zmienna x = (1 + 2);");

            tokens.Where(t => t.Type != TokenType.NUMBER && t.Type != TokenType.STRING)
                  .Should().OnlyContain(t => t.Literal == null);
        }

        [Fact]
        public void ScanTokens_ShouldScanClassDefinition()
        {
            const string source = @"
klasa Pies {
    statyczna metoda stworz() { zwróć wartość_pusta; }
    metoda szczekaj() { wypisz(""Hau""); }
}";
            Types(source).Should().Equal(
                TokenType.CLASS, TokenType.IDENTIFIER, TokenType.LEFT_BRACE,
                TokenType.STATIC, TokenType.METHOD, TokenType.IDENTIFIER, TokenType.LEFT_PAREN, TokenType.RIGHT_PAREN,
                TokenType.LEFT_BRACE, TokenType.RETURN, TokenType.NULL, TokenType.SEMICOLON, TokenType.RIGHT_BRACE,
                TokenType.METHOD, TokenType.IDENTIFIER, TokenType.LEFT_PAREN, TokenType.RIGHT_PAREN,
                TokenType.LEFT_BRACE, TokenType.IDENTIFIER, TokenType.LEFT_PAREN, TokenType.STRING,
                TokenType.RIGHT_PAREN, TokenType.SEMICOLON, TokenType.RIGHT_BRACE,
                TokenType.RIGHT_BRACE, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldScanIfElseWithBooleansAndBreak()
        {
            Types("jeżeli (prawda) { przerwij; } w_przeciwnym_wypadku { zwróć fałsz; }").Should().Equal(
                TokenType.IF, TokenType.LEFT_PAREN, TokenType.TRUE, TokenType.RIGHT_PAREN,
                TokenType.LEFT_BRACE, TokenType.BREAK, TokenType.SEMICOLON, TokenType.RIGHT_BRACE,
                TokenType.ELSE, TokenType.LEFT_BRACE, TokenType.RETURN, TokenType.FALSE,
                TokenType.SEMICOLON, TokenType.RIGHT_BRACE, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldScanCompoundAssignmentsInStatement()
        {
            Types("x += 1; y -= 2; z *= 3; w /= 4; p %= 5; q &= 6;").Should().Equal(
                TokenType.IDENTIFIER, TokenType.PLUS_EQUAL, TokenType.NUMBER, TokenType.SEMICOLON,
                TokenType.IDENTIFIER, TokenType.MINUS_EQUAL, TokenType.NUMBER, TokenType.SEMICOLON,
                TokenType.IDENTIFIER, TokenType.STAR_EQUAL, TokenType.NUMBER, TokenType.SEMICOLON,
                TokenType.IDENTIFIER, TokenType.SLASH_EQUAL, TokenType.NUMBER, TokenType.SEMICOLON,
                TokenType.IDENTIFIER, TokenType.PERCENT_EQUAL, TokenType.NUMBER, TokenType.SEMICOLON,
                TokenType.IDENTIFIER, TokenType.AMPERSAND_EQUAL, TokenType.NUMBER, TokenType.SEMICOLON,
                TokenType.EOF);
        }

        // ---------------------------------------------------------------
        // Keyword aliases and inheritance
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("wpw", TokenType.ELSE)]
        [InlineData("zm", TokenType.VAR)]
        [InlineData("pgdy", TokenType.WHILE)]
        [InlineData("dziedziczy_po", TokenType.EXTENDS)]
        public void ScanTokens_ShouldRecognizeShortKeywordAliasesAndExtends(string keyword, TokenType expected)
        {
            var tokens = Scan(keyword);

            tokens.Should().HaveCount(2);
            tokens[0].Type.Should().Be(expected);
            tokens[0].Lexeme.Should().Be(keyword);
            tokens[0].Literal.Should().BeNull();
        }

        [Theory]
        [InlineData("wpw", "w_przeciwnym_wypadku")]
        [InlineData("zm", "zmienna")]
        [InlineData("pgdy", "podczas_gdy")]
        public void ScanTokens_ShouldGiveAliasTheSameTokenTypeAsFullKeyword(string alias, string full)
        {
            Types(alias).Should().Equal(Types(full));
        }

        [Theory]
        [InlineData("wpw1")]
        [InlineData("wpwx")]
        [InlineData("zm1")]
        [InlineData("zm_")]
        [InlineData("zmm")]
        [InlineData("pgdy2")]
        [InlineData("pgd")]
        [InlineData("dziedziczy")]
        [InlineData("dziedziczy_po_")]
        [InlineData("dziedziczy_pol")]
        [InlineData("w_przeciwnym")]
        [InlineData("Wpw")]
        [InlineData("ZM")]
        [InlineData("Dziedziczy_po")]
        public void ScanTokens_ShouldTreatAliasLookalikesAsIdentifiers(string text)
        {
            var tokens = Scan(text);

            tokens.Should().HaveCount(2);
            tokens[0].Type.Should().Be(TokenType.IDENTIFIER);
            tokens[0].Lexeme.Should().Be(text);
        }

        [Fact]
        public void ScanTokens_ShouldScanClassWithInheritance()
        {
            Types("klasa Pies dziedziczy_po Zwierze { }").Should().Equal(
                TokenType.CLASS, TokenType.IDENTIFIER, TokenType.EXTENDS, TokenType.IDENTIFIER,
                TokenType.LEFT_BRACE, TokenType.RIGHT_BRACE, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldScanStatementsWrittenWithShortAliases()
        {
            Types("zm i = 0; pgdy (i < 3) { i += 1; }").Should().Equal(
                TokenType.VAR, TokenType.IDENTIFIER, TokenType.EQUAL, TokenType.NUMBER, TokenType.SEMICOLON,
                TokenType.WHILE, TokenType.LEFT_PAREN, TokenType.IDENTIFIER, TokenType.LESS, TokenType.NUMBER,
                TokenType.RIGHT_PAREN, TokenType.LEFT_BRACE, TokenType.IDENTIFIER, TokenType.PLUS_EQUAL,
                TokenType.NUMBER, TokenType.SEMICOLON, TokenType.RIGHT_BRACE, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldScanIfWithShortElseAlias()
        {
            Types("jeżeli (a) { } wpw { }").Should().Equal(
                TokenType.IF, TokenType.LEFT_PAREN, TokenType.IDENTIFIER, TokenType.RIGHT_PAREN,
                TokenType.LEFT_BRACE, TokenType.RIGHT_BRACE,
                TokenType.ELSE, TokenType.LEFT_BRACE, TokenType.RIGHT_BRACE, TokenType.EOF);
        }

        // ---------------------------------------------------------------
        // Square brackets
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("[", TokenType.LEFT_SQUARE_BRACKET)]
        [InlineData("]", TokenType.RIGHT_SQUARE_BRACKET)]
        public void ScanTokens_ShouldScanSquareBrackets(string source, TokenType expected)
        {
            var tokens = Scan(source);

            tokens.Should().HaveCount(2);
            tokens[0].Type.Should().Be(expected);
            tokens[0].Lexeme.Should().Be(source);
            tokens[0].Literal.Should().BeNull();
        }

        [Fact]
        public void ScanTokens_ShouldScanEmptyArrayLiteral()
        {
            Types("[]").Should().Equal(
                TokenType.LEFT_SQUARE_BRACKET, TokenType.RIGHT_SQUARE_BRACKET, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldScanArrayLiteralAndIndexing()
        {
            Types("a[0] = [1, 2.5];").Should().Equal(
                TokenType.IDENTIFIER, TokenType.LEFT_SQUARE_BRACKET, TokenType.NUMBER, TokenType.RIGHT_SQUARE_BRACKET,
                TokenType.EQUAL,
                TokenType.LEFT_SQUARE_BRACKET, TokenType.NUMBER, TokenType.COMMA, TokenType.NUMBER,
                TokenType.RIGHT_SQUARE_BRACKET, TokenType.SEMICOLON, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldScanNestedIndexing()
        {
            Types("a[b[1]]").Should().Equal(
                TokenType.IDENTIFIER, TokenType.LEFT_SQUARE_BRACKET,
                TokenType.IDENTIFIER, TokenType.LEFT_SQUARE_BRACKET, TokenType.NUMBER, TokenType.RIGHT_SQUARE_BRACKET,
                TokenType.RIGHT_SQUARE_BRACKET, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldScanIndexedCompoundAssignment()
        {
            Types("t[i] += 1").Should().Equal(
                TokenType.IDENTIFIER, TokenType.LEFT_SQUARE_BRACKET, TokenType.IDENTIFIER,
                TokenType.RIGHT_SQUARE_BRACKET, TokenType.PLUS_EQUAL, TokenType.NUMBER, TokenType.EOF);
        }

        [Fact]
        public void ScanTokens_ShouldNotTreatBracketsInsideStringsAsTokens()
        {
            var tokens = Scan("\"[1, 2]\"");

            tokens.Select(t => t.Type).Should().Equal(TokenType.STRING, TokenType.EOF);
            tokens[0].Literal.Should().Be("[1, 2]");
        }

        // ---------------------------------------------------------------
        // Identifiers vs. non-ASCII digits
        // ---------------------------------------------------------------

        [Fact]
        public void ScanTokens_ShouldAllowNonAsciiDigitInsideIdentifier()
        {
            // Documents current behavior: only the START of a number is restricted to '0'-'9',
            // while identifiers use char.IsLetterOrDigit, which accepts e.g. Arabic-Indic digits.
            var tokens = Scan("a٣");

            tokens.Should().HaveCount(2);
            tokens[0].Type.Should().Be(TokenType.IDENTIFIER);
            tokens[0].Lexeme.Should().Be("a٣");
        }

        // ---------------------------------------------------------------
        // Additional comment edge cases
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("/*/ 5", new[] { TokenType.EOF })]                                   // "/*/" does not close itself
        [InlineData("/***/ 5", new[] { TokenType.NUMBER, TokenType.EOF })]
        [InlineData("/* a **/ 5", new[] { TokenType.NUMBER, TokenType.EOF })]
        [InlineData("/* a */5", new[] { TokenType.NUMBER, TokenType.EOF })]
        [InlineData("/*", new[] { TokenType.EOF })]
        [InlineData("/* a *", new[] { TokenType.EOF })]
        [InlineData("/* a /", new[] { TokenType.EOF })]
        [InlineData("a //= 5", new[] { TokenType.IDENTIFIER, TokenType.EOF })]            // comment wins over "/="
        [InlineData("a /*= 5 */ b", new[] { TokenType.IDENTIFIER, TokenType.IDENTIFIER, TokenType.EOF })]
        [InlineData("a /// b", new[] { TokenType.IDENTIFIER, TokenType.EOF })]
        [InlineData("a / /* x */ b", new[] { TokenType.IDENTIFIER, TokenType.SLASH, TokenType.IDENTIFIER, TokenType.EOF })]
        [InlineData("5//komentarz\r\n6", new[] { TokenType.NUMBER, TokenType.NUMBER, TokenType.EOF })]
        [InlineData("*/", new[] { TokenType.STAR, TokenType.SLASH, TokenType.EOF })]     // closing delimiter outside a comment
        [InlineData("/* a */ */ 5", new[] { TokenType.STAR, TokenType.SLASH, TokenType.NUMBER, TokenType.EOF })]
        public void ScanTokens_ShouldHandleCommentEdgeCases(string source, TokenType[] expected)
        {
            Types(source).Should().Equal(expected);
        }

        [Theory]
        [InlineData("/*")]
        [InlineData("/* a *")]
        [InlineData("/*/")]
        [InlineData("5 /* a\nb\nc")]
        public void ScanTokens_ShouldNotThrowOnUnterminatedBlockCommentVariants(string source)
        {
            Action act = () => Scan(source);

            act.Should().NotThrow();
        }

        [Fact]
        public void ScanTokens_ShouldAssignCorrectLineToTokenAfterCommentsOfBothKinds()
        {
            var tokens = Scan("a // x\n/* y\nz */ b\n// w\nc");

            tokens.Where(t => t.Type != TokenType.EOF)
                  .Select(t => (t.Lexeme, t.Line))
                  .Should().Equal(("a", 1), ("b", 3), ("c", 5));
        }

        // ---------------------------------------------------------------
        // Line of the EOF token
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("", 1)]
        [InlineData("5", 1)]
        [InlineData("5\n", 2)]
        [InlineData("\n\n\n", 4)]
        [InlineData("5\r\n6\r\n", 3)]
        [InlineData("// a\n", 2)]
        [InlineData("/* a\nb */", 2)]
        [InlineData("a\n/* x\ny", 3)]          // unterminated block comment still counts lines
        [InlineData("\"a\nb\"", 2)]
        public void ScanTokens_ShouldAssignLastLineNumberToEofToken(string source, int expectedLine)
        {
            var tokens = Scan(source);

            tokens.Last().Type.Should().Be(TokenType.EOF);
            tokens.Last().Line.Should().Be(expectedLine);
        }

        // ---------------------------------------------------------------
        // Numbers - additional cases
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("1.5.", new[] { TokenType.NUMBER, TokenType.DOT, TokenType.EOF })]
        [InlineData("1..2", new[] { TokenType.NUMBER, TokenType.DOT, TokenType.DOT, TokenType.NUMBER, TokenType.EOF })]
        [InlineData("a[1.5]", new[] { TokenType.IDENTIFIER, TokenType.LEFT_SQUARE_BRACKET, TokenType.NUMBER,
                                      TokenType.RIGHT_SQUARE_BRACKET, TokenType.EOF })]
        [InlineData("1+2", new[] { TokenType.NUMBER, TokenType.PLUS, TokenType.NUMBER, TokenType.EOF })]
        [InlineData("1.5**2", new[] { TokenType.NUMBER, TokenType.STAR_STAR, TokenType.NUMBER, TokenType.EOF })]
        public void ScanTokens_ShouldSplitNumbersCorrectlyNextToPunctuation(string source, TokenType[] expected)
        {
            Types(source).Should().Equal(expected);
        }

        [Fact]
        public void ScanTokens_ShouldParseNumbersIndependentlyOfCurrentCulture()
        {
            var original = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("pl-PL");

                var tokens = Scan("3.14");

                tokens[0].Type.Should().Be(TokenType.NUMBER);
                tokens[0].Literal.Should().Be(3.14);
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = original;
            }
        }

        [Fact]
        public void ScanTokens_ShouldBeRepeatableAcrossSeparateScannerInstances()
        {
            const string source = "zmienna x = 1;";

            Types(source).Should().Equal(Types(source));
        }
    }
}