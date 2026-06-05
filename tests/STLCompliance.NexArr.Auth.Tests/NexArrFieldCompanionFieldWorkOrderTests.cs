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

public sealed class NexArrFieldCompanionFieldWorkOrderTests : IAsyncLifetime
{
    private readonly Guid _workOrderId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private WebApplicationFactory<global::NexArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;
    private HttpRequestMessage? _capturedStatusRequest;
    private HttpRequestMessage? _capturedLaborRequest;

    public async Task InitializeAsync()
    {
        _capturedStatusRequest = null;
        _capturedLaborRequest = null;
        var dbName = $"NexArrFieldCompanionWorkOrder-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("ServiceToken:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("MaintainArr__BaseUrl", "http://maintainarr.test");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(dbName));
                services.AddHttpClient(nameof(FieldCompanionProductClient))
                    .ConfigurePrimaryHttpMessageHandler(() => new MaintainArrWorkOrderCaptureHandler(
                        _workOrderId,
                        request => _capturedStatusRequest = request,
                        request => _capturedLaborRequest = request));
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
    public async Task Get_fieldcompanion_field_work_order_detail_proxies_to_maintainarr()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(
            HttpMethod.Get,
            $"/api/fieldcompanion/field-tasks/work-order?taskKey=maintainarr:work-order:{_workOrderId:D}",
            token);

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = (await response.Content.ReadFromJsonAsync<FieldCompanionFieldWorkOrderDetailResponse>())!;
        Assert.Equal("maintainarr", payload.ProductKey);
        Assert.Equal(_workOrderId, payload.WorkOrderId);
        Assert.Equal("WO-1001", payload.WorkOrderNumber);
        Assert.Single(payload.Tasks);
    }

    [Fact]
    public async Task Update_fieldcompanion_field_work_order_status_proxies_to_maintainarr()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/fieldcompanion/field-tasks/work-order/status", token);
        request.Content = JsonContent.Create(new UpdateFieldCompanionFieldWorkOrderStatusRequest(
            $"maintainarr:work-order:{_workOrderId:D}",
            "in_progress"));

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = (await response.Content.ReadFromJsonAsync<FieldCompanionFieldWorkOrderStatusResponse>())!;
        Assert.Equal("in_progress", payload.Status);

        Assert.NotNull(_capturedStatusRequest);
        Assert.Equal(HttpMethod.Patch, _capturedStatusRequest!.Method);
        Assert.Contains($"/api/work-orders/{_workOrderId:D}/status", _capturedStatusRequest.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task Log_fieldcompanion_field_work_order_labor_proxies_to_maintainarr()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/fieldcompanion/field-tasks/work-order/labor", token);
        request.Content = JsonContent.Create(new LogFieldCompanionFieldWorkOrderLaborRequest(
            $"maintainarr:work-order:{_workOrderId:D}",
            1.5m,
            "regular",
            "Field labor note",
            null));

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = (await response.Content.ReadFromJsonAsync<FieldCompanionFieldWorkOrderLaborResponse>())!;
        Assert.Equal(1.5m, payload.HoursWorked);
        Assert.Equal("regular", payload.LaborTypeKey);

        Assert.NotNull(_capturedLaborRequest);
        Assert.Equal(HttpMethod.Post, _capturedLaborRequest!.Method);
        Assert.Contains($"/api/work-orders/{_workOrderId:D}/labor", _capturedLaborRequest.RequestUri!.AbsolutePath);
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

    private sealed class MaintainArrWorkOrderCaptureHandler(
        Guid workOrderId,
        Action<HttpRequestMessage> onStatusRequest,
        Action<HttpRequestMessage> onLaborRequest) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath.EndsWith("/api/field-inbox", StringComparison.OrdinalIgnoreCase) == true)
            {
                var inbox = new FieldInboxResponse(
                    new FieldInboxSummary(1, 0, new Dictionary<string, int> { ["maintainarr"] = 1 }),
                    [
                        new FieldInboxTaskItem(
                            $"maintainarr:work-order:{workOrderId:D}",
                            "maintainarr",
                            "work_order",
                            "Replace pump seal",
                            "PMP-100 · Pump 1",
                            "open",
                            "high",
                            DateTimeOffset.UtcNow,
                            DateTimeOffset.UtcNow,
                            $"/work-orders/{workOrderId:D}",
                            null,
                            $"http://maintainarr.test/work-orders/{workOrderId:D}"),
                    ]);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(inbox),
                };
            }

            if (request.Method == HttpMethod.Get
                && request.RequestUri?.AbsolutePath.Equals($"/api/work-orders/{workOrderId:D}", StringComparison.OrdinalIgnoreCase) == true)
            {
                var detailJson = JsonSerializer.Serialize(new
                {
                    workOrderId,
                    workOrderNumber = "WO-1001",
                    assetId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    assetTag = "PMP-100",
                    assetName = "Pump 1",
                    title = "Replace pump seal",
                    description = "Seal leaking at coupling.",
                    priority = "high",
                    status = "open",
                    updatedAt = DateTimeOffset.UtcNow,
                });

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(detailJson, Encoding.UTF8, "application/json"),
                };
            }

            if (request.Method == HttpMethod.Get
                && request.RequestUri?.AbsolutePath.EndsWith("/tasks", StringComparison.OrdinalIgnoreCase) == true)
            {
                var tasksJson = JsonSerializer.Serialize(new[]
                {
                    new
                    {
                        taskLineId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                        workOrderId,
                        title = "Remove old seal",
                        description = string.Empty,
                        sortOrder = 1,
                        status = "open",
                        completedAt = (DateTimeOffset?)null,
                    },
                });

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(tasksJson, Encoding.UTF8, "application/json"),
                };
            }

            if (request.Method == HttpMethod.Get
                && request.RequestUri?.AbsolutePath.EndsWith("/labor", StringComparison.OrdinalIgnoreCase) == true
                && request.Method == HttpMethod.Get)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]", Encoding.UTF8, "application/json"),
                };
            }

            if (request.Method == HttpMethod.Patch
                && request.RequestUri?.AbsolutePath.EndsWith("/status", StringComparison.OrdinalIgnoreCase) == true)
            {
                onStatusRequest(request);

                var statusJson = JsonSerializer.Serialize(new
                {
                    workOrderId,
                    workOrderNumber = "WO-1001",
                    assetId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    assetTag = "PMP-100",
                    assetName = "Pump 1",
                    title = "Replace pump seal",
                    description = "Seal leaking at coupling.",
                    priority = "high",
                    status = "in_progress",
                    updatedAt = DateTimeOffset.UtcNow,
                });

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(statusJson, Encoding.UTF8, "application/json"),
                };
            }

            if (request.Method == HttpMethod.Post
                && request.RequestUri?.AbsolutePath.EndsWith("/labor", StringComparison.OrdinalIgnoreCase) == true)
            {
                onLaborRequest(request);

                var laborJson = JsonSerializer.Serialize(new
                {
                    laborEntryId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    workOrderId,
                    workOrderTaskLineId = (Guid?)null,
                    personId = Guid.Parse("11111111-1111-1111-1111-111111111111").ToString(),
                    hoursWorked = 1.5m,
                    laborTypeKey = "regular",
                    notes = "Field labor note",
                    loggedAt = DateTimeOffset.UtcNow,
                });

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(laborJson, Encoding.UTF8, "application/json"),
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}
