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
        await fixture.Store.CloseFiscalPeriodAsync(fixture.Principal, fixture.Period.Id, "Close again before hard lock");
        await fixture.Store.LockFiscalPeriodAsync(fixture.Principal, fixture.Period.Id, "Lock test period");
        var locked = await Assert.ThrowsAsync<StlApiException>(() =>
            fixture.Store.PostJournalAsync(fixture.Principal, journal.Journal.Id));
        Assert.Equal("ledgarr.period.locked", locked.Code);
    }

    [Fact]
    public async Task Period_status_changes_require_reason_and_valid_transition()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);

        var missingReason = await Assert.ThrowsAsync<StlApiException>(() =>
            fixture.Store.CloseFiscalPeriodAsync(fixture.Principal, fixture.Period.Id, null));
        Assert.Equal("ledgarr.validation.required", missingReason.Code);

        var invalidLock = await Assert.ThrowsAsync<StlApiException>(() =>
            fixture.Store.LockFiscalPeriodAsync(fixture.Principal, fixture.Period.Id, "Attempt hard close before soft close"));
        Assert.Equal("ledgarr.period.lock_requires_closed", invalidLock.Code);

        await fixture.Store.CloseFiscalPeriodAsync(fixture.Principal, fixture.Period.Id, "Soft close complete");
        await fixture.Store.LockFiscalPeriodAsync(fixture.Principal, fixture.Period.Id, "Hard close complete");

        var invalidReopen = await Assert.ThrowsAsync<StlApiException>(() =>
            fixture.Store.CloseFiscalPeriodAsync(fixture.Principal, fixture.Period.Id, "Close again without reopening"));
        Assert.Equal("ledgarr.period.close_requires_open", invalidReopen.Code);
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
    public async Task Fixed_asset_listing_includes_next_schedule_summary()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);

        var asset = await fixture.Store.CapitalizeFixedAssetAsync(
            fixture.Principal,
            new CapitalizeAssetRequest(
                fixture.Entity.Id,
                new StlProductObjectReference("maintainarr", "asset", "asset-200", "AST-200"),
                "vehicle",
                fixture.AccountingDate,
                2400m,
                "straight_line",
                24,
                0m));

        var assets = await fixture.Store.ListFixedAssetsAsync(fixture.Principal);
        var listed = Assert.Single(assets, item => item.Id == asset.Id);

        Assert.Equal(asset.AssetNumber, listed.AssetNumber);
        Assert.Equal(fixture.AccountingDate.AddMonths(1), listed.NextDepreciationDate);
        Assert.Equal(24, listed.RemainingScheduleCount);
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
    public async Task Budget_listing_returns_line_count_and_total()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);

        var budget = await fixture.Store.CreateBudgetAsync(
            fixture.Principal,
            new CreateBudgetRequest(
                fixture.Entity.Id,
                "Field operations budget",
                [
                    new CreateBudgetLineRequest("5000", null, 100m, 0.80m, 1.00m),
                    new CreateBudgetLineRequest("5100", "department=maintenance", 250m, 0.85m, 1.00m),
                ]));

        var budgets = await fixture.Store.ListBudgetsAsync(fixture.Principal);
        var listed = Assert.Single(budgets, item => item.Id == budget.Id);

        Assert.Equal(2, listed.LineCount);
        Assert.Equal(350m, listed.TotalBudgetAmount);
    }

    [Fact]
    public async Task Payment_run_and_customer_payment_lists_return_operational_summaries()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);
        var bill = await fixture.Store.CreateVendorBillAsync(
            fixture.Principal,
            new CreateVendorBillRequest(
                fixture.Entity.Id,
                new StlProductObjectReference("supplyarr", "vendor", "vendor-200", "VEN-200"),
                "Riverfront Supply",
                "INV-200",
                fixture.AccountingDate,
                fixture.AccountingDate.AddDays(15),
                "USD",
                200m,
                0m,
                200m,
                [new VendorBillLineRequest(1, "Inventory", 1m, 200m, 200m)]));
        await fixture.Store.MatchVendorBillAsync(
            fixture.Principal,
            bill.Id,
            new MatchVendorBillRequest(
                new StlProductObjectReference("supplyarr", "purchase_order", "po-200", "PO-200"),
                200m,
                0m));
        await fixture.Store.ApproveVendorBillAsync(fixture.Principal, bill.Id);
        await fixture.Store.PostVendorBillAsync(fixture.Principal, bill.Id);

        var run = await fixture.Store.CreatePaymentRunAsync(fixture.Principal, new PaymentRunRequest([bill.Id]));
        await fixture.Store.ApprovePaymentRunAsync(fixture.Principal, run.Id);
        await fixture.Store.ExportPaymentRunAsync(fixture.Principal, run.Id);

        var invoice = await fixture.Store.CreateCustomerInvoiceAsync(
            fixture.Principal,
            new CreateCustomerInvoiceRequest(
                fixture.Entity.Id,
                new StlProductObjectReference("customarr", "customer", "cust-200", "CUS-200"),
                "Ozark Fleet",
                "INV-AR-200",
                fixture.AccountingDate,
                fixture.AccountingDate.AddDays(10),
                "USD",
                200m,
                0m,
                200m,
                [new CustomerInvoiceLineRequest(1, "Service", 1m, 200m, 200m)]));
        var payment = await fixture.Store.CreateCustomerPaymentAsync(
            fixture.Principal,
            new CreateCustomerPaymentRequest(
                fixture.Entity.Id,
                new StlProductObjectReference("customarr", "customer", "cust-200", "CUS-200"),
                125m,
                [new CustomerPaymentApplicationRequest(invoice.Id, 100m)]));

        var paymentRuns = await fixture.Store.ListPaymentRunsAsync(fixture.Principal);
        var listedRun = Assert.Single(paymentRuns, item => item.Id == run.Id);
        Assert.Equal("exported", listedRun.LatestExportStatus);
        Assert.NotNull(listedRun.LatestExportedAt);

        var customerPayments = await fixture.Store.ListCustomerPaymentsAsync(fixture.Principal);
        var listedPayment = Assert.Single(customerPayments, item => item.Id == payment.Id);
        Assert.Equal(100m, listedPayment.AppliedAmount);
    }

    [Fact]
    public async Task Banking_workspace_records_accounts_transactions_and_balanced_reconciliations()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);

        var account = await fixture.Store.CreateBankAccountAsync(
            fixture.Principal,
            new CreateBankAccountRequest(
                fixture.Entity.Id,
                "First Midwest Bank",
                "Operating cash",
                "checking",
                "****4321",
                "USD",
                fixture.Account("1000").Id,
                true));
        var transactionOne = await fixture.Store.CreateBankTransactionAsync(
            fixture.Principal,
            new CreateBankTransactionRequest(account.Id, fixture.AccountingDate, "Customer deposit", 150m, "debit", "imported", "unmatched"));
        var transactionTwo = await fixture.Store.CreateBankTransactionAsync(
            fixture.Principal,
            new CreateBankTransactionRequest(account.Id, fixture.AccountingDate.AddDays(1), "Vendor ACH", -50m, "credit", "imported", "unmatched"));

        await fixture.Store.MatchBankTransactionAsync(
            fixture.Principal,
            transactionOne.Id,
            new MatchBankTransactionRequest("customer_payment", "pay-100"));

        var reconciliation = await fixture.Store.CreateBankReconciliationAsync(
            fixture.Principal,
            new CreateBankReconciliationRequest(
                account.Id,
                fixture.AccountingDate,
                fixture.AccountingDate.AddDays(1),
                1000m,
                1100m,
                fixture.AccountingDate.AddDays(1),
                0m,
                [transactionOne.Id, transactionTwo.Id]));

        Assert.Equal("balanced", reconciliation.Status);
        Assert.Equal(0, reconciliation.ExceptionCount);

        await fixture.Store.ApproveBankReconciliationAsync(fixture.Principal, reconciliation.Id);
        await fixture.Store.LockBankReconciliationAsync(fixture.Principal, reconciliation.Id);

        var accounts = await fixture.Store.ListBankAccountsAsync(fixture.Principal);
        var listedAccount = Assert.Single(accounts, item => item.Id == account.Id);
        Assert.Equal("1000", listedAccount.GLCashAccountCode);

        var transactions = await fixture.Store.ListBankTransactionsAsync(fixture.Principal, account.Id);
        Assert.Equal(2, transactions.Count);
        Assert.Contains(transactions, item => item.Id == transactionOne.Id && item.MatchStatus == "matched");

        var reconciliations = await fixture.Store.ListBankReconciliationsAsync(fixture.Principal);
        var listedReconciliation = Assert.Single(reconciliations, item => item.Id == reconciliation.Id);
        Assert.Equal("approved", listedReconciliation.ApprovalStatus);
        Assert.Equal("locked", listedReconciliation.LockStatus);
    }

    [Fact]
    public async Task Tax_workspace_and_intercompany_relationship_lists_reflect_created_records()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);
        var secondEntity = await fixture.Store.CreateFinancialLegalEntityAsync(
            fixture.Principal,
            new CreateFinancialLegalEntityRequest("SUB01", "Subsidiary One", "subsidiary", "USD", null, null));

        var taxCode = await fixture.Store.CreateTaxCodeAsync(
            fixture.Principal,
            new CreateTaxCodeRequest("TX-STL", "STL Sales Tax", "active"));
        var taxAdjustment = await fixture.Store.CreateTaxAdjustmentAsync(
            fixture.Principal,
            new CreateTaxAdjustmentRequest(
                fixture.Entity.Id,
                taxCode.Id,
                fixture.AccountingDate,
                18.25m,
                "USD",
                "Quarter-end tax accrual true-up",
                "posted"));
        var relationship = await fixture.Store.CreateFinancialLegalEntityRelationshipAsync(
            fixture.Principal,
            new CreateFinancialLegalEntityRelationshipRequest(fixture.Entity.Id, secondEntity.Id, "intercompany", 1.0m, "active"));

        var taxCodes = await fixture.Store.ListTaxCodesAsync(fixture.Principal);
        Assert.Single(taxCodes, code => code.Id == taxCode.Id);
        var taxAdjustments = await fixture.Store.ListTaxAdjustmentsAsync(fixture.Principal);
        var listedAdjustment = Assert.Single(taxAdjustments, adjustment => adjustment.Id == taxAdjustment.Id);
        Assert.Equal("TX-STL", listedAdjustment.TaxCodeKey);

        var liabilities = await fixture.Store.ListTaxLiabilitySummariesAsync(fixture.Principal);
        var liability = Assert.Single(liabilities, item => item.TaxCodeId == taxCode.Id);
        Assert.Equal(18.25m, liability.LiabilityAmount);
        Assert.Equal(1, liability.AdjustmentCount);

        var relationships = await fixture.Store.ListFinancialLegalEntityRelationshipsAsync(fixture.Principal);
        var listedRelationship = Assert.Single(relationships, item => item.Id == relationship.Id);
        Assert.Equal(fixture.Entity.DisplayName, listedRelationship.ParentDisplayName);
        Assert.Equal(secondEntity.DisplayName, listedRelationship.ChildDisplayName);
    }

    [Fact]
    public async Task Intercompany_transactions_and_balance_summaries_reflect_settlement_state()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);
        var secondEntity = await fixture.Store.CreateFinancialLegalEntityAsync(
            fixture.Principal,
            new CreateFinancialLegalEntityRequest("SUB02", "Subsidiary Two", "subsidiary", "USD", null, null));
        secondEntity.FiscalCalendarId = fixture.Period.FiscalCalendarId;
        await db.SaveChangesAsync();
        await fixture.Store.CreateFiscalPeriodAsync(
            fixture.Principal,
            new CreateFiscalPeriodRequest(
                fixture.Period.FiscalCalendarId,
                secondEntity.Id,
                "2026-01-SUB02",
                "January 2026 - Subsidiary Two",
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 1, 31)));
        var relationship = await fixture.Store.CreateFinancialLegalEntityRelationshipAsync(
            fixture.Principal,
            new CreateFinancialLegalEntityRelationshipRequest(fixture.Entity.Id, secondEntity.Id, "intercompany", 1.0m, "active"));

        var transaction = await fixture.Store.CreateIntercompanyTransactionAsync(
            fixture.Principal,
            new CreateIntercompanyTransactionRequest(
                relationship.Id,
                fixture.Entity.Id,
                secondEntity.Id,
                fixture.AccountingDate,
                fixture.AccountingDate.AddDays(10),
                250m,
                "USD",
                "Shared services recharge",
                "due_to_due_from",
                "posted",
                "open"));

        var transactions = await fixture.Store.ListIntercompanyTransactionsAsync(fixture.Principal);
        var listedTransaction = Assert.Single(transactions, item => item.Id == transaction.Id);
        Assert.Equal("open", listedTransaction.SettlementStatus);

        var balances = await fixture.Store.ListIntercompanyBalanceSummariesAsync(fixture.Principal);
        var balance = Assert.Single(balances, item => item.FromFinancialLegalEntityId == fixture.Entity.Id && item.ToFinancialLegalEntityId == secondEntity.Id);
        Assert.Equal(250m, balance.OpenAmount);
        Assert.Equal(1, balance.OpenTransactionCount);

        await fixture.Store.SettleIntercompanyTransactionAsync(fixture.Principal, transaction.Id, "Offset through month-end settlement");

        var settledTransactions = await fixture.Store.ListIntercompanyTransactionsAsync(fixture.Principal);
        var settled = Assert.Single(settledTransactions, item => item.Id == transaction.Id);
        Assert.Equal("settled", settled.SettlementStatus);

        var settledBalances = await fixture.Store.ListIntercompanyBalanceSummariesAsync(fixture.Principal);
        Assert.DoesNotContain(settledBalances, item => item.FromFinancialLegalEntityId == fixture.Entity.Id && item.ToFinancialLegalEntityId == secondEntity.Id);
    }

    [Fact]
    public async Task External_finance_listings_include_system_and_batch_history()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);
        var journal = await CreateBalancedJournalAsync(fixture, "Posted external export entry");
        await fixture.Store.PostJournalAsync(fixture.Principal, journal.Journal.Id);

        var system = await fixture.Store.CreateExternalFinanceSystemAsync(
            fixture.Principal,
            new CreateExternalFinanceSystemRequest("intacct", "Sage Intacct", "export_only"));
        var batch = await fixture.Store.CreateExternalPostingBatchAsync(
            fixture.Principal,
            new CreateExternalPostingBatchRequest(system.Id, [journal.Journal.Id]));
        await fixture.Store.ExportExternalPostingBatchAsync(fixture.Principal, batch.Id);

        var systems = await fixture.Store.ListExternalFinanceSystemsAsync(fixture.Principal);
        Assert.Single(systems, item => item.Id == system.Id);

        var batches = await fixture.Store.ListExternalPostingBatchesAsync(fixture.Principal);
        var listedBatch = Assert.Single(batches, item => item.Id == batch.Id);
        Assert.Equal("Sage Intacct", listedBatch.ExternalFinanceSystemDisplayName);
        Assert.Equal(1, listedBatch.JournalCount);
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

    [Fact]
    public async Task Billing_billable_events_progress_from_packet_to_invoice_draft_signal()
    {
        await using var db = CreateDb();
        var fixture = await BootstrapAsync(db);
        var packet = await fixture.Store.IngestFinancialPacketAsync(
            fixture.Principal,
            new FinancialPacketIngestRequest(
                fixture.Entity.Id,
                "ordarr",
                "evt-order-300",
                1,
                "source_document",
                "order-300",
                "Order 300",
                DateTimeOffset.UtcNow,
                "customer_invoice",
                "freight_charge",
                fixture.AccountingDate,
                "USD",
                150m,
                0m,
                150m,
                [
                    new FinancialPacketLineRequest(
                        1,
                        "line-1",
                        "service",
                        null,
                        null,
                        new StlProductObjectReference("customarr", "customer", "cust-300", "CUS-300"),
                        null,
                        null,
                        new StlProductObjectReference("ordarr", "order", "ORD-300", "ORD-300"),
                        null,
                        null,
                        null,
                        1m,
                        "EA",
                        150m,
                        150m,
                        0m,
                        150m,
                        null,
                        null,
                        null,
                        "freight_charge"),
                ],
                [new FinancialPacketSourceRefRequest("ordarr", "order", "order-300", "Order 300", "evt-order-300", 1, "billing snapshot")],
                null,
                null,
                null,
                "packet-billing-300"));

        var billableEvent = await fixture.Store.CreateBillableEventFromPacketAsync(fixture.Principal, packet.Packet.Id);
        await fixture.Store.ApproveBillableEventAsync(fixture.Principal, billableEvent.Id);
        await fixture.Store.GenerateInvoiceDraftForBillableEventAsync(fixture.Principal, billableEvent.Id);

        var events = await fixture.Store.ListBillableEventsAsync(fixture.Principal);
        var listed = Assert.Single(events, item => item.Id == billableEvent.Id);
        Assert.Equal("approved", listed.ApprovalStatus);
        Assert.Equal("draft_generated", listed.InvoiceStatus);
        Assert.Equal("cust-300", listed.CustomerRefId);
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
