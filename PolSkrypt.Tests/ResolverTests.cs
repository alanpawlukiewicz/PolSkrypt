using FluentAssertions;
using PolSkrypt.Constants;
using PolSkrypt.Errors;
using PolSkrypt.Interpretation;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing;
using PolSkrypt.Parsing.Expressions;
using PolSkrypt.Parsing.Statements;
using System.Text.RegularExpressions;

namespace PolSkrypt.Tests
{
    // Claude: generating tests
    /// <summary>
    /// Resolver tests. Resolver errors are reported through ErrorHandler (static state), so this class
    /// must belong to the "ErrorHandler" collection. The correctness of variable resolution itself is
    /// verified indirectly: the program is executed by the Interpreter, which relies on the depths
    /// computed by the Resolver (nothing is written to the console in these tests).
    /// </summary>
    [Collection("ErrorHandler")]
    public class ResolverTests : IDisposable
    {
        private readonly TextWriter _originalError;
        private readonly StringWriter _errorOutput;

        public ResolverTests()
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

        // In the source, "@init" is replaced with the constructor name (FunctionConstants.CONSTRUCTOR).
        private List<Stmt> ParseValidSource(string source)
        {
            var statements = new Parser(
                new Scanner(source.Replace("@init", FunctionConstants.CONSTRUCTOR)).ScanTokens()).Parse();

            ErrorHandler.HadError.Should().BeFalse(
                "źródło testu musi być poprawne składniowo, a parser zgłosił: {0}", _errorOutput.ToString());
            return statements;
        }

        private void ResolveOnly(string source)
        {
            var statements = ParseValidSource(source);
            new Resolver(new Interpreter()).Resolve(statements);
        }

        // Resolver + execution. Additionally requires that the resolver reports no errors.
        private Interpreter Run(string source)
        {
            var statements = ParseValidSource(source);
            var interpreter = new Interpreter();
            new Resolver(interpreter).Resolve(statements);

            ErrorHandler.HadError.Should().BeFalse(
                "resolver zgłosił błąd w poprawnym programie: {0}", _errorOutput.ToString());

            foreach (var statement in statements)
            {
                statement.Accept(interpreter);
            }

            return interpreter;
        }

        private static object? Global(Interpreter interpreter, string name)
            => interpreter.VisitVariableExpression(new Variable(new Token(TokenType.IDENTIFIER, name, null, 1)));

        private static object? Eval(Interpreter interpreter) => Global(interpreter, "wynik");

        private int ErrorCount(string fragment)
            => Regex.Matches(_errorOutput.ToString(), Regex.Escape(fragment)).Count;

        // ---------------------------------------------------------------
        // Reported errors
        // ---------------------------------------------------------------

