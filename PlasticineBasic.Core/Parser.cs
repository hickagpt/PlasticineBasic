using System.Drawing;

namespace PlasticineBasic.Core
{
    public class GosubStatement : StatementNode
    {
        #region Public Properties

        public int TargetLineNumber { get; set; }

        #endregion Public Properties
    }

    public class Parser
    {
        #region Public Constructors

        public Parser(List<Token> tokens, bool verbose = false)
        {
            _verbose = verbose;
            _tokens = tokens;
        }

        #endregion Public Constructors

        #region Public Methods

        public ProgramNode Parse()
        {
            var program = new ProgramNode();

            while (!IsAtEnd())
            {
                SkipEndOfLine(); // 👈 Add this

                if (IsAtEnd()) break;

                var stmt = ParseStatement();
                if (stmt != null)
                    program.Statements.Add(stmt);
            }

            return program;
        }

        #endregion Public Methods

        #region Private Fields

        private int _position;
        private List<Token> _tokens;
        private bool _verbose;

        #endregion Private Fields

        #region Private Methods

        private Exception Error(Token ifToken, string v)
        {
            return new Exception($"Error at {ifToken.Line}:{ifToken.Column} - {v}. Found '{ifToken.Value}'");
        }

        private int GetPrecedence(TokenType type)
        {
            return type switch
            {
                TokenType.Power => 4,
                TokenType.Multiply or TokenType.Divide or TokenType.Modulus => 3,
                TokenType.Plus or TokenType.Minus => 2,
                // Add TokenType.Assign here:
                TokenType.Equal or TokenType.Assign or TokenType.NotEqual or TokenType.LessThan or TokenType.LessThanOrEqual or TokenType.GreaterThan or TokenType.GreaterThanOrEqual => 1,
                _ => 0
            };
        }

        private bool IsAtEnd()
        {
            return _position >= _tokens.Count || _tokens[_position].Type == TokenType.EndOfFile;
        }

        private bool Match(TokenType let)
        {
            if (IsAtEnd() || Peek().Type != let)
                return false;
            _position++;
            return true;
        }

        private ExpressionNode ParseExpression(int parentPrecedence = 0)
        {
            if (_verbose)
                Console.WriteLine($"ParseExpression at token {_position}: {Peek().Type} '{Peek().Value}'");

            ExpressionNode left = ParsePrimary();

            while (true)
            {
                var current = Peek();
                int precedence = GetPrecedence(current.Type);

                if (precedence == 0 || precedence <= parentPrecedence)
                    break;

                if (_verbose)
                    Console.WriteLine($"Consuming operator '{current.Value}' at position {_position}");

                _position++; // consume operator
                var op = current;

                ExpressionNode right = ParseExpression(precedence);
                string opValue = op.Value;
                if (op.Type == TokenType.Assign || op.Type == TokenType.Equal)
                    opValue = "=";
                left = new BinaryExpression
                {
                    Left = left,
                    Operator = opValue,
                    Right = right
                };
            }

            if (_verbose)
                Console.WriteLine($"Returning expression at position {_position}");

            return left;
        }

        private StatementNode ParseGoto()
        {
            var gotoToken = Peek();
            if (gotoToken.Type != TokenType.Goto)
                throw Error(gotoToken, "Expected GOTO keyword");
            _position++; // Consume GOTO
            if (Peek().Type != TokenType.NumberLiteral)
                throw Error(Peek(), "Expected line number after GOTO");
            int lineNumber = int.Parse(Peek().Value);
            _position++; // Consume line number
            return new GotoStatement { TargetLineNumber = lineNumber };
        }

        private StatementNode ParseIf()
        {
            var ifToken = Peek();
            if (ifToken.Type != TokenType.If)
                throw Error(ifToken, "Expected IF keyword");
            _position++; // Consume IF
            var condition = ParseExpression();
            if (!Match(TokenType.Then))
                throw Error(Peek(), "Expected THEN after IF condition");
            var thenBranch = ParseStatement();
            IfStatement ifStatement = new IfStatement
            {
                Condition = condition,
                ThenBranch = thenBranch
            };
            // Optional ELSE branch
            if (Match(TokenType.Else))
            {
                var elseBranch = ParseStatement();
                ifStatement.ThenBranch = elseBranch; // For simplicity, we replace the then branch with the else branch
            }
            return ifStatement;
        }

