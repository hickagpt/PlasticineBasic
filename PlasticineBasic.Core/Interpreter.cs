namespace PlasticineBasic.Core
{
    public class Interpreter
    {
        #region Public Constructors

        public Interpreter()
        {
            _context = new ExecutionContext();
        }

        public Interpreter(ExecutionContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
        private Stack<int> callStack = new Stack<int>();

        private Stack<LoopContext> loopStack = new();

        #endregion Private Fields

        #region Private Methods

        private object EvaluateBinaryExpression(BinaryExpression bin)
        {
            var left = EvaluateExpression(bin.Left);
            var right = EvaluateExpression(bin.Right);

            switch (bin.Operator)
            {
                case "+":
                    if (left is string || right is string)
                        return $"{left}{right}";
                    if (left is double lNum && right is double rNum)
                        return lNum + rNum;
                    throw new InterpreterTypeException($"Cannot add types {left?.GetType().Name} and {right?.GetType().Name}");

                case "-":
                case "*":
                case "/":
                case "%":
                case "^":
                    if (left is double l && right is double r)
                    {
                        return bin.Operator switch
                        {
                            "-" => l - r,
                            "*" => l * r,
                            "/" => l / r,
                            "%" => l % r,
                            "^" => Math.Pow(l, r),
                            _ => throw new InterpreterTypeException($"Unknown operator {bin.Operator}")
                        };
                    }
                    throw new InterpreterTypeException($"Operator '{bin.Operator}' requires numeric operands, got {left?.GetType().Name} and {right?.GetType().Name}");

                // ... handle comparisons similarly

                default:
                    throw new InterpreterTypeException($"Unknown binary operator '{bin.Operator}'");
            }
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
                    if (_lineIndex.TryGetValue(gotoStmt.TargetLineNumber, out var targetIndex))
                    {
                        _currentIndex = targetIndex - 1; // -1 because it will be incremented after this call
                    }
                    else
                    {
                        throw new Exception($"GOTO target line {gotoStmt.TargetLineNumber} not found.");
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

                case GosubStatement gosubStatement:
                    callStack.Push(_currentIndex);
                    if (_lineIndex.TryGetValue(gosubStatement.TargetLineNumber, out var subIndex))
                    {
                        _currentIndex = subIndex - 1; // -1 because it will be incremented after this call
                    }
                    else
                    {
                        throw new Exception($"GOSUB target line {gosubStatement.TargetLineNumber} not found.");
                    }
                    break;

                case ReturnStatement returnStatement:
                    if (callStack.Count > 0)
                    {
                        _currentIndex = callStack.Pop();
                    }
                    else
                    {
                        throw new Exception("Return called without a matching GOSUB.");
                    }
                    break;

                case EndStatement endStatement:
                    _context.IsRunning = false;
                    break;

                case ForStatement forStmt:
                    var start = Convert.ToDouble(EvaluateExpression(forStmt.Start));
                    var end = Convert.ToDouble(EvaluateExpression(forStmt.End));
                    var step = forStmt.Step != null ? Convert.ToDouble(EvaluateExpression(forStmt.Step)) : 1.0;
                    _context.Variables[forStmt.VariableName] = start;
                    loopStack.Push(new LoopContext
                    {
                        VariableName = forStmt.VariableName,
                        EndValue = end,
                        StepValue = step,
                        ForIndex = _currentIndex
                    });
                    break;

                case NextStatement nextStmt:
                    if (loopStack.Count == 0)
                        throw new Exception("NEXT without FOR");
                    var loop = loopStack.Peek();
                    if (loop.VariableName != nextStmt.VariableName)
                        throw new Exception($"NEXT variable '{nextStmt.VariableName}' does not match FOR variable '{loop.VariableName}'");
                    var current = Convert.ToDouble(_context.Variables[loop.VariableName]);
                    current += loop.StepValue;
                    _context.Variables[loop.VariableName] = current;
                    if ((loop.StepValue > 0 && current <= loop.EndValue) ||
                        (loop.StepValue < 0 && current >= loop.EndValue))
                    {
                        _currentIndex = loop.ForIndex; // Loop again
                    }
                    else
                    {
                        loopStack.Pop(); // Loop finished
                    }
                    break;

                default:
                    throw new NotImplementedException($"Statement type '{stmt.GetType().Name}' not handled.");
            }
        }

        #endregion Private Methods

        #region Private Classes

        private class LoopContext
        {
            #region Public Fields

            public double EndValue;
            public int ForIndex;
            public double StepValue;
            public string VariableName;

            #endregion Public Fields

            // Index in the statement list where the FOR is
        }

        #endregion Private Classes
    }
}