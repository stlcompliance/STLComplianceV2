using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using StaffArr.Api.Data;
using StaffArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class StaffArrFieldsetAccessTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        const string signingKey = "staffarr-fieldset-access-test-signing-key";
        var databaseName = $"StaffArrFieldsetAccess-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(databaseName));
            });
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
    public async Task People_profile_fieldset_denies_tenant_member()
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/fieldsets/people/profile", CreateStaffArrToken("tenant_member")));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task People_profile_fieldset_allows_tenant_admin()
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/fieldsets/people/profile", CreateStaffArrToken("tenant_admin")));

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Employment_application_builder_catalog_denies_tenant_member()
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/fieldsets/employment-applications/builder", CreateStaffArrToken("tenant_member")));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Employment_application_builder_catalog_allows_tenant_admin()
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/fieldsets/employment-applications/builder", CreateStaffArrToken("tenant_admin")));

        response.EnsureSuccessStatusCode();
    }

    private string CreateStaffArrToken(string tenantRoleKey)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<StaffArrTokenService>();
        var (token, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Fieldset Tester",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            ["staffarr"],
            isPlatformAdmin: false);
        return token;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        return request;
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
