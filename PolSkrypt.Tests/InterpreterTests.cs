using FluentAssertions;
using PolSkrypt.Constants;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Interpretation;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing;
using PolSkrypt.Parsing.Expressions;
using PolSkrypt.Parsing.Statements;

namespace PolSkrypt.Tests
{
    // Claude: class generation
    [Collection("Console")]
    public class InterpreterTests
    {
        // ---------- Helpers ----------

        private static Token T(TokenType type, string lexeme, object? literal = null, int line = 1)
            => new Token(type, lexeme, literal, line);

        private static Literal Lit(object? value) => new Literal(value);

        private static Binary Bin(Expr left, TokenType type, string lexeme, Expr right)
            => new Binary(left, T(type, lexeme), right);

        private static Variable Ident(string name) => new Variable(T(TokenType.IDENTIFIER, name));

        private static Var Declare(string name, Expr? initializer = null)
            => new Var(T(TokenType.IDENTIFIER, name), initializer);

        private static Assign AssignTo(string name, Expr value)
            => new Assign(T(TokenType.IDENTIFIER, name), value);

        private static string CaptureOutput(Action action)
        {
            var writer = new StringWriter();
            var original = Console.Out;
            Console.SetOut(writer);
            try
            {
                action();
            }
            finally
            {
                Console.SetOut(original);
            }

            return writer.ToString();
        }

        // Resolver musi przejść po AST przed wykonaniem - wypełnia mapę _locals interpretera.
        // Węzły AST muszą być tymi samymi obiektami, które potem wykonuje interpreter.
        private static Interpreter CreateResolved(params Stmt[] statements)
        {
            var interpreter = new Interpreter();
            new Resolver(interpreter).Resolve(statements.ToList());
            return interpreter;
        }

        private static string Run(Interpreter interpreter, params Stmt[] statements)
            => CaptureOutput(() =>
            {
                var list = statements.ToList();
                new Resolver(interpreter).Resolve(list);
                interpreter.Interpret(list);
            });

        private static string Run(params Stmt[] statements) => Run(new Interpreter(), statements);

        // ---------- Binary ----------

        [Fact]
        public void VisitBinaryExpression_ReturnsExpectedResult()
        {
            var expr = new Binary(new Literal(5.0), T(TokenType.PLUS, "+"), new Literal(3.0));
            var interpreter = new Interpreter();

            object? result = interpreter.VisitBinaryExpression(expr);

            result.Should().Be(8.0);
        }

        [Theory]
        [InlineData(TokenType.MINUS, "-", 10.0, 4.0, 6.0)]
        [InlineData(TokenType.STAR, "*", 6.0, 7.0, 42.0)]
        [InlineData(TokenType.SLASH, "/", 9.0, 3.0, 3.0)]
        [InlineData(TokenType.PLUS, "+", 1.0, 2.0, 3.0)]
        public void Binary_Arithmetic_ReturnsExpectedNumber(
            TokenType type, string lexeme, double left, double right, double expected)
        {
            var interpreter = new Interpreter();

            var result = interpreter.VisitBinaryExpression(Bin(Lit(left), type, lexeme, Lit(right)));

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(TokenType.GREATER, ">", 5.0, 3.0, true)]
        [InlineData(TokenType.GREATER, ">", 3.0, 3.0, false)]
        [InlineData(TokenType.GREATER_EQUAL, ">=", 3.0, 3.0, true)]
        [InlineData(TokenType.GREATER_EQUAL, ">=", 2.0, 3.0, false)]
        [InlineData(TokenType.LESS, "<", 2.0, 3.0, true)]
        [InlineData(TokenType.LESS, "<", 3.0, 3.0, false)]
        [InlineData(TokenType.LESS_EQUAL, "<=", 3.0, 3.0, true)]
        [InlineData(TokenType.LESS_EQUAL, "<=", 4.0, 3.0, false)]
        public void Binary_Comparison_ReturnsExpectedBool(
            TokenType type, string lexeme, double left, double right, bool expected)
        {
            var interpreter = new Interpreter();

            var result = interpreter.VisitBinaryExpression(Bin(Lit(left), type, lexeme, Lit(right)));

            result.Should().Be(expected);
        }

        [Fact]
        public void Binary_DivisionByZero_ThrowsRuntimeException()
        {
            var interpreter = new Interpreter();
            var expr = Bin(Lit(5.0), TokenType.SLASH, "/", Lit(0.0));

            Action act = () => interpreter.VisitBinaryExpression(expr);

            act.Should().Throw<RuntimeException>();
        }

        [Theory]
        [InlineData(TokenType.MINUS, "-")]
        [InlineData(TokenType.STAR, "*")]
        [InlineData(TokenType.SLASH, "/")]
        [InlineData(TokenType.GREATER, ">")]
        [InlineData(TokenType.LESS, "<")]
        [InlineData(TokenType.PLUS, "+")]
        public void Binary_NonNumberOperand_ThrowsRuntimeException(TokenType type, string lexeme)
        {
            var interpreter = new Interpreter();
            var expr = Bin(Lit("a"), type, lexeme, Lit(1.0));

            Action act = () => interpreter.VisitBinaryExpression(expr);

            act.Should().Throw<RuntimeException>();
        }

        [Fact]
        public void Binary_Plus_OnStrings_ThrowsRuntimeException()
        {
            var interpreter = new Interpreter();
            var expr = Bin(Lit("a"), TokenType.PLUS, "+", Lit("b"));

            Action act = () => interpreter.VisitBinaryExpression(expr);

            act.Should().Throw<RuntimeException>();
        }

        [Fact]
        public void Binary_Ampersand_ConcatenatesStrings()
        {
            var interpreter = new Interpreter();

            var result = interpreter.VisitBinaryExpression(Bin(Lit("ala "), TokenType.AMPERSAND, "&", Lit("ma kota")));

            result.Should().Be("ala ma kota");
        }

        [Fact]
        public void Binary_Ampersand_ConcatenatesStringAndNumber()
        {
            var interpreter = new Interpreter();

            var result = interpreter.VisitBinaryExpression(Bin(Lit("liczba: "), TokenType.AMPERSAND, "&", Lit(5.0)));

            result.Should().Be("liczba: 5");
        }

        [Fact]
        public void Binary_Ampersand_WithNull_UsesNullKeyword()
        {
            var interpreter = new Interpreter();

            var result = interpreter.VisitBinaryExpression(Bin(Lit("x"), TokenType.AMPERSAND, "&", Lit(null)));

            result.Should().Be("x" + Keywords.NULL);
        }

        [Theory]
        [InlineData(1.0, 1.0, true)]
        [InlineData(1.0, 2.0, false)]
        [InlineData("a", "a", true)]
        [InlineData("a", "b", false)]
        [InlineData(1.0, "1", false)]
        public void Binary_EqualEqual_ComparesValues(object left, object right, bool expected)
        {
            var interpreter = new Interpreter();

            var result = interpreter.VisitBinaryExpression(Bin(Lit(left), TokenType.EQUAL_EQUAL, "==", Lit(right)));

            result.Should().Be(expected);
        }

        [Fact]
        public void Binary_EqualEqual_NullWithNull_IsTrue()
        {
            var interpreter = new Interpreter();

            var result = interpreter.VisitBinaryExpression(Bin(Lit(null), TokenType.EQUAL_EQUAL, "==", Lit(null)));

            result.Should().Be(true);
        }

        [Fact]
        public void Binary_EqualEqual_NullWithValue_IsFalse()
        {
            var interpreter = new Interpreter();

            var result = interpreter.VisitBinaryExpression(Bin(Lit(null), TokenType.EQUAL_EQUAL, "==", Lit(1.0)));

            result.Should().Be(false);
        }

        [Fact]
        public void Binary_BangEqual_IsNegationOfEqual()
        {
            var interpreter = new Interpreter();

            interpreter.VisitBinaryExpression(Bin(Lit(1.0), TokenType.BANG_EQUAL, "!=", Lit(2.0))).Should().Be(true);
            interpreter.VisitBinaryExpression(Bin(Lit(1.0), TokenType.BANG_EQUAL, "!=", Lit(1.0))).Should().Be(false);
        }

        // ---------- Literal / Grouping / Unary ----------

        [Theory]
        [InlineData(5.0)]
        [InlineData("tekst")]
        [InlineData(true)]
        [InlineData(null)]
        public void VisitLiteralExpression_ReturnsValue(object? value)
        {
            var interpreter = new Interpreter();

            interpreter.VisitLiteralExpression(new Literal(value)).Should().Be(value);
        }

        [Fact]
        public void VisitGroupingExpression_EvaluatesInnerExpression()
        {
            var interpreter = new Interpreter();
            // (2 + 3) * 4
            var expr = Bin(
                new Grouping(Bin(Lit(2.0), TokenType.PLUS, "+", Lit(3.0))),
                TokenType.STAR, "*",
                Lit(4.0));

            interpreter.VisitBinaryExpression(expr).Should().Be(20.0);
        }

