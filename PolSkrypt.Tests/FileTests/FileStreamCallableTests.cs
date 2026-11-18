using FluentAssertions;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Interpretation.Callables.FileCallables;
using PolSkrypt.Interpretation.NativeObjects;
using PolSkrypt.Lexing;
using Xunit;

namespace PolSkrypt.Tests.FileTests
{
    // Claude: generating class
    public class FileStreamCallableTests : TempFileTestBase
    {
        // ---------------------------------------------------------------
        // UWAGA: jedyne miejsce zależne od konstruktora Token.
        // Jeśli Twój Token ma inną sygnaturę, popraw tylko tę metodę.
        // ---------------------------------------------------------------
        private static Token MakeToken(string lexeme)
            => new Token(TokenType.IDENTIFIER, lexeme, null, 1);

        private static FileStreamCallable Create(PFile file, string name)
            => new FileStreamCallable(file, MakeToken(name));

        // Interpreter nie jest używany przez FileStreamCallable
        private static object? Call(FileStreamCallable callable, params object?[] args)
            => callable.Call(null!, args.ToList());

        private static object? Invoke(PFile file, string name, params object?[] args)
            => Call(Create(file, name), args);

        // ---------- konstruktor ----------

        [Fact]
        public void Constructor_UnknownFunctionName_Throws()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");

            var act = () => Create(f, "nieistnieje");

            act.Should().Throw<RuntimeException>();
        }

        [Theory]
        [InlineData("czytaj")]
        [InlineData("czytaj_linie")]
        [InlineData("czytaj_znaki")]
        [InlineData("wpisz")]
        [InlineData("wpisz_linie")]
        [InlineData("zamknij")]
        [InlineData("ustaw")]
        [InlineData("koniec")]
        [InlineData("przewiń")]
        [InlineData("pozycja")]
        [InlineData("rozmiar")]
        [InlineData("przytnij")]
        [InlineData("opróżnij")]
        [InlineData("zamknięty")]
        public void Constructor_KnownFunctionNames_DoNotThrow(string name)
        {
            var f = Open(CreateFile("a.txt", "abc"), "rw");

            var act = () => Create(f, name);

            act.Should().NotThrow();
        }

        // ---------- Arity ----------

        [Theory]
        [InlineData("czytaj", 0)]
        [InlineData("czytaj_linie", 0)]
        [InlineData("czytaj_znaki", 1)]
        [InlineData("wpisz", 1)]
        [InlineData("wpisz_linie", 1)]
        [InlineData("zamknij", 0)]
        [InlineData("ustaw", 1)]
        [InlineData("koniec", 0)]
        [InlineData("przewiń", 0)]
        [InlineData("pozycja", 0)]
        [InlineData("rozmiar", 0)]
        [InlineData("przytnij", 0)]
        [InlineData("opróżnij", 0)]
        [InlineData("zamknięty", 0)]
        public void Arity_MatchesFunction(string name, int expected)
        {
            var f = Open(CreateFile("a.txt", "abc"), "rw");

            Create(f, name).Arity.Should().Be(expected);
        }

        // ---------- czytanie ----------

        [Fact]
        public void Czytaj_ReturnsWholeContent()
        {
            var f = Open(CreateFile("a.txt", "a\nb\nżółw"), "r");

            Invoke(f, "czytaj").Should().Be("a\nb\nżółw");
        }

        [Fact]
        public void CzytajLinie_ReturnsLinesThenNull()
        {
            var f = Open(CreateFile("a.txt", "a\nb"), "r");
            var callable = Create(f, "czytaj_linie");

            Call(callable).Should().Be("a");
            Call(callable).Should().Be("b");
            Call(callable).Should().BeNull();
        }

        [Fact]
        public void CzytajZnaki_ReturnsRequestedCharacters()
        {
            var f = Open(CreateFile("a.txt", "żółw"), "r");

            Invoke(f, "czytaj_znaki", 3.0).Should().Be("żół");
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(1.5)]
        [InlineData(double.NaN)]
        [InlineData(3000000000.0)]
        [InlineData("3")]
        [InlineData(null)]
        [InlineData(true)]
        public void CzytajZnaki_InvalidCount_Throws(object? count)
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");

            var act = () => Invoke(f, "czytaj_znaki", count);

