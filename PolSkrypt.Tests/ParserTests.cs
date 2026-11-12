using FluentAssertions;
using PolSkrypt.Constants;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing;
using PolSkrypt.Parsing.Expressions;
using PolSkrypt.Parsing.Expressions.ListExpressions;
using PolSkrypt.Parsing.Statements;

namespace PolSkrypt.Tests
{
    // Claude: generating new tests for Parser class
    /// <summary>
    /// Tests of the AST produced by the parser for valid sources.
    /// Sources are tokenized with the real Scanner (single-line sources only, so every token has Line = 1).
    /// </summary>
    public class ParserTests
    {
        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private static List<Stmt> Parse(string source, bool promptMode = false)
            => new Parser(new Scanner(source).ScanTokens(), promptMode).Parse();

        private static void AssertParsesTo(string source, params Stmt[] expected)
        {
            var statements = Parse(source);

            statements.Should().HaveCount(expected.Length);
            statements.Should().BeEquivalentTo(expected, o => o.PreferringRuntimeMemberTypes().WithStrictOrdering());
        }

        private static void AssertExpression(string expression, Expr expected)
            => AssertParsesTo(expression + ";", new StmtExpression(expected));

        private static Token Op(TokenType type, string lexeme) => new Token(type, lexeme, null, 1);
        private static Token Id(string name) => new Token(TokenType.IDENTIFIER, name, null, 1);
        private static List<Token> Params(params string[] names) => names.Select(Id).ToList();

        private static Variable V(string name) => new Variable(Id(name));
        private static Literal Num(double value) => new Literal(value);
        private static Literal Str(string value) => new Literal(value);

        private static Binary Bin(Expr left, TokenType type, string lexeme, Expr right)
            => new Binary(left, Op(type, lexeme), right);

        private static Logical Log(Expr left, TokenType type, string lexeme, Expr right)
            => new Logical(left, Op(type, lexeme), right);

        private static Unary Un(TokenType type, string lexeme, Expr operand)
            => new Unary(Op(type, lexeme), operand);

        private static Call Invoke(Expr callee, params Expr[] args)
            => new Call(callee, Op(TokenType.RIGHT_PAREN, ")"), args.ToList());

        private static StmtExpression ExprStmt(Expr expr) => new StmtExpression(expr);
        private static Break Brk() => new Break(Op(TokenType.BREAK, "przerwij"));
        private static Return Ret(Expr? value = null) => new Return(Op(TokenType.RETURN, "zwróć"), value);
        private static Block Blk(params Stmt[] statements) => new Block(statements.ToList());

        private static Function Fn(string name, string[] parameters, params Stmt[] body)
            => new Function(Id(name), Params(parameters), body.ToList());

        private static Class Cls(string name, List<Function>? methods = null, List<Function>? staticMethods = null,
            string? superclass = null)
            => new Class(Id(name), methods ?? new List<Function>(), staticMethods ?? new List<Function>(),
                superclass == null ? null : V(superclass));

        private static Token LSq() => Op(TokenType.LEFT_SQUARE_BRACKET, "[");
        private static PList Lst(params Expr[] items) => new PList(LSq(), items.ToList());

        private static PListIndex IdxOf(Expr target, Expr index) => new PListIndex(LSq(), target, index);
        private static PListIndex Idx(string name, Expr index) => IdxOf(V(name), index);
        private static PListIndexAssign IdxSetOf(Expr target, Expr index, Expr value)
            => new PListIndexAssign(LSq(), target, index, value);
        private static PListIndexAssign IdxSet(string name, Expr index, Expr value)
            => IdxSetOf(V(name), index, value);
        private static Super Sup(string method) => new Super(Op(TokenType.SUPER, "super"), Id(method));

        // ---------------------------------------------------------------
        // Basics and primary expressions
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldReturnEmptyList_ForOnlyEofToken()
        {
            var parser = new Parser(new List<Token> { new Token(TokenType.EOF, string.Empty, null, 1) });

            parser.Parse().Should().BeEmpty();
        }

        [Fact]
        public void Parse_ShouldReturnEmptyList_ForEmptySource()
        {
            Parse(string.Empty).Should().BeEmpty();
        }

        [Fact]
        public void Parse_ShouldParseLiteralKeywords()
        {
            AssertExpression("prawda", new Literal(true));
            AssertExpression("fałsz", new Literal(false));
            AssertExpression("wartość_pusta", new Literal(null));
        }

        [Fact]
        public void Parse_ShouldParseNumberAndStringLiterals()
        {
            AssertExpression("3.14", Num(3.14));
            AssertExpression("\"tekst\"", Str("tekst"));
        }

        [Fact]
        public void Parse_ShouldParseSelfExpression()
        {
            AssertExpression("ja", new Self(Op(TokenType.SELF, "ja")));
        }

        [Fact]
        public void Parse_ShouldParseVariableReference()
        {
            AssertExpression("x", V("x"));
        }

        [Fact]
        public void Parse_ShouldParseSequenceOfStatements()
        {
            AssertParsesTo("zmienna x = 1; x = x + 1; x;",
                new Var(Id("x"), Num(1)),
                ExprStmt(new Assign(Id("x"), Bin(V("x"), TokenType.PLUS, "+", Num(1)))),
                ExprStmt(V("x")));
        }

        // ---------------------------------------------------------------
        // Binary operators: associativity and precedence
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldTreatSubtractionAsLeftAssociative()
        {
            AssertExpression("1 - 2 - 3",
                Bin(Bin(Num(1), TokenType.MINUS, "-", Num(2)), TokenType.MINUS, "-", Num(3)));
        }

