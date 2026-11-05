using PolSkrypt.Contracts;
using PolSkrypt.Errors;
using PolSkrypt.Interpretation.Callables;
using PolSkrypt.Lexing;
using PolSkrypt.Parsing.Expressions;
using PolSkrypt.Parsing.Statements;

namespace PolSkrypt.Interpretation
{
    public class Resolver : IStmtVisitor<object?>, IExprVisitor<object?>
    {
        private enum FunctionType
        {
            NONE,
            FUNCTION,
            METHOD,
            INITIALIZER
        }

        private enum ClassType
        {
            NONE,
            CLASS
        }

        private readonly Interpreter _interpreter;
        private readonly Stack<Dictionary<string, bool>> _scopes = new();
        private FunctionType _currentFunction = FunctionType.NONE;
        private bool _inLoop = false;
        private ClassType _currentClass = ClassType.NONE;

        public Resolver(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public object? VisitAnonFunctionExpression(AnonFunction expression)
        {
            ResolveFunction(expression, FunctionType.FUNCTION);
            return null;
        }

        public object? VisitAssignExpression(Assign expression)
        {
            Resolve(expression.Value);
            ResolveLocal(expression, expression.Name);

            return null;
        }

        public object? VisitBinaryExpression(Binary expression)
        {
            Resolve(expression.Left);
            Resolve(expression.Right);
            return null;
        }

        public object? VisitBlockStmt(Block statement)
        {
            BeginScope();
            Resolve(statement.Statements);
            EndScope();

            return null;
        }

        private void BeginScope()
        {
            _scopes.Push(new Dictionary<string, bool>());
        }

        public void Resolve(List<Stmt> statements)
        {
            foreach (var stmt in statements)
            {
                Resolve(stmt);
            }
        }

        private void Resolve(Stmt statement)
        {
            statement.Accept(this);
        }

        private void Resolve(Expr expression)
        {
            expression.Accept(this);
        }

        private void EndScope()
        {
            _scopes.Pop();
        }

        public object? VisitBreakStmt(Break statement)
        {
            if (!_inLoop)
            {
                ErrorHandler.Error(statement.KeyWord, "Wyrzucono 'przerwij' poza pętlą.");
            }
            return null;
        }

        public object? VisitCallExpression(Call expression)
        {
            Resolve(expression.Callee);
            foreach (var arg in expression.Arguments)
            {
                Resolve(arg);
            }
            return null;
        }

        public object? VisitCompoundAssignmentExpression(CompoundAssignment expression)
        {
            Resolve(expression.Value);
            ResolveLocal(expression, expression.Name);
            return null;
        }

        public object? VisitConditionalExpression(Conditional expression)
        {
            Resolve(expression.Condition);
            Resolve(expression.ThenBranch);
            Resolve(expression.ElseBranch);

            return null;
        }

        public object? VisitExpressionStmt(StmtExpression statement)
        {
            Resolve(statement.Expression);
            return null;
        }

        public object? VisitFunctionStmt(Function statement)
        {
            Declare(statement.Name);
            Define(statement.Name);

            ResolveFunction(statement, FunctionType.FUNCTION);
            return null;
        }

        private void ResolveFunction(IFunction function, FunctionType type)
        {
            FunctionType enclosingFunction = _currentFunction;
            bool enclosingInLoop = _inLoop;
            _currentFunction = type;
            _inLoop = false;

            BeginScope();
            foreach (var param in function.Parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.Body);
            EndScope();

            _currentFunction = enclosingFunction;
            _inLoop = enclosingInLoop;
        }

        public object? VisitGroupingExpression(Grouping expression)
        {
            Resolve(expression.Expression);
            return null;
        }

        public object? VisitIfStmt(If statement)
        {
            Resolve(statement.Condition);
            Resolve(statement.ThenBranch);
            if (statement.ElseBranch != null)
            {
                Resolve(statement.ElseBranch);
            }
            return null;
        }

        public object? VisitLiteralExpression(Literal expression)
        {
            return null;
        }

        public object? VisitLogicalExpression(Logical expression)
        {
            Resolve(expression.Left);
            Resolve(expression.Right);
            return null;
        }

        public object? VisitPrintStmt(Print statement)
        {
            Resolve(statement.Expression);
            return null;
        }

        public object? VisitReturnStmt(Return statement)
        {
            if (_currentFunction == FunctionType.NONE)
            {
                ErrorHandler.Error(statement.KeyWord, "Nie można zwrócić wartości spoza funkcji.");
            }
            if (statement.Value != null)
            {
                if (_currentFunction == FunctionType.INITIALIZER)
                {
                    ErrorHandler.Error(statement.KeyWord, "Nie można zwrócić wartości z konstruktora.");
                }
                Resolve(statement.Value);
            }
            return null;
        }

        public object? VisitUnaryExpression(Unary expression)
        {
            Resolve(expression.Right);
            return null;
        }

        public object? VisitVariableExpression(Variable expression)
        {
            if (_scopes.Count > 0
                && _scopes.Peek().TryGetValue(expression.Name.Lexeme, out bool defined) && defined == false)
            {
                ErrorHandler.Error(expression.Name, "Nie można odczytać lokalnej zmiennej w jej własnym inicjatorze.");
            }

            ResolveLocal(expression, expression.Name);
            return null;
        }

        private void ResolveLocal(Expr expression, Token name)
        {
            int distance = 0;
            foreach (var scope in _scopes)
            {
                if (scope.ContainsKey(name.Lexeme))
                {
                    _interpreter.Resolve(expression, distance);
                    return;
                }
                distance++;
            }
        }

        public object? VisitVarStmt(Var statement)
        {
            Declare(statement.Name);
            if (statement.Initializer != null)
            {
                Resolve(statement.Initializer);
            }
            Define(statement.Name);

            return null;
        }

        private void Declare(Token name)
        {
            if (_scopes.Count == 0)
            {
                return;
            }

            Dictionary<string, bool> scope = _scopes.Peek();
            if (scope.ContainsKey(name.Lexeme))
            {
                ErrorHandler.Error(name, "Zmienna o tej nazwie już istnieje w tym zakresie.");
            }

            scope[name.Lexeme] = false;
        }

        private void Define(Token name)
        {
            if (_scopes.Count == 0)
            {
                return;
            }
            _scopes.Peek()[name.Lexeme] = true;
        }

        public object? VisitWhileStmt(While statement)
        {
            bool previousInLoop = _inLoop;
            _inLoop = true;
            Resolve(statement.Condition);
            Resolve(statement.Body);
            _inLoop = previousInLoop;
            return null;
        }

        public object? VisitClassStmt(Class statement)
        {
            ClassType previousClass = _currentClass;
            _currentClass = ClassType.CLASS;

            Declare(statement.Name);
            Define(statement.Name);

            BeginScope();
            _scopes.Peek()["ja"] = true;

            foreach (var method in statement.Methods)
            {
                FunctionType declaration = FunctionType.METHOD;
                if (method.Name.Lexeme == "konstruktor")
                {
                    declaration = FunctionType.INITIALIZER;
                }
                ResolveFunction(method, declaration);
            }
            
            EndScope();
            _currentClass = previousClass;

            return null;
        }

        public object? VisitGetExpression(Get expression)
        {
            Resolve(expression.Object);
            return null;
        }

        public object? VisitSetExpression(Set expression)
        {
            Resolve(expression.Value);
            Resolve(expression.Object);

            return null;
        }

        public object? VisitSelfExpression(Self expression)
        {
            if (_currentClass == ClassType.NONE)
            {
                ErrorHandler.Error(expression.Keyword, "Nie można użyć 'ja' poza klasą.");
            }
            ResolveLocal(expression, expression.Keyword);

            return null;
        }
    }
}
