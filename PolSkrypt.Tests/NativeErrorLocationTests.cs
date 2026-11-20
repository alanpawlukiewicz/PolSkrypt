using FluentAssertions;
using PolSkrypt.Errors.Exceptions;
using static PolSkrypt.Tests.SourceTestHelper;

namespace PolSkrypt.Tests
{
    // Claude: generating tests
    /// <summary>
    /// Errors thrown by native functions are created without a token (the function does not know
    /// where it was called from). The interpreter attaches the call site, so the error reports a line.
    /// Errors that already have a token must keep it.
    /// </summary>
    [Collection("Console")]
    public class NativeErrorLocationTests
    {
        private static RuntimeException ErrorOf(string source)
        {
            Action act = () => ExecuteSource(source);
            return act.Should().Throw<RuntimeException>().Which;
        }

        [Theory]
        [InlineData("zmienna a = 1;\n\nzmienna b = liczba(\"abc\");", 3, "Argument musi być liczbą.")]
        [InlineData("zmienna a = 1;\njest_instancją(a, 5);", 2, "Drugi argument 'jest_instancją' musi być klasą.")]
        [InlineData("\n\n\nPlik.otwórz(\"x.txt\", \"zły\");", 4, "Nieprawidłowy tryb pliku.")]
        [InlineData("Plik.otwórz(\"\", \"r\");", 1, "Nazwa pliku nie może być pusta.")]
        public void NativeFunctionError_ReportsLineOfTheCall(string source, int line, string message)
        {
            var error = ErrorOf(source);

            error.Message.Should().Contain(message);
            error.Token.Should().NotBeNull();
            error.Token!.Line.Should().Be(line);
        }

        [Fact]
        public void NativeFunctionError_InsideUserFunction_ReportsLineOfTheNativeCall_NotOfTheOuterCall()
        {
            var error = ErrorOf(@"funkcja parsuj(t)
{
    zwróć liczba(t);
}

parsuj(""abc"");");

            error.Token!.Line.Should().Be(3);
        }

        [Fact]
        public void NativeFunctionError_ArgumentSpanningLines_ReportsLineOfClosingParenthesis()
        {
            var error = ErrorOf("zmienna x = liczba(\n    \"abc\"\n);");

            error.Token!.Line.Should().Be(3);
            error.Token.Lexeme.Should().Be(")");
        }

        [Fact]
        public void ErrorThatAlreadyHasToken_KeepsIt()
        {
            // List methods report errors on the method name token.
            var error = ErrorOf("zmienna l = [1];\nl\n.usuń(5);");

            error.Token!.Lexeme.Should().Be("usuń");
            error.Token.Line.Should().Be(3);
        }

        [Fact]
        public void ErrorInUserFunctionBody_KeepsTokenOfTheFailingExpression()
        {
            var error = ErrorOf(@"funkcja dziel(a, b)
{
    zwróć a / b;
}
dziel(1, 0);");

            error.Token!.Lexeme.Should().Be("/");
            error.Token.Line.Should().Be(3);
        }

        [Fact]
        public void BreakInsideLoopInsideFunction_IsNotTurnedIntoError()
        {
            Eval(@"
funkcja pierwszaWieksza(lista, prog)
{
    zmienna wynik = wartość_pusta;
    dla (zmienna i = 0; i < lista.długość(); i += 1)
    {
        jeżeli (lista[i] > prog) { wynik = lista[i]; przerwij; }
    }
    zwróć wynik;
}
zmienna wynik = pierwszaWieksza([1, 5, 9], 3);").Should().Be(5.0);
        }
    }
}