        [Fact]
        public void Parse_ShouldTreatDivisionAsLeftAssociative()
        {
            AssertExpression("8 / 4 / 2",
                Bin(Bin(Num(8), TokenType.SLASH, "/", Num(4)), TokenType.SLASH, "/", Num(2)));
        }

        [Fact]
        public void Parse_ShouldHandleModuloAtFactorLevel()
        {
            AssertExpression("10 % 3 * 2",
                Bin(Bin(Num(10), TokenType.PERCENT, "%", Num(3)), TokenType.STAR, "*", Num(2)));
        }

        [Fact]
        public void Parse_ShouldTreatMixedAdditionAndSubtractionLeftToRight()
        {
            AssertExpression("1 - 2 + 3",
                Bin(Bin(Num(1), TokenType.MINUS, "-", Num(2)), TokenType.PLUS, "+", Num(3)));
        }

        [Fact]
        public void Parse_ShouldTreatAmpersandAsLeftAssociativeTermOperator()
        {
            AssertExpression("\"a\" & \"b\" & \"c\"",
                Bin(Bin(Str("a"), TokenType.AMPERSAND, "&", Str("b")), TokenType.AMPERSAND, "&", Str("c")));
        }

        [Fact]
        public void Parse_ShouldBindMultiplicationTighterThanAmpersand()
        {
            AssertExpression("a & b * c",
                Bin(V("a"), TokenType.AMPERSAND, "&", Bin(V("b"), TokenType.STAR, "*", V("c"))));
        }

        [Fact]
        public void Parse_ShouldBindComparisonTighterThanEquality()
        {
            AssertExpression("a < b == c > d",
                Bin(
                    Bin(V("a"), TokenType.LESS, "<", V("b")),
                    TokenType.EQUAL_EQUAL, "==",
                    Bin(V("c"), TokenType.GREATER, ">", V("d"))));
        }

        [Theory]
        [InlineData("<", TokenType.LESS)]
        [InlineData("<=", TokenType.LESS_EQUAL)]
        [InlineData(">", TokenType.GREATER)]
        [InlineData(">=", TokenType.GREATER_EQUAL)]
        [InlineData("==", TokenType.EQUAL_EQUAL)]
        [InlineData("!=", TokenType.BANG_EQUAL)]
        public void Parse_ShouldParseComparisonAndEqualityOperators(string op, TokenType type)
        {
            AssertExpression($"a {op} b", Bin(V("a"), type, op, V("b")));
        }

        [Fact]
        public void Parse_ShouldBindArithmeticTighterThanComparison()
        {
            AssertExpression("a + 1 < b * 2",
                Bin(
                    Bin(V("a"), TokenType.PLUS, "+", Num(1)),
                    TokenType.LESS, "<",
                    Bin(V("b"), TokenType.STAR, "*", Num(2))));
        }

        [Fact]
        public void Parse_ShouldRespectGroupingOverPrecedence()
        {
            AssertExpression("(1 + 2) * 3",
                Bin(new Grouping(Bin(Num(1), TokenType.PLUS, "+", Num(2))), TokenType.STAR, "*", Num(3)));
        }

        [Fact]
        public void Parse_ShouldParseNestedGroupings()
        {
            AssertExpression("((1))", new Grouping(new Grouping(Num(1))));
        }

        // ---------------------------------------------------------------
        // Logical operators
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldBindAndTighterThanOr()
        {
            AssertExpression("a lub b oraz c",
                Log(V("a"), TokenType.OR, "lub", Log(V("b"), TokenType.AND, "oraz", V("c"))));
        }

        [Fact]
        public void Parse_ShouldGroupAndBeforeOrWhenAndComesFirst()
        {
            AssertExpression("a oraz b lub c",
                Log(Log(V("a"), TokenType.AND, "oraz", V("b")), TokenType.OR, "lub", V("c")));
        }

        [Fact]
        public void Parse_ShouldTreatOrAsLeftAssociative()
        {
            AssertExpression("a lub b lub c",
                Log(Log(V("a"), TokenType.OR, "lub", V("b")), TokenType.OR, "lub", V("c")));
        }

        [Fact]
        public void Parse_ShouldBindEqualityTighterThanAnd()
        {
            AssertExpression("a == 1 oraz b != 2",
                Log(
                    Bin(V("a"), TokenType.EQUAL_EQUAL, "==", Num(1)),
                    TokenType.AND, "oraz",
                    Bin(V("b"), TokenType.BANG_EQUAL, "!=", Num(2))));
        }

        // ---------------------------------------------------------------
        // Unary operators and exponentiation
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldParseRepeatedUnaryOperators()
        {
            AssertExpression("!!a", Un(TokenType.BANG, "!", Un(TokenType.BANG, "!", V("a"))));
            AssertExpression("--a", Un(TokenType.MINUS, "-", Un(TokenType.MINUS, "-", V("a"))));
        }

        [Fact]
        public void Parse_ShouldBindUnaryTighterThanEquality()
        {
            AssertExpression("!a == b",
                Bin(Un(TokenType.BANG, "!", V("a")), TokenType.EQUAL_EQUAL, "==", V("b")));
        }

        [Fact]
        public void Parse_ShouldBindUnaryMinusTighterThanBinaryMinus()
        {
            AssertExpression("-a - b",
                Bin(Un(TokenType.MINUS, "-", V("a")), TokenType.MINUS, "-", V("b")));
        }

        [Fact]
        public void Parse_ShouldParseExponentiation()
        {
            AssertExpression("2 ** 3", Bin(Num(2), TokenType.STAR_STAR, "**", Num(3)));
        }

