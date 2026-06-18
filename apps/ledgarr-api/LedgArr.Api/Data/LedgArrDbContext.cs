using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace LedgArr.Api.Data;

public sealed class LedgArrDbContext(DbContextOptions<LedgArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<TenantFinancialProfile> TenantFinancialProfiles => Set<TenantFinancialProfile>();
    public DbSet<FinancialLegalEntity> FinancialLegalEntities => Set<FinancialLegalEntity>();
    public DbSet<FinancialLegalEntityRelationship> FinancialLegalEntityRelationships => Set<FinancialLegalEntityRelationship>();
    public DbSet<FinancialLegalEntityRegistration> FinancialLegalEntityRegistrations => Set<FinancialLegalEntityRegistration>();
    public DbSet<FinancialLegalEntityAddressSnapshot> FinancialLegalEntityAddressSnapshots => Set<FinancialLegalEntityAddressSnapshot>();
    public DbSet<FiscalCalendar> FiscalCalendars => Set<FiscalCalendar>();
    public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<NumberingSequence> NumberingSequences => Set<NumberingSequence>();
    public DbSet<FinancialCloseRun> FinancialCloseRuns => Set<FinancialCloseRun>();
    public DbSet<PeriodLockAudit> PeriodLockAudits => Set<PeriodLockAudit>();
    public DbSet<ChartOfAccounts> ChartsOfAccounts => Set<ChartOfAccounts>();
    public DbSet<GLAccount> GLAccounts => Set<GLAccount>();
    public DbSet<AccountAlias> AccountAliases => Set<AccountAlias>();
    public DbSet<AccountMapping> AccountMappings => Set<AccountMapping>();
    public DbSet<FinancialDimensionType> FinancialDimensionTypes => Set<FinancialDimensionType>();
    public DbSet<FinancialDimensionValue> FinancialDimensionValues => Set<FinancialDimensionValue>();
    public DbSet<FinancialDimensionCombination> FinancialDimensionCombinations => Set<FinancialDimensionCombination>();
    public DbSet<DimensionRequirementRule> DimensionRequirementRules => Set<DimensionRequirementRule>();
    public DbSet<DimensionMappingRule> DimensionMappingRules => Set<DimensionMappingRule>();
    public DbSet<SourceDimensionMapping> SourceDimensionMappings => Set<SourceDimensionMapping>();
    public DbSet<PostingRule> PostingRules => Set<PostingRule>();
    public DbSet<PostingRuleLine> PostingRuleLines => Set<PostingRuleLine>();
    public DbSet<PostingPreview> PostingPreviews => Set<PostingPreview>();
    public DbSet<PostingPreviewLine> PostingPreviewLines => Set<PostingPreviewLine>();
    public DbSet<PostingBatch> PostingBatches => Set<PostingBatch>();
    public DbSet<PostingBatchLine> PostingBatchLines => Set<PostingBatchLine>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();
    public DbSet<JournalEntryReversal> JournalEntryReversals => Set<JournalEntryReversal>();
    public DbSet<JournalAttachmentRef> JournalAttachmentRefs => Set<JournalAttachmentRef>();
    public DbSet<JournalApproval> JournalApprovals => Set<JournalApproval>();
    public DbSet<JournalAuditTrail> JournalAuditTrails => Set<JournalAuditTrail>();
    public DbSet<FinancialPacket> FinancialPackets => Set<FinancialPacket>();
    public DbSet<FinancialPacketLine> FinancialPacketLines => Set<FinancialPacketLine>();
    public DbSet<FinancialPacketSourceRef> FinancialPacketSourceRefs => Set<FinancialPacketSourceRef>();
    public DbSet<FinancialPacketStatusHistory> FinancialPacketStatusHistory => Set<FinancialPacketStatusHistory>();
    public DbSet<FinancialPacketValidationIssue> FinancialPacketValidationIssues => Set<FinancialPacketValidationIssue>();
    public DbSet<FinancialPacketMappingResult> FinancialPacketMappingResults => Set<FinancialPacketMappingResult>();
    public DbSet<FinancialPacketPostingResult> FinancialPacketPostingResults => Set<FinancialPacketPostingResult>();
    public DbSet<FinancialPacketIdempotencyKey> FinancialPacketIdempotencyKeys => Set<FinancialPacketIdempotencyKey>();
    public DbSet<SubledgerDocument> SubledgerDocuments => Set<SubledgerDocument>();
    public DbSet<SubledgerDocumentLine> SubledgerDocumentLines => Set<SubledgerDocumentLine>();
    public DbSet<SubledgerApplication> SubledgerApplications => Set<SubledgerApplication>();
    public DbSet<SubledgerReconciliationRun> SubledgerReconciliationRuns => Set<SubledgerReconciliationRun>();
    public DbSet<SubledgerReconciliationIssue> SubledgerReconciliationIssues => Set<SubledgerReconciliationIssue>();
    public DbSet<VendorFinancialProfile> VendorFinancialProfiles => Set<VendorFinancialProfile>();
    public DbSet<VendorBill> VendorBills => Set<VendorBill>();
    public DbSet<VendorBillLine> VendorBillLines => Set<VendorBillLine>();
    public DbSet<VendorBillApproval> VendorBillApprovals => Set<VendorBillApproval>();
    public DbSet<VendorBillMatch> VendorBillMatches => Set<VendorBillMatch>();
    public DbSet<VendorBillVariance> VendorBillVariances => Set<VendorBillVariance>();
    public DbSet<VendorCredit> VendorCredits => Set<VendorCredit>();
    public DbSet<APPayment> APPayments => Set<APPayment>();
    public DbSet<APPaymentLine> APPaymentLines => Set<APPaymentLine>();
    public DbSet<PaymentRun> PaymentRuns => Set<PaymentRun>();
    public DbSet<PaymentExportBatch> PaymentExportBatches => Set<PaymentExportBatch>();
    public DbSet<APDispute> APDisputes => Set<APDispute>();
    public DbSet<APAgingSnapshot> APAgingSnapshots => Set<APAgingSnapshot>();
    public DbSet<CustomerFinancialProfile> CustomerFinancialProfiles => Set<CustomerFinancialProfile>();
    public DbSet<CustomerInvoice> CustomerInvoices => Set<CustomerInvoice>();
    public DbSet<CustomerInvoiceLine> CustomerInvoiceLines => Set<CustomerInvoiceLine>();
    public DbSet<CustomerInvoiceApproval> CustomerInvoiceApprovals => Set<CustomerInvoiceApproval>();
    public DbSet<CustomerCreditMemo> CustomerCreditMemos => Set<CustomerCreditMemo>();
    public DbSet<CustomerPayment> CustomerPayments => Set<CustomerPayment>();
    public DbSet<CustomerPaymentApplication> CustomerPaymentApplications => Set<CustomerPaymentApplication>();
    public DbSet<CustomerStatement> CustomerStatements => Set<CustomerStatement>();
    public DbSet<CollectionStatus> CollectionStatuses => Set<CollectionStatus>();
    public DbSet<ARAgingSnapshot> ARAgingSnapshots => Set<ARAgingSnapshot>();
    public DbSet<InventoryValuationProfile> InventoryValuationProfiles => Set<InventoryValuationProfile>();
    public DbSet<ItemCostProfile> ItemCostProfiles => Set<ItemCostProfile>();
    public DbSet<InventoryCostLayer> InventoryCostLayers => Set<InventoryCostLayer>();
    public DbSet<InventoryValuationMovement> InventoryValuationMovements => Set<InventoryValuationMovement>();
    public DbSet<InventoryValuationAdjustment> InventoryValuationAdjustments => Set<InventoryValuationAdjustment>();
    public DbSet<LandedCostAllocation> LandedCostAllocations => Set<LandedCostAllocation>();
    public DbSet<LandedCostAllocationLine> LandedCostAllocationLines => Set<LandedCostAllocationLine>();
    public DbSet<InventorySubledgerBalance> InventorySubledgerBalances => Set<InventorySubledgerBalance>();
    public DbSet<COGSPostingRun> COGSPostingRuns => Set<COGSPostingRun>();
    public DbSet<InventoryReconciliationRun> InventoryReconciliationRuns => Set<InventoryReconciliationRun>();
    public DbSet<FixedAssetFinancialRecord> FixedAssetFinancialRecords => Set<FixedAssetFinancialRecord>();
    public DbSet<AssetCapitalizationEvent> AssetCapitalizationEvents => Set<AssetCapitalizationEvent>();
    public DbSet<AssetDepreciationBook> AssetDepreciationBooks => Set<AssetDepreciationBook>();
    public DbSet<AssetDepreciationSchedule> AssetDepreciationSchedules => Set<AssetDepreciationSchedule>();
    public DbSet<AssetDepreciationRun> AssetDepreciationRuns => Set<AssetDepreciationRun>();
    public DbSet<AssetDisposal> AssetDisposals => Set<AssetDisposal>();
    public DbSet<AssetImpairment> AssetImpairments => Set<AssetImpairment>();
    public DbSet<AssetRevaluation> AssetRevaluations => Set<AssetRevaluation>();
    public DbSet<FinancialProject> FinancialProjects => Set<FinancialProject>();
    public DbSet<FinancialProjectTask> FinancialProjectTasks => Set<FinancialProjectTask>();
    public DbSet<JobCostCode> JobCostCodes => Set<JobCostCode>();
    public DbSet<ProjectBudget> ProjectBudgets => Set<ProjectBudget>();
    public DbSet<ProjectBudgetLine> ProjectBudgetLines => Set<ProjectBudgetLine>();
    public DbSet<ProjectActualCost> ProjectActualCosts => Set<ProjectActualCost>();
    public DbSet<ProjectCommittedCost> ProjectCommittedCosts => Set<ProjectCommittedCost>();
    public DbSet<ProjectCostAllocation> ProjectCostAllocations => Set<ProjectCostAllocation>();
    public DbSet<ProjectBillingStatus> ProjectBillingStatuses => Set<ProjectBillingStatus>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetVersion> BudgetVersions => Set<BudgetVersion>();
    public DbSet<BudgetLine> BudgetLines => Set<BudgetLine>();
    public DbSet<BudgetApproval> BudgetApprovals => Set<BudgetApproval>();
    public DbSet<BudgetActualSnapshot> BudgetActualSnapshots => Set<BudgetActualSnapshot>();
    public DbSet<BudgetVarianceSnapshot> BudgetVarianceSnapshots => Set<BudgetVarianceSnapshot>();
    public DbSet<TaxCode> TaxCodes => Set<TaxCode>();
    public DbSet<TaxJurisdiction> TaxJurisdictions => Set<TaxJurisdiction>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<TaxRule> TaxRules => Set<TaxRule>();
    public DbSet<TaxPosting> TaxPostings => Set<TaxPosting>();
    public DbSet<TaxExemptionCertificateRef> TaxExemptionCertificateRefs => Set<TaxExemptionCertificateRef>();
    public DbSet<TaxReportingRun> TaxReportingRuns => Set<TaxReportingRun>();
    public DbSet<ExternalFinanceSystem> ExternalFinanceSystems => Set<ExternalFinanceSystem>();
    public DbSet<ExternalFinanceConnection> ExternalFinanceConnections => Set<ExternalFinanceConnection>();
    public DbSet<ExternalAccountMapping> ExternalAccountMappings => Set<ExternalAccountMapping>();
    public DbSet<ExternalDimensionMapping> ExternalDimensionMappings => Set<ExternalDimensionMapping>();
    public DbSet<ExternalCustomerMapping> ExternalCustomerMappings => Set<ExternalCustomerMapping>();
    public DbSet<ExternalVendorMapping> ExternalVendorMappings => Set<ExternalVendorMapping>();
    public DbSet<ExternalItemMapping> ExternalItemMappings => Set<ExternalItemMapping>();
    public DbSet<ExternalPostingBatch> ExternalPostingBatches => Set<ExternalPostingBatch>();
    public DbSet<ExternalPostingResult> ExternalPostingResults => Set<ExternalPostingResult>();
    public DbSet<ExternalSyncRun> ExternalSyncRuns => Set<ExternalSyncRun>();
    public DbSet<ExternalSyncIssue> ExternalSyncIssues => Set<ExternalSyncIssue>();
    public DbSet<FinancialAuditEvent> FinancialAuditEvents => Set<FinancialAuditEvent>();
    public DbSet<ApprovalPolicy> ApprovalPolicies => Set<ApprovalPolicy>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();
    public DbSet<ApprovalDecision> ApprovalDecisions => Set<ApprovalDecision>();
    public DbSet<SegregationOfDutiesRule> SegregationOfDutiesRules => Set<SegregationOfDutiesRule>();
    public DbSet<FinancialControlException> FinancialControlExceptions => Set<FinancialControlException>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties().Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }

        modelBuilder.Entity<TenantFinancialProfile>()
            .HasIndex(e => e.TenantId)
            .IsUnique();

        modelBuilder.Entity<FinancialLegalEntity>()
            .HasIndex(e => new { e.TenantId, e.EntityCode })
            .IsUnique();

        modelBuilder.Entity<FiscalPeriod>()
            .HasIndex(e => new { e.TenantId, e.FiscalCalendarId, e.PeriodKey })
            .IsUnique();

        modelBuilder.Entity<GLAccount>()
            .HasIndex(e => new { e.TenantId, e.AccountCode })
            .IsUnique();

        modelBuilder.Entity<FinancialPacket>()
            .HasIndex(e => new { e.TenantId, e.SourceProductKey, e.SourceEventId, e.SourceEventVersion })
            .IsUnique();

        modelBuilder.Entity<FinancialPacketIdempotencyKey>()
            .HasIndex(e => new { e.TenantId, e.SourceProductKey, e.SourceEventId, e.SourceEventVersion })
            .IsUnique();

        modelBuilder.Entity<JournalEntry>()
            .HasIndex(e => new { e.TenantId, e.JournalNumber })
            .IsUnique();

        modelBuilder.Entity<VendorBill>()
            .HasIndex(e => new { e.TenantId, e.VendorRefProductKey, e.VendorRefId, e.VendorInvoiceNumber });

        modelBuilder.Entity<CustomerInvoice>()
            .HasIndex(e => new { e.TenantId, e.InvoiceNumber })
            .IsUnique();
    }
}

