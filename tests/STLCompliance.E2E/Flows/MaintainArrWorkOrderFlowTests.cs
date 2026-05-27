using System.Net.Http.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using STLCompliance.E2E.Support;
using MaintainArrRedeemRequest = MaintainArr.Api.Contracts.RedeemHandoffRequest;
using MaintainArrHandoffSessionResponse = MaintainArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.E2E.Flows;

/// <summary>
/// NexArr handoff → MaintainArr work order create → in_progress → completed lifecycle.
/// </summary>
[Trait("Category", "Integration")]
public sealed class MaintainArrWorkOrderFlowTests : IAsyncLifetime
{
    private E2ENexArrHost _nexarr = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _maintainarrClient = null!;

    public async Task InitializeAsync()
    {
        _nexarr = new E2ENexArrHost();
        await _nexarr.InitializeAsync();

        var adminToken = await _nexarr.LoginAsync();
        var handoffToken = await _nexarr.IssueServiceTokenAsync(adminToken, "maintainarr", "launch.redeem");
        var maintainArrDbName = $"E2E-MaintainArr-WO-{Guid.NewGuid():N}";

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarr.Client.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", handoffToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));
                services.AddHttpClient<NexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarr.Factory.Server.CreateHandler());
            });
        });

        _maintainarrClient = _maintainarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _nexarr.DisposeAsync();
    }

    [Fact]
    public async Task Work_order_create_start_and_complete_via_handoff_session()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetAsync(managerToken);

        var createRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/work-orders", managerToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "E2E hydraulic hose replacement",
            "Cross-product E2E work order flow",
            "high",
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Equal("open", created.Status);
        Assert.StartsWith("WO-", created.WorkOrderNumber);

        var startRequest = HttpTestClient.Authorized(
            HttpMethod.Patch,
            $"/api/work-orders/{created.WorkOrderId}/status",
            managerToken);
        startRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("in_progress"));
        (await _maintainarrClient.SendAsync(startRequest)).EnsureSuccessStatusCode();

        var completeRequest = HttpTestClient.Authorized(
            HttpMethod.Patch,
            $"/api/work-orders/{created.WorkOrderId}/status",
            managerToken);
        completeRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("completed"));
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Equal("completed", completed.Status);
        Assert.NotNull(completed.CompletedAt);
    }

    private async Task<string> RedeemMaintainArrTokenAsync()
    {
        var handoffCode = await _nexarr.CreateHandoffAsync("maintainarr", "http://localhost:5178/launch");
        var redeemResponse = await _maintainarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new MaintainArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<MaintainArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<Guid> SeedAssetAsync(string token)
    {
        var createClassRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            $"e2e-class-{Guid.NewGuid():N}".Substring(0, 12),
            "E2E Vehicles",
            string.Empty));
        var assetClass = (await (await _maintainarrClient.SendAsync(createClassRequest)).Content
            .ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            $"e2e-type-{Guid.NewGuid():N}".Substring(0, 12),
            "E2E Forklift",
            string.Empty));
        var assetType = (await (await _maintainarrClient.SendAsync(createTypeRequest)).Content
            .ReadFromJsonAsync<AssetTypeResponse>())!;

        var createAssetRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetType.AssetTypeId,
            $"E2E-{Guid.NewGuid():N}".Substring(0, 12),
            "E2E Asset",
            string.Empty,
            null));
        var asset = (await (await _maintainarrClient.SendAsync(createAssetRequest)).Content
            .ReadFromJsonAsync<AssetResponse>())!;
        return asset.AssetId;
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
}
