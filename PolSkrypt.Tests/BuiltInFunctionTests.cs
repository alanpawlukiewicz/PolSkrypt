using FluentAssertions;
using PolSkrypt.Constants;
using static PolSkrypt.Tests.SourceTestHelper;

namespace PolSkrypt.Tests
{
    // Claude: generating tests
    /// <summary>
    /// Tests for built-in functions (liczba, napis, typ, wypisz, wprowadź), string indexing
    /// and textual representation of runtime values. Uses the "Console" collection, because
    /// it redirects Console.Out / Console.In.
    /// </summary>
    [Collection("Console")]
    public class BuiltInFunctionTests
    {
        // ---------- liczba ----------

        [Theory]
        [InlineData("liczba(5)", 5.0)]
        [InlineData("liczba(\"42\")", 42.0)]
        [InlineData("liczba(\"-7\")", -7.0)]
        [InlineData("liczba(\"  12 \")", 12.0)]
        public void Liczba_ConvertsNumbersAndNumericStrings(string expression, double expected)
        {
            Eval($"zmienna wynik = {expression};").Should().Be(expected);
        }

        [Theory]
        [InlineData("liczba(\"abc\")")]
        [InlineData("liczba(\"\")")]
        [InlineData("liczba(prawda)")]
        [InlineData("liczba(wartość_pusta)")]
        [InlineData("liczba([1])")]
        public void Liczba_NonNumericArgument_ThrowsRuntimeError(string expression)
        {
            AssertRuntimeError($"zmienna wynik = {expression};", "Argument musi być liczbą.");
        }

        [Theory]
        [InlineData("pl-PL")]
        [InlineData("en-US")]
        [InlineData("")]
        public void Liczba_OfNapis_RoundTripsFractionalNumber_InAnyCulture(string culture)
        {
            WithCulture(culture, () => Eval("zmienna wynik = liczba(napis(3.25));")).Should().Be(3.25);
        }

        [Fact(Skip = "Znany błąd: liczba() używa double.TryParse z kulturą systemu, więc na polskim Windowsie liczba(\"3.5\") zgłasza błąd, choć w kodzie źródłowym liczby zapisuje się z kropką.")]
        public void Liczba_ParsesDotAsDecimalSeparator_RegardlessOfCulture()
        {
            WithCulture("pl-PL", () => Eval("zmienna wynik = liczba(\"3.5\");")).Should().Be(3.5);
        }

        // ---------- napis ----------

        [Theory]
        [InlineData("napis(12)", "12")]
        [InlineData("napis(-3)", "-3")]
        [InlineData("napis(\"tekst\")", "tekst")]
        [InlineData("napis(prawda)", Keywords.TRUE)]
        [InlineData("napis(fałsz)", Keywords.FALSE)]
        [InlineData("napis(wartość_pusta)", Keywords.NULL)]
        [InlineData("napis([1, \"a\", [prawda]])", "[1, a, [PRAWDA]]")]
        [InlineData("napis([])", "[]")]
        public void Napis_ConvertsValueToItsTextualForm(string expression, string expected)
        {
            Eval($"zmienna wynik = {expression};").Should().Be(expected);
        }

        [Fact(Skip = "Znany błąd: StringHelper.Stringify używa double.ToString() z kulturą systemu, więc na polskim Windowsie napis(3.5) zwraca \"3,5\", a ten sam program wypisuje co innego na różnych komputerach.")]
        public void Napis_UsesDotAsDecimalSeparator_RegardlessOfCulture()
        {
            WithCulture("pl-PL", () => Eval("zmienna wynik = napis(3.5);")).Should().Be("3.5");
        }

