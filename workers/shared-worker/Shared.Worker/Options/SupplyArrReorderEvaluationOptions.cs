namespace Shared.Worker.Options;

public sealed class SupplyArrReorderEvaluationOptions
{
    public const string SectionName = "SupplyArrReorderEvaluation";

    public bool Enabled { get; set; } = true;

    public string SupplyArrBaseUrl { get; set; } = "http://localhost:5106";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 60;

    public int BatchSize { get; set; } = 100;

    public bool CreateDraftPurchaseRequests { get; set; } = true;

    public Guid? TenantId { get; set; }
}