        [Theory]
        // return / break
        [InlineData("zwróć 1;", "Nie można zwrócić wartości spoza funkcji.")]
        [InlineData("zwróć;", "Nie można zwrócić wartości spoza funkcji.")]
        [InlineData("przerwij;", "Wyrzucono 'przerwij' poza pętlą.")]
        [InlineData("funkcja f() { przerwij; }", "Wyrzucono 'przerwij' poza pętlą.")]
        [InlineData("podczas_gdy (prawda) { funkcja f() { przerwij; } }", "Wyrzucono 'przerwij' poza pętlą.")]
        [InlineData("podczas_gdy (prawda) { zmienna f = funkcja() { przerwij; }; }", "Wyrzucono 'przerwij' poza pętlą.")]
        [InlineData("jeżeli (prawda) przerwij;", "Wyrzucono 'przerwij' poza pętlą.")]
        // duplicates
        [InlineData("{ zmienna a = 1; zmienna a = 2; }", "Zmienna o tej nazwie już istnieje w tym zakresie.")]
        [InlineData("funkcja f(a, a) { }", "Zmienna o tej nazwie już istnieje w tym zakresie.")]
        [InlineData("funkcja f(a) { zmienna a = 1; }", "Zmienna o tej nazwie już istnieje w tym zakresie.")]
        [InlineData("{ funkcja f() { } funkcja f() { } }", "Zmienna o tej nazwie już istnieje w tym zakresie.")]
        [InlineData("{ klasa A { } klasa A { } }", "Zmienna o tej nazwie już istnieje w tym zakresie.")]
        [InlineData("klasa A { metoda f() { } metoda f() { } }", "została już zdefiniowana w tej klasie.")]
        [InlineData("klasa A { statyczna metoda f() { } statyczna metoda f() { } }", "została już zdefiniowana w tej klasie.")]
        // reading a variable in its own initializer
        [InlineData("{ zmienna a = a; }", "Nie można odczytać lokalnej zmiennej w jej własnym inicjatorze.")]
        [InlineData("{ zmienna x = 1; { zmienna x = x; } }", "Nie można odczytać lokalnej zmiennej w jej własnym inicjatorze.")]
        [InlineData("{ zmienna a = a + 1; }", "Nie można odczytać lokalnej zmiennej w jej własnym inicjatorze.")]
        [InlineData("{ zmienna b = b[0]; }", "Nie można odczytać lokalnej zmiennej w jej własnym inicjatorze.")]
        [InlineData("{ zmienna b = b[0] = 1; }", "Nie można odczytać lokalnej zmiennej w jej własnym inicjatorze.")]
        [InlineData("{ zmienna x = x += 1; }", "Nie można odczytać lokalnej zmiennej w jej własnym inicjatorze.")]
        // ja
        [InlineData("ja;", "Nie można użyć 'ja' poza klasą.")]
        [InlineData("funkcja f() { zwróć ja; }", "Nie można użyć 'ja' poza klasą.")]
        [InlineData("klasa A { statyczna metoda f() { zwróć ja; } }", "Nie można użyć 'ja' w metodzie statycznej.")]
        [InlineData("klasa A { statyczna metoda f() { funkcja g() { zwróć ja; } } }", "Nie można użyć 'ja' w metodzie statycznej.")]
        [InlineData("klasa A { statyczna metoda f() { zmienna g = funkcja() { zwróć ja; }; } }", "Nie można użyć 'ja' w metodzie statycznej.")]
        // super
        [InlineData("super.f();", "Nie można użyć słowa kluczowego 'super' poza blokiem klasy.")]
        [InlineData("funkcja f() { super.f(); }", "Nie można użyć słowa kluczowego 'super' poza blokiem klasy.")]
        [InlineData("klasa A { metoda f() { super.f(); } }", "Nie można użyć słowa kluczowego 'super' w klasie bez superklasy.")]
        [InlineData("klasa A { } klasa B dziedziczy_po A { statyczna metoda f() { super.g(); } }", "Nie można użyć słowa kluczowego 'super' w metodzie statycznej.")]
        // inheritance, constructor
        [InlineData("klasa A dziedziczy_po A { }", "Klasa nie może dziedziczyć po samej sobie.")]
        [InlineData("klasa A { @init() { zwróć 1; } }", "Nie można zwrócić wartości z konstruktora.")]
        public void Resolve_ShouldReportExpectedError(string source, string expectedMessage)
        {
            ResolveOnly(source);

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain(expectedMessage);
        }

        [Fact]
        public void Resolve_ShouldReportLineOfOffendingToken()
        {
            ResolveOnly("zmienna a = 1;\nprzerwij;");

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("[Linia 2]");
        }

        [Fact]
        public void Resolve_ShouldReportEveryDuplicateDeclaration()
        {
            ResolveOnly("{ zmienna a = 1; zmienna a = 2; zmienna a = 3; }");

            ErrorCount("Zmienna o tej nazwie już istnieje w tym zakresie.").Should().Be(2);
        }

        [Fact]
        public void Resolve_ShouldReportEveryDuplicateMethod()
        {
            ResolveOnly("klasa A { metoda f() { } metoda f() { } metoda f() { } }");

            ErrorCount("została już zdefiniowana w tej klasie.").Should().Be(2);
        }

        [Fact]
        public void Resolve_ShouldMentionMethodNameInDuplicateMethodError()
        {
            ResolveOnly("klasa A { metoda dubel() { } metoda dubel() { } }");

            _errorOutput.ToString().Should().Contain("'dubel'");
        }

        [Fact]
        public void Resolve_ShouldReportSelfInStaticMethodExactlyOnce()
        {
            ResolveOnly("klasa A { statyczna metoda f() { zwróć ja; } }");

            ErrorCount("Nie można użyć 'ja'").Should().Be(1);
            _errorOutput.ToString().Should().NotContain("poza klasą");
        }

