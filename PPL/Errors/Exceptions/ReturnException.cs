namespace PPL.Errors.Exceptions
{
    public class ReturnException : Exception
    {
        private readonly object? _value;
        public object? Value => _value;

        public ReturnException(object? value)
        {
            _value = value;
        }
    }
}
