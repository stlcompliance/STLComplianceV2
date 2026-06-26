using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using LedgArr.Api.Data;
using LedgArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.LedgArr.Tests;

public sealed class LedgArrTenantSettingsServiceTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");
    private static readonly Guid UserId = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");
    private static readonly Guid PersonId = Guid.Parse("cccccccc-3333-3333-3333-333333333333");

    [Fact]
    public async Task New_tenant_receives_default_settings()
    {
        await using var db = CreateDb();
        await BootstrapLedgArrAsync(db);
        var service = new LedgArrTenantSettingsService(db);

        var settings = await service.GetTenantSettingsAsync(TenantId, Principal(["ledgarr"], ["ledgarr.settings.view"]), default);

        var generalLedger = settings.Sections.Single(section => section.SectionKey == "generalLedger");
        var serialized = JsonSerializer.Serialize(generalLedger.Value);

        Assert.Contains("\"accountingBasis\":\"accrual\"", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"baseCurrency\":\"USD\"", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task High_impact_change_requires_reason()
    {
        await using var db = CreateDb();
        await BootstrapLedgArrAsync(db);
        var service = new LedgArrTenantSettingsService(db);

        var ex = await Assert.ThrowsAsync<StlApiException>(() =>
            service.UpdateTenantSettingsSectionAsync(
                TenantId,
                "generalLedger",
                UpdateRequest(new
                {
                    accountingBasis = "cash",
                    baseCurrency = "USD",
                    reportingCurrency = "USD",
                }),
                Principal(["ledgarr"], ["ledgarr.settings.view", "ledgarr.settings.manage"]),
                default));

        Assert.Equal("ledgarr.settings.reason_required", ex.Code);
    }

    [Fact]
    public async Task Settings_permissions_are_enforced()
    {
        await using var db = CreateDb();
        await BootstrapLedgArrAsync(db);
        var service = new LedgArrTenantSettingsService(db);

        var ex = await Assert.ThrowsAsync<StlApiException>(() =>
            service.GetTenantSettingsAsync(TenantId, Principal(["ledgarr"], []), default));

        Assert.Equal("ledgarr.settings.forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task Finance_packet_validation_honors_posting_source_and_legal_entity_requirements()
    {
        await using var db = CreateDb();
        var store = await BootstrapLedgArrAsync(db);
        var service = new LedgArrTenantSettingsService(db);
        var principal = Principal(
            ["ledgarr"],
            ["ledgarr.settings.view", "ledgarr.settings.manage", "ledgarr.postingRules.view", "ledgarr.postingRules.manage"]);

        await service.UpdateTenantSettingsSectionAsync(
            TenantId,
            "postingSources",
            UpdateRequest(new PostingSourceSettingsSection
            {
                MaintainArr = new MaintainArrPostingSettings
                {
                    PostWorkOrderLaborCosts = false,
                    PostPartsConsumption = false,
                    PostOutsideVendorRepairInvoices = false,
                },
            }, reason: "Disable MaintainArr packet posting for validation coverage."),
            principal,
            default);

        var packet = await store.IngestFinancialPacketAsync(
            Principal(["ledgarr"]),
            new FinancialPacketIngestRequest(
                null,
                "maintainarr",
                "evt-maint-100",
                1,
                "work_order",
                "wo-100",
                "WO-100",
                DateTimeOffset.UtcNow,
                "manual_adjustment",
                null,
                new DateOnly(2026, 1, 1),
                "USD",
                75m,
                0m,
                75m,
                [
                    new FinancialPacketLineRequest(
                        1,
                        "line-1",
                        "expense",
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        1m,
                        "EA",
                        75m,
                        75m,
                        0m,
                        75m,
                        null,
                        "expense",
                        null,
                        null),
                ],
                [new FinancialPacketSourceRefRequest("maintainarr", "work_order", "wo-100", "WO-100", "evt-maint-100", 1, "maint snapshot")],
                null,
                null,
                null,
                "packet-maint-100"),
            default);

        Assert.Equal("validation_failed", packet.Packet.Status);
        Assert.Contains(packet.ValidationIssues, issue => issue.Code == "missingLegalEntity");
        Assert.Contains(packet.ValidationIssues, issue => issue.Code == "sourceProductPostingDisabled");
    }

    private static LedgArrTenantSettingsUpdateRequest UpdateRequest(object value, string? reason = null)
    {
        var json = JsonSerializer.SerializeToElement(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return new LedgArrTenantSettingsUpdateRequest(json, null, reason);
    }

    private static async Task<LedgArrStore> BootstrapLedgArrAsync(LedgArrDbContext db)
    {
        var store = new LedgArrStore(db);
        await store.GetDashboardAsync(Principal(["ledgarr"]));
        return store;
    }

    private static LedgArrDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<LedgArrDbContext>()
            .UseInMemoryDatabase($"ledgarr-settings-tests-{Guid.NewGuid():N}")
            .Options;
        return new LedgArrDbContext(options);
    }

    private static ClaimsPrincipal Principal(string[] entitlements, string[]? permissions = null, string tenantRoleKey = "member")
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, UserId.ToString("D")),
            new(ClaimTypes.NameIdentifier, UserId.ToString("D")),
            new(StlClaimTypes.TenantId, TenantId.ToString("D")),
            new(StlClaimTypes.PersonId, PersonId.ToString("D")),
            new(StlClaimTypes.SessionId, Guid.NewGuid().ToString("D")),
            new(StlClaimTypes.TenantRoleKey, tenantRoleKey),
            new(StlClaimTypes.PlatformAdmin, "false"),
            new(StlClaimTypes.LaunchableProductKeys, string.Join(',', entitlements)),
        };

        if (permissions is { Length: > 0 })
        {
            claims.Add(new Claim("permissions", string.Join(',', permissions)));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}

