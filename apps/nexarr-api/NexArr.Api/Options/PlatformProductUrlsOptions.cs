namespace NexArr.Api.Options;

public sealed class PlatformProductUrlsOptions
{
    public const string SectionName = "PlatformProducts";

    public string NexArrBaseUrl { get; set; } = "http://localhost:5101";

    public string StaffArrBaseUrl { get; set; } = "http://localhost:5102";

    public string TrainArrBaseUrl { get; set; } = "http://localhost:5103";

    public string MaintainArrBaseUrl { get; set; } = "http://localhost:5104";

    public string RoutArrBaseUrl { get; set; } = "http://localhost:5105";

    public string SupplyArrBaseUrl { get; set; } = "http://localhost:5106";

    public string OrdArrBaseUrl { get; set; } = "http://localhost:5112";

    public string ComplianceCoreBaseUrl { get; set; } = "http://localhost:5107";

    public string LoadArrBaseUrl { get; set; } = "http://localhost:5108";

    public string AssurArrBaseUrl { get; set; } = "http://localhost:5109";

    public string ReportArrBaseUrl { get; set; } = "http://localhost:5111";

    public string RecordArrBaseUrl { get; set; } = "http://localhost:5110";
}