public sealed class TenantFinancialProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public bool RequireFinancialLegalEntity { get; set; } = true;
    public string LedgerMode { get; set; } = "stl_ledger_master";
    public string BaseCurrencyCode { get; set; } = "USD";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class FinancialLegalEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string EntityCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string EntityType { get; set; } = "company";
    public string BaseCurrencyCode { get; set; } = "USD";
    public Guid? FiscalCalendarId { get; set; }
    public string Status { get; set; } = "active";
    public string? StaffArrLocationRefId { get; set; }
    public string? SnapshotLabel { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeactivatedAt { get; set; }
}

public sealed class FinancialLegalEntityRelationship
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ParentFinancialLegalEntityId { get; set; }
    public Guid ChildFinancialLegalEntityId { get; set; }
    public string RelationshipType { get; set; } = "consolidation";
    public decimal OwnershipPercentage { get; set; }
    public string Status { get; set; } = "active";
}

public sealed class FinancialLegalEntityRegistration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialLegalEntityId { get; set; }
    public string RegistrationType { get; set; } = "tax";
    public string RegistrationNumber { get; set; } = string.Empty;
    public string JurisdictionLabel { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
}

public sealed class FinancialLegalEntityAddressSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialLegalEntityId { get; set; }
    public string SnapshotLabel { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string CountryCode { get; set; } = "US";
}

