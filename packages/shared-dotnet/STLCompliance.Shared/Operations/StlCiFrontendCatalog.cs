namespace STLCompliance.Shared.Operations;

/// <summary>
/// Main CI frontend build/test jobs declared in <c>.github/workflows/ci.yml</c>.
/// Catalog tests reference this list so product frontend gates stay explicit and auditable (W340).
/// </summary>
public sealed record StlCiFrontendJob(
    string JobId,
    string AppDirectory,
    string PackageLockRelativePath,
    bool RunsBuild,
    bool RunsTest,
    bool IsProductFrontendGate = false);

public static class StlCiFrontendCatalog
{
    public const string MainCiWorkflowRelativePath = ".github/workflows/ci.yml";
    public const string NodeVersion = "22";

    public static readonly StlCiFrontendJob StlComplianceSite =
        new(
            "stlcompliancesite",
            "apps/stlcompliancesite",
            "apps/stlcompliancesite/package-lock.json",
            RunsBuild: true,
            RunsTest: true);

    public static readonly StlCiFrontendJob StlComplianceKb =
        new(
            "stlcompliancekb",
            "apps/stlcompliancekb",
            "apps/stlcompliancekb/package-lock.json",
            RunsBuild: true,
            RunsTest: true);

    public static readonly StlCiFrontendJob SuiteFrontend =
        new(
            "suite-frontend",
            "apps/suite-frontend",
            "apps/suite-frontend/package-lock.json",
            RunsBuild: true,
            RunsTest: true);

    public static readonly StlCiFrontendJob RoutArrFrontend =
        new(
            "routarr-frontend",
            "apps/routarr-frontend",
            "apps/routarr-frontend/package-lock.json",
            RunsBuild: true,
            RunsTest: true,
            IsProductFrontendGate: true);

    public static readonly StlCiFrontendJob StaffArrFrontend =
        new(
            "staffarr-frontend",
            "apps/staffarr-frontend",
            "apps/staffarr-frontend/package-lock.json",
            RunsBuild: true,
            RunsTest: true,
            IsProductFrontendGate: true);

    public static readonly StlCiFrontendJob TrainArrFrontend =
        new(
            "trainarr-frontend",
            "apps/trainarr-frontend",
            "apps/trainarr-frontend/package-lock.json",
            RunsBuild: true,
            RunsTest: true,
            IsProductFrontendGate: true);

    public static readonly StlCiFrontendJob MaintainArrFrontend =
        new(
            "maintainarr-frontend",
            "apps/maintainarr-frontend",
            "apps/maintainarr-frontend/package-lock.json",
            RunsBuild: true,
            RunsTest: true,
            IsProductFrontendGate: true);

    public static readonly StlCiFrontendJob SupplyArrFrontend =
        new(
            "supplyarr-frontend",
            "apps/supplyarr-frontend",
            "apps/supplyarr-frontend/package-lock.json",
            RunsBuild: true,
            RunsTest: true,
            IsProductFrontendGate: true);

    public static readonly StlCiFrontendJob CustomArrFrontend =
        new(
            "customarr-frontend",
            "apps/customarr-frontend",
            "apps/customarr-frontend/package-lock.json",
            RunsBuild: true,
            RunsTest: true,
            IsProductFrontendGate: true);

    public static readonly StlCiFrontendJob OrdArrFrontend =
        new(
            "ordarr-frontend",
            "apps/ordarr-frontend",
            "apps/ordarr-frontend/package-lock.json",
            RunsBuild: true,
            RunsTest: true,
            IsProductFrontendGate: true);

    public static readonly StlCiFrontendJob ComplianceCoreFrontend =
        new(
            "compliancecore-frontend",
            "apps/compliancecore-frontend",
            "apps/compliancecore-frontend/package-lock.json",
            RunsBuild: true,
            RunsTest: true,
            IsProductFrontendGate: true);

    public static readonly StlCiFrontendJob LoadArrFrontend =
        new(
            "loadarr-frontend",
            "apps/loadarr-frontend",
            "apps/loadarr-frontend/package-lock.json",
            RunsBuild: true,
            RunsTest: true,
            IsProductFrontendGate: true);

    public static readonly StlCiFrontendJob ReportArrFrontend =
        new(
            "reportarr-frontend",
            "apps/reportarr-frontend",
            "apps/reportarr-frontend/package-lock.json",
            RunsBuild: true,
            RunsTest: true,
            IsProductFrontendGate: true);

    public static readonly StlCiFrontendJob AssurArrFrontend =
        new(
            "assurarr-frontend",
            "apps/assurarr-frontend",
            "apps/assurarr-frontend/package-lock.json",
            RunsBuild: true,
            RunsTest: true,
            IsProductFrontendGate: true);

    /// <summary>All npm frontend jobs in main CI (marketing site + suite + gated product frontends).</summary>
    public static readonly IReadOnlyList<StlCiFrontendJob> MainCiFrontendJobs =
    [
        StlComplianceSite,
        StlComplianceKb,
        SuiteFrontend,
        RoutArrFrontend,
        StaffArrFrontend,
        TrainArrFrontend,
        MaintainArrFrontend,
        SupplyArrFrontend,
        CustomArrFrontend,
        OrdArrFrontend,
        ComplianceCoreFrontend,
        LoadArrFrontend,
        ReportArrFrontend,
        AssurArrFrontend,
    ];

    /// <summary>Arr product frontends that gate main CI via dedicated build/test jobs (W340+).</summary>
    public static readonly IReadOnlyList<StlCiFrontendJob> GatedProductFrontendJobs =
    [
        RoutArrFrontend,
        StaffArrFrontend,
        TrainArrFrontend,
        MaintainArrFrontend,
        SupplyArrFrontend,
        CustomArrFrontend,
        OrdArrFrontend,
        ComplianceCoreFrontend,
        LoadArrFrontend,
        ReportArrFrontend,
        AssurArrFrontend,
    ];

    public static StlCiFrontendJob? TryGetByJobId(string jobId) =>
        MainCiFrontendJobs.FirstOrDefault(
            job => string.Equals(job.JobId, jobId, StringComparison.OrdinalIgnoreCase));
}
