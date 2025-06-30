namespace Domain.Exceptions
{
    public class BusTourException : Exception
    {
        public string ErrorCode { get; }

        public BusTourException(string message, string errorCode = "BUS_TOUR_ERROR") 
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public BusTourException(string message, Exception innerException, string errorCode = "BUS_TOUR_ERROR") 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

    public class ExternalApiException : BusTourException
    {
        public int StatusCode { get; }

        public ExternalApiException(string message, int statusCode) 
            : base(message, $"EXTERNAL_API_{statusCode}")
        {
            StatusCode = statusCode;
        }
    }

    public class ValidationException : BusTourException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(string message) 
            : base(message, "VALIDATION_ERROR")
        {
            Errors = new Dictionary<string, string[]>();
        }
    }
} 