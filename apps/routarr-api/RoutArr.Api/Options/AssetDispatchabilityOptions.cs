namespace RoutArr.Api.Options;

public sealed class AssetDispatchabilityOptions
{
    public const string SectionName = "AssetDispatchability";

    public bool CheckMaintainArrReadiness { get; set; } = true;
}
