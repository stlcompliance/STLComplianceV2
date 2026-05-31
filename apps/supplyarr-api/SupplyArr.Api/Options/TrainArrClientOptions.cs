namespace SupplyArr.Api.Options;

public sealed class TrainArrClientOptions
{
    public const string SectionName = "TrainArr";

    public string BaseUrl { get; set; } = "http://localhost:5176";

    public string ServiceToken { get; set; } = string.Empty;

    public string ReceivingQualificationKey { get; set; } = string.Empty;

    public string? ReceivingRulePackKey { get; set; }
}