        [Fact]
        public void Unary_Minus_NegatesNumber()
        {
            var interpreter = new Interpreter();

            var result = interpreter.VisitUnaryExpression(new Unary(T(TokenType.MINUS, "-"), Lit(5.0)));

            result.Should().Be(-5.0);
        }

        [Fact]
        public void Unary_Minus_OnString_ThrowsRuntimeException()
        {
            var interpreter = new Interpreter();

            Action act = () => interpreter.VisitUnaryExpression(new Unary(T(TokenType.MINUS, "-"), Lit("a")));

            act.Should().Throw<RuntimeException>();
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(null, true)]
        [InlineData(0.0, false)]
        [InlineData("", false)]
        public void Unary_Bang_NegatesTruthiness(object? operand, bool expected)
        {
            var interpreter = new Interpreter();

            var result = interpreter.VisitUnaryExpression(new Unary(T(TokenType.BANG, "!"), Lit(operand)));

            result.Should().Be(expected);
        }

        // ---------- Conditional ----------

        [Fact]
        public void Conditional_TruthyCondition_ReturnsThenBranch()
        {
            var interpreter = new Interpreter();

            var result = interpreter.VisitConditionalExpression(new Conditional(Lit(true), Lit("tak"), Lit("nie")));

            result.Should().Be("tak");
        }

        [Fact]
        public void Conditional_FalsyCondition_ReturnsElseBranch()
        {
            var interpreter = new Interpreter();

            var result = interpreter.VisitConditionalExpression(new Conditional(Lit(null), Lit("tak"), Lit("nie")));

            result.Should().Be("nie");
        }

        // ---------- Logical ----------

        [Fact]
        public void Logical_Or_ReturnsLeftWhenTruthy()
        {
            var interpreter = new Interpreter();
            var expr = new Logical(Lit("lewy"), T(TokenType.OR, "lub"), Lit("prawy"));

            interpreter.VisitLogicalExpression(expr).Should().Be("lewy");
        }

        [Fact]
        public void Logical_Or_ReturnsRightWhenLeftFalsy()
        {
            var interpreter = new Interpreter();
            var expr = new Logical(Lit(false), T(TokenType.OR, "lub"), Lit("prawy"));

            interpreter.VisitLogicalExpression(expr).Should().Be("prawy");
        }

        [Fact]
        public void Logical_And_ReturnsLeftWhenFalsy()
        {
            var interpreter = new Interpreter();
            var expr = new Logical(Lit(null), T(TokenType.AND, "i"), Lit("prawy"));

            interpreter.VisitLogicalExpression(expr).Should().BeNull();
        }

        [Fact]
        public void Logical_And_ReturnsRightWhenLeftTruthy()
        {
            var interpreter = new Interpreter();
            var expr = new Logical(Lit(true), T(TokenType.AND, "i"), Lit("prawy"));

            interpreter.VisitLogicalExpression(expr).Should().Be("prawy");
        }

        [Fact]
        public void Logical_Or_ShortCircuits_DoesNotEvaluateRight()
        {
            var interpreter = new Interpreter();
            var expr = new Logical(Lit(true), T(TokenType.OR, "lub"), Ident("nieistnieje"));

            Action act = () => interpreter.VisitLogicalExpression(expr);

            act.Should().NotThrow();
        }

        [Fact]
        public void Logical_And_ShortCircuits_DoesNotEvaluateRight()
        {
            var interpreter = new Interpreter();
            var expr = new Logical(Lit(false), T(TokenType.AND, "i"), Ident("nieistnieje"));

            Action act = () => interpreter.VisitLogicalExpression(expr);

            act.Should().NotThrow();
        }

        // ---------- Zmienne ----------

        [Fact]
        public void Var_WithInitializer_DefinesVariable()
        {
            var interpreter = new Interpreter();

            interpreter.VisitVarStmt(Declare("x", Lit(10.0)));

            interpreter.VisitVariableExpression((Variable)Ident("x")).Should().Be(10.0);
        }

        [Fact]
        public void Var_WithoutInitializer_DefinesNull()
        {
            var interpreter = new Interpreter();

            interpreter.VisitVarStmt(Declare("x"));

            interpreter.VisitVariableExpression((Variable)Ident("x")).Should().BeNull();
        }

        [Fact]
        public void Variable_Undefined_ThrowsRuntimeException()
        {
            var interpreter = new Interpreter();

            Action act = () => interpreter.VisitVariableExpression((Variable)Ident("nieistnieje"));

            act.Should().Throw<RuntimeException>();
        }

        [Fact]
        public void Assign_UpdatesVariableAndReturnsValue()
        {
            var interpreter = new Interpreter();
            interpreter.VisitVarStmt(Declare("x", Lit(1.0)));

            var result = interpreter.VisitAssignExpression(AssignTo("x", Lit(2.0)));

            result.Should().Be(2.0);
            interpreter.VisitVariableExpression((Variable)Ident("x")).Should().Be(2.0);
        }

        [Fact]
        public void Assign_ToUndefinedVariable_ThrowsRuntimeException()
        {
            var interpreter = new Interpreter();

            Action act = () => interpreter.VisitAssignExpression(AssignTo("nieistnieje", Lit(1.0)));

            act.Should().Throw<RuntimeException>();
        }

        // ---------- Blocks and scope ----------

        [Fact]
        public void Block_VariableDeclaredInside_IsNotVisibleOutside()
        {
            var interpreter = new Interpreter();
            var block = new Block(new List<Stmt> { Declare("wewnatrz", Lit(1.0)) });

            interpreter.VisitBlockStmt(block);

            Action act = () => interpreter.VisitVariableExpression((Variable)Ident("wewnatrz"));
            act.Should().Throw<RuntimeException>();
        }

        [Fact]
        public void Block_CanAssignToOuterVariable()
        {
            var interpreter = new Interpreter();
            interpreter.VisitVarStmt(Declare("x", Lit(1.0)));
            var block = new Block(new List<Stmt>
            {
                new StmtExpression(AssignTo("x", Lit(99.0)))
            });

            interpreter.VisitBlockStmt(block);

            interpreter.VisitVariableExpression((Variable)Ident("x")).Should().Be(99.0);
        }

        [Fact]
        public void Block_Shadowing_DoesNotChangeOuterVariable()
        {
            var interpreter = new Interpreter();
            interpreter.VisitVarStmt(Declare("x", Lit(1.0)));
            var block = new Block(new List<Stmt> { Declare("x", Lit(2.0)) });

            interpreter.VisitBlockStmt(block);

            interpreter.VisitVariableExpression((Variable)Ident("x")).Should().Be(1.0);
        }

        [Fact]
        public void ExecuteBlock_RestoresEnvironment_EvenWhenExceptionThrown()
        {
            var interpreter = new Interpreter();
            var block = new Block(new List<Stmt>
            {
                Declare("tmp", Lit(1.0)),
                new StmtExpression(Ident("nieistnieje")) // rzuci wyjątek
            });

            Action act = () => interpreter.VisitBlockStmt(block);
            act.Should().Throw<RuntimeException>();

            // środowisko wróciło do globalnego, więc "tmp" nie jest widoczne
            Action read = () => interpreter.VisitVariableExpression((Variable)Ident("tmp"));
            read.Should().Throw<RuntimeException>();
        }

        // ---------- Print / Expression statement ----------

        [Theory]
        [InlineData(5.0, "5")]
        [InlineData("tekst", "tekst")]
        public void Print_WritesStringifiedValue(object value, string expected)
        {
            var output = Run(new Print(Lit(value)));

            output.Should().Be(expected + Environment.NewLine);
        }

        [Fact]
        public void Print_Null_WritesNullKeyword()
        {
            var output = Run(new Print(Lit(null)));

            output.Should().Be(Keywords.NULL + Environment.NewLine);
        }

        [Fact]
        public void Print_Booleans_WriteTrueAndFalseKeywords()
        {
            Run(new Print(Lit(true))).Should().Be(Keywords.TRUE + Environment.NewLine);
            Run(new Print(Lit(false))).Should().Be(Keywords.FALSE + Environment.NewLine);
        }

        [Fact]
        public void ExpressionStmt_EvaluatesSideEffects()
        {
            var interpreter = new Interpreter();
            interpreter.VisitVarStmt(Declare("x", Lit(1.0)));

            Run(interpreter, new StmtExpression(AssignTo("x", Lit(5.0))));

            interpreter.VisitVariableExpression((Variable)Ident("x")).Should().Be(5.0);
        }

        // ---------- If ----------

        [Fact]
        public void If_TruthyCondition_ExecutesThenBranch()
        {
            var output = Run(new If(Lit(true), new Print(Lit("then")), new Print(Lit("else"))));

            output.Should().Be("then" + Environment.NewLine);
        }

