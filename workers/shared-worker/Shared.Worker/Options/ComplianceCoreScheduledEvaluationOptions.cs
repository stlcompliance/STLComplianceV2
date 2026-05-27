namespace Shared.Worker.Options;

public sealed class ComplianceCoreScheduledEvaluationOptions
{
    public const string SectionName = "ComplianceCoreScheduledEvaluation";

    public bool Enabled { get; set; } = true;

    public string ComplianceCoreBaseUrl { get; set; } = "http://localhost:5107";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 60;

    public int BatchSize { get; set; } = 50;

    public int IntervalHours { get; set; } = 24;

    public Guid? TenantId { get; set; }

    public bool EmitFindings { get; set; } = true;
}
