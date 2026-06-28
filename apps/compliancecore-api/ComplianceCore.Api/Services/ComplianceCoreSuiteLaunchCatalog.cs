using STLCompliance.Shared.Integration;

namespace ComplianceCore.Api.Services;

internal static class ComplianceCoreSuiteLaunchCatalog
{
    public static readonly IReadOnlyList<string> PlatformAdminProductKeys =
    [
        StlProductKeys.NexArr,
        StlProductKeys.StaffArr,
        StlProductKeys.ComplianceCore,
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
