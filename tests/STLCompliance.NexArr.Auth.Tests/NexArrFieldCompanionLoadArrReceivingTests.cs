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

public sealed class NexArrFieldCompanionLoadArrReceivingTests : IAsyncLifetime
{
    private readonly Guid _taskId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private const string ReceivingSessionId = "recv-24018";
    private WebApplicationFactory<global::NexArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;
    private string? _capturedCompleteRequestPath;
    private string? _capturedCompleteRequestBody;

    public async Task InitializeAsync()
    {
        _capturedCompleteRequestPath = null;
        _capturedCompleteRequestBody = null;
        var dbName = $"NexArrFieldCompanionLoadArrReceiving-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("ServiceToken:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("LoadArr__BaseUrl", "http://loadarr.test");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(dbName));
                services.AddHttpClient(nameof(FieldCompanionProductClient))
                    .ConfigurePrimaryHttpMessageHandler(() => new LoadArrReceivingCaptureHandler(
                        _taskId,
                        (requestPath, requestBody) =>
                        {
                            _capturedCompleteRequestPath = requestPath;
                            _capturedCompleteRequestBody = requestBody;
                        }));
            });
        });

        _client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, passwordHasher);
        await EnsureFieldCompanionEntitlementAsync(db);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Get_fieldcompanion_field_receiving_detail_maps_loadarr_session()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(
            HttpMethod.Get,
            $"/api/fieldcompanion/field-tasks/receiving?taskKey=loadarr:receiving:{_taskId:D}",
            token);

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = (await response.Content.ReadFromJsonAsync<FieldCompanionFieldReceivingDetailResponse>())!;
        Assert.Equal("loadarr", payload.ProductKey);
        Assert.Equal(ReceivingSessionId, payload.ReceivingReceiptId);
        Assert.Equal("RCV-24018", payload.ReceiptKey);
        Assert.Equal("PO-10492", payload.PurchaseOrderKey);
        Assert.Single(payload.Lines);
        Assert.Equal("line-24018-1", payload.Lines[0].LineId);
    }

    [Fact]
    public async Task Post_fieldcompanion_field_receiving_completes_loadarr_session()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/fieldcompanion/field-tasks/receiving/post", token);
        request.Content = JsonContent.Create(new PostFieldCompanionFieldReceivingRequest(
            $"loadarr:receiving:{_taskId:D}"));

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = (await response.Content.ReadFromJsonAsync<FieldCompanionFieldReceivingPostResponse>())!;
        Assert.Equal("loadarr", payload.ProductKey);
        Assert.Equal(ReceivingSessionId, payload.ReceivingReceiptId);
        Assert.Equal("completed", payload.Status);

        Assert.False(string.IsNullOrWhiteSpace(_capturedCompleteRequestPath));
        Assert.Contains(
            $"/api/v1/receiving/{ReceivingSessionId}/complete",
            _capturedCompleteRequestPath,
            StringComparison.Ordinal);

        Assert.False(string.IsNullOrWhiteSpace(_capturedCompleteRequestBody));
        Assert.Contains("\"sourceObjectId\":\"PO-10492\"", _capturedCompleteRequestBody, StringComparison.Ordinal);
        Assert.Contains("\"warehouseLocationId\":\"loc-dock-01\"", _capturedCompleteRequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Update_fieldcompanion_field_receiving_line_rejects_loadarr_edits()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/fieldcompanion/field-tasks/receiving/line", token);
        request.Content = JsonContent.Create(new UpdateFieldCompanionFieldReceivingLineRequest(
            $"loadarr:receiving:{_taskId:D}",
            "line-24018-1",
            40m));

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

    private static async Task EnsureFieldCompanionEntitlementAsync(NexArrDbContext db)
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

    private sealed class LoadArrReceivingCaptureHandler(
        Guid taskId,
        Action<string, string> onCompleteRequest) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath.EndsWith("/api/field-inbox", StringComparison.OrdinalIgnoreCase) == true
                && request.RequestUri.Host.Contains("loadarr", StringComparison.OrdinalIgnoreCase))
            {
                var inbox = new FieldInboxResponse(
                    new FieldInboxSummary(1, 0, new Dictionary<string, int> { ["loadarr"] = 1 }),
                    [
                        new FieldInboxTaskItem(
                            $"loadarr:receiving:{taskId:D}",
                            "loadarr",
                            "receiving",
                            "RCV-24018",
                            "PO-10492 · Midwest Fleet Supply · STL North Yard",
                            "open",
                            "high",
                            null,
                            DateTimeOffset.UtcNow,
                            $"/work/receiving/{ReceivingSessionId}?taskKey=loadarr:receiving:{taskId:D}",
                            null,
                            $"http://loadarr.test/work/receiving/{ReceivingSessionId}?taskKey=loadarr:receiving:{taskId:D}"),
                    ]);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(inbox),
                };
            }

            if (request.Method == HttpMethod.Get
                && request.RequestUri?.AbsolutePath.Equals($"/api/v1/receiving/{ReceivingSessionId}", StringComparison.OrdinalIgnoreCase) == true)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(BuildSessionJson("open"), Encoding.UTF8, "application/json"),
                };
            }

            if (request.Method == HttpMethod.Post
                && request.RequestUri?.AbsolutePath.Equals($"/api/v1/receiving/{ReceivingSessionId}/complete", StringComparison.OrdinalIgnoreCase) == true)
            {
                var body = request.Content is null
                    ? string.Empty
                    : await request.Content.ReadAsStringAsync(cancellationToken);
                onCompleteRequest(request.RequestUri.AbsolutePath, body);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(BuildCompletionJson(), Encoding.UTF8, "application/json"),
                };
            }

            var empty = new FieldInboxResponse(
                FieldInboxRules.BuildProductResponse([]).Summary,
                []);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(empty),
            };
        }

        private static string BuildSessionJson(string status)
        {
            var payload = new
            {
                id = ReceivingSessionId,
                receivingNumber = "RCV-24018",
                receivingType = "purchase_order",
                status,
                staffarrSiteOrgUnitId = "staff-site-stl-north",
                staffarrSiteNameSnapshot = "STL North Yard",
                sourceProductKey = "supplyarr",
                sourceObjectType = "purchase_order",
                sourceObjectId = "PO-10492",
                supplierNameSnapshot = "Midwest Fleet Supply",
                startedByPersonId = "person-inventory-clerk",
                completedByPersonId = (string?)null,
                startedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-5).ToString("O"),
                completedAtUtc = (string?)null,
                lines = new[]
                {
                    new
                    {
                        id = "line-24018-1",
                        supplyarrItemId = "SUP-VALVE-KIT-A",
                        itemNameSnapshot = "Valve repair kit A",
                        expectedQuantity = 38m,
                        receivedQuantity = 38m,
                        unitOfMeasure = "each",
                        warehouseLocationId = "loc-dock-01",
                        locationNameSnapshot = "Receiving Dock 1",
                        lotCode = "L2405-77",
                        serialCode = (string?)null,
                        condition = "new",
                        status = "ready_to_complete",
                        discrepancyReasonCode = (string?)null,
                        evidenceSummary = "Dock receipt photo attached",
                    },
                },
            };

            return JsonSerializer.Serialize(payload);
        }

        private static string BuildCompletionJson()
        {
            var payload = new
            {
                session = new
                {
                    id = ReceivingSessionId,
                    receivingNumber = "RCV-24018",
                    receivingType = "purchase_order",
                    status = "completed",
                    staffarrSiteOrgUnitId = "staff-site-stl-north",
                    staffarrSiteNameSnapshot = "STL North Yard",
                    sourceProductKey = "supplyarr",
                    sourceObjectType = "purchase_order",
                    sourceObjectId = "PO-10492",
                    supplierNameSnapshot = "Midwest Fleet Supply",
                    startedByPersonId = "person-inventory-clerk",
                    completedByPersonId = "33333333-3333-3333-3333-333333333333",
                    startedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-5).ToString("O"),
                    completedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
                    lines = new[]
                    {
                        new
                        {
                            id = "line-24018-1",
                            supplyarrItemId = "SUP-VALVE-KIT-A",
                            itemNameSnapshot = "Valve repair kit A",
                            expectedQuantity = 38m,
                            receivedQuantity = 38m,
                            unitOfMeasure = "each",
                            warehouseLocationId = "loc-dock-01",
                            locationNameSnapshot = "Receiving Dock 1",
                            lotCode = "L2405-77",
                            serialCode = (string?)null,
                            condition = "new",
                            status = "ready_to_complete",
                            discrepancyReasonCode = (string?)null,
                            evidenceSummary = "Dock receipt photo attached",
                        },
                    },
                },
            };

            return JsonSerializer.Serialize(payload);
        }
    }
}
