namespace NexArr.Api.Options;

public sealed class FieldCompanionProductUrlsOptions
{
    public const string SectionName = "FieldCompanionProducts";

    public string StaffArrBaseUrl { get; set; } = "http://localhost:5102";

    public string TrainArrBaseUrl { get; set; } = "http://localhost:5103";

    public string MaintainArrBaseUrl { get; set; } = "http://localhost:5104";

    public string RoutArrBaseUrl { get; set; } = "http://localhost:5105";

    public string SupplyArrBaseUrl { get; set; } = "http://localhost:5106";
}
