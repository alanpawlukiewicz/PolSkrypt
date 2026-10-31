using PPL.Constants;
using PPL.Contracts;
using PPL.Errors;
using PPL.Errors.Exceptions;
using PPL.Interpretation.Callables;
using PPL.Lexing;
using PPL.Parsing.Expressions;
using PPL.Parsing.Statements;

namespace PPL.Interpretation
{
    public class Interpreter : IExprVisitor<object?>, IStmtVisitor<object?>
    {
        private readonly Env _globals = new Env();
        public Env Globals => _globals;
        private Env _environment;

        public Interpreter()
        {
            _environment = _globals;
            _globals.Define("zegar", new ClockCallable());
            _globals.Define("wypisz", new PrintCallable());
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeException ex)
            {
                ErrorHandler.RuntimeError(ex);
            }
        }

        private void Execute(Stmt statement)
        {
            statement.Accept(this);
        }

        private object? Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        public object? VisitBinaryExpression(Binary expression)
        {
            object? left = Evaluate(expression.Left);
            object? right = Evaluate(expression.Right);

            switch (expression.Operator.Type)
            {
                case TokenType.MINUS:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double?)left - (double?)right;
                case TokenType.SLASH:
                    CheckNumberOperands(expression.Operator, left, right);
                    if ((double?)right == 0)
                    {
                        throw new RuntimeException(expression.Operator, "Pamiętaj cholero, nigdy nie dziel przez 0!");
                    }
                    return (double?)left / (double?)right;
                case TokenType.STAR:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double?)left * (double?)right;
                case TokenType.PLUS:
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }

                    throw new RuntimeException(expression.Operator, "Operandy muszą być liczbami.");
                case TokenType.DOT:
                    object? parsedRight = new();
                    if (right is double)
                    {
                        parsedRight = right.ToString();
                    }
                    if (right is string)
                    {
                        parsedRight = (string)right;
                    }
                    if (left is string && parsedRight is string)
                    {
                        return (string)left + parsedRight;
                    }

                    throw new RuntimeException(expression.Operator, "Operandy muszą być liczbami lub ciągami znaków.");
                case TokenType.GREATER:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double?)left > (double?)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double?)left >= (double?)right;
                case TokenType.LESS:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double?)left < (double?)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double?)left <= (double?)right;
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
                case TokenType.COMMA:
                    break;
            }

            return null;
        }

        public object? VisitConditionalExpression(Conditional expression)
        {
            object? condition = Evaluate(expression.Condition);

            if (IsTruthy(condition))
            {
                return Evaluate(expression.ThenBranch);
            }
            else
            {
                return Evaluate(expression.ElseBranch);
            }
        }

        public object? VisitGroupingExpression(Grouping expression)
        {
            return Evaluate(expression.Expression);
        }

        public object? VisitLiteralExpression(Literal expression)
        {
            return expression.Value;
        }

        public object? VisitUnaryExpression(Unary expression)
        {
            object? right = Evaluate(expression.Right);

            switch (expression.Operator.Type)
            {
                case TokenType.MINUS:
                    CheckNumberOperand(expression.Operator, right);
                    return -(double?)right;
                case TokenType.BANG:
                    return !IsTruthy(right);
            }

            return null;
        }

        private bool IsTruthy(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is bool)
            {
                return (bool)obj;
            }

            return true;
        }

        private bool IsEqual(object? left, object? right)
        {
            if (left == null && right == null)
            {
                return true;
            }
            if (left == null)
            {
                return false;
            }

            return left.Equals(right);
        }

        private void CheckNumberOperand(Token op, object? operand)
        {
            if (operand is double)
                return;
            throw new RuntimeException(op, "Operand musi być liczbą.");
        }

        private void CheckNumberOperands(Token op, object? left, object? right)
        {
            if (left is double && right is double)
                return;
            throw new RuntimeException(op, "Operandy muszą być liczbami.");
        }

        private string Stringify(object? value)
        {
            if (value == null)
            {
                return KeyWord.NULL;
            }

            if (value is double)
            {
                string text = value.ToString()!;

                return text;
            }

            if (value is bool)
            {
                return (bool)value ? KeyWord.TRUE : KeyWord.FALSE;
            }

            return value.ToString() ?? string.Empty;
        }

        object? IStmtVisitor<object?>.VisitExpressionStmt(StmtExpression statement)
        {
            Evaluate(statement.Expression);
            return null;
        }

        object? IStmtVisitor<object?>.VisitPrintStmt(Print statement)
        {
            object? value = Evaluate(statement.Expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object? VisitVariableExpression(Variable expression)
        {
            return _environment.Get(expression.Name);
        }

        public object? VisitVarStmt(Var statement)
        {
            object? value = null;
            if (statement.Initializer != null)
            {
                value = Evaluate(statement.Initializer);
            }

            _environment.Define(statement.Name.Lexeme, value);
            return null;
        }

        public object? VisitAssignExpression(Assign expression)
        {
            object? value = Evaluate(expression.Value);
            _environment.Assign(expression.Name, value);
            return value;
        }

        public object? VisitBlockStmt(Block statement)
        {
            ExecuteBlock(statement.Statements, new Env(_environment));
            return null;
        }

        public void ExecuteBlock(List<Stmt> stmts, Env env)
        {
            Env previous = _environment;
            try
            {
                _environment = env;
                foreach (var stmt in stmts)
                {
                    Execute(stmt);
                }
            }
            finally
            {
                _environment = previous;
            }
        }

        public object? VisitIfStmt(If statement)
        {
            if (IsTruthy(Evaluate(statement.Condition)))
            {
                Execute(statement.ThenBranch);
            }
            else if (statement.ElseBranch != null)
            {
                Execute(statement.ElseBranch);
            }

            return null;
        }

        public object? VisitLogicalExpression(Logical expression)
        {
            var left = Evaluate(expression.Left);

            if (expression.Operator.Type == TokenType.OR)
            {
                if (IsTruthy(left))
                {
                    return left;
                }
            }
            else
            {
                if (!IsTruthy(left))
                {
                    return left;
                }
            }

            return Evaluate(expression.Right);
        }

        public object? VisitWhileStmt(While statement)
        {
            try
            {
                while (IsTruthy(Evaluate(statement.Condition)))
                {
                    Execute(statement.Body);
                }
            }
            catch (BreakException)
            {
            }


            return null;
        }

        public object? VisitBreakStmt(Break statement)
        {
            throw new BreakException();
        }

        public object? VisitCallExpression(Call expression)
        {
            var callee = Evaluate(expression.Callee);

            List<object?> arguments = new() { };
            foreach (var argument in expression.Arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            if (callee is not ICallable)
            {
                throw new RuntimeException(expression.Paren, "Można tylko wywoływać funkcje oraz klasy.");
            }

            ICallable fun = (ICallable)callee;
            if (fun.Arity != null && arguments.Count != fun.Arity)
            {
                throw new RuntimeException(expression.Paren,
                    $"Oczekiwano {fun.Arity} argumentów, ale otrzymano {arguments.Count}.");
            }
            return fun.Call(this, arguments);
        }

        public object? VisitFunctionStmt(Function statement)
        {
            FunctionCallable fun = new FunctionCallable(statement, _environment);
            _environment.Define(statement.Name.Lexeme, fun);

            return null;
        }

        public object? VisitReturnStmt(Return statement)
        {
            var value = statement.Value == null ? null : Evaluate(statement.Value);

            throw new ReturnException(value);
        }
    }
}
