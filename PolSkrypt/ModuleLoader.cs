using PolSkrypt.Errors;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Interpretation;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing;
using PolSkrypt.Parsing.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt
{
    // Claude: generating class
    public class ModuleLoader
    {
        private readonly Dictionary<string, PModule> _cache = new();
        private readonly HashSet<string> _loading = new();
        private readonly Interpreter _interpreter;

        private readonly Stack<string> _dirs = new();
        public string CurrentDirectory => _dirs.Count > 0 ? _dirs.Peek() : Directory.GetCurrentDirectory();

        public ModuleLoader(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public void RegisterEntryFile(string fullPath)
        {
            _loading.Add(fullPath);
            _dirs.Push(Path.GetDirectoryName(fullPath)!);
        }

        public List<Stmt>? Compile(string source, bool promptMode = false, string? file = null)
        {
            var tokens = new Scanner(source, file).ScanTokens();
            var stmts = new Parser(tokens, promptMode).Parse();
            if (ErrorHandler.HadError) 
                return null;

            new Resolver(_interpreter).Resolve(stmts);
            if (ErrorHandler.HadError) 
                return null;

            return stmts;
        }

        public PModule LoadModule(string fullPath, Token importToken)
        {
            if (_cache.TryGetValue(fullPath, out var cached))
                return cached;

            if (!_loading.Add(fullPath))
                throw new RuntimeException(importToken, $"Cykliczny import pliku '{fullPath}'.");

            try
            {
                if (!File.Exists(fullPath))
                    throw new RuntimeException(importToken, $"Nie znaleziono modułu '{fullPath}'.");

                string source = File.ReadAllText(fullPath, System.Text.Encoding.UTF8);
                string displayName = DisplayName(fullPath);
                var stmts = Compile(source, false, displayName);

                if (stmts == null)
                    throw new RuntimeException(importToken, $"Błąd w module '{displayName}'.");

                var moduleEnv = new Env(_interpreter.Builtins);
                _dirs.Push(Path.GetDirectoryName(fullPath)!);
                try 
                { 
                    _interpreter.ExecuteBlock(stmts, moduleEnv);
                }
                finally 
                { 
                    _dirs.Pop();
                }

                var module = new PModule(System.IO.Path.GetFileNameWithoutExtension(fullPath), fullPath, moduleEnv);
                _cache[fullPath] = module;
                return module;
            }
            finally
            {
                _loading.Remove(fullPath);
            }
        }

        private static string DisplayName(string fullPath)
        {
            return Path.GetRelativePath(Directory.GetCurrentDirectory(), fullPath);
        }

    }
}
