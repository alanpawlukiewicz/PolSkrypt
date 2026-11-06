using PolSkrypt.Constants;
using PolSkrypt.Contracts;
using PolSkrypt.Errors;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Interpretation.Callables;
using PolSkrypt.Interpretation.Callables.NativeCallables;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing.Expressions;
using PolSkrypt.Parsing.Statements;

namespace PolSkrypt.Interpretation
{
    public class Interpreter : IExprVisitor<object?>, IStmtVisitor<object?>
    {
        private readonly Env _globals = new Env();
        public Env Globals => _globals;
        private Env _environment;
        private readonly Dictionary<Expr, int> _locals = new();

        public Interpreter()
        {
            _environment = _globals;
            _globals.Define("zegar", new ClockCallable());
            _globals.Define("wypisz", new PrintCallable());
            _globals.Define("wprowadź", new InputCallable());
            _globals.Define("liczba", new NumberCallable());
            _globals.Define("napis", new StringCallable());
            _globals.Define("typ", new TypeCallable());
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

        public void Resolve(Expr expr, int depth)
        {
            _locals[expr] = depth;
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
                case TokenType.PERCENT:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double?)left % (double?)right;
                case TokenType.PLUS:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double?)left + (double?)right;
                case TokenType.AMPERSAND:
                    return Stringify(left) + Stringify(right);
                case TokenType.STAR_STAR:
                    CheckNumberOperands(expression.Operator, left, right);
                    return Math.Pow((double?)left ?? 0, (double?)right ?? 0);
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
                return Keywords.NULL;
            }

            if (value is double)
            {
                string text = value.ToString()!;

                return text;
            }

            if (value is bool)
            {
                return (bool)value ? Keywords.TRUE : Keywords.FALSE;
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
            return LookUpVariable(expression.Name, expression);
        }

        private object? LookUpVariable(Token name, Expr expression)
        {
            if (_locals.TryGetValue(expression, out int depth))
            {
                return _environment.GetAt(depth, name);
            }
            else
            {
                return _globals.Get(name);
            }
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
            if (_locals.TryGetValue(expression, out int depth))
            {
                _environment.AssignAt(depth, expression.Name, value);
            }
            else
            {
                _globals.Assign(expression.Name, value);
            }

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
            throw new BreakException(statement.Keyword, "Wyrzucono 'przerwij' poza pętlą.");
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

        public object? VisitAnonFunctionExpression(AnonFunction expression)
        {
            return new AnonFunctionCallable(expression, _environment);
        }

        public object? VisitCompoundAssignmentExpression(CompoundAssignment expression)
        {
            var currentValue = _environment.Get(expression.Name);
            var value = Evaluate(expression.Value);
            object? newValue = null;
            switch (expression.Operator.Type)
            {
                case TokenType.PLUS_EQUAL:
                    CheckNumberOperands(expression.Operator, currentValue, value);
                    newValue = (double?)currentValue + (double?)value;
                    break;
                case TokenType.MINUS_EQUAL:
                    CheckNumberOperands(expression.Operator, currentValue, value);
                    newValue = (double?)currentValue - (double?)value;
                    break;
                case TokenType.STAR_EQUAL:
                    CheckNumberOperands(expression.Operator, currentValue, value);
                    newValue = (double?)currentValue * (double?)value;
                    break;
                case TokenType.SLASH_EQUAL:
                    CheckNumberOperands(expression.Operator, currentValue, value);
                    newValue = (double?)currentValue / (double?)value;
                    break;
                case TokenType.PERCENT_EQUAL:
                    CheckNumberOperands(expression.Operator, currentValue, value);
                    newValue = (double?)currentValue % (double?)value;
                    break;
                case TokenType.AMPERSAND_EQUAL:
                    newValue = Stringify(currentValue) + Stringify(value);
                    break;
            }

            if (_locals.TryGetValue(expression, out int depth))
            {
                _environment.AssignAt(depth, expression.Name, newValue);
            }
            else
            {
                _globals.Assign(expression.Name, newValue);
            }

            return newValue;
        }

        public object? VisitClassStmt(Class statement)
        {
            _environment.Define(statement.Name.Lexeme, null);

            var methods = new Dictionary<string, FunctionCallable>();
            foreach (var method in statement.Methods)
                methods[method.Name.Lexeme] = new FunctionCallable(method, _environment, method.Name.Lexeme == Keywords.CONSTRUCTOR);

            var staticMethods = new Dictionary<string, FunctionCallable>();
            foreach (var method in statement.StaticMethods)
                staticMethods[method.Name.Lexeme] = new FunctionCallable(method, _environment, false);

            var classCallable = new ClassCallable(statement.Name.Lexeme, methods, staticMethods);
            _environment.Assign(statement.Name, classCallable);
            return null;
        }

        public object? VisitGetExpression(Get expression)
        {
            object? obj = Evaluate(expression.Object);

            if (obj is Instance instance)
                return instance.Get(expression.Name);

            if (obj is ClassCallable klass)
            {
                var m = klass.FindStaticMethod(expression.Name.Lexeme);
                if (m != null) return m;
                throw new RuntimeException(expression.Name,
                    $"Klasa '{klass.Name}' nie posiada metody statycznej '{expression.Name.Lexeme}'.");
            }

            throw new RuntimeException(expression.Name, "Tylko instancje i klasy mają właściwości.");
        }

        public object? VisitSetExpression(Set expression)
        {
            var obj = Evaluate(expression.Object);

            if (obj is Instance instance)
            {
                var value = Evaluate(expression.Value);
                instance.Set(expression.Name, value);

                return value;
            }

            throw new RuntimeException(expression.Name, "Tylko instancje mają atrybuty.");
        }

        public object? VisitSelfExpression(Self expression)
        {
            return LookUpVariable(expression.Keyword, expression);
        }
    }
}