        [Fact]
        public void Parse_ShouldTreatExponentiationAsRightAssociative()
        {
            AssertExpression("2 ** 3 ** 2",
                Bin(Num(2), TokenType.STAR_STAR, "**", Bin(Num(3), TokenType.STAR_STAR, "**", Num(2))));
        }

        [Fact]
        public void Parse_ShouldApplyUnaryMinusAfterExponentiation()
        {
            AssertExpression("-2 ** 2",
                Un(TokenType.MINUS, "-", Bin(Num(2), TokenType.STAR_STAR, "**", Num(2))));
        }

        [Fact]
        public void Parse_ShouldBindExponentiationTighterThanMultiplication()
        {
            AssertExpression("2 * 3 ** 2",
                Bin(Num(2), TokenType.STAR, "*", Bin(Num(3), TokenType.STAR_STAR, "**", Num(2))));
        }

        [Fact]
        public void Parse_ShouldAllowCallAsExponentiationBase()
        {
            AssertExpression("f(1) ** 2",
                Bin(Invoke(V("f"), Num(1)), TokenType.STAR_STAR, "**", Num(2)));
        }

        // ---------------------------------------------------------------
        // Ternary operator
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldTreatTernaryAsRightAssociativeInElseBranch()
        {
            AssertExpression("a ? 1 : b ? 2 : 3",
                new Conditional(V("a"), Num(1), new Conditional(V("b"), Num(2), Num(3))));
        }

        [Fact]
        public void Parse_ShouldAllowTernaryInThenBranch()
        {
            AssertExpression("a ? b ? 1 : 2 : 3",
                new Conditional(V("a"), new Conditional(V("b"), Num(1), Num(2)), Num(3)));
        }

        [Fact]
        public void Parse_ShouldAllowAssignmentInThenBranchOfTernary()
        {
            AssertExpression("a ? x = 1 : 2",
                new Conditional(V("a"), new Assign(Id("x"), Num(1)), Num(2)));
        }

        [Fact]
        public void Parse_ShouldBindTernaryTighterThanAssignment()
        {
            AssertExpression("x = a ? 1 : 2",
                new Assign(Id("x"), new Conditional(V("a"), Num(1), Num(2))));
        }

        [Fact]
        public void Parse_ShouldBindOrTighterThanTernary()
        {
            AssertExpression("a lub b ? 1 : 2",
                new Conditional(Log(V("a"), TokenType.OR, "lub", V("b")), Num(1), Num(2)));
        }

        // ---------------------------------------------------------------
        // Assignment
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldTreatAssignmentAsRightAssociative()
        {
            AssertExpression("a = b = 1",
                new Assign(Id("a"), new Assign(Id("b"), Num(1))));
        }

        [Theory]
        [InlineData("+=", TokenType.PLUS_EQUAL)]
        [InlineData("-=", TokenType.MINUS_EQUAL)]
        [InlineData("*=", TokenType.STAR_EQUAL)]
        [InlineData("/=", TokenType.SLASH_EQUAL)]
        [InlineData("%=", TokenType.PERCENT_EQUAL)]
        [InlineData("&=", TokenType.AMPERSAND_EQUAL)]
        public void Parse_ShouldParseCompoundAssignment(string op, TokenType type)
        {
            AssertExpression($"x {op} 1", new CompoundAssignment(Id("x"), Op(type, op), Num(1)));
        }

        [Fact]
        public void Parse_ShouldParseCompoundAssignmentWithComplexValue()
        {
            AssertExpression("x += 1 + 2",
                new CompoundAssignment(
                    Id("x"),
                    Op(TokenType.PLUS_EQUAL, "+="),
                    Bin(Num(1), TokenType.PLUS, "+", Num(2))));
        }

        [Fact]
        public void Parse_ShouldParseCompoundAssignmentWithAssignmentValue()
        {
            AssertExpression("x += y = 2",
                new CompoundAssignment(Id("x"), Op(TokenType.PLUS_EQUAL, "+="), new Assign(Id("y"), Num(2))));
        }

        // ---------------------------------------------------------------
        // Property access
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldParsePropertyGet()
        {
            AssertExpression("a.b", new Get(V("a"), Id("b")));
        }

        [Fact]
        public void Parse_ShouldParseChainedPropertyGet()
        {
            AssertExpression("a.b.c", new Get(new Get(V("a"), Id("b")), Id("c")));
        }

        [Fact]
        public void Parse_ShouldParsePropertySet()
        {
            AssertExpression("a.b = 1", new Set(V("a"), Id("b"), Num(1)));
        }

        [Fact]
        public void Parse_ShouldParseChainedPropertySet()
        {
            AssertExpression("a.b.c = 1", new Set(new Get(V("a"), Id("b")), Id("c"), Num(1)));
        }

        [Fact]
        public void Parse_ShouldParseSelfPropertySet()
        {
            AssertExpression("ja.x = 5", new Set(new Self(Op(TokenType.SELF, "ja")), Id("x"), Num(5)));
        }

        [Fact]
        public void Parse_ShouldParseSelfPropertyGetInExpression()
        {
            AssertExpression("ja.x + 1",
                Bin(new Get(new Self(Op(TokenType.SELF, "ja")), Id("x")), TokenType.PLUS, "+", Num(1)));
        }

        // ---------------------------------------------------------------
        // Calls
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldParseCallWithoutArguments()
        {
            AssertExpression("f()", Invoke(V("f")));
        }

        [Fact]
        public void Parse_ShouldParseCurriedCalls()
        {
            AssertExpression("f(1)(2)", Invoke(Invoke(V("f"), Num(1)), Num(2)));
        }

