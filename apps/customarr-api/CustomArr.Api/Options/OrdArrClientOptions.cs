namespace CustomArr.Api.Options;

public sealed class OrdArrClientOptions
{
    public const string SectionName = "OrdArr";

    public string BaseUrl { get; set; } = "http://localhost:5112/";
}
