using FluentAssertions;
using PolSkrypt.Constants;
using PolSkrypt.Errors;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing;
using PolSkrypt.Parsing.Expressions;
using PolSkrypt.Parsing.Statements;
using System.Text.RegularExpressions;

namespace PolSkrypt.Tests
{
    // Claude: generating new tests for Parser class
    /// <summary>
    /// Tests verifying reported errors and error recovery. They must be in the "ErrorHandler" collection,
    /// because ErrorHandler holds static state.
    /// </summary>
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

        private static List<Stmt> Parse(string source, bool promptMode = false)
            => new Parser(new Scanner(source).ScanTokens(), promptMode).Parse();

        private static Token Id(string name) => new Token(TokenType.IDENTIFIER, name, null, 1);

        private static string ParamList(int count)
            => string.Join(", ", Enumerable.Range(0, count).Select(i => $"p{i}"));

        private static string ArgList(int count)
            => string.Join(", ", Enumerable.Range(0, count));

        // ---------------------------------------------------------------
        // Error messages
        // ---------------------------------------------------------------

        [Theory]
        // Variables
        [InlineData("zmienna;", "Oczekiwano nazwy zmiennej.")]
        [InlineData("zmienna x = 1, ;", "Oczekiwano nazwy zmiennej.")]
        [InlineData("zmienna x = 1", "Oczekiwano ';' po deklaracji zmiennej.")]
        // Expressions
        [InlineData("1 + 2", "Oczekiwano ';' po wartości.")]
        [InlineData(";", "Oczekiwano wyrażenia.")]
        [InlineData("1 + ;", "Oczekiwano wyrażenia.")]
        [InlineData("(1 + 2;", "Oczekiwano ')' po wyrażeniu.")]
        [InlineData("f(1, 2;", "Oczekiwano ')' po liście argumentów.")]
        [InlineData("a.;", "Oczekiwano nazwy po '.'")]
        [InlineData("a ? b;", "Oczekiwano ':' po gałęzi.")]
        // Invalid assignment targets (reported without throwing)
        [InlineData("1 = 2;", "Nieprawidłowy cel przypisania.")]
        [InlineData("(a) = 1;", "Nieprawidłowy cel przypisania.")]
        [InlineData("1 += 2;", "Nieprawidłowy cel przypisania.")]
        [InlineData("a.b += 1;", "Nieprawidłowy cel przypisania.")]
        // Return / break / blocks
        [InlineData("zwróć 1", "Oczekiwano ';' po wartości zwróconej.")]
        [InlineData("przerwij", "Oczekiwano ';' po 'przerwij'.")]
        [InlineData("{ x;", "Oczekiwano '}' po bloku.")]
        // if / while / for
        [InlineData("jeżeli x) y;", "Oczekiwano znaku '(' po 'jeżeli'")]
        [InlineData("jeżeli (x y;", "Oczekiwano znaku ')' po warunku jeżeli")]
        [InlineData("podczas_gdy x) y;", "Oczekiwano '(' po 'podczas_gdy'.")]
        [InlineData("podczas_gdy (x y;", "Oczekiwano ')' po warunku podczas_gdy.")]
        [InlineData("dla x;", "Oczekiwano '(' po 'dla'.")]
        [InlineData("dla (; x y) z;", "Oczekiwano ';' po warunku pętli.")]
        [InlineData("dla (;; x y) z;", "Oczekiwano ')'.")]
        // Classes
        [InlineData("klasa {}", "Oczekiwano nazwy klasy.")]
        [InlineData("klasa A }", "Oczekiwano '{' po nazwie klasy.")]
        [InlineData("klasa A { metoda f() {}", "Oczekiwano '}' po ciele klasy.")]
        [InlineData("klasa A { statyczna f() {} }", "Oczekiwano 'metoda' po 'statyczna'.")]
        [InlineData("klasa A { x }", "Niepoprawna składnia klasy.")]
        [InlineData("klasa A { metoda () {} }", "Oczekiwano nazwy dla 'metoda'.")]
        [InlineData("klasa A { statyczna metoda () {} }", "Oczekiwano nazwy dla 'statyczna metoda'.")]
        [InlineData("klasa A { metoda f {} }", "Oczekiwano '(' po nazwie dla 'metoda'.")]
        // Anonymous functions
        [InlineData("zmienna f = funkcja { };", "Oczekiwano '('.")]
        [InlineData("zmienna f = funkcja(1) { };", "Oczekiwano nazwy parametru.")]
        [InlineData("zmienna f = funkcja(a ;", "Oczekiwano ')' po liście argumentów.")]
        [InlineData("zmienna f = funkcja() ;", "Oczekiwano '{'.")]
        public void Parse_ShouldReportExpectedError(string source, string expectedMessage)
        {
            Parse(source);

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain(expectedMessage);
        }

