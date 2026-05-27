namespace MaintainArr.Api.Options;

public sealed class SupplyArrClientOptions
{
    public const string SectionName = "SupplyArr";

    public string BaseUrl { get; set; } = "http://localhost:5106";

    public string ServiceToken { get; set; } = string.Empty;
}
