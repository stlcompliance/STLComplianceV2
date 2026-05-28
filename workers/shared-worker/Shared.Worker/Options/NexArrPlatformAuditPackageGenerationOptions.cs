namespace Shared.Worker.Options;

public sealed class NexArrPlatformAuditPackageGenerationOptions
{
    public const string SectionName = "NexArrPlatformAuditPackageGeneration";

    public bool Enabled { get; set; } = true;

    public string NexArrBaseUrl { get; set; } = "http://localhost:5101";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 2;

    public int BatchSize { get; set; } = 5;

    public Guid? TenantId { get; set; }
}
