using PPL;
using PPL.Enums;
using PPL.Expressions;
using System.Runtime.CompilerServices;

public class Program
{
    private static readonly Interpreter _interpreter = new Interpreter();
    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
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
        byte[] bytes = File.ReadAllBytes(filePath);
        var str = System.Text.Encoding.Default.GetString(bytes);
        Run(str);

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

        _interpreter.Interpret(stmts);
    }
}