        [Fact]
        public void If_FalsyCondition_ExecutesElseBranch()
        {
            var output = Run(new If(Lit(false), new Print(Lit("then")), new Print(Lit("else"))));

            output.Should().Be("else" + Environment.NewLine);
        }

        [Fact]
        public void If_FalsyConditionWithoutElse_DoesNothing()
        {
            var output = Run(new If(Lit(null), new Print(Lit("then")), null));

            output.Should().BeEmpty();
        }

        // ---------- While / Break ----------

        [Fact]
        public void While_RunsUntilConditionIsFalse()
        {
            var interpreter = new Interpreter();
            // zmienna i = 0; dopóki (i < 3) { i = i + 1; }
            var loop = new While(
                Bin(Ident("i"), TokenType.LESS, "<", Lit(3.0)),
                new Block(new List<Stmt>
                {
                    new StmtExpression(AssignTo("i", Bin(Ident("i"), TokenType.PLUS, "+", Lit(1.0))))
                }));

            Run(interpreter, Declare("i", Lit(0.0)), loop);

            interpreter.VisitVariableExpression((Variable)Ident("i")).Should().Be(3.0);
        }

        [Fact]
        public void While_FalseCondition_NeverExecutesBody()
        {
            var output = Run(new While(Lit(false), new Print(Lit("nigdy"))));

            output.Should().BeEmpty();
        }

        [Fact]
        public void While_Break_StopsLoop()
        {
            var interpreter = new Interpreter();
            // dopóki (prawda) { i = i + 1; jeśli (i == 5) przerwij; }
            var loop = new While(
                Lit(true),
                new Block(new List<Stmt>
                {
                    new StmtExpression(AssignTo("i", Bin(Ident("i"), TokenType.PLUS, "+", Lit(1.0)))),
                    new If(Bin(Ident("i"), TokenType.EQUAL_EQUAL, "==", Lit(5.0)), new Break(T(TokenType.BREAK, "przerwij")), null)
                }));

            Run(interpreter, Declare("i", Lit(0.0)), loop);

            interpreter.VisitVariableExpression((Variable)Ident("i")).Should().Be(5.0);
        }

        [Fact]
        public void Break_OutsideLoop_ThrowsBreakException()
        {
            var interpreter = new Interpreter();

            Action act = () => interpreter.VisitBreakStmt(new Break(T(TokenType.BREAK, "przerwij")));

            act.Should().Throw<BreakException>();
        }

        // ---------- Functions and calls ----------

        [Fact]
        public void Call_NonCallable_ThrowsRuntimeException()
        {
            var interpreter = new Interpreter();
            var call = new Call(Lit(5.0), T(TokenType.RIGHT_PAREN, ")"), new List<Expr>());

            Action act = () => interpreter.VisitCallExpression(call);

            act.Should().Throw<RuntimeException>();
        }

        [Fact]
        public void Call_BuiltInPrint_WritesToConsole()
        {
            var interpreter = new Interpreter();
            var call = new Call(Ident("wypisz"), T(TokenType.RIGHT_PAREN, ")"), new List<Expr> { Lit("cześć") });

            var output = CaptureOutput(() => interpreter.VisitCallExpression(call));

            output.Should().Contain("cześć");
        }

        [Fact]
        public void Call_BuiltInClock_ReturnsNumber()
        {
            var interpreter = new Interpreter();
            var call = new Call(Ident("zegar"), T(TokenType.RIGHT_PAREN, ")"), new List<Expr>());

            var result = interpreter.VisitCallExpression(call);

            result.Should().BeOfType<double>();
        }

        [Fact]
        public void Call_WrongArgumentCount_ThrowsRuntimeException()
        {
            var interpreter = new Interpreter();
            var function = new Function(
                T(TokenType.IDENTIFIER, "f"),
                new List<Token> { T(TokenType.IDENTIFIER, "a") },
                new List<Stmt>());
            interpreter.VisitFunctionStmt(function);
            var call = new Call(Ident("f"), T(TokenType.RIGHT_PAREN, ")"), new List<Expr>());

            Action act = () => interpreter.VisitCallExpression(call);

            act.Should().Throw<RuntimeException>();
        }

        [Fact]
        public void Function_Declaration_DefinesCallableInEnvironment()
        {
            var interpreter = new Interpreter();
            var function = new Function(T(TokenType.IDENTIFIER, "f"), new List<Token>(), new List<Stmt>());

            interpreter.VisitFunctionStmt(function);

            interpreter.VisitVariableExpression((Variable)Ident("f")).Should().NotBeNull();
        }

        [Fact]
        public void Function_WithReturn_ReturnsValue()
        {
            // funkcja dodaj(a, b) { zwróć a + b; }
            var function = new Function(
                T(TokenType.IDENTIFIER, "dodaj"),
                new List<Token> { T(TokenType.IDENTIFIER, "a"), T(TokenType.IDENTIFIER, "b") },
                new List<Stmt>
                {
                    new Return(T(TokenType.RETURN, "zwróć"), Bin(Ident("a"), TokenType.PLUS, "+", Ident("b")))
                });
            var interpreter = CreateResolved(function);
            interpreter.VisitFunctionStmt(function);
            var call = new Call(
                Ident("dodaj"),
                T(TokenType.RIGHT_PAREN, ")"),
                new List<Expr> { Lit(2.0), Lit(3.0) });

            var result = interpreter.VisitCallExpression(call);

            result.Should().Be(5.0);
        }

        [Fact]
        public void Function_WithoutReturn_ReturnsNull()
        {
            var interpreter = new Interpreter();
            var function = new Function(T(TokenType.IDENTIFIER, "pusta"), new List<Token>(), new List<Stmt>());
            interpreter.VisitFunctionStmt(function);
            var call = new Call(Ident("pusta"), T(TokenType.RIGHT_PAREN, ")"), new List<Expr>());

            interpreter.VisitCallExpression(call).Should().BeNull();
        }

        [Fact]
        public void Function_Recursion_ComputesFactorial()
        {
            var paren = T(TokenType.RIGHT_PAREN, ")");
            // funkcja silnia(n) { jeśli (n <= 1) zwróć 1; zwróć n * silnia(n - 1); }
            var function = new Function(
                T(TokenType.IDENTIFIER, "silnia"),
                new List<Token> { T(TokenType.IDENTIFIER, "n") },
                new List<Stmt>
                {
                    new If(
                        Bin(Ident("n"), TokenType.LESS_EQUAL, "<=", Lit(1.0)),
                        new Return(T(TokenType.RETURN, "zwróć"), Lit(1.0)),
                        null),
                    new Return(
                        T(TokenType.RETURN, "zwróć"),
                        Bin(
                            Ident("n"), TokenType.STAR, "*",
                            new Call(
                                Ident("silnia"), paren,
                                new List<Expr> { Bin(Ident("n"), TokenType.MINUS, "-", Lit(1.0)) })))
                });
            var interpreter = CreateResolved(function);
            interpreter.VisitFunctionStmt(function);
            var call = new Call(Ident("silnia"), paren, new List<Expr> { Lit(5.0) });

            interpreter.VisitCallExpression(call).Should().Be(120.0);
        }

        [Fact]
        public void Function_Closure_CapturesEnclosingVariable()
        {
            var interpreter = new Interpreter();
            var paren = T(TokenType.RIGHT_PAREN, ")");
            interpreter.VisitVarStmt(Declare("x", Lit(10.0)));
            // funkcja pobierz() { zwróć x; }
            var function = new Function(
                T(TokenType.IDENTIFIER, "pobierz"),
                new List<Token>(),
                new List<Stmt> { new Return(T(TokenType.RETURN, "zwróć"), Ident("x")) });
            interpreter.VisitFunctionStmt(function);
            interpreter.VisitAssignExpression(AssignTo("x", Lit(42.0)));

            var result = interpreter.VisitCallExpression(new Call(Ident("pobierz"), paren, new List<Expr>()));

            result.Should().Be(42.0);
        }

        [Fact]
        public void Return_OutsideFunction_ThrowsReturnException()
        {
            var interpreter = new Interpreter();
            var stmt = new Return(T(TokenType.RETURN, "zwróć"), Lit(1.0));

            Action act = () => interpreter.VisitReturnStmt(stmt);

            act.Should().Throw<ReturnException>();
        }

        [Fact]
        public void AnonFunction_CanBeAssignedAndCalled()
        {
            var paren = T(TokenType.RIGHT_PAREN, ")");
            // zmienna podwoj = funkcja(a) { zwróć a * 2; };
            var anon = new AnonFunction(
                new List<Token> { T(TokenType.IDENTIFIER, "a") },
                new List<Stmt>
                {
                    new Return(T(TokenType.RETURN, "zwróć"), Bin(Ident("a"), TokenType.STAR, "*", Lit(2.0)))
                });
            var declaration = Declare("podwoj", anon);
            var interpreter = CreateResolved(declaration);
            interpreter.VisitVarStmt(declaration);

            var result = interpreter.VisitCallExpression(
                new Call(Ident("podwoj"), paren, new List<Expr> { Lit(21.0) }));

            result.Should().Be(42.0);
        }

