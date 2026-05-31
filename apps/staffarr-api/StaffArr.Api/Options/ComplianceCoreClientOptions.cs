namespace StaffArr.Api.Options;

public sealed class ComplianceCoreClientOptions
{
    public const string SectionName = "ComplianceCore";

    public string BaseUrl { get; set; } = "http://localhost:5107";

    public string ServiceToken { get; set; } = string.Empty;

    public string PersonReadinessActionKey { get; set; } = "can-use-person";

    public string? PersonReadinessWorkflowKey { get; set; }

    public string PersonReadinessActivityContextKey { get; set; } = "person_readiness";

    public bool EmitPersonReadinessFindings { get; set; }
}
