using FluentAssertions;
using PolSkrypt.Errors;
using PolSkrypt.Lexing;

namespace PolSkrypt.Tests
{
    // Claude: Fix the test class to ensure that it does not run in parallel with other tests that may affect the ErrorHandler state.
    [Collection("ErrorHandler")]
    public class ScannerErrorTests : IDisposable
    {
        // Claude: Capture the console error output to verify error messages.
        private readonly TextWriter _originalError;
        private readonly StringWriter _errorOutput;

        public ScannerErrorTests()
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

        [Fact]
        public void ScanTokens_ShouldThrowErrorForUnterminatedString()
        {
            var scanner = new Scanner("\"This is an unterminated string");

            scanner.ScanTokens();

            ErrorHandler.HadError.Should().BeTrue();
            _errorOutput.ToString().Should().Contain("[Linia 1]");
            _errorOutput.ToString().Should().Contain("Nieoczekiwany koniec pliku w stringu.");
        }
    }
}