        [Fact]
        public void Parse_ShouldParseMethodCall()
        {
            AssertExpression("a.b(1)", Invoke(new Get(V("a"), Id("b")), Num(1)));
        }

        [Fact]
        public void Parse_ShouldParsePropertyAccessOnCallResult()
        {
            AssertExpression("a.b(1).c", new Get(Invoke(new Get(V("a"), Id("b")), Num(1)), Id("c")));
        }

        [Fact]
        public void Parse_ShouldParseNestedCallsAsArguments()
        {
            AssertExpression("f(g(1), 2 + 3)",
                Invoke(V("f"), Invoke(V("g"), Num(1)), Bin(Num(2), TokenType.PLUS, "+", Num(3))));
        }

        [Fact]
        public void Parse_ShouldAllowAssignmentAsCallArgument()
        {
            AssertExpression("f(x = 1)", Invoke(V("f"), new Assign(Id("x"), Num(1))));
        }

        // ---------------------------------------------------------------
        // Anonymous functions
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldParseAnonymousFunctionAssignedToVariable()
        {
            AssertParsesTo("zmienna f = funkcja(a, b) { zwróć a; };",
                new Var(Id("f"),
                    new AnonFunction(Params("a", "b"), new List<Stmt> { Ret(V("a")) })));
        }

        [Fact]
        public void Parse_ShouldParseAnonymousFunctionWithoutParametersAndBody()
        {
            AssertParsesTo("zmienna f = funkcja() { };",
                new Var(Id("f"), new AnonFunction(new List<Token>(), new List<Stmt>())));
        }

        [Fact]
        public void Parse_ShouldParseImmediatelyInvokedAnonymousFunction()
        {
            AssertParsesTo("zmienna r = funkcja() { zwróć 1; }();",
                new Var(Id("r"),
                    new Call(
                        new AnonFunction(new List<Token>(), new List<Stmt> { Ret(Num(1)) }),
                        Op(TokenType.RIGHT_PAREN, ")"),
                        new List<Expr>())));
        }

        [Fact]
        public void Parse_ShouldParseAnonymousFunctionAsArgument()
        {
            AssertParsesTo("wykonaj(funkcja(x) { zwróć x; });",
                ExprStmt(Invoke(V("wykonaj"),
                    new AnonFunction(Params("x"), new List<Stmt> { Ret(V("x")) }))));
        }

        // ---------------------------------------------------------------
        // Variable declarations
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldParseVarWithoutInitializer()
        {
            AssertParsesTo("zmienna x;", new Var(Id("x"), null));
        }

        [Fact]
        public void Parse_ShouldParseMultipleVarsWithInitializers()
        {
            AssertParsesTo("zmienna a = 1, b = 2, c;",
                new Var(Id("a"), Num(1)),
                new Var(Id("b"), Num(2)),
                new Var(Id("c"), null));
        }

        [Fact]
        public void Parse_ShouldParseChainedAssignmentAsInitializer()
        {
            AssertParsesTo("zmienna a = b = 1;",
                new Var(Id("a"), new Assign(Id("b"), Num(1))));
        }

        [Fact]
        public void Parse_ShouldParseStringConcatenationInitializer()
        {
            AssertParsesTo("zmienna s = \"a\" & \"b\";",
                new Var(Id("s"), Bin(Str("a"), TokenType.AMPERSAND, "&", Str("b"))));
        }

        // ---------------------------------------------------------------
        // Blocks and control flow
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldParseEmptyBlock()
        {
            AssertParsesTo("{}", Blk());
        }

        [Fact]
        public void Parse_ShouldParseNestedBlocks()
        {
            AssertParsesTo("{ x; { y; } }",
                Blk(ExprStmt(V("x")), Blk(ExprStmt(V("y")))));
        }

        [Fact]
        public void Parse_ShouldParseIfWithoutElse()
        {
            AssertParsesTo("jeżeli (x) y;",
                new If(V("x"), ExprStmt(V("y")), null));
        }

        [Fact]
        public void Parse_ShouldParseIfElseWithBlocks()
        {
            AssertParsesTo("jeżeli (x) { a; } w_przeciwnym_wypadku { b; }",
                new If(V("x"), Blk(ExprStmt(V("a"))), Blk(ExprStmt(V("b")))));
        }

        [Fact]
        public void Parse_ShouldBindDanglingElseToNearestIf()
        {
            AssertParsesTo("jeżeli (a) jeżeli (b) x; w_przeciwnym_wypadku y;",
                new If(V("a"),
                    new If(V("b"), ExprStmt(V("x")), ExprStmt(V("y"))),
                    null));
        }

        [Fact]
        public void Parse_ShouldParseElseIfChain()
        {
            AssertParsesTo("jeżeli (a) x; w_przeciwnym_wypadku jeżeli (b) y; w_przeciwnym_wypadku z;",
                new If(V("a"),
                    ExprStmt(V("x")),
                    new If(V("b"), ExprStmt(V("y")), ExprStmt(V("z")))));
        }

        [Fact]
        public void Parse_ShouldParseWhileWithSingleStatementBody()
        {
            AssertParsesTo("podczas_gdy (prawda) przerwij;",
                new While(new Literal(true), Brk()));
        }

        [Fact]
        public void Parse_ShouldParseNestedWhileLoops()
        {
            AssertParsesTo("podczas_gdy (a) podczas_gdy (b) x;",
                new While(V("a"), new While(V("b"), ExprStmt(V("x")))));
        }

        [Fact]
        public void Parse_ShouldParseBreakStatement()
        {
            AssertParsesTo("przerwij;", Brk());
        }

        // ---------------------------------------------------------------
        // For loop desugaring
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldDesugarForWithAllClausesOmittedToInfiniteWhile()
        {
            AssertParsesTo("dla (;;) przerwij;",
                new While(new Literal(true), Brk()));
        }