        // ---------- Resolver + Interpreter (zakres leksykalny) ----------

        [Fact]
        public void Closure_BindsToDeclarationVisibleAtDefinition_NotLaterShadowing()
        {
            var paren = T(TokenType.RIGHT_PAREN, ")");
            Stmt CallShowA() => new StmtExpression(new Call(Ident("showA"), paren, new List<Expr>()));
            // zmienna a = "global";
            // { funkcja showA() { wypisz a; } showA(); zmienna a = "blok"; showA(); }
            var program = new Stmt[]
            {
                Declare("a", Lit("global")),
                new Block(new List<Stmt>
                {
                    new Function(
                        T(TokenType.IDENTIFIER, "showA"),
                        new List<Token>(),
                        new List<Stmt> { new Print(Ident("a")) }),
                    CallShowA(),
                    Declare("a", Lit("blok")),
                    CallShowA()
                })
            };

            var output = Run(program);

            output.Should().Be("global" + Environment.NewLine + "global" + Environment.NewLine);
        }

        [Fact]
        public void Closure_KeepsLocalStateBetweenCalls()
        {
            var paren = T(TokenType.RIGHT_PAREN, ")");
            // funkcja stworzLicznik() { zmienna i = 0; funkcja licz() { i = i + 1; wypisz i; } zwróć licz; }
            // zmienna c = stworzLicznik(); c(); c();
            var program = new Stmt[]
            {
                new Function(
                    T(TokenType.IDENTIFIER, "stworzLicznik"),
                    new List<Token>(),
                    new List<Stmt>
                    {
                        Declare("i", Lit(0.0)),
                        new Function(
                            T(TokenType.IDENTIFIER, "licz"),
                            new List<Token>(),
                            new List<Stmt>
                            {
                                new StmtExpression(AssignTo("i", Bin(Ident("i"), TokenType.PLUS, "+", Lit(1.0)))),
                                new Print(Ident("i"))
                            }),
                        new Return(T(TokenType.RETURN, "zwróć"), Ident("licz"))
                    }),
                Declare("c", new Call(Ident("stworzLicznik"), paren, new List<Expr>())),
                new StmtExpression(new Call(Ident("c"), paren, new List<Expr>())),
                new StmtExpression(new Call(Ident("c"), paren, new List<Expr>()))
            };

            var output = Run(program);

            output.Should().Be("1" + Environment.NewLine + "2" + Environment.NewLine);
        }

        // ---------- Interpret error handling ----------

        [Fact]
        public void Interpret_RuntimeException_IsHandledAndDoesNotPropagate()
        {
            var interpreter = new Interpreter();
            var statements = new List<Stmt>
            {
                new StmtExpression(Bin(Lit(1.0), TokenType.SLASH, "/", Lit(0.0)))
            };

            Action act = () => CaptureOutput(() => interpreter.Interpret(statements));

            act.Should().NotThrow();
        }

        [Fact]
        public void Interpret_StopsExecutionAfterRuntimeError()
        {
            var interpreter = new Interpreter();
            var statements = new List<Stmt>
            {
                new StmtExpression(Bin(Lit(1.0), TokenType.SLASH, "/", Lit(0.0))),
                new Print(Lit("nie powinno się wypisać"))
            };

            var output = CaptureOutput(() => interpreter.Interpret(statements));

            output.Should().NotContain("nie powinno się wypisać");
        }

        [Fact]
        public void Interpret_ExecutesStatementsInOrder()
        {
            var output = Run(new Print(Lit("a")), new Print(Lit("b")), new Print(Lit("c")));

            output.Should().Be("a" + Environment.NewLine + "b" + Environment.NewLine + "c" + Environment.NewLine);
        }

        [Fact]
        public void Globals_ContainBuiltInFunctions()
        {
            var interpreter = new Interpreter();

            foreach (var name in new[] { "zegar", "wypisz", "wprowadź" })
            {
                interpreter.VisitVariableExpression((Variable)Ident(name)).Should().NotBeNull();
            }
        }

        // =====================================================================
        // Testy oparte na kodzie źródłowym (Scanner -> Parser -> Resolver -> Interpreter)
        // =====================================================================

        // W źródle "@init" jest zamieniane na nazwę konstruktora (FunctionConstants.CONSTRUCTOR).
        private static List<Stmt> ParseSource(string source, bool promptMode = false)
            => new Parser(
                new Scanner(source.Replace("@init", FunctionConstants.CONSTRUCTOR)).ScanTokens(),
                promptMode).Parse();

        // Wykonuje program bez Interpret(), więc RuntimeException nie jest połykany i można go sprawdzać.
        // Wyniki odczytujemy ze zmiennych globalnych (domyślnie "wynik").
        private static Interpreter ExecuteSource(string source)
        {
            var statements = ParseSource(source);
            var interpreter = new Interpreter();
            new Resolver(interpreter).Resolve(statements);
            foreach (var statement in statements)
            {
                statement.Accept(interpreter);
            }

            return interpreter;
        }

        private static object? Global(Interpreter interpreter, string name)
            => interpreter.VisitVariableExpression(Ident(name));

        private static object? Eval(string source, string variable = "wynik")
            => Global(ExecuteSource(source), variable);

        private static List<object?> AsList(object? value)
            => value.Should().BeOfType<List<object?>>().Subject;

        private static void AssertRuntimeError(string source, string? messageFragment = null)
        {
            Action act = () => ExecuteSource(source);

            var assertion = act.Should().Throw<RuntimeException>();
            if (messageFragment != null)
            {
                assertion.Where(e => e.Message.Contains(messageFragment));
            }
        }

        private static string RunPrompt(Interpreter interpreter, string line)
            => CaptureOutput(() =>
            {
                var statements = ParseSource(line, promptMode: true);
                new Resolver(interpreter).Resolve(statements);
                interpreter.Interpret(statements);
            });

        // ---------- Arytmetyka: %, ** ----------

        [Theory]
        [InlineData("10 % 3", 1.0)]
        [InlineData("9 % 3", 0.0)]
        [InlineData("5.5 % 2", 1.5)]
        [InlineData("7 % 4 * 2", 6.0)]
        [InlineData("2 ** 10", 1024.0)]
        [InlineData("2 ** 0", 1.0)]
        [InlineData("2 ** -1", 0.5)]
        [InlineData("9 ** 0.5", 3.0)]
        [InlineData("2 ** 3 ** 2", 512.0)]          // prawostronna łączność: 2 ** 9
        [InlineData("-2 ** 2", -4.0)]               // minus jednoargumentowy działa po potęgowaniu
        [InlineData("(-2) ** 2", 4.0)]
        [InlineData("10 - 2 * 3 ** 2", -8.0)]
        public void Arithmetic_ModuloAndPower_EvaluatesToExpectedNumber(string expression, double expected)
        {
            Eval($"zmienna wynik = {expression};").Should().Be(expected);
        }

        [Fact]
        public void Modulo_NegativeDividend_KeepsSignOfDividend()
        {
            // Dokumentuje obecne zachowanie (reszta jak w C#), a nie modulo matematyczne.
            Eval("zmienna wynik = -7 % 3;").Should().Be(-1.0);
        }

        [Theory]
        [InlineData("\"a\" % 2")]
        [InlineData("2 ** \"a\"")]
        [InlineData("1 <= \"a\"")]
        [InlineData("\"a\" >= 1")]
        [InlineData("prawda * 2")]
        [InlineData("wartość_pusta - 1")]
        [InlineData("1 + prawda")]
        public void Arithmetic_NonNumberOperands_ThrowRuntimeErrorWithMessage(string expression)
        {
            AssertRuntimeError($"zmienna wynik = {expression};", "Operandy muszą być liczbami.");
        }

        [Theory]
        [InlineData("-\"a\"")]
        [InlineData("-prawda")]
        [InlineData("-wartość_pusta")]
        public void Unary_Minus_OnNonNumber_ThrowsRuntimeErrorWithMessage(string expression)
        {
            AssertRuntimeError($"zmienna wynik = {expression};", "Operand musi być liczbą.");
        }

        [Theory]
        [InlineData("1 / 0")]
        [InlineData("0 / 0")]
        [InlineData("5 / (3 - 3)")]
        [InlineData("5 / -0")]
        public void Division_ByZero_ThrowsRuntimeErrorWithMessage(string expression)
        {
            AssertRuntimeError($"zmienna wynik = {expression};", "dziel przez 0");
        }

        // ---------- Operator & ----------

