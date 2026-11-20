using FluentAssertions;
using PolSkrypt.Errors;
using PolSkrypt.Interpretation;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing;
using PolSkrypt.Parsing.Expressions;
using PolSkrypt.Parsing.Statements;
using System.Text;

namespace PolSkrypt.Tests
{
    // Claude: generating tests
    /// <summary>
    /// Tests for module imports ("importuj ... jako ..." and "importuj ... zsw ...").
    /// Each test writes .pols files to its own temporary directory and runs the main file
    /// the same way Program.RunFile does (RegisterEntryFile -> Compile -> Interpret).
    /// Errors go through ErrorHandler (static state) and some tests capture Console.Out,
    /// so the class belongs to the non-parallel "ErrorHandler" collection.
    /// </summary>
    [Collection("ErrorHandler")]
    public class ImportTests : IDisposable
    {
        private readonly string _dir;
        private readonly TextWriter _originalError;
        private readonly TextWriter _originalOut;
        private readonly StringWriter _errorOutput;
        private readonly StringWriter _standardOutput;

        public ImportTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), "polskrypt_import_tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);

            _originalError = Console.Error;
            _originalOut = Console.Out;
            _errorOutput = new StringWriter();
            _standardOutput = new StringWriter();
            Console.SetError(_errorOutput);
            Console.SetOut(_standardOutput);

