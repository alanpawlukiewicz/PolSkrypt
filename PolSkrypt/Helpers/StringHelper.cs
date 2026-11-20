using PolSkrypt.Constants;
using System.Globalization;

namespace PolSkrypt.Helpers
{
    public static class StringHelper
    {
        public static string Stringify(object? value)
        {
            if (value is null)
            {
                return Keywords.NULL;
            }

            if (value is double d)
            {
                if (double.IsPositiveInfinity(d)) return "nieskończoność";
                if (double.IsNegativeInfinity(d)) return "-nieskończoność";
                if (double.IsNaN(d)) return "nie-liczba";
                if (d == 0) return "0";

                return d.ToString(CultureInfo.InvariantCulture);
            }

            if (value is bool)
            {
                return (bool)value ? Keywords.TRUE : Keywords.FALSE;
            }

            if (value is List<object?> list)
            {
                return $"[{string.Join(", ", list.Select(Stringify))}]";
            }

            return value.ToString() ?? string.Empty;
        }
    }
}