        [Theory]
        [InlineData("1 & 2", "12")]
        [InlineData("\"a\" & 1 & 2", "a12")]
        [InlineData("\"a\" & (1 + 2)", "a3")]
        [InlineData("\"\" & 10", "10")]
        [InlineData("1 & \"a\"", "1a")]
        public void Ampersand_ConcatenatesStringifiedOperands(string expression, string expected)
        {
            Eval($"zmienna wynik = {expression};").Should().Be(expected);
        }

        [Fact]
        public void Ampersand_WithBooleans_UsesTrueAndFalseKeywords()
        {
            Eval("zmienna wynik = \"x\" & prawda & fałsz;").Should().Be("x" + Keywords.TRUE + Keywords.FALSE);
        }

        // ---------- Równość i prawdziwość ----------

        [Theory]
        [InlineData("1 == 1", true)]
        [InlineData("\"a\" == \"a\"", true)]
        [InlineData("\"a\" == \"A\"", false)]
        [InlineData("prawda == prawda", true)]
        [InlineData("prawda == fałsz", false)]
        [InlineData("wartość_pusta == wartość_pusta", true)]
        [InlineData("wartość_pusta != 0", true)]
        [InlineData("0 == wartość_pusta", false)]
        [InlineData("1 == prawda", false)]
        [InlineData("0 == fałsz", false)]
        [InlineData("1 != \"1\"", true)]
        public void Equality_ComparesValuesAndTypes(string expression, bool expected)
        {
            Eval($"zmienna wynik = {expression};").Should().Be(expected);
        }

