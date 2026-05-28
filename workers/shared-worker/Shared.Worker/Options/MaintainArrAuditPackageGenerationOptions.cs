namespace Shared.Worker.Options;

public sealed class MaintainArrAuditPackageGenerationOptions
{
    public const string SectionName = "MaintainArrAuditPackageGeneration";

    public bool Enabled { get; set; } = true;

    public string MaintainArrBaseUrl { get; set; } = "http://localhost:5104";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 2;

    public int BatchSize { get; set; } = 5;

    public Guid? TenantId { get; set; }
}
