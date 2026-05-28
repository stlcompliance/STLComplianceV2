namespace SupplyArr.Api.Options;

public sealed class StaffArrClientOptions
{
    public const string SectionName = "StaffArr";

    public string BaseUrl { get; set; } = "http://localhost:5175";

    public string ServiceToken { get; set; } = string.Empty;

    public bool EnforceProcurementApprovalAuthority { get; set; }
}
