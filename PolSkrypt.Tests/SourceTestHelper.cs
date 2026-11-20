using FluentAssertions;
using PolSkrypt.Constants;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Interpretation;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing;
using PolSkrypt.Parsing.Expressions;
using PolSkrypt.Parsing.Statements;
using System.Globalization;

namespace PolSkrypt.Tests
{
    // Claude: generating class
    /// <summary>
    /// Shared helpers for tests that run PolSkrypt source code
    /// (Scanner -> Parser -> Resolver -> Interpreter) and read results from global variables.
    /// Statements are executed directly (not through Interpret), so RuntimeException is not swallowed.
    /// </summary>
    internal static class SourceTestHelper
    {
        public static Interpreter ExecuteSource(string source)
        {
            var statements = new Parser(
                new Scanner(source.Replace("@init", FunctionConstants.CONSTRUCTOR)).ScanTokens()).Parse();
            var interpreter = new Interpreter();
            new Resolver(interpreter).Resolve(statements);

            foreach (Stmt statement in statements)
            {
                statement.Accept(interpreter);
            }

            return interpreter;
        }

        public static object? Global(Interpreter interpreter, string name)
            => interpreter.VisitVariableExpression(new Variable(new Token(TokenType.IDENTIFIER, name, null, 1)));

        // Runs the program and returns the value of the "wynik" global (or another one).
        public static object? Eval(string source, string variable = "wynik")
            => Global(ExecuteSource(source), variable);

        public static List<object?> AsList(object? value)
            => value.Should().BeOfType<List<object?>>().Subject;

        public static void AssertRuntimeError(string source, string? messageFragment = null)
        {
            Action act = () => ExecuteSource(source);

            var assertion = act.Should().Throw<RuntimeException>();
            if (messageFragment != null)
            {
                assertion.Where(e => e.Message.Contains(messageFragment),
                    "komunikat błędu powinien zawierać \"{0}\"", messageFragment);
            }
        }

        public static string CaptureOutput(Action action)
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

        // Runs the action with the given CurrentCulture and restores the previous one afterwards.
        public static T WithCulture<T>(string cultureName, Func<T> action)
        {
            var original = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            try
            {
                return action();
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }
    }
}