        [Fact]
        public void Parse_ShouldDesugarForWithBlockBody()
        {
            AssertParsesTo("dla (;;) { przerwij; }",
                new While(new Literal(true), Blk(Brk())));
        }

        [Fact]
        public void Parse_ShouldDesugarForWithExpressionInitializer()
        {
            AssertParsesTo("dla (i = 0; i < 3;) x;",
                Blk(
                    ExprStmt(new Assign(Id("i"), Num(0))),
                    new While(
                        Bin(V("i"), TokenType.LESS, "<", Num(3)),
                        ExprStmt(V("x")))));
        }

        [Fact]
        public void Parse_ShouldDesugarForWithOnlyIncrement()
        {
            AssertParsesTo("dla (; ; i += 1) x;",
                new While(
                    new Literal(true),
                    Blk(
                        ExprStmt(V("x")),
                        ExprStmt(new CompoundAssignment(Id("i"), Op(TokenType.PLUS_EQUAL, "+="), Num(1))))));
        }

        [Fact]
        public void Parse_ShouldDesugarForWithMultipleVarDeclarations()
        {
            AssertParsesTo("dla (zmienna i = 0, j = 10; i < j;) przerwij;",
                Blk(
                    new Var(Id("i"), Num(0)),
                    new Var(Id("j"), Num(10)),
                    new While(Bin(V("i"), TokenType.LESS, "<", V("j")), Brk())));
        }

        [Fact]
        public void Parse_ShouldDesugarForWithoutInitializerButWithCondition()
        {
            AssertParsesTo("dla (; i < 3;) x;",
                new While(Bin(V("i"), TokenType.LESS, "<", Num(3)), ExprStmt(V("x"))));
        }

        // ---------------------------------------------------------------
        // Functions and return
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldParseFunctionWithoutParametersAndBody()
        {
            AssertParsesTo("funkcja f() {}", Fn("f", new string[0]));
        }

        [Fact]
        public void Parse_ShouldParseReturnWithoutValue()
        {
            AssertParsesTo("funkcja f() { zwróć; }", Fn("f", new string[0], Ret()));
        }

        [Fact]
        public void Parse_ShouldParseReturnWithExpression()
        {
            AssertParsesTo("funkcja f(x) { zwróć x * 2 + 1; }",
                Fn("f", new[] { "x" },
                    Ret(Bin(Bin(V("x"), TokenType.STAR, "*", Num(2)), TokenType.PLUS, "+", Num(1)))));
        }

        [Fact]
        public void Parse_ShouldParseNestedFunctionDeclarations()
        {
            AssertParsesTo("funkcja a() { funkcja b() {} zwróć b; }",
                Fn("a", new string[0],
                    Fn("b", new string[0]),
                    Ret(V("b"))));
        }

        [Fact]
        public void Parse_ShouldParseFunctionWithMultipleStatements()
        {
            AssertParsesTo("funkcja f(n) { zmienna w = n; jeżeli (n) zwróć w; zwróć 0; }",
                Fn("f", new[] { "n" },
                    new Var(Id("w"), V("n")),
                    new If(V("n"), Ret(V("w")), null),
                    Ret(Num(0))));
        }

        [Fact]
        public void Parse_ShouldParseRecursiveCall()
        {
            AssertParsesTo("funkcja s(n) { zwróć n + s(n - 1); }",
                Fn("s", new[] { "n" },
                    Ret(Bin(V("n"), TokenType.PLUS, "+",
                        Invoke(V("s"), Bin(V("n"), TokenType.MINUS, "-", Num(1)))))));
        }

        // ---------------------------------------------------------------
        // Classes
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldParseEmptyClass()
        {
            AssertParsesTo("klasa A {}", Cls("A"));
        }

        [Fact]
        public void Parse_ShouldParseClassWithMethod()
        {
            AssertParsesTo("klasa A { metoda f(x) { zwróć x; } }",
                Cls("A", new List<Function> { Fn("f", new[] { "x" }, Ret(V("x"))) }));
        }

        [Fact]
        public void Parse_ShouldParseClassWithStaticMethod()
        {
            AssertParsesTo("klasa A { statyczna metoda g() {} }",
                Cls("A", staticMethods: new List<Function> { Fn("g", new string[0]) }));
        }

        [Fact]
        public void Parse_ShouldSplitMethodsAndStaticMethods()
        {
            AssertParsesTo("klasa A { metoda f() {} statyczna metoda g() {} metoda h() {} }",
                Cls("A",
                    new List<Function> { Fn("f", new string[0]), Fn("h", new string[0]) },
                    new List<Function> { Fn("g", new string[0]) }));
        }

        [Fact]
        public void Parse_ShouldParseConstructorWithoutMethodKeyword()
        {
            AssertParsesTo($"klasa A {{ {FunctionConstants.CONSTRUCTOR}(x) {{ ja.x = x; }} }}",
                Cls("A", new List<Function>
                {
                    Fn(FunctionConstants.CONSTRUCTOR, new[] { "x" },
                        ExprStmt(new Set(new Self(Op(TokenType.SELF, "ja")), Id("x"), V("x"))))
                }));
        }

        [Fact]
        public void Parse_ShouldParseClassFollowedByOtherStatements()
        {
            AssertParsesTo("klasa A {} zmienna a;",
                Cls("A"),
                new Var(Id("a"), null));
        }

        [Fact]
        public void Parse_ShouldParseClassInstantiationAndMethodCall()
        {
            AssertParsesTo("zmienna a = A(); a.f();",
                new Var(Id("a"), Invoke(V("A"))),
                ExprStmt(Invoke(new Get(V("a"), Id("f")))));
        }