        [Fact]
        public void Resolve_ShouldReportSelfInheritanceExactlyOnce()
        {
            ResolveOnly("klasa A dziedziczy_po A { }");

            ErrorCount("Klasa nie może dziedziczyć po samej sobie.").Should().Be(1);
        }

        [Fact]
        public void Resolve_ShouldNotReportSuperInSubclassMethod()
        {
            // Regression: every use of 'super' used to end with an error.
            ResolveOnly("klasa A { metoda f() { } } klasa B dziedziczy_po A { metoda f() { zwróć super.f(); } }");

            ErrorHandler.HadError.Should().BeFalse();
            _errorOutput.ToString().Should().BeEmpty();
        }

        // ---------------------------------------------------------------
        // Valid programs (no errors)
        // ---------------------------------------------------------------

        [Theory]
        // variables and scopes
        [InlineData("zmienna a = 1; zmienna a = 2;")]                                        // globals can be redeclared
        [InlineData("zmienna a = a;")]                                                       // globals: the error only appears at runtime
        [InlineData("{ zmienna a = 1; { zmienna a = 2; } }")]
        [InlineData("{ zmienna x = 1; { zmienna y = x; } }")]
        [InlineData("{ zmienna x = 1; { zmienna x = 2; { zmienna z = x; } } }")]
        [InlineData("funkcja f(a) { { zmienna a = 2; } zwróć a; }")]
        [InlineData("funkcja f() { funkcja f() { } }")]
        [InlineData("{ zmienna f = funkcja() { zwróć f; }; }")]                              // recursion through an anonymous function
        [InlineData("{ zmienna a = [1, 2]; zmienna b = a[0]; a[1] = b; }")]
        [InlineData("funkcja f(l, i) { l[i] = 1; zwróć l[i]; }")]
        [InlineData("{ zmienna x = 1; x += 2; }")]
        // loops
        [InlineData("podczas_gdy (prawda) { przerwij; }")]
        [InlineData("dla (;;) { przerwij; }")]
        [InlineData("podczas_gdy (prawda) { podczas_gdy (fałsz) { } przerwij; }")]           // loop flag restored after the inner loop
        [InlineData("podczas_gdy (prawda) { funkcja f() { } przerwij; }")]                   // ... and after a function
        [InlineData("podczas_gdy (prawda) { funkcja f() { podczas_gdy (prawda) { przerwij; } } przerwij; }")]
        // functions
        [InlineData("funkcja f(a) { zwróć a; }")]
        [InlineData("funkcja f() { zwróć; }")]
        // classes
        [InlineData("klasa A { metoda f() { zwróć ja; } }")]
        [InlineData("klasa A { metoda f() { funkcja g() { zwróć ja; } zwróć g(); } }")]
        [InlineData("klasa A { metoda f() { zwróć funkcja() { zwróć ja; }; } }")]
        [InlineData("klasa A { @init() { zwróć; } }")]                                       // a bare return in a constructor is allowed
        [InlineData("klasa A { statyczna metoda f() { zwróć 1; } metoda f() { zwróć 2; } }")] // same name: static and instance method
        [InlineData("klasa A { statyczna metoda f() { klasa B { metoda g() { zwróć ja; } } } }")] // class nested in a static method
        [InlineData("klasa A { statyczna metoda f() { zwróć 1; } } klasa B { metoda g() { zwróć ja; } }")] // static flag restored
        // inheritance and super
        [InlineData("klasa A { } klasa B dziedziczy_po A { metoda f() { zwróć super.f(); } }")]
        [InlineData("klasa A { } klasa B dziedziczy_po A { metoda f() { zwróć funkcja() { zwróć super.f(); }; } }")]
        [InlineData("klasa A { } klasa B dziedziczy_po A { @init() { super.@init(); } }")]
        public void Resolve_ShouldNotReportErrorForValidSource(string source)
        {
            ResolveOnly(source);

            ErrorHandler.HadError.Should().BeFalse();
            _errorOutput.ToString().Should().BeEmpty();
        }

