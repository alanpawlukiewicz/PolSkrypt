using Microsoft.VisualBasic;
using PolSkrypt.Constants;
using PolSkrypt.Contracts;
using PolSkrypt.Errors;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Lexing;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;

namespace PolSkrypt.Interpretation.Callables
{
    public class ListCallable : ICallable
    {
        private enum ListCallType
        {
            APPEND,
            REMOVE,
            LENGTH
        }

        private ListCallType ToListCallType(string type)
        {
            var x = FunctionConstants.LIST_FUNCTIONS[type].ToUpper();
            if (Enum.TryParse(FunctionConstants.LIST_FUNCTIONS[type].ToUpper(), out ListCallType parsedType))
            {
                return parsedType;
            }
            throw new RuntimeException(_name, "Podano niepoprawną funkcję listy.");
        }

        private readonly ListCallType _callType;
        private readonly List<object?> _list;
        private readonly Token _name;

        public ListCallable(List<object?> list, Token name)
        {
            if (FunctionConstants.LIST_FUNCTIONS.ContainsKey(name.Lexeme))
            {
                _callType = ToListCallType(name.Lexeme);
            }
            else
            {
                throw new RuntimeException(name, "Podano niepoprawną funkcję listy.");
            }
            
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