        private StatementNode ParseLet()
        {
            var variableToken = Peek();
            if (variableToken.Type != TokenType.Identifier)
                throw Error(variableToken, "Expected variable name after LET");
            _position++; // Consume identifier
            if (!Match(TokenType.Assign))
                throw Error(Peek(), "Expected '=' after variable name");
            var expression = ParseExpression();
            return new LetStatement
            {
                VariableName = variableToken.Value,
                Expression = expression
            };
        }

        private ExpressionNode ParsePrimary()
        {
            var token = Peek();
            switch (token.Type)
            {
                case TokenType.NumberLiteral:
                    _position++;
                    return new NumberLiteral { Value = double.Parse(token.Value) };

                case TokenType.StringLiteral:
                    _position++;
                    return new StringLiteral { Value = token.Value }; // ✅ Add this line

                case TokenType.Identifier:
                    _position++;
                    return new VariableReference { Name = token.Value };

                case TokenType.LeftParen:
                    _position++;
                    var expr = ParseExpression();
                    if (!Match(TokenType.RightParen))
                        throw Error(Peek(), "Expected ')' after expression");
                    return expr;

                default:
                    throw Error(token, "Unexpected token in expression");
            }
        }

        private StatementNode ParsePrint()
        {
            var printStatement = new PrintStatement();

            do
            {
                SkipEndOfLine(); // Just in case

                if (IsAtEnd() || Peek().Type == TokenType.EndOfLine || Peek().Type == TokenType.EndOfFile)
                    break;

                var expr = ParseExpression();
                printStatement.Values.Add(expr);
            } while (Match(TokenType.Comma)); // Allow multiple expressions

            return printStatement;
        }

        private StatementNode ParsePrintLine()
        {
            var printStatement = new PrintLineStatement();
            do
            {
                SkipEndOfLine(); // Just in case
                if (IsAtEnd() || Peek().Type == TokenType.EndOfLine || Peek().Type == TokenType.EndOfFile)
                    break;
                var expr = ParseExpression();
                printStatement.Values.Add(expr);
            } while (Match(TokenType.Comma)); // Allow multiple expressions
            return printStatement;
        }