        // ---------------------------------------------------------------
        // Restoring state after leaving a construct
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("podczas_gdy (fałsz) { } przerwij;", "Wyrzucono 'przerwij' poza pętlą.")]
        [InlineData("dla (;;) { przerwij; } przerwij;", "Wyrzucono 'przerwij' poza pętlą.")]
        [InlineData("funkcja f() { } zwróć 1;", "Nie można zwrócić wartości spoza funkcji.")]
        [InlineData("klasa A { metoda f() { zwróć 1; } } zwróć 1;", "Nie można zwrócić wartości spoza funkcji.")]
        [InlineData("klasa A { metoda f() { } } ja;", "Nie można użyć 'ja' poza klasą.")]
        [InlineData("klasa A { statyczna metoda f() { } } ja;", "Nie można użyć 'ja' poza klasą.")]
        [InlineData("klasa A { } klasa B dziedziczy_po A { } super.f();", "poza blokiem klasy")]
        [InlineData("klasa A { statyczna metoda f() { } metoda g() { zwróć super.h(); } }", "w klasie bez superklasy")]
        public void Resolve_ShouldRestoreStateAfterLeavingConstruct(string source, string expectedMessage)
        {
            ResolveOnly(source);

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain(expectedMessage);
        }

        [Fact]
        public void Resolve_ShouldNotLetStaticFlagLeakIntoLaterClassesAndFunctions()
        {
            // Static methods are analyzed after instance methods, and the flag must not leak
            // into the next class or into code following the class.
            ResolveOnly(@"
klasa A { statyczna metoda f() { zwróć 1; } }
klasa B { metoda g() { zwróć ja; } }
funkcja h() { zwróć 1; }");

            ErrorHandler.HadError.Should().BeFalse();
        }

        // ---------------------------------------------------------------
        // Correctness of variable resolution (result of running the program)
        // ---------------------------------------------------------------

        [Fact]
        public void Run_Closure_BindsVariableToDeclarationNotToLaterShadowing()
        {
            var interpreter = Run(@"
zmienna a = ""global"";
zmienna r1;
zmienna r2;
{
    funkcja pokaz() { zwróć a; }
    r1 = pokaz();
    zmienna a = ""blok"";
    r2 = pokaz();
}");

            Global(interpreter, "r1").Should().Be("global");
            Global(interpreter, "r2").Should().Be("global");
        }

        [Fact]
        public void Run_NestedBlocks_ReadInnermostVariable()
        {
            var interpreter = Run(@"
zmienna wynik;
{ zmienna x = 1; { zmienna x = 2; { wynik = x; } } }");

            Eval(interpreter).Should().Be(2.0);
        }

        [Fact]
        public void Run_AssignmentInNestedBlock_UpdatesOuterLocal()
        {
            var interpreter = Run(@"
zmienna wynik;
{
    zmienna x = 1;
    { x = 5; }
    wynik = x;
}");

            Eval(interpreter).Should().Be(5.0);
        }

        [Fact]
        public void Run_ParameterAndBodyShareScope_InnerBlockShadowingDoesNotLeak()
        {
            var interpreter = Run(@"
funkcja f(a) { { zmienna a = 2; } zwróć a; }
zmienna wynik = f(1);");

            Eval(interpreter).Should().Be(1.0);
        }

        [Fact]
        public void Run_ParameterShadowsGlobal()
        {
            var interpreter = Run(@"
zmienna x = ""global"";
funkcja f(x) { zwróć x; }
zmienna wynik = f(""param"");");

            Eval(interpreter).Should().Be("param");
        }

        [Fact]
        public void Run_ReturnedClosure_KeepsCapturedParameter()
        {
            var interpreter = Run(@"
funkcja dodajacy(n) { zwróć funkcja(x) { zwróć x + n; }; }
zmienna wynik = dodajacy(3)(4);");

            Eval(interpreter).Should().Be(7.0);
        }

        [Fact]
        public void Run_SiblingClosures_ShareCapturedVariable()
        {
            var interpreter = Run(@"
funkcja para() {
    zmienna n = 0;
    funkcja zwieksz() { n = n + 1; }
    funkcja pobierz() { zwróć n; }
    zwróć [zwieksz, pobierz];
}
zmienna p = para();
p[0]();
p[0]();
zmienna wynik = p[1]();");

            Eval(interpreter).Should().Be(2.0);
        }

        [Fact]
        public void Run_CompoundAssignmentInNestedBlock_ResolvesToEnclosingLocal()
        {
            var interpreter = Run(@"
funkcja f() { zmienna i = 0; { i += 2; } i *= 5; zwróć i; }
zmienna wynik = f();");

            Eval(interpreter).Should().Be(10.0);
        }

        [Fact]
        public void Run_LocalFunction_CanCallItselfRecursively()
        {
            var interpreter = Run(@"
zmienna wynik;
{
    funkcja suma(n) { jeżeli (n == 0) zwróć 0; zwróć n + suma(n - 1); }
    wynik = suma(4);
}");

            Eval(interpreter).Should().Be(10.0);
        }

        [Fact]
        public void Run_VariableDeclaredInLoopBody_IsVisibleOnlyInThatIteration()
        {
            var interpreter = Run(@"
zmienna suma = 0;
dla (zmienna i = 1; i <= 3; i += 1) {
    zmienna kwadrat = i * i;
    suma += kwadrat;
}
zmienna wynik = suma;");

            Eval(interpreter).Should().Be(14.0);
        }

        // ---------- lists (the index must be resolved) ----------

        [Fact]
        public void Run_ListIndexAssign_UsesLocalIndexVariableInsteadOfGlobalOne()
        {
            // Regression: the Resolver did not resolve the index expression in a[i] = x,
            // so the global 'i' (0) was used instead of the local 'i' (2).
            var interpreter = Run(@"
zmienna i = 0;
funkcja f() {
    zmienna l = [0, 0, 0];
    zmienna i = 2;
    l[i] = 7;
    zwróć l[2];
}
zmienna wynik = f();");

            Eval(interpreter).Should().Be(7.0);
        }

        [Fact]
        public void Run_ListIndexAssign_UsesParameterAsIndex()
        {
            var interpreter = Run(@"
zmienna k = 9;
funkcja ustaw(l, k, v) { l[k] = v; }
zmienna a = [1, 2, 3];
ustaw(a, 1, 5);
zmienna wynik = a[1];");

            Eval(interpreter).Should().Be(5.0);
        }

        [Fact]
        public void Run_ListIndexRead_UsesLocalIndexAndLocalList()
        {
            var interpreter = Run(@"
zmienna i = 2;
funkcja f(l) { zmienna i = 0; zwróć l[i]; }
zmienna wynik = f([10, 20, 30]);");

            Eval(interpreter).Should().Be(10.0);
        }

        [Fact]
        public void Run_ListIndexAssign_ResolvesIndexInsideNestedBlock()
        {
            var interpreter = Run(@"
zmienna wynik;
{
    zmienna l = [0, 0];
    zmienna j = 1;
    { l[j] = 4; }
    wynik = l[1];
}");

            Eval(interpreter).Should().Be(4.0);
        }

        // ---------- ja (self) and super ----------

        [Fact]
        public void Run_Self_IsAvailableInNestedFunctionAndAnonFunctionInsideMethod()
        {
            var interpreter = Run(@"
klasa A {
    @init(x) { ja.x = x; }
    metoda f() { funkcja g() { zwróć ja.x; } zwróć g(); }
    metoda h() { zwróć funkcja() { zwróć ja.x * 2; }(); }
}
zmienna a = A(4);
zmienna r1 = a.f();
zmienna r2 = a.h();");

            Global(interpreter, "r1").Should().Be(4.0);
            Global(interpreter, "r2").Should().Be(8.0);
        }

        [Fact]
        public void Run_Super_InMultiLevelHierarchy_BindsToDefiningClass()
        {
            var interpreter = Run(@"
klasa A { metoda f() { zwróć ""A""; } }
klasa B dziedziczy_po A { metoda f() { zwróć super.f() & ""B""; } }
klasa C dziedziczy_po B { metoda f() { zwróć super.f() & ""C""; } }
zmienna wynik = C().f();");

            Eval(interpreter).Should().Be("ABC");
        }

        [Fact]
        public void Run_Super_InsideAnonFunctionInMethod_Works()
        {
            var interpreter = Run(@"
klasa A { metoda f() { zwróć ""A""; } }
klasa B dziedziczy_po A {
    metoda f() { zmienna g = funkcja() { zwróć super.f() & ""B""; }; zwróć g(); }
}
zmienna wynik = B().f();");

            Eval(interpreter).Should().Be("AB");
        }

        [Fact]
        public void Run_Super_InConstructor_InitializesSuperclassFields()
        {
            var interpreter = Run(@"
klasa A { @init(x) { ja.x = x; } }
klasa B dziedziczy_po A { @init(x, y) { super.@init(x); ja.y = y; } }
zmienna b = B(1, 2);
zmienna wynik = b.x + b.y;");

            Eval(interpreter).Should().Be(3.0);
        }

        // ---------- local classes and static methods ----------

        [Fact]
        public void Run_LocalClass_MethodCapturesVariableFromEnclosingFunction()
        {
            var interpreter = Run(@"
funkcja f() {
    zmienna baza = 10;
    klasa A { metoda daj() { zwróć baza + 1; } }
    zwróć A().daj();
}
zmienna wynik = f();");

            Eval(interpreter).Should().Be(11.0);
        }

        [Fact]
        public void Run_StaticMethod_CanUseNestedClosureOverItsParameters()
        {
            var interpreter = Run(@"
klasa M {
    statyczna metoda f(x) { funkcja g(y) { zwróć x + y; } zwróć g(1); }
}
zmienna wynik = M.f(2);");

            Eval(interpreter).Should().Be(3.0);
        }

        [Fact]
        public void Run_ClassNestedInStaticMethod_CanUseSelfInItsOwnMethods()
        {
            var interpreter = Run(@"
klasa M {
    statyczna metoda zbuduj() {
        klasa W { @init() { ja.v = 5; } metoda daj() { zwróć ja.v; } }
        zwróć W().daj();
    }
}
zmienna wynik = M.zbuduj();");

            Eval(interpreter).Should().Be(5.0);
        }

        [Fact]
        public void Run_StaticAndInstanceMethodWithSameName_BothAreCallable()
        {
            var interpreter = Run(@"
klasa A {
    statyczna metoda f() { zwróć ""s""; }
    metoda f() { zwróć ""i""; }
}
zmienna wynik = A.f() & A().f();");

            Eval(interpreter).Should().Be("si");
        }

        [Fact]
        public void Run_StaticMethod_CanReadLocalVariableFromEnclosingFunction()
        {
            // Regression: static methods used to be analyzed inside the 'ja' scope, which the interpreter
            // does not create for them, so the distance to variables outside the class was 1 too large.
            var interpreter = Run(@"
funkcja f() {
    zmienna baza = 10;
    klasa A { statyczna metoda g() { zwróć baza + 1; } }
    zwróć A.g();
}
zmienna wynik = f();");

            Eval(interpreter).Should().Be(11.0);
        }

        [Fact]
        public void Run_StaticMethodOfSubclass_CanReadLocalVariableFromEnclosingFunction()
        {
            var interpreter = Run(@"
funkcja f() {
    zmienna baza = 10;
    klasa A { }
    klasa B dziedziczy_po A { statyczna metoda g() { zwróć baza * 2; } }
    zwróć B.g();
}
zmienna wynik = f();");

            Eval(interpreter).Should().Be(20.0);
        }

        [Fact]
        public void Run_InstanceAndStaticMethods_BothSeeSameEnclosingLocal()
        {
            var interpreter = Run(@"
funkcja f() {
    zmienna baza = 5;
    klasa A {
        metoda i() { zwróć baza; }
        statyczna metoda s() { zwróć baza; }
    }
    zwróć A().i() + A.s();
}
zmienna wynik = f();");

            Eval(interpreter).Should().Be(10.0);
        }

        [Fact]
        public void Run_StaticMethodInBlock_CanAssignEnclosingBlockLocal()
        {
            var interpreter = Run(@"
zmienna wynik;
{
    zmienna licznik = 0;
    klasa A { statyczna metoda zwieksz() { licznik = licznik + 1; zwróć licznik; } }
    A.zwieksz();
    wynik = A.zwieksz();
}");

            Eval(interpreter).Should().Be(2.0);
        }

        // ---------------------------------------------------------------
        // Reusing a single interpreter (REPL)
        // ---------------------------------------------------------------

        [Fact]
        public void Resolve_ShouldWorkAcrossSeparateResolverRunsOnSameInterpreter()
        {
            var interpreter = new Interpreter();

            var first = ParseValidSource("zmienna x = 2; funkcja podwoj(a) { zwróć a * 2; }");
            new Resolver(interpreter).Resolve(first);
            foreach (var statement in first) statement.Accept(interpreter);

            var second = ParseValidSource("zmienna wynik = podwoj(x);");
            new Resolver(interpreter).Resolve(second);
            foreach (var statement in second) statement.Accept(interpreter);

            ErrorHandler.HadError.Should().BeFalse();
            Eval(interpreter).Should().Be(4.0);
        }
    }
}