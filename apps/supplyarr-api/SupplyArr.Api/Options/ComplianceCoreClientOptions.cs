namespace SupplyArr.Api.Options;

public sealed class ComplianceCoreClientOptions
{
    public const string SectionName = "ComplianceCore";

    public string BaseUrl { get; set; } = "http://localhost:5107";

    public string ServiceToken { get; set; } = string.Empty;
}
