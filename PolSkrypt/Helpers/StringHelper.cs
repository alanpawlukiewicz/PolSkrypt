using PolSkrypt.Constants;

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
    }
}
