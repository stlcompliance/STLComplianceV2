using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using AssurArr.Api.Contracts;

namespace STLCompliance.AssurArr.Api.Tests;

public sealed class AssurArrApiTests(WebApplicationFactory<global::AssurArr.Api.Program> factory)
    : IClassFixture<WebApplicationFactory<global::AssurArr.Api.Program>>
{
    private readonly HttpClient _client = factory
        .WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
        })
        .CreateClient();

    [Fact]
    public async Task Dashboard_includes_seeded_quality_counts()
    {
        var response = await _client.GetAsync("/api/v1/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dashboard = await response.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(dashboard);
        Assert.Contains(dashboard!.Cards, card => card.Key == "nonconformances" && card.Count >= 1);
        Assert.Contains(dashboard.Cards, card => card.Key == "holds" && card.Count >= 1);
    }

    [Fact]
    public async Task Can_create_and_list_nonconformance_records()
    {
        var title = $"Test nonconformance {Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/nonconformances",
            new CreateAssurArrNonconformanceRequest(
                title,
                "Created from automated test coverage.",
                "high",
                "receiving",
                "failed_inspection",
                "loadarr",
                "loadarr:receiving:test",
                ["loadarr:inventory:test"],
                null,
                null,
                null,
                null,
                null,
                false,
                null,
                DateTimeOffset.UtcNow.AddDays(2)));

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<AssurArrNonconformanceResponse>();
        Assert.NotNull(created);
        Assert.Equal(title, created!.Title);

        var listResponse = await _client.GetAsync("/api/v1/nonconformances");
        listResponse.EnsureSuccessStatusCode();

        var list = await listResponse.Content.ReadFromJsonAsync<List<AssurArrNonconformanceResponse>>();
        Assert.NotNull(list);
        Assert.Contains(list!, item => item.Title == title);
    }

    [Fact]
    public async Task Can_create_quality_review_and_release_records()
    {
        var reviewTitle = $"Test quality review {Guid.NewGuid():N}";
        var reviewResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/quality-reviews",
            new CreateAssurArrQualityReviewRequest(
                reviewTitle,
                "Automated coverage for the quality review workflow.",
                "moderate",
                "hold_release",
                "assurarr",
                "HOLD-000001",
                ["loadarr:inventory:test"],
                null,
                "HOLD-000001",
                null,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddDays(2),
                "Review evidence before release.",
                ["recordarr:doc:test"],
                ["recordarr:doc:test"],
                "Review notes"));

        Assert.Equal(HttpStatusCode.OK, reviewResponse.StatusCode);

        var review = await reviewResponse.Content.ReadFromJsonAsync<AssurArrQualityReviewResponse>();
        Assert.NotNull(review);
        Assert.Equal(reviewTitle, review!.Title);

        var releaseTitle = $"Test quality release {Guid.NewGuid():N}";
        var releaseResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/quality-releases",
            new CreateAssurArrQualityReleaseRequest(
                releaseTitle,
                "Automated coverage for the quality release workflow.",
                "low",
                "assurarr",
                "HOLD-000001",
                ["loadarr:inventory:test"],
                null,
                "HOLD-000001",
                "full",
                null,
                DateTimeOffset.UtcNow,
                "Inspection evidence retained in RecordArr.",
                DateTimeOffset.UtcNow.AddDays(1),
                ["recordarr:doc:test"],
                "Release notes"));

        Assert.Equal(HttpStatusCode.OK, releaseResponse.StatusCode);

        var release = await releaseResponse.Content.ReadFromJsonAsync<AssurArrQualityReleaseResponse>();
        Assert.NotNull(release);
        Assert.Equal(releaseTitle, release!.Title);

        var listResponse = await _client.GetAsync("/api/v1/integrations/quality-reviews");
        listResponse.EnsureSuccessStatusCode();
        var reviews = await listResponse.Content.ReadFromJsonAsync<List<AssurArrQualityReviewResponse>>();
        Assert.NotNull(reviews);
        Assert.Contains(reviews!, item => item.Title == reviewTitle);
    }

    [Fact]
    public async Task Can_create_supplier_quality_issue_and_customer_complaint_records()
    {
        var supplierTitle = $"Test supplier quality issue {Guid.NewGuid():N}";
        var supplierResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/supplier-quality-issues",
            new CreateAssurArrSupplierQualityIssueRequest(
                supplierTitle,
                "Automated coverage for supplier quality issues.",
                "high",
                "damaged_received",
                "loadarr",
                "loadarr:receipt:test",
                ["loadarr:receipt:test"],
                ["supplyarr:po:test"],
                ["supplyarr:item:test"],
                "supplyarr:supplier:test",
                "NCR-000001",
                "SCAR-000001",
                ["HOLD-000001"],
                ["recordarr:doc:test"],
                null,
                DateTimeOffset.UtcNow));

        Assert.Equal(HttpStatusCode.OK, supplierResponse.StatusCode);
        var supplierIssue = await supplierResponse.Content.ReadFromJsonAsync<AssurArrSupplierQualityIssueResponse>();
        Assert.NotNull(supplierIssue);
        Assert.Equal(supplierTitle, supplierIssue!.Title);

        var complaintTitle = $"Test complaint case {Guid.NewGuid():N}";
        var complaintResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/customer-complaint-quality-cases",
            new CreateAssurArrCustomerComplaintQualityCaseRequest(
                complaintTitle,
                "Automated coverage for customer complaint quality cases.",
                "high",
                "delivery_quality",
                "routarr",
                "routarr:shipment:test",
                ["ordarr:order:test"],
                ["routarr:shipment:test"],
                ["loadarr:item:test"],
                ["maintainarr:asset:test"],
                "customarr:customer:test",
                "Jordan Lee, logistics manager",
                "customarr:location:test",
                "NCR-000001",
                ["HOLD-000001"],
                ["CAPA-000001"],
                ["recordarr:doc:response-test"],
                ["recordarr:doc:test"],
                null,
                DateTimeOffset.UtcNow,
                null,
                DateTimeOffset.UtcNow.AddDays(4)));

        Assert.Equal(HttpStatusCode.OK, complaintResponse.StatusCode);
        var complaint = await complaintResponse.Content.ReadFromJsonAsync<AssurArrCustomerComplaintQualityCaseResponse>();
        Assert.NotNull(complaint);
        Assert.Equal(complaintTitle, complaint!.Title);

        var supplierList = await _client.GetAsync("/api/v1/integrations/supplier-quality-issues");
        supplierList.EnsureSuccessStatusCode();
        var supplierIssues = await supplierList.Content.ReadFromJsonAsync<List<AssurArrSupplierQualityIssueResponse>>();
        Assert.NotNull(supplierIssues);
        Assert.Contains(supplierIssues!, item => item.Title == supplierTitle);

        var complaintList = await _client.GetAsync("/api/v1/integrations/customer-complaint-quality-cases");
        complaintList.EnsureSuccessStatusCode();
        var complaintCases = await complaintList.Content.ReadFromJsonAsync<List<AssurArrCustomerComplaintQualityCaseResponse>>();
        Assert.NotNull(complaintCases);
        Assert.Contains(complaintCases!, item => item.Title == complaintTitle);
    }

    [Fact]
    public async Task Can_create_containment_action_and_disposition_records()
    {
        var containmentTitle = $"Test containment action {Guid.NewGuid():N}";
        var containmentResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/containment-actions",
            new CreateAssurArrContainmentActionRequest(
                containmentTitle,
                "Automated coverage for containment actions.",
                "high",
                "quarantine",
                "loadarr",
                "loadarr:inventory:test",
                ["loadarr:inventory:test"],
                "NCR-000001",
                null,
                null,
                "loadarr:receiving:action:test",
                DateTimeOffset.UtcNow.AddDays(1),
                true,
                ["recordarr:doc:test"],
                "Containment notes"));

        Assert.Equal(HttpStatusCode.OK, containmentResponse.StatusCode);
        var containment = await containmentResponse.Content.ReadFromJsonAsync<AssurArrContainmentActionResponse>();
        Assert.NotNull(containment);
        Assert.Equal(containmentTitle, containment!.Title);

        var dispositionTitle = $"Test disposition {Guid.NewGuid():N}";
        var dispositionResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/dispositions",
            new CreateAssurArrDispositionRequest(
                dispositionTitle,
                "Automated coverage for disposition records.",
                "moderate",
                "conditional_release",
                "assurarr",
                "NCR-000001",
                ["loadarr:inventory:test"],
                "NCR-000001",
                null,
                DateTimeOffset.UtcNow,
                null,
                null,
                "Inspection evidence pending.",
                ["Complete inspection"],
                "loadarr",
                "loadarr:inventory:test",
                ["recordarr:doc:test"],
                "Disposition notes"));

        Assert.Equal(HttpStatusCode.OK, dispositionResponse.StatusCode);
        var disposition = await dispositionResponse.Content.ReadFromJsonAsync<AssurArrDispositionResponse>();
        Assert.NotNull(disposition);
        Assert.Equal(dispositionTitle, disposition!.Title);

        var containmentList = await _client.GetAsync("/api/v1/integrations/containment-actions");
        containmentList.EnsureSuccessStatusCode();
        var containmentActions = await containmentList.Content.ReadFromJsonAsync<List<AssurArrContainmentActionResponse>>();
        Assert.NotNull(containmentActions);
        Assert.Contains(containmentActions!, item => item.Title == containmentTitle);

        var dispositionList = await _client.GetAsync("/api/v1/integrations/dispositions");
        dispositionList.EnsureSuccessStatusCode();
        var dispositions = await dispositionList.Content.ReadFromJsonAsync<List<AssurArrDispositionResponse>>();
        Assert.NotNull(dispositions);
        Assert.Contains(dispositions!, item => item.Title == dispositionTitle);
    }
}
