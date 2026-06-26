using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CustomArr.Api.Data;
using CustomArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace STLCompliance.CustomArr.Api.Tests;

public sealed class CustomArrTenantSettingsServiceTests
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid PersonId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task Settings_allow_users_after_non_customarr_launch_context()
    {
        await using var db = CreateDb();
        var service = new CustomArrTenantSettingsService(db);

        var settings = await service.GetSettingsAsync(Principal(includeLaunchContext: false));

        Assert.Equal("tenant", settings.Scope);
    }

    [Fact]
    public async Task Settings_bootstrap_seeds_typed_customer_configuration()
    {
        await using var db = CreateDb();
        var service = new CustomArrTenantSettingsService(db);

        var settings = await service.GetSettingsAsync(Principal());

        Assert.Equal("tenant", settings.Scope);
        Assert.Single(db.TenantSettings);
        var bootstrapAudit = Assert.Single(db.TenantSettingsAuditEvents);
        Assert.Equal("bootstrap", bootstrapAudit.SectionKey);
        Assert.Equal(1, bootstrapAudit.SettingsVersion);
        Assert.Contains(settings.LifecycleStages, stage => stage.Key == "active" && stage.IsActiveCustomerStage);
        Assert.Contains(settings.ClassificationCatalogs, item => item.CatalogType == "customer_type" && item.Key == "standard");
        Assert.Contains(settings.RequiredFieldRules, rule => rule.LifecycleStageKey == "active" && rule.FieldKey == "primaryContact");
        Assert.Contains(settings.Warnings, warning => warning.Key == "active_customer_requirements");
    }

    [Fact]
    public async Task Validation_enforces_configured_active_customer_required_fields()
    {
        await using var db = CreateDb();
        var service = new CustomArrTenantSettingsService(db);

        var result = await service.ValidateCustomerAsync(
            Principal(),
            new CustomArrCustomerValidationRequest(
                null,
                "Harbor View Cold Chain LLC",
                null,
                "Harbor View",
                "active",
                "standard",
                null,
                null,
                null,
                null,
                "net_30",
                null,
                null,
                null,
                null,
                null,
                null,
                false,
                false,
                false));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.FieldKey == "primaryContact");
        Assert.Contains(result.Errors, error => error.FieldKey == "billingAddress");
        Assert.Contains(result.Errors, error => error.FieldKey == "accountOwner");
    }

    [Fact]
    public void Customer_create_rejects_active_customer_missing_configured_requirements()
    {
        using var db = CreateDb();
        var store = new CustomArrStore(db);

        var ex = Assert.Throws<StlApiException>(() => store.CreateCustomer(
            Principal(),
            new CustomArrCreateCustomerRequest(
                "Activation Gap Logistics LLC",
                "Activation Gap",
                "active",
                "standard",
                string.Empty,
                PersonId.ToString("D"),
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty),
            "customer-create-active-missing"));

        Assert.Equal("customarr.customer.required_field_missing", ex.Code);
    }

    [Fact]
    public async Task Customer_numbering_uses_tenant_settings_and_advances_sequence()
    {
        await using var db = CreateDb();
        var principal = Principal();
        var settingsService = new CustomArrTenantSettingsService(db);
        var current = await settingsService.GetSettingsAsync(principal);
        await settingsService.UpdateSettingsAsync(
            principal,
            current.ToUpdateRequest() with
            {
                Numbering = current.Numbering with
                {
                    Prefix = "VIP",
                    PaddingLength = 5,
                    NextNumber = 42,
                    DisplayFormat = "{prefix}-{number}"
                }
            });

        var store = new CustomArrStore(db);
        var created = store.CreateCustomer(
            principal,
            new CustomArrCreateCustomerRequest(
                "Numbered Harbor Customer LLC",
                "Numbered Harbor",
                "prospect",
                "standard",
                string.Empty,
                PersonId.ToString("D"),
                string.Empty,
                "Avery Nolan",
                "avery@example.test",
                "555-0100",
                "Dallas",
                "TX",
                "Fort Worth",
                "TX",
                string.Empty),
            "customer-create-numbering");

        Assert.Equal("VIP-00042", created.CustomerNumber);
        Assert.Equal(43, db.CustomerNumberingSettings.Single(x => x.TenantId == TenantId).NextNumber);
        Assert.Contains(db.TenantSettingsAuditEvents, audit =>
            audit.SettingsVersion == 2 &&
            audit.SectionKey == "all" &&
            audit.ChangeSummary.Contains("numbering=VIP-42", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Duplicate_detection_blocks_exact_external_identifier_matches()
    {
        await using var db = CreateDb();
        var principal = Principal();
        var service = new CustomArrTenantSettingsService(db);
        await service.GetSettingsAsync(principal);
        SeedDuplicateCustomer(db);

        var result = await service.CheckDuplicatesAsync(
            principal,
            new CustomArrDuplicateCheckRequest(
                "Acme Freight Systems LLC",
                null,
                "ops@acmefreight.example",
                null,
                "TX-45-9988123",
                "QB-CUST-1001",
                null,
                null,
                null));

        Assert.Equal("block", result.Recommendation);
        var candidate = Assert.Single(result.Candidates);
        Assert.Equal("cust-dupe-1001", candidate.CustomerId);
        Assert.Equal("block", candidate.Recommendation);
        Assert.Contains(candidate.Reasons, reason => reason.Contains("External ID", StringComparison.OrdinalIgnoreCase));
    }

    private static CustomArrDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<CustomArrDbContext>()
            .UseInMemoryDatabase($"customarr-settings-{Guid.NewGuid():N}")
            .Options;
        return new CustomArrDbContext(options);
    }

    private static ClaimsPrincipal Principal(string roleKey = "tenant_admin", bool includeLaunchContext = true)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, UserId.ToString("D")),
            new(StlClaimTypes.TenantId, TenantId.ToString("D")),
            new(StlClaimTypes.PersonId, PersonId.ToString("D")),
            new(StlClaimTypes.SessionId, Guid.NewGuid().ToString("D")),
            new(StlClaimTypes.TenantRoleKey, roleKey),
            new(StlClaimTypes.PlatformAdmin, "false")
        };

        if (includeLaunchContext)
        {
            claims.Add(new Claim(StlClaimTypes.LaunchableProductKeys, StlProductKeys.CustomArr));
        }

        var identity = new ClaimsIdentity(claims, "test");

        return new ClaimsPrincipal(identity);
    }

    private static void SeedDuplicateCustomer(CustomArrDbContext db)
    {
        var customer = new CustomArrCustomer
        {
            CustomerId = "cust-dupe-1001",
            TenantId = TenantId,
            CustomerNumber = "CUS-1001",
            CustomerCode = "CUS-1001",
            LegalName = "Acme Freight Systems LLC",
            DisplayName = "Acme Freight",
            CustomerTypeKey = "standard",
            StatusKey = "active",
            AccountOwnerPersonId = PersonId.ToString("D"),
            SourceKey = "test",
            Notes = "Seeded duplicate candidate",
            Tags = ["test"],
            HoldStatusKey = "clear",
            RiskRatingKey = "low",
            PortalInviteStatusKey = "not_invited",
            CreatedAt = DateTimeOffset.Parse("2026-06-01T15:00:00Z"),
            CreatedByPersonId = PersonId.ToString("D"),
            UpdatedAt = DateTimeOffset.Parse("2026-06-17T15:00:00Z"),
            UpdatedByPersonId = PersonId.ToString("D")
        };
        customer.Contacts.Add(new CustomArrCustomerContact
        {
            ContactId = "ct-dupe-1001",
            TenantId = TenantId,
            CustomerId = customer.CustomerId,
            DisplayName = "Operations",
            Email = "ops@acmefreight.example",
            Phone = "555-0100",
            IsPrimary = true,
            StatusKey = "active"
        });
        customer.Identifiers.Add(new CustomArrCustomerIdentifier
        {
            IdentifierId = "id-dupe-1001",
            TenantId = TenantId,
            CustomerId = customer.CustomerId,
            IdentifierTypeKey = "tax_id",
            IdentifierValue = "TX-45-9988123"
        });
        customer.ExternalRefs.Add(new CustomArrCustomerExternalRef
        {
            ExternalRefId = "xref-dupe-1001",
            TenantId = TenantId,
            CustomerId = customer.CustomerId,
            SystemKey = "quickbooks",
            ExternalId = "QB-CUST-1001",
            ExternalEntityType = "customer",
            SyncStatusKey = "active"
        });
        db.Customers.Add(customer);
        db.SaveChanges();
    }
}

