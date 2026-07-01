using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;
using CreateSupplierRequest = SupplyArr.Api.Contracts.CreateSupplierRequest;
using SupplierResponse = SupplyArr.Api.Contracts.SupplierResponse;
using SupplyArrRedeemHandoffRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrProcurementExceptionTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _userToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ProcurementExceptionNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"ProcurementExceptionSupplyArr-{Guid.NewGuid():N}";

        _nexarrFactory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(nexArrDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        await SeedNexArrAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var handoffToken = await IssueHandoffServiceTokenAsync(adminToken);
        var handoffCode = await CreateHandoffAsync(adminToken);

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", handoffToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(supplyArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
            });
        });

        _supplyarrClient = _supplyarrFactory.CreateClient();
        _userToken = await RedeemHandoffAsync(handoffCode);
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Procurement_exception_workflow_on_purchase_request_with_waive_approval()
    {
        var supplier = await CreateSupplierAsync();
        var part = await CreatePartAsync();

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", _userToken);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            $"pex-pr-{Guid.NewGuid():N}"[..20],
            "Exception subject PR",
            string.Empty,
            supplier.SupplierId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 2m, string.Empty)]));
        var prResponse = await _supplyarrClient.SendAsync(createPrRequest);
        prResponse.EnsureSuccessStatusCode();
        var pr = (await prResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var createExceptionRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{pr.PurchaseRequestId}/procurement-exceptions",
            _userToken);
        createExceptionRequest.Content = JsonContent.Create(new CreateProcurementExceptionRequest(
            "PEX-APPROVAL-001",
            ProcurementExceptionCategories.ApprovalDelay,
            "Stuck in approval queue",
            "PR exceeded SLA for manager approval.",
            null));
        var createExceptionResponse = await _supplyarrClient.SendAsync(createExceptionRequest);
        createExceptionResponse.EnsureSuccessStatusCode();
        var exception = (await createExceptionResponse.Content.ReadFromJsonAsync<ProcurementExceptionResponse>())!;
        Assert.Equal(ProcurementExceptionStatuses.Open, exception.Status);
        Assert.Equal(ProcurementExceptionSubjectTypes.PurchaseRequest, exception.SubjectType);
        Assert.Equal(pr.PurchaseRequestId, exception.SubjectId);
        Assert.Equal(supplier.SupplierId, exception.SupplierId);
        Assert.Equal(supplier.SupplierKey, exception.SupplierKey);
        Assert.Equal(supplier.DisplayName, exception.SupplierDisplayName);
        Assert.Equal("identity", exception.SupplierUnitKind);
        Assert.Contains("parts", exception.SupplierServiceTypes);

        var investigateResponse = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                $"/api/procurement-exceptions/{exception.ExceptionId}/start-investigation",
                _userToken));
        investigateResponse.EnsureSuccessStatusCode();
        var investigating = (await investigateResponse.Content.ReadFromJsonAsync<ProcurementExceptionResponse>())!;
        Assert.Equal(ProcurementExceptionStatuses.Investigating, investigating.Status);

        var waiveRequest = Authorized(
            HttpMethod.Post,
            $"/api/procurement-exceptions/{exception.ExceptionId}/request-waive",
            _userToken);
        waiveRequest.Content = JsonContent.Create(new RequestProcurementExceptionWaiveRequest(
            "Emergency maintenance parts; approval SLA waived per operations policy."));
        var waivePendingResponse = await _supplyarrClient.SendAsync(waiveRequest);
        waivePendingResponse.EnsureSuccessStatusCode();
        var waivePending = (await waivePendingResponse.Content.ReadFromJsonAsync<ProcurementExceptionResponse>())!;
        Assert.Equal(ProcurementExceptionStatuses.WaivePending, waivePending.Status);

        var approveWaiveResponse = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                $"/api/procurement-exceptions/{exception.ExceptionId}/approve-waive",
                _userToken));
        approveWaiveResponse.EnsureSuccessStatusCode();
        var waived = (await approveWaiveResponse.Content.ReadFromJsonAsync<ProcurementExceptionResponse>())!;
        Assert.Equal(ProcurementExceptionStatuses.Waived, waived.Status);
        Assert.NotNull(waived.WaivedAt);

        var closeRequest = Authorized(
            HttpMethod.Post,
            $"/api/procurement-exceptions/{exception.ExceptionId}/close",
            _userToken);
        closeRequest.Content = JsonContent.Create(new CloseProcurementExceptionRequest(null));
        var closeResponse = await _supplyarrClient.SendAsync(closeRequest);
        closeResponse.EnsureSuccessStatusCode();
        var closed = (await closeResponse.Content.ReadFromJsonAsync<ProcurementExceptionResponse>())!;
        Assert.Equal(ProcurementExceptionStatuses.Closed, closed.Status);
    }

    [Fact]
    public async Task Procurement_exception_resolve_and_cancel_paths()
    {
        var supplier = await CreateSupplierAsync();
        var part = await CreatePartAsync();

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", _userToken);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            $"pex-pr2-{Guid.NewGuid():N}"[..20],
            "Resolve path PR",
            string.Empty,
            supplier.SupplierId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 1m, string.Empty)]));
        var pr = (await (await _supplyarrClient.SendAsync(createPrRequest)).Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var createResolve = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{pr.PurchaseRequestId}/procurement-exceptions",
            _userToken);
        createResolve.Content = JsonContent.Create(new CreateProcurementExceptionRequest(
            "PEX-RESOLVE-001",
            ProcurementExceptionCategories.PricingVariance,
            "Quote mismatch",
            "Supplier quote differed from catalog price.",
            null));
        (await _supplyarrClient.SendAsync(createResolve)).EnsureSuccessStatusCode();

        var listResponse = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/purchase-requests/{pr.PurchaseRequestId}/procurement-exceptions",
                _userToken));
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<List<ProcurementExceptionResponse>>())!;
        var resolveTarget = listed.Single(x => x.ExceptionKey == "PEX-RESOLVE-001");

        (await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                $"/api/procurement-exceptions/{resolveTarget.ExceptionId}/start-investigation",
                _userToken))).EnsureSuccessStatusCode();

        var resolveRequest = Authorized(
            HttpMethod.Post,
            $"/api/procurement-exceptions/{resolveTarget.ExceptionId}/resolve",
            _userToken);
        resolveRequest.Content = JsonContent.Create(new ResolveProcurementExceptionRequest(
            "Supplier issued revised quote matching catalog."));
        (await _supplyarrClient.SendAsync(resolveRequest)).EnsureSuccessStatusCode();

        var createCancel = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{pr.PurchaseRequestId}/procurement-exceptions",
            _userToken);
        createCancel.Content = JsonContent.Create(new CreateProcurementExceptionRequest(
            "PEX-CANCEL-001",
            ProcurementExceptionCategories.Other,
            "Opened in error",
            "Duplicate exception logged.",
            null));
        var cancelCreated = (await (await _supplyarrClient.SendAsync(createCancel)).Content.ReadFromJsonAsync<ProcurementExceptionResponse>())!;

        var cancelRequest = Authorized(
            HttpMethod.Post,
            $"/api/procurement-exceptions/{cancelCreated.ExceptionId}/cancel",
            _userToken);
        cancelRequest.Content = JsonContent.Create(new CancelProcurementExceptionRequest("Logged against wrong PR line."));
        var cancelResponse = await _supplyarrClient.SendAsync(cancelRequest);
        cancelResponse.EnsureSuccessStatusCode();
        var cancelled = (await cancelResponse.Content.ReadFromJsonAsync<ProcurementExceptionResponse>())!;
        Assert.Equal(ProcurementExceptionStatuses.Cancelled, cancelled.Status);
    }

    [Fact]
    public async Task Procurement_exception_cancel_then_reopen_resumes_investigation()
    {
        var supplier = await CreateSupplierAsync();
        var part = await CreatePartAsync();

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", _userToken);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            $"pex-reopen-{Guid.NewGuid():N}"[..20],
            "Reopen path PR",
            string.Empty,
            supplier.SupplierId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 1m, string.Empty)]));
        var pr = (await (await _supplyarrClient.SendAsync(createPrRequest)).Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var createExceptionRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{pr.PurchaseRequestId}/procurement-exceptions",
            _userToken);
        createExceptionRequest.Content = JsonContent.Create(new CreateProcurementExceptionRequest(
            "PEX-REOPEN-001",
            ProcurementExceptionCategories.Other,
            "Cancelled then reopened",
            "Exception cancelled in error.",
            null));
        var created = (await (await _supplyarrClient.SendAsync(createExceptionRequest))
            .Content.ReadFromJsonAsync<ProcurementExceptionResponse>())!;

        (await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                $"/api/procurement-exceptions/{created.ExceptionId}/start-investigation",
                _userToken))).EnsureSuccessStatusCode();

        var cancelRequest = Authorized(
            HttpMethod.Post,
            $"/api/procurement-exceptions/{created.ExceptionId}/cancel",
            _userToken);
        cancelRequest.Content = JsonContent.Create(new CancelProcurementExceptionRequest("Opened against wrong subject."));
        (await _supplyarrClient.SendAsync(cancelRequest)).EnsureSuccessStatusCode();

        var reopenRequest = Authorized(
            HttpMethod.Post,
            $"/api/procurement-exceptions/{created.ExceptionId}/reopen",
            _userToken);
        reopenRequest.Content = JsonContent.Create(new ReopenProcurementExceptionRequest(
            "Corrected subject linkage; resume investigation."));
        var reopenResponse = await _supplyarrClient.SendAsync(reopenRequest);
        reopenResponse.EnsureSuccessStatusCode();
        var reopened = (await reopenResponse.Content.ReadFromJsonAsync<ProcurementExceptionResponse>())!;
        Assert.Equal(ProcurementExceptionStatuses.Investigating, reopened.Status);
        Assert.Equal(1, reopened.ReopenCount);
        Assert.Contains("Corrected subject", reopened.LastReopenReason, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(reopened.ReopenedAt);
        Assert.NotNull(reopened.SlaDueAt);
        Assert.Equal("Opened against wrong subject.", reopened.CancellationReason);

        var invalidReopen = Authorized(
            HttpMethod.Post,
            $"/api/procurement-exceptions/{created.ExceptionId}/reopen",
            _userToken);
        invalidReopen.Content = JsonContent.Create(new ReopenProcurementExceptionRequest(
            "Second reopen should fail while investigating."));
        var invalidResponse = await _supplyarrClient.SendAsync(invalidReopen);
        Assert.Equal(HttpStatusCode.Conflict, invalidResponse.StatusCode);
    }

    [Fact]
    public async Task Procurement_exception_resolution_depth_assign_sla_template_and_links()
    {
        var supplier = await CreateSupplierAsync();
        var part = await CreatePartAsync();

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", _userToken);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            $"pex-depth-{Guid.NewGuid():N}"[..20],
            "Depth PR",
            string.Empty,
            supplier.SupplierId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 1m, string.Empty)]));
        var pr = (await (await _supplyarrClient.SendAsync(createPrRequest)).Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var createExceptionRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{pr.PurchaseRequestId}/procurement-exceptions",
            _userToken);
        createExceptionRequest.Content = JsonContent.Create(new CreateProcurementExceptionRequest(
            "PEX-DEPTH-001",
            ProcurementExceptionCategories.ApprovalDelay,
            "Approval queue stuck",
            "PR blocked in approval longer than SLA.",
            PlatformSeeder.DemoAdminUserId,
            null));
        var created = (await (await _supplyarrClient.SendAsync(createExceptionRequest))
            .Content.ReadFromJsonAsync<ProcurementExceptionResponse>())!;
        Assert.NotNull(created.SlaDueAt);
        Assert.Equal(PlatformSeeder.DemoAdminUserId, created.AssignedToUserId);

        var templatesResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/procurement-exceptions/resolution-templates", _userToken));
        templatesResponse.EnsureSuccessStatusCode();
        var templates = (await templatesResponse.Content.ReadFromJsonAsync<List<ProcurementExceptionResolutionTemplateResponse>>())!;
        Assert.Contains(templates, x => x.TemplateKey == ProcurementExceptionResolutionTemplates.PrResubmit);

        var linkRequest = Authorized(
            HttpMethod.Put,
            $"/api/procurement-exceptions/{created.ExceptionId}/link-actions",
            _userToken);
        linkRequest.Content = JsonContent.Create(new LinkProcurementExceptionActionsRequest(
            pr.PurchaseRequestId,
            null));
        var linked = (await (await _supplyarrClient.SendAsync(linkRequest)).Content.ReadFromJsonAsync<ProcurementExceptionResponse>())!;
        Assert.Equal(pr.RequestKey, linked.LinkedPurchaseRequestKey);

        (await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                $"/api/procurement-exceptions/{created.ExceptionId}/start-investigation",
                _userToken))).EnsureSuccessStatusCode();

        var resolveRequest = Authorized(
            HttpMethod.Post,
            $"/api/procurement-exceptions/{created.ExceptionId}/resolve",
            _userToken);
        resolveRequest.Content = JsonContent.Create(new ResolveProcurementExceptionRequest(
            "Resubmitted with corrected budget code.",
            ProcurementExceptionResolutionTemplates.PrResubmit));
        var resolved = (await (await _supplyarrClient.SendAsync(resolveRequest)).Content.ReadFromJsonAsync<ProcurementExceptionResponse>())!;
        Assert.Equal(ProcurementExceptionStatuses.Resolved, resolved.Status);
        Assert.Equal(ProcurementExceptionResolutionTemplates.PrResubmit, resolved.ResolutionTemplateKey);
        Assert.Contains("PR resubmit", resolved.ResolutionNotes, StringComparison.OrdinalIgnoreCase);

        var overdueResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/procurement-exceptions?overdueOnly=true", _userToken));
        overdueResponse.EnsureSuccessStatusCode();
        var overdue = (await overdueResponse.Content.ReadFromJsonAsync<List<ProcurementExceptionResponse>>())!;
        Assert.DoesNotContain(overdue, x => x.ExceptionId == created.ExceptionId);
    }

    [Fact]
    public async Task Create_procurement_exception_enqueues_outbox_event()
    {
        var supplier = await CreateSupplierAsync();
        var part = await CreatePartAsync();

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", _userToken);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            $"pex-out-{Guid.NewGuid():N}"[..20],
            "Outbox PR",
            string.Empty,
            supplier.SupplierId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 1m, string.Empty)]));
        var pr = (await (await _supplyarrClient.SendAsync(createPrRequest)).Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var createExceptionRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{pr.PurchaseRequestId}/procurement-exceptions",
            _userToken);
        createExceptionRequest.Content = JsonContent.Create(new CreateProcurementExceptionRequest(
            "PEX-OUTBOX-001",
            ProcurementExceptionCategories.PolicyViolation,
            "Policy check failed",
            "Missing budget code on PR header.",
            null));
        (await _supplyarrClient.SendAsync(createExceptionRequest)).EnsureSuccessStatusCode();

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var outbox = await db.IntegrationOutboxEvents
            .Where(x => x.EventKind == IntegrationOutboxEventKinds.ProcurementExceptionCreated)
            .ToListAsync();
        Assert.NotEmpty(outbox);
    }

    private async Task<SupplierResponse> CreateSupplierAsync()
    {
        var createSupplier = Authorized(HttpMethod.Post, "/api/suppliers", _userToken);
        createSupplier.Content = JsonContent.Create(new CreateSupplierRequest(
            $"v-pex-{Guid.NewGuid():N}"[..12],
            null,
            null,
            "Exception Supplier",
            string.Empty,
            null,
            string.Empty,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var response = await _supplyarrClient.SendAsync(createSupplier);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SupplierResponse>())!;
    }

    private async Task<PartResponse> CreatePartAsync()
    {
        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", _userToken);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"pex-part-{Guid.NewGuid():N}"[..20],
            null,
            "Exception Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var response = await _supplyarrClient.SendAsync(createPartRequest);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PartResponse>())!;
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

    private async Task SeedNexArrAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!.AccessToken;
    }

    private async Task<string> IssueHandoffServiceTokenAsync(string adminToken)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"supplyarr-pex-handoff-{Guid.NewGuid():N}",
            "supplyarr procurement exception handoff test",
            "supplyarr",
            ["supplyarr"]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "launch.redeem",
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        return (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!.AccessToken;
    }

    private async Task<string> CreateHandoffAsync(string adminToken)
    {
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", adminToken);
        request.Content = JsonContent.Create(new CreateHandoffRequest("supplyarr", "http://localhost:5179/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!.HandoffCode;
    }

    private async Task<string> RedeemHandoffAsync(string handoffCode)
    {
        var redeemResponse = await _supplyarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new SupplyArrRedeemHandoffRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        return (await redeemResponse.Content.ReadFromJsonAsync<SupplyArrHandoffSessionResponse>())!.AccessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
