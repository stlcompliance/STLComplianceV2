namespace MaintainArr.Api.Options;

public sealed class StaffArrClientOptions
{
    public const string SectionName = "StaffArr";

    public string BaseUrl { get; set; } = "http://localhost:5102";

    public string ServiceToken { get; set; } = string.Empty;
}
