using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Data;

public sealed class SupplyArrDbContext(DbContextOptions<SupplyArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<SupplierContact> SupplierContacts => Set<SupplierContact>();

    public DbSet<SupplierComplianceDocument> SupplierComplianceDocuments => Set<SupplierComplianceDocument>();

    public DbSet<SupplierOnboarding> SupplierOnboardings => Set<SupplierOnboarding>();

    public DbSet<TenantSupplierOnboardingSettings> TenantSupplierOnboardingSettings =>
        Set<TenantSupplierOnboardingSettings>();

    public DbSet<SupplierRestriction> SupplierRestrictions => Set<SupplierRestriction>();

    public DbSet<SupplierIncident> SupplierIncidents => Set<SupplierIncident>();

    public DbSet<ProcurementException> ProcurementExceptions => Set<ProcurementException>();

    public DbSet<StaffarrProcurementApprovalAuthorityMirror> StaffarrProcurementApprovalAuthorityMirrors =>
        Set<StaffarrProcurementApprovalAuthorityMirror>();

    public DbSet<SupplyArrAuditEvent> AuditEvents => Set<SupplyArrAuditEvent>();

    public DbSet<PartCatalog> PartCatalogs => Set<PartCatalog>();

    public DbSet<Part> Parts => Set<Part>();

    public DbSet<PartManufacturerAlias> PartManufacturerAliases => Set<PartManufacturerAlias>();

    public DbSet<PartSource> PartSources => Set<PartSource>();

    public DbSet<PartSupplierLink> PartSupplierLinks => Set<PartSupplierLink>();

    public DbSet<InventoryLocation> InventoryLocations => Set<InventoryLocation>();

    public DbSet<InventoryBin> InventoryBins => Set<InventoryBin>();

    public DbSet<PartStockLevel> PartStockLevels => Set<PartStockLevel>();

    public DbSet<PartStockReservation> PartStockReservations => Set<PartStockReservation>();

    public DbSet<WmsStockLedgerEntry> WmsStockLedgerEntries => Set<WmsStockLedgerEntry>();

    public DbSet<WmsOutboundShipment> WmsOutboundShipments => Set<WmsOutboundShipment>();

    public DbSet<WmsOutboundShipmentLine> WmsOutboundShipmentLines => Set<WmsOutboundShipmentLine>();

    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();

    public DbSet<PurchaseRequestLine> PurchaseRequestLines => Set<PurchaseRequestLine>();

    public DbSet<SupplierOrder> SupplierOrders => Set<SupplierOrder>();

    public DbSet<SupplierOrderStatusUpdate> SupplierOrderStatusUpdates => Set<SupplierOrderStatusUpdate>();

    public DbSet<SupplierOrderMagicLink> SupplierOrderMagicLinks => Set<SupplierOrderMagicLink>();

    public DbSet<SupplierOrderDocumentLink> SupplierOrderDocumentLinks => Set<SupplierOrderDocumentLink>();

    public DbSet<SupplierOrderBrokerDecision> SupplierOrderBrokerDecisions => Set<SupplierOrderBrokerDecision>();

    public DbSet<TenantSupplierOrderSettings> TenantSupplierOrderSettings => Set<TenantSupplierOrderSettings>();

    public DbSet<Rfq> Rfqs => Set<Rfq>();

    public DbSet<RfqLine> RfqLines => Set<RfqLine>();

    public DbSet<RfqSupplierInvitation> RfqSupplierInvitations => Set<RfqSupplierInvitation>();

    public DbSet<SupplierQuote> SupplierQuotes => Set<SupplierQuote>();

    public DbSet<SupplierQuoteLine> SupplierQuoteLines => Set<SupplierQuoteLine>();

    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

    public DbSet<ReceivingReceipt> ReceivingReceipts => Set<ReceivingReceipt>();

    public DbSet<ReceivingReceiptLine> ReceivingReceiptLines => Set<ReceivingReceiptLine>();

    public DbSet<ReceivingException> ReceivingExceptions => Set<ReceivingException>();

    public DbSet<Backorder> Backorders => Set<Backorder>();

    public DbSet<SupplierReturn> SupplierReturns => Set<SupplierReturn>();

    public DbSet<SupplierReturnLine> SupplierReturnLines => Set<SupplierReturnLine>();

    public DbSet<WarrantyClaim> WarrantyClaims => Set<WarrantyClaim>();

    public DbSet<SupplyContract> SupplyContracts => Set<SupplyContract>();

    public DbSet<PartSupplierPricingSnapshot> PartSupplierPricingSnapshots => Set<PartSupplierPricingSnapshot>();

    public DbSet<PartSupplierLeadTimeSnapshot> PartSupplierLeadTimeSnapshots => Set<PartSupplierLeadTimeSnapshot>();

    public DbSet<PartSupplierAvailabilitySnapshot> PartSupplierAvailabilitySnapshots => Set<PartSupplierAvailabilitySnapshot>();

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

    public DbSet<PartSupplierPriceCaptureState> PartSupplierPriceCaptureStates => Set<PartSupplierPriceCaptureState>();

    public DbSet<PriceSnapshotRun> PriceSnapshotRuns => Set<PriceSnapshotRun>();

    public DbSet<TenantLeadTimeSnapshotSettings> TenantLeadTimeSnapshotSettings => Set<TenantLeadTimeSnapshotSettings>();

    public DbSet<PartSupplierLeadTimeCaptureState> PartSupplierLeadTimeCaptureStates => Set<PartSupplierLeadTimeCaptureState>();

    public DbSet<LeadTimeSnapshotRun> LeadTimeSnapshotRuns => Set<LeadTimeSnapshotRun>();

    public DbSet<TenantAvailabilitySnapshotSettings> TenantAvailabilitySnapshotSettings =>
        Set<TenantAvailabilitySnapshotSettings>();

    public DbSet<PartSupplierAvailabilityCaptureState> PartSupplierAvailabilityCaptureStates =>
        Set<PartSupplierAvailabilityCaptureState>();

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

    public DbSet<SupplierEmailInboxMessage> SupplierEmailInboxMessages => Set<SupplierEmailInboxMessage>();

    public DbSet<IntegrationEventProcessingRun> IntegrationEventProcessingRuns =>
        Set<IntegrationEventProcessingRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("supplyarr_suppliers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SupplierKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.UnitKind).HasMaxLength(32).HasDefaultValue("identity").IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.LegalName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.TaxIdentifier).HasMaxLength(64);
            entity.Property(x => x.ApprovalStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ServiceTypesJson).HasMaxLength(2048).HasDefaultValue("[]").IsRequired();
            entity.Property(x => x.AddressLine1).HasMaxLength(256).HasDefaultValue(string.Empty).IsRequired();
            entity.Property(x => x.AddressLine2).HasMaxLength(256).HasDefaultValue(string.Empty).IsRequired();
            entity.Property(x => x.Locality).HasMaxLength(128).HasDefaultValue(string.Empty).IsRequired();
            entity.Property(x => x.RegionCode).HasMaxLength(64).HasDefaultValue(string.Empty).IsRequired();
            entity.Property(x => x.PostalCode).HasMaxLength(32).HasDefaultValue(string.Empty).IsRequired();
            entity.Property(x => x.CountryCode).HasMaxLength(2).HasDefaultValue(string.Empty).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SupplierKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ApprovalStatus });
            entity.HasIndex(x => new { x.TenantId, x.ParentSupplierId });
            entity.HasOne(x => x.ParentSupplier)
                .WithMany(x => x.ChildSuppliers)
                .HasForeignKey(x => x.ParentSupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SupplierContact>(entity =>
        {
            entity.ToTable("supplyarr_supplier_contacts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ContactName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RoleLabel).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SupplierId });
            entity.HasOne(x => x.Supplier)
                .WithMany(x => x.Contacts)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SupplierComplianceDocument>(entity =>
        {
            entity.ToTable("supplyarr_supplier_compliance_documents");
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
            entity.HasIndex(x => new { x.TenantId, x.SupplierId });
            entity.HasIndex(x => new { x.TenantId, x.SupplierId, x.DocumentKey, x.Version }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ExpiresAt });
            entity.HasIndex(x => new { x.TenantId, x.ReviewStatus, x.UpdatedAt });
            entity.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SupplierOnboarding>(entity =>
        {
            entity.ToTable("supplyarr_supplier_onboarding");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OnboardingStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.RejectionReason).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SupplierId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.OnboardingStatus, x.SubmittedAt });
            entity.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenantSupplierOnboardingSettings>(entity =>
        {
            entity.ToTable("supplyarr_tenant_supplier_onboarding_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequiredDocumentTypeKeysJson).HasMaxLength(2048).IsRequired();
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<SupplierRestriction>(entity =>
        {
            entity.ToTable("supplyarr_supplier_restrictions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RestrictionKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ScopesJson).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.LiftNotes).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SupplierId });
            entity.HasIndex(x => new { x.TenantId, x.SupplierId, x.RestrictionKey, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
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
            entity.Property(x => x.LastReopenReason).HasMaxLength(512).IsRequired();
            entity.Property(x => x.ResolutionTemplateKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ExceptionKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SubjectType, x.SubjectId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.SlaDueAt });
            entity.HasIndex(x => new { x.TenantId, x.LastEscalatedAt });
        });

        modelBuilder.Entity<SupplierOrder>(entity =>
        {
            entity.ToTable("supplyarr_supplier_orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BrokerOrderNumberSnapshot).HasMaxLength(128);
            entity.Property(x => x.SupplierNameSnapshot).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PickupLocationNameSnapshot).HasMaxLength(256);
            entity.Property(x => x.PickupAddressSnapshot).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.CustomerIdSnapshot).HasMaxLength(128);
            entity.Property(x => x.DeliveryLocationNameSnapshot).HasMaxLength(256);
            entity.Property(x => x.DeliveryAddressSnapshot).HasMaxLength(1024);
            entity.Property(x => x.ItemDescription).HasMaxLength(512).IsRequired();
            entity.Property(x => x.OrderedQuantity).HasPrecision(18, 4);
            entity.Property(x => x.QuantityReady).HasPrecision(18, 4);
            entity.Property(x => x.QuantityRemaining).HasPrecision(18, 4);
            entity.Property(x => x.QuantityUom).HasMaxLength(32).IsRequired();
            entity.Property(x => x.PickupInstructions).HasMaxLength(2048);
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CreatedByPersonId).HasMaxLength(128);
            entity.Property(x => x.SplitReason).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SupplierId, x.Status, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.BrokerOrderId });
            entity.HasIndex(x => new { x.TenantId, x.ParentSupplierOrderId });
            entity.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ParentSupplierOrder)
                .WithMany(x => x.ChildSupplierOrders)
                .HasForeignKey(x => x.ParentSupplierOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SupplierOrderStatusUpdate>(entity =>
        {
            entity.ToTable("supplyarr_supplier_order_status_updates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PreviousStatus).HasMaxLength(64);
            entity.Property(x => x.NewStatus).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OrderedQuantitySnapshot).HasPrecision(18, 4);
            entity.Property(x => x.QuantityReady).HasPrecision(18, 4);
            entity.Property(x => x.QuantityRemaining).HasPrecision(18, 4);
            entity.Property(x => x.Note).HasMaxLength(2048);
            entity.Property(x => x.ExceptionReason).HasMaxLength(1024);
            entity.Property(x => x.Source).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SubmittedByPersonId).HasMaxLength(128);
            entity.Property(x => x.SubmittedIpHash).HasMaxLength(128);
            entity.Property(x => x.SubmittedUserAgentHash).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SupplierOrderId, x.CreatedAt });
            entity.HasOne(x => x.SupplierOrder)
                .WithMany(x => x.StatusUpdates)
                .HasForeignKey(x => x.SupplierOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SupplierOrderMagicLink>(entity =>
        {
            entity.ToTable("supplyarr_supplier_order_magic_links");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CreatedByPersonId).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SupplierOrderId });
            entity.HasIndex(x => new { x.TenantId, x.TokenHash }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ExpiresAt });
            entity.HasOne(x => x.SupplierOrder)
                .WithMany(x => x.MagicLinks)
                .HasForeignKey(x => x.SupplierOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SupplierOrderDocumentLink>(entity =>
        {
            entity.ToTable("supplyarr_supplier_order_document_links");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DocumentType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.StorageProvider).HasMaxLength(64);
            entity.Property(x => x.StorageKey).HasMaxLength(512);
            entity.Property(x => x.RecordArrRecordId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.RecordArrRecordNumberSnapshot).HasMaxLength(128).IsRequired();
            entity.Property(x => x.RecordArrFileId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.UploadedBySupplierContactId).HasMaxLength(128);
            entity.Property(x => x.UploadedByPersonId).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SupplierOrderId, x.UploadedAt });
            entity.HasOne(x => x.SupplierOrder)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.SupplierOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SupplierOrderBrokerDecision>(entity =>
        {
            entity.ToTable("supplyarr_supplier_order_broker_decisions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DecisionType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AuthorizedQuantity).HasPrecision(18, 4);
            entity.Property(x => x.Note).HasMaxLength(1024);
            entity.Property(x => x.DecidedByPersonId).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SupplierOrderId, x.CreatedAt });
            entity.HasOne(x => x.SupplierOrder)
                .WithMany(x => x.BrokerDecisions)
                .HasForeignKey(x => x.SupplierOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenantSupplierOrderSettings>(entity =>
        {
            entity.ToTable("supplyarr_tenant_supplier_order_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
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
            entity.Property(x => x.LastReopenReason).HasMaxLength(512).IsRequired();
            entity.Property(x => x.StaffarrIncidentRouteStatus).HasMaxLength(32).HasDefaultValue(string.Empty).IsRequired();
            entity.Property(x => x.TrainarrIncidentRouteStatus).HasMaxLength(32).HasDefaultValue(string.Empty).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IncidentKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SupplierId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.Severity });
            entity.HasIndex(x => new { x.TenantId, x.InvolvedStaffarrPersonId })
                .HasDatabaseName("IX_supplyarr_supplier_incidents_staffarr_person");
            entity.HasIndex(x => new { x.TenantId, x.StaffarrPersonnelIncidentId })
                .HasDatabaseName("IX_supplyarr_supplier_incidents_staffarr_incident");
            entity.HasIndex(x => new { x.TenantId, x.TrainarrIncidentRemediationId })
                .HasDatabaseName("IX_supplyarr_supplier_incidents_trainarr_remediation");
            entity.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.SupplierRestriction)
                .WithMany()
                .HasForeignKey(x => x.SupplierRestrictionId)
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
            entity.Property(x => x.IsTrackable).HasDefaultValue(true);
            entity.Property(x => x.IsStocked).HasDefaultValue(true);
            entity.Property(x => x.RequiresSerialLotTracking).HasDefaultValue(false);
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

        modelBuilder.Entity<PartSource>(entity =>
        {
            entity.ToTable("supplyarr_part_sources");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PartId });
            entity.HasIndex(x => new { x.TenantId, x.PartId, x.SourceType, x.Label });
            entity.HasOne(x => x.Part)
                .WithMany(x => x.Sources)
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PartSupplierLink>(entity =>
        {
            entity.ToTable("supplyarr_part_supplier_links");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SupplierPartNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CatalogUnitPrice).HasPrecision(18, 4);
            entity.Property(x => x.CatalogCurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.CatalogMinimumOrderQuantity).HasPrecision(18, 4);
            entity.Property(x => x.CatalogQuantityAvailable).HasPrecision(18, 4);
            entity.Property(x => x.CatalogAvailabilityStatus).HasMaxLength(32);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PartId });
            entity.HasIndex(x => new { x.TenantId, x.PartId, x.SupplierId }).IsUnique();
            entity.HasOne(x => x.Part)
                .WithMany(x => x.SupplierLinks)
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
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
            entity.Property(x => x.StaffarrSiteNameSnapshot).HasMaxLength(256).HasDefaultValue(string.Empty).IsRequired();
            entity.Property(x => x.StaffarrSiteResolutionStatus).HasMaxLength(32).HasDefaultValue("unassigned").IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.LocationKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.LocationType, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.StaffarrSiteOrgUnitId });
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

        modelBuilder.Entity<PartStockReservation>(entity =>
        {
            entity.ToTable("supplyarr_part_stock_reservations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReservationKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.QuantityReserved).HasPrecision(18, 4);
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ReleaseReason).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ReservationKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PartId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.InventoryBinId, x.Status });
            entity.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InventoryBin)
                .WithMany()
                .HasForeignKey(x => x.InventoryBinId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PartStockLevel)
                .WithMany()
                .HasForeignKey(x => x.PartStockLevelId)
                .OnDelete(DeleteBehavior.Restrict);
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
            entity.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
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
            entity.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
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
            entity.Property(x => x.PackingSlipReference).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PackingSlipFileName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.InvoiceReference).HasMaxLength(256).IsRequired();
            entity.Property(x => x.InvoiceFileName).HasMaxLength(256).IsRequired();
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
            entity.Property(x => x.Condition).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SerialLotNumbersJson).HasMaxLength(4096).IsRequired();
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
            entity.Property(x => x.CancellationReason).HasMaxLength(512).IsRequired();
            entity.Property(x => x.LastReopenReason).HasMaxLength(512).IsRequired();
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

        modelBuilder.Entity<SupplierReturn>(entity =>
        {
            entity.ToTable("supplyarr_supplier_returns");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReturnKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RmaNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.CancellationReason).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ReturnKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SupplierId });
            entity.HasIndex(x => new { x.TenantId, x.PurchaseOrderId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
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

        modelBuilder.Entity<SupplierReturnLine>(entity =>
        {
            entity.ToTable("supplyarr_supplier_return_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Quantity).HasPrecision(18, 4);
            entity.Property(x => x.Notes).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SupplierReturnId });
            entity.HasIndex(x => new { x.TenantId, x.SupplierReturnId, x.LineNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PartId });
            entity.HasOne(x => x.SupplierReturn)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.SupplierReturnId)
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
            entity.Property(x => x.SupplierRmaNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SupplierDisposition).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SupplierResponseNotes).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.ClosureNotes).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.DenialReason).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.CancellationReason).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ClaimKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SupplierId });
            entity.HasIndex(x => new { x.TenantId, x.PartId });
            entity.HasIndex(x => new { x.TenantId, x.PurchaseOrderId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
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

        modelBuilder.Entity<SupplyContract>(entity =>
        {
            entity.ToTable("supplyarr_contracts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ContractKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ContractType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PaymentTerms).HasMaxLength(256).IsRequired();
            entity.Property(x => x.FreightTerms).HasMaxLength(256).IsRequired();
            entity.Property(x => x.WarrantyTerms).HasMaxLength(512).IsRequired();
            entity.Property(x => x.MinimumSpend).HasPrecision(18, 2);
            entity.Property(x => x.ServiceLevelAgreement).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ApprovalStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ContractKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SupplierId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ExpiresAt });
            entity.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WmsStockLedgerEntry>(entity =>
        {
            entity.ToTable("supplyarr_wms_stock_ledger");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.MovementType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.QuantityOnHandDelta).HasPrecision(18, 4);
            entity.Property(x => x.QuantityReservedDelta).HasPrecision(18, 4);
            entity.Property(x => x.QuantityOnHandAfter).HasPrecision(18, 4);
            entity.Property(x => x.QuantityReservedAfter).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IdempotencyKey });
            entity.HasIndex(x => new { x.TenantId, x.MovementGroupId });
            entity.HasIndex(x => new { x.TenantId, x.PartId, x.InventoryBinId, x.CreatedAt });
            entity.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InventoryBin)
                .WithMany()
                .HasForeignKey(x => x.InventoryBinId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RelatedInventoryBin)
                .WithMany()
                .HasForeignKey(x => x.RelatedInventoryBinId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WmsOutboundShipment>(entity =>
        {
            entity.ToTable("supplyarr_wms_outbound_shipments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ShipmentKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ShipVia).HasMaxLength(32).IsRequired();
            entity.Property(x => x.DestinationName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DestinationAddressSnapshot).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.RoutarrStatus).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ShipmentKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
        });

        modelBuilder.Entity<WmsOutboundShipmentLine>(entity =>
        {
            entity.ToTable("supplyarr_wms_outbound_shipment_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.QuantityRequested).HasPrecision(18, 4);
            entity.Property(x => x.QuantityReserved).HasPrecision(18, 4);
            entity.Property(x => x.QuantityPicked).HasPrecision(18, 4);
            entity.Property(x => x.QuantityShipped).HasPrecision(18, 4);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.OutboundShipmentId });
            entity.HasOne(x => x.OutboundShipment)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.OutboundShipmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.FromInventoryBin)
                .WithMany()
                .HasForeignKey(x => x.FromInventoryBinId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PartSupplierPricingSnapshot>(entity =>
        {
            entity.ToTable("supplyarr_part_supplier_pricing_snapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SnapshotKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.UnitPrice).HasPrecision(18, 4);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.MinimumOrderQuantity).HasPrecision(18, 4);
            entity.Property(x => x.Source).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SnapshotKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PartSupplierLinkId, x.EffectiveFrom });
            entity.HasIndex(x => new { x.TenantId, x.PartSupplierLinkId, x.EffectiveTo });
            entity.HasOne(x => x.PartSupplierLink)
                .WithMany()
                .HasForeignKey(x => x.PartSupplierLinkId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PartSupplierLeadTimeSnapshot>(entity =>
        {
            entity.ToTable("supplyarr_part_supplier_lead_time_snapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SnapshotKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SnapshotKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PartSupplierLinkId, x.EffectiveFrom });
            entity.HasIndex(x => new { x.TenantId, x.PartSupplierLinkId, x.EffectiveTo });
            entity.HasOne(x => x.PartSupplierLink)
                .WithMany()
                .HasForeignKey(x => x.PartSupplierLinkId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PartSupplierAvailabilitySnapshot>(entity =>
        {
            entity.ToTable("supplyarr_part_supplier_availability_snapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SnapshotKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.QuantityAvailable).HasPrecision(18, 4);
            entity.Property(x => x.AvailabilityStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SnapshotKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PartSupplierLinkId, x.EffectiveFrom });
            entity.HasIndex(x => new { x.TenantId, x.PartSupplierLinkId, x.EffectiveTo });
            entity.HasOne(x => x.PartSupplierLink)
                .WithMany()
                .HasForeignKey(x => x.PartSupplierLinkId)
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

        modelBuilder.Entity<PartSupplierPriceCaptureState>(entity =>
        {
            entity.ToTable("supplyarr_part_supplier_price_capture_states");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LastCapturedUnitPrice).HasPrecision(18, 4);
            entity.Property(x => x.LastCapturedCurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.LastCapturedMinimumOrderQuantity).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PartSupplierLinkId }).IsUnique();
            entity.HasOne(x => x.PartSupplierLink)
                .WithMany()
                .HasForeignKey(x => x.PartSupplierLinkId)
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

        modelBuilder.Entity<PartSupplierLeadTimeCaptureState>(entity =>
        {
            entity.ToTable("supplyarr_part_supplier_lead_time_capture_states");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PartSupplierLinkId }).IsUnique();
            entity.HasOne(x => x.PartSupplierLink)
                .WithMany()
                .HasForeignKey(x => x.PartSupplierLinkId)
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

        modelBuilder.Entity<PartSupplierAvailabilityCaptureState>(entity =>
        {
            entity.ToTable("supplyarr_part_supplier_availability_capture_states");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LastCapturedAvailabilityStatus).HasMaxLength(32);
            entity.Property(x => x.LastCapturedQuantityAvailable).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PartSupplierLinkId }).IsUnique();
            entity.HasOne(x => x.PartSupplierLink)
                .WithMany()
                .HasForeignKey(x => x.PartSupplierLinkId)
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
            entity.Property(x => x.SupplierKey).HasMaxLength(128);
            entity.Property(x => x.SupplierDisplayName).HasMaxLength(256);
            entity.Property(x => x.ParentSupplierDisplayName).HasMaxLength(256);
            entity.Property(x => x.SupplierUnitKind).HasMaxLength(32);
            entity.Property(x => x.SupplierServiceTypesJson).HasMaxLength(2048).HasDefaultValue("[]").IsRequired();
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
            entity.Property(x => x.EscalationCooldownHours).HasDefaultValue(ProcurementExceptionEscalationDefaults.EscalationCooldownHours);
            entity.Property(x => x.MaxEscalationsPerException).HasDefaultValue(ProcurementExceptionEscalationDefaults.MaxEscalationsPerException);
            entity.Property(x => x.NotifyOnProcurementExceptionSlaEscalation).HasDefaultValue(true);
            entity.Property(x => x.AutoCloseCompletedExceptionsEnabled).HasDefaultValue(false);
            entity.Property(x => x.AutoCloseCompletedExceptionsAfterHours).HasDefaultValue(
                ProcurementExceptionEscalationDefaults.AutoCloseCompletedExceptionsAfterHours);
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

        modelBuilder.Entity<SupplierEmailInboxMessage>(entity =>
        {
            entity.ToTable("supplyarr_supplier_email_inbox_messages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MessageKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.MessageKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SenderEmail).HasMaxLength(256).IsRequired();
            entity.Property(x => x.SenderName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(512).IsRequired();
            entity.Property(x => x.BodyPreview).HasMaxLength(4096).IsRequired();
            entity.Property(x => x.MatchStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.MatchReason).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.SupplierKey).HasMaxLength(128);
            entity.Property(x => x.SupplierDisplayName).HasMaxLength(256);
            entity.Property(x => x.LinkedReferenceType).HasMaxLength(64);
            entity.Property(x => x.LinkedReferenceKey).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.MessageKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.MessageKind, x.ReceivedAt });
            entity.HasIndex(x => new { x.TenantId, x.MatchStatus, x.ReceivedAt });
            entity.HasIndex(x => new { x.TenantId, x.SupplierId, x.ReceivedAt });
            entity.HasIndex(x => new { x.TenantId, x.LinkedReferenceType, x.LinkedReferenceId });
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

        modelBuilder.Entity<RfqSupplierInvitation>(entity =>
        {
            entity.ToTable("supplyarr_rfq_supplier_invitations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.PortalAccessCode).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PortalAccessCodeIssuedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PortalAccessCodeExpiresAt).HasColumnType("timestamp with time zone");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RfqId, x.SupplierId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RfqId, x.PortalAccessCode }).IsUnique();
            entity.HasOne(x => x.Rfq).WithMany(x => x.SupplierInvitations).HasForeignKey(x => x.RfqId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SupplierQuote>(entity =>
        {
            entity.ToTable("supplyarr_supplier_quotes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.QuoteKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(8).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RfqId, x.QuoteKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RfqId, x.SupplierId });
            entity.HasOne(x => x.Rfq).WithMany(x => x.SupplierQuotes).HasForeignKey(x => x.RfqId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SupplierQuoteLine>(entity =>
        {
            entity.ToTable("supplyarr_supplier_quote_lines");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SupplierQuoteId, x.RfqLineId }).IsUnique();
            entity.HasOne(x => x.SupplierQuote).WithMany(x => x.Lines).HasForeignKey(x => x.SupplierQuoteId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.RfqLine).WithMany().HasForeignKey(x => x.RfqLineId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}

