namespace Shared.Worker.Options;

public sealed class StaffArrAuditPackageGenerationOptions
{
    public const string SectionName = "StaffArrAuditPackageGeneration";

    public bool Enabled { get; set; } = true;

    public string StaffArrBaseUrl { get; set; } = "http://localhost:5102";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 2;

    public int BatchSize { get; set; } = 5;

    public Guid? TenantId { get; set; }
}
