using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Data;

public sealed class SupplyArrDbContext(DbContextOptions<SupplyArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<ExternalParty> ExternalParties => Set<ExternalParty>();

    public DbSet<PartyContact> PartyContacts => Set<PartyContact>();

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

    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

    public DbSet<ReceivingReceipt> ReceivingReceipts => Set<ReceivingReceipt>();

    public DbSet<ReceivingReceiptLine> ReceivingReceiptLines => Set<ReceivingReceiptLine>();

    public DbSet<ReceivingException> ReceivingExceptions => Set<ReceivingException>();

    public DbSet<Backorder> Backorders => Set<Backorder>();

    public DbSet<VendorReturn> VendorReturns => Set<VendorReturn>();

    public DbSet<VendorReturnLine> VendorReturnLines => Set<VendorReturnLine>();

    public DbSet<PartVendorPricingSnapshot> PartVendorPricingSnapshots => Set<PartVendorPricingSnapshot>();

    public DbSet<PartVendorLeadTimeSnapshot> PartVendorLeadTimeSnapshots => Set<PartVendorLeadTimeSnapshot>();

    public DbSet<PartVendorAvailabilitySnapshot> PartVendorAvailabilitySnapshots => Set<PartVendorAvailabilitySnapshot>();

    public DbSet<MaintainArrDemandRef> MaintainArrDemandRefs => Set<MaintainArrDemandRef>();

    public DbSet<MaintainArrDemandRefLine> MaintainArrDemandRefLines => Set<MaintainArrDemandRefLine>();

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

    public DbSet<TenantDemandProcessingSettings> TenantDemandProcessingSettings =>
        Set<TenantDemandProcessingSettings>();

    public DbSet<DemandProcessingState> DemandProcessingStates => Set<DemandProcessingState>();

    public DbSet<DemandProcessingRun> DemandProcessingRuns => Set<DemandProcessingRun>();

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
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RequestKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
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
            entity.Property(x => x.MaintainarrWorkOrderNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ProcessingOutcome).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RecommendedAction).HasMaxLength(32).IsRequired();
            entity.Property(x => x.LastProcessingMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DemandRefId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.LastProcessedAt });
            entity.HasOne(x => x.DemandRef)
                .WithMany()
                .HasForeignKey(x => x.DemandRefId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DemandProcessingRun>(entity =>
        {
            entity.ToTable("supplyarr_demand_processing_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });
    }
}
