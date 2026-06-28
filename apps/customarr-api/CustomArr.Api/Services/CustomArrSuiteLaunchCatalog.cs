using STLCompliance.Shared.Integration;

namespace CustomArr.Api.Services;

internal static class CustomArrSuiteLaunchCatalog
{
    public static readonly IReadOnlyList<string> OrdinaryProductKeys =
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
        StlProductKeys.LedgArr,
    ];
}
