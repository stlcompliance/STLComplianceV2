namespace LoadArr.Api.Options;

public sealed class SupplyArrClientOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5105/";

    public string ServiceToken { get; set; } = string.Empty;
}
