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
}