public sealed class FiscalCalendar
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int FiscalYearStartMonth { get; set; } = 1;
    public string BaseCurrencyCode { get; set; } = "USD";
    public string Status { get; set; } = "active";
}

public sealed class FiscalYear
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FiscalCalendarId { get; set; }
    public int YearNumber { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = "open";
}

public sealed class FiscalPeriod
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FiscalCalendarId { get; set; }
    public Guid? FinancialLegalEntityId { get; set; }
    public string PeriodKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = "open";
    public DateTimeOffset? ClosedAt { get; set; }
    public DateTimeOffset? LockedAt { get; set; }
}

public sealed class Currency
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CurrencyCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int MinorUnits { get; set; } = 2;
}

public sealed class ExchangeRate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string FromCurrencyCode { get; set; } = string.Empty;
    public string ToCurrencyCode { get; set; } = string.Empty;
    public DateOnly EffectiveDate { get; set; }
    public decimal Rate { get; set; }
}

public sealed class NumberingSequence
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string SequenceKey { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public long NextNumber { get; set; } = 1;
}

public sealed class FinancialCloseRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FiscalPeriodId { get; set; }
    public string Status { get; set; } = "pending";
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
}

public sealed class PeriodLockAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FiscalPeriodId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class ChartOfAccounts
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid? FinancialLegalEntityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
}

