using FluentAssertions;
using PolSkrypt.Constants;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Interpretation;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing;
using PolSkrypt.Parsing.Expressions;
using PolSkrypt.Parsing.Statements;
using Xunit;

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

            result.Should().Be("x" + KeyWord.NULL);
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

        [Fact]
        public void Var_Redeclaration_OverwritesValue()
        {
            var interpreter = new Interpreter();
            interpreter.VisitVarStmt(Declare("x", Lit(1.0)));
            interpreter.VisitVarStmt(Declare("x", Lit(2.0)));

            interpreter.VisitVariableExpression((Variable)Ident("x")).Should().Be(2.0);
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

            output.Should().Be(KeyWord.NULL + Environment.NewLine);
        }

        [Fact]
        public void Print_Booleans_WriteTrueAndFalseKeywords()
        {
            Run(new Print(Lit(true))).Should().Be(KeyWord.TRUE + Environment.NewLine);
            Run(new Print(Lit(false))).Should().Be(KeyWord.FALSE + Environment.NewLine);
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
    }
}