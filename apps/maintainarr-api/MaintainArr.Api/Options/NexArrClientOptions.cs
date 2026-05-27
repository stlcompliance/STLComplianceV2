namespace MaintainArr.Api.Options;

public sealed class NexArrClientOptions
{
    public const string SectionName = "NexArr";

    public string BaseUrl { get; set; } = "http://localhost:5101";
}

public sealed class HandoffOptions
{
    public const string SectionName = "Handoff";

    public string ServiceToken { get; set; } = string.Empty;
}
