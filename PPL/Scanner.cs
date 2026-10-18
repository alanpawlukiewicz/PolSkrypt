using PPL.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PPL
{
    public class Scanner
    {
        private readonly string _source;
        private readonly List<Token> _tokens = new List<Token>();

        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        private Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>
        {
            {"oraz", TokenType.AND},
            {"klasa", TokenType.CLASS},
            {"w_przeciwnym_wypadku", TokenType.ELSE},
            {"fałsz", TokenType.FALSE},
            {"funkcja", TokenType.FUN},
            {"dla", TokenType.FOR},
            {"jeśli", TokenType.IF},
            {"wartość_pusta", TokenType.NULL},
            {"lub", TokenType.OR},
            {"wyświetl", TokenType.PRINT},
            {"zwróc", TokenType.RETURN},
            {"super", TokenType.SUPER},
            {"to", TokenType.THIS},
            {"prawda", TokenType.TRUE},
            {"zmienna", TokenType.VAR},
            {"podczas_gdy", TokenType.WHILE}
        };

        public Scanner(string source)
        {
            _source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, string.Empty, null, 0)); // Add an EOF token at the end
            return _tokens;
        }

        private bool IsAtEnd()
        {
            return _current >= _source.Length;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '?': AddToken(TokenType.QUESTION_MARK); break;
                case ':': AddToken(TokenType.COLON); break;
                case '/':
                    // Handle comments
                    if (CheckNext('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd()) 
                            Advance();
                    }
                    if (CheckNext('*'))
                    {
                        while (!(Peek() == '*' && CheckNext('/')) && !IsAtEnd())
                        {
                            if (Peek() == '\n') _line++;
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case '!':
                    AddToken(CheckNext('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(CheckNext('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(CheckNext('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(CheckNext('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;

                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    _line++;
                    break;

                case '"': HandleString(); break;
                default:
                    if (char.IsDigit(c))
                    {
                        HandleNumber();
                    }
                    else if (char.IsLetter(c) || c == '_')
                    {
                        HandleIdentifier();
                    }
                    else
                    {
                        ErrorHandler.Error(_line, $"Nieoczekiwany znak: '{c}'");
                    }
                    break;
            }
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object? literal = null)
        {
            string text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line));
        }

        private char Advance()
        {
            return _source[_current++];
        }

        private bool CheckNext(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;

            _current++;
            return true;
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return _source[_current];
        }

        private void HandleString()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;
                Advance();
            }

            if (IsAtEnd())
            {
                ErrorHandler.Error(_line, "Nieoczekiwany koniec pliku w stringu.");
                return;
            }

            Advance();

            string value = _source.Substring(_start + 1, _current - _start - 2);
            AddToken(TokenType.STRING, value);
        }

        private void HandleNumber()
        {
            while (char.IsDigit(Peek()))
            {
                Advance();
            }
            
            if (Peek() == '.' && char.IsDigit(PeekNext()))
            {
                // Ignore the "."
                Advance();

                while (char.IsDigit(Peek()))
                {
                    Advance();
                }
            }

            AddToken(TokenType.NUMBER, double.Parse(_source.Substring(_start, _current - _start), CultureInfo.InvariantCulture));
        }

        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) return '\0';
            return _source[_current + 1];
        }

        private void HandleIdentifier()
        {
            while (char.IsLetterOrDigit(Peek()) || Peek() == '_')
            {
                Advance();
            }

            string text = _source.Substring(_start, _current - _start);
            TokenType type = _keywords.TryGetValue(text, out TokenType foundType) ? foundType : TokenType.IDENTIFIER;
            AddToken(type);
        }
    }
}
