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
using SupplyArrRedeemHandoffRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrComplianceReportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _serviceToken = null!;
    private string _userToken = null!;
    private Guid _supplierId;
    private Guid _missingDocsSupplierId;
    private Guid _expiredDocumentId;
    private Guid _pendingDocumentId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ComplianceReportNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"ComplianceReportSupplyArr-{Guid.NewGuid():N}";

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
        _serviceToken = await IssueServiceTokenAsync(adminToken, "supplyarr");
        var handoffCode = await CreateHandoffAsync(adminToken);

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", _serviceToken);
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
        (_supplierId, _missingDocsSupplierId, _expiredDocumentId, _pendingDocumentId) = await SeedComplianceDocumentsAsync();
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Compliance_summary_returns_document_rollups()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/compliance/summary", _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content.ReadFromJsonAsync<SupplierComplianceReportSummaryResponse>();
        Assert.NotNull(summary);
        Assert.Equal(4, summary!.Totals.DocumentCount);
        Assert.Equal(1, summary.Totals.ExpiredCount);
        Assert.Equal(1, summary.Totals.ExpiringSoonCount);
        Assert.Equal(1, summary.Totals.ReviewPendingCount);
        Assert.Equal(2, summary.Totals.ApprovedCount);
        Assert.Contains(summary.Documents, x => x.DocumentId == _expiredDocumentId && x.IsExpired);
        Assert.Contains(summary.Documents, x => x.DocumentId == _pendingDocumentId);
    }

    [Fact]
    public async Task Compliance_attention_filter_excludes_approved_current_docs()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/reports/compliance/summary?attentionOnly=true",
                _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content.ReadFromJsonAsync<SupplierComplianceReportSummaryResponse>();
        Assert.NotNull(summary);
        Assert.Equal(3, summary!.Totals.DocumentCount);
        Assert.DoesNotContain(summary.Documents, x => x.DocumentKey == "W9-CURRENT");
    }

    [Fact]
    public async Task Compliance_supplier_detail_returns_documents()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/reports/compliance/suppliers/{_supplierId}",
                _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var detail = await response.Content.ReadFromJsonAsync<SupplierComplianceDetailResponse>();
        Assert.NotNull(detail);
        Assert.Equal(_supplierId, detail!.Summary.SupplierId);
        Assert.Equal(4, detail.Documents.Count);
    }

    [Fact]
    public async Task Compliance_export_returns_csv()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/compliance/summary/export", _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("supplierKey,supplierDisplayName", csv, StringComparison.Ordinal);
        Assert.Contains("COI-EXPIRED", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Compliance_summary_denied_without_auth()
    {
        var response = await _supplyarrClient.GetAsync("/api/reports/compliance/summary");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Compliance_alerts_include_required_feature_signals()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/compliance/alerts", _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var alerts = await response.Content.ReadFromJsonAsync<List<ComplianceReportAlertResponse>>();
        Assert.NotNull(alerts);
        Assert.Contains(alerts!, x =>
            x.AlertType == "missing_required_documents"
            && x.SupplierId == _missingDocsSupplierId);
        Assert.Contains(alerts!, x => x.AlertType == "expiring_compliance_document");
        Assert.Contains(alerts!, x => x.AlertType == "purchase_approval_missing_evidence");
        Assert.Contains(alerts!, x => x.AlertType == "emergency_purchase_exception");
    }

    private async Task<(Guid SupplierId, Guid MissingDocsSupplierId, Guid ExpiredDocumentId, Guid PendingDocumentId)> SeedComplianceDocumentsAsync()
    {
        await using var scope = _supplyarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var now = DateTimeOffset.UtcNow;

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierKey = "SUP-COMPL",
            
            DisplayName = "Compliance Supplier",
            LegalName = "Compliance Supplier LLC",
            ApprovalStatus = "approved",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var missingDocsSupplier = new Supplier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierKey = "SUP-MISSING",
            
            DisplayName = "Missing Docs Supplier",
            LegalName = "Missing Docs Supplier LLC",
            ApprovalStatus = "approved",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var expiredDocument = new SupplierComplianceDocument
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierId = supplier.Id,
            DocumentKey = "COI-EXPIRED",
            DocumentTypeKey = "certificate_of_insurance",
            Title = "Expired COI",
            Version = 1,
            ReviewStatus = SupplierComplianceDocumentReviewStatuses.Approved,
            ExpiresAt = now.AddDays(-30),
            FileName = "coi.pdf",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var pendingDocument = new SupplierComplianceDocument
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierId = supplier.Id,
            DocumentKey = "W9-PENDING",
            DocumentTypeKey = "w9",
            Title = "Pending W9",
            Version = 1,
            ReviewStatus = SupplierComplianceDocumentReviewStatuses.Pending,
            ExpiresAt = now.AddDays(90),
            FileName = "w9.pdf",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var currentDocument = new SupplierComplianceDocument
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierId = supplier.Id,
            DocumentKey = "W9-CURRENT",
            DocumentTypeKey = "w9",
            Title = "Current W9",
            Version = 2,
            ReviewStatus = SupplierComplianceDocumentReviewStatuses.Approved,
            ExpiresAt = now.AddDays(365),
            FileName = "w9-v2.pdf",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var expiringDocument = new SupplierComplianceDocument
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierId = supplier.Id,
            DocumentKey = "INS-EXPIRING",
            DocumentTypeKey = "insurance_certificate",
            Title = "Expiring insurance",
            Version = 1,
            ReviewStatus = SupplierComplianceDocumentReviewStatuses.Approved,
            ExpiresAt = now.AddDays(10),
            FileName = "insurance.pdf",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var approvedMissingEvidence = new PurchaseRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RequestKey = "pr-missing-evidence",
            Title = "Approved missing evidence",
            Notes = string.Empty,
            Status = PurchaseRequestStatuses.Approved,
            SupplierId = supplier.Id,
            RequestedByUserId = Guid.NewGuid(),
            ApprovedAt = now,
            ApprovedByUserId = null,
            IsEmergency = false,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var emergencyApproved = new PurchaseRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RequestKey = "pr-emergency-exception",
            Title = "Emergency exception PR",
            Notes = string.Empty,
            Status = PurchaseRequestStatuses.Approved,
            SupplierId = supplier.Id,
            RequestedByUserId = Guid.NewGuid(),
            ApprovedAt = now,
            ApprovedByUserId = Guid.NewGuid(),
            IsEmergency = true,
            ManagerOverrideApproved = true,
            ManagerOverrideJustification = "Critical outage",
            EmergencyReason = "Critical outage",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var procurementException = new ProcurementException
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExceptionKey = "PEX-EMERGENCY-1",
            SubjectType = ProcurementExceptionSubjectTypes.PurchaseRequest,
            SubjectId = emergencyApproved.Id,
            SubjectKey = emergencyApproved.RequestKey,
            SupplierId = supplier.Id,
            ExceptionCategory = ProcurementExceptionCategories.PolicyViolation,
            Title = "Emergency purchasing exception",
            Description = "Emergency approval exception pending remediation.",
            Status = ProcurementExceptionStatuses.Open,
            CreatedByUserId = Guid.NewGuid(),
            LinkedPurchaseRequestId = emergencyApproved.Id,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Suppliers.AddRange(supplier, missingDocsSupplier);
        db.SupplierComplianceDocuments.AddRange(expiredDocument, pendingDocument, currentDocument, expiringDocument);
        db.PurchaseRequests.AddRange(approvedMissingEvidence, emergencyApproved);
        db.ProcurementExceptions.Add(procurementException);
        await db.SaveChangesAsync();
        return (supplier.Id, missingDocsSupplier.Id, expiredDocument.Id, pendingDocument.Id);
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
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-compliance-report-test",
            $"{productKey} compliance report test",
            productKey,
            [productKey]));
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
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private async Task<string> CreateHandoffAsync(string token)
    {
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest("supplyarr", "http://localhost:5179/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> RedeemHandoffAsync(string handoffCode)
    {
        var redeemResponse = await _supplyarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new SupplyArrRedeemHandoffRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<SupplyArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
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



