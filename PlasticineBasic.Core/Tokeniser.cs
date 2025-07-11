namespace PlasticineBasic.Core
{
    public class Tokeniser
    {
        #region Public Constructors

        public Tokeniser(string source)
        {
            _source = source.Replace("\r\n", "\n"); // Normalize line endings
            _position = 0;
            _line = 1;
            _column = 1;
        }

        #endregion Public Constructors

        #region Public Methods

        public List<Token> Tokenise()
        {
            while (!IsAtEnd())
            {
                SkipWhitespace();
                var startColumn = _column;

                char current = Peek();

                if (char.IsLetter(current))
                    TokenizeWord();
                else if (char.IsDigit(current))
                    TokenizeNumber();
                else if (current == '"')
                    TokenizeString();
                else
                    TokenizeSymbol();

                // Optional: handle line breaks (e.g., count line numbers)
            }

            _tokens.Add(new Token(TokenType.EndOfFile, string.Empty, _line, _column));
            _previousTokenType = TokenType.EndOfFile;
            return _tokens;
        }

        #endregion Public Methods

        #region Private Fields

        private readonly string _source;
        private readonly List<Token> _tokens = new();
        private int _column;
        private int _line;
        private int _position;
        private TokenType? _previousTokenType = null;

        #endregion Private Fields

        #region Private Methods

        private bool IsAtEnd()
        {
            return _position >= _source.Length;
        }

        private char Peek(int v = 0)
        {
            int peekPosition = _position + v;
            return peekPosition < _source.Length ? _source[peekPosition] : '\0';
        }

        private void SkipWhitespace()
        {
            while (!IsAtEnd() && char.IsWhiteSpace(Peek()))
            {
                if (Peek() == '\n')
                {
                    _line++;
                    _column = 1;
                    _position++;
                    var token = new Token(TokenType.EndOfLine, "\\n", _line - 1, _column);
                    _tokens.Add(token);
                    _previousTokenType = token.Type;
                }
                else
                {
                    _column++;
                    _position++;
                }
            }
        }

        private void TokenizeNumber()
        {
            int start = _position;
            while (!IsAtEnd() && (char.IsDigit(Peek()) || Peek() == '.'))
            {
                _position++;
                _column++;
            }

            string value = _source.Substring(start, _position - start);
            bool isInteger = !value.Contains(".");
            TokenType type = TokenType.NumberLiteral;

            // Determine if this is a LineNumber:
            if (isInteger &&
                (_tokens.Count == 0 || _previousTokenType == null || _previousTokenType == TokenType.EndOfLine || _previousTokenType == TokenType.Colon))
            {
                type = TokenType.LineNumber;
            }

            var token = new Token(type, value, _line, start + 1);
            _tokens.Add(token);
            _previousTokenType = token.Type;
        }

        private void TokenizeString()
        {
            int start = _position + 1; // Skip the opening quote
            _position++; // Move past the opening quote
            _column++;
            while (!IsAtEnd() && Peek() != '"')
            {
                if (Peek() == '\n')
                {
                    _line++;
                    _column = 1;
                }
                else
                {
                    _column++;
                }
                _position++;
            }
            if (IsAtEnd())
            {
                throw new Exception($"Unterminated string literal at line {_line}, column {_column}");
            }
            _position++; // Move past the closing quote
            _column++;
            string value = _source.Substring(start, _position - start - 1);
            _tokens.Add(new Token(TokenType.StringLiteral, value, _line, start + 1));
            _previousTokenType = TokenType.StringLiteral;
        }

        private void TokenizeSymbol()
        {
            char current = Peek();
            TokenType type;
            switch (current)
            {
                case '+':
                    type = TokenType.Plus;
                    break;

                case '-':
                    type = TokenType.Minus;
                    break;

                case '*':
                    type = TokenType.Multiply;
                    break;

                case '/':
                    type = TokenType.Divide;
                    break;

                case '%':
                    type = TokenType.Modulus;
                    break;

                case '^':
                    type = TokenType.Power;
                    break;

                case '=':
                    type = TokenType.Assign; // For LET
                    break;

                case '<':
                    if (Peek(1) == '>')
                    {
                        _position++;
                        _column++;
                        _tokens.Add(new Token(TokenType.NotEqual, "<>", _line, _column - 1));
                        _previousTokenType = TokenType.NotEqual;
                        return;
                    }
                    type = TokenType.LessThan;
                    break;

                case '>':
                    type = TokenType.GreaterThan;
                    break;

                case '(':
                    type = TokenType.LeftParen;
                    break;

                case ')':
                    type = TokenType.RightParen;
                    break;

                case ',':
                    type = TokenType.Comma;
                    break;

                case ':':
                    type = TokenType.Colon; // Statement separator
                    break;

                default:
                    throw new Exception($"Unexpected character '{current}' at line {_line}, column {_column}");
            }
            _tokens.Add(new Token(type, current.ToString(), _line, _column));
            _previousTokenType = type;
            _position++;
            _column++;
        }

        private void TokenizeWord()
        {
            int start = _position;
            while (!IsAtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
            {
                _position++;
                _column++;
            }
            string value = _source.Substring(start, _position - start);
            TokenType type = value.ToUpper() switch
            {
                "LET" => TokenType.Let,
                "PRINT" => TokenType.Print,
                "INPUT" => TokenType.Input,
                "IF" => TokenType.If,
                "THEN" => TokenType.Then,
                "ELSE" => TokenType.Else,
                "GOTO" => TokenType.Goto,
                "GOSUB" => TokenType.Gosub,
                "RETURN" => TokenType.Return,
                "FOR" => TokenType.For,
                "TO" => TokenType.To,
                "STEP" => TokenType.Step,
                "NEXT" => TokenType.Next,
                "DIM" => TokenType.Dim,
                "REM" => TokenType.Rem,
                "END" => TokenType.End,
                "STOP" => TokenType.Stop,
                "DEF" => TokenType.Def,
                "FN" => TokenType.Fn,
                "SUB" => TokenType.Sub,
                _ => TokenType.Identifier
            };
            _tokens.Add(new Token(type, value, _line, start + 1));
            _previousTokenType = type;
        }

        #endregion Private Methods
    }
}