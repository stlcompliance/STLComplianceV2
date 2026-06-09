namespace MaintainArr.Api.Options;

public sealed class ExternalIntelligenceOptions
{
    public const string SectionName = "ExternalIntelligence";

    public bool EnableNhtsa { get; set; } = true;

    public string NhtsaVehicleApiBaseUrl { get; set; } = "https://vpic.nhtsa.dot.gov/api/";

    public string NhtsaSafetyApiBaseUrl { get; set; } = "https://api.nhtsa.gov/";

    public int DecodeCacheMinutes { get; set; } = 720;

    public int RecallCacheMinutes { get; set; } = 180;

    public int ComplaintCacheMinutes { get; set; } = 180;

    public int ReferenceCacheMinutes { get; set; } = 1440;

    public int MaxBatchSize { get; set; } = 50;
}
