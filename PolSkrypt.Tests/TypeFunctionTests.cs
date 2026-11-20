using FluentAssertions;
using PolSkrypt.Constants;
using PolSkrypt.Interpretation.Callables;
using static PolSkrypt.Tests.SourceTestHelper;

namespace PolSkrypt.Tests
{
    // Claude: generating tests
    /// <summary>
    /// Tests for type introspection built-ins: typ (fixed set of categories),
    /// jest_instancją (class check with inheritance) and klasa_obiektu (class object of an instance).
    /// </summary>
    [Collection("Console")]
    public class TypeFunctionTests
    {
        private const string Hierarchy = @"
klasa Zwierze
{
    konstruktor(imie) { ja.imie = imie; }
    metoda przedstaw() { zwróć ""Jestem "" & ja.imie; }
    statyczna metoda gatunek() { zwróć ""zwierzę""; }
}
klasa Pies dziedziczy_po Zwierze
{
    metoda szczekaj() { zwróć ""hau""; }
}
klasa Jamnik dziedziczy_po Pies {}
klasa Kot dziedziczy_po Zwierze {}
klasa Kamien {}
";

        // =====================================================================
        // typ
        // =====================================================================

        [Theory]
        [InlineData("funkcja f() {}\nzmienna wynik = typ(f);")]
        [InlineData("zmienna wynik = typ(funkcja(a) { zwróć a; });")]
        [InlineData("zmienna wynik = typ(zegar);")]
        [InlineData("zmienna wynik = typ(typ);")]
        [InlineData("zmienna wynik = typ([1].dodaj);")]
        [InlineData("klasa K { metoda m() {} }\nzmienna wynik = typ(K().m);")]
        [InlineData("klasa K { statyczna metoda s() {} }\nzmienna wynik = typ(K.s);")]
        public void Typ_EveryCallableValue_IsFunkcja(string source)
        {
            Eval(source).Should().Be(Keywords.FUNCTION);
        }

        [Theory]
        [InlineData("klasa K {}\nzmienna wynik = typ(K);")]
        [InlineData("klasa A {}\nklasa B dziedziczy_po A {}\nzmienna wynik = typ(B);")]
        [InlineData("zmienna wynik = typ(Plik);")]
        public void Typ_UserAndNativeClasses_AreKlasa(string source)
        {
            Eval(source).Should().Be(Keywords.CLASS);
        }

