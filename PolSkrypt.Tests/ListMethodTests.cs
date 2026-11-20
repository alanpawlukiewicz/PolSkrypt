using FluentAssertions;
using static PolSkrypt.Tests.SourceTestHelper;

namespace PolSkrypt.Tests
{
    // Claude: generating tests
    /// <summary>
    /// Tests for list methods (ListCallable): dodaj, usuń, długość, zawiera, wstaw, pusta,
    /// kopiuj, szukaj, policz, wyczyść. Uses the "Console" collection like InterpreterTests,
    /// because some tests print and the console is shared global state.
    /// </summary>
    [Collection("Console")]
    public class ListMethodTests
    {
        // ---------- dodaj ----------

        [Fact]
        public void Dodaj_AppendsAllArgumentsInOrder()
        {
            AsList(Eval("zmienna wynik = [1]; wynik.dodaj(2, 3); wynik.dodaj(4);"))
                .Should().Equal(1.0, 2.0, 3.0, 4.0);
        }

        [Fact]
        public void Dodaj_WithoutArguments_LeavesListUnchanged()
        {
            AsList(Eval("zmienna wynik = [1]; wynik.dodaj();")).Should().Equal(1.0);
        }

        [Fact]
        public void Dodaj_ReturnsTheSameList_AllowingChaining()
        {
            var interpreter = ExecuteSource(@"
zmienna l = [];
zmienna zwrocona = l.dodaj(1).dodaj(2);
zmienna ta_sama = zwrocona == l;");

            Global(interpreter, "ta_sama").Should().Be(true);
            AsList(Global(interpreter, "l")).Should().Equal(1.0, 2.0);
        }

        [Fact]
        public void Dodaj_AcceptsValuesOfAnyType()
        {
            AsList(Eval("zmienna wynik = []; wynik.dodaj(\"a\", prawda, wartość_pusta, [1]);"))
                .Should().HaveCount(4).And.Contain("a").And.Contain(true).And.Contain((object?)null);
        }

        // ---------- usuń ----------

        [Fact]
        public void Usun_RemovesElementAtIndex()
        {
            AsList(Eval("zmienna wynik = [10, 20, 30]; wynik.usuń(1);")).Should().Equal(10.0, 30.0);
        }

        [Theory]
        [InlineData("[1].usuń(1);", "Indeks znajduje się poza zakresem listy.")]
        [InlineData("[].usuń(0);", "Indeks znajduje się poza zakresem listy.")]
        [InlineData("[1].usuń(-1);", "Indeks musi być dodatnią liczbą całkowitą.")]
        [InlineData("[1, 2].usuń(0.5);", "Indeks musi być dodatnią liczbą całkowitą.")]
        [InlineData("[1].usuń(\"0\");", "Podano niepoprawny indeks.")]
        public void Usun_InvalidIndex_ThrowsRuntimeError(string source, string message)
        {
            AssertRuntimeError(source, message);
        }

        // ---------- długość / pusta ----------

        [Theory]
        [InlineData("[]", 0.0)]
        [InlineData("[1]", 1.0)]
        [InlineData("[1, [2, 3], \"a\"]", 3.0)]
        public void Dlugosc_ReturnsNumberOfTopLevelElements(string list, double expected)
        {
            Eval($"zmienna wynik = {list}.długość();").Should().Be(expected);
        }

        [Fact]
        public void Dlugosc_ReflectsChangesMadeAfterwards()
        {
            Eval("zmienna l = [1]; l.dodaj(2); l.usuń(0); l.dodaj(3, 4); zmienna wynik = l.długość();")
                .Should().Be(3.0);
        }

        [Theory]
        [InlineData("[]", true)]
        [InlineData("[wartość_pusta]", false)]
        [InlineData("[1, 2]", false)]
        public void Pusta_ReturnsWhetherListHasNoElements(string list, bool expected)
        {
            Eval($"zmienna wynik = {list}.pusta();").Should().Be(expected);
        }

        // ---------- zawiera ----------

        [Theory]
        [InlineData("[1, 2, 3].zawiera(2)", true)]
        [InlineData("[1, 2, 3].zawiera(4)", false)]
        [InlineData("[\"a\", \"b\"].zawiera(\"b\")", true)]
        [InlineData("[1].zawiera(\"1\")", false)]
        [InlineData("[1, wartość_pusta].zawiera(wartość_pusta)", true)]
        [InlineData("[prawda].zawiera(fałsz)", false)]
        [InlineData("[].zawiera(1)", false)]
        public void Zawiera_ComparesByValueAndType(string expression, bool expected)
        {
            Eval($"zmienna wynik = {expression};").Should().Be(expected);
        }

        [Fact]
        public void Zawiera_NestedList_ComparesByReference()
        {
            var interpreter = ExecuteSource(@"
zmienna wewn = [1];
zmienna l = [wewn];
zmienna ta_sama = l.zawiera(wewn);
zmienna inna_rowna = l.zawiera([1]);");

            Global(interpreter, "ta_sama").Should().Be(true);
            Global(interpreter, "inna_rowna").Should().Be(false);
        }

        // ---------- wstaw ----------

        [Theory]
        [InlineData(0, new[] { 9.0, 1.0, 2.0 })]
        [InlineData(1, new[] { 1.0, 9.0, 2.0 })]
        [InlineData(2, new[] { 1.0, 2.0, 9.0 })]
        public void Wstaw_InsertsBeforeIndex_AndAllowsInsertingAtEnd(int index, double[] expected)
        {
            AsList(Eval($"zmienna wynik = [1, 2]; wynik.wstaw({index}, 9);"))
                .Should().Equal(expected.Cast<object?>());
        }

        [Theory]
        [InlineData("[1].wstaw(2, 0);", "Indeks znajduje się poza zakresem listy.")]
        [InlineData("[1].wstaw(-1, 0);", "Indeks musi być dodatnią liczbą całkowitą.")]
        [InlineData("[1].wstaw(\"a\", 0);", "Podano niepoprawny indeks.")]
        public void Wstaw_InvalidIndex_ThrowsRuntimeError(string source, string message)
        {
            AssertRuntimeError(source, message);
        }

        // ---------- kopiuj ----------

        [Fact]
        public void Kopiuj_ReturnsNewList_IndependentOfOriginal()
        {
            var interpreter = ExecuteSource(@"
zmienna l = [1, 2];
zmienna k = l.kopiuj();
k.dodaj(3);
k[0] = 9;
zmienna rozne = k == l;");

            AsList(Global(interpreter, "l")).Should().Equal(1.0, 2.0);
            AsList(Global(interpreter, "k")).Should().Equal(9.0, 2.0, 3.0);
            Global(interpreter, "rozne").Should().Be(false);
        }

        [Fact]
        public void Kopiuj_IsShallow_NestedListsAreShared()
        {
            var interpreter = ExecuteSource(@"
zmienna l = [[1]];
zmienna k = l.kopiuj();
k[0][0] = 9;
zmienna wynik = l[0][0];");

            Global(interpreter, "wynik").Should().Be(9.0);
        }

        // ---------- szukaj ----------

        [Theory]
        [InlineData("[5, 6, 7].szukaj(6)", 1.0)]
        [InlineData("[5, 6, 6].szukaj(6)", 1.0)]
        [InlineData("[\"a\", \"b\"].szukaj(\"b\")", 1.0)]
        public void Szukaj_ReturnsIndexOfFirstMatch(string expression, double expected)
        {
            Eval($"zmienna wynik = {expression};").Should().Be(expected);
        }

        [Theory]
        [InlineData("[1, 2].szukaj(5)")]
        [InlineData("[].szukaj(1)")]
        [InlineData("[1].szukaj(\"1\")")]
        public void Szukaj_NotFound_ReturnsNull(string expression)
        {
            Eval($"zmienna wynik = {expression};").Should().BeNull();
        }

        [Theory]
        [InlineData("[wartość_pusta, 1].szukaj(1)", 1.0)]
        [InlineData("[1, wartość_pusta].szukaj(wartość_pusta)", 1.0)]
        [InlineData("[wartość_pusta, wartość_pusta].szukaj(wartość_pusta)", 0.0)]
        public void Szukaj_ListContainingNull_FindsElements(string expression, double expected)
        {
            Eval($"zmienna wynik = {expression};").Should().Be(expected);
        }

        [Theory]
        [InlineData("[wartość_pusta].szukaj(1)")]
        [InlineData("[1, 2].szukaj(wartość_pusta)")]
        public void Szukaj_ListContainingNull_NotFound_ReturnsNull(string expression)
        {
            Eval($"zmienna wynik = {expression};").Should().BeNull();
        }

        // ---------- policz ----------

        [Theory]
        [InlineData("[1, 2, 1, 1].policz(1)", 3.0)]
        [InlineData("[1, 2].policz(3)", 0.0)]
        [InlineData("[].policz(1)", 0.0)]
        [InlineData("[\"a\", \"a\"].policz(\"a\")", 2.0)]
        public void Policz_ReturnsNumberOfMatches(string expression, double expected)
        {
            Eval($"zmienna wynik = {expression};").Should().Be(expected);
        }

        [Theory]
        [InlineData("[1, wartość_pusta, 1].policz(1)", 2.0)]
        [InlineData("[wartość_pusta, 1, wartość_pusta].policz(wartość_pusta)", 2.0)]
        [InlineData("[1, 2].policz(wartość_pusta)", 0.0)]
        public void Policz_ListContainingNull_CountsCorrectly(string expression, double expected)
        {
            Eval($"zmienna wynik = {expression};").Should().Be(expected);
        }

        [Fact]
        public void SzukajAndPolicz_UseSameEqualityAsEqualsOperator()
        {
            var interpreter = ExecuteSource(@"
zmienna l = [1, ""1"", prawda, wartość_pusta];
zmienna s = [l.szukaj(1), l.szukaj(""1""), l.szukaj(prawda), l.szukaj(wartość_pusta)];
zmienna p = [l.policz(1), l.policz(""1""), l.policz(prawda), l.policz(wartość_pusta)];");

            AsList(Global(interpreter, "s")).Should().Equal(0.0, 1.0, 2.0, 3.0);
            AsList(Global(interpreter, "p")).Should().Equal(1.0, 1.0, 1.0, 1.0);
        }

        // ---------- wyczyść ----------

        [Fact]
        public void Wyczysc_RemovesAllElements_AndIsVisibleThroughAliases()
        {
            var interpreter = ExecuteSource(@"
zmienna l = [1, 2, 3];
zmienna alias = l;
l.wyczyść();
zmienna wynik = alias.długość();");

            Global(interpreter, "wynik").Should().Be(0.0);
        }

        // ---------- arity and unknown methods ----------

        [Theory]
        [InlineData("[1].usuń();")]
        [InlineData("[1].usuń(0, 1);")]
        [InlineData("[1].długość(1);")]
        [InlineData("[1].zawiera();")]
        [InlineData("[1].wstaw(0);")]
        [InlineData("[1].pusta(1);")]
        [InlineData("[1].kopiuj(1);")]
        [InlineData("[1].policz();")]
        [InlineData("[1].wyczyść(1);")]
        public void ListMethod_WrongArgumentCount_ThrowsRuntimeError(string source)
        {
            AssertRuntimeError(source);
        }

        [Fact]
        public void ListMethod_Unknown_ThrowsRuntimeErrorWithMessage()
        {
            AssertRuntimeError("zmienna l = []; l.sortuj();", "Podano niepoprawną funkcję listy.");
        }

        [Fact]
        public void ListMethod_TakenAsValue_StaysBoundToItsList()
        {
            var interpreter = ExecuteSource(@"
zmienna l = [];
zmienna dodajDoL = l.dodaj;
dodajDoL(1);
dodajDoL(2);");

            AsList(Global(interpreter, "l")).Should().Equal(1.0, 2.0);
        }

        [Fact]
        public void ListMethod_OnListPassedToFunction_MutatesCallersList()
        {
            AsList(Eval(@"
funkcja wypelnij(lista, n) { dla (zmienna i = 0; i < n; i += 1) { lista.dodaj(i); } }
zmienna wynik = [];
wypelnij(wynik, 3);")).Should().Equal(0.0, 1.0, 2.0);
        }
    }
}
