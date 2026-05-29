using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Data;

public sealed class SupplyArrDbContext(DbContextOptions<SupplyArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<ExternalParty> ExternalParties => Set<ExternalParty>();

    public DbSet<PartyContact> PartyContacts => Set<PartyContact>();

    public DbSet<PartyComplianceDocument> PartyComplianceDocuments => Set<PartyComplianceDocument>();

    public DbSet<PartySupplierOnboarding> PartySupplierOnboardings => Set<PartySupplierOnboarding>();

    public DbSet<TenantSupplierOnboardingSettings> TenantSupplierOnboardingSettings =>
        Set<TenantSupplierOnboardingSettings>();

    public DbSet<VendorRestriction> VendorRestrictions => Set<VendorRestriction>();

    public DbSet<SupplierIncident> SupplierIncidents => Set<SupplierIncident>();

    public DbSet<ProcurementException> ProcurementExceptions => Set<ProcurementException>();

    public DbSet<StaffarrProcurementApprovalAuthorityMirror> StaffarrProcurementApprovalAuthorityMirrors =>
        Set<StaffarrProcurementApprovalAuthorityMirror>();

    public DbSet<SupplyArrAuditEvent> AuditEvents => Set<SupplyArrAuditEvent>();

    public DbSet<PartCatalog> PartCatalogs => Set<PartCatalog>();

    public DbSet<Part> Parts => Set<Part>();

    public DbSet<PartManufacturerAlias> PartManufacturerAliases => Set<PartManufacturerAlias>();

    public DbSet<PartVendorLink> PartVendorLinks => Set<PartVendorLink>();

    public DbSet<InventoryLocation> InventoryLocations => Set<InventoryLocation>();

    public DbSet<InventoryBin> InventoryBins => Set<InventoryBin>();

    public DbSet<PartStockLevel> PartStockLevels => Set<PartStockLevel>();

    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();

    public DbSet<PurchaseRequestLine> PurchaseRequestLines => Set<PurchaseRequestLine>();

    public DbSet<Rfq> Rfqs => Set<Rfq>();

    public DbSet<RfqLine> RfqLines => Set<RfqLine>();

    public DbSet<RfqVendorInvitation> RfqVendorInvitations => Set<RfqVendorInvitation>();

    public DbSet<VendorQuote> VendorQuotes => Set<VendorQuote>();

    public DbSet<VendorQuoteLine> VendorQuoteLines => Set<VendorQuoteLine>();

    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

    public DbSet<ReceivingReceipt> ReceivingReceipts => Set<ReceivingReceipt>();

    public DbSet<ReceivingReceiptLine> ReceivingReceiptLines => Set<ReceivingReceiptLine>();

    public DbSet<ReceivingException> ReceivingExceptions => Set<ReceivingException>();

    public DbSet<Backorder> Backorders => Set<Backorder>();

    public DbSet<VendorReturn> VendorReturns => Set<VendorReturn>();

    public DbSet<VendorReturnLine> VendorReturnLines => Set<VendorReturnLine>();

    public DbSet<WarrantyClaim> WarrantyClaims => Set<WarrantyClaim>();

    public DbSet<PartVendorPricingSnapshot> PartVendorPricingSnapshots => Set<PartVendorPricingSnapshot>();

    public DbSet<PartVendorLeadTimeSnapshot> PartVendorLeadTimeSnapshots => Set<PartVendorLeadTimeSnapshot>();

    public DbSet<PartVendorAvailabilitySnapshot> PartVendorAvailabilitySnapshots => Set<PartVendorAvailabilitySnapshot>();

    public DbSet<MaintainArrDemandRef> MaintainArrDemandRefs => Set<MaintainArrDemandRef>();

    public DbSet<MaintainArrDemandRefLine> MaintainArrDemandRefLines => Set<MaintainArrDemandRefLine>();

    public DbSet<RoutArrDemandRef> RoutArrDemandRefs => Set<RoutArrDemandRef>();

    public DbSet<RoutArrDemandRefLine> RoutArrDemandRefLines => Set<RoutArrDemandRefLine>();

    public DbSet<TrainArrDemandRef> TrainArrDemandRefs => Set<TrainArrDemandRef>();

    public DbSet<TrainArrDemandRefLine> TrainArrDemandRefLines => Set<TrainArrDemandRefLine>();

    public DbSet<StaffArrDemandRef> StaffArrDemandRefs => Set<StaffArrDemandRef>();

    public DbSet<StaffArrDemandRefLine> StaffArrDemandRefLines => Set<StaffArrDemandRefLine>();

    public DbSet<TenantProcurementNotificationSettings> TenantProcurementNotificationSettings =>
        Set<TenantProcurementNotificationSettings>();

    public DbSet<ProcurementNotificationDispatch> ProcurementNotificationDispatches =>
        Set<ProcurementNotificationDispatch>();

    public DbSet<TenantPriceSnapshotSettings> TenantPriceSnapshotSettings => Set<TenantPriceSnapshotSettings>();

    public DbSet<PartVendorPriceCaptureState> PartVendorPriceCaptureStates => Set<PartVendorPriceCaptureState>();

    public DbSet<PriceSnapshotRun> PriceSnapshotRuns => Set<PriceSnapshotRun>();

    public DbSet<TenantLeadTimeSnapshotSettings> TenantLeadTimeSnapshotSettings => Set<TenantLeadTimeSnapshotSettings>();

    public DbSet<PartVendorLeadTimeCaptureState> PartVendorLeadTimeCaptureStates => Set<PartVendorLeadTimeCaptureState>();

    public DbSet<LeadTimeSnapshotRun> LeadTimeSnapshotRuns => Set<LeadTimeSnapshotRun>();

    public DbSet<TenantAvailabilitySnapshotSettings> TenantAvailabilitySnapshotSettings =>
        Set<TenantAvailabilitySnapshotSettings>();

    public DbSet<PartVendorAvailabilityCaptureState> PartVendorAvailabilityCaptureStates =>
        Set<PartVendorAvailabilityCaptureState>();

    public DbSet<AvailabilitySnapshotRun> AvailabilitySnapshotRuns => Set<AvailabilitySnapshotRun>();

    public DbSet<TenantProcurementCoordinationSettings> TenantProcurementCoordinationSettings =>
        Set<TenantProcurementCoordinationSettings>();

    public DbSet<ProcurementCoordinationRecord> ProcurementCoordinationRecords =>
        Set<ProcurementCoordinationRecord>();

    public DbSet<ProcurementCoordinationEvent> ProcurementCoordinationEvents =>
        Set<ProcurementCoordinationEvent>();

    public DbSet<ProcurementCoordinationRun> ProcurementCoordinationRuns => Set<ProcurementCoordinationRun>();

    public DbSet<TenantApprovalReminderSettings> TenantApprovalReminderSettings =>
        Set<TenantApprovalReminderSettings>();

    public DbSet<ApprovalReminderState> ApprovalReminderStates => Set<ApprovalReminderState>();

    public DbSet<ApprovalReminderRun> ApprovalReminderRuns => Set<ApprovalReminderRun>();

    public DbSet<TenantProcurementExceptionEscalationSettings> TenantProcurementExceptionEscalationSettings =>
        Set<TenantProcurementExceptionEscalationSettings>();

    public DbSet<ProcurementExceptionEscalationEvent> ProcurementExceptionEscalationEvents =>
        Set<ProcurementExceptionEscalationEvent>();

    public DbSet<ProcurementExceptionEscalationRun> ProcurementExceptionEscalationRuns =>
        Set<ProcurementExceptionEscalationRun>();

    public DbSet<TenantDemandProcessingSettings> TenantDemandProcessingSettings =>
        Set<TenantDemandProcessingSettings>();

    public DbSet<DemandProcessingState> DemandProcessingStates => Set<DemandProcessingState>();

    public DbSet<DemandProcessingRun> DemandProcessingRuns => Set<DemandProcessingRun>();

    public DbSet<TenantIntegrationEventSettings> TenantIntegrationEventSettings =>
        Set<TenantIntegrationEventSettings>();

    public DbSet<IntegrationOutboxEvent> IntegrationOutboxEvents => Set<IntegrationOutboxEvent>();

    public DbSet<IntegrationInboxEvent> IntegrationInboxEvents => Set<IntegrationInboxEvent>();

    public DbSet<IntegrationEventProcessingRun> IntegrationEventProcessingRuns =>
        Set<IntegrationEventProcessingRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ExternalParty>(entity =>
        {
            entity.ToTable("supplyarr_external_parties");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PartyKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PartyType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.LegalName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.TaxIdentifier).HasMaxLength(64);
            entity.Property(x => x.ApprovalStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PartyKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PartyType, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.ApprovalStatus });
        });

        modelBuilder.Entity<PartyContact>(entity =>
        {
            entity.ToTable("supplyarr_party_contacts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ContactName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RoleLabel).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ExternalPartyId });
            entity.HasOne(x => x.ExternalParty)
                .WithMany(x => x.Contacts)
                .HasForeignKey(x => x.ExternalPartyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PartyComplianceDocument>(entity =>
        {
            entity.ToTable("supplyarr_party_compliance_documents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DocumentKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DocumentTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ReviewStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(512);
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ExternalPartyId });
            entity.HasIndex(x => new { x.TenantId, x.ExternalPartyId, x.DocumentKey, x.Version }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ExpiresAt });
            entity.HasIndex(x => new { x.TenantId, x.ReviewStatus, x.UpdatedAt });
            entity.HasOne(x => x.ExternalParty)
                .WithMany()
                .HasForeignKey(x => x.ExternalPartyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PartySupplierOnboarding>(entity =>
        {
            entity.ToTable("supplyarr_party_supplier_onboarding");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OnboardingStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.RejectionReason).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ExternalPartyId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.OnboardingStatus, x.SubmittedAt });
            entity.HasOne(x => x.ExternalParty)
                .WithMany()
                .HasForeignKey(x => x.ExternalPartyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenantSupplierOnboardingSettings>(entity =>
        {
            entity.ToTable("supplyarr_tenant_supplier_onboarding_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequiredDocumentTypeKeysJson).HasMaxLength(2048).IsRequired();
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<VendorRestriction>(entity =>
        {
            entity.ToTable("supplyarr_vendor_restrictions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RestrictionKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ScopesJson).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.LiftNotes).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ExternalPartyId });
            entity.HasIndex(x => new { x.TenantId, x.ExternalPartyId, x.RestrictionKey, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasOne(x => x.ExternalParty)
                .WithMany()
                .HasForeignKey(x => x.ExternalPartyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StaffarrProcurementApprovalAuthorityMirror>(entity =>
        {
            entity.ToTable("supplyarr_staffarr_procurement_approval_authority_mirrors");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OrgUnitScopeIdsJson).HasMaxLength(4096).IsRequired();
            entity.Property(x => x.GrantsJson).HasMaxLength(16384).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.StaffarrPersonId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ExternalUserId });
            entity.HasIndex(x => new { x.TenantId, x.RefreshedAt });
        });

        modelBuilder.Entity<ProcurementException>(entity =>
        {
            entity.ToTable("supplyarr_procurement_exceptions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExceptionKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SubjectType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SubjectKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ExceptionCategory).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ResolutionNotes).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.WaiveJustification).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.WaiveRejectionReason).HasMaxLength(512).IsRequired();
            entity.Property(x => x.CancellationReason).HasMaxLength(512).IsRequired();
            entity.Property(x => x.ResolutionTemplateKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ExceptionKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SubjectType, x.SubjectId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.SlaDueAt });
            entity.HasIndex(x => new { x.TenantId, x.LastEscalatedAt });
        });

        modelBuilder.Entity<SupplierIncident>(entity =>
        {
            entity.ToTable("supplyarr_supplier_incidents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IncidentKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.IncidentType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Severity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ResolutionNotes).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.CancellationReason).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IncidentKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ExternalPartyId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.Severity });
            entity.HasOne(x => x.ExternalParty)
                .WithMany()
                .HasForeignKey(x => x.ExternalPartyId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.VendorRestriction)
                .WithMany()
                .HasForeignKey(x => x.VendorRestrictionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SupplyArrAuditEvent>(entity =>
        {
            entity.ToTable("supplyarr_audit_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasMaxLength(128).IsRequired();
            entity.Property(x => x.TargetType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TargetId).HasMaxLength(128);
            entity.Property(x => x.Result).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(64);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.OccurredAt });
        });

        modelBuilder.Entity<PartCatalog>(entity =>
        {
            entity.ToTable("supplyarr_part_catalogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CatalogKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CatalogKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status });
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.ToTable("supplyarr_parts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PartKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.CategoryKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ManufacturerName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ManufacturerPartNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ReorderPoint).HasPrecision(18, 4);
            entity.Property(x => x.ReorderQuantity).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PartKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PartCatalogId });
            entity.HasIndex(x => new { x.TenantId, x.CategoryKey, x.Status });
            entity.HasOne(x => x.PartCatalog)
                .WithMany(x => x.Parts)
                .HasForeignKey(x => x.PartCatalogId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PartManufacturerAlias>(entity =>
        {
            entity.ToTable("supplyarr_part_manufacturer_aliases");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AliasKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ManufacturerName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ManufacturerPartNumber).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PartId });
            entity.HasIndex(x => new { x.TenantId, x.PartId, x.AliasKey }).IsUnique();
            entity.HasOne(x => x.Part)
                .WithMany(x => x.ManufacturerAliases)
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PartVendorLink>(entity =>
        {
            entity.ToTable("supplyarr_part_vendor_links");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.VendorPartNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CatalogUnitPrice).HasPrecision(18, 4);
            entity.Property(x => x.CatalogCurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.CatalogMinimumOrderQuantity).HasPrecision(18, 4);
            entity.Property(x => x.CatalogQuantityAvailable).HasPrecision(18, 4);
            entity.Property(x => x.CatalogAvailabilityStatus).HasMaxLength(32);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PartId });
            entity.HasIndex(x => new { x.TenantId, x.PartId, x.ExternalPartyId }).IsUnique();
            entity.HasOne(x => x.Part)
                .WithMany(x => x.VendorLinks)
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ExternalParty)
                .WithMany()
                .HasForeignKey(x => x.ExternalPartyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryLocation>(entity =>
        {
            entity.ToTable("supplyarr_inventory_locations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LocationKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
            entity.Property(x => x.LocationType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.AddressLine).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.LocationKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.LocationType, x.Status });
        });

        modelBuilder.Entity<InventoryBin>(entity =>
        {
            entity.ToTable("supplyarr_inventory_bins");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BinKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.InventoryLocationId });
            entity.HasIndex(x => new { x.TenantId, x.InventoryLocationId, x.BinKey }).IsUnique();
            entity.HasOne(x => x.InventoryLocation)
                .WithMany(x => x.Bins)
                .HasForeignKey(x => x.InventoryLocationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PartStockLevel>(entity =>
        {
            entity.ToTable("supplyarr_part_stock_levels");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.QuantityOnHand).HasPrecision(18, 4);
            entity.Property(x => x.QuantityReserved).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PartId });
            entity.HasIndex(x => new { x.TenantId, x.InventoryBinId });
            entity.HasIndex(x => new { x.TenantId, x.PartId, x.InventoryBinId }).IsUnique();
            entity.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.InventoryBin)
                .WithMany(x => x.StockLevels)
                .HasForeignKey(x => x.InventoryBinId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseRequest>(entity =>
        {
            entity.ToTable("supplyarr_purchase_requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequestKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RejectionReason).HasMaxLength(512).IsRequired();
            entity.Property(x => x.EmergencyReason).HasMaxLength(512).IsRequired();
            entity.Property(x => x.ManagerOverrideJustification).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RequestKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.IsEmergency, x.Status });
            entity.HasOne(x => x.VendorParty)
                .WithMany()
                .HasForeignKey(x => x.VendorPartyId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PurchaseRequestLine>(entity =>
        {
            entity.ToTable("supplyarr_purchase_request_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(512).IsRequired();
            entity.Property(x => x.QuantityRequested).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PurchaseRequestId });
            entity.HasIndex(x => new { x.TenantId, x.PurchaseRequestId, x.LineNumber }).IsUnique();
            entity.HasOne(x => x.PurchaseRequest)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.PurchaseRequestId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.ToTable("supplyarr_purchase_orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OrderKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CancellationReason).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.OrderKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PurchaseRequestId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasOne(x => x.PurchaseRequest)
                .WithMany()
                .HasForeignKey(x => x.PurchaseRequestId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.VendorParty)
                .WithMany()
                .HasForeignKey(x => x.VendorPartyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.ToTable("supplyarr_purchase_order_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(512).IsRequired();
            entity.Property(x => x.QuantityOrdered).HasPrecision(18, 4);
            entity.Property(x => x.QuantityReceived).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PurchaseOrderId });
            entity.HasIndex(x => new { x.TenantId, x.PurchaseOrderId, x.LineNumber }).IsUnique();
            entity.HasOne(x => x.PurchaseOrder)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PurchaseRequestLine)
                .WithMany()
                .HasForeignKey(x => x.PurchaseRequestLineId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReceivingReceipt>(entity =>
        {
            entity.ToTable("supplyarr_receiving_receipts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReceiptKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ReceiptKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PurchaseOrderId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasOne(x => x.PurchaseOrder)
                .WithMany()
                .HasForeignKey(x => x.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InventoryBin)
                .WithMany()
                .HasForeignKey(x => x.InventoryBinId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReceivingReceiptLine>(entity =>
        {
            entity.ToTable("supplyarr_receiving_receipt_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.QuantityExpected).HasPrecision(18, 4);
            entity.Property(x => x.QuantityReceived).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ReceivingReceiptId });
            entity.HasIndex(x => new { x.TenantId, x.ReceivingReceiptId, x.LineNumber }).IsUnique();
            entity.HasOne(x => x.ReceivingReceipt)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.ReceivingReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.PurchaseOrderLine)
                .WithMany()
                .HasForeignKey(x => x.PurchaseOrderLineId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReceivingException>(entity =>
        {
            entity.ToTable("supplyarr_receiving_exceptions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExceptionType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 4);
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ReceivingReceiptId });
            entity.HasIndex(x => new { x.TenantId, x.ReceivingReceiptLineId });
            entity.HasIndex(x => new { x.TenantId, x.ReceivingReceiptLineId, x.ExceptionType, x.Status });
            entity.HasOne(x => x.ReceivingReceipt)
                .WithMany()
                .HasForeignKey(x => x.ReceivingReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ReceivingReceiptLine)
                .WithMany(x => x.Exceptions)
                .HasForeignKey(x => x.ReceivingReceiptLineId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Backorder>(entity =>
        {
            entity.ToTable("supplyarr_backorders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BackorderKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.QuantityBackordered).HasPrecision(18, 4);
            entity.Property(x => x.QuantityFulfilled).HasPrecision(18, 4);
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.CancellationReason).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.BackorderKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PurchaseOrderId });
            entity.HasIndex(x => new { x.TenantId, x.PurchaseOrderLineId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.PartId, x.Status });
            entity.HasOne(x => x.PurchaseOrder)
                .WithMany()
                .HasForeignKey(x => x.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PurchaseOrderLine)
                .WithMany()
                .HasForeignKey(x => x.PurchaseOrderLineId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VendorReturn>(entity =>
        {
            entity.ToTable("supplyarr_vendor_returns");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReturnKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RmaNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.CancellationReason).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ReturnKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.VendorPartyId });
            entity.HasIndex(x => new { x.TenantId, x.PurchaseOrderId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasOne(x => x.VendorParty)
                .WithMany()
                .HasForeignKey(x => x.VendorPartyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PurchaseOrder)
                .WithMany()
                .HasForeignKey(x => x.PurchaseOrderId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.InventoryBin)
                .WithMany()
                .HasForeignKey(x => x.InventoryBinId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VendorReturnLine>(entity =>
        {
            entity.ToTable("supplyarr_vendor_return_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Quantity).HasPrecision(18, 4);
            entity.Property(x => x.Notes).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.VendorReturnId });
            entity.HasIndex(x => new { x.TenantId, x.VendorReturnId, x.LineNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PartId });
            entity.HasOne(x => x.VendorReturn)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.VendorReturnId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PurchaseOrderLine)
                .WithMany()
                .HasForeignKey(x => x.PurchaseOrderLineId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WarrantyClaim>(entity =>
        {
            entity.ToTable("supplyarr_warranty_claims");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ClaimKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ClaimType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.QuantityClaimed).HasPrecision(18, 4);
            entity.Property(x => x.ProblemDescription).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.VendorRmaNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.VendorDisposition).HasMaxLength(32).IsRequired();
            entity.Property(x => x.VendorResponseNotes).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.ClosureNotes).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.DenialReason).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.CancellationReason).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ClaimKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.VendorPartyId });
            entity.HasIndex(x => new { x.TenantId, x.PartId });
            entity.HasIndex(x => new { x.TenantId, x.PurchaseOrderId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasOne(x => x.VendorParty)
                .WithMany()
                .HasForeignKey(x => x.VendorPartyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PurchaseOrder)
                .WithMany()
                .HasForeignKey(x => x.PurchaseOrderId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.PurchaseOrderLine)
                .WithMany()
                .HasForeignKey(x => x.PurchaseOrderLineId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.ReceivingReceipt)
                .WithMany()
                .HasForeignKey(x => x.ReceivingReceiptId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.ReceivingReceiptLine)
                .WithMany()
                .HasForeignKey(x => x.ReceivingReceiptLineId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PartVendorPricingSnapshot>(entity =>
        {
            entity.ToTable("supplyarr_part_vendor_pricing_snapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SnapshotKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.UnitPrice).HasPrecision(18, 4);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.MinimumOrderQuantity).HasPrecision(18, 4);
            entity.Property(x => x.Source).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SnapshotKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PartVendorLinkId, x.EffectiveFrom });
            entity.HasIndex(x => new { x.TenantId, x.PartVendorLinkId, x.EffectiveTo });
            entity.HasOne(x => x.PartVendorLink)
                .WithMany()
                .HasForeignKey(x => x.PartVendorLinkId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PartVendorLeadTimeSnapshot>(entity =>
        {
            entity.ToTable("supplyarr_part_vendor_lead_time_snapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SnapshotKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SnapshotKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PartVendorLinkId, x.EffectiveFrom });
            entity.HasIndex(x => new { x.TenantId, x.PartVendorLinkId, x.EffectiveTo });
            entity.HasOne(x => x.PartVendorLink)
                .WithMany()
                .HasForeignKey(x => x.PartVendorLinkId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PartVendorAvailabilitySnapshot>(entity =>
        {
            entity.ToTable("supplyarr_part_vendor_availability_snapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SnapshotKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.QuantityAvailable).HasPrecision(18, 4);
            entity.Property(x => x.AvailabilityStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SnapshotKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PartVendorLinkId, x.EffectiveFrom });
            entity.HasIndex(x => new { x.TenantId, x.PartVendorLinkId, x.EffectiveTo });
            entity.HasOne(x => x.PartVendorLink)
                .WithMany()
                .HasForeignKey(x => x.PartVendorLinkId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MaintainArrDemandRef>(entity =>
        {
            entity.ToTable("supplyarr_maintainarr_demand_refs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MaintainarrWorkOrderNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProcurementStatus).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.MaintainarrPublicationId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.MaintainarrWorkOrderId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ReceivedAt });
            entity.HasIndex(x => new { x.TenantId, x.PurchaseRequestId });
            entity.HasIndex(x => new { x.TenantId, x.PurchaseOrderId });
            entity.HasOne(x => x.PurchaseRequest)
                .WithMany()
                .HasForeignKey(x => x.PurchaseRequestId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<MaintainArrDemandRefLine>(entity =>
        {
            entity.ToTable("supplyarr_maintainarr_demand_ref_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PartNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(256).IsRequired();
            entity.Property(x => x.QuantityRequested).HasPrecision(18, 4);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DemandRefId, x.LineNumber });
            entity.HasIndex(x => new { x.TenantId, x.MaintainarrDemandLineId });
            entity.HasOne(x => x.DemandRef)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.DemandRefId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RoutArrDemandRef>(entity =>
        {
            entity.ToTable("supplyarr_routarr_demand_refs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RoutarrTripNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RoutarrVehicleRefKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProcurementStatus).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RoutarrPublicationId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RoutarrTripId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ReceivedAt });
            entity.HasIndex(x => new { x.TenantId, x.PurchaseRequestId });
            entity.HasOne(x => x.PurchaseRequest)
                .WithMany()
                .HasForeignKey(x => x.PurchaseRequestId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RoutArrDemandRefLine>(entity =>
        {
            entity.ToTable("supplyarr_routarr_demand_ref_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PartNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(256).IsRequired();
            entity.Property(x => x.QuantityRequested).HasPrecision(18, 4);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DemandRefId, x.LineNumber });
            entity.HasIndex(x => new { x.TenantId, x.RoutarrDemandLineId });
            entity.HasOne(x => x.DemandRef)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.DemandRefId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TrainArrDemandRef>(entity =>
        {
            entity.ToTable("supplyarr_trainarr_demand_refs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TrainarrAssignmentRefKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProcurementStatus).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TrainarrPublicationId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.TrainarrAssignmentId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ReceivedAt });
            entity.HasIndex(x => new { x.TenantId, x.PurchaseRequestId });
            entity.HasOne(x => x.PurchaseRequest)
                .WithMany()
                .HasForeignKey(x => x.PurchaseRequestId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TrainArrDemandRefLine>(entity =>
        {
            entity.ToTable("supplyarr_trainarr_demand_ref_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PartNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(256).IsRequired();
            entity.Property(x => x.QuantityRequested).HasPrecision(18, 4);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DemandRefId, x.LineNumber });
            entity.HasIndex(x => new { x.TenantId, x.TrainarrDemandLineId });
            entity.HasOne(x => x.DemandRef)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.DemandRefId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StaffArrDemandRef>(entity =>
        {
            entity.ToTable("supplyarr_staffarr_demand_refs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StaffarrIncidentTitle).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProcurementStatus).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.StaffarrPublicationId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.StaffarrIncidentId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ReceivedAt });
            entity.HasIndex(x => new { x.TenantId, x.PurchaseRequestId });
            entity.HasOne(x => x.PurchaseRequest)
                .WithMany()
                .HasForeignKey(x => x.PurchaseRequestId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StaffArrDemandRefLine>(entity =>
        {
            entity.ToTable("supplyarr_staffarr_demand_ref_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PartNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(256).IsRequired();
            entity.Property(x => x.QuantityRequested).HasPrecision(18, 4);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DemandRefId, x.LineNumber });
            entity.HasIndex(x => new { x.TenantId, x.StaffarrDemandLineId });
            entity.HasOne(x => x.DemandRef)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.DemandRefId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TenantProcurementNotificationSettings>(entity =>
        {
            entity.ToTable("supplyarr_tenant_notification_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NotificationWebhookUrl).HasMaxLength(2048);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<ProcurementNotificationDispatch>(entity =>
        {
            entity.ToTable("supplyarr_notification_dispatches");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RelatedEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DispatchStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.WebhookHost).HasMaxLength(256);
            entity.Property(x => x.ErrorMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DispatchStatus, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.EventKind, x.RelatedEntityType, x.RelatedEntityId });
        });

        modelBuilder.Entity<TenantPriceSnapshotSettings>(entity =>
        {
            entity.ToTable("supplyarr_tenant_price_snapshot_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<PartVendorPriceCaptureState>(entity =>
        {
            entity.ToTable("supplyarr_part_vendor_price_capture_states");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LastCapturedUnitPrice).HasPrecision(18, 4);
            entity.Property(x => x.LastCapturedCurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.LastCapturedMinimumOrderQuantity).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PartVendorLinkId }).IsUnique();
            entity.HasOne(x => x.PartVendorLink)
                .WithMany()
                .HasForeignKey(x => x.PartVendorLinkId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PriceSnapshotRun>(entity =>
        {
            entity.ToTable("supplyarr_price_snapshot_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<TenantLeadTimeSnapshotSettings>(entity =>
        {
            entity.ToTable("supplyarr_tenant_lead_time_snapshot_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<PartVendorLeadTimeCaptureState>(entity =>
        {
            entity.ToTable("supplyarr_part_vendor_lead_time_capture_states");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PartVendorLinkId }).IsUnique();
            entity.HasOne(x => x.PartVendorLink)
                .WithMany()
                .HasForeignKey(x => x.PartVendorLinkId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LeadTimeSnapshotRun>(entity =>
        {
            entity.ToTable("supplyarr_lead_time_snapshot_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<TenantAvailabilitySnapshotSettings>(entity =>
        {
            entity.ToTable("supplyarr_tenant_availability_snapshot_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<PartVendorAvailabilityCaptureState>(entity =>
        {
            entity.ToTable("supplyarr_part_vendor_availability_capture_states");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LastCapturedAvailabilityStatus).HasMaxLength(32);
            entity.Property(x => x.LastCapturedQuantityAvailable).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PartVendorLinkId }).IsUnique();
            entity.HasOne(x => x.PartVendorLink)
                .WithMany()
                .HasForeignKey(x => x.PartVendorLinkId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AvailabilitySnapshotRun>(entity =>
        {
            entity.ToTable("supplyarr_availability_snapshot_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<TenantProcurementCoordinationSettings>(entity =>
        {
            entity.ToTable("supplyarr_tenant_procurement_coordination_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<ProcurementCoordinationRecord>(entity =>
        {
            entity.ToTable("supplyarr_procurement_coordination_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SubjectType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.DocumentKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CoordinationStage).HasMaxLength(64).IsRequired();
            entity.Property(x => x.NextActionRequired).HasMaxLength(256).IsRequired();
            entity.Property(x => x.VendorDisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DocumentStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.QuantityOrdered).HasPrecision(18, 4);
            entity.Property(x => x.QuantityReceived).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SubjectType, x.SubjectId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.CoordinationStage, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.IsTerminal, x.UpdatedAt });
        });

        modelBuilder.Entity<ProcurementCoordinationEvent>(entity =>
        {
            entity.ToTable("supplyarr_procurement_coordination_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SubjectType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.EventKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Detail).HasMaxLength(512);
            entity.Property(x => x.SourceEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceEntityId).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CoordinationRecordId, x.SequenceNumber });
            entity.HasOne(x => x.CoordinationRecord)
                .WithMany(x => x.Events)
                .HasForeignKey(x => x.CoordinationRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProcurementCoordinationRun>(entity =>
        {
            entity.ToTable("supplyarr_procurement_coordination_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<TenantApprovalReminderSettings>(entity =>
        {
            entity.ToTable("supplyarr_tenant_approval_reminder_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<ApprovalReminderState>(entity =>
        {
            entity.ToTable("supplyarr_approval_reminder_states");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SubjectType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.DocumentKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DocumentStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.LastReminderEventKind).HasMaxLength(64);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SubjectType, x.SubjectId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.LastReminderSentAt });
        });

        modelBuilder.Entity<ApprovalReminderRun>(entity =>
        {
            entity.ToTable("supplyarr_approval_reminder_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<TenantProcurementExceptionEscalationSettings>(entity =>
        {
            entity.ToTable("supplyarr_tenant_procurement_exception_escalation_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<ProcurementExceptionEscalationEvent>(entity =>
        {
            entity.ToTable("supplyarr_procurement_exception_escalation_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ActionKind).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ProcurementExceptionId });
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<ProcurementExceptionEscalationRun>(entity =>
        {
            entity.ToTable("supplyarr_procurement_exception_escalation_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<TenantDemandProcessingSettings>(entity =>
        {
            entity.ToTable("supplyarr_tenant_demand_processing_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<DemandProcessingState>(entity =>
        {
            entity.ToTable("supplyarr_demand_processing_states");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DemandRefSource).HasMaxLength(32).IsRequired();
            entity.Property(x => x.MaintainarrWorkOrderNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ProcessingOutcome).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RecommendedAction).HasMaxLength(32).IsRequired();
            entity.Property(x => x.LastProcessingMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DemandRefId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.LastProcessedAt });
            entity.HasIndex(x => new { x.TenantId, x.DemandRefSource });
        });

        modelBuilder.Entity<DemandProcessingRun>(entity =>
        {
            entity.ToTable("supplyarr_demand_processing_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<TenantIntegrationEventSettings>(entity =>
        {
            entity.ToTable("supplyarr_tenant_integration_event_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<IntegrationOutboxEvent>(entity =>
        {
            entity.ToTable("supplyarr_integration_outbox_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.RelatedEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ProcessingStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ProcessingStatus, x.NextRetryAt });
        });

        modelBuilder.Entity<IntegrationInboxEvent>(entity =>
        {
            entity.ToTable("supplyarr_integration_inbox_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceProduct).HasMaxLength(32).IsRequired();
            entity.Property(x => x.EventKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.RelatedEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RelatedEntityId).HasMaxLength(64);
            entity.Property(x => x.ProcessingStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ProcessingStatus, x.NextRetryAt });
        });

        modelBuilder.Entity<IntegrationEventProcessingRun>(entity =>
        {
            entity.ToTable("supplyarr_integration_event_processing_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<Rfq>(entity =>
        {
            entity.ToTable("supplyarr_rfqs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RfqKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RfqKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
        });

        modelBuilder.Entity<RfqLine>(entity =>
        {
            entity.ToTable("supplyarr_rfq_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(16).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RfqId, x.LineNumber }).IsUnique();
            entity.HasOne(x => x.Rfq).WithMany(x => x.Lines).HasForeignKey(x => x.RfqId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RfqVendorInvitation>(entity =>
        {
            entity.ToTable("supplyarr_rfq_vendor_invitations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RfqId, x.VendorPartyId }).IsUnique();
            entity.HasOne(x => x.Rfq).WithMany(x => x.VendorInvitations).HasForeignKey(x => x.RfqId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.VendorParty).WithMany().HasForeignKey(x => x.VendorPartyId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VendorQuote>(entity =>
        {
            entity.ToTable("supplyarr_vendor_quotes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.QuoteKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(8).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RfqId, x.QuoteKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RfqId, x.VendorPartyId });
            entity.HasOne(x => x.Rfq).WithMany(x => x.VendorQuotes).HasForeignKey(x => x.RfqId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.VendorParty).WithMany().HasForeignKey(x => x.VendorPartyId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VendorQuoteLine>(entity =>
        {
            entity.ToTable("supplyarr_vendor_quote_lines");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.VendorQuoteId, x.RfqLineId }).IsUnique();
            entity.HasOne(x => x.VendorQuote).WithMany(x => x.Lines).HasForeignKey(x => x.VendorQuoteId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.RfqLine).WithMany().HasForeignKey(x => x.RfqLineId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