        // ---------------------------------------------------------------
        // Prompt (REPL) mode
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_InPromptMode_ShouldWrapExpressionWithoutSemicolonInPrint()
        {
            var statements = Parse("1 + 2", promptMode: true);

            statements.Should().HaveCount(1);
            statements[0].Should().BeEquivalentTo(
                new Print(Bin(Num(1), TokenType.PLUS, "+", Num(2))),
                o => o.PreferringRuntimeMemberTypes());
        }

        [Fact]
        public void Parse_InPromptMode_ShouldPrintBareVariable()
        {
            var statements = Parse("x", promptMode: true);

            statements.Should().HaveCount(1);
            statements[0].Should().BeEquivalentTo(new Print(V("x")), o => o.PreferringRuntimeMemberTypes());
        }

        [Fact]
        public void Parse_InPromptMode_ShouldKeepExpressionStatementWhenSemicolonPresent()
        {
            var statements = Parse("1 + 2;", promptMode: true);

            statements.Should().HaveCount(1);
            statements[0].Should().BeEquivalentTo(
                ExprStmt(Bin(Num(1), TokenType.PLUS, "+", Num(2))),
                o => o.PreferringRuntimeMemberTypes());
        }

        [Fact]
        public void Parse_InPromptMode_ShouldStillRequireSemicolonInDeclarations()
        {
            var statements = Parse("zmienna x = 1;", promptMode: true);

            statements.Should().HaveCount(1);
            statements[0].Should().BeEquivalentTo(new Var(Id("x"), Num(1)), o => o.PreferringRuntimeMemberTypes());
        }

        [Fact]
        public void Parse_InPromptMode_ShouldParseSeveralBareExpressionsAsSeparatePrints()
        {
            var statements = Parse("1 2", promptMode: true);

            statements.Should().HaveCount(2);
            statements.Should().BeEquivalentTo(
                new Stmt[] { new Print(Num(1)), new Print(Num(2)) },
                o => o.PreferringRuntimeMemberTypes().WithStrictOrdering());
        }

        // ---------------------------------------------------------------
        // List literals
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldParseEmptyListLiteral()
        {
            AssertExpression("[]", Lst());
        }

        [Fact]
        public void Parse_ShouldParseListLiteralWithItems()
        {
            AssertExpression("[1, 2, 3]", Lst(Num(1), Num(2), Num(3)));
        }

        [Fact]
        public void Parse_ShouldParseListLiteralWithMixedExpressions()
        {
            AssertExpression("[1 + 2, \"a\", x, f(1)]",
                Lst(Bin(Num(1), TokenType.PLUS, "+", Num(2)), Str("a"), V("x"), Invoke(V("f"), Num(1))));
        }

        [Fact]
        public void Parse_ShouldParseNestedListLiterals()
        {
            AssertExpression("[[1], [2, 3], []]",
                Lst(Lst(Num(1)), Lst(Num(2), Num(3)), Lst()));
        }

        [Fact]
        public void Parse_ShouldParseTernaryAsListItem()
        {
            AssertExpression("[a ? 1 : 2, 3]",
                Lst(new Conditional(V("a"), Num(1), Num(2)), Num(3)));
        }

        [Fact]
        public void Parse_ShouldAllowTrailingCommaInListLiteral()
        {
            AssertExpression("[1, 2,]", Lst(Num(1), Num(2)));
        }

        [Fact]
        public void Parse_ShouldParseListLiteralAsInitializer()
        {
            AssertParsesTo("zmienna a = [1, 2];", new Var(Id("a"), Lst(Num(1), Num(2))));
        }

        [Fact]
        public void Parse_ShouldParseListLiteralAsCallArgument()
        {
            AssertExpression("f([1, 2], 3)", Invoke(V("f"), Lst(Num(1), Num(2)), Num(3)));
        }

        // ---------------------------------------------------------------
        // List indexing
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldParseListIndexing()
        {
            AssertExpression("a[0]", Idx("a", Num(0)));
        }

        [Fact]
        public void Parse_ShouldParseListIndexingWithWhitespaceBeforeBracket()
        {
            AssertExpression("a [0]", Idx("a", Num(0)));
        }

        [Fact]
        public void Parse_ShouldParseListIndexingWithExpressionIndex()
        {
            AssertExpression("a[i + 1]", Idx("a", Bin(V("i"), TokenType.PLUS, "+", Num(1))));
        }

        [Fact]
        public void Parse_ShouldParseNestedListIndexingInsideIndex()
        {
            AssertExpression("a[b[1]]", Idx("a", Idx("b", Num(1))));
        }

        [Fact]
        public void Parse_ShouldParseCallInsideIndex()
        {
            AssertExpression("a[f(1)]", Idx("a", Invoke(V("f"), Num(1))));
        }

        [Fact]
        public void Parse_ShouldParseIndexingInsideBinaryExpression()
        {
            AssertExpression("a[0] + a[1]",
                Bin(Idx("a", Num(0)), TokenType.PLUS, "+", Idx("a", Num(1))));
        }

        [Fact]
        public void Parse_ShouldBindIndexingTighterThanUnaryMinus()
        {
            AssertExpression("-a[0]", Un(TokenType.MINUS, "-", Idx("a", Num(0))));
        }

        [Fact]
        public void Parse_ShouldAllowIndexingAsExponentiationBase()
        {
            AssertExpression("a[0] ** 2", Bin(Idx("a", Num(0)), TokenType.STAR_STAR, "**", Num(2)));
        }

