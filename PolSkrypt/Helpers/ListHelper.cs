using PolSkrypt.Errors.Exceptions;
using PolSkrypt.Lexing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolSkrypt.Helpers
{
    public static class ListHelper
    {
        public static int HandleIndex(object? argument, Token name)
        {
            if (argument is not double index)
            {
                throw new RuntimeException(name, "Podano niepoprawny indeks.");
            }
            if (index < 0 || index % 1 != 0 || index > int.MaxValue)
            {
                throw new RuntimeException(name, "Indeks musi być dodatnią liczbą całkowitą.");
            }

            return (int)index;
        }
    }
}
