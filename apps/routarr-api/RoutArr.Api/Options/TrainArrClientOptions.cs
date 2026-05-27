namespace RoutArr.Api.Options;

public sealed class TrainArrClientOptions
{
    public const string SectionName = "TrainArr";

    public string BaseUrl { get; set; } = "http://localhost:5103";

    public string ServiceToken { get; set; } = string.Empty;
}
