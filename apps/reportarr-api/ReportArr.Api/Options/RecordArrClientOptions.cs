namespace ReportArr.Api.Options;

public sealed class RecordArrClientOptions
{
    public const string SectionName = "RecordArr";

    public string BaseUrl { get; set; } = "http://localhost:5104";

    public string ServiceToken { get; set; } = string.Empty;
}