        [Fact]
        public void Napis_OfFunctionClassAndInstance_UsesDescriptiveForms()
        {
            var interpreter = ExecuteSource(@"
funkcja f() {}
klasa K {}
zmienna nf = napis(f);
zmienna nk = napis(K);
zmienna ni = napis(K());
zmienna nn = napis(zegar);");

            Global(interpreter, "nf").Should().Be("<funkcja f>");
            Global(interpreter, "nk").Should().Be("<klasa K>");
            Global(interpreter, "ni").Should().Be("<instancja klasy K>");
            Global(interpreter, "nn").Should().Be(FunctionConstants.NATIVE_FN_STRING);
        }

        // ---------- typ ----------

        [Theory]
        [InlineData("1", Keywords.NUMBER)]
        [InlineData("\"a\"", Keywords.STRING)]
        [InlineData("\"\"", Keywords.STRING)]
        [InlineData("prawda", Keywords.BOOLEAN)]
        [InlineData("fałsz", Keywords.BOOLEAN)]
        [InlineData("wartość_pusta", Keywords.NULL)]
        [InlineData("[]", Keywords.LIST)]
        [InlineData("\"abc\"[0]", Keywords.STRING)]
        public void Typ_ReturnsNameOfPrimitiveType(string expression, string expected)
        {
            Eval($"zmienna wynik = typ({expression});").Should().Be(expected);
        }

        [Fact]
        public void Typ_CanBeComparedInConditions()
        {
            Eval(@"
funkcja opisz(x) { zwróć typ(x) == ""LICZBA"" ? ""liczba"" : ""inne""; }
zmienna wynik = opisz(1) & opisz(""1"");").Should().Be("liczbainne");
        }

        [Fact]
        public void Typ_WrongArgumentCount_ThrowsRuntimeError()
        {
            AssertRuntimeError("typ();");
            AssertRuntimeError("typ(1, 2);");
        }

        // ---------- wypisz ----------

        [Fact]
        public void Wypisz_MultipleArguments_AreSeparatedBySpaces()
        {
            var output = CaptureOutput(() => ExecuteSource("wypisz(1, \"a\", prawda, wartość_pusta);"));

            output.Should().StartWith($"1 a {Keywords.TRUE} {Keywords.NULL}");
        }

        [Fact]
        public void Wypisz_TranslatesEscapeSequences()
        {
            // In PolSkrypt source the string contains a literal backslash followed by a letter.
            var output = CaptureOutput(() => ExecuteSource(@"wypisz(""a\nb\tc"");"));

            output.Should().Be("a\nb\tc");
        }

        [Fact]
        public void Wypisz_DoesNotAddNewLine()
        {
            var output = CaptureOutput(() => ExecuteSource("wypisz(\"a\"); wypisz(\"b\");"));

            output.Should().Be("ab");
        }

        [Fact]
        public void Wypisz_ReturnsNull()
        {
            CaptureOutput(() =>
                Eval("zmienna wynik = wypisz(\"x\");").Should().BeNull());
        }

        // ---------- wprowadź ----------

        [Fact]
        public void Wprowadz_ReadsLinesFromStandardInput()
        {
            var interpreter = WithInput("Ala\n42\n", () => ExecuteSource(@"
zmienna imie = wprowadź();
zmienna wiek = liczba(wprowadź());"));

            Global(interpreter, "imie").Should().Be("Ala");
            Global(interpreter, "wiek").Should().Be(42.0);
        }

        [Fact]
        public void Wprowadz_AtEndOfInput_ReturnsNull()
        {
            WithInput("", () => Eval("zmienna wynik = wprowadź();")).Should().BeNull();
        }

        private static T WithInput<T>(string input, Func<T> action)
        {
            var original = Console.In;
            Console.SetIn(new StringReader(input));
            try
            {
                return action();
            }
            finally
            {
                Console.SetIn(original);
            }
        }

        // ---------- string indexing ----------

        [Theory]
        [InlineData("\"abc\"[0]", "a")]
        [InlineData("\"abc\"[2]", "c")]
        [InlineData("\"żółw\"[1]", "ó")]
        public void StringIndex_ReturnsOneCharacterString(string expression, string expected)
        {
            Eval($"zmienna wynik = {expression};").Should().Be(expected);
        }

        [Fact]
        public void StringIndex_WorksOnVariablesAndExpressions()
        {
            var interpreter = ExecuteSource(@"
zmienna s = ""kot"";
zmienna i = 1;
zmienna a = s[i];
zmienna b = (s & ""ek"")[3];
funkcja daj() { zwróć ""pies""; }
zmienna c = daj()[0];
zmienna d = [""xy""][0][1];");

            Global(interpreter, "a").Should().Be("o");
            Global(interpreter, "b").Should().Be("e");
            Global(interpreter, "c").Should().Be("p");
            Global(interpreter, "d").Should().Be("y");
        }

        [Fact]
        public void StringIndex_CanBeUsedToBuildReversedString()
        {
            Eval(@"
zmienna s = ""abc"";
zmienna wynik = """";
dla (zmienna i = 2; i >= 0; i -= 1) { wynik &= s[i]; }").Should().Be("cba");
        }

        [Theory]
        [InlineData("\"abc\"[3]", "Indeks znajduje się poza zakresem ciągu znaków.")]
        [InlineData("\"\"[0]", "Indeks znajduje się poza zakresem ciągu znaków.")]
        [InlineData("\"abc\"[-1]", "Indeks musi być dodatnią liczbą całkowitą.")]
        [InlineData("\"abc\"[1.5]", "Indeks musi być dodatnią liczbą całkowitą.")]
        [InlineData("\"abc\"[\"0\"]", "Podano niepoprawny indeks.")]
        [InlineData("5[0]", "Zmienna musi być listą lub ciągiem znaków")]
        [InlineData("wartość_pusta[0]", "Zmienna musi być listą lub ciągiem znaków")]
        public void StringIndex_InvalidAccess_ThrowsRuntimeError(string expression, string message)
        {
            AssertRuntimeError($"zmienna wynik = {expression};", message);
        }

        [Fact]
        public void StringIndex_Assignment_IsNotAllowed_StringsAreImmutable()
        {
            AssertRuntimeError("zmienna s = \"abc\"; s[0] = \"x\";", "Zmienna musi być listą");
            AssertRuntimeError("zmienna s = \"abc\"; s[0] &= \"x\";");
        }
    }
}