        [Fact]
        public void Typ_Instance_IsInstancja_RegardlessOfClass()
        {
            var interpreter = ExecuteSource(Hierarchy + @"
zmienna a = typ(Pies(""Burek""));
zmienna b = typ(Kamien());");

            Global(interpreter, "a").Should().Be(Keywords.INSTANCE);
            Global(interpreter, "b").Should().Be(Keywords.INSTANCE);
        }

        [Fact]
        public void Typ_DoesNotDependOnNamesOrToString()
        {
            var interpreter = ExecuteSource(@"
funkcja f() {}
funkcja g(a, b) { zwróć a; }
klasa K {}
klasa L {}
zmienna funkcje = typ(f) == typ(g);
zmienna klasy = typ(K) == typ(L);
zmienna instancje = typ(K()) == typ(L());");

            Global(interpreter, "funkcje").Should().Be(true);
            Global(interpreter, "klasy").Should().Be(true);
            Global(interpreter, "instancje").Should().Be(true);
        }

        // =====================================================================
        // jest_instancją
        // =====================================================================

        [Theory]
        [InlineData("Pies(\"a\")", "Pies", true)]
        [InlineData("Pies(\"a\")", "Zwierze", true)]
        [InlineData("Jamnik(\"a\")", "Zwierze", true)]
        [InlineData("Jamnik(\"a\")", "Pies", true)]
        [InlineData("Zwierze(\"a\")", "Pies", false)]
        [InlineData("Kot(\"a\")", "Pies", false)]
        [InlineData("Pies(\"a\")", "Kot", false)]
        [InlineData("Kamien()", "Zwierze", false)]
        public void JestInstancja_FollowsInheritanceChain(string value, string klass, bool expected)
        {
            Eval(Hierarchy + $"zmienna wynik = jest_instancją({value}, {klass});").Should().Be(expected);
        }

        [Theory]
        [InlineData("5")]
        [InlineData("\"Pies\"")]
        [InlineData("wartość_pusta")]
        [InlineData("[]")]
        [InlineData("Pies")]
        [InlineData("zegar")]
        public void JestInstancja_NonInstanceValue_ReturnsFalse(string value)
        {
            Eval(Hierarchy + $"zmienna wynik = jest_instancją({value}, Pies);").Should().Be(false);
        }

        [Theory]
        [InlineData("5")]
        [InlineData("\"Pies\"")]
        [InlineData("wartość_pusta")]
        [InlineData("Pies(\"a\")")]
        public void JestInstancja_SecondArgumentNotAClass_ThrowsRuntimeError(string notAClass)
        {
            AssertRuntimeError(Hierarchy + $"jest_instancją(Pies(\"a\"), {notAClass});",
                "Drugi argument 'jest_instancją' musi być klasą.");
        }

        [Fact]
        public void JestInstancja_ComparesClassesByIdentity_NotByName()
        {
            // Each call of the function declares a new, distinct class named Punkt.
            Eval(@"
funkcja nowaKlasa() { klasa Punkt {} zwróć Punkt; }
zmienna A = nowaKlasa();
zmienna B = nowaKlasa();
zmienna wynik = jest_instancją(A(), B);").Should().Be(false);
        }

        [Fact]
        public void JestInstancja_WrongArgumentCount_ThrowsRuntimeError()
        {
            AssertRuntimeError(Hierarchy + "jest_instancją(Pies(\"a\"));");
            AssertRuntimeError(Hierarchy + "jest_instancją(Pies(\"a\"), Pies, Pies);");
        }

        // =====================================================================
        // klasa_obiektu
        // =====================================================================

        [Fact]
        public void KlasaObiektu_ReturnsTheClassObjectOfInstance()
        {
            var interpreter = ExecuteSource(Hierarchy + @"
zmienna burek = Pies(""Burek"");
zmienna k = klasa_obiektu(burek);
zmienna to_pies = k == Pies;");

            Global(interpreter, "k").Should().BeOfType<ClassCallable>()
                .Which.Name.Should().Be("Pies");
            Global(interpreter, "k").Should().BeSameAs(Global(interpreter, "Pies"));
            Global(interpreter, "to_pies").Should().Be(true);
        }

        [Theory]
        [InlineData("Jamnik(\"a\")", "Jamnik", true)]
        [InlineData("Jamnik(\"a\")", "Pies", false)]
        [InlineData("Jamnik(\"a\")", "Zwierze", false)]
        [InlineData("Zwierze(\"a\")", "Zwierze", true)]
        public void KlasaObiektu_ReturnsExactClass_WithoutWalkingInheritance(string value, string klass, bool expected)
        {
            Eval(Hierarchy + $"zmienna wynik = klasa_obiektu({value}) == {klass};").Should().Be(expected);
        }

        [Fact]
        public void KlasaObiektu_ResultCanCreateNewInstanceOfSameClass()
        {
            var interpreter = ExecuteSource(Hierarchy + @"
zmienna burek = Pies(""Burek"");
zmienna kopia = klasa_obiektu(burek)(""Azor"");
zmienna opis = kopia.przedstaw();
zmienna dzwiek = kopia.szczekaj();
zmienna ta_sama_klasa = klasa_obiektu(kopia) == klasa_obiektu(burek);");

            Global(interpreter, "opis").Should().Be("Jestem Azor");
            Global(interpreter, "dzwiek").Should().Be("hau");
            Global(interpreter, "ta_sama_klasa").Should().Be(true);
        }

        [Fact]
        public void KlasaObiektu_ResultGivesAccessToStaticMethods_IncludingInherited()
        {
            Eval(Hierarchy + "zmienna wynik = klasa_obiektu(Jamnik(\"a\")).gatunek();").Should().Be("zwierzę");
        }

        [Fact]
        public void KlasaObiektu_ResultIsAClass_ForTypAndJestInstancja()
        {
            var interpreter = ExecuteSource(Hierarchy + @"
zmienna burek = Jamnik(""Burek"");
zmienna t = typ(klasa_obiektu(burek));
zmienna sam_siebie = jest_instancją(burek, klasa_obiektu(burek));
zmienna rodzenstwo = jest_instancją(Kot(""Mruczek""), klasa_obiektu(burek));");

            Global(interpreter, "t").Should().Be(Keywords.CLASS);
            Global(interpreter, "sam_siebie").Should().Be(true);
            Global(interpreter, "rodzenstwo").Should().Be(false);
        }

        [Fact]
        public void KlasaObiektu_ResultIsPrintedAsClass()
        {
            Eval(Hierarchy + "zmienna wynik = napis(klasa_obiektu(Kot(\"a\")));").Should().Be("<klasa Kot>");
        }

        [Fact]
        public void KlasaObiektu_InstanceCreatedInsideMethodWithSelf_ReturnsItsClass()
        {
            Eval(@"
klasa Licznik
{
    konstruktor(n) { ja.n = n; }
    metoda nastepny() { zwróć klasa_obiektu(ja)(ja.n + 1); }
}
zmienna wynik = Licznik(1).nastepny().nastepny().n;").Should().Be(3.0);
        }

        [Fact]
        public void KlasaObiektu_DistinguishesClassesWithTheSameName()
        {
            Eval(@"
funkcja nowaKlasa() { klasa Punkt {} zwróć Punkt; }
zmienna A = nowaKlasa();
zmienna B = nowaKlasa();
zmienna wynik = klasa_obiektu(A()) == klasa_obiektu(B());").Should().Be(false);
        }

        [Theory]
        [InlineData("5")]
        [InlineData("\"tekst\"")]
        [InlineData("prawda")]
        [InlineData("wartość_pusta")]
        [InlineData("[1, 2]")]
        [InlineData("zegar")]
        [InlineData("Pies")]
        [InlineData("Plik")]
        [InlineData("funkcja() {}")]
        public void KlasaObiektu_NonInstanceValue_ReturnsNull(string value)
        {
            Eval(Hierarchy + $"zmienna wynik = klasa_obiektu({value});").Should().BeNull();
        }

        [Fact]
        public void KlasaObiektu_WrongArgumentCount_ThrowsRuntimeError()
        {
            AssertRuntimeError("klasa_obiektu();");
            AssertRuntimeError("klasa K {}\nklasa_obiektu(K(), K());");
        }

        // =====================================================================
        // Native class Plik and open files
        // =====================================================================

        private static T WithTempFile<T>(Func<string, T> action)
        {
            var path = Path.Combine(Path.GetTempPath(), "polskrypt_type_" + Guid.NewGuid().ToString("N") + ".txt");
            File.WriteAllText(path, "abc");
            try
            {
                // Forward slashes, so the path can be embedded in a PolSkrypt string literal.
                return action(path.Replace('\\', '/'));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void OpenFile_IsInstanceOfPlik_AndKlasaObiektuReturnsPlik()
        {
            var interpreter = WithTempFile(path => ExecuteSource(Hierarchy + $@"
zmienna f = Plik.otwórz(""{path}"", ""r"");
zmienna jest_plikiem = jest_instancją(f, Plik);
zmienna jest_psem = jest_instancją(f, Pies);
zmienna klasa_to_plik = klasa_obiektu(f) == Plik;
zmienna istnieje = klasa_obiektu(f).istnieje(""{path}"");
zmienna t = typ(f);
f.zamknij();"));

            Global(interpreter, "jest_plikiem").Should().Be(true);
            Global(interpreter, "jest_psem").Should().Be(false);
            Global(interpreter, "klasa_to_plik").Should().Be(true);
            Global(interpreter, "istnieje").Should().Be(true);
            Global(interpreter, "t").Should().Be(Keywords.FILE);
            Global(interpreter, "Plik").Should().BeSameAs(interpreter.FileClass);
        }

        [Theory]
        [InlineData("Pies(\"a\")")]
        [InlineData("5")]
        [InlineData("\"plik.txt\"")]
        [InlineData("Plik")]
        public void JestInstancja_NonFileValue_IsNotInstanceOfPlik(string value)
        {
            Eval(Hierarchy + $"zmienna wynik = jest_instancją({value}, Plik);").Should().Be(false);
        }

        // =====================================================================
        // Textual representation of callables
        // =====================================================================

        [Theory]
        [InlineData("typ", "<funkcja natywna>")]
        [InlineData("jest_instancją", "<funkcja natywna>")]
        [InlineData("klasa_obiektu", "<funkcja natywna>")]
        [InlineData("funkcja(a) { zwróć a; }", "<funkcja anonimowa>")]
        [InlineData("[1].dodaj", "<funkcja listy dodaj>")]
        [InlineData("[].długość", "<funkcja listy długość>")]
        [InlineData("Plik", "<klasa natywna Plik>")]
        public void Napis_OfCallable_UsesReadableForm(string value, string expected)
        {
            Eval($"zmienna wynik = napis({value});").Should().Be(expected);
        }

        [Fact]
        public void EveryBuiltIn_HasReadableTextualForm_NotDotNetTypeName()
        {
            var interpreter = new PolSkrypt.Interpretation.Interpreter();

            foreach (var name in interpreter.Builtins.OwnNames)
            {
                interpreter.Builtins.TryGetOwn(name, out var value);
                var text = PolSkrypt.Helpers.StringHelper.Stringify(value);

                text.Should().StartWith("<", "funkcja wbudowana '{0}' powinna mieć czytelny opis, a ma '{1}'", name, text);
                text.Should().NotContain("PolSkrypt.", "funkcja wbudowana '{0}' wypisuje nazwę klasy C#", name);
            }
        }

        [Fact]
        public void KlasaObiektu_IsANativeFunction()
        {
            var interpreter = ExecuteSource(@"
zmienna t = typ(klasa_obiektu);
zmienna n = napis(klasa_obiektu);");

            Global(interpreter, "t").Should().Be(Keywords.FUNCTION);
            Global(interpreter, "n").Should().Be(FunctionConstants.NATIVE_FN_STRING);
        }
    }
}
