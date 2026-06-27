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
    private static readonly Guid SeedTenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private WebApplicationFactory<global::RecordArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;
    private string _storageRoot = null!;

    public Task InitializeAsync()
    {
        _storageRoot = Path.Combine(Path.GetTempPath(), $"recordarr-upload-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_storageRoot);

        _factory = new WebApplicationFactory<global::RecordArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", SigningKey);
            builder.UseSetting("DocumentStorage:RootPath", _storageRoot);
        });

        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();

        if (Directory.Exists(_storageRoot))
        {
            Directory.Delete(_storageRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Smart_import_source_retention_uses_canonical_import_metadata_source()
    {
        var tenantId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var token = CreateAccessToken(tenantId, personId);
        var contentBytes = "placeholder file content"u8.ToArray();

        var request = new RecordArrIntegrationEndpoints.SmartImportRetainSourceRequest(
            tenantId,
            Guid.NewGuid(),
            "smart-import-source.pdf",
            "application/pdf",
            contentBytes.LongLength,
            "abc123def456",
            Convert.ToBase64String(contentBytes),
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

        var storedFilePath = Path.Combine(_storageRoot, retained!.StorageKey.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(storedFilePath));
        Assert.Equal(contentBytes, await File.ReadAllBytesAsync(storedFilePath));
    }

    [Fact]
    public async Task Create_record_persists_uploaded_file_bytes_to_the_recordarr_disk()
    {
        var tenantId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var token = CreateAccessToken(tenantId, personId);
        var contentBytes = "record upload content"u8.ToArray();

        var request = new WorkspaceEndpoints.CreateRecordRequest(
            "Uploaded record",
            "Created from the create-record form.",
            "document",
            "bol",
            "shipping",
            "standard",
            "internal",
            "nexarr",
            "source_file",
            "source-file-123",
            "Source file 123",
            personId.ToString("D"),
            "source-file.pdf",
            "application/pdf",
            Convert.ToBase64String(contentBytes));

        var createMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/workspace/records")
        {
            Content = JsonContent.Create(request)
        };
        createMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.SendAsync(createMessage);
        createResponse.EnsureSuccessStatusCode();

        var record = await createResponse.Content.ReadFromJsonAsync<RecordArrRecordResponse>();
        Assert.NotNull(record);

        var fileMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/integrations/files/{record!.CurrentFileRef}");
        fileMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var fileResponse = await _client.SendAsync(fileMessage);
        fileResponse.EnsureSuccessStatusCode();

        var file = await fileResponse.Content.ReadFromJsonAsync<RecordArrFileResponse>();
        Assert.NotNull(file);
        Assert.Equal("recordarr", file!.StorageProvider);
        Assert.False(string.IsNullOrWhiteSpace(file.StorageKey));

        var storedFilePath = Path.Combine(_storageRoot, file.StorageKey.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(storedFilePath));
        Assert.Equal(contentBytes, await File.ReadAllBytesAsync(storedFilePath));
    }

    [Fact]
    public async Task Refresh_workflows_moves_due_controlled_documents_into_review_and_surfaces_reminders()
    {
        var personId = Guid.NewGuid();
        var token = CreateAccessToken(SeedTenantId, personId);
        var effectiveAt = DateTimeOffset.UtcNow.AddDays(-181);
        var expectedNextReviewAt = effectiveAt.AddDays(180);

        var createDocumentResponse = await SendAuthorizedJson(
            HttpMethod.Post,
            "/api/v1/integrations/controlled-documents",
            token,
            new RecordArrIntegrationEndpoints.CreateControlledDocumentRequest(
                "Periodic review procedure",
                "Verifies the periodic review refresh workflow.",
                "procedure",
                "operations",
                "review_cycle",
                personId.ToString("D"),
                "org-receiving",
                "site-north-yard",
                true));
        createDocumentResponse.EnsureSuccessStatusCode();

        var createdDocument = await createDocumentResponse.Content.ReadFromJsonAsync<RecordArrControlledDocumentResponse>();
        Assert.NotNull(createdDocument);

        var createVersionResponse = await SendAuthorizedJson(
            HttpMethod.Post,
            $"/api/v1/integrations/controlled-documents/{createdDocument!.ControlledDocumentId}/versions",
            token,
            new RecordArrIntegrationEndpoints.CreateControlledDocumentVersionRequest(
                "periodic-review.pdf",
                "Initial release candidate."));
        createVersionResponse.EnsureSuccessStatusCode();

        var createdVersion = await createVersionResponse.Content.ReadFromJsonAsync<RecordArrControlledDocumentVersionResponse>();
        Assert.NotNull(createdVersion);

        var promoteResponse = await SendAuthorizedJson(
            HttpMethod.Post,
            $"/api/v1/integrations/controlled-documents/{createdDocument.ControlledDocumentId}/versions/{createdVersion!.VersionId}/promote",
            token,
            new WorkspaceEndpoints.PromoteControlledDocumentVersionRequest(effectiveAt));
        promoteResponse.EnsureSuccessStatusCode();

        var refreshResponse = await SendAuthorizedJson(
            HttpMethod.Post,
            "/api/v1/integrations/controlled-documents/refresh-workflows",
            token,
            new { });
        refreshResponse.EnsureSuccessStatusCode();

        var refreshedDocuments = await refreshResponse.Content.ReadFromJsonAsync<RecordArrControlledDocumentResponse[]>();
        Assert.NotNull(refreshedDocuments);

        var refreshedDocument = Assert.Single(
            refreshedDocuments!,
            document => document.ControlledDocumentId == createdDocument.ControlledDocumentId);
        Assert.Equal("review", refreshedDocument.Status);
        Assert.Equal(expectedNextReviewAt, refreshedDocument.NextReviewAt);
        Assert.Contains(
            refreshedDocument.AuditTrail,
            entry => entry.Action == "periodic_review_due" &&
                     entry.Details.Contains("Periodic review became due", StringComparison.OrdinalIgnoreCase));

        var remindersResponse = await SendAuthorizedRequest(
            HttpMethod.Get,
            "/api/v1/integrations/reminders",
            token);
        remindersResponse.EnsureSuccessStatusCode();

        var reminders = await remindersResponse.Content.ReadFromJsonAsync<RecordArrReminderResponse[]>();
        Assert.NotNull(reminders);
        Assert.Contains(
            reminders!,
            reminder =>
                reminder.ReminderType == "controlled_document_review" &&
                reminder.ControlledDocumentId == createdDocument.ControlledDocumentId &&
                reminder.Status == "due_for_review");
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

    private async Task<HttpResponseMessage> SendAuthorizedRequest(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> SendAuthorizedJson(HttpMethod method, string path, string token, object body)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(body);
        return await _client.SendAsync(request);
    }
}
