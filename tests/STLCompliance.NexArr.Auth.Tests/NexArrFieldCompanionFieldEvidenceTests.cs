using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class NexArrFieldCompanionFieldEvidenceTests : IAsyncLifetime
{
    private readonly Guid _assignmentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private WebApplicationFactory<global::NexArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;
    private HttpRequestMessage? _capturedTrainArrEvidenceRequest;

    public async Task InitializeAsync()
    {
        _capturedTrainArrEvidenceRequest = null;
        var dbName = $"NexArrFieldCompanionEvidence-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("ServiceToken:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("TrainArr__BaseUrl", "http://trainarr.test");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(dbName));
                services.AddHttpClient(nameof(FieldCompanionProductClient))
                    .ConfigurePrimaryHttpMessageHandler(() => new TrainArrEvidenceCaptureHandler(
                        _assignmentId,
                        request => _capturedTrainArrEvidenceRequest = request));
            });
        });

        _client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, passwordHasher);
        await EnsureFieldCompanionLaunchDestinationCompatibilityAsync(db);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Submit_fieldcompanion_field_evidence_proxies_to_trainarr_assignment()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/fieldcompanion/field-tasks/evidence", token);
        request.Content = JsonContent.Create(new SubmitFieldCompanionFieldEvidenceRequest(
            $"trainarr:assignment:{_assignmentId:D}",
            FieldCompanionFieldEvidenceCaptureKinds.Photo,
            "site-photo.jpg",
            "image/jpeg",
            Convert.ToBase64String("fake jpeg bytes"u8.ToArray()),
            "fieldcompanion field capture"));

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = (await response.Content.ReadFromJsonAsync<FieldCompanionFieldEvidenceResponse>())!;
        Assert.Equal("trainarr", payload.ProductKey);
        Assert.Equal("photo", payload.EvidenceTypeKey);

        Assert.NotNull(_capturedTrainArrEvidenceRequest);
        Assert.Equal(HttpMethod.Post, _capturedTrainArrEvidenceRequest!.Method);
        Assert.Contains($"/api/training-assignments/{_assignmentId:D}/evidence", _capturedTrainArrEvidenceRequest.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task Submit_rejects_unsupported_product_task()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/fieldcompanion/field-tasks/evidence", token);
        request.Content = JsonContent.Create(new SubmitFieldCompanionFieldEvidenceRequest(
            "maintainarr:work_order:11111111-1111-1111-1111-111111111111",
            FieldCompanionFieldEvidenceCaptureKinds.Document,
            "inspection.pdf",
            "application/pdf",
            Convert.ToBase64String("pdf"u8.ToArray()),
            null));

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private async Task<string> LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static async Task EnsureFieldCompanionLaunchDestinationCompatibilityAsync(NexArrDbContext db)
    {
        if (await db.Entitlements.AnyAsync(e =>
                e.TenantId == PlatformSeeder.DemoTenantId && e.ProductKey == "fieldcompanion"))
        {
            return;
        }

        db.Entitlements.Add(new TenantProductEntitlement
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ProductKey = "fieldcompanion",
            Status = EntitlementStatuses.Active,
            GrantedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
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

    private sealed class TrainArrEvidenceCaptureHandler(
        Guid assignmentId,
        Action<HttpRequestMessage> onRequest) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath.EndsWith("/api/field-inbox", StringComparison.OrdinalIgnoreCase) == true)
            {
                var inbox = new FieldInboxResponse(
                    new FieldInboxSummary(1, 0, new Dictionary<string, int> { ["trainarr"] = 1 }),
                    [
                        new FieldInboxTaskItem(
                            $"trainarr:assignment:{assignmentId:D}",
                            "trainarr",
                            "training_assignment",
                            "Evidence assignment",
                            null,
                            "assigned",
                            null,
                            null,
                            DateTimeOffset.UtcNow,
                            $"/assignments/{assignmentId:D}",
                            "Upload training evidence to begin",
                            $"http://trainarr.test/assignments/{assignmentId:D}"),
                    ]);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(inbox),
                };
            }

            onRequest(request);

            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            var evidenceType = document.RootElement.GetProperty("evidenceTypeKey").GetString();

            var responseJson = JsonSerializer.Serialize(new
            {
                evidenceId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                trainingAssignmentId = assignmentId,
                evidenceTypeKey = evidenceType,
                fileName = "site-photo.jpg",
                contentType = "image/jpeg",
                sizeBytes = 17,
                notes = "fieldcompanion field capture",
                uploadedByUserId = PlatformSeeder.DemoAdminUserId,
                createdAt = DateTimeOffset.UtcNow,
            });

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            };
        }
    }
}