public sealed class GLAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ChartOfAccountsId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = "asset";
    public string Category { get; set; } = "current";
    public string NormalBalance { get; set; } = "debit";
    public string Status { get; set; } = "active";
}

public sealed class AccountAlias
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid GLAccountId { get; set; }
    public string Alias { get; set; } = string.Empty;
}

public sealed class AccountMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string SourceProductKey { get; set; } = string.Empty;
    public string SourceRecordType { get; set; } = string.Empty;
    public string PacketType { get; set; } = string.Empty;
    public Guid GLAccountId { get; set; }
    public string MappingRole { get; set; } = "primary";
}

public sealed class FinancialDimensionType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string DimensionKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
}

public sealed class FinancialDimensionValue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid DimensionTypeId { get; set; }
    public string ValueKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? SourceProductKey { get; set; }
    public string? SourceRecordType { get; set; }
    public string? SourceRecordId { get; set; }
    public string Status { get; set; } = "active";
}

public sealed class FinancialDimensionCombination
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string CombinationKey { get; set; } = string.Empty;
    public string DisplayLabel { get; set; } = string.Empty;
}

public sealed class DimensionRequirementRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string AppliesTo { get; set; } = string.Empty;
    public string DimensionKey { get; set; } = string.Empty;
    public bool Required { get; set; } = true;
}

public sealed class DimensionMappingRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string SourceProductKey { get; set; } = string.Empty;
    public string SourceField { get; set; } = string.Empty;
    public string DimensionKey { get; set; } = string.Empty;
}

