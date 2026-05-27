namespace Shared.Worker.Options;

public sealed class MaintainArrPmDueScanOptions
{
    public const string SectionName = "MaintainArrPmDueScan";

    public bool Enabled { get; set; } = true;

    public string MaintainArrBaseUrl { get; set; } = "http://localhost:5104";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 15;

    public int BatchSize { get; set; } = 100;

    public int OverdueGraceDays { get; set; } = 1;

    public Guid? TenantId { get; set; }
}
