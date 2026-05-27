namespace STLCompliance.Shared.Contracts;

public sealed class StlApiException : Exception
{
    public StlApiException(string code, string message, int statusCode = 400, object? details = null)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
        Details = details;
    }

    public string Code { get; }

    public int StatusCode { get; }

    public object? Details { get; }
}
