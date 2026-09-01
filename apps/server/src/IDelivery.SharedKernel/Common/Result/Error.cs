namespace IDelivery.SharedKernel.Common.Result
{
    public class Error
    {
        public string Code { get; }
        public string Message { get; }

        public Error(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public static readonly Error None = new(string.Empty, string.Empty);
        public static readonly Error NullValue = new("Error.NullValue", "O valor não pode ser nulo");
    }
}
