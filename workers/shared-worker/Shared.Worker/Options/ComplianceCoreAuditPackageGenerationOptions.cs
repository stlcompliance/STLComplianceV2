namespace Shared.Worker.Options;

public sealed class ComplianceCoreAuditPackageGenerationOptions
{
    public const string SectionName = "ComplianceCoreAuditPackageGeneration";

    public bool Enabled { get; set; } = true;

    public string ComplianceCoreBaseUrl { get; set; } = "http://localhost:5107";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 2;

    public int BatchSize { get; set; } = 5;

    public Guid? TenantId { get; set; }
}
