using PolSkrypt.Errors;
using PolSkrypt.Interpretation;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing;

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
        string source = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
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
        for (; ; )
        {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if (line == null) break;
            Run(line, true);
            ErrorHandler.HadError = false;
        }
    }

    private static void Run(string source, bool promptMode = false)
    {
        Scanner scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();
        Parser parser = new Parser(tokens, promptMode);
        var stmts = parser.Parse();

        if (ErrorHandler.HadError)
        {
            return;
        }

        Resolver resolver = new Resolver(_interpreter);
        resolver.Resolve(stmts);

        if (ErrorHandler.HadError)
        {
            return;
        }

        _interpreter.Interpret(stmts);
    }
}