public sealed class SourceDimensionMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string SourceProductKey { get; set; } = string.Empty;
    public string SourceRecordType { get; set; } = string.Empty;
    public string SourceRecordId { get; set; } = string.Empty;
    public Guid DimensionValueId { get; set; }
}

public sealed class PostingRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string PacketType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public int Version { get; set; } = 1;
}

public sealed class PostingRuleLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PostingRuleId { get; set; }
    public string LineRole { get; set; } = string.Empty;
    public string AccountCode { get; set; } = string.Empty;
    public string Direction { get; set; } = "debit";
}

public sealed class PostingPreview
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid? FinancialPacketId { get; set; }
    public Guid? SourceDocumentId { get; set; }
    public string SourceDocumentType { get; set; } = string.Empty;
    public string Status { get; set; } = "preview_ready";
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
}

public sealed class PostingPreviewLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PostingPreviewId { get; set; }
    public int LineNumber { get; set; }
    public Guid GLAccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Memo { get; set; } = string.Empty;
    public string? DimensionSummary { get; set; }
}

public sealed class PostingBatch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
}

public sealed class PostingBatchLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PostingBatchId { get; set; }
    public Guid JournalEntryId { get; set; }
}

public sealed class JournalEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialLegalEntityId { get; set; }
    public Guid FiscalPeriodId { get; set; }
    public string JournalNumber { get; set; } = string.Empty;
    public Guid? FinancialPacketId { get; set; }
    public string SourceDocumentType { get; set; } = string.Empty;
    public Guid? SourceDocumentId { get; set; }
    public string Status { get; set; } = "draft";
    public DateOnly AccountingDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PostedAt { get; set; }
    public Guid? ReversalOfJournalEntryId { get; set; }
}

public sealed class JournalLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid JournalEntryId { get; set; }
    public int LineNumber { get; set; }
    public Guid GLAccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Memo { get; set; } = string.Empty;
    public string? DimensionSummary { get; set; }
}

public sealed class JournalEntryReversal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid OriginalJournalEntryId { get; set; }
    public Guid ReversalJournalEntryId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset ReversedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class JournalAttachmentRef
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid JournalEntryId { get; set; }
    public string RecordArrDocumentId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class JournalApproval
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid JournalEntryId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string ActorPersonId { get; set; } = string.Empty;
    public DateTimeOffset DecidedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class JournalAuditTrail
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid JournalEntryId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class FinancialPacket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid? FinancialLegalEntityId { get; set; }
    public string SourceProductKey { get; set; } = string.Empty;
    public string SourceEventId { get; set; } = string.Empty;
    public int SourceEventVersion { get; set; } = 1;
    public string SourceRecordType { get; set; } = string.Empty;
    public string SourceRecordId { get; set; } = string.Empty;
    public string SourceRecordDisplayName { get; set; } = string.Empty;
    public DateTimeOffset SourceOccurredAt { get; set; } = DateTimeOffset.UtcNow;
    public string PacketType { get; set; } = string.Empty;
    public string PacketSubType { get; set; } = string.Empty;
    public DateOnly AccountingDate { get; set; }
    public string TransactionCurrency { get; set; } = "USD";
    public decimal SourceAmount { get; set; }
    public decimal SourceTaxAmount { get; set; }
    public decimal SourceTotalAmount { get; set; }
    public string Status { get; set; } = "received";
    public string IdempotencyKey { get; set; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid? PostedJournalEntryId { get; set; }
}

public sealed class FinancialPacketLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialPacketId { get; set; }
    public int LineNumber { get; set; }
    public string SourceLineId { get; set; } = string.Empty;
    public string LineType { get; set; } = string.Empty;
    public string? ItemRefId { get; set; }
    public string? VendorRefId { get; set; }
    public string? CustomerRefId { get; set; }
    public string? AssetRefId { get; set; }
    public string? WorkOrderRefId { get; set; }
    public string? OrderRefId { get; set; }
    public string? ShipmentRefId { get; set; }
    public string? TripRefId { get; set; }
    public string? LocationRefId { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public decimal ExtendedAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? DimensionSummary { get; set; }
    public string? CostBehavior { get; set; }
    public string? CapitalizationHint { get; set; }
    public string? BillableHint { get; set; }
}

public sealed class FinancialPacketSourceRef
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialPacketId { get; set; }
    public string ProductKey { get; set; } = string.Empty;
    public string SourceRecordType { get; set; } = string.Empty;
    public string SourceRecordId { get; set; } = string.Empty;
    public string SourceRecordDisplayName { get; set; } = string.Empty;
    public string? SourceEventId { get; set; }
    public int? SourceVersion { get; set; }
    public string SnapshotLabel { get; set; } = string.Empty;
}

