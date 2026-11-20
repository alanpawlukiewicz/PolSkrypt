namespace PolSkrypt.Lexing
{
    public class Token
    {
        private readonly TokenType _type;
        private readonly string _lexeme = string.Empty;
        private readonly object? _literal;
        private readonly int _line;
        private readonly string? _file;

        public TokenType Type { get { return _type; } }
        public string Lexeme { get { return _lexeme; } }
        public object? Literal { get { return _literal; } }
        public int Line { get { return _line; } }

        /// <summary>
        /// The name of the source file from which the token comes (for error messages).
        /// null for the main file, REPL, and code-created tokens.
        /// </summary>
        public string? File { get { return _file; } }

        public Token(TokenType type, string lexeme, object? literal, int line, string? file = null)
        {
            _type = type;
            _lexeme = lexeme;
            _literal = literal;
            _line = line;
            _file = file;
        }

        public override string ToString()
        {
            return $"{_type} {_lexeme} {_literal}";
        }
    }
}
