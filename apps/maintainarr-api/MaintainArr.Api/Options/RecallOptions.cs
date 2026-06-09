namespace MaintainArr.Api.Options;

public sealed class RecallOptions
{
    public const string SectionName = "Recall";

    public bool Enabled { get; set; } = true;

    public bool NhtsaEnabled { get; set; } = true;

    public bool TransportCanadaEnabled { get; set; } = false;

    public bool AutoCreateWorkOrder { get; set; } = false;

    public bool AutoCreateReadinessHold { get; set; } = false;

    public bool RequireEvidenceForCompletedVerified { get; set; } = true;

    public bool ParkItAutoHold { get; set; } = true;

    public bool ParkOutsideAutoHold { get; set; } = true;

    public int DefaultRecheckDays { get; set; } = 30;

    public int NhtsaCacheTtlHours { get; set; } = 24;

    public int NhtsaTimeoutMs { get; set; } = 8000;

    public int NhtsaMaxRetries { get; set; } = 2;

    public int NhtsaRateLimitPerMinute { get; set; } = 60;
}
