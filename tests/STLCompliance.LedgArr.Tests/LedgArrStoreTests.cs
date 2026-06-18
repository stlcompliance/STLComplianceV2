using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LedgArr.Api.Data;
using LedgArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace STLCompliance.LedgArr.Tests;

public sealed class LedgArrStoreTests
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid PersonId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task Store_requires_LedgArr_entitlement()
    {
        await using var db = CreateDb();
        var store = new LedgArrStore(db);

        var ex = await Assert.ThrowsAsync<StlApiException>(() =>
            store.GetDashboardAsync(Principal("ordarr")));

        Assert.Equal("ledgarr.not_entitled", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task FinancialLegalEntity_rejects_ComplianceCore_governing_body_like_values()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);

        var ex = await Assert.ThrowsAsync<StlApiException>(() =>
            fixture.Store.CreateFinancialLegalEntityAsync(
                fixture.Principal,
                new CreateFinancialLegalEntityRequest(
                    "FMCSA",
                    "Federal Motor Carrier Safety Administration",
                    "regulator",
                    "USD",
                    null,
                    null,
                    new StlProductObjectReference("compliancecore", "governing_body", "fmcsa", "FMCSA"))));

        Assert.Equal("ledgarr.financial_legal_entity.governing_body_forbidden", ex.Code);
    }

    [Fact]
    public async Task Manual_journals_must_be_balanced()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);

        var ex = await Assert.ThrowsAsync<StlApiException>(() =>
            fixture.Store.CreateJournalAsync(
                fixture.Principal,
                new CreateJournalRequest(
                    fixture.Entity.Id,
                    fixture.AccountingDate,
                    "Unbalanced manual entry",
                    [
                        new(fixture.Account("1000").Id, 100m, 0m, "Cash debit", null),
                        new(fixture.Account("4000").Id, 0m, 90m, "Revenue credit", null),
                    ])));

        Assert.Equal("ledgarr.posting.unbalanced", ex.Code);
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Period_close_and_lock_rules_block_posting_server_side()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);
        var journal = await CreateBalancedJournalAsync(fixture, "Period enforcement entry");

        await fixture.Store.CloseFiscalPeriodAsync(fixture.Principal, fixture.Period.Id, "Close test period");
        var closed = await Assert.ThrowsAsync<StlApiException>(() =>
            fixture.Store.PostJournalAsync(fixture.Principal, journal.Journal.Id));
        Assert.Equal("ledgarr.period.closed", closed.Code);

        await fixture.Store.ReopenFiscalPeriodAsync(fixture.Principal, fixture.Period.Id, "Reopen for lock test");
        await fixture.Store.LockFiscalPeriodAsync(fixture.Principal, fixture.Period.Id, "Lock test period");
        var locked = await Assert.ThrowsAsync<StlApiException>(() =>
            fixture.Store.PostJournalAsync(fixture.Principal, journal.Journal.Id));
        Assert.Equal("ledgarr.period.locked", locked.Code);
    }

    [Fact]
    public async Task Financial_packet_ingestion_is_idempotent_by_source_event_version()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);
        var request = PacketRequest(fixture, "supplyarr", "evt-po-100", 1, "packet-idem-100");

        var first = await fixture.Store.IngestFinancialPacketAsync(fixture.Principal, request);
        var second = await fixture.Store.IngestFinancialPacketAsync(fixture.Principal, request);

        Assert.Equal(first.Packet.Id, second.Packet.Id);
        Assert.Single(db.FinancialPackets);
        Assert.Single(db.FinancialPacketIdempotencyKeys);
    }

    [Fact]
    public async Task Packet_preview_approval_posts_balanced_journal()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);
        var packet = await fixture.Store.IngestFinancialPacketAsync(
            fixture.Principal,
            PacketRequest(fixture, "loadarr", "evt-receipt-200", 1, "packet-preview-200", totalAmount: 240m));

        var preview = await fixture.Store.CreatePostingPreviewForPacketAsync(fixture.Principal, packet.Packet.Id);
        Assert.NotNull(preview);
        Assert.Equal(preview!.Preview.TotalDebits, preview.Preview.TotalCredits);

        await fixture.Store.ApprovePostingPreviewAsync(fixture.Principal, preview.Preview.Id);
        var posted = await fixture.Store.PostPostingPreviewAsync(fixture.Principal, preview.Preview.Id);

        Assert.NotNull(posted);
        Assert.Equal("posted", posted!.Journal.Status);
        Assert.Equal(posted.Journal.TotalDebits, posted.Journal.TotalCredits);
        Assert.Equal("posted", (await db.FinancialPackets.SingleAsync()).Status);
    }

    [Fact]
    public async Task AP_match_variance_blocks_bill_approval()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);
        var bill = await fixture.Store.CreateVendorBillAsync(
            fixture.Principal,
            new CreateVendorBillRequest(
                fixture.Entity.Id,
                new StlProductObjectReference("supplyarr", "vendor", "vendor-100", "VEN-100"),
                "Metro Industrial Supply",
                "INV-100",
                fixture.AccountingDate,
                fixture.AccountingDate.AddDays(30),
                "USD",
                110m,
                0m,
                110m,
                [new VendorBillLineRequest(1, "Parts", 1m, 110m, 110m)]));

        var matched = await fixture.Store.MatchVendorBillAsync(
            fixture.Principal,
            bill.Id,
            new MatchVendorBillRequest(
                new StlProductObjectReference("supplyarr", "purchase_order", "po-100", "PO-100"),
                100m,
                5m));

        Assert.Equal("variance_open", matched!.MatchStatus);
        var ex = await Assert.ThrowsAsync<StlApiException>(() =>
            fixture.Store.ApproveVendorBillAsync(fixture.Principal, bill.Id));
        Assert.Equal("ledgarr.vendor_bill.unresolved_variance", ex.Code);
    }

    [Fact]
    public async Task AR_invoice_issue_post_and_payment_application_update_ledger_state()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);
        var invoice = await fixture.Store.CreateCustomerInvoiceAsync(
            fixture.Principal,
            new CreateCustomerInvoiceRequest(
                fixture.Entity.Id,
                new StlProductObjectReference("customarr", "customer", "cust-100", "CUS-100"),
                "Acme Freight",
                "INV-AR-100",
                fixture.AccountingDate,
                fixture.AccountingDate.AddDays(30),
                "USD",
                100m,
                8m,
                108m,
                [new CustomerInvoiceLineRequest(1, "Freight revenue", 1m, 100m, 100m)]));

        await fixture.Store.ApproveCustomerInvoiceAsync(fixture.Principal, invoice.Id);
        await fixture.Store.IssueCustomerInvoiceAsync(fixture.Principal, invoice.Id);
        var posted = await fixture.Store.PostCustomerInvoiceAsync(fixture.Principal, invoice.Id);
        var payment = await fixture.Store.CreateCustomerPaymentAsync(
            fixture.Principal,
            new CreateCustomerPaymentRequest(
                fixture.Entity.Id,
                new StlProductObjectReference("customarr", "customer", "cust-100", "CUS-100"),
                108m,
                [new CustomerPaymentApplicationRequest(invoice.Id, 108m)]));

        Assert.Equal("posted", posted!.Status);
        Assert.NotNull(posted.PostedJournalEntryId);
        Assert.Single(db.CustomerPaymentApplications, application => application.CustomerPaymentId == payment.Id);
    }

    [Fact]
    public void Inventory_weighted_average_and_fifo_consumption_are_deterministic()
    {
        var weightedAverage = LedgArrStore.CalculateWeightedAverageCost(
            [new WeightedAverageInput(10m, 2m), new WeightedAverageInput(30m, 4m)]);
        var layerOne = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var layerTwo = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        var fifo = LedgArrStore.CalculateFifoConsumption(
            [
                new FifoLayerInput(layerOne, new DateOnly(2026, 1, 1), 5m, 2m),
                new FifoLayerInput(layerTwo, new DateOnly(2026, 1, 2), 10m, 3m),
            ],
            8m);

        Assert.Equal(3.5m, weightedAverage);
        Assert.Equal(19m, fifo.TotalAmount);
        Assert.Collection(
            fifo.Layers,
            layer =>
            {
                Assert.Equal(layerOne, layer.LayerId);
                Assert.Equal(5m, layer.Quantity);
            },
            layer =>
            {
                Assert.Equal(layerTwo, layer.LayerId);
                Assert.Equal(3m, layer.Quantity);
            });
    }

    [Fact]
    public async Task Fixed_asset_capitalization_generates_straight_line_schedule()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);

        var asset = await fixture.Store.CapitalizeFixedAssetAsync(
            fixture.Principal,
            new CapitalizeAssetRequest(
                fixture.Entity.Id,
                new StlProductObjectReference("maintainarr", "asset", "asset-100", "AST-100"),
                "equipment",
                fixture.AccountingDate,
                1200m,
                "straight_line",
                12,
                0m));

        var schedules = await db.AssetDepreciationSchedules
            .Where(schedule => schedule.FixedAssetFinancialRecordId == asset.Id)
            .OrderBy(schedule => schedule.SequenceNumber)
            .ToListAsync();

        Assert.Equal(12, schedules.Count);
        Assert.All(schedules, schedule => Assert.Equal(100m, schedule.DepreciationAmount));
        Assert.Equal(1200m, schedules.Last().AccumulatedDepreciation);
    }

    [Fact]
    public async Task Budget_thresholds_return_warning_and_blocked_decisions()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);

        await fixture.Store.CreateBudgetAsync(
            fixture.Principal,
            new CreateBudgetRequest(
                fixture.Entity.Id,
                "Operations budget",
                [new CreateBudgetLineRequest("5000", null, 100m, 0.80m, 1.00m)]));

        var warning = await fixture.Store.CheckBudgetAsync(fixture.Principal, new BudgetCheckRequest("5000", 90m, null));
        var blocked = await fixture.Store.CheckBudgetAsync(fixture.Principal, new BudgetCheckRequest("5000", 110m, null));

        Assert.Equal("warning", warning.Decision);
        Assert.Equal("blocked", blocked.Decision);
    }

    [Fact]
    public async Task External_export_rejects_unposted_journals()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);
        var journal = await CreateBalancedJournalAsync(fixture, "Unposted export entry");
        var system = await fixture.Store.CreateExternalFinanceSystemAsync(
            fixture.Principal,
            new CreateExternalFinanceSystemRequest("netsuite", "NetSuite", "external_gl_master"));

        var ex = await Assert.ThrowsAsync<StlApiException>(() =>
            fixture.Store.CreateExternalPostingBatchAsync(
                fixture.Principal,
                new CreateExternalPostingBatchRequest(system.Id, [journal.Journal.Id])));

        Assert.Equal("ledgarr.external_export.unposted_journal", ex.Code);
    }

    private static async Task<JournalEntryResponse> CreateBalancedJournalAsync(LedgArrFixture fixture, string description) =>
        await fixture.Store.CreateJournalAsync(
            fixture.Principal,
            new CreateJournalRequest(
                fixture.Entity.Id,
                fixture.AccountingDate,
                description,
                [
                    new(fixture.Account("1000").Id, 100m, 0m, "Cash debit", null),
                    new(fixture.Account("4000").Id, 0m, 100m, "Revenue credit", null),
                ]));

    private static FinancialPacketIngestRequest PacketRequest(
        LedgArrFixture fixture,
        string sourceProductKey,
        string sourceEventId,
        int sourceEventVersion,
        string idempotencyKey,
        decimal totalAmount = 120m) =>
        new(
            fixture.Entity.Id,
            sourceProductKey,
            sourceEventId,
            sourceEventVersion,
            "source_document",
            $"record-{sourceEventId}",
            $"Source record {sourceEventId}",
            DateTimeOffset.UtcNow,
            "manual_adjustment",
            null,
            fixture.AccountingDate,
            "USD",
            totalAmount,
            0m,
            totalAmount,
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
                    totalAmount,
                    totalAmount,
                    0m,
                    totalAmount,
                    null,
                    "expense",
                    null,
                    null),
            ],
            [new FinancialPacketSourceRefRequest(sourceProductKey, "source_document", $"record-{sourceEventId}", $"Source record {sourceEventId}", sourceEventId, sourceEventVersion, "immutable test snapshot")],
            null,
            null,
            null,
            idempotencyKey);

    private static async Task<LedgArrFixture> BootstrapAsync(LedgArrDbContext db)
    {
        var store = new LedgArrStore(db);
        var principal = Principal("ledgarr");
        await store.GetDashboardAsync(principal);

        var entity = await db.FinancialLegalEntities.SingleAsync();
        var period = await db.FiscalPeriods
            .Where(p => p.FinancialLegalEntityId == entity.Id)
            .OrderByDescending(p => p.StartDate)
            .FirstAsync();
        var accounts = (await db.GLAccounts.ToListAsync())
            .GroupBy(account => account.AccountCode)
            .ToDictionary(group => group.Key, group => group.First());

        return new LedgArrFixture(store, principal, entity, period, accounts);
    }

    private static LedgArrDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<LedgArrDbContext>()
            .UseInMemoryDatabase($"ledgarr-tests-{Guid.NewGuid():N}")
            .Options;
        return new LedgArrDbContext(options);
    }

    private static ClaimsPrincipal Principal(params string[] entitlements)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, UserId.ToString("D")),
                new Claim(ClaimTypes.NameIdentifier, UserId.ToString("D")),
                new Claim(StlClaimTypes.TenantId, TenantId.ToString("D")),
                new Claim(StlClaimTypes.PersonId, PersonId.ToString("D")),
                new Claim(StlClaimTypes.SessionId, Guid.NewGuid().ToString("D")),
                new Claim(StlClaimTypes.TenantRoleKey, "tenant_admin"),
                new Claim(StlClaimTypes.PlatformAdmin, "false"),
                new Claim(StlClaimTypes.Entitlements, string.Join(',', entitlements)),
            ],
            "test");

        return new ClaimsPrincipal(identity);
    }

    private sealed record LedgArrFixture(
        LedgArrStore Store,
        ClaimsPrincipal Principal,
        FinancialLegalEntity Entity,
        FiscalPeriod Period,
        IReadOnlyDictionary<string, GLAccount> Accounts)
    {
        public DateOnly AccountingDate => Period.StartDate;

        public GLAccount Account(string accountCode) => Accounts[accountCode];
    }
}
