using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;
using STLCompliance.Shared.Operations;

namespace STLCompliance.E2E.Catalog;

[Trait("Category", "Ci")]
[Trait("Area", "RenderBlueprint")]
public sealed class StlRenderBlueprintCatalogTests
{
    [Fact]
    public void Blueprint_catalog_lists_seven_apis_eight_workers_nine_static_sites_and_seven_databases()
    {
        Assert.Equal(7, StlRenderBlueprintCatalog.ApiServices.Count);
        Assert.Equal(8, StlRenderBlueprintCatalog.WorkerServices.Count);
        Assert.Equal(9, StlRenderBlueprintCatalog.StaticSites.Count);
        Assert.Equal(7, StlRenderBlueprintCatalog.Databases.Count);
        Assert.Equal(6, StlRenderBlueprintCatalog.EnvGroupNames.Count);
        Assert.Equal(2, StlRenderBlueprintCatalog.EvidenceDisks.Count);
    }

    [Fact]
    public void Api_url_env_keys_map_to_deployed_render_base_urls()
    {
        Assert.Equal(7, StlRenderBlueprintCatalog.ApiBaseUrlEnvKeys.Count);

        foreach (var (envKey, apiServiceName, baseUrl) in StlRenderBlueprintCatalog.ApiBaseUrlEnvKeys)
        {
            Assert.StartsWith("https://", baseUrl, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith(".onrender.com", baseUrl, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(apiServiceName, baseUrl, StringComparison.OrdinalIgnoreCase);
            Assert.False(string.IsNullOrWhiteSpace(envKey));
        }
    }

    [Fact]
    public void Sync_false_env_keys_by_consumer_cover_integration_token_catalog()
    {
        var syncFalseByConsumer = StlRenderBlueprintCatalog.SyncFalseEnvKeysByConsumer();

        foreach (var profile in StlIntegrationTokenCatalog.All)
        {
            Assert.True(syncFalseByConsumer.ContainsKey(profile.ConsumerService));
            Assert.Contains(profile.ConfigurationKey, syncFalseByConsumer[profile.ConsumerService]);
        }
    }

    [Fact]
    public void Nexarr_worker_auto_provisioning_can_mint_platform_outbox_token_without_http_bootstrap()
    {
        const string signingKey = "test-service-token-signing-key-32chars";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [StlIntegrationTokenProvisioner.AutoProvisionConfigurationKey] = "true",
                ["STL_SERVICE_NAME"] = "nexarr-worker",
                ["SERVICE_TOKEN_SIGNING_KEY"] = signingKey,
                ["SERVICE_TOKEN_ISSUER"] = "stl-compliance-services",
                ["SERVICE_TOKEN_AUDIENCE"] = "stl-compliance-services",
            })
            .Build();

        var tokens = StlIntegrationTokenProvisioner.ProvisionSynchronously(configuration);

        Assert.True(tokens.TryGetValue("NexArrPlatformOutboxPublisher__ServiceToken", out var token));

        var validator = new StlServiceTokenValidator(
            configuration,
            Options.Create(new StlServiceTokenOptions()));
        var validated = validator.ValidateOrThrow(
            token,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "nexarr-worker",
                RequiredTargetProduct = "nexarr",
                TenantId = Guid.Empty,
                RequiredActionScope = "nexarr.platform_outbox.publish",
            });

