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
}
