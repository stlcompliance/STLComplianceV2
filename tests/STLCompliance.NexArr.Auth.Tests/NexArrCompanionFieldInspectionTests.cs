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

public sealed class NexArrCompanionFieldInspectionTests : IAsyncLifetime
{
    private readonly Guid _inspectionRunId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    private WebApplicationFactory<global::NexArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;
    private HttpRequestMessage? _capturedMaintainArrAnswersRequest;
    private HttpRequestMessage? _capturedMaintainArrCompleteRequest;

    public async Task InitializeAsync()
    {
        _capturedMaintainArrAnswersRequest = null;
        _capturedMaintainArrCompleteRequest = null;
        var dbName = $"NexArrCompanionInspection-{Guid.NewGuid():N}";

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
                services.AddHttpClient(nameof(CompanionProductClient))
                    .ConfigurePrimaryHttpMessageHandler(() => new MaintainArrInspectionCaptureHandler(
                        _inspectionRunId,
                        request => _capturedMaintainArrAnswersRequest = request,
                        request => _capturedMaintainArrCompleteRequest = request));
            });
        });

        _client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, passwordHasher);
        await EnsureCompanionEntitlementAsync(db);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Get_companion_field_inspection_detail_proxies_to_maintainarr()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(
            HttpMethod.Get,
            $"/api/companion/field-tasks/inspection?taskKey=maintainarr:inspection:{_inspectionRunId:D}",
            token);

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = (await response.Content.ReadFromJsonAsync<CompanionFieldInspectionDetailResponse>())!;
        Assert.Equal("maintainarr", payload.ProductKey);
        Assert.Equal(_inspectionRunId, payload.InspectionRunId);
        Assert.Single(payload.ChecklistItems);
    }

    [Fact]
    public async Task Submit_companion_field_inspection_answers_proxies_to_maintainarr()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var checklistItemId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var request = Authorized(HttpMethod.Post, "/api/companion/field-tasks/inspection/answers", token);
        request.Content = JsonContent.Create(new SubmitCompanionFieldInspectionAnswersRequest(
            $"maintainarr:inspection:{_inspectionRunId:D}",
            [
                new CompanionFieldInspectionAnswerInput(checklistItemId, "pass", null, null),
            ]));

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = (await response.Content.ReadFromJsonAsync<CompanionFieldInspectionAnswersResponse>())!;
        Assert.Equal("maintainarr", payload.ProductKey);
        Assert.Equal(1, payload.AnswerCount);

        Assert.NotNull(_capturedMaintainArrAnswersRequest);
        Assert.Equal(HttpMethod.Put, _capturedMaintainArrAnswersRequest!.Method);
        Assert.Contains($"/api/inspections/{_inspectionRunId:D}/answers", _capturedMaintainArrAnswersRequest.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task Complete_companion_field_inspection_proxies_to_maintainarr()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/companion/field-tasks/inspection/complete", token);
        request.Content = JsonContent.Create(new CompleteCompanionFieldInspectionRequest(
            $"maintainarr:inspection:{_inspectionRunId:D}"));

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = (await response.Content.ReadFromJsonAsync<CompanionFieldInspectionCompleteResponse>())!;
        Assert.Equal("completed", payload.Status);
        Assert.Equal("passed", payload.Result);

        Assert.NotNull(_capturedMaintainArrCompleteRequest);
        Assert.Equal(HttpMethod.Post, _capturedMaintainArrCompleteRequest!.Method);
        Assert.Contains($"/api/inspections/{_inspectionRunId:D}/complete", _capturedMaintainArrCompleteRequest.RequestUri!.AbsolutePath);
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

    private static async Task EnsureCompanionEntitlementAsync(NexArrDbContext db)
    {
        if (await db.Entitlements.AnyAsync(e =>
                e.TenantId == PlatformSeeder.DemoTenantId && e.ProductKey == "companion"))
        {
            return;
        }

        db.Entitlements.Add(new TenantProductEntitlement
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ProductKey = "companion",
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

    private sealed class MaintainArrInspectionCaptureHandler(
        Guid inspectionRunId,
        Action<HttpRequestMessage> onAnswersRequest,
        Action<HttpRequestMessage> onCompleteRequest) : HttpMessageHandler
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
                            $"maintainarr:inspection:{inspectionRunId:D}",
                            "maintainarr",
                            "inspection",
                            "Daily walkaround",
                            "PMP-100 · Pump 1",
                            "in_progress",
                            null,
                            null,
                            DateTimeOffset.UtcNow,
                            $"/inspections/{inspectionRunId:D}",
                            null,
                            $"http://maintainarr.test/inspections/{inspectionRunId:D}"),
                    ]);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(inbox),
                };
            }

            if (request.Method == HttpMethod.Get
                && request.RequestUri?.AbsolutePath.Contains($"/api/inspections/{inspectionRunId:D}", StringComparison.OrdinalIgnoreCase) == true
                && !request.RequestUri.AbsolutePath.EndsWith("/answers", StringComparison.OrdinalIgnoreCase)
                && !request.RequestUri.AbsolutePath.EndsWith("/complete", StringComparison.OrdinalIgnoreCase))
            {
                var detailJson = JsonSerializer.Serialize(new
                {
                    inspectionRunId,
                    assetTag = "PMP-100",
                    assetName = "Pump 1",
                    templateName = "Daily walkaround",
                    status = "in_progress",
                    result = (string?)null,
                    completedAt = (DateTimeOffset?)null,
                    checklistItems = new[]
                    {
                        new
                        {
                            checklistItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                            itemKey = "visual_leaks",
                            prompt = "Check for visible leaks",
                            itemType = "pass_fail",
                            isRequired = true,
                            sortOrder = 1,
                        },
                    },
                    answers = Array.Empty<object>(),
                });

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(detailJson, Encoding.UTF8, "application/json"),
                };
            }

            if (request.Method == HttpMethod.Put
                && request.RequestUri?.AbsolutePath.EndsWith("/answers", StringComparison.OrdinalIgnoreCase) == true)
            {
                onAnswersRequest(request);

                var answersJson = JsonSerializer.Serialize(new
                {
                    inspectionRunId,
                    assetTag = "PMP-100",
                    assetName = "Pump 1",
                    templateName = "Daily walkaround",
                    status = "in_progress",
                    result = (string?)null,
                    completedAt = (DateTimeOffset?)null,
                    checklistItems = new[]
                    {
                        new
                        {
                            checklistItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                            itemKey = "visual_leaks",
                            prompt = "Check for visible leaks",
                            itemType = "pass_fail",
                            isRequired = true,
                            sortOrder = 1,
                        },
                    },
                    answers = new[]
                    {
                        new
                        {
                            checklistItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                            itemKey = "visual_leaks",
                            passFailValue = "pass",
                            numericValue = (decimal?)null,
                            textValue = (string?)null,
                            answeredAt = DateTimeOffset.UtcNow,
                        },
                    },
                });

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(answersJson, Encoding.UTF8, "application/json"),
                };
            }

            if (request.Method == HttpMethod.Post
                && request.RequestUri?.AbsolutePath.EndsWith("/complete", StringComparison.OrdinalIgnoreCase) == true)
            {
                onCompleteRequest(request);

                var completedJson = JsonSerializer.Serialize(new
                {
                    inspectionRunId,
                    assetTag = "PMP-100",
                    assetName = "Pump 1",
                    templateName = "Daily walkaround",
                    status = "completed",
                    result = "passed",
                    completedAt = DateTimeOffset.UtcNow,
                    checklistItems = Array.Empty<object>(),
                    answers = Array.Empty<object>(),
                });

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(completedJson, Encoding.UTF8, "application/json"),
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}
