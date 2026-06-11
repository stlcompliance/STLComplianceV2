namespace SupplyArr.Api.Options;

public sealed class RoutArrClientOptions
{
    public const string SectionName = "RoutArr";

    public string BaseUrl { get; set; } = "http://localhost:5180";

    public string ServiceToken { get; set; } = string.Empty;

    public string ShipmentServiceToken { get; set; } = string.Empty;

    public string VendorOrderEventServiceToken { get; set; } = string.Empty;
}
