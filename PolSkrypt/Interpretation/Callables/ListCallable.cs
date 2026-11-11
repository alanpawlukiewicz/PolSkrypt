using Microsoft.VisualBasic;
using PolSkrypt.Constants;
using PolSkrypt.Contracts;
using PolSkrypt.Errors;
using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Helpers;
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
            APPEND, REMOVE, LENGTH, CONTAINS,
            INSERT, EMPTY, COPY, LOOKUP,
            COUNT, CLEAR
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

        public int? Arity { get
            {
                if (_callType == ListCallType.APPEND)
                {
                    return null;
                }
                else if (_callType == ListCallType.REMOVE || _callType == ListCallType.CONTAINS || _callType == ListCallType.COUNT)
                {
                    return 1;
                }
                else if (_callType == ListCallType.LENGTH || _callType == ListCallType.EMPTY 
                    || _callType == ListCallType.COPY || _callType == ListCallType.CLEAR)
                {
                    return 0;
                }
                else if (_callType == ListCallType.INSERT)
                {
                    return 2;
                }
                return null;
            }
        }

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            if (_callType == ListCallType.APPEND)
            {
                _list.AddRange(arguments);
            }
            else if (_callType == ListCallType.REMOVE)
            {
                HandleRemove(arguments[0]);
            }
            else if (_callType == ListCallType.LENGTH)
            {
                return (double)_list.Count;
            }
            else if (_callType == ListCallType.CONTAINS)
            {
                return _list.Contains(arguments[0]);
            }
            else if (_callType == ListCallType.INSERT)
            {
                HandleInsert(ListHelper.HandleIndex(arguments[0], _name), arguments[1]);
            }
            else if (_callType == ListCallType.EMPTY)
            {
                return _list.Count == 0;
            }
            else if (_callType == ListCallType.COPY)
            {
                return new List<object?>(_list);
            }
            else if (_callType == ListCallType.LOOKUP)
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    if (_list[i]!.Equals(arguments[0]))
                    {
                        return (double)i;
                    }
                }
                return null;
            }
            else if (_callType == ListCallType.COUNT)
            {
                int counter = 0;
                for (int i = 0; i < _list.Count; i++)
                {
                    if (_list[i]!.Equals(arguments[0]))
                    {
                        counter++;
                    }
                }
                return (double)counter;

            }
            else if (_callType == ListCallType.CLEAR)
            {
                _list.Clear();
            }
            return _list;
        }

        private void HandleRemove(object? argument)
        {
            try
            {
                _list.RemoveAt(ListHelper.HandleIndex(argument, _name));
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new RuntimeException(_name, "Indeks znajduje się poza zakresem listy.");
            }
        }

        private void HandleInsert(int index, object? argument)
        {
            try
            {
                _list.Insert(index, argument);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new RuntimeException(_name, "Indeks znajduje się poza zakresem listy.");
            }
        }
    }
}
