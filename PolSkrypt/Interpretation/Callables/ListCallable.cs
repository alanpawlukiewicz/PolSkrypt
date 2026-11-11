using PolSkrypt.Contracts;
using PolSkrypt.Errors;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Lexing;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PolSkrypt.Interpretation.Callables
{
    public class ListCallable : ICallable
    {
        public enum ListCallType
        {
            APPEND,
            REMOVE
        }
        private readonly ListCallType _callType;
        private readonly List<object?> _list;
        private readonly Token _name;

        public ListCallable(ListCallType callType, List<object?> list, Token name)
        {
            _callType = callType;
            _list = list;
            _name = name;
        }

        public int? Arity => _callType == ListCallType.REMOVE ? 1 : null;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            if (_callType == ListCallType.APPEND)
            {
                _list.AddRange(arguments);
            }
            else if (_callType == ListCallType.REMOVE)
            {
                if (arguments[0] is not double index)
                {
                    ErrorHandler.Error(_name, "Podano niepoprawny indeks.");
                    return _list;
                }
                try
                {
                    int parsedIndex = (int)index;
                    _list.RemoveAt(parsedIndex);
                }
                catch (ArgumentOutOfRangeException)
                {
                    ErrorHandler.Error(_name, "Indeks znajduje się poza zakresem listy.");
                    return _list;
                }
            }
            return _list;
        }
    }
}
