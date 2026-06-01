namespace STLCompliance.Shared.Operations;

using STLCompliance.Shared.Integration;

/// <summary>
/// Render Blueprint V1 inventory and hardening conventions (W89/W350).
/// CI catalog tests reference this list so deployment wiring stays explicit and auditable.
/// </summary>
public sealed record StlRenderApiService(string Name, string DockerfileRelativePath);

public sealed record StlRenderStaticSite(string Name, string RootDir);

public sealed record StlRenderWorkerService(string Name, string DockerfileRelativePath);

public sealed record StlRenderDatabase(string Name, string DatabaseName);

public sealed record StlRenderEvidenceDisk(string ApiServiceName, string MountPath, int SizeGb);

public static class StlRenderBlueprintCatalog
{
    public const string BlueprintRelativePath = "render.yaml";
    public const string EnvVarsDocRelativePath = "docs/deployment/ENV_VARS_V1.md";
    public const string W89SliceDocRelativePath = "docs/implementation/worker-slices/W89_RENDER_V1_DEPLOYMENT_HARDENING.md";
    public const string ApiHealthCheckPath = "/health/ready";

    public static readonly IReadOnlyList<string> EnvGroupNames =
    [
        "stl-shared",
        "stl-auth",
        "stl-internal-api-urls",
        "stl-public-frontend-urls",
        "stl-vite-product-frontend-urls",
        "stl-public-api-urls",
    ];

    public static readonly IReadOnlyList<StlRenderApiService> ApiServices =
    [
        new("nexarr-api", "./apps/nexarr-api/Dockerfile"),
        new("staffarr-api", "./apps/staffarr-api/Dockerfile"),
        new("trainarr-api", "./apps/trainarr-api/Dockerfile"),
        new("maintainarr-api", "./apps/maintainarr-api/Dockerfile"),
        new("routarr-api", "./apps/routarr-api/Dockerfile"),
        new("supplyarr-api", "./apps/supplyarr-api/Dockerfile"),
        new("compliancecore-api", "./apps/compliancecore-api/Dockerfile"),
    ];

    public static readonly IReadOnlyList<StlRenderStaticSite> StaticSites =
    [
        new("stlcompliancesite", "apps/stlcompliancesite"),
        new("suite-frontend", "apps/suite-frontend"),
        new("staffarr-frontend", "apps/staffarr-frontend"),
        new("trainarr-frontend", "apps/trainarr-frontend"),
        new("maintainarr-frontend", "apps/maintainarr-frontend"),
        new("routarr-frontend", "apps/routarr-frontend"),
        new("supplyarr-frontend", "apps/supplyarr-frontend"),
        new("compliancecore-frontend", "apps/compliancecore-frontend"),
        new("companion-frontend", "apps/companion-frontend"),
    ];

    public static readonly IReadOnlyList<StlRenderWorkerService> WorkerServices =
    [
        new("shared-worker", "./workers/shared-worker/Dockerfile"),
        new("nexarr-worker", "./workers/nexarr-worker/Dockerfile"),
        new("staffarr-worker", "./workers/staffarr-worker/Dockerfile"),
        new("trainarr-worker", "./workers/trainarr-worker/Dockerfile"),
        new("maintainarr-worker", "./workers/maintainarr-worker/Dockerfile"),
        new("routarr-worker", "./workers/routarr-worker/Dockerfile"),
        new("supplyarr-worker", "./workers/supplyarr-worker/Dockerfile"),
        new("compliancecore-worker", "./workers/compliancecore-worker/Dockerfile"),
    ];

    public static readonly IReadOnlyList<StlRenderDatabase> Databases =
    [
        new("nexarr-db", "nexarr"),
        new("staffarr-db", "staffarr"),
        new("trainarr-db", "trainarr"),
        new("maintainarr-db", "maintainarr"),
        new("routarr-db", "routarr"),
        new("supplyarr-db", "supplyarr"),
        new("compliancecore-db", "compliancecore"),
    ];

    public static readonly IReadOnlyList<StlRenderEvidenceDisk> EvidenceDisks =
    [
        new("trainarr-api", "/var/data/trainarr-evidence", 10),
        new("maintainarr-api", "/var/data/maintainarr-evidence", 10),
    ];

    public static readonly IReadOnlyList<string> StaticSecurityHeaderNames =
    [
        "X-Content-Type-Options",
        "X-Frame-Options",
        "Referrer-Policy",
        "Permissions-Policy",
    ];

    public static readonly IReadOnlyList<(string EnvKey, string ApiServiceName, string BaseUrl)> ApiBaseUrlEnvKeys =
    [
        ("NexArr__BaseUrl", "nexarr-api", "https://nexarr-api-jdyi.onrender.com"),
        ("StaffArr__BaseUrl", "staffarr-api", "https://staffarr-api-srdo.onrender.com"),
        ("TrainArr__BaseUrl", "trainarr-api", "https://trainarr-api-ae2t.onrender.com"),
        ("MaintainArr__BaseUrl", "maintainarr-api", "https://maintainarr-api-82rg.onrender.com"),
        ("RoutArr__BaseUrl", "routarr-api", "https://routarr-api-ut1u.onrender.com"),
        ("SupplyArr__BaseUrl", "supplyarr-api", "https://supplyarr-api-r963.onrender.com"),
        ("ComplianceCore__BaseUrl", "compliancecore-api", "https://compliancecore-api-qfu7.onrender.com"),
    ];

    public static IReadOnlyDictionary<string, IReadOnlyList<string>> SyncFalseEnvKeysByConsumer() =>
        StlIntegrationTokenCatalog.All
            .GroupBy(profile => profile.ConsumerService, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(profile => profile.ConfigurationKey)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(key => key, StringComparer.Ordinal)
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<string> ApiServicesRequiringHandoffToken() =>
        ApiServices
            .Select(service => service.Name)
            .Where(name => !string.Equals(name, "nexarr-api", StringComparison.OrdinalIgnoreCase))
            .ToList();
}