        private StatementNode ParseStatement()
        {
            SkipEndOfLine();

            if (_verbose)
            {
                Console.WriteLine($"Parsing statement at token: {_tokens[_position].Type} ('{_tokens[_position].Value}') at position {_position}");
            }

            int? lineNumber = null;
            if ((Peek().Type == TokenType.LineNumber || Peek().Type == TokenType.NumberLiteral) && int.TryParse(Peek().Value, out var label))
            {
                lineNumber = label;
                _position++; // consume line number
            }

            StatementNode stmt;
            if (Peek().Type == TokenType.Let)
            {
                _position++;  // consume LET
                stmt = ParseLet();
            }
            else if (Peek().Type == TokenType.Rem)
            {
                _position++;
                var comment = new CommentStatement { Text = Peek().Value };
                _position++;  // consume comment text
                stmt = comment;
            }
            else if (Peek().Type == TokenType.Random)
            {
                _position++;  // consume RANDOM
                if (Peek().Type != TokenType.Identifier)
                    throw Error(Peek(), "Expected variable name after RANDOM");
                var variableName = Peek().Value;
                _position++;  // consume identifier
                stmt = new RandomStatement { VariableName = variableName };
            }
            else if (Peek().Type == TokenType.Print)
            {
                _position++;  // consume PRINT
                stmt = ParsePrint();
            }
            else if (Peek().Type == TokenType.PrintLine)
            {
                _position++;  // consume PRINT
                stmt = ParsePrintLine();
            }
            else if (Peek().Type == TokenType.If)
            {
                stmt = ParseIf();  // do NOT consume here, ParseIf will consume IF token
            }
            else if (Peek().Type == TokenType.Goto)
            {
                stmt = ParseGoto();
            }
            else if (Peek().Type == TokenType.Return)
            {
                _position++;  // consume RETURN
                stmt = new ReturnStatement();
            }
            else if (Peek().Type == TokenType.EndOfLine)
            {
                _position++;  // consume END OF LINE
                return null; // skip empty lines
            }
            else if (Peek().Type == TokenType.Gosub)
            {
                _position++;  // consume GOSUB
                if (Peek().Type != TokenType.NumberLiteral)
                    throw Error(Peek(), "Expected line number after GOSUB");
                int lineNumberGosub = int.Parse(Peek().Value);
                _position++;  // consume line number
                stmt = new GosubStatement { TargetLineNumber = lineNumberGosub };
            }
            else if (Peek().Type == TokenType.Input)
            {
                _position++;  // consume INPUT
                var inputStatement = new InputStatement();
                while (!IsAtEnd() && Peek().Type != TokenType.EndOfLine && Peek().Type != TokenType.EndOfFile)
                {
                    if (Peek().Type != TokenType.Identifier)
                        throw Error(Peek(), "Expected variable name after INPUT");

                    inputStatement.Variables.Add(new VariableReference { Name = Peek().Value });
                    _position++;  // consume identifier

                    if (Match(TokenType.Comma))
                        continue; // allow comma-separated variables

                    break; // exit loop on non-comma
                }
                stmt = inputStatement;
            }
            else if (Peek().Type == TokenType.SetForegroundColor)
            {
                _position++;
                if (Peek().Type != TokenType.Identifier)
                    throw Error(Peek(), "Expected color name after SET FOREGROUND COLOR");
                var colorToken = Peek();
                _position++; // consume color name
                stmt = new SetForegroundColorStatement
                {
                    Color = Color.FromName(colorToken.Value)
                };
            }
            else if (Peek().Type == TokenType.SetBackgroundColor)
            {
                _position++;
                if (Peek().Type != TokenType.Identifier)
                    throw Error(Peek(), "Expected color name after SET BACKGROUND COLOR");
                var colorToken = Peek();
                _position++; // consume color name
                stmt = new SetBackgroundColorStatement
                {
                    Color = Color.FromName(colorToken.Value)
                };
            }
            else if (Peek().Type == TokenType.For)
            {
                _position++; // consume FOR
                var varToken = Peek();
                if (varToken.Type != TokenType.Identifier)
                    throw Error(varToken, "Expected variable name after FOR");
                _position++; // consume variable

                if (!Match(TokenType.Assign))
                    throw Error(Peek(), "Expected '=' after variable name");

                var startExpr = ParseExpression();
                if (!Match(TokenType.To))
                    throw Error(Peek(), "Expected TO after start expression");

                var endExpr = ParseExpression();
                ExpressionNode stepExpr = null;
                if (Match(TokenType.Step))
                    stepExpr = ParseExpression();

                return new ForStatement
                {
                    VariableName = varToken.Value,
                    Start = startExpr,
                    End = endExpr,
                    Step = stepExpr
                };
            }
            else if (Peek().Type == TokenType.Next)
            {
                _position++; // consume NEXT
                var varToken = Peek();
                if (varToken.Type != TokenType.Identifier)
                    throw Error(varToken, "Expected variable name after NEXT");
                _position++; // consume variable
                return new NextStatement { VariableName = varToken.Value };
            }
            else if (Peek().Type == TokenType.End)
            {
                _position++;  // consume END
                stmt = new EndStatement();
            }
            else
            {
                throw Error(Peek(), "Unexpected token");
            }

            stmt.LineNumber = lineNumber;
            return stmt;
        }

        private Token Peek()
        {
            if (IsAtEnd())
                return new Token(TokenType.EndOfFile, string.Empty, 0, 0);
            return _tokens[_position];
        }

        private void SkipEndOfLine()
        {
            while (!IsAtEnd() && Peek().Type == TokenType.EndOfLine)
                _position++;
        }

        #endregion Private Methods
    }
}