        [Fact]
        public void Equality_OnLists_ComparesByReference()
        {
            // Dokumentuje obecne zachowanie: listy nie są porównywane element po elemencie.
            var interpreter = ExecuteSource(@"
zmienna a = [1];
zmienna b = a;
zmienna c = [1];
zmienna sama = a == b;
zmienna rozne = a == c;");

            Global(interpreter, "sama").Should().Be(true);
            Global(interpreter, "rozne").Should().Be(false);
        }

        [Theory]
        [InlineData("0", true)]
        [InlineData("\"\"", true)]
        [InlineData("[]", true)]
        [InlineData("prawda", true)]
        [InlineData("wartość_pusta", false)]
        [InlineData("fałsz", false)]
        public void If_Truthiness_OnlyNullAndFalseAreFalsy(string condition, bool expectedThen)
        {
            var source = "zmienna wynik = fałsz; jeżeli (" + condition + ") wynik = prawda;";

            Eval(source).Should().Be(expectedThen);
        }

        // ---------- Operatory logiczne i warunkowy ----------

        [Fact]
        public void Logical_ReturnsOperandValuesNotBooleans()
        {
            var interpreter = ExecuteSource(@"
zmienna a = wartość_pusta lub ""x"";
zmienna b = 0 oraz 5;
zmienna c = ""l"" lub ""p"";
zmienna d = wartość_pusta oraz ""p"";");

            Global(interpreter, "a").Should().Be("x");
            Global(interpreter, "b").Should().Be(5.0);
            Global(interpreter, "c").Should().Be("l");
            Global(interpreter, "d").Should().BeNull();
        }

        [Fact]
        public void Logical_ShortCircuit_SkipsRightSideCall()
        {
            var interpreter = ExecuteSource(@"
zmienna licznik = 0;
funkcja boom() { licznik += 1; zwróć prawda; }
zmienna a = fałsz oraz boom();
zmienna b = prawda lub boom();
zmienna wynik = licznik;");

            Global(interpreter, "wynik").Should().Be(0.0);
        }

        [Fact]
        public void Logical_WithoutShortCircuit_EvaluatesRightSideCall()
        {
            var interpreter = ExecuteSource(@"
zmienna licznik = 0;
funkcja boom() { licznik += 1; zwróć prawda; }
zmienna a = prawda oraz boom();
zmienna b = fałsz lub boom();
zmienna wynik = licznik;");

            Global(interpreter, "wynik").Should().Be(2.0);
        }

        [Fact]
        public void Ternary_EvaluatesOnlyChosenBranch()
        {
            var interpreter = ExecuteSource(@"
zmienna a = prawda ? 1 : 1 / 0;
zmienna b = fałsz ? 1 / 0 : 2;");

            Global(interpreter, "a").Should().Be(1.0);
            Global(interpreter, "b").Should().Be(2.0);
        }

        [Fact]
        public void Ternary_Nested_PicksMatchingBranch()
        {
            Eval("zmienna x = 2; zmienna wynik = x == 1 ? \"a\" : x == 2 ? \"b\" : \"c\";").Should().Be("b");
        }

        // ---------- Przypisania złożone ----------

        [Theory]
        [InlineData("+=", 14.0)]
        [InlineData("-=", 6.0)]
        [InlineData("*=", 40.0)]
        [InlineData("/=", 2.5)]
        [InlineData("%=", 2.0)]
        public void CompoundAssignment_AppliesOperatorToVariable(string op, double expected)
        {
            Eval($"zmienna x = 10; x {op} 4; zmienna wynik = x;").Should().Be(expected);
        }

        [Fact]
        public void CompoundAssignment_Ampersand_ConcatenatesStringifiedValues()
        {
            var interpreter = ExecuteSource(@"
zmienna s = ""a"";
s &= ""b"";
s &= 1;
zmienna n = 5;
n &= ""x"";");

            Global(interpreter, "s").Should().Be("ab1");
            Global(interpreter, "n").Should().Be("5x");
        }

        [Fact]
        public void CompoundAssignment_ReturnsNewValue()
        {
            var interpreter = ExecuteSource("zmienna x = 1; zmienna wynik = x += 2;");

            Global(interpreter, "wynik").Should().Be(3.0);
            Global(interpreter, "x").Should().Be(3.0);
        }

        [Theory]
        [InlineData("zmienna x = \"a\"; x += 1;")]
        [InlineData("zmienna x = 1; x -= \"b\";")]
        [InlineData("zmienna x = prawda; x *= 2;")]
        [InlineData("zmienna x = 1; x %= wartość_pusta;")]
        public void CompoundAssignment_NonNumberOperands_ThrowsRuntimeError(string source)
        {
            AssertRuntimeError(source, "Operandy muszą być liczbami.");
        }

        [Fact]
        public void CompoundAssignment_ToUndefinedVariable_ThrowsRuntimeError()
        {
            AssertRuntimeError("nieistnieje += 1;");
        }

        [Fact]
        public void CompoundAssignment_OnLocalVariable_UpdatesLocal()
        {
            Eval(@"
funkcja f() { zmienna i = 1; i += 4; zwróć i; }
zmienna wynik = f();").Should().Be(5.0);
        }

        [Fact]
        public void CompoundAssignment_InBlockWithShadowing_DoesNotTouchOuterVariable()
        {
            Eval(@"
zmienna x = 1;
{ zmienna x = 10; x += 5; }
zmienna wynik = x;").Should().Be(1.0);
        }

        [Fact]
        public void CompoundAssignment_InClosure_KeepsIndependentStatePerClosure()
        {
            Eval(@"
funkcja stworz() {
    zmienna i = 0;
    funkcja licz() { i += 1; zwróć i; }
    zwróć licz;
}
zmienna a = stworz();
zmienna b = stworz();
a();
a();
zmienna wynik = a() & "","" & b();").Should().Be("3,1");
        }

        [Fact]
        public void CompoundAssignment_InForLoop_AccumulatesSum()
        {
            Eval(@"
zmienna suma = 0;
dla (zmienna i = 1; i <= 4; i += 1) suma += i;
zmienna wynik = suma;").Should().Be(10.0);
        }

        // ---------- Pętle ----------

        [Fact]
        public void For_LoopVariable_IsNotVisibleAfterLoop()
        {
            AssertRuntimeError("dla (zmienna i = 0; i < 1; i += 1) { } zmienna wynik = i;");
        }

        [Fact]
        public void For_WithoutClauses_RunsUntilBreak()
        {
            Eval(@"
zmienna n = 0;
dla (;;) { n += 1; jeżeli (n == 5) przerwij; }
zmienna wynik = n;").Should().Be(5.0);
        }

        [Fact]
        public void Break_InNestedLoops_StopsOnlyInnerLoop()
        {
            Eval(@"
zmienna suma = 0;
dla (zmienna i = 0; i < 3; i += 1) {
    dla (zmienna j = 0; j < 10; j += 1) {
        jeżeli (j == 2) przerwij;
        suma += 1;
    }
}
zmienna wynik = suma;").Should().Be(6.0);
        }

        [Fact]
        public void While_WithBreakInsideIf_StopsLoop()
        {
            Eval(@"
zmienna i = 0;
podczas_gdy (prawda) {
    i += 1;
    jeżeli (i >= 3) przerwij;
}
zmienna wynik = i;").Should().Be(3.0);
        }

        // ---------- Funkcje, funkcje anonimowe i domknięcia ----------

        [Fact]
        public void Function_Recursion_ComputesFibonacci()
        {
            Eval(@"
funkcja fib(n) { jeżeli (n < 2) zwróć n; zwróć fib(n - 1) + fib(n - 2); }
zmienna wynik = fib(10);").Should().Be(55.0);
        }

        [Fact]
        public void Function_LocalFunction_CanRecurse()
        {
            Eval(@"
zmienna wynik;
{
    funkcja suma(n) { jeżeli (n == 0) zwróć 0; zwróć n + suma(n - 1); }
    wynik = suma(4);
}").Should().Be(10.0);
        }

        [Fact]
        public void Function_ReturnInsideLoop_ExitsFunctionImmediately()
        {
            Eval(@"
funkcja pierwsza(l) {
    dla (zmienna i = 0; i < 5; i += 1) {
        jeżeli (l[i] > 3) zwróć l[i];
    }
    zwróć wartość_pusta;
}
zmienna wynik = pierwsza([1, 5, 2, 7, 9]);").Should().Be(5.0);
        }

        [Fact]
        public void Function_ReturnWithoutValue_ReturnsNull()
        {
            Eval("funkcja f() { zwróć; } zmienna wynik = f();").Should().BeNull();
        }

        [Fact]
        public void Function_Parameter_ShadowsGlobalWithoutChangingIt()
        {
            Eval(@"
zmienna x = 1;
funkcja f(x) { x = 5; zwróć x; }
f(2);
zmienna wynik = x;").Should().Be(1.0);
        }

        [Fact]
        public void Function_GlobalDefinedAfterFunction_IsResolvedAtCallTime()
        {
            Eval(@"
funkcja f() { zwróć pozniej; }
zmienna pozniej = 5;
zmienna wynik = f();").Should().Be(5.0);
        }

        [Fact]
        public void Call_ArgumentsAreEvaluatedLeftToRight()
        {
            Eval(@"
zmienna n = 0;
funkcja nast() { n += 1; zwróć n; }
funkcja sklej(a, b) { zwróć a & b; }
zmienna wynik = sklej(nast(), nast());").Should().Be("12");
        }

        [Fact]
        public void Call_ReturnedFunction_CanBeCalledImmediately()
        {
            Eval(@"
funkcja mnoznik(a) { zwróć funkcja(b) { zwróć a * b; }; }
zmienna wynik = mnoznik(3)(4);").Should().Be(12.0);
        }

        [Fact]
        public void Call_FunctionPassedAsArgument_IsInvoked()
        {
            Eval(@"
funkcja zastosuj(f, x) { zwróć f(x); }
funkcja podwoj(a) { zwróć a * 2; }
zmienna wynik = zastosuj(podwoj, 21);").Should().Be(42.0);
        }

        [Fact]
        public void AnonFunction_ImmediatelyInvoked_ReturnsValue()
        {
            Eval("zmienna wynik = funkcja() { zwróć 7; }();").Should().Be(7.0);
        }

        [Fact]
        public void AnonFunction_CapturesVariableFromEnclosingFunction()
        {
            Eval(@"
funkcja dodajacy(n) { zwróć funkcja(x) { zwróć x + n; }; }
zmienna dodaj3 = dodajacy(3);
zmienna wynik = dodaj3(4);").Should().Be(7.0);
        }

        [Fact]
        public void AnonFunction_AsCallback_IsInvoked()
        {
            Eval(@"
funkcja zastosuj(f, x) { zwróć f(x); }
zmienna wynik = zastosuj(funkcja(a) { zwróć a & ""!""; }, ""hej"");").Should().Be("hej!");
        }

        [Fact]
        public void Call_WrongArgumentCount_ReportsExpectedAndActualCount()
        {
            AssertRuntimeError("funkcja f(a) { } f();", "Oczekiwano 1 argumentów, ale otrzymano 0.");
            AssertRuntimeError("funkcja f(a) { } f(1, 2);", "Oczekiwano 1 argumentów, ale otrzymano 2.");
        }

        [Fact]
        public void Call_WrongArgumentCountForAnonFunction_ThrowsRuntimeError()
        {
            AssertRuntimeError("zmienna f = funkcja(a) { zwróć a; }; f();");
        }

        [Theory]
        [InlineData("zmienna x = 5; x();")]
        [InlineData("\"tekst\"();")]
        [InlineData("wartość_pusta();")]
        [InlineData("zmienna l = [1]; l();")]
        public void Call_NonCallable_ThrowsRuntimeErrorWithMessage(string source)
        {
            AssertRuntimeError(source, "Można tylko wywoływać funkcje oraz klasy.");
        }

        // ---------- Listy ----------

        [Fact]
        public void List_Literal_EvaluatesItemsInOrder()
        {
            var list = AsList(Eval("zmienna wynik = [1 + 1, \"a\" & 1, prawda, wartość_pusta];"));

            list.Should().Equal(2.0, "a1", true, null);
        }

        [Fact]
        public void List_EmptyLiteral_IsEmptyList()
        {
            AsList(Eval("zmienna wynik = [];")).Should().BeEmpty();
        }

        [Fact]
        public void List_NestedLiteral_ProducesNestedLists()
        {
            var outer = AsList(Eval("zmienna wynik = [[1], [], [2, 3]];"));

            outer.Should().HaveCount(3);
            AsList(outer[0]).Should().Equal(1.0);
            AsList(outer[1]).Should().BeEmpty();
            AsList(outer[2]).Should().Equal(2.0, 3.0);
        }

        [Fact]
        public void List_LiteralInFunction_CreatesNewListOnEachCall()
        {
            var interpreter = ExecuteSource(@"
funkcja nowa() { zwróć [1]; }
zmienna a = nowa();
zmienna b = nowa();
a[0] = 9;
zmienna wynik = b[0];");

            Global(interpreter, "wynik").Should().Be(1.0);
        }

        [Theory]
        [InlineData("a[0]", 10.0)]
        [InlineData("a[2]", 30.0)]
        [InlineData("a[1 + 1]", 30.0)]
        [InlineData("a[a[0] / 10]", 20.0)]
        [InlineData("a[0] + a[1]", 30.0)]
        [InlineData("-a[0]", -10.0)]
        [InlineData("a[1] ** 2", 400.0)]
        public void ListIndex_Read_ReturnsExpectedElement(string expression, double expected)
        {
            Eval($"zmienna a = [10, 20, 30]; zmienna wynik = {expression};").Should().Be(expected);
        }

        [Fact]
        public void ListIndex_WithVariableIndex_ReadsElement()
        {
            Eval("zmienna a = [10, 20, 30]; zmienna i = 1; zmienna wynik = a[i];").Should().Be(20.0);
        }

        [Fact]
        public void ListIndex_NestedList_CanBeReadThroughIntermediateVariable()
        {
            Eval(@"
zmienna m = [[1, 2], [3, 4]];
zmienna wiersz = m[1];
zmienna wynik = wiersz[0];").Should().Be(3.0);
        }

        [Fact]
        public void ListIndex_InsideFunction_UsesLocalList()
        {
            Eval(@"
funkcja f() { zmienna a = [1, 2]; zwróć a[1]; }
zmienna wynik = f();").Should().Be(2.0);
        }

        [Fact]
        public void ListIndex_PropertyCallOnElement_CallsMethodOfInstance()
        {
            Eval(@"
klasa P { @init(n) { ja.n = n; } metoda daj() { zwróć ja.n; } }
zmienna l = [P(1), P(2)];
zmienna wynik = l[1].daj();").Should().Be(2.0);
        }

        [Theory]
        [InlineData("a[3]")]
        [InlineData("a[100]")]
        public void ListIndex_ReadOutOfRange_ThrowsRuntimeErrorWithMessage(string expression)
        {
            AssertRuntimeError($"zmienna a = [1, 2, 3]; zmienna wynik = {expression};", "poza zakresem");
        }

        [Fact]
        public void ListIndex_ReadFromEmptyList_ThrowsRuntimeError()
        {
            AssertRuntimeError("zmienna a = []; zmienna wynik = a[0];", "poza zakresem");
        }

        [Theory]
        [InlineData("zmienna x = 5; zmienna wynik = x[0];")]
        [InlineData("zmienna x = \"tekst\"; zmienna wynik = x[0];")]
        [InlineData("zmienna x = wartość_pusta; zmienna wynik = x[0];")]
        [InlineData("zmienna x = 5; x[0] = 1;")]
        public void ListIndex_OnNonList_ThrowsRuntimeErrorWithMessage(string source)
        {
            AssertRuntimeError(source, "musi być listą");
        }

        [Fact]
        public void ListIndexAssign_ReplacesElementAndReturnsAssignedValue()
        {
            var interpreter = ExecuteSource(@"
zmienna a = [10, 20, 30];
zmienna wynik = a[1] = 99;");

            Global(interpreter, "wynik").Should().Be(99.0);
            AsList(Global(interpreter, "a")).Should().Equal(10.0, 99.0, 30.0);
        }

        [Fact]
        public void ListIndexAssign_Chained_AssignsSameValueToBothElements()
        {
            var interpreter = ExecuteSource(@"
zmienna a = [1, 2, 3];
a[0] = a[1] = 7;");

            AsList(Global(interpreter, "a")).Should().Equal(7.0, 7.0, 3.0);
        }

        [Fact]
        public void ListIndexAssign_CanCopyBetweenElements()
        {
            var interpreter = ExecuteSource(@"
zmienna a = [1, 2, 3];
a[0] = a[2];");

            AsList(Global(interpreter, "a")).Should().Equal(3.0, 2.0, 3.0);
        }

        [Fact]
        public void ListIndexAssign_ThroughAlias_IsVisibleInOriginalList()
        {
            Eval(@"
zmienna a = [1, 2];
zmienna b = a;
b[0] = 9;
zmienna wynik = a[0];").Should().Be(9.0);
        }

        [Fact]
        public void ListIndexAssign_InsideFunction_MutatesListPassedAsArgument()
        {
            Eval(@"
funkcja ustaw(l) { l[0] = 5; }
zmienna a = [1, 2];
ustaw(a);
zmienna wynik = a[0];").Should().Be(5.0);
        }

        [Theory]
        [InlineData("a[3] = 1;")]
        [InlineData("a[100] = 1;")]
        public void ListIndexAssign_OutOfRange_ThrowsRuntimeErrorWithMessage(string statement)
        {
            AssertRuntimeError("zmienna a = [1, 2, 3]; " + statement, "poza zakresem");
        }

        [Fact]
        public void ListIndex_InForLoop_ReadsAndWritesElements()
        {
            var interpreter = ExecuteSource(@"
zmienna a = [1, 2, 3, 4];
zmienna suma = 0;
dla (zmienna i = 0; i < 4; i += 1) {
    suma += a[i];
    a[i] = a[i] * a[i];
}");

            Global(interpreter, "suma").Should().Be(10.0);
            AsList(Global(interpreter, "a")).Should().Equal(1.0, 4.0, 9.0, 16.0);
        }

        // ---------- Klasy: instancje, pola, metody ----------

        [Fact]
        public void Class_Constructor_InitializesFieldsAndMethodsReadThem()
        {
            Eval(@"
klasa Licznik {
    @init(start) { ja.n = start; }
    metoda zwieksz() { ja.n = ja.n + 1; zwróć ja; }
    metoda wartosc() { zwróć ja.n; }
}
zmienna l = Licznik(5);
l.zwieksz();
zmienna wynik = l.wartosc();").Should().Be(6.0);
        }

        [Fact]
        public void Class_MethodReturningSelf_AllowsChaining()
        {
            Eval(@"
klasa Licznik {
    @init() { ja.n = 0; }
    metoda zwieksz() { ja.n = ja.n + 1; zwróć ja; }
}
zmienna wynik = Licznik().zwieksz().zwieksz().zwieksz().n;").Should().Be(3.0);
        }

        [Fact]
        public void Class_Instances_HaveIndependentState()
        {
            var interpreter = ExecuteSource(@"
klasa A { @init(x) { ja.x = x; } }
zmienna a = A(1);
zmienna b = A(2);
a.x = 10;
zmienna ax = a.x;
zmienna bx = b.x;");

            Global(interpreter, "ax").Should().Be(10.0);
            Global(interpreter, "bx").Should().Be(2.0);
        }

        [Fact]
        public void Class_WithoutConstructor_CanBeInstantiatedAndFieldsAssignedExternally()
        {
            Eval(@"
klasa Puste { }
zmienna p = Puste();
p.x = 5;
zmienna wynik = p.x;").Should().Be(5.0);
        }

        [Fact]
        public void Class_SetExpression_ReturnsAssignedValue()
        {
            Eval(@"
klasa Puste { }
zmienna p = Puste();
zmienna wynik = p.x = 9;").Should().Be(9.0);
        }

        [Fact]
        public void Class_MethodCallsOtherMethodThroughSelf()
        {
            Eval(@"
klasa A {
    metoda podwoj(x) { zwróć x * 2; }
    metoda f() { zwróć ja.podwoj(21); }
}
zmienna wynik = A().f();").Should().Be(42.0);
        }

        [Fact]
        public void Class_MethodTakenAsValue_RemainsBoundToInstance()
        {
            Eval(@"
klasa A { @init(x) { ja.x = x; } metoda daj() { zwróć ja.x; } }
zmienna a = A(7);
zmienna m = a.daj;
zmienna wynik = m();").Should().Be(7.0);
        }

        [Fact]
        public void Class_CanBeAssignedToVariableAndInstantiatedThroughIt()
        {
            Eval(@"
klasa A { metoda f() { zwróć 1; } }
zmienna K = A;
zmienna wynik = K().f();").Should().Be(1.0);
        }

        [Fact]
        public void Class_MethodCanReadVariableFromEnclosingScope()
        {
            Eval(@"
zmienna stala = 100;
klasa A { metoda f() { zwróć stala + 1; } }
zmienna wynik = A().f();").Should().Be(101.0);
        }

        [Fact]
        public void Class_DeclaredInBlock_IsUsableInsideBlock()
        {
            Eval(@"
zmienna wynik;
{
    klasa A { metoda f() { zwróć 1; } }
    wynik = A().f();
}").Should().Be(1.0);
        }

        [Fact]
        public void Class_ReadingUndefinedProperty_ThrowsRuntimeError()
        {
            AssertRuntimeError("klasa A { } zmienna a = A(); zmienna wynik = a.nieistnieje;");
        }

        [Fact]
        public void Class_ConstructorCalledWithWrongArgumentCount_ThrowsRuntimeError()
        {
            AssertRuntimeError("klasa A { @init(x) { ja.x = x; } } A();");
        }

        [Theory]
        [InlineData("zmienna x = 5; zmienna wynik = x.y;")]
        [InlineData("zmienna x = \"tekst\"; zmienna wynik = x.y;")]
        [InlineData("zmienna x = wartość_pusta; zmienna wynik = x.y;")]
        public void Get_OnNonInstance_ThrowsRuntimeErrorWithMessage(string source)
        {
            AssertRuntimeError(source, "Tylko instancje i klasy mają właściwości.");
        }

        [Theory]
        [InlineData("zmienna x = 5; x.y = 1;")]
        [InlineData("zmienna x = [1]; x.y = 1;")]
        [InlineData("klasa A { } A.y = 1;")]
        public void Set_OnNonInstance_ThrowsRuntimeErrorWithMessage(string source)
        {
            AssertRuntimeError(source, "Tylko instancje mają atrybuty.");
        }

        // ---------- Klasy: metody statyczne ----------

        [Fact]
        public void StaticMethod_CalledOnClass_ReturnsValue()
        {
            Eval(@"
klasa Matma { statyczna metoda dodaj(a, b) { zwróć a + b; } }
zmienna wynik = Matma.dodaj(2, 3);").Should().Be(5.0);
        }

        [Fact]
        public void StaticMethod_CanCreateInstanceOfItsClass()
        {
            Eval(@"
klasa A {
    @init(x) { ja.x = x; }
    statyczna metoda nowy() { zwróć A(42); }
}
zmienna wynik = A.nowy().x;").Should().Be(42.0);
        }

        [Theory]
        [InlineData("klasa A { } A.brak();", "nie posiada metody statycznej")]
        [InlineData("klasa A { metoda f() { } } A.f();", "nie posiada metody statycznej")]
        public void StaticMethod_Missing_ThrowsRuntimeErrorWithMessage(string source, string fragment)
        {
            AssertRuntimeError(source, fragment);
        }

        // ---------- Klasy: dziedziczenie i super ----------

        [Fact]
        public void Inheritance_SubclassInheritsMethodsFromSuperclass()
        {
            Eval(@"
klasa A { metoda f() { zwróć ""A""; } }
klasa B dziedziczy_po A { }
zmienna wynik = B().f();").Should().Be("A");
        }

        [Fact]
        public void Inheritance_MethodIsInheritedThroughWholeChain()
        {
            Eval(@"
klasa A { metoda f() { zwróć ""A""; } }
klasa B dziedziczy_po A { }
klasa C dziedziczy_po B { }
zmienna wynik = C().f();").Should().Be("A");
        }

        [Fact]
        public void Inheritance_SubclassMethodOverridesSuperclassMethod()
        {
            Eval(@"
klasa A { metoda f() { zwróć ""A""; } }
klasa B dziedziczy_po A { metoda f() { zwróć ""B""; } }
zmienna wynik = B().f();").Should().Be("B");
        }

        [Fact]
        public void Inheritance_InheritedMethodUsesOverriddenMethodOfSubclass()
        {
            Eval(@"
klasa A {
    metoda opisz() { zwróć ""Jestem "" & ja.nazwa(); }
    metoda nazwa() { zwróć ""A""; }
}
klasa B dziedziczy_po A { metoda nazwa() { zwróć ""B""; } }
zmienna wynik = B().opisz();").Should().Be("Jestem B");
        }

        [Fact]
        public void Super_Call_InvokesSuperclassVersionOfMethod()
        {
            Eval(@"
klasa A { metoda f() { zwróć ""A""; } }
klasa B dziedziczy_po A { metoda f() { zwróć super.f() & ""B""; } }
zmienna wynik = B().f();").Should().Be("AB");
        }

        [Fact]
        public void Super_InMultiLevelHierarchy_BindsToDefiningClassNotInstanceClass()
        {
            Eval(@"
klasa A { metoda f() { zwróć ""A""; } }
klasa B dziedziczy_po A { metoda f() { zwróć super.f() & ""B""; } }
klasa C dziedziczy_po B { metoda f() { zwróć super.f() & ""C""; } }
zmienna wynik = C().f();").Should().Be("ABC");
        }

        [Fact]
        public void Super_Call_FindsMethodDefinedInGrandparent()
        {
            Eval(@"
klasa A { metoda f() { zwróć ""A""; } }
klasa B dziedziczy_po A { }
klasa C dziedziczy_po B { metoda f() { zwróć super.f() & ""C""; } }
zmienna wynik = C().f();").Should().Be("AC");
        }

        [Fact]
        public void Super_Constructor_InitializesFieldsFromSuperclass()
        {
            var interpreter = ExecuteSource(@"
klasa Zwierze {
    @init(imie) { ja.imie = imie; }
    metoda powiedz() { zwróć ja.imie & "" robi dzwiek""; }
}
klasa Pies dziedziczy_po Zwierze {
    @init(imie) { super.@init(imie); ja.rasa = ""kundel""; }
    metoda powiedz() { zwróć super.powiedz() & "" hau""; }
}
zmienna p = Pies(""Burek"");
zmienna wynik = p.powiedz();
zmienna rasa = p.rasa;");

            Global(interpreter, "wynik").Should().Be("Burek robi dzwiek hau");
            Global(interpreter, "rasa").Should().Be("kundel");
        }

        [Fact]
        public void Super_InClassDeclaredInsideFunction_Works()
        {
            Eval(@"
zmienna wynik;
funkcja zbuduj() {
    klasa A { metoda f() { zwróć ""A""; } }
    klasa B dziedziczy_po A { metoda f() { zwróć super.f() & ""B""; } }
    zwróć B();
}
wynik = zbuduj().f();").Should().Be("AB");
        }

        [Fact]
        public void Super_MissingMethodInSuperclass_ThrowsRuntimeErrorWithMessage()
        {
            AssertRuntimeError(@"
klasa A { }
klasa B dziedziczy_po A { metoda f() { zwróć super.g(); } }
B().f();", "nie posiada metody 'g'");
        }

        [Theory]
        [InlineData("zmienna X = 5; klasa A dziedziczy_po X { }")]
        [InlineData("zmienna X = \"tekst\"; klasa A dziedziczy_po X { }")]
        [InlineData("funkcja X() { } klasa A dziedziczy_po X { }")]
        public void Inheritance_SuperclassThatIsNotAClass_ThrowsRuntimeErrorWithMessage(string source)
        {
            AssertRuntimeError(source, "Nadrzędna klasa musi być klasą.");
        }

        [Fact]
        public void Inheritance_UndefinedSuperclass_ThrowsRuntimeError()
        {
            AssertRuntimeError("klasa A dziedziczy_po Nieistnieje { }");
        }

        // ---------- Zakres i domknięcia ----------

        [Fact]
        public void Scope_InnerBlockFunction_SeesOuterLocalVariables()
        {
            Eval(@"
zmienna wynik;
funkcja f() {
    zmienna a = 1;
    {
        zmienna b = 2;
        funkcja g() { zwróć a + b; }
        wynik = g();
    }
}
f();").Should().Be(3.0);
        }

        // ---------- Wbudowane funkcje ----------

        [Fact]
        public void Globals_ContainNumberStringAndTypeFunctions()
        {
            var interpreter = new Interpreter();

            foreach (var name in new[] { "liczba", "napis", "typ" })
            {
                interpreter.VisitVariableExpression(Ident(name)).Should().NotBeNull();
            }
        }

        [Fact]
        public void Call_BuiltInPrint_WritesConcatenatedStringToConsole()
        {
            var output = CaptureOutput(() => ExecuteSource("wypisz(\"a\" & 1);"));

            output.Should().Contain("a1");
        }

        [Fact]
        public void Call_BuiltInPrint_InsideMethod_WritesToConsole()
        {
            var output = CaptureOutput(() => ExecuteSource(@"
klasa A { metoda powiedz() { wypisz(""hej""); } }
A().powiedz();"));

            output.Should().Contain("hej");
        }

        [Fact]
        public void Call_BuiltInPrint_InLoop_WritesLinesInOrder()
        {
            var output = CaptureOutput(() => ExecuteSource(
                "dla (zmienna i = 0; i < 3; i += 1) wypisz(i);"));

            output.Should().Be("012");
        }

        // ---------- Tryb prompt (REPL) ----------

        [Fact]
        public void Prompt_ExpressionWithoutSemicolon_PrintsResult()
        {
            RunPrompt(new Interpreter(), "1 + 2").Should().Be("3" + Environment.NewLine);
        }

        [Fact]
        public void Prompt_ExpressionWithSemicolon_PrintsNothing()
        {
            RunPrompt(new Interpreter(), "1 + 2;").Should().BeEmpty();
        }

        [Fact]
        public void Prompt_PrintsBooleansAndNullWithKeywords()
        {
            var interpreter = new Interpreter();

            RunPrompt(interpreter, "1 < 2").Should().Be(Keywords.TRUE + Environment.NewLine);
            RunPrompt(interpreter, "1 > 2").Should().Be(Keywords.FALSE + Environment.NewLine);
            RunPrompt(interpreter, "wartość_pusta").Should().Be(Keywords.NULL + Environment.NewLine);
        }

        [Fact]
        public void Prompt_KeepsStateBetweenLines()
        {
            var interpreter = new Interpreter();

            RunPrompt(interpreter, "zmienna x = 2;").Should().BeEmpty();
            RunPrompt(interpreter, "funkcja podwoj(a) { zwróć a * 2; }").Should().BeEmpty();

            RunPrompt(interpreter, "podwoj(x) * 10").Should().Be("40" + Environment.NewLine);
        }

        [Fact]
        public void Prompt_ConcatenationOfStrings_PrintsConcatenatedText()
        {
            RunPrompt(new Interpreter(), "\"a\" & \"b\"").Should().Be("ab" + Environment.NewLine);
        }

        [Fact]
        public void CompoundAssignment_ReadsVariableLexically_KnownIssue()
        {
            Eval(@"
zmienna a = 1;
{
    funkcja f() { a += 1; }
    zmienna a = 100;
    f();
}
zmienna wynik = a;").Should().Be(2.0);
        }

        [Fact]
        public void Modulo_ByZero_ThrowsRuntimeError_KnownIssue()
        {
            AssertRuntimeError("zmienna wynik = 5 % 0;", "Pamiętaj cholero, nigdy nie dziel przez 0!");
        }

        [Theory]
        [InlineData("/=")]
        [InlineData("%=")]
        public void CompoundDivisionAndModulo_ByZero_ThrowRuntimeError_KnownIssue(string op)
        {
            AssertRuntimeError($"zmienna x = 5; x {op} 0;", "Pamiętaj cholero, nigdy nie dziel przez 0!");
        }

        [Fact]
        public void ListIndex_ChainedIndexing_ReadsAndWritesNestedElement()
        {
            var interpreter = ExecuteSource(@"
zmienna m = [[1, 2], [3, 4]];
m[1][0] = 9;
zmienna wynik = m[1][0] + m[0][1];");

            Global(interpreter, "wynik").Should().Be(11.0);
        }

        [Fact]
        public void ListIndex_OnFieldAndCallResult_Works()
        {
            Eval(@"
klasa K { @init() { ja.l = [10, 20]; } metoda daj() { zwróć ja.l; } }
zmienna k = K();
k.l[1] = 99;
zmienna wynik = k.daj()[1];").Should().Be(99.0);
        }

        [Fact]
        public void ListIndex_ChainedOnNonList_ThrowsRuntimeErrorWithMessage()
        {
            AssertRuntimeError("zmienna m = [1, 2]; zmienna wynik = m[0][0];", "musi być listą");
        }
    }
}