public sealed class FinancialPacketStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialPacketId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class FinancialPacketValidationIssue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialPacketId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "blocked";
}

public sealed class FinancialPacketMappingResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialPacketId { get; set; }
    public string Status { get; set; } = "mapped";
    public string Summary { get; set; } = string.Empty;
}

public sealed class FinancialPacketPostingResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialPacketId { get; set; }
    public Guid? JournalEntryId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

public sealed class FinancialPacketIdempotencyKey
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string SourceProductKey { get; set; } = string.Empty;
    public string SourceEventId { get; set; } = string.Empty;
    public int SourceEventVersion { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public Guid FinancialPacketId { get; set; }
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class SubledgerDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public Guid? JournalEntryId { get; set; }
}

public sealed class SubledgerDocumentLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid SubledgerDocumentId { get; set; }
    public int LineNumber { get; set; }
    public decimal Amount { get; set; }
}

public sealed class SubledgerApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid SourceSubledgerDocumentId { get; set; }
    public Guid TargetSubledgerDocumentId { get; set; }
    public decimal AppliedAmount { get; set; }
}

public sealed class SubledgerReconciliationRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string SubledgerType { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
}

public sealed class SubledgerReconciliationIssue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ReconciliationRunId { get; set; }
    public string IssueCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class VendorFinancialProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string VendorRefProductKey { get; set; } = "supplyarr";
    public string VendorRefId { get; set; } = string.Empty;
    public string VendorDisplayName { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = "net_30";
}

public sealed class VendorBill
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialLegalEntityId { get; set; }
    public string VendorRefProductKey { get; set; } = "supplyarr";
    public string VendorRefId { get; set; } = string.Empty;
    public string VendorDisplayName { get; set; } = string.Empty;
    public string VendorInvoiceNumber { get; set; } = string.Empty;
    public DateOnly BillDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "draft";
    public string MatchStatus { get; set; } = "not_matched";
    public Guid? PostedJournalEntryId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class VendorBillLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid VendorBillId { get; set; }
    public int LineNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Amount { get; set; }
}

public sealed class VendorBillApproval
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid VendorBillId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string ActorPersonId { get; set; } = string.Empty;
    public DateTimeOffset DecidedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class VendorBillMatch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid VendorBillId { get; set; }
    public string SourceProductKey { get; set; } = string.Empty;
    public string SourceRecordType { get; set; } = string.Empty;
    public string SourceRecordId { get; set; } = string.Empty;
    public string Status { get; set; } = "matched";
    public decimal ExpectedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal VarianceAmount { get; set; }
}

public sealed class VendorBillVariance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid VendorBillId { get; set; }
    public string VarianceCode { get; set; } = string.Empty;
    public decimal VarianceAmount { get; set; }
    public string Status { get; set; } = "open";
}

public sealed class VendorCredit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialLegalEntityId { get; set; }
    public string VendorRefId { get; set; } = string.Empty;
    public decimal CreditAmount { get; set; }
    public string Status { get; set; } = "draft";
}

public sealed class APPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialLegalEntityId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "draft";
}

public sealed class APPaymentLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid APPaymentId { get; set; }
    public Guid VendorBillId { get; set; }
    public decimal AppliedAmount { get; set; }
}

public sealed class PaymentRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string PaymentRunNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public decimal TotalAmount { get; set; }
}

public sealed class PaymentExportBatch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PaymentRunId { get; set; }
    public string Status { get; set; } = "created";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class APDispute
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid VendorBillId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "open";
}

public sealed class APAgingSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public DateOnly SnapshotDate { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal PastDueAmount { get; set; }
}

public sealed class CustomerFinancialProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string CustomerRefProductKey { get; set; } = "customarr";
    public string CustomerRefId { get; set; } = string.Empty;
    public string CustomerDisplayName { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = "net_30";
}

public sealed class CustomerInvoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialLegalEntityId { get; set; }
    public string CustomerRefProductKey { get; set; } = "customarr";
    public string CustomerRefId { get; set; } = string.Empty;
    public string CustomerDisplayName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "draft";
    public Guid? PostedJournalEntryId { get; set; }
}

public sealed class CustomerInvoiceLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid CustomerInvoiceId { get; set; }
    public int LineNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}

public sealed class CustomerInvoiceApproval
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid CustomerInvoiceId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string ActorPersonId { get; set; } = string.Empty;
}

public sealed class CustomerCreditMemo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid CustomerInvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "draft";
}