        [Fact]
        public void Parse_ShouldAllowPropertyAccessOnIndexedElement()
        {
            AssertExpression("a[0].x", new Get(Idx("a", Num(0)), Id("x")));
        }

        [Fact]
        public void Parse_ShouldAllowCallOnIndexedElement()
        {
            AssertExpression("a[0](1)", Invoke(Idx("a", Num(0)), Num(1)));
        }

        [Fact]
        public void Parse_ShouldAllowIndexingAsCallArgument()
        {
            AssertExpression("f(a[0], b[1])", Invoke(V("f"), Idx("a", Num(0)), Idx("b", Num(1))));
        }

        [Fact]
        public void Parse_ShouldAllowIndexingAsTernaryCondition()
        {
            AssertExpression("a[0] ? 1 : 2", new Conditional(Idx("a", Num(0)), Num(1), Num(2)));
        }

        [Fact]
        public void Parse_ShouldParseIndexingAsVariableInitializer()
        {
            AssertParsesTo("zmienna x = a[0];", new Var(Id("x"), Idx("a", Num(0))));
        }

        // ---------------------------------------------------------------
        // List index assignment
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldParseListIndexAssignment()
        {
            AssertExpression("a[0] = 5", IdxSet("a", Num(0), Num(5)));
        }

        [Fact]
        public void Parse_ShouldParseListIndexAssignmentWithExpressionIndexAndValue()
        {
            AssertExpression("a[i + 1] = x * 2",
                IdxSet("a",
                    Bin(V("i"), TokenType.PLUS, "+", Num(1)),
                    Bin(V("x"), TokenType.STAR, "*", Num(2))));
        }

        [Fact]
        public void Parse_ShouldTreatListIndexAssignmentAsRightAssociative()
        {
            AssertExpression("a[0] = b[1] = 2", IdxSet("a", Num(0), IdxSet("b", Num(1), Num(2))));
        }

        [Fact]
        public void Parse_ShouldAllowListLiteralAsIndexAssignmentValue()
        {
            AssertExpression("a[0] = [1, 2]", IdxSet("a", Num(0), Lst(Num(1), Num(2))));
        }

        [Fact]
        public void Parse_ShouldAllowIndexingAsAssignmentValue()
        {
            AssertExpression("x = a[0]", new Assign(Id("x"), Idx("a", Num(0))));
            AssertExpression("a[0] = a[1]", IdxSet("a", Num(0), Idx("a", Num(1))));
        }

        [Fact]
        public void Parse_ShouldAllowIndexAssignmentInsideForLoopBody()
        {
            AssertParsesTo("dla (zmienna i = 0; i < 3; i += 1) a[i] = i;",
                Blk(
                    new Var(Id("i"), Num(0)),
                    new While(
                        Bin(V("i"), TokenType.LESS, "<", Num(3)),
                        Blk(
                            ExprStmt(IdxSet("a", V("i"), V("i"))),
                            ExprStmt(new CompoundAssignment(Id("i"), Op(TokenType.PLUS_EQUAL, "+="), Num(1)))))));
        }

        // ---------------------------------------------------------------
        // Lists in prompt (REPL) mode
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_InPromptMode_ShouldPrintListLiteral()
        {
            var statements = Parse("[1, 2]", promptMode: true);

            statements.Should().HaveCount(1);
            statements[0].Should().BeEquivalentTo(new Print(Lst(Num(1), Num(2))), o => o.PreferringRuntimeMemberTypes());
        }

        [Fact]
        public void Parse_InPromptMode_ShouldPrintIndexedElement()
        {
            var statements = Parse("a[0]", promptMode: true);

            statements.Should().HaveCount(1);
            statements[0].Should().BeEquivalentTo(new Print(Idx("a", Num(0))), o => o.PreferringRuntimeMemberTypes());
        }

        [Fact]
        public void Parse_InPromptMode_ShouldKeepIndexAssignmentWithSemicolonAsExpressionStatement()
        {
            var statements = Parse("a[0] = 1;", promptMode: true);

            statements.Should().HaveCount(1);
            statements[0].Should().BeEquivalentTo(
                ExprStmt(IdxSet("a", Num(0), Num(1))), o => o.PreferringRuntimeMemberTypes());
        }

        // ---------------------------------------------------------------
        // Inheritance
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldParseEmptyClassWithSuperclass()
        {
            AssertParsesTo("klasa B dziedziczy_po A {}", Cls("B", superclass: "A"));
        }

        [Fact]
        public void Parse_ShouldParseClassWithSuperclassAndMethods()
        {
            AssertParsesTo("klasa B dziedziczy_po A { metoda f() {} statyczna metoda g() {} }",
                Cls("B",
                    new List<Function> { Fn("f", new string[0]) },
                    new List<Function> { Fn("g", new string[0]) },
                    superclass: "A"));
        }

        [Fact]
        public void Parse_ShouldParseClassWithSuperclassFollowedByOtherStatements()
        {
            AssertParsesTo("klasa B dziedziczy_po A {} zmienna b = B();",
                Cls("B", superclass: "A"),
                new Var(Id("b"), Invoke(V("B"))));
        }

        // ---------------------------------------------------------------
        // super
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldParseSuperMethodReference()
        {
            AssertExpression("super.f", Sup("f"));
        }

        [Fact]
        public void Parse_ShouldParseSuperMethodCall()
        {
            AssertExpression("super.f(1)", Invoke(Sup("f"), Num(1)));
        }

        [Fact]
        public void Parse_ShouldParsePropertyAccessOnSuperMethodCallResult()
        {
            AssertExpression("super.f().g", new Get(Invoke(Sup("f")), Id("g")));
        }

