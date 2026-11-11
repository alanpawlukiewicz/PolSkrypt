namespace PolSkrypt.Constants
{
    public static class FunctionConstants
    {
        public const int MAX_PARAMS_COUNT = 255;
        public const string NATIVE_FN_STRING = "<funkcja natywna>";
        public static readonly Dictionary<string, string> LIST_FUNCTIONS = new Dictionary<string, string>{
            { "dodaj", "append" }, 
            { "usuń", "remove" }, 
            { "długość" , "length" } 
        };
    }
}
