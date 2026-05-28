namespace Shared.Worker.Options;

public sealed class MaintainArrDefectEscalationOptions
{
    public const string SectionName = "MaintainArrDefectEscalation";

    public bool Enabled { get; set; } = true;

    public string MaintainArrBaseUrl { get; set; } = "http://localhost:5104";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 30;

    public int BatchSize { get; set; } = 25;

    public Guid? TenantId { get; set; }
}
