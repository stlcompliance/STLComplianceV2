namespace Shared.Worker.Options;

public sealed class TrainArrAuditPackageGenerationOptions
{
    public const string SectionName = "TrainArrAuditPackageGeneration";

    public bool Enabled { get; set; } = true;

    public string TrainArrBaseUrl { get; set; } = "http://localhost:5103";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 2;

    public int BatchSize { get; set; } = 5;

    public Guid? TenantId { get; set; }
}
