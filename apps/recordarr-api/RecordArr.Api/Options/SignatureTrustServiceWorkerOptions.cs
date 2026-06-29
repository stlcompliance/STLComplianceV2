namespace RecordArr.Api.Options;

public sealed class SignatureTrustServiceWorkerOptions
{
    public const string SectionName = "SignatureTrustServiceWorker";

    public bool Enabled { get; set; }

    public string[] TenantIds { get; set; } = [];

    public string RequestedByPersonId { get; set; } = "recordarr-signature-trust-worker";

    public string? ManifestPath { get; set; }

    public int PollIntervalSeconds { get; set; } = 300;
}