            ErrorHandler.HadError = false;
            ErrorHandler.HadRuntimeError = false;
        }

        public void Dispose()
        {
            Console.SetError(_originalError);
            Console.SetOut(_originalOut);
            _errorOutput.Dispose();
            _standardOutput.Dispose();

            try
            {
                Directory.Delete(_dir, true);
            }
            catch (IOException)
            {
                // Temp directory cleanup is best-effort.
            }
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private string CreateFile(string relativePath, string content)
        {
            var path = Path.Combine(_dir, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content, new UTF8Encoding(false));
            return path;
        }

        // Runs a main file located at <temp>/<mainName>, mirroring Program.RunFile.
        private Interpreter RunMain(string source, string mainName = "main.pols")
        {
            var mainPath = CreateFile(mainName, source);
            var interpreter = new Interpreter();
            interpreter.Modules.RegisterEntryFile(Path.GetFullPath(mainPath));

            var statements = interpreter.Modules.Compile(source);
            if (statements != null)
            {
                interpreter.Interpret(statements);
            }

            return interpreter;
        }

        // Runs a program that must finish without any compile or runtime error.
        private Interpreter RunValid(string source, string mainName = "main.pols")
        {
            var interpreter = RunMain(source, mainName);

            ErrorHandler.HadError.Should().BeFalse("program powinien być poprawny, a zgłoszono: {0}", _errorOutput.ToString());
            ErrorHandler.HadRuntimeError.Should().BeFalse("program powinien działać bez błędów, a zgłoszono: {0}", _errorOutput.ToString());

            return interpreter;
        }

        private static object? Global(Interpreter interpreter, string name)
            => interpreter.VisitVariableExpression(new Variable(new Token(TokenType.IDENTIFIER, name, null, 1)));

        private static Import ParseSingleImport(string source)
        {
            var statements = new Parser(new Scanner(source).ScanTokens()).Parse();
            statements.Should().ContainSingle();
            return statements[0].Should().BeOfType<Import>().Subject;
        }

        private const string MathModule = @"
zmienna PI = 3;
zmienna _ukryte = 1;
zmienna licznik = 0;

funkcja pole(r) { zwróć PI * r * r; }
funkcja zwieksz() { licznik += 1; zwróć licznik; }

klasa Punkt
{
    konstruktor(x) { ja.x = x; }
    metoda przesuniete() { zwróć ja.x + PI; }
}
";

        // ---------------------------------------------------------------
        // Parsing
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_AliasForm_CreatesImportWithAliasAndNoNames()
        {
            var import = ParseSingleImport("importuj \"mat.pols\" jako mat;");

            ErrorHandler.HadError.Should().BeFalse();
            import.Path.Literal.Should().Be("mat.pols");
            import.Alias!.Lexeme.Should().Be("mat");
            import.Imports.Should().BeNull();
            import.Keyword.Type.Should().Be(TokenType.IMPORT);
        }

        [Theory]
        [InlineData("zsw")]
        [InlineData("znajdujący_się_w")]
        public void Parse_NamesForm_CreatesImportWithNamesAndNoAlias(string fromKeyword)
        {
            var import = ParseSingleImport($"importuj pole, PI {fromKeyword} \"mat.pols\";");

            ErrorHandler.HadError.Should().BeFalse();
            import.Path.Literal.Should().Be("mat.pols");
            import.Alias.Should().BeNull();
            import.Imports!.Select(t => t.Lexeme).Should().Equal("pole", "PI");
        }

        [Theory]
        [InlineData("importuj \"mat.pols\";", "Oczekiwano 'jako'.")]
        [InlineData("importuj \"mat.pols\" jako;", "Oczekiwano aliasu dla importu")]
        [InlineData("importuj \"mat.pols\" jako mat", "Oczekiwano ';' po deklaracji importu.")]
        [InlineData("importuj pole \"mat.pols\";", "Oczekiwano 'znajdujący_się_w'.")]
        [InlineData("importuj pole zsw mat;", "Oczekiwano ścieżki importu.")]
        [InlineData("importuj pole zsw \"mat.pols\"", "Oczekiwano ';' po deklaracji importu.")]
        [InlineData("importuj zsw \"mat.pols\";", "Oczekiwano nazwy zmiennej.")]
        public void Parse_MalformedImport_ReportsError(string source, string expectedMessage)
        {
            new Parser(new Scanner(source).ScanTokens()).Parse();

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain(expectedMessage);
        }

        // ---------------------------------------------------------------
        // Resolver: imports only at the top level
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("{ importuj \"mat.pols\" jako mat; }")]
        [InlineData("funkcja f() { importuj pole zsw \"mat.pols\"; }")]
        [InlineData("jeżeli (prawda) { importuj \"mat.pols\" jako mat; }")]
        [InlineData("klasa K { metoda m() { importuj \"mat.pols\" jako mat; } }")]
        public void Import_NotAtTopLevel_ReportsResolverError(string source)
        {
            CreateFile("mat.pols", MathModule);

            RunMain(source);

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("Import jest dozwolony tylko na najwyższym poziomie pliku.");
        }

        // ---------------------------------------------------------------
        // Alias form: importuj "x" jako m;
        // ---------------------------------------------------------------

        [Fact]
        public void AliasImport_ExposesVariablesFunctionsAndClasses()
        {
            CreateFile("mat.pols", MathModule);

            var interpreter = RunValid(@"
importuj ""mat.pols"" jako mat;
zmienna pi = mat.PI;
zmienna p = mat.pole(2);
zmienna punkt = mat.Punkt(5).przesuniete();
");

            Global(interpreter, "pi").Should().Be(3.0);
            Global(interpreter, "p").Should().Be(12.0);
            Global(interpreter, "punkt").Should().Be(8.0);
        }

        [Fact]
        public void AliasImport_BindsModuleObject()
        {
            CreateFile("mat.pols", MathModule);

            var interpreter = RunValid("importuj \"mat.pols\" jako mat;");

            var module = Global(interpreter, "mat").Should().BeOfType<PModule>().Subject;
            module.Name.Should().Be("mat");
            module.ToString().Should().Be("<moduł mat>");
        }

        // ---------------------------------------------------------------
        // Names form: importuj a, b zsw "x";
        // ---------------------------------------------------------------

        [Fact]
        public void NamedImport_DefinesSelectedNamesOnly()
        {
            CreateFile("mat.pols", MathModule);

            var interpreter = RunValid(@"
importuj pole, PI zsw ""mat.pols"";
zmienna p = pole(1);
");

            Global(interpreter, "p").Should().Be(3.0);
            Global(interpreter, "PI").Should().Be(3.0);

            var act = () => Global(interpreter, "zwieksz");
            act.Should().Throw<PolSkrypt.Errors.Exceptions.RuntimeException>();
        }

        [Fact]
        public void NamedImport_IsACopyOfTheValue_NotALiveBinding()
        {
            CreateFile("mat.pols", MathModule);

            var interpreter = RunValid(@"
importuj licznik, zwieksz zsw ""mat.pols"";
importuj ""mat.pols"" jako mat;
zwieksz();
zmienna kopia = licznik;
zmienna aktualny = mat.licznik;
");

            Global(interpreter, "kopia").Should().Be(0.0);
            Global(interpreter, "aktualny").Should().Be(1.0);
        }

        // ---------------------------------------------------------------
        // Scoping: module globals vs. importer globals vs. builtins
        // ---------------------------------------------------------------

        [Fact]
        public void ModuleFunction_UsesGlobalsOfItsOwnModule_NotOfTheImporter()
        {
            CreateFile("mat.pols", MathModule);

            var interpreter = RunValid(@"
zmienna PI = 999;
importuj pole zsw ""mat.pols"";
zmienna p = pole(1);
");

            Global(interpreter, "p").Should().Be(3.0);
            Global(interpreter, "PI").Should().Be(999.0);
        }

        [Fact]
        public void ModuleFunction_CanModifyGlobalsOfItsOwnModule()
        {
            CreateFile("mat.pols", MathModule);

            var interpreter = RunValid(@"
importuj ""mat.pols"" jako mat;
mat.zwieksz();
mat.zwieksz();
zmienna wynik = mat.licznik;
");

            Global(interpreter, "wynik").Should().Be(2.0);
        }

        [Fact]
        public void Module_CannotSeeVariablesOfTheImporter()
        {
            CreateFile("mod.pols", "zmienna y = x;");

            RunMain(@"
zmienna x = 1;
importuj ""mod.pols"" jako mod;
");

            ErrorHandler.HadRuntimeError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("Niezidentyfikowana zmienna 'x'.");
        }

        [Fact]
        public void Module_CanUseBuiltInFunctions()
        {
            CreateFile("mod.pols", "zmienna t = typ(1);\nwypisz(\"z modułu\");");

            var interpreter = RunValid("importuj t zsw \"mod.pols\";");

            Global(interpreter, "t").Should().Be(PolSkrypt.Constants.Keywords.NUMBER);
            _standardOutput.ToString().Should().Contain("z modułu");
        }

        [Fact]
        public void Module_DoesNotExposeBuiltInFunctions()
        {
            CreateFile("mat.pols", MathModule);

            RunMain(@"
importuj ""mat.pols"" jako mat;
zmienna w = mat.wypisz;
");

            ErrorHandler.HadRuntimeError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("Moduł 'mat' nie zawiera 'wypisz'.");
        }

        // ---------------------------------------------------------------
        // Caching
        // ---------------------------------------------------------------

        [Fact]
        public void Module_ImportedTwice_IsExecutedOnceAndShared()
        {
            CreateFile("mat.pols", MathModule + "\nwypisz(\"ładowanie\");");

            var interpreter = RunValid(@"
importuj ""mat.pols"" jako a;
importuj ""mat.pols"" jako b;
importuj zwieksz zsw ""mat.pols"";
a.zwieksz();
zwieksz();
zmienna wynik = b.zwieksz();
");

            Global(interpreter, "wynik").Should().Be(3.0);
            Global(interpreter, "a").Should().BeSameAs(Global(interpreter, "b"));
            _standardOutput.ToString().Split("ładowanie").Length.Should().Be(2, "moduł powinien wykonać się dokładnie raz");
        }

        [Fact]
        public void Module_ImportedFromTwoModules_IsExecutedOnce()
        {
            CreateFile("wspolny.pols", "zmienna licznik = 0;\nfunkcja zwieksz() { licznik += 1; zwróć licznik; }");
            CreateFile("a.pols", "importuj zwieksz zsw \"wspolny.pols\";\nzmienna wynikA = zwieksz();");
            CreateFile("b.pols", "importuj zwieksz zsw \"wspolny.pols\";\nzmienna wynikB = zwieksz();");

            var interpreter = RunValid(@"
importuj wynikA zsw ""a.pols"";
importuj wynikB zsw ""b.pols"";
");

            Global(interpreter, "wynikA").Should().Be(1.0);
            Global(interpreter, "wynikB").Should().Be(2.0);
        }

        // ---------------------------------------------------------------
        // Paths
        // ---------------------------------------------------------------

        [Fact]
        public void Import_PathIsRelativeToTheImportingFile()
        {
            CreateFile(Path.Combine("lib", "inny.pols"), "zmienna X = \"z lib\";");
            CreateFile(Path.Combine("lib", "helper.pols"), "importuj \"inny.pols\" jako inny;\nfunkcja h() { zwróć inny.X; }");

            var interpreter = RunValid(@"
importuj h zsw ""../lib/helper.pols"";
zmienna wynik = h();
", Path.Combine("src", "main.pols"));

            Global(interpreter, "wynik").Should().Be("z lib");
        }

        [Fact]
        public void Import_SameFileThroughDifferentRelativePaths_IsCachedOnce()
        {
            CreateFile(Path.Combine("lib", "licznik.pols"), "zmienna n = 0;\nfunkcja zwieksz() { n += 1; zwróć n; }");

            var interpreter = RunValid(@"
importuj ""lib/licznik.pols"" jako a;
importuj ""lib/../lib/licznik.pols"" jako b;
a.zwieksz();
zmienna wynik = b.zwieksz();
");

            Global(interpreter, "wynik").Should().Be(2.0);
        }

        // ---------------------------------------------------------------
        // Error cases
        // ---------------------------------------------------------------

        [Fact]
        public void Import_MissingFile_ReportsRuntimeError()
        {
            RunMain("importuj \"brak.pols\" jako b;");

            ErrorHandler.HadRuntimeError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("Nie znaleziono modułu").And.Contain("brak.pols");
        }

        [Fact]
        public void Import_MissingName_ReportsRuntimeError()
        {
            CreateFile("mat.pols", MathModule);

            RunMain("importuj nieistnieje zsw \"mat.pols\";");

            ErrorHandler.HadRuntimeError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("Moduł 'mat' nie zawiera 'nieistnieje'.");
        }

        [Theory]
        [InlineData("importuj \"mat.pols\" jako mat;\nzmienna u = mat._ukryte;")]
        [InlineData("importuj _ukryte zsw \"mat.pols\";")]
        public void Import_PrivateName_ReportsRuntimeError(string source)
        {
            CreateFile("mat.pols", MathModule);

            RunMain(source);

            ErrorHandler.HadRuntimeError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("'_ukryte' jest prywatne w module 'mat'.");
        }

        [Fact]
        public void Import_CycleBetweenModules_ReportsRuntimeError()
        {
            CreateFile("a.pols", "importuj \"b.pols\" jako b;");
            CreateFile("b.pols", "importuj \"a.pols\" jako a;");

            RunMain("importuj \"a.pols\" jako a;");

            ErrorHandler.HadRuntimeError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("Cykliczny import pliku").And.Contain("a.pols");
        }

        [Fact]
        public void Import_OfTheEntryFile_ReportsCycle()
        {
            RunMain("importuj \"main.pols\" jako ja_sam;");

            ErrorHandler.HadRuntimeError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("Cykliczny import pliku").And.Contain("main.pols");
        }

        [Fact]
        public void Import_ModuleWithSyntaxError_ReportsErrorWithModuleFileName()
        {
            CreateFile("zly.pols", "zmienna a = 1;\nzmienna = ;");

            var interpreter = RunMain(@"
importuj ""zly.pols"" jako zly;
zmienna dalej = 1;
");

            ErrorHandler.HadError.Should().BeTrue();
            var output = _errorOutput.ToString();
            output.Should().Contain("zly.pols, Linia 2]");
            output.Should().Contain("Błąd w module").And.Contain("zly.pols");

            var act = () => Global(interpreter, "dalej");
            act.Should().Throw<PolSkrypt.Errors.Exceptions.RuntimeException>("wykonanie powinno zatrzymać się na imporcie");
        }

        [Fact]
        public void Import_ModuleWithScannerError_ReportsErrorWithModuleFileName()
        {
            CreateFile("znak.pols", "zmienna a = 1;\nzmienna b = 2 $ 3;");

            RunMain("importuj \"znak.pols\" jako z;");

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("znak.pols, Linia 2] Błąd : Nieoczekiwany znak: '$'");
        }

        [Fact]
        public void RuntimeErrorWhileLoadingModule_ReportsModuleFileName()
        {
            CreateFile("dzielenie.pols", "zmienna a = 1;\nzmienna b = a / 0;");

            RunMain("importuj \"dzielenie.pols\" jako d;");

            ErrorHandler.HadRuntimeError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("dzielenie.pols, Linia 2] Błąd: Pamiętaj cholero, nigdy nie dziel przez 0!");
        }

        [Fact]
        public void RuntimeErrorInModuleFunctionCalledFromMain_ReportsModuleFileName()
        {
            CreateFile(Path.Combine("lib", "dziel.pols"), "funkcja dziel(a, b)\n{\n    zwróć a / b;\n}");

            RunMain("importuj dziel zsw \"lib/dziel.pols\";\nzmienna x = dziel(1, 0);");

            ErrorHandler.HadRuntimeError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("dziel.pols, Linia 3] Błąd: Pamiętaj cholero, nigdy nie dziel przez 0!");
        }

        [Fact]
        public void NativeFunctionErrorInModule_ReportsModuleFileNameAndLine()
        {
            CreateFile("parser.pols", "funkcja parsuj(t)\n{\n    zwróć liczba(t);\n}");

            RunMain("importuj parsuj zsw \"parser.pols\";\nzmienna x = parsuj(\"abc\");");

            ErrorHandler.HadRuntimeError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("parser.pols, Linia 3] Błąd: Argument musi być liczbą.");
        }

        [Fact]
        public void RuntimeErrorInMainFile_KeepsFormatWithoutFileName()
        {
            CreateFile("mat.pols", MathModule);

            RunMain("importuj \"mat.pols\" jako mat;\nzmienna x = 1 / 0;");

            ErrorHandler.HadRuntimeError.Should().BeTrue();
            var output = _errorOutput.ToString();
            output.Should().StartWith("[Linia 2] Błąd:");
            output.Should().NotContain(".pols");
        }

        [Fact]
        public void AssigningToModuleField_ReportsRuntimeError()
        {
            CreateFile("mat.pols", MathModule);

            var interpreter = RunMain(@"
importuj ""mat.pols"" jako mat;
mat.PI = 4;
");

            ErrorHandler.HadRuntimeError.Should().BeTrue();
            ((PModule)Global(interpreter, "mat")!).Env.TryGetOwn("PI", out var pi).Should().BeTrue();
            pi.Should().Be(3.0);
        }

        // ---------------------------------------------------------------
        // Name conflicts
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("zmienna pole = 1;\nimportuj pole zsw \"mat.pols\";", "pole")]
        [InlineData("zmienna mat = 1;\nimportuj \"mat.pols\" jako mat;", "mat")]
        [InlineData("funkcja pole() {}\nimportuj pole zsw \"mat.pols\";", "pole")]
        [InlineData("importuj pole zsw \"mat.pols\";\nimportuj pole zsw \"mat.pols\";", "pole")]
        [InlineData("importuj \"mat.pols\" jako mat;\nimportuj \"mat.pols\" jako mat;", "mat")]
        public void Import_NameAlreadyDefined_ReportsRuntimeErrorOnImportLine(string source, string name)
        {
            CreateFile("mat.pols", MathModule);

            RunMain(source);

            ErrorHandler.HadRuntimeError.Should().BeTrue();
            ErrorHandler.HadError.Should().BeFalse("konflikt przy imporcie to błąd wykonania, nie Env.Define");
            _errorOutput.ToString().Should().Contain(
                $"[Linia 2] Błąd: Nie można zaimportować '{name}': nazwa jest już zdefiniowana w tym pliku.");
        }

        [Fact]
        public void Import_DuplicateNameInOneImport_ReportsRuntimeError()
        {
            CreateFile("mat.pols", MathModule);

            RunMain("importuj pole, pole zsw \"mat.pols\";");

            ErrorHandler.HadRuntimeError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("Nie można zaimportować 'pole': nazwa jest już zdefiniowana w tym pliku.");
        }

        [Fact]
        public void Import_NameConflict_StopsExecution()
        {
            CreateFile("mat.pols", MathModule);

            var interpreter = RunMain(@"
zmienna pole = 1;
importuj pole zsw ""mat.pols"";
zmienna dalej = 1;
");

            ErrorHandler.HadRuntimeError.Should().BeTrue();
            Global(interpreter, "pole").Should().Be(1.0, "istniejąca zmienna nie powinna zostać nadpisana");
            var act = () => Global(interpreter, "dalej");
            act.Should().Throw<PolSkrypt.Errors.Exceptions.RuntimeException>();
        }

        [Fact]
        public void Import_SameNameInDifferentModules_DoesNotConflict()
        {
            CreateFile("a.pols", "zmienna wartosc = \"a\";");
            CreateFile("b.pols", "importuj \"a.pols\" jako a;\nzmienna wartosc = \"b\";\nzmienna zA = a.wartosc;");

            var interpreter = RunValid(@"
importuj ""b.pols"" jako b;
importuj ""a.pols"" jako a;
zmienna wynikA = a.wartosc;
zmienna wynikB = b.wartosc;
zmienna wynikZB = b.zA;
");

            Global(interpreter, "wynikA").Should().Be("a");
            Global(interpreter, "wynikB").Should().Be("b");
            Global(interpreter, "wynikZB").Should().Be("a");
        }
    }
}
