namespace RecordArr.Api.Options;

public sealed class BackupVerificationWorkerOptions
{
    public const string SectionName = "BackupVerificationWorker";

    public bool Enabled { get; set; }

    public string[] TenantIds { get; set; } = [];

    public string RequestedByPersonId { get; set; } = "recordarr-backup-verification-worker";

    public string? ManifestPath { get; set; }

    public int DefaultRpoTargetMinutes { get; set; } = 60;

    public int PollIntervalSeconds { get; set; } = 3600;
}