public sealed class CustomerPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialLegalEntityId { get; set; }
    public string CustomerRefId { get; set; } = string.Empty;
    public string PaymentNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "recorded";
}

public sealed class CustomerPaymentApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid CustomerPaymentId { get; set; }
    public Guid CustomerInvoiceId { get; set; }
    public decimal AppliedAmount { get; set; }
}

public sealed class CustomerStatement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string CustomerRefId { get; set; } = string.Empty;
    public DateOnly StatementDate { get; set; }
    public decimal Balance { get; set; }
}

public sealed class CollectionStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid CustomerInvoiceId { get; set; }
    public string Status { get; set; } = "current";
}

public sealed class ARAgingSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public DateOnly SnapshotDate { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal PastDueAmount { get; set; }
}

public sealed class InventoryValuationProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string CostMethod { get; set; } = "fifo";
}

public sealed class ItemCostProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string ItemRefProductKey { get; set; } = "supplyarr";
    public string ItemRefId { get; set; } = string.Empty;
    public string CostMethod { get; set; } = "fifo";
    public decimal StandardCost { get; set; }
}

public sealed class InventoryCostLayer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialLegalEntityId { get; set; }
    public string ItemRefProductKey { get; set; } = "supplyarr";
    public string ItemRefId { get; set; } = string.Empty;
    public string SourceProductKey { get; set; } = string.Empty;
    public string SourceRecordType { get; set; } = string.Empty;
    public string SourceRecordId { get; set; } = string.Empty;
    public DateOnly LayerDate { get; set; }
    public decimal QuantityOriginal { get; set; }
    public decimal QuantityRemaining { get; set; }
    public decimal UnitCost { get; set; }
    public string Status { get; set; } = "open";
}

public sealed class InventoryValuationMovement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialLegalEntityId { get; set; }
    public string ItemRefId { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public string CostMethod { get; set; } = "fifo";
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal ExtendedAmount { get; set; }
    public Guid? JournalEntryId { get; set; }
}

public sealed class InventoryValuationAdjustment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string ItemRefId { get; set; } = string.Empty;
    public decimal AdjustmentAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class LandedCostAllocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string AllocationNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "draft";
}

public sealed class LandedCostAllocationLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid LandedCostAllocationId { get; set; }
    public string ItemRefId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public sealed class InventorySubledgerBalance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string ItemRefId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Value { get; set; }
}

public sealed class COGSPostingRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Status { get; set; } = "pending";
}

public sealed class InventoryReconciliationRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Status { get; set; } = "pending";
    public int IssueCount { get; set; }
}

public sealed class FixedAssetFinancialRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialLegalEntityId { get; set; }
    public string MaintainArrAssetRefId { get; set; } = string.Empty;
    public string AssetNumber { get; set; } = string.Empty;
    public string AssetClass { get; set; } = string.Empty;
    public DateOnly InServiceDate { get; set; }
    public decimal CapitalizedCost { get; set; }
    public decimal BookValue { get; set; }
    public string DepreciationMethod { get; set; } = "straight_line";
    public int UsefulLifeMonths { get; set; }
    public decimal SalvageValue { get; set; }
    public string Status { get; set; } = "active";
}

public sealed class AssetCapitalizationEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FixedAssetFinancialRecordId { get; set; }
    public decimal Amount { get; set; }
}

public sealed class AssetDepreciationBook
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FixedAssetFinancialRecordId { get; set; }
    public string BookName { get; set; } = "primary";
}

public sealed class AssetDepreciationSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FixedAssetFinancialRecordId { get; set; }
    public int SequenceNumber { get; set; }
    public DateOnly DepreciationDate { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public string Status { get; set; } = "scheduled";
    public Guid? PostedJournalEntryId { get; set; }
}

public sealed class AssetDepreciationRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Status { get; set; } = "pending";
}

public sealed class AssetDisposal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FixedAssetFinancialRecordId { get; set; }
    public decimal Proceeds { get; set; }
    public string Status { get; set; } = "draft";
}

public sealed class AssetImpairment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FixedAssetFinancialRecordId { get; set; }
    public decimal ImpairmentAmount { get; set; }
}

public sealed class AssetRevaluation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FixedAssetFinancialRecordId { get; set; }
    public decimal RevaluedAmount { get; set; }
}

public sealed class FinancialProject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialLegalEntityId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
}

public sealed class FinancialProjectTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialProjectId { get; set; }
    public string TaskCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class JobCostCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string CostCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class ProjectBudget
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialProjectId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "draft";
}

public sealed class ProjectBudgetLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ProjectBudgetId { get; set; }
    public string CostCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public sealed class ProjectActualCost
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialProjectId { get; set; }
    public decimal Amount { get; set; }
}