        Assert.Equal("nexarr-worker", validated.SourceProductKey);
        Assert.Contains("nexarr", validated.AllowedProductKeys);
        Assert.Equal("nexarr.platform_outbox.publish", validated.ActionScope);
    }

    [Fact]
    public void Render_yaml_declares_blueprint_inventory_health_checks_private_urls_and_sync_false_tokens()
    {
        var repoRoot = FindRepoRoot();
        var blueprintPath = Path.Combine(repoRoot, StlRenderBlueprintCatalog.BlueprintRelativePath);
        Assert.True(File.Exists(blueprintPath), $"Missing Blueprint at {blueprintPath}.");

        var yaml = File.ReadAllText(blueprintPath);
        var envVarsDocPath = Path.Combine(repoRoot, StlRenderBlueprintCatalog.EnvVarsDocRelativePath);
        Assert.True(File.Exists(envVarsDocPath), $"Missing env var doc at {envVarsDocPath}.");

        foreach (var groupName in StlRenderBlueprintCatalog.EnvGroupNames)
        {
            Assert.Contains($"- name: {groupName}", yaml, StringComparison.Ordinal);
        }

        foreach (var database in StlRenderBlueprintCatalog.Databases)
        {
            Assert.Contains($"- name: {database.Name}", yaml, StringComparison.Ordinal);
            Assert.Contains($"databaseName: {database.DatabaseName}", yaml, StringComparison.Ordinal);
        }

        foreach (var api in StlRenderBlueprintCatalog.ApiServices)
        {
            Assert.Contains($"name: {api.Name}", yaml, StringComparison.Ordinal);
            Assert.Contains(api.DockerfileRelativePath, yaml, StringComparison.Ordinal);

            var block = ExtractServiceBlock(yaml, api.Name);
            Assert.Contains($"healthCheckPath: {StlRenderBlueprintCatalog.ApiHealthCheckPath}", block, StringComparison.Ordinal);
        }

        foreach (var worker in StlRenderBlueprintCatalog.WorkerServices)
        {
            Assert.Contains($"name: {worker.Name}", yaml, StringComparison.Ordinal);
            Assert.Contains(worker.DockerfileRelativePath, yaml, StringComparison.Ordinal);
        }

        foreach (var site in StlRenderBlueprintCatalog.StaticSites)
        {
            Assert.Contains($"name: {site.Name}", yaml, StringComparison.Ordinal);
            Assert.Contains($"rootDir: {site.RootDir}", yaml, StringComparison.Ordinal);

            var block = ExtractServiceBlock(yaml, site.Name);
            foreach (var headerName in StlRenderBlueprintCatalog.StaticSecurityHeaderNames)
            {
                Assert.Contains($"name: {headerName}", block, StringComparison.Ordinal);
            }
        }

        foreach (var (envKey, _, baseUrl) in StlRenderBlueprintCatalog.ApiBaseUrlEnvKeys)
        {
            Assert.Contains($"- key: {envKey}", yaml, StringComparison.Ordinal);
            Assert.Contains($"value: {baseUrl}", yaml, StringComparison.Ordinal);
        }

        foreach (var disk in StlRenderBlueprintCatalog.EvidenceDisks)
        {
            var block = ExtractServiceBlock(yaml, disk.ApiServiceName);
            Assert.Contains($"mountPath: {disk.MountPath}", block, StringComparison.Ordinal);
            Assert.Contains($"sizeGB: {disk.SizeGb}", block, StringComparison.Ordinal);
        }

        foreach (var (consumerService, envKeys) in StlRenderBlueprintCatalog.SyncFalseEnvKeysByConsumer())
        {
            var block = ExtractServiceBlock(yaml, consumerService);
            foreach (var envKey in envKeys)
            {
                Assert.Contains($"- key: {envKey}", block, StringComparison.Ordinal);
                Assert.Contains("sync: false", block, StringComparison.Ordinal);
            }
        }

        Assert.Contains("name: redis", yaml, StringComparison.Ordinal);
        Assert.Contains("STL_INTEGRATION_TOKEN_AUTO_PROVISION", yaml, StringComparison.Ordinal);
        Assert.Contains("STL_INTEGRATION_BOOTSTRAP_SECRET", yaml, StringComparison.Ordinal);
    }

    [Fact]
    public void Main_ci_workflow_runs_render_blueprint_catalog_checks()
    {
        var repoRoot = FindRepoRoot();
        var workflowPath = Path.Combine(repoRoot, ".github/workflows/ci.yml");
        Assert.True(File.Exists(workflowPath), $"Missing CI workflow at {workflowPath}.");

        var workflow = File.ReadAllText(workflowPath);
        Assert.Contains("Render blueprint catalog checks", workflow, StringComparison.Ordinal);
        Assert.Contains("Category=Ci&Area=RenderBlueprint", workflow, StringComparison.Ordinal);
    }

    private static string ExtractServiceBlock(string yaml, string serviceName)
    {
        var marker = $"name: {serviceName}";
        var start = yaml.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Service '{serviceName}' not found in render.yaml.");

        var lineStart = yaml.LastIndexOf('\n', start) + 1;
        var indent = start - lineStart;

        var nextServiceIndex = yaml.IndexOf("\n  - type:", start + marker.Length, StringComparison.Ordinal);
        if (nextServiceIndex < 0)
        {
            nextServiceIndex = yaml.IndexOf("\nenvVarGroups:", start + marker.Length, StringComparison.Ordinal);
        }

        if (nextServiceIndex < 0)
        {
            nextServiceIndex = yaml.Length;
        }

        var block = yaml[lineStart..nextServiceIndex];
        Assert.StartsWith(new string(' ', indent), block, StringComparison.Ordinal);
        return block;
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "STLCompliance.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root from test output directory.");
    }
}
