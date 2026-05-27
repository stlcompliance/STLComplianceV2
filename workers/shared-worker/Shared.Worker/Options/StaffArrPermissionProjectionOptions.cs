namespace Shared.Worker.Options;

public sealed class StaffArrPermissionProjectionOptions
{
    public const string SectionName = "StaffArrPermissionProjection";

    public bool Enabled { get; set; } = true;

    public string StaffArrBaseUrl { get; set; } = "http://localhost:5102";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 30;

    public int BatchSize { get; set; } = 100;

    public int StalenessHours { get; set; } = 1;

    public Guid? TenantId { get; set; }
}
