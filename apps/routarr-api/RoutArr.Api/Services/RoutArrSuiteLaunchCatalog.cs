using STLCompliance.Shared.Integration;

namespace RoutArr.Api.Services;

internal static class RoutArrSuiteLaunchCatalog
{
    public static IReadOnlyList<string> OrdinaryProductKeys { get; } =
    [
        StlProductKeys.NexArr,
        StlProductKeys.StaffArr,
        StlProductKeys.RecordArr,
        StlProductKeys.MaintainArr,
        StlProductKeys.TrainArr,
        StlProductKeys.SupplyArr,
        StlProductKeys.LoadArr,
        StlProductKeys.AssurArr,
        StlProductKeys.CustomArr,
        StlProductKeys.OrdArr,
        StlProductKeys.RoutArr,
        StlProductKeys.ReportArr,
        StlProductKeys.FieldCompanion,
        StlProductKeys.LedgArr
    ];
}
