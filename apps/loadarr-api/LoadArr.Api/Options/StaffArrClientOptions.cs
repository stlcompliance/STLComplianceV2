namespace LoadArr.Api.Options;

public sealed class StaffArrClientOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5102/";

    public string ServiceToken { get; set; } = string.Empty;
}
