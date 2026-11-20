using PolSkrypt;
using PolSkrypt.Constants;
using PolSkrypt.Errors;
using PolSkrypt.Helpers;
using PolSkrypt.Interpretation;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing;
using System.Text;

public class Program
{
    private static readonly Interpreter _interpreter = new Interpreter();
    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Użycie: PolSkrypt [plik.pols]");
            Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            var filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Nie znaleziono pliku: {filePath}");
                Environment.Exit(66);
            }
            RunFile(filePath);
        }
        else
        {
            RunPrompt();
        }
    }

    private static void RunFile(string filePath)
    {
        string fullPath = Path.GetFullPath(filePath);
        _interpreter.Modules.RegisterEntryFile(fullPath);
        string source = File.ReadAllText(fullPath, Encoding.UTF8);
        Run(source);

        if (ErrorHandler.HadError)
        {
            Environment.Exit(65);
        }
        if (ErrorHandler.HadRuntimeError)
        {
            Environment.Exit(70);
        }
    }

    private static void RunPrompt()
    {
        LineTrackingWriter.Install();

        Console.WriteLine($"PolSkrypt v{InterpreterVersion.CurrentVersion} REPL");
        for (; ; )
        {
            LineTrackingWriter.EnsureNewLine();
            Console.Write("> ");
            string? line = Console.ReadLine();
            if (line == null) break;
            LineTrackingWriter.MarkLineStart();
            Run(line, true);
            ErrorHandler.HadError = false;
        }
    }

    private static void Run(string source, bool promptMode = false)
    {
        var stmts = _interpreter.Modules.Compile(source, promptMode);
        if (stmts == null) return;
        _interpreter.Interpret(stmts);
    }
}