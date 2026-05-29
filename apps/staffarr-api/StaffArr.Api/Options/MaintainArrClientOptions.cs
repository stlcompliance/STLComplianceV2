namespace StaffArr.Api.Options;

public sealed class MaintainArrClientOptions
{
    public const string SectionName = "MaintainArr";

    public string BaseUrl { get; set; } = "http://localhost:5104";

    public string ServiceToken { get; set; } = string.Empty;
}
