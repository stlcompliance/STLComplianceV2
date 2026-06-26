using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CustomArr.Api.Data;
using CustomArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace STLCompliance.CustomArr.Api.Tests;

public sealed class CustomArrCrmWorkspaceServiceTests
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid PersonId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task Service_allows_users_after_non_customarr_launch_context()
    {
        await using var db = CreateDb();
        var service = new CustomArrCrmWorkspaceService(db);

        var leads = await service.ListLeadsAsync(Principal("nexarr"));

        Assert.Empty(leads);
    }

    [Fact]
    public async Task Create_lead_is_idempotent()
    {
        await using var db = CreateDb();
        var service = new CustomArrCrmWorkspaceService(db);
        var principal = Principal(StlProductKeys.CustomArr);
        var request = new CustomArrCreateLeadRequest(
            "Harbor View Cold Chain",
            "Avery Nolan",
            "avery@example.test",
            "555-0100",
            "referral",
            "new",
            82,
            "Recurring cold-chain work",
            null,
            null,
            null,
            "warehouse, transportation",
            null,
            null,
            DateTimeOffset.Parse("2026-06-20T15:00:00Z"));

        var first = await service.CreateLeadAsync(principal, request, "lead-create-1");
        var second = await service.CreateLeadAsync(principal, request, "lead-create-1");

        Assert.Equal(first.Id, second.Id);
        Assert.Single(db.Leads);
        Assert.Single(db.IdempotencyRecords, x => x.OperationKey == "customarr.lead.create");
    }

    [Fact]
    public async Task Lead_conversion_creates_customer_and_opportunity_once()
    {
        await using var db = CreateDb();
        var service = new CustomArrCrmWorkspaceService(db);
        var principal = Principal(StlProductKeys.CustomArr);
        var lead = await service.CreateLeadAsync(
            principal,
            new CustomArrCreateLeadRequest("Metro Builders Supply", null, null, null, "web", "qualified", null, "Regional distribution", null, null, null, "transportation", null, null, null),
            "lead-create-2");

        var first = await service.ConvertLeadAsync(
            principal,
            lead.Id,
            new CustomArrConvertLeadRequest(null, "Metro Builders Supply LLC", "Metro Builders", "Launch service package", 42000m),
            "lead-convert-1");
        var second = await service.ConvertLeadAsync(
            principal,
            lead.Id,
            new CustomArrConvertLeadRequest(null, "Metro Builders Supply LLC", "Metro Builders", "Launch service package", 42000m),
            "lead-convert-1");

        Assert.Equal(first.OpportunityId, second.OpportunityId);
        Assert.Single(db.Customers);
        Assert.Single(db.Opportunities);
        Assert.Equal("converted", (await db.Leads.SingleAsync()).StatusKey);
    }

    [Fact]
    public async Task Opportunity_won_creates_idempotent_OrdArr_handoff()
    {
        await using var db = CreateDb();
        SeedCustomer(db);
        var service = new CustomArrCrmWorkspaceService(db);
        var principal = Principal(StlProductKeys.CustomArr);
        var opportunity = await service.CreateOpportunityAsync(
            principal,
            new CustomArrCreateOpportunityRequest("cust-1001", "Recurring service expansion", "proposal", 70, "best_case", null, 125000m, 26000m, [StlProductKeys.OrdArr], "Recurring order coordination", null, "Customer approval", null),
            "opp-create-1");

        var first = await service.MarkOpportunityWonAsync(principal, opportunity.Id, new CustomArrOpportunityWonRequest("Customer accepted scope"), "opp-won-1");
        var second = await service.MarkOpportunityWonAsync(principal, opportunity.Id, new CustomArrOpportunityWonRequest("Customer accepted scope"), "opp-won-1");

        Assert.Equal(StlProductKeys.OrdArr, first.TargetProductKey);
        Assert.Equal(StlProductKeys.OrdArr, second.TargetProductKey);
        Assert.Equal("won", (await db.Opportunities.SingleAsync()).StatusKey);
        Assert.Single(db.IdempotencyRecords, x => x.OperationKey == "customarr.opportunity.win");
    }

    [Fact]
    public async Task Proposal_acceptance_creates_idempotent_OrdArr_handoff()
    {
        await using var db = CreateDb();
        SeedCustomer(db);
        var service = new CustomArrCrmWorkspaceService(db);
        var principal = Principal(StlProductKeys.CustomArr);
        var proposal = await service.CreateProposalAsync(
            principal,
            new CustomArrCreateProposalRequest("cust-1001", null, 1, "sent", "Recurring logistics service package", """{"currency":"USD","monthly":10000}""", "Net 30", null, "approved", DateTimeOffset.Parse("2026-07-17T15:00:00Z")),
            "proposal-create-1");

        var first = await service.AcceptProposalAsync(principal, proposal.Id, new CustomArrProposalAcceptanceRequest("order_request"), "proposal-accept-1");
        var second = await service.AcceptProposalAsync(principal, proposal.Id, new CustomArrProposalAcceptanceRequest("order_request"), "proposal-accept-1");

        Assert.Equal(StlProductKeys.OrdArr, first.TargetProductKey);
        Assert.Equal(StlProductKeys.OrdArr, second.TargetProductKey);
        Assert.Equal("accepted", (await db.Proposals.SingleAsync()).StatusKey);
        Assert.Single(db.IdempotencyRecords, x => x.OperationKey == "customarr.proposal.accept");
    }

    [Fact]
    public async Task Eligibility_blocks_customers_with_CustomArr_hold()
    {
        await using var db = CreateDb();
        SeedCustomer(db, statusKey: "active", holdStatusKey: "blocked");
        var service = new CustomArrCrmWorkspaceService(db);

        var result = await service.CheckEligibilityAsync(
            Principal(StlProductKeys.CustomArr),
            new CustomArrEligibilityCheckRequest("cust-1001", null, null, "ordarr.order.create", StlProductKeys.CustomArr, "test"),
            "eligibility-1",
            requireIdempotency: true);

        Assert.Equal("blocked", result.ResultKey);
        Assert.Contains(result.Blockers, blocker => blocker.Contains("hold", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Reference_search_supports_customer_owned_reference_types()
    {
        await using var db = CreateDb();
        SeedCustomer(db);
        SeedReferenceRecords(db);
        var service = new CustomArrCrmWorkspaceService(db);
        var principal = Principal(StlProductKeys.CustomArr);

        Assert.Contains(await service.SearchReferencesAsync(principal, "customer_location", "Dallas", 10), x => x.Id == "addr-1001");
        Assert.Contains(await service.SearchReferencesAsync(principal, "customer_contact", "Maria", 10), x => x.Id == "ct-1001");
        Assert.Contains(await service.SearchReferencesAsync(principal, "customer_requirement", "Insurance", 10), x => x.Id == "req-1001");
        Assert.Contains(await service.SearchReferencesAsync(principal, "customer_agreement", "Master", 10), x => x.Id == "agr-1001");
        Assert.Contains(await service.SearchReferencesAsync(principal, "customer_case", "Portal", 10), x => x.Id == "case-1001");
    }

    private static CustomArrDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<CustomArrDbContext>()
            .UseInMemoryDatabase($"customarr-crm-{Guid.NewGuid():N}")
            .Options;
        return new CustomArrDbContext(options);
    }

    private static void SeedCustomer(CustomArrDbContext db, string statusKey = "active", string holdStatusKey = "clear")
    {
        db.Customers.Add(new CustomArrCustomer
        {
            CustomerId = "cust-1001",
            TenantId = TenantId,
            CustomerNumber = "CUS-1001",
            CustomerCode = "CUS-1001",
            LegalName = "Acme Freight Systems LLC",
            DisplayName = "Acme Freight",
            CustomerTypeKey = "business",
            StatusKey = statusKey,
            RelationshipRoleKey = "buyer",
            AccountClassKey = "strategic",
            OnboardingStatusKey = "approved",
            ServiceEligibilityStatusKey = "eligible",
            ComplianceStatusKey = "acceptable",
            AccountOwnerPersonId = PersonId.ToString("D"),
            SourceKey = "test",
            Notes = "Seed customer",
            Tags = ["test"],
            HoldStatusKey = holdStatusKey,
            RiskRatingKey = "low",
            PortalDisplayName = "Acme Freight",
            PortalInviteStatusKey = "not_invited",
            DefaultOrderTypeKey = "customer_order",
            DefaultServiceLevelKey = "standard",
            NotificationPreferenceKey = "email",
            CreatedAt = DateTimeOffset.Parse("2026-06-01T15:00:00Z"),
            CreatedByPersonId = PersonId.ToString("D"),
            UpdatedAt = DateTimeOffset.Parse("2026-06-17T15:00:00Z"),
            UpdatedByPersonId = PersonId.ToString("D")
        });
        db.SaveChanges();
    }

    private static void SeedReferenceRecords(CustomArrDbContext db)
    {
        db.CustomerAddresses.Add(new CustomArrCustomerAddress
        {
            AddressId = "addr-1001",
            TenantId = TenantId,
            CustomerId = "cust-1001",
            AddressTypeKey = "service",
            LocationName = "Dallas Service Center",
            Line1 = "410 Harbor Ave",
            City = "Dallas",
            StateProvince = "TX",
            PostalCode = "75201",
            CountryCode = "US",
            StatusKey = "active"
        });
        db.CustomerContacts.Add(new CustomArrCustomerContact
        {
            ContactId = "ct-1001",
            TenantId = TenantId,
            CustomerId = "cust-1001",
            DisplayName = "Maria Jensen",
            Email = "maria@example.test",
            Phone = "555-0101",
            PreferredContactMethodKey = "email",
            StatusKey = "active"
        });
        db.CustomerRequirements.Add(new CustomArrCustomerRequirement
        {
            RequirementId = "req-1001",
            TenantId = TenantId,
            CustomerId = "cust-1001",
            RequirementTypeKey = "insurance",
            RequirementName = "Insurance certificate",
            Description = "Current certificate of insurance",
            StatusKey = "complete",
            OwnerTeam = "Risk"
        });
        db.Agreements.Add(new CustomArrAgreement
        {
            AgreementId = "agr-1001",
            TenantId = TenantId,
            CustomerId = "cust-1001",
            AgreementNumber = "AGR-1001",
            AgreementTypeKey = "master_service_agreement",
            Title = "Master service agreement",
            StatusKey = "active",
            CreatedAt = DateTimeOffset.Parse("2026-06-01T15:00:00Z"),
            UpdatedAt = DateTimeOffset.Parse("2026-06-17T15:00:00Z")
        });
        db.CustomerCases.Add(new CustomArrCustomerCase
        {
            CaseId = "case-1001",
            TenantId = TenantId,
            CustomerId = "cust-1001",
            CaseNumber = "CASE-1001",
            Subject = "Portal notification review",
            Description = "Confirm portal notifications.",
            StatusKey = "new",
            CreatedAt = DateTimeOffset.Parse("2026-06-17T15:00:00Z"),
            UpdatedAt = DateTimeOffset.Parse("2026-06-17T15:00:00Z")
        });
        db.SaveChanges();
    }

    private static ClaimsPrincipal Principal(string? launchableProductKey = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, UserId.ToString("D")),
            new(StlClaimTypes.TenantId, TenantId.ToString("D")),
            new(StlClaimTypes.PersonId, PersonId.ToString("D")),
            new(StlClaimTypes.SessionId, Guid.NewGuid().ToString("D")),
            new(StlClaimTypes.TenantRoleKey, "tenant_admin"),
            new(StlClaimTypes.PlatformAdmin, "false")
        };

        if (!string.IsNullOrWhiteSpace(launchableProductKey))
        {
            claims.Add(new Claim(StlClaimTypes.LaunchableProductKeys, launchableProductKey));
        }

        var identity = new ClaimsIdentity(claims, "test");

        return new ClaimsPrincipal(identity);
    }
}