public sealed class ProjectCommittedCost
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialProjectId { get; set; }
    public decimal Amount { get; set; }
}

public sealed class ProjectCostAllocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialProjectId { get; set; }
    public decimal Amount { get; set; }
}

public sealed class ProjectBillingStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialProjectId { get; set; }
    public string Status { get; set; } = "not_billable";
}

public sealed class Budget
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid FinancialLegalEntityId { get; set; }
    public string BudgetNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
}

public sealed class BudgetVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid BudgetId { get; set; }
    public int VersionNumber { get; set; }
    public string Status { get; set; } = "draft";
}

public sealed class BudgetLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid BudgetId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string? DimensionSummary { get; set; }
    public decimal Amount { get; set; }
    public decimal WarningThresholdPercent { get; set; } = 0.9m;
    public decimal BlockThresholdPercent { get; set; } = 1.0m;
}

public sealed class BudgetApproval
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid BudgetId { get; set; }
    public string Decision { get; set; } = string.Empty;
}

public sealed class BudgetActualSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid BudgetLineId { get; set; }
    public decimal ActualAmount { get; set; }
    public DateOnly SnapshotDate { get; set; }
}

public sealed class BudgetVarianceSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid BudgetLineId { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal VarianceAmount { get; set; }
}

public sealed class TaxCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string TaxCodeKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
}

public sealed class TaxJurisdiction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string JurisdictionKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class TaxRate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid TaxCodeId { get; set; }
    public decimal Rate { get; set; }
    public DateOnly EffectiveDate { get; set; }
}

public sealed class TaxRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid TaxCodeId { get; set; }
    public string RuleKey { get; set; } = string.Empty;
}

public sealed class TaxPosting
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid TaxCodeId { get; set; }
    public Guid JournalEntryId { get; set; }
    public decimal Amount { get; set; }
}

public sealed class TaxExemptionCertificateRef
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string RecordArrDocumentId { get; set; } = string.Empty;
    public string CustomerRefId { get; set; } = string.Empty;
}

public sealed class TaxReportingRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Status { get; set; } = "pending";
}

public sealed class ExternalFinanceSystem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string SystemKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Mode { get; set; } = "export_only";
    public string Status { get; set; } = "active";
}

public sealed class ExternalFinanceConnection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ExternalFinanceSystemId { get; set; }
    public string Status { get; set; } = "configured";
}

public sealed class ExternalAccountMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ExternalFinanceSystemId { get; set; }
    public Guid GLAccountId { get; set; }
    public string ExternalAccountId { get; set; } = string.Empty;
}

public sealed class ExternalDimensionMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ExternalFinanceSystemId { get; set; }
    public string DimensionKey { get; set; } = string.Empty;
    public string ExternalDimensionId { get; set; } = string.Empty;
}

public sealed class ExternalCustomerMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ExternalFinanceSystemId { get; set; }
    public string CustomerRefId { get; set; } = string.Empty;
    public string ExternalCustomerId { get; set; } = string.Empty;
}

public sealed class ExternalVendorMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ExternalFinanceSystemId { get; set; }
    public string VendorRefId { get; set; } = string.Empty;
    public string ExternalVendorId { get; set; } = string.Empty;
}

public sealed class ExternalItemMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ExternalFinanceSystemId { get; set; }
    public string ItemRefId { get; set; } = string.Empty;
    public string ExternalItemId { get; set; } = string.Empty;
}

public sealed class ExternalPostingBatch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ExternalFinanceSystemId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "created";
    public string JournalEntryIdsCsv { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExportedAt { get; set; }
}

public sealed class ExternalPostingResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ExternalPostingBatchId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class ExternalSyncRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ExternalFinanceSystemId { get; set; }
    public string Status { get; set; } = "pending";
}

public sealed class ExternalSyncIssue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ExternalSyncRunId { get; set; }
    public string IssueCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class FinancialAuditEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string ProductKey { get; set; } = "ledgarr";
    public string Action { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class ApprovalPolicy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string PolicyKey { get; set; } = string.Empty;
    public string AppliesTo { get; set; } = string.Empty;
    public bool RequiresApproval { get; set; } = true;
}

public sealed class ApprovalStep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ApprovalPolicyId { get; set; }
    public int StepNumber { get; set; }
    public string RequiredPermissionKey { get; set; } = string.Empty;
}

public sealed class ApprovalDecision
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
}

public sealed class SegregationOfDutiesRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string RuleKey { get; set; } = string.Empty;
    public string IncompatibleActions { get; set; } = string.Empty;
}

public sealed class FinancialControlException
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string ControlKey { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string Status { get; set; } = "open";
}
