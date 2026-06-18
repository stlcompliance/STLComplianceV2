namespace STLCompliance.Shared.Integration;

public static class StlProductKeys
{
    public const string AssurArr = "assurarr";
    public const string ComplianceCore = "compliancecore";
    public const string CustomArr = "customarr";
    public const string FieldCompanion = "fieldcompanion";
    public const string LedgArr = "ledgarr";
    public const string LoadArr = "loadarr";
    public const string MaintainArr = "maintainarr";
    public const string NexArr = "nexarr";
    public const string OrdArr = "ordarr";
    public const string RecordArr = "recordarr";
    public const string ReferenceDataCore = "referencedatacore";
    public const string ReportArr = "reportarr";
    public const string RoutArr = "routarr";
    public const string StaffArr = "staffarr";
    public const string StlComplianceSite = "stlcompliancesite";
    public const string SupplyArr = "supplyarr";
    public const string TrainArr = "trainarr";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        AssurArr,
        ComplianceCore,
        CustomArr,
        FieldCompanion,
        LedgArr,
        LoadArr,
        MaintainArr,
        NexArr,
        OrdArr,
        RecordArr,
        ReferenceDataCore,
        ReportArr,
        RoutArr,
        StaffArr,
        StlComplianceSite,
        SupplyArr,
        TrainArr,
    };

    public static bool IsCanonical(string productKey) =>
        All.Contains(productKey.Trim());
}
