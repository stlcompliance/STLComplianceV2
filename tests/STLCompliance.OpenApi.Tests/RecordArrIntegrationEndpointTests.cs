using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RecordArr.Api.Endpoints;
using RecordArr.Api.Models;
using RecordArr.Api.Services;

namespace STLCompliance.OpenApi.Tests;

public sealed class RecordArrIntegrationEndpointTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::RecordArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<global::RecordArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", SigningKey);
        });

        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Smart_import_source_retention_uses_canonical_import_metadata_source()
    {
        var tenantId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var token = CreateAccessToken(tenantId, personId);

        var request = new RecordArrIntegrationEndpoints.SmartImportRetainSourceRequest(
            tenantId,
            personId,
            Guid.NewGuid(),
            "smart-import-source.pdf",
            "application/pdf",
            4096,
            "abc123def456",
            Convert.ToBase64String("placeholder file content"u8.ToArray()),
            "routarr");

        var createMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/integrations/smart-import/source-files")
        {
            Content = JsonContent.Create(request)
        };
        createMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.SendAsync(createMessage);
        createResponse.EnsureSuccessStatusCode();

        var retained = await createResponse.Content.ReadFromJsonAsync<RecordArrIntegrationEndpoints.SmartImportRetainSourceResponse>();
        Assert.NotNull(retained);

        var metadataMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/integrations/records/{retained!.RecordId}/metadata");
        metadataMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var metadataResponse = await _client.SendAsync(metadataMessage);
        metadataResponse.EnsureSuccessStatusCode();

        var metadata = await metadataResponse.Content.ReadFromJsonAsync<IReadOnlyList<RecordArrRecordMetadataResponse>>();
        Assert.NotNull(metadata);
        Assert.Contains(metadata!, entry => entry.Key == "sha256" && entry.Source == "import");
        Assert.Contains(metadata!, entry => entry.Key == "destination_product_hint" && entry.Source == "import");
    }

    private string CreateAccessToken(Guid tenantId, Guid personId)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<RecordArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            Guid.NewGuid(),
            personId,
            "recordarr.user@demo.stl",
            "RecordArr User",
            tenantId,
            Guid.NewGuid(),
            "evidence_manager",
            ["recordarr"],
            false);
        return accessToken;
    }
}
