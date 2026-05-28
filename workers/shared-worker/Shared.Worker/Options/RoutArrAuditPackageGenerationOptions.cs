namespace Shared.Worker.Options;

public sealed class RoutArrAuditPackageGenerationOptions
{
    public const string SectionName = "RoutArrAuditPackageGeneration";

    public bool Enabled { get; set; } = true;

    public string RoutArrBaseUrl { get; set; } = "http://localhost:5105";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 2;

    public int BatchSize { get; set; } = 5;

    public Guid? TenantId { get; set; }
}