        [Fact]
        public void Parse_ShouldReportLineOfOffendingToken()
        {
            Parse("zmienna x = 1;\nzmienna = 2;");

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("[Linia 2]");
            _errorOutput.ToString().Should().Contain("Oczekiwano nazwy zmiennej.");
        }

        [Fact]
        public void Parse_ShouldReportMissingSemicolonInNonPromptMode()
        {
            Parse("1 + 2", promptMode: false);

            ErrorHandler.HadError.Should().BeTrue();
        }

        [Fact]
        public void Parse_ShouldNotReportMissingSemicolonInPromptMode()
        {
            Parse("1 + 2", promptMode: true);

            ErrorHandler.HadError.Should().BeFalse();
        }

        // ---------------------------------------------------------------
        // No errors for valid sources
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("")]
        [InlineData("zmienna x = 1;")]
        [InlineData("x = y = 3;")]
        [InlineData("a.b.c = f(1)(2);")]
        [InlineData("jeżeli (a) b; w_przeciwnym_wypadku c;")]
        [InlineData("dla (zmienna i = 0; i < 3; i += 1) { wypisz(i); }")]
        [InlineData("funkcja f(a, b) { zwróć a & b; }")]
        [InlineData("klasa A { metoda f() {} statyczna metoda g() {} }")]
        [InlineData("zmienna f = funkcja(x) { zwróć x ** 2; };")]
        public void Parse_ShouldNotReportErrorForValidSource(string source)
        {
            Parse(source);

            ErrorHandler.HadError.Should().BeFalse();
            _errorOutput.ToString().Should().BeEmpty();
        }

        // ---------------------------------------------------------------
        // Error recovery (Synchronize)
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldRecoverAtNextSemicolonAndContinueParsing()
        {
            var statements = Parse("zmienna = 5; zmienna y = 1;");

            ErrorHandler.HadError.Should().BeTrue();
            statements.Should().HaveCount(1);
            statements[0].Should().BeEquivalentTo(
                new Var(Id("y"), new Literal(1.0)), o => o.PreferringRuntimeMemberTypes());
        }

        [Fact]
        public void Parse_ShouldRecoverAtStatementKeyword()
        {
            var statements = Parse("zmienna = zmienna y;");

            ErrorHandler.HadError.Should().BeTrue();
            statements.Should().HaveCount(1);
            statements[0].Should().BeEquivalentTo(new Var(Id("y"), null), o => o.PreferringRuntimeMemberTypes());
        }

        [Fact]
        public void Parse_ShouldRecoverFromExpressionErrorAndKeepFollowingStatement()
        {
            var statements = Parse("1 + ; x = 2;");

            ErrorHandler.HadError.Should().BeTrue();
            statements.Should().HaveCount(1);
            statements[0].Should().BeOfType<StmtExpression>();
        }

        [Fact]
        public void Parse_ShouldReportEveryIndependentError()
        {
            Parse("zmienna = 1; zmienna = 2;");

            var occurrences = Regex.Matches(_errorOutput.ToString(), Regex.Escape("Oczekiwano nazwy zmiennej.")).Count;
            occurrences.Should().Be(2);
        }

        [Fact]
        public void Parse_ShouldRecoverInsideBlock()
        {
            var statements = Parse("{ zmienna = 1; x; }");

            ErrorHandler.HadError.Should().BeTrue();
            statements.Should().HaveCount(1);
            statements[0].Should().BeOfType<Block>();
            statements[0].Should().BeEquivalentTo(
                new Block(new List<Stmt> { new StmtExpression(new Variable(Id("x"))) }),
                o => o.PreferringRuntimeMemberTypes());
        }

        [Fact]
        public void Parse_ShouldRecoverFromErrorInFunctionBodyAndParseNextDeclaration()
        {
            var statements = Parse("funkcja f() { zmienna; } zmienna y;");

            ErrorHandler.HadError.Should().BeTrue();
            statements.Should().HaveCount(2);
            statements[0].Should().BeOfType<Function>();
            statements[1].Should().BeEquivalentTo(new Var(Id("y"), null), o => o.PreferringRuntimeMemberTypes());
        }

        [Fact]
        public void Parse_ShouldTerminateOnUnterminatedInput()
        {
            Action act = () => Parse("funkcja f(");

            act.Should().NotThrow();
            ErrorHandler.HadError.Should().BeTrue();
        }

        [Fact]
        public void Parse_ShouldContinueAfterInvalidAssignmentTarget()
        {
            // Invalid assignment target is reported but does not throw, so the statement is still produced.
            var statements = Parse("1 = 2; zmienna y = 1;");

            ErrorHandler.HadError.Should().BeTrue();
            statements.Should().HaveCount(2);
            statements[1].Should().BeEquivalentTo(
                new Var(Id("y"), new Literal(1.0)), o => o.PreferringRuntimeMemberTypes());
        }

        // ---------------------------------------------------------------
        // Parameter / argument limit
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldAcceptFunctionWithMaxParameters()
        {
            Parse($"funkcja f({ParamList(FunctionConstants.MAX_PARAMS_COUNT)}) {{}}");

            ErrorHandler.HadError.Should().BeFalse();
        }

        [Fact]
        public void Parse_ShouldReportFunctionDeclarationWithTooManyParameters()
        {
            var statements = Parse($"funkcja f({ParamList(FunctionConstants.MAX_PARAMS_COUNT + 1)}) {{}}");

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain(
                $"Funkcja nie może mieć więcej niż {FunctionConstants.MAX_PARAMS_COUNT} argumentów.");
            // The limit error is not fatal, the function is still parsed.
            statements.Should().ContainSingle().Which.Should().BeOfType<Function>();
        }

        [Fact]
        public void Parse_ShouldAcceptCallWithMaxArguments()
        {
            Parse($"f({ArgList(FunctionConstants.MAX_PARAMS_COUNT)});");

            ErrorHandler.HadError.Should().BeFalse();
        }

        [Fact]
        public void Parse_ShouldReportCallWithTooManyArguments()
        {
            var statements = Parse($"f({ArgList(FunctionConstants.MAX_PARAMS_COUNT + 1)});");

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain(
                $"Funkcja nie może mieć więcej niż {FunctionConstants.MAX_PARAMS_COUNT} argumentów.");
            statements.Should().ContainSingle().Which.Should().BeOfType<StmtExpression>();
        }

        [Fact]
        public void Parse_ShouldReportAnonymousFunctionWithTooManyParameters()
        {
            Parse($"zmienna f = funkcja({ParamList(FunctionConstants.MAX_PARAMS_COUNT + 1)}) {{ }};");

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain(
                $"Funkcja nie może mieć więcej niż {FunctionConstants.MAX_PARAMS_COUNT} parametrów.");
        }

        // ---------------------------------------------------------------
        // Errors at the end of the file
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldReportLastLineForErrorAtEndOfFile()
        {
            // Regression test: Scanner used to create the EOF token with Line = 0,
            // so errors at the end of the file said "[Linia 0]".
            Parse("zmienna x = 1");

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("[Linia 1]");
        }

        [Fact]
        public void Parse_ShouldReportLastLineForErrorAtEndOfMultilineFile()
        {
            Parse("zmienna x = 1;\nzmienna y = 2");

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("[Linia 2]");
        }

        // ---------------------------------------------------------------
        // Lists, indexing, inheritance and super: error messages
        // ---------------------------------------------------------------

        [Theory]
        // super
        [InlineData("super;", "Oczekiwano '.' po 'super'.")]
        [InlineData("super.;", "Oczekiwano nazwy metody po 'super'.")]
        [InlineData("super.f = 1;", "Nieprawidłowy cel przypisania.")]
        // inheritance
        [InlineData("klasa A dziedziczy_po {}", "Oczekiwano nazwy klasy nadrzędnej")]
        [InlineData("klasa A dziedziczy_po B C {}", "Oczekiwano '{' po nazwie klasy.")]
        [InlineData("klasa A dziedziczy_po B.C {}", "Oczekiwano '{' po nazwie klasy.")]
        [InlineData("klasa A dziedziczy_po B", "Oczekiwano '{' po nazwie klasy.")]
        // list literals
        [InlineData("[1, ;", "Oczekiwano wyrażenia.")]
        [InlineData("[,];", "Oczekiwano wyrażenia.")]
        [InlineData("[1,,2];", "Oczekiwano wyrażenia.")]
        // indexing
        [InlineData("a[0;", "Oczekiwano znaku ']'.")]
        [InlineData("a[;", "Oczekiwano wyrażenia.")]
        [InlineData("a[];", "Oczekiwano wyrażenia.")]
        [InlineData("a[b[1];", "Oczekiwano znaku ']'.")]
        // invalid assignment targets
        [InlineData("a[0] += 1;", "Nieprawidłowy cel przypisania.")]
        [InlineData("a[0] *= 2;", "Nieprawidłowy cel przypisania.")]
        [InlineData("[1] = 2;", "Nieprawidłowy cel przypisania.")]
        // function / method declarations
        [InlineData("funkcja () {}", "Oczekiwano nazwy dla 'funkcja'.")]
        [InlineData("funkcja f( {}", "Oczekiwano nazwy parametru.")]
        [InlineData("funkcja f(a b) {}", "Oczekiwano ')' po parametrach.")]
        [InlineData("funkcja f() ;", "Oczekiwano '{' przed ciałem 'funkcja'")]
        [InlineData("klasa A { metoda f() ; }", "Oczekiwano '{' przed ciałem 'metoda'")]
        [InlineData("klasa A { statyczna metoda f() ; }", "Oczekiwano '{' przed ciałem 'statyczna metoda'")]
        public void Parse_ShouldReportExpectedErrorForListsIndexingInheritanceAndSuper(string source, string expectedMessage)
        {
            Parse(source);

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain(expectedMessage);
        }

        [Theory]
        [InlineData("[];")]
        [InlineData("[1, 2, 3];")]
        [InlineData("[1, 2,];")]
        [InlineData("[[1], [2]];")]
        [InlineData("a[0];")]
        [InlineData("a[b[0]];")]
        [InlineData("a[0] = 1;")]
        [InlineData("a[b[0]] = c[1];")]
        [InlineData("a[0].x = 1;")]
        [InlineData("zmienna a = [1, 2]; a[0] = a[1];")]
        [InlineData("klasa B dziedziczy_po A {}")]
        [InlineData("klasa B dziedziczy_po A { metoda f() { zwróć super.f(); } }")]
        [InlineData("2 ** -1;")]
        [InlineData("2 ** !a;")]
        public void Parse_ShouldNotReportErrorForValidListsIndexingInheritanceAndSuper(string source)
        {
            Parse(source);

            ErrorHandler.HadError.Should().BeFalse();
            _errorOutput.ToString().Should().BeEmpty();
        }

        [Fact]
        public void Parse_ShouldReportUnterminatedIndexAtEndOfFile()
        {
            Parse("a[0");

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("Oczekiwano znaku ']'.");
        }

        [Fact]
        public void Parse_ShouldReportMissingSemicolonAfterIndexExpressionInNonPromptMode()
        {
            Parse("a[0]");

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("Oczekiwano ';' po wartości.");
        }

        [Fact]
        public void Parse_ShouldNotReportMissingSemicolonAfterIndexExpressionInPromptMode()
        {
            Parse("a[0]", promptMode: true);

            ErrorHandler.HadError.Should().BeFalse();
        }

        // ---------------------------------------------------------------
        // Error recovery - lists, indexing, inheritance and super
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldRecoverFromInvalidSuperAndParseNextStatement()
        {
            var statements = Parse("super; zmienna y = 1;");

            ErrorHandler.HadError.Should().BeTrue();
            statements.Should().HaveCount(1);
            statements[0].Should().BeEquivalentTo(
                new Var(Id("y"), new Literal(1.0)), o => o.PreferringRuntimeMemberTypes());
        }

        [Fact]
        public void Parse_ShouldRecoverFromErrorInClassBodyAndParseNextDeclaration()
        {
            var statements = Parse("klasa A { x } zmienna y;");

            ErrorHandler.HadError.Should().BeTrue();
            statements.Should().HaveCount(1);
            statements[0].Should().BeEquivalentTo(new Var(Id("y"), null), o => o.PreferringRuntimeMemberTypes());
        }

        [Fact]
        public void Parse_ShouldRecoverFromMissingSuperclassNameAndParseNextDeclaration()
        {
            var statements = Parse("klasa A dziedziczy_po { } zmienna y;");

            ErrorHandler.HadError.Should().BeTrue();
            statements.Should().ContainSingle().Which.Should().BeOfType<Var>();
        }

        [Fact]
        public void Parse_ShouldRecoverFromUnterminatedIndexAndParseNextStatement()
        {
            var statements = Parse("a[0; zmienna y = 1;");

            ErrorHandler.HadError.Should().BeTrue();
            statements.Should().HaveCount(1);
            statements[0].Should().BeEquivalentTo(
                new Var(Id("y"), new Literal(1.0)), o => o.PreferringRuntimeMemberTypes());
        }

        [Fact]
        public void Parse_ShouldContinueAfterInvalidCompoundAssignmentToIndex()
        {
            // Like other invalid assignment targets, this is reported but does not throw.
            var statements = Parse("a[0] += 1; zmienna y = 1;");

            ErrorHandler.HadError.Should().BeTrue();
            statements.Should().HaveCount(2);
            statements[1].Should().BeEquivalentTo(
                new Var(Id("y"), new Literal(1.0)), o => o.PreferringRuntimeMemberTypes());
        }

        [Fact]
        public void Parse_ShouldRecoverFromErrorInsideListLiteral()
        {
            var statements = Parse("zmienna a = [1, ;\nzmienna b = 2;");

            ErrorHandler.HadError.Should().BeTrue();
            statements.Should().HaveCount(1);
            statements[0].Should().BeEquivalentTo(
                new Var(new Token(TokenType.IDENTIFIER, "b", null, 2), new Literal(2.0)),
                o => o.PreferringRuntimeMemberTypes());
        }

        [Fact]
        public void Parse_ShouldTerminateOnUnterminatedListAndIndex()
        {
            Action act = () =>
            {
                Parse("[1, 2");
                Parse("a[");
                Parse("klasa A dziedziczy_po");
                Parse("super.");
            };

            act.Should().NotThrow();
            ErrorHandler.HadError.Should().BeTrue();
        }

        // ---------------------------------------------------------------
        // Known issues (these tests fail until the parser is fixed)
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldReportUnterminatedListLiteralInPromptMode_KnownIssue()
        {
            Parse("[1, 2", promptMode: true);

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("Oczekiwano znaku ']'.");
        }
    }
}