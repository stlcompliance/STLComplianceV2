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

public sealed class NexArrFieldCompanionFieldDvirTests : IAsyncLifetime
{
    private readonly Guid _tripId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private WebApplicationFactory<global::NexArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;
    private HttpRequestMessage? _capturedRoutArrDvirRequest;

    public async Task InitializeAsync()
    {
        _capturedRoutArrDvirRequest = null;
        var dbName = $"NexArrFieldCompanionDvir-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("ServiceToken:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("RoutArr__BaseUrl", "http://routarr.test");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(dbName));
                services.AddHttpClient(nameof(FieldCompanionProductClient))
                    .ConfigurePrimaryHttpMessageHandler(() => new RoutArrDvirCaptureHandler(
                        _tripId,
                        request => _capturedRoutArrDvirRequest = request));
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
    public async Task Submit_fieldcompanion_field_dvir_proxies_to_routarr_trip()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/fieldcompanion/field-tasks/dvir", token);
        request.Content = JsonContent.Create(new SubmitFieldCompanionFieldDvirRequest(
            $"routarr:trip:{_tripId:D}",
            "pre_trip",
            "pass",
            12345,
            null,
            null));

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = (await response.Content.ReadFromJsonAsync<FieldCompanionFieldDvirResponse>())!;
        Assert.Equal("routarr", payload.ProductKey);
        Assert.Equal("pre_trip", payload.Phase);
        Assert.Equal("pass", payload.Result);

        Assert.NotNull(_capturedRoutArrDvirRequest);
        Assert.Equal(HttpMethod.Post, _capturedRoutArrDvirRequest!.Method);
        Assert.Contains($"/api/trips/{_tripId:D}/dvir", _capturedRoutArrDvirRequest.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task Submit_rejects_unsupported_product_task_for_dvir()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/fieldcompanion/field-tasks/dvir", token);
        request.Content = JsonContent.Create(new SubmitFieldCompanionFieldDvirRequest(
            "maintainarr:work_order:11111111-1111-1111-1111-111111111111",
            "pre_trip",
            "pass",
            null,
            null,
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

    private sealed class RoutArrDvirCaptureHandler(
        Guid tripId,
        Action<HttpRequestMessage> onRequest) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath.EndsWith("/api/field-inbox", StringComparison.OrdinalIgnoreCase) == true)
            {
                var inbox = new FieldInboxResponse(
                    new FieldInboxSummary(1, 1, new Dictionary<string, int> { ["routarr"] = 1 }),
                    [
                        new FieldInboxTaskItem(
                            $"routarr:trip:{tripId:D}",
                            "routarr",
                            "trip",
                            "North route",
                            "TR-100",
                            "assigned",
                            null,
                            null,
                            DateTimeOffset.UtcNow,
                            $"/trips/{tripId:D}",
                            "Pre-trip DVIR required",
                            $"http://routarr.test/trips/{tripId:D}"),
                    ]);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(inbox),
                };
            }

            onRequest(request);

            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            var phase = document.RootElement.GetProperty("phase").GetString();

            var responseJson = JsonSerializer.Serialize(new
            {
                dvirId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                tripId,
                phase,
                vehicleRefKey = "VEH-1",
                result = "pass",
                odometerReading = 12345,
                defectNotes = string.Empty,
                submittedByPersonId = PlatformSeeder.DemoAdminUserId.ToString(),
                submittedAt = DateTimeOffset.UtcNow,
            });

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            };
        }
    }
}