            act.Should().Throw<RuntimeException>();
        }

        [Fact]
        public void Koniec_ReturnsBoolean()
        {
            var f = Open(CreateFile("a.txt", "a"), "r");

            Invoke(f, "koniec").Should().Be(false);
            Invoke(f, "czytaj");
            Invoke(f, "koniec").Should().Be(true);
        }

        // ---------- zapis ----------

        [Fact]
        public void Wpisz_WritesTextAndReturnsSameFile()
        {
            var path = PathFor("w.txt");
            var f = Open(path, "w");

            var result = Invoke(f, "wpisz", "abc");

            result.Should().BeSameAs(f);
            ReadDisk(path).Should().Be("abc");
        }

        [Fact]
        public void Wpisz_StringifiesNonStringArguments()
        {
            var path = PathFor("w.txt");
            var f = Open(path, "w");

            Invoke(f, "wpisz", 42.0);

            ReadDisk(path).Should().Be("42");
        }

        [Fact]
        public void WpiszLinie_AppendsNewline_AndReturnsSameFile()
        {
            var path = PathFor("w.txt");
            var f = Open(path, "w");

            var result = Invoke(f, "wpisz_linie", "abc");

            result.Should().BeSameAs(f);
            ReadDisk(path).Should().Be("abc\n");
        }

        [Fact]
        public void Wpisz_CanBeChained()
        {
            var path = PathFor("w.txt");
            var f = Open(path, "w");

            var r1 = Invoke(f, "wpisz_linie", "a");
            var r2 = Invoke((PFile)r1!, "wpisz_linie", "b");

            r2.Should().BeSameAs(f);
            ReadDisk(path).Should().Be("a\nb\n");
        }

        [Fact]
        public void Wpisz_InReadMode_ThrowsRuntimeException()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");

            var act = () => Invoke(f, "wpisz", "x");

            act.Should().Throw<RuntimeException>();
        }

        [Fact]
        public void Czytaj_InWriteMode_ThrowsRuntimeException()
        {
            var f = Open(PathFor("w.txt"), "w");

            var act = () => Invoke(f, "czytaj");

            act.Should().Throw<RuntimeException>();
        }

        // ---------- pozycja ----------

        [Fact]
        public void Ustaw_MovesPositionAndReturnsSameFile()
        {
            var f = Open(CreateFile("a.txt", "abcdef"), "r");

            var result = Invoke(f, "ustaw", 3.0);

            result.Should().BeSameAs(f);
            Invoke(f, "czytaj").Should().Be("def");
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(0.5)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(1e30)]
        [InlineData("2")]
        [InlineData(null)]
        public void Ustaw_InvalidIndex_Throws(object? index)
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");

            var act = () => Invoke(f, "ustaw", index);

            act.Should().Throw<RuntimeException>();
        }

        [Fact]
        public void Ustaw_Zero_IsAllowed()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");
            Invoke(f, "czytaj");

            var act = () => Invoke(f, "ustaw", 0.0);

            act.Should().NotThrow();
        }

        [Fact]
        public void Ustaw_BeforeEndInAppendMode_ThrowsRuntimeException()
        {
            var f = Open(CreateFile("a.txt", "abc"), "a");

            var act = () => Invoke(f, "ustaw", 0.0);

            act.Should().Throw<RuntimeException>();
        }

        [Fact]
        public void Przewin_ReturnsToStart()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");
            Invoke(f, "czytaj");

            var result = Invoke(f, "przewiń");

            result.Should().BeSameAs(f);
            Invoke(f, "czytaj").Should().Be("abc");
        }

        [Fact]
        public void Pozycja_ReturnsDouble()
        {
            var f = Open(CreateFile("a.txt", "abc\ndef"), "r");
            Invoke(f, "czytaj_linie");

            var result = Invoke(f, "pozycja");

            result.Should().BeOfType<double>().Which.Should().Be(4.0);
        }

        [Fact]
        public void Rozmiar_ReturnsDouble()
        {
            var f = Open(CreateFile("a.txt", "żółw"), "r");

            var result = Invoke(f, "rozmiar");

            result.Should().BeOfType<double>().Which.Should().Be(7.0);
        }

        // ---------- przytnij / opróżnij ----------

        [Fact]
        public void Przytnij_CutsFileAtCurrentPosition_AndReturnsSameFile()
        {
            var path = CreateFile("rw.txt", "AAA\nBBB");
            var f = Open(path, "rw");
            Invoke(f, "czytaj_linie");

            var result = Invoke(f, "przytnij");

            result.Should().BeSameAs(f);
            Invoke(f, "zamknij");
            ReadDisk(path).Should().Be("AAA\n");
        }

        [Fact]
        public void Przytnij_InReadMode_Throws()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");

            var act = () => Invoke(f, "przytnij");

            act.Should().Throw<RuntimeException>();
        }

        [Fact]
        public void Oproznij_ReturnsSameFile()
        {
            var f = Open(PathFor("w.txt"), "w");

            Invoke(f, "opróżnij").Should().BeSameAs(f);
        }

        // ---------- zamykanie ----------

        [Fact]
        public void Zamknij_ClosesFile_AndReturnsNull()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");

            var result = Invoke(f, "zamknij");

            result.Should().BeNull();
            f.Closed.Should().BeTrue();
        }

        [Fact]
        public void Zamknij_CalledTwice_DoesNotThrow()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");
            Invoke(f, "zamknij");

            var act = () => Invoke(f, "zamknij");

            act.Should().NotThrow();
        }

        [Fact]
        public void Zamkniety_ReflectsState_AndNeverThrows()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");

            Invoke(f, "zamknięty").Should().Be(false);
            Invoke(f, "zamknij");
            Invoke(f, "zamknięty").Should().Be(true);
        }

        [Theory]
        [InlineData("czytaj")]
        [InlineData("czytaj_linie")]
        [InlineData("koniec")]
        [InlineData("przewiń")]
        [InlineData("pozycja")]
        [InlineData("rozmiar")]
        [InlineData("opróżnij")]
        [InlineData("przytnij")]
        public void ZeroArgOperations_AfterClose_ThrowRuntimeException(string name)
        {
            var f = Open(CreateFile("a.txt", "abc"), "rw");
            Invoke(f, "zamknij");

            var act = () => Invoke(f, name);

            act.Should().Throw<RuntimeException>();
        }

        [Theory]
        [InlineData("wpisz", "x")]
        [InlineData("wpisz_linie", "x")]
        [InlineData("ustaw", 0.0)]
        [InlineData("czytaj_znaki", 1.0)]
        public void OneArgOperations_AfterClose_ThrowRuntimeException(string name, object arg)
        {
            var f = Open(CreateFile("a.txt", "abc"), "rw");
            Invoke(f, "zamknij");

            var act = () => Invoke(f, name, arg);

            act.Should().Throw<RuntimeException>();
        }

        // ---------- scenariusze ----------

        [Fact]
        public void Scenario_ReadFileLineByLineUsingKoniec()
        {
            var f = Open(CreateFile("a.txt", "1\n2\n3"), "r");
            var eof = Create(f, "koniec");
            var readLine = Create(f, "czytaj_linie");
            var lines = new List<object?>();

            while (Call(eof) is false)
            {
                lines.Add(Call(readLine));
            }

            lines.Should().Equal("1", "2", "3");
        }

        [Fact]
        public void Scenario_ReadWrite_OverwriteAfterReadingFirstLine()
        {
            var path = CreateFile("rw.txt", "AAA\nBBB\nCCC");
            var f = Open(path, "rw");

            Invoke(f, "czytaj_linie").Should().Be("AAA");
            Invoke(f, "wpisz", "XYZ");
            Invoke(f, "zamknij");

            ReadDisk(path).Should().Be("AAA\nXYZ\nCCC");
        }

        [Fact]
        public void Scenario_WriteThenRewindAndRead()
        {
            var f = Open(PathFor("rw.txt"), "rw");

            Invoke(f, "wpisz_linie", "pierwsza");
            Invoke(f, "wpisz_linie", "druga");
            Invoke(f, "przewiń");

            Invoke(f, "czytaj").Should().Be("pierwsza\ndruga\n");
        }

        // ---------- ToString ----------

        [Fact]
        public void ToString_ContainsFunctionName()
        {
            var f = Open(CreateFile("a.txt", "abc"), "r");

            Create(f, "czytaj").ToString().Should().Be("<funkcja pliku czytaj>");
        }
    }
}