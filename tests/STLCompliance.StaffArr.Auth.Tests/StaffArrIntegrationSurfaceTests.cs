using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NexArr.Api.Services;
using StaffArr.Api.Contracts;
using StaffArr.Api.Endpoints;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class StaffArrIntegrationSurfaceTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "staffarr-integration-surface-test-signing-key";
        var databaseName = $"StaffArrIntegrationSurface-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(databaseName));
            });
        });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedAsync(db);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Integration_aliases_are_served_for_staffarr_surface()
    {
        var token = CreateStaffArrToken();
        var locationsToken = CreateServiceToken(
            "maintainarr",
            $"{IntegrationEndpoints.SitesReadActionScope},{IntegrationEndpoints.LocationsReadActionScope}");
        var personId = await SeedPersonOnlyAsync();

        var routes = new (string Route, string Token)[]
        {
            ("/api/v1/integrations/persons", token),
            ("/api/v1/integrations/org-units", token),
            ($"/api/v1/integrations/sites?tenantId={PlatformSeeder.DemoTenantId}", locationsToken),
            ($"/api/v1/integrations/locations?tenantId={PlatformSeeder.DemoTenantId}", locationsToken),
            ($"/api/v1/integrations/persons/{personId}", token),
            ($"/api/v1/integrations/persons/{personId}/readiness", token),
            ($"/api/v1/integrations/persons/{personId}/permissions", token),
            ($"/api/v1/integrations/persons/{personId}/history", token),
            ($"/api/v1/integrations/persons/{personId}/restrictions", token),
            ("/api/v1/integrations/incidents", token)
        };

        foreach (var (route, routeToken) in routes)
        {
            using var request = Authorized(HttpMethod.Get, route, routeToken);

            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(
                response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Accepted or HttpStatusCode.Created,
                $"{route} returned {(int)response.StatusCode} {response.StatusCode}: {body}");
        }
    }

    [Fact]
    public async Task Integration_restrictions_lift_round_trips()
    {
        var token = CreateStaffArrToken();
        var personId = await SeedPersonOnlyAsync();

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/integrations/restrictions", token);
        createRequest.Content = JsonContent.Create(new CreateRestrictionRequest(
            personId,
            "Temporary restriction for integration testing.",
            DateTimeOffset.UtcNow.AddHours(1)));

        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<ReadinessOverrideResponse>())!;

        var liftResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/integrations/restrictions/{created.OverrideId}/lift", token));
        liftResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Integrations_index_omits_audit_package_surface()
    {
        var serviceToken = CreateServiceToken("maintainarr", IntegrationEndpoints.PermissionCheckReadActionScope);
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations", serviceToken));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, IReadOnlyList<Dictionary<string, string>>>>();
        Assert.NotNull(payload);
        var items = payload!["items"];
        Assert.DoesNotContain(items, item => item.TryGetValue("key", out var key) && key == "audit-packages");
    }

    [Fact]
    public async Task Integrations_index_rejects_user_launch_tokens()
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations", CreateStaffArrToken()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Reference_types_hide_staffarr_catalog_from_tenant_member()
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/reference-types", CreateStaffArrToken("tenant_member")));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ReferenceTypeDescriptor[]>();
        Assert.NotNull(payload);
        Assert.Empty(payload!);
    }

    [Fact]
    public async Task Reference_types_hide_staffarr_catalog_from_platform_admin_without_staffarr_role()
    {
        var response = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/v1/integrations/reference-types",
                CreateStaffArrToken("routarr_driver", isPlatformAdmin: true)));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ReferenceTypeDescriptor[]>();
        Assert.NotNull(payload);
        Assert.Empty(payload!);
    }

    [Fact]
    public async Task Reference_types_catalog_includes_site_quick_create_for_staffarr_admin()
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/reference-types", CreateStaffArrToken()));

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ReferenceTypeDescriptor[]>();
        Assert.NotNull(payload);

        var siteDescriptor = Assert.Single(payload!, item => item.ReferenceType == "site");
        Assert.Equal("staffarr", siteDescriptor.OwnerProductKey);
        Assert.Equal("Site", siteDescriptor.Label);
        Assert.True(siteDescriptor.CanQuickCreate);
        Assert.Equal("staffarr.sites.quick_create", siteDescriptor.QuickCreatePermission);
        Assert.Equal("StaffArr-owned site org unit reference.", siteDescriptor.Description);
    }

    [Fact]
    public async Task Site_quick_create_schema_exposes_the_governed_site_fields()
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/references/site/quick-create-schema", CreateStaffArrToken()));

        response.EnsureSuccessStatusCode();

        var schema = await response.Content.ReadFromJsonAsync<QuickCreateSchemaResponse>();
        Assert.NotNull(schema);
        Assert.True(schema!.Allowed);
        Assert.Equal("staffarr", schema.OwnerProductKey);
        Assert.Equal("site", schema.ReferenceType);
        Assert.Equal("StaffArr", schema.ManagedByLabel);
        Assert.Equal("staffarr.sites.quick_create", schema.PermissionKey);

        var nameField = Assert.Single(schema.Fields!, field => field.Key == "name");
        Assert.True(nameField.Required);

        var siteTypeField = Assert.Single(schema.Fields!, field => field.Key == "siteType");
        Assert.Equal("select", siteTypeField.FieldType);
        Assert.Equal("other", siteTypeField.DefaultValue);
        Assert.Contains(siteTypeField.Options!, option => option.Value == "office");
        Assert.Contains(siteTypeField.Options!, option => option.Value == "yard");

        var timezoneField = Assert.Single(schema.Fields!, field => field.Key == "timezone");
        Assert.Equal("America/Chicago", timezoneField.Placeholder);
        Assert.Contains(schema.Fields!, field => field.Key == "emergencyContact");
        Assert.Contains(schema.Fields!, field => field.Key == "description");
    }

    [Fact]
    public async Task Site_quick_create_round_trip_creates_a_reference_summary()
    {
        var createRequest = Authorized(
            HttpMethod.Post,
            "/api/v1/integrations/references/site/quick-create",
            CreateStaffArrToken());
        createRequest.Content = JsonContent.Create(
            new QuickCreateRequest(
                "site",
                new Dictionary<string, string>
                {
                    ["name"] = "North Yard Quick Create",
                    ["code"] = "NYQ-001",
                    ["siteType"] = "yard",
                    ["timezone"] = "America/Chicago",
                    ["phone"] = "555-1000",
                    ["emergencyContact"] = "On Call Supervisor",
                    ["description"] = "Created by the integration test."
                }));

        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<QuickCreateResponse>();
        Assert.NotNull(created);
        Assert.True(created!.Created);
        Assert.Empty(created.DuplicateCandidates);
        Assert.Equal("planned", created.ReviewStatus);
        Assert.Equal("Site was created in StaffArr as a planned org unit.", created.Message);
        Assert.NotNull(created.Reference);
        Assert.Equal("staffarr", created.Reference!.OwnerProductKey);
        Assert.Equal("site", created.Reference.ReferenceType);
        Assert.Equal("North Yard Quick Create", created.Reference.DisplayLabelSnapshot);
        Assert.Equal("planned", created.Reference.StatusSnapshot);
        Assert.Equal("quick_create", created.Reference.CreatedVia);

        var summaryResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/integrations/references/site/{created.Reference.ReferenceId}/summary",
                CreateStaffArrToken()));

        summaryResponse.EnsureSuccessStatusCode();

        var summary = await summaryResponse.Content.ReadFromJsonAsync<ReferenceSummaryResponse>();
        Assert.NotNull(summary);
        Assert.Equal("staffarr", summary!.OwnerProductKey);
        Assert.Equal("site", summary.ReferenceType);
        Assert.Equal("North Yard Quick Create", summary.DisplayLabel);
        Assert.Equal("NYQ-001 / yard / planned", summary.SecondaryLabel);
        Assert.Equal("planned", summary.Status);
        Assert.Equal("/organization/" + created.Reference.ReferenceId, summary.DetailPath);
        Assert.Equal("site", summary.Metadata!["unitType"]);
        Assert.Equal("NYQ-001", summary.Metadata["code"]);
        Assert.Equal("yard", summary.Metadata["siteType"]);
        Assert.Equal("America/Chicago", summary.Metadata["timezone"]);
        Assert.Equal("555-1000", summary.Metadata["phone"]);
    }

    [Fact]
    public async Task Location_reference_search_respects_site_filter()
    {
        var request = new ReferenceSearchRequest(
            "location",
            "Dock",
            25,
            new Dictionary<string, string>
            {
                ["siteOrgUnitId"] = "22222222-2222-2222-2222-222222222222"
            });
        var searchRequest = Authorized(
            HttpMethod.Post,
            "/api/v1/integrations/references/search",
            CreateStaffArrToken());
        searchRequest.Content = JsonContent.Create(request);

        var searchResponse = await _client.SendAsync(searchRequest);
        searchResponse.EnsureSuccessStatusCode();

        var search = await searchResponse.Content.ReadFromJsonAsync<ReferenceSearchResponse>();
        Assert.NotNull(search);
        var result = Assert.Single(search!.Results);
        Assert.Equal("staffarr", result.OwnerProductKey);
        Assert.Equal("location", result.ReferenceType);
        Assert.Equal("North Dock", result.DisplayLabel);
        Assert.Equal("22222222-2222-2222-2222-222222222222", result.Metadata!["siteOrgUnitId"]);
        Assert.Equal("dock", result.Metadata["locationType"]);
        Assert.Equal("LOC-100", result.Metadata["locationNumber"]);
    }

    [Fact]
    public async Task Site_quick_create_schema_rejects_platform_admin_without_staffarr_role()
    {
        var response = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/v1/integrations/references/site/quick-create-schema",
                CreateStaffArrToken("routarr_driver", isPlatformAdmin: true)));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Location_quick_create_schema_rejects_platform_admin_without_staffarr_role()
    {
        var response = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/v1/integrations/references/location/quick-create-schema",
                CreateStaffArrToken("routarr_driver", isPlatformAdmin: true)));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task SeedAsync(StaffArrDbContext db)
    {
        var now = DateTimeOffset.UtcNow;
        db.People.Add(new StaffPerson
        {
            Id = PlatformSeeder.DemoAdminUserId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = "Integration",
            FamilyName = "Tester",
            DisplayName = "Integration Tester",
            PrimaryEmail = "integration.tester@example.com",
            EmploymentStatus = "active",
            CreatedAt = now,
            UpdatedAt = now
        });

        db.OrgUnits.Add(new OrgUnit
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            TenantId = PlatformSeeder.DemoTenantId,
            UnitType = "site",
            Name = "Integration Site",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        });

        db.OrgUnits.Add(new OrgUnit
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            TenantId = PlatformSeeder.DemoTenantId,
            UnitType = "site",
            Name = "Secondary Integration Site",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        });

        db.OrgUnits.Add(new OrgUnit
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            TenantId = PlatformSeeder.DemoTenantId,
            UnitType = "department",
            Name = "Integration Department",
            ParentOrgUnitId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        });

        db.InternalLocations.AddRange(
            new InternalLocation
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                TenantId = PlatformSeeder.DemoTenantId,
                LocationNumber = "LOC-100",
                Name = "North Dock",
                LocationType = "dock",
                SiteOrgUnitId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Status = "active",
                AllowedProductUsage = "all",
                CreatedAt = now,
                UpdatedAt = now
            },
            new InternalLocation
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                TenantId = PlatformSeeder.DemoTenantId,
                LocationNumber = "LOC-200",
                Name = "South Dock",
                LocationType = "dock",
                SiteOrgUnitId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Status = "active",
                AllowedProductUsage = "all",
                CreatedAt = now,
                UpdatedAt = now
            });

        await db.SaveChangesAsync();
    }

    private async Task<Guid> SeedPersonOnlyAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var personId = Guid.NewGuid();
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = "Route",
            FamilyName = "Person",
            DisplayName = "Route Person",
            PrimaryEmail = $"route.person.{Guid.NewGuid():N}@example.com",
            EmploymentStatus = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
        return personId;
    }

    private string CreateStaffArrToken(string tenantRoleKey = "tenant_admin", bool isPlatformAdmin = false)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<StaffArrTokenService>();
        var (token, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Integration Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            ["staffarr"],
            isPlatformAdmin);
        return token;
    }

    private string CreateServiceToken(string sourceProduct, string actionScope)
    {
        using var scope = _factory.Services.CreateScope();
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
            new(StlServiceTokenClaimTypes.AllowedProducts, "staffarr"),
            new(StlServiceTokenClaimTypes.TokenId, tokenId.ToString()),
            new(StlServiceTokenClaimTypes.TenantScope, PlatformSeeder.DemoTenantId.ToString()),
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
