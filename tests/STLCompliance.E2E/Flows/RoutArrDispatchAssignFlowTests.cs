using System.Net;
using System.Net.Http.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Services;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Services;
using STLCompliance.E2E.Support;
using RoutArrRedeemRequest = RoutArr.Api.Contracts.RedeemHandoffRequest;
using RoutArrHandoffSessionResponse = RoutArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.E2E.Flows;

/// <summary>
/// RoutArr trip create → dispatch assignment preview with Compliance Core workflow gates → override assign.
/// </summary>
[Trait("Category", "Integration")]
public sealed class RoutArrDispatchAssignFlowTests : IAsyncLifetime
{
    private E2ENexArrHost _nexarr = null!;
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _routarrClient = null!;

    public async Task InitializeAsync()
    {
        _nexarr = new E2ENexArrHost();
        await _nexarr.InitializeAsync();

        var adminToken = await _nexarr.LoginAsync();
        var routarrHandoffToken = await _nexarr.IssueServiceTokenAsync(adminToken, "routarr", "launch.redeem");
        var routarrToComplianceToken = await _nexarr.IssueServiceTokenAsync(
            adminToken,
            "routarr",
            WorkflowGateService.CheckActionScope,
            ["compliancecore"]);

        var complianceDbName = $"E2E-ComplianceCore-Gate-{Guid.NewGuid():N}";
        _complianceCoreFactory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", E2ENexArrHost.SigningKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(complianceDbName));
            });
        });
        _complianceCoreClient = _complianceCoreFactory.CreateClient();

        using (var scope = _complianceCoreFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
            await db.Database.EnsureCreatedAsync();
            var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
            await vocabularyService.EnsureVocabularyTypesSeededAsync();
        }

        var complianceAdminToken = CreateComplianceCoreAccessToken(["compliancecore"], "compliance_admin");
        await SeedDriverQualificationRulePackAsync(complianceAdminToken);
        var seedRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/workflow-gates/seed/dispatch", complianceAdminToken);
        (await _complianceCoreClient.SendAsync(seedRequest)).EnsureSuccessStatusCode();

        var routArrDbName = $"E2E-RoutArr-Dispatch-{Guid.NewGuid():N}";
        _routarrFactory = new WebApplicationFactory<global::RoutArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarr.Client.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", routarrHandoffToken);
            builder.UseSetting("ComplianceCore:BaseUrl", _complianceCoreClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("ComplianceCore:ServiceToken", routarrToComplianceToken);
            builder.UseSetting("DriverEligibility:CheckTrainArrQualification", "false");
            builder.UseSetting("DriverEligibility:CheckStaffArrReadiness", "false");
            builder.UseSetting("DispatchWorkflowGates:DriverAssignmentGateKeys:0", "dispatch_driver_qualification");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<RoutArrDbContext>(services);
                services.AddDbContext<RoutArrDbContext>(options => options.UseInMemoryDatabase(routArrDbName));
                services.AddHttpClient<global::RoutArr.Api.Services.NexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarr.Factory.Server.CreateHandler());
                services.AddHttpClient<ComplianceCoreWorkflowGateClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreFactory.Server.CreateHandler());
            });
        });
        _routarrClient = _routarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _complianceCoreClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarr.DisposeAsync();
    }

    [Fact]
    public async Task Dispatch_assign_blocked_by_workflow_gate_then_succeeds_with_override()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var driverPersonId = PlatformSeeder.DemoAdminUserId.ToString();
        var now = DateTimeOffset.UtcNow;
        var trip = await CreateTripAsync(dispatcherToken, now.AddHours(2), now.AddHours(6));

        var gateCheckRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/dispatch-workflow-gates/check", dispatcherToken);
        gateCheckRequest.Content = JsonContent.Create(new DispatchWorkflowGateCheckRequest(
            trip.TripId,
            driverPersonId,
            AssignmentKind: "driver"));
        var gateCheckResponse = await _routarrClient.SendAsync(gateCheckRequest);
        gateCheckResponse.EnsureSuccessStatusCode();
        var gateCheck = (await gateCheckResponse.Content.ReadFromJsonAsync<DispatchWorkflowGateCheckResponse>())!;
        Assert.Equal(DispatchWorkflowGateOutcomes.Block, gateCheck.Outcome);
        Assert.True(gateCheck.IsBlocking);

        var assignRequest = HttpTestClient.Authorized(
            HttpMethod.Patch,
            $"/api/trips/{trip.TripId}/assign-driver",
            dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(driverPersonId));
        var blockedAssign = await _routarrClient.SendAsync(assignRequest);
        Assert.Equal(HttpStatusCode.Conflict, blockedAssign.StatusCode);

        var previewRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/dispatch/assignments/preview", dispatcherToken);
        previewRequest.Content = JsonContent.Create(new DispatchAssignmentPreviewRequest(
            trip.TripId,
            "driver",
            driverPersonId,
            null));
        var previewResponse = await _routarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<DispatchAssignmentPreviewResponse>())!;
        Assert.True(preview.HasBlockingConflicts);
        Assert.NotNull(preview.WorkflowGates);
        Assert.Equal(DispatchWorkflowGateOutcomes.Block, preview.WorkflowGates!.Outcome);

        assignRequest = HttpTestClient.Authorized(
            HttpMethod.Patch,
            $"/api/trips/{trip.TripId}/assign-driver",
            dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            driverPersonId,
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: false,
            IgnoreWorkflowGateBlocks: true));
        var assignResponse = await _routarrClient.SendAsync(assignRequest);
        assignResponse.EnsureSuccessStatusCode();
        var assigned = (await assignResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
        Assert.Equal(driverPersonId, assigned.AssignedDriverPersonId);
    }

    private async Task<TripDetailResponse> CreateTripAsync(
        string dispatcherToken,
        DateTimeOffset tripStart,
        DateTimeOffset tripEnd)
    {
        var createTripRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/trips", dispatcherToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "E2E dispatch trip",
            "Cross-product dispatch assign flow",
            null,
            tripStart,
            tripEnd,
            null));
        var createTripResponse = await _routarrClient.SendAsync(createTripRequest);
        createTripResponse.EnsureSuccessStatusCode();
        return (await createTripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
    }

    private async Task<string> RedeemRoutArrTokenAsync()
    {
        var handoffCode = await _nexarr.CreateHandoffAsync("routarr", "http://localhost:5180/launch");
        var redeemResponse = await _routarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new RoutArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<RoutArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task SeedDriverQualificationRulePackAsync(string adminToken)
    {
        var programId = await CreateSampleProgramAsync(adminToken);

        var createPackRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        createPackRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            "driver_qualification",
            "Driver Qualification Rules",
            "Baseline driver qualification rule pack."));
        var createPackResponse = await _complianceCoreClient.SendAsync(createPackRequest);
        createPackResponse.EnsureSuccessStatusCode();
        var pack = (await createPackResponse.Content.ReadFromJsonAsync<RulePackResponse>())!;

        var licenseFactId = await CreateBooleanFactDefinitionAsync(adminToken, "driver_license_valid");

        var licenseSourceRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        licenseSourceRequest.Content = JsonContent.Create(new CreateFactSourceRequest(
            licenseFactId,
            "default_license_flag",
            "static_config",
            "Default license valid",
            "Static default for driver license validity checks.",
            null,
            null,
            """{"booleanValue":false}""",
            0));
        (await _complianceCoreClient.SendAsync(licenseSourceRequest)).EnsureSuccessStatusCode();

        var content = new RulePackContentBody(
            1,
            "all",
            [
                new RuleDefinitionDto("license_valid", "Valid driver license", "fact_boolean", "driver_license_valid", true),
            ]);

        var updateRequest = HttpTestClient.Authorized(HttpMethod.Put, $"/api/rule-packs/{pack.RulePackId}/content", adminToken);
        updateRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        (await _complianceCoreClient.SendAsync(updateRequest)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateBooleanFactDefinitionAsync(string adminToken, string factKey)
    {
        var request = HttpTestClient.Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            factKey,
            factKey.Replace('_', ' '),
            "Test fact for dispatch workflow gates.",
            "boolean"));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<FactDefinitionResponse>())!;
        return created.FactDefinitionId;
    }

    private async Task<Guid> CreateSampleProgramAsync(string adminToken)
    {
        var bodyRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "dot",
            "U.S. Department of Transportation",
            "Federal transportation safety and compliance authority."));
        var body = (await (await _complianceCoreClient.SendAsync(bodyRequest)).Content.ReadFromJsonAsync<GoverningBodyResponse>())!;

        var jurisdictionRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
        jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
            body.GoverningBodyId,
            "us_federal",
            "United States Federal",
            "Federal jurisdiction."));
        var jurisdiction = (await (await _complianceCoreClient.SendAsync(jurisdictionRequest)).Content.ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "fmcsa_safety",
            "FMCSA Safety Compliance",
            "Federal motor carrier safety compliance program."));
        var program = (await (await _complianceCoreClient.SendAsync(programRequest)).Content.ReadFromJsonAsync<RegulatoryProgramResponse>())!;
        return program.RegulatoryProgramId;
    }

    private string CreateComplianceCoreAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey)
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ComplianceCoreTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "E2E Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return accessToken;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
            .ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
