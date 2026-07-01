using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;
using SupplyArr.Api.Data;
using SupplyArr.Api.Endpoints;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrReferenceIntegrationAuthTests : IAsyncLifetime
{
    private static readonly Guid SecondaryTenantId = Guid.Parse("11111111-1111-1111-1111-111111111102");

    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _supplyarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"SupplyArrReferenceTypes-{Guid.NewGuid():N}";

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _supplyarrClient = _supplyarrFactory.CreateClient();
        await SeedPartsAsync();
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Reference_types_catalog_allows_supplyarr_reader()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "tenant_member");

        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/reference-types", token));

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ReferenceTypeDescriptor[]>();
        Assert.NotNull(payload);
        Assert.Contains(payload!, item => item.ReferenceType == "part");
        Assert.Contains(payload!, item => item.ReferenceType == "supplier");
    }

    [Fact]
    public async Task Reference_types_catalog_rejects_unrelated_launched_role()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "routarr_driver");

        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/reference-types", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Reference_types_catalog_rejects_platform_admin_without_supplyarr_role()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "routarr_driver", isPlatformAdmin: true);

        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/reference-types", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Supplier_quick_create_schema_disables_platform_admin_without_supplyarr_role()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "routarr_driver", isPlatformAdmin: true);

        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/references/supplier/quick-create-schema", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Item_reference_lookup_allows_loadarr_service_token_and_scopes_results_to_active_stocked_tenant_items()
    {
        var token = CreateServiceToken("loadarr", SupplyArrItemReferenceIntegrationScopes.ItemReferencesRead);

        var response = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/integrations/item-references?tenantId={PlatformSeeder.DemoTenantId:D}",
                token));

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<IReadOnlyList<SupplyArrItemReferenceLookupResponse>>();
        Assert.NotNull(payload);
        Assert.Equal(2, payload!.Count);
        Assert.Collection(
            payload,
            first =>
            {
                Assert.Equal("SUP-ADH-49", first.PartKey);
                Assert.Equal("Regulated adhesive cartridge", first.DisplayName);
                Assert.True(first.RequiresSerialLotTracking);
            },
            second =>
            {
                Assert.Equal("SUP-VALVE-KIT-A", second.PartKey);
                Assert.Equal("Valve repair kit A", second.DisplayName);
                Assert.False(second.RequiresSerialLotTracking);
            });
    }

    [Fact]
    public async Task Item_reference_lookup_applies_query_filter_for_loadarr_service_token()
    {
        var token = CreateServiceToken("loadarr", SupplyArrItemReferenceIntegrationScopes.ItemReferencesRead);

        var response = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/integrations/item-references?tenantId={PlatformSeeder.DemoTenantId:D}&query=adh",
                token));

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<IReadOnlyList<SupplyArrItemReferenceLookupResponse>>();
        Assert.NotNull(payload);
        var item = Assert.Single(payload!);
        Assert.Equal("SUP-ADH-49", item.PartKey);
        Assert.DoesNotContain(payload!, entry => string.Equals(entry.PartKey, "SUP-SOUTH-BRAKE-01", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Item_reference_lookup_rejects_unapproved_source_product()
    {
        var token = CreateServiceToken("routarr", SupplyArrItemReferenceIntegrationScopes.ItemReferencesRead);

        var response = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/integrations/item-references?tenantId={PlatformSeeder.DemoTenantId:D}",
                token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Item_reference_lookup_rejects_wrong_action_scope()
    {
        var token = CreateServiceToken("loadarr", IntegrationEndpoints.SupplyReferenceReadActionScope);

        var response = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/integrations/item-references?tenantId={PlatformSeeder.DemoTenantId:D}",
                token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private string CreateSupplyArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin",
        bool isPlatformAdmin = false)
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<SupplyArrTokenService>();
        var (token, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin);
        return token;
    }

    private string CreateServiceToken(string sourceProduct, string actionScope, Guid? tenantId = null)
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var options = new StlServiceTokenOptions();
        var credentials = StlServiceTokenKeyMaterial.CreateSigningCredentials(configuration, options);
        var issuer = StlServiceTokenKeyMaterial.ResolveIssuer(configuration, options);
        var audience = StlServiceTokenKeyMaterial.ResolveAudience(configuration, options);
        var tokenId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, tokenId.ToString()),
            new(StlServiceTokenClaimTypes.TokenType, StlServiceTokenClaimTypes.ServiceTokenTypeValue),
            new(StlServiceTokenClaimTypes.ServiceClientId, Guid.NewGuid().ToString()),
            new(StlServiceTokenClaimTypes.SourceProduct, sourceProduct),
            new(StlServiceTokenClaimTypes.AllowedProducts, "supplyarr"),
            new(StlServiceTokenClaimTypes.TokenId, tokenId.ToString()),
            new(StlServiceTokenClaimTypes.TenantScope, (tenantId ?? PlatformSeeder.DemoTenantId).ToString()),
            new(StlServiceTokenClaimTypes.ActionScope, actionScope)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTimeOffset.UtcNow.AddHours(1).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task SeedPartsAsync()
    {
        await using var scope = _supplyarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var now = DateTimeOffset.UtcNow;

        db.Parts.AddRange(
            new Part
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                PartKey = "SUP-VALVE-KIT-A",
                DisplayName = "Valve repair kit A",
                Description = "Warehouse valve repair kit",
                CategoryKey = "maintenance_part",
                UnitOfMeasure = "each",
                ManufacturerName = "ValveCo",
                ManufacturerPartNumber = "VK-A",
                Status = "active",
                IsTrackable = true,
                IsStocked = true,
                RequiresSerialLotTracking = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                PartKey = "SUP-ADH-49",
                DisplayName = "Regulated adhesive cartridge",
                Description = "Hazmat adhesive cartridge",
                CategoryKey = "regulated_consumable",
                UnitOfMeasure = "case",
                ManufacturerName = "Adhesive Labs",
                ManufacturerPartNumber = "ADH-49",
                Status = "active",
                IsTrackable = true,
                IsStocked = true,
                RequiresSerialLotTracking = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                PartKey = "SUP-INACTIVE-01",
                DisplayName = "Retired filter stock",
                Description = "Inactive stock should not be projected to LoadArr.",
                CategoryKey = "maintenance_part",
                UnitOfMeasure = "each",
                ManufacturerName = "FilterCo",
                ManufacturerPartNumber = "FLT-RET",
                Status = "inactive",
                IsTrackable = true,
                IsStocked = true,
                RequiresSerialLotTracking = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                PartKey = "SUP-NONSTOCK-01",
                DisplayName = "Non-stock planning placeholder",
                Description = "Planning-only part should not appear in LoadArr.",
                CategoryKey = "planning_part",
                UnitOfMeasure = "each",
                ManufacturerName = "PlanCo",
                ManufacturerPartNumber = "PLAN-01",
                Status = "active",
                IsTrackable = false,
                IsStocked = false,
                RequiresSerialLotTracking = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Part
            {
                Id = Guid.NewGuid(),
                TenantId = SecondaryTenantId,
                PartKey = "SUP-SOUTH-BRAKE-01",
                DisplayName = "South depot brake kit",
                Description = "Secondary tenant stock must stay isolated.",
                CategoryKey = "maintenance_part",
                UnitOfMeasure = "each",
                ManufacturerName = "BrakeCo",
                ManufacturerPartNumber = "BRK-01",
                Status = "active",
                IsTrackable = true,
                IsStocked = true,
                RequiresSerialLotTracking = false,
                CreatedAt = now,
                UpdatedAt = now
            });

        await db.SaveChangesAsync();
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>)
                || d.ServiceType == typeof(TContext))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }
}
