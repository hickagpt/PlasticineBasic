namespace PlasticineBasic.Core
{
    public class Interpreter
    {
        #region Public Constructors

        public Interpreter()
        {
            _context = new ExecutionContext();
        }

        #endregion Public Constructors

        #region Public Methods

        public void Execute(ProgramNode program)
        {
            _program = program;

            for (int i = 0; i < program.Statements.Count; i++)
            {
                var stmt = program.Statements[i];
                if (stmt.LineNumber.HasValue)
                    _lineIndex[stmt.LineNumber.Value] = i;
            }

            _currentIndex = 0;

            while (_currentIndex < program.Statements.Count && _context.IsRunning)
            {
                var statement = program.Statements[_currentIndex];
                ExecuteStatement(statement);
                _currentIndex++;
            }
        }

        #endregion Public Methods

        #region Private Fields

        private readonly ExecutionContext _context;
        private readonly Dictionary<int, int> _lineIndex = new();
        private int _currentIndex;
        private ProgramNode _program;

        #endregion Private Fields

        #region Private Methods

        private object EvaluateBinaryExpression(BinaryExpression bin)
        {
            var left = EvaluateExpression(bin.Left);
            var right = EvaluateExpression(bin.Right);

            return bin.Operator switch
            {
                "+" =>
                    left is string || right is string
                        ? $"{left}{right}"
                        : Convert.ToDouble(left) + Convert.ToDouble(right),

                "-" => Convert.ToDouble(left) - Convert.ToDouble(right),
                "*" => Convert.ToDouble(left) * Convert.ToDouble(right),
                "/" => Convert.ToDouble(left) / Convert.ToDouble(right),
                "%" => Convert.ToDouble(left) % Convert.ToDouble(right),
                "^" => Math.Pow(Convert.ToDouble(left), Convert.ToDouble(right)),

                "=" => Equals(left, right),
                "<>" => !Equals(left, right),
                "<" => Convert.ToDouble(left) < Convert.ToDouble(right),
                "<=" => Convert.ToDouble(left) <= Convert.ToDouble(right),
                ">" => Convert.ToDouble(left) > Convert.ToDouble(right),
                ">=" => Convert.ToDouble(left) >= Convert.ToDouble(right),

                _ => throw new Exception($"Unknown binary operator '{bin.Operator}'")
            };
        }

        private object EvaluateExpression(ExpressionNode expr)
        {
            return expr switch
            {
                NumberLiteral num => num.Value,
                StringLiteral str => str.Value,
                VariableReference var =>
                    _context.Variables.TryGetValue(var.Name, out var val) ? val : throw new Exception($"Undefined variable '{var.Name}'"),

                BinaryExpression bin => EvaluateBinaryExpression(bin),
                _ => throw new NotImplementedException($"Expression type '{expr.GetType().Name}' not handled.")
            };
        }

        private void ExecuteStatement(StatementNode stmt)
        {
            switch (stmt)
            {
                case LetStatement let:
                    var value = EvaluateExpression(let.Expression);
                    _context.Variables[let.VariableName] = value;
                    break;

                case PrintStatement print:
                    foreach (var expr in print.Values)
                    {
                        Console.Write(EvaluateExpression(expr));
                    }
                    Console.WriteLine();
                    break;

                case GotoStatement gotoStmt:
                    if (_lineIndex.TryGetValue(gotoStmt.LineNumber, out var targetIndex))
                    {
                        _currentIndex = targetIndex - 1; // -1 because it will be incremented after this call
                    }
                    else
                    {
                        throw new Exception($"GOTO target line {gotoStmt.LineNumber} not found.");
                    }
                    break;

                case IfStatement ifStmt:
                    var condition = EvaluateExpression(ifStmt.Condition);
                    if (Convert.ToBoolean(condition))
                        ExecuteStatement(ifStmt.ThenBranch);
                    break;

                case InputStatement inputStmt:

                    foreach (var variable in inputStmt.Variables)
                    {
                        Console.Write($"Enter value for {variable.Name}: ");
                        var input = Console.ReadLine();
                        if (double.TryParse(input, out var number))
                        {
                            _context.Variables[variable.Name] = number;
                        }
                        else
                        {
                            _context.Variables[variable.Name] = input; // Store as string if not a number
                        }
                    }
                    break;

                case EndStatement endStatement:
                    _context.IsRunning = false;
                    break;

                default:
                    throw new NotImplementedException($"Statement type '{stmt.GetType().Name}' not handled.");
            }
        }

        #endregion Private Methods
    }
}