        [Fact]
        public void Parse_ShouldParseSuperCallInsideExpression()
        {
            AssertExpression("super.f() + 1", Bin(Invoke(Sup("f")), TokenType.PLUS, "+", Num(1)));
        }

        [Fact]
        public void Parse_ShouldParseSuperCallInsideMethodOfDerivedClass()
        {
            AssertParsesTo("klasa B dziedziczy_po A { metoda f() { zwróć super.f(); } }",
                Cls("B",
                    new List<Function> { Fn("f", new string[0], Ret(Invoke(Sup("f")))) },
                    superclass: "A"));
        }

        [Fact]
        public void Parse_ShouldParseSuperConstructorCall()
        {
            AssertParsesTo(
                $"klasa B dziedziczy_po A {{ {FunctionConstants.CONSTRUCTOR}(x) {{ super.{FunctionConstants.CONSTRUCTOR}(x); }} }}",
                Cls("B",
                    new List<Function>
                    {
                        Fn(FunctionConstants.CONSTRUCTOR, new[] { "x" },
                            ExprStmt(Invoke(Sup(FunctionConstants.CONSTRUCTOR), V("x"))))
                    },
                    superclass: "A"));
        }

        // ---------------------------------------------------------------
        // Unary operators in the exponent
        // ---------------------------------------------------------------

        [Fact]
        public void Parse_ShouldAllowUnaryMinusInExponent()
        {
            // Regression test: the right operand of '**' is parsed with Unary(),
            // so it may start with '-' or '!'.
            AssertExpression("2 ** -1",
                Bin(Num(2), TokenType.STAR_STAR, "**", Un(TokenType.MINUS, "-", Num(1))));
        }

        [Fact]
        public void Parse_ShouldAllowRepeatedUnaryMinusInExponent()
        {
            AssertExpression("2 ** --1",
                Bin(Num(2), TokenType.STAR_STAR, "**", Un(TokenType.MINUS, "-", Un(TokenType.MINUS, "-", Num(1)))));
        }

        [Fact]
        public void Parse_ShouldAllowBangInExponent()
        {
            AssertExpression("2 ** !a",
                Bin(Num(2), TokenType.STAR_STAR, "**", Un(TokenType.BANG, "!", V("a"))));
        }

        [Fact]
        public void Parse_ShouldKeepExponentiationRightAssociativeAfterUnaryInExponent()
        {
            AssertExpression("2 ** -3 ** 2",
                Bin(Num(2), TokenType.STAR_STAR, "**",
                    Un(TokenType.MINUS, "-", Bin(Num(3), TokenType.STAR_STAR, "**", Num(2)))));
        }

        [Fact]
        public void Parse_ShouldContinueWithFactorAfterExponentWithUnaryOperand()
        {
            AssertExpression("a ** -b * c",
                Bin(
                    Bin(V("a"), TokenType.STAR_STAR, "**", Un(TokenType.MINUS, "-", V("b"))),
                    TokenType.STAR, "*",
                    V("c")));
        }

        [Fact]
        public void Parse_ShouldAllowIndexingAndUnaryMinusInExponent()
        {
            AssertExpression("2 ** -a[0]",
                Bin(Num(2), TokenType.STAR_STAR, "**", Un(TokenType.MINUS, "-", Idx("a", Num(0)))));
        }

        [Fact]
        public void Parse_ShouldParseChainedIndexing()
        {
            AssertExpression("m[0][1]", IdxOf(Idx("m", Num(0)), Num(1)));
        }

        [Fact]
        public void Parse_ShouldParseTripleChainedIndexing()
        {
            AssertExpression("m[0][1][2]", IdxOf(IdxOf(Idx("m", Num(0)), Num(1)), Num(2)));
        }

        [Fact]
        public void Parse_ShouldParseIndexingOfProperty()
        {
            AssertExpression("a.b[0]", IdxOf(new Get(V("a"), Id("b")), Num(0)));
        }

        [Fact]
        public void Parse_ShouldParseIndexingOfCallResult()
        {
            AssertExpression("f()[0]", IdxOf(Invoke(V("f")), Num(0)));
        }

        [Fact]
        public void Parse_ShouldParseIndexingOfListLiteral()
        {
            AssertExpression("[1, 2][0]", IdxOf(Lst(Num(1), Num(2)), Num(0)));
        }

        [Fact]
        public void Parse_ShouldParseIndexingOfSelf()
        {
            // Dostosuj konstruktor Self do swojej definicji (tak jak w istniejących testach 'ja').
            AssertExpression("ja[0]", IdxOf(new Self(Op(TokenType.SELF, "ja")), Num(0)));
        }

        [Fact]
        public void Parse_ShouldParseMixedIndexingPropertyAndCallChain()
        {
            // a[0].b(1)[2].c
            AssertExpression("a[0].b(1)[2].c",
                new Get(IdxOf(Invoke(new Get(Idx("a", Num(0)), Id("b")), Num(1)), Num(2)), Id("c")));
        }

        [Fact]
        public void Parse_ShouldParseAssignmentToChainedIndex()
        {
            AssertExpression("m[0][1] = 5", IdxSetOf(Idx("m", Num(0)), Num(1), Num(5)));
        }

        [Fact]
        public void Parse_ShouldParseAssignmentToIndexedProperty()
        {
            AssertExpression("a.b[0] = 5", IdxSetOf(new Get(V("a"), Id("b")), Num(0), Num(5)));
        }

        [Fact]
        public void Parse_ShouldParseAssignmentToIndexOfCallResult()
        {
            AssertExpression("f()[0] = 5", IdxSetOf(Invoke(V("f")), Num(0), Num(5)));
        }


    }
}