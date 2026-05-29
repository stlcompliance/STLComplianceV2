using System.Net.Http.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArrIntegration = MaintainArr.Api.Endpoints.IntegrationEndpoints;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using STLCompliance.E2E.Support;
using NexArr.Api.Services;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace STLCompliance.E2E.Flows;

/// <summary>
/// docs/23 workflow 3 step 5 — StaffArr person lifecycle mirror consumed by MaintainArr technician refs.
/// </summary>
[Trait("Category", "Integration")]
public sealed class StaffArrMaintainArrTechnicianSyncFlowTests : IAsyncLifetime
{
    private E2ENexArrHost _nexarr = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _staffarrToMaintainarrToken = null!;

    public async Task InitializeAsync()
    {
        _nexarr = new E2ENexArrHost();
        await _nexarr.InitializeAsync();

        var adminToken = await _nexarr.LoginAsync();
        _staffarrToMaintainarrToken = await _nexarr.IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            MaintainArrIntegration.StaffarrPersonSyncActionScope,
            ["maintainarr"]);

        var staffArrDbName = $"E2E-TechSync-StaffArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"E2E-TechSync-MaintainArr-{Guid.NewGuid():N}";

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarr.Client.BaseAddress!.ToString().TrimEnd('/'));
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArr.Api.Data.StaffArrDbContext>(services);
                services.AddDbContext<StaffArr.Api.Data.StaffArrDbContext>(options =>
                    options.UseInMemoryDatabase(staffArrDbName));
            });
        });
        _staffarrClient = _staffarrFactory.CreateClient();

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarr.Client.BaseAddress!.ToString().TrimEnd('/'));
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarr.Factory.Server.CreateHandler());
            });
        });
        _maintainarrClient = _maintainarrFactory.CreateClient();

        _staffarrFactory = _staffarrFactory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("MaintainArr:BaseUrl", _maintainarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("MaintainArr:ServiceToken", _staffarrToMaintainarrToken);
            builder.ConfigureServices(services =>
            {
                services.AddHttpClient<MaintainArrTechnicianRefSyncClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _maintainarrFactory.Server.CreateHandler());
            });
        });
        _staffarrClient = _staffarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _staffarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _staffarrFactory.DisposeAsync();
        await _nexarr.DisposeAsync();
    }

    [Fact]
    public async Task Staffarr_person_sync_ingest_updates_maintainarr_technician_ref()
    {
        var personId = Guid.NewGuid();
        var request = HttpTestClient.Authorized(
            HttpMethod.Post,
            "/api/integrations/staffarr-person-sync",
            _staffarrToMaintainarrToken);
        request.Content = JsonContent.Create(new IngestStaffarrPersonSyncRequest(
            PlatformSeeder.DemoTenantId,
            personId,
            "Jordan Tech",
            "active",
            "yard-b",
            "staffarr.person.updated",
            DateTimeOffset.UtcNow,
            $"staffarr.person.updated:{personId:D}"));

        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var ingested = (await response.Content.ReadFromJsonAsync<IngestStaffarrPersonSyncResponse>())!;
        Assert.False(ingested.IdempotentReplay);
        Assert.Equal(personId.ToString("D"), ingested.PersonId);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var mirror = await db.StaffPersonRefs
            .AsNoTracking()
            .SingleAsync(x => x.TenantId == PlatformSeeder.DemoTenantId && x.StaffarrPersonId == personId.ToString("D"));
        Assert.Equal("Jordan Tech", mirror.DisplayNameSnapshot);
        Assert.Equal("active", mirror.ActiveStatusSnapshot);
        Assert.Equal("yard-b", mirror.PrimarySiteSnapshot);
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
