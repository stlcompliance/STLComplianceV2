namespace Shared.Worker.Options;

public sealed class StaffArrCertificationExpirationOptions
{
    public const string SectionName = "StaffArrCertificationExpiration";

    public bool Enabled { get; set; } = true;

    public string StaffArrBaseUrl { get; set; } = "http://localhost:5102";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 15;

    public int BatchSize { get; set; } = 100;

    public Guid? TenantId { get; set; }
}
