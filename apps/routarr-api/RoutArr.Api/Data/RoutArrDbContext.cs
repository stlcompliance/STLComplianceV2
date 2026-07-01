using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Data;

namespace RoutArr.Api.Data;

public sealed class RoutArrDbContext(DbContextOptions<RoutArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<Trip> Trips => Set<Trip>();

    public DbSet<TripLoad> TripLoads => Set<TripLoad>();

    public DbSet<TripPartsDemandLine> TripPartsDemandLines => Set<TripPartsDemandLine>();

    public DbSet<TripPartsDemandStatusEvent> TripPartsDemandStatusEvents => Set<TripPartsDemandStatusEvent>();

    public DbSet<SupplyArrShipmentIntent> SupplyArrShipmentIntents => Set<SupplyArrShipmentIntent>();

    public DbSet<SupplyArrShipmentIntentLine> SupplyArrShipmentIntentLines => Set<SupplyArrShipmentIntentLine>();

    public DbSet<DispatchRoute> Routes => Set<DispatchRoute>();

    public DbSet<DispatchPlan> DispatchPlans => Set<DispatchPlan>();

    public DbSet<RouteStop> RouteStops => Set<RouteStop>();

    public DbSet<DriverAvailability> DriverAvailabilities => Set<DriverAvailability>();

    public DbSet<EquipmentAvailability> EquipmentAvailabilities => Set<EquipmentAvailability>();

    public DbSet<DriverTimeEntry> DriverTimeEntries => Set<DriverTimeEntry>();

    public DbSet<RoutArrAuditEvent> AuditEvents => Set<RoutArrAuditEvent>();

    public DbSet<TenantDispatchNotificationSettings> TenantDispatchNotificationSettings =>
        Set<TenantDispatchNotificationSettings>();

    public DbSet<TenantTripExecutionSettings> TenantTripExecutionSettings =>
        Set<TenantTripExecutionSettings>();

    public DbSet<DispatchNotificationDispatch> DispatchNotificationDispatches =>
        Set<DispatchNotificationDispatch>();

    public DbSet<TenantTripCompletionRollupSettings> TenantTripCompletionRollupSettings =>
        Set<TenantTripCompletionRollupSettings>();

    public DbSet<TripCompletionRollup> TripCompletionRollups => Set<TripCompletionRollup>();

    public DbSet<TripCompletionEvent> TripCompletionEvents => Set<TripCompletionEvent>();

    public DbSet<TripCompletionRollupRun> TripCompletionRollupRuns => Set<TripCompletionRollupRun>();

    public DbSet<TenantDispatchBoardState> TenantDispatchBoardStates => Set<TenantDispatchBoardState>();

    public DbSet<StaffarrPersonRef> StaffarrPersonRefs => Set<StaffarrPersonRef>();

    public DbSet<RoutarrVehicleRef> RoutarrVehicleRefs => Set<RoutarrVehicleRef>();

    public DbSet<DispatchException> DispatchExceptions => Set<DispatchException>();

    public DbSet<DispatchMessage> DispatchMessages => Set<DispatchMessage>();

    public DbSet<TripDispatchReleaseSnapshot> TripDispatchReleaseSnapshots => Set<TripDispatchReleaseSnapshot>();

    public DbSet<DispatchBlock> DispatchBlocks => Set<DispatchBlock>();

    public DbSet<SupplyArrSupplierOrderEventReceipt> SupplyArrSupplierOrderEventReceipts =>
        Set<SupplyArrSupplierOrderEventReceipt>();

    public DbSet<TripProofRecord> TripProofRecords => Set<TripProofRecord>();

    public DbSet<TripDvirInspection> TripDvirInspections => Set<TripDvirInspection>();

    public DbSet<TripCaptureAttachment> TripCaptureAttachments => Set<TripCaptureAttachment>();

    public DbSet<TenantAttachmentRetentionSettings> TenantAttachmentRetentionSettings =>
        Set<TenantAttachmentRetentionSettings>();

    public DbSet<AttachmentRetentionRun> AttachmentRetentionRuns => Set<AttachmentRetentionRun>();

    public DbSet<AuditPackageGenerationJob> AuditPackageGenerationJobs => Set<AuditPackageGenerationJob>();

    public DbSet<TenantIntegrationEventSettings> TenantIntegrationEventSettings =>
        Set<TenantIntegrationEventSettings>();

    public DbSet<IntegrationOutboxEvent> IntegrationOutboxEvents => Set<IntegrationOutboxEvent>();

    public DbSet<TransportationDemand> TransportationDemands => Set<TransportationDemand>();

    public DbSet<TransportationDemandLine> TransportationDemandLines => Set<TransportationDemandLine>();

    public DbSet<TransportationDemandRequirement> TransportationDemandRequirements => Set<TransportationDemandRequirement>();

    public DbSet<TransportationDemandSourceRef> TransportationDemandSourceRefs => Set<TransportationDemandSourceRef>();

    public DbSet<CarrierTender> CarrierTenders => Set<CarrierTender>();

    public DbSet<RoutingGuideStep> RoutingGuideSteps => Set<RoutingGuideStep>();

    public DbSet<FreightRating> FreightRatings => Set<FreightRating>();

    public DbSet<FreightAccessorial> FreightAccessorials => Set<FreightAccessorial>();

    public DbSet<TransportationVisibilityEvent> TransportationVisibilityEvents => Set<TransportationVisibilityEvent>();

    public DbSet<TransportationTrackingSnapshot> TransportationTrackingSnapshots => Set<TransportationTrackingSnapshot>();

    public DbSet<TransportationPlanningScenario> TransportationPlanningScenarios => Set<TransportationPlanningScenario>();

    public DbSet<TransportationPlanningSuggestion> TransportationPlanningSuggestions => Set<TransportationPlanningSuggestion>();

    public DbSet<DriverCapacitySnapshot> DriverCapacitySnapshots => Set<DriverCapacitySnapshot>();

    public DbSet<TransportationYardEvent> TransportationYardEvents => Set<TransportationYardEvent>();

    public DbSet<PortalCollaborationSubmission> PortalCollaborationSubmissions => Set<PortalCollaborationSubmission>();

    public DbSet<FreightClaim> FreightClaims => Set<FreightClaim>();

    public DbSet<TransportationDocumentPacketRequest> TransportationDocumentPacketRequests =>
        Set<TransportationDocumentPacketRequest>();

    public DbSet<TransportationAppointmentClock> TransportationAppointmentClocks => Set<TransportationAppointmentClock>();

    public DbSet<ModeSpecificRequirementRef> ModeSpecificRequirementRefs => Set<ModeSpecificRequirementRef>();

    public DbSet<TransportationFinancePacketContribution> TransportationFinancePacketContributions =>
        Set<TransportationFinancePacketContribution>();

    public DbSet<RoutArrTenantSettings> RoutArrTenantSettings => Set<RoutArrTenantSettings>();

    public DbSet<RoutArrTenantSettingValue> RoutArrTenantSettingValues => Set<RoutArrTenantSettingValue>();

    public DbSet<RoutArrTenantSettingListItem> RoutArrTenantSettingListItems => Set<RoutArrTenantSettingListItem>();

    public DbSet<RoutArrTenantSettingOverride> RoutArrTenantSettingOverrides => Set<RoutArrTenantSettingOverride>();

    public DbSet<RoutArrTenantSettingOverrideListItem> RoutArrTenantSettingOverrideListItems =>
        Set<RoutArrTenantSettingOverrideListItem>();

    public DbSet<RoutArrTenantSettingAuditEntry> RoutArrTenantSettingAuditEntries =>
        Set<RoutArrTenantSettingAuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.ToTable("routarr_trips");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TripNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.DispatchStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.AssignedDriverPersonId).HasMaxLength(128);
            entity.Property(x => x.VehicleRefKey).HasMaxLength(128);
            entity.Property(x => x.DispatchBlockReason).HasMaxLength(64);
            entity.Property(x => x.SupplierReadinessStatusSnapshot).HasMaxLength(64);
            entity.Property(x => x.SupplierQuantityReadySnapshot).HasPrecision(18, 4);
            entity.Property(x => x.SupplierOrderedQuantitySnapshot).HasPrecision(18, 4);
            entity.Property(x => x.DispatchOverrideByPersonId).HasMaxLength(128);
            entity.Property(x => x.DispatchOverrideReason).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TripNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.DispatchStatus, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.AssignedDriverPersonId });
            entity.HasIndex(x => new { x.TenantId, x.AcceptedAt });
            entity.HasIndex(x => new { x.TenantId, x.SupplierOrderId });
            entity.HasIndex(x => new { x.TenantId, x.BrokerOrderId });
            entity.HasOne(x => x.DispatchReleaseSnapshot)
                .WithOne(x => x.Trip)
                .HasForeignKey<TripDispatchReleaseSnapshot>(x => x.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripDispatchReleaseSnapshot>(entity =>
        {
            entity.ToTable("routarr_trip_dispatch_release_snapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Summary).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.SnapshotJson).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TripId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ReleasedAt });
        });

        modelBuilder.Entity<DispatchBlock>(entity =>
        {
            entity.ToTable("routarr_dispatch_blocks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BlockType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.BlockReason).HasMaxLength(64).IsRequired();
            entity.Property(x => x.BlockingEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.BlockingEntityId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ResolvedByPersonId).HasMaxLength(128);
            entity.Property(x => x.OverrideReason).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.BlockType, x.Status });
            entity.HasOne(x => x.Trip)
                .WithMany(x => x.DispatchBlocks)
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SupplyArrSupplierOrderEventReceipt>(entity =>
        {
            entity.ToTable("routarr_supplyarr_supplier_order_event_receipts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.EventId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SupplierOrderId, x.ProcessedAt });
        });

        modelBuilder.Entity<TripLoad>(entity =>
        {
            entity.ToTable("routarr_trip_loads");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LoadKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512).IsRequired();
            entity.Property(x => x.LoadType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.OriginLabel).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DestinationLabel).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TripId });
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.LoadKey }).IsUnique();
            entity.HasOne(x => x.Trip)
                .WithMany(x => x.Loads)
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripPartsDemandLine>(entity =>
        {
            entity.ToTable("routarr_trip_parts_demand_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PartNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(256).IsRequired();
            entity.Property(x => x.QuantityRequested).HasPrecision(18, 4);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProcurementStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProcurementStatusMessage).HasMaxLength(512);
            entity.Property(x => x.QuantityReceived).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TripId });
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.LineNumber });
            entity.HasIndex(x => new { x.TenantId, x.RoutarrPublicationId });
            entity.HasIndex(x => new { x.TenantId, x.ProcurementStatus });
            entity.HasOne(x => x.Trip)
                .WithMany(x => x.PartsDemandLines)
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripPartsDemandStatusEvent>(entity =>
        {
            entity.ToTable("routarr_trip_parts_demand_status_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProcurementStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RoutarrPublicationId, x.OccurredAt });
            entity.HasIndex(x => new { x.TenantId, x.SupplyarrCallbackPublicationId }).IsUnique();
        });

        modelBuilder.Entity<SupplyArrShipmentIntent>(entity =>
        {
            entity.ToTable("routarr_supplyarr_shipment_intents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ShipmentKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DestinationName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DestinationAddressSnapshot).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SupplyarrShipmentId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
        });

        modelBuilder.Entity<SupplyArrShipmentIntentLine>(entity =>
        {
            entity.ToTable("routarr_supplyarr_shipment_intent_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PartDisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ShipmentIntentId });
            entity.HasOne(x => x.ShipmentIntent)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.ShipmentIntentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DispatchRoute>(entity =>
        {
            entity.ToTable("routarr_routes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RouteNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.RouteStatus).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RouteNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.TripId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RouteStatus, x.UpdatedAt });
            entity.HasOne(x => x.Trip)
                .WithMany()
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DispatchPlan>(entity =>
        {
            entity.ToTable("routarr_dispatch_plans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DispatchNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.DispatchType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.PlannerPersonId).HasMaxLength(128);
            entity.Property(x => x.DispatcherPersonId).HasMaxLength(128);
            entity.Property(x => x.Notes).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.CreatedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ReleasedByPersonId).HasMaxLength(128);
            entity.Property(x => x.CancelReason).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DispatchNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.DispatchDate });
        });

        modelBuilder.Entity<RouteStop>(entity =>
        {
            entity.ToTable("routarr_route_stops");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StopKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(256).IsRequired();
            entity.Property(x => x.AddressLabel).HasMaxLength(512).IsRequired();
            entity.Property(x => x.StaffarrSiteNameSnapshot).HasMaxLength(256).HasDefaultValue(string.Empty).IsRequired();
            entity.Property(x => x.StopType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.StopStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.GeofenceAnchorLatitude).HasPrecision(10, 6);
            entity.Property(x => x.GeofenceAnchorLongitude).HasPrecision(10, 6);
            entity.Property(x => x.LastGeofenceDistanceMeters).HasPrecision(18, 2);
            entity.Property(x => x.LastGeofenceReportedLatitude).HasPrecision(10, 6);
            entity.Property(x => x.LastGeofenceReportedLongitude).HasPrecision(10, 6);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RouteId });
            entity.HasIndex(x => new { x.TenantId, x.RouteId, x.StopKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RouteId, x.SequenceNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.StaffarrSiteOrgUnitId });
            entity.HasOne(x => x.Route)
                .WithMany(x => x.Stops)
                .HasForeignKey(x => x.RouteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DriverAvailability>(entity =>
        {
            entity.ToTable("routarr_driver_availability");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AvailabilityStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.StartsAt });
            entity.HasIndex(x => new { x.TenantId, x.StartsAt, x.EndsAt });
        });

        modelBuilder.Entity<EquipmentAvailability>(entity =>
        {
            entity.ToTable("routarr_equipment_availability");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.VehicleRefKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AvailabilityStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.VehicleRefKey, x.StartsAt });
            entity.HasIndex(x => new { x.TenantId, x.StartsAt, x.EndsAt });
        });

        modelBuilder.Entity<DriverTimeEntry>(entity =>
        {
            entity.ToTable("routarr_driver_time_entries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EntryType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.EditReason).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.StartsAt });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.EntryType, x.StartsAt });
        });

        modelBuilder.Entity<RoutArrAuditEvent>(entity =>
        {
            entity.ToTable("routarr_audit_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasMaxLength(128).IsRequired();
            entity.Property(x => x.TargetType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TargetId).HasMaxLength(128);
            entity.Property(x => x.Result).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(64);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.OccurredAt });
        });

        modelBuilder.Entity<TenantDispatchNotificationSettings>(entity =>
        {
            entity.ToTable("routarr_tenant_notification_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NotificationWebhookUrl).HasMaxLength(2048);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<TenantTripExecutionSettings>(entity =>
        {
            entity.ToTable("routarr_tenant_trip_execution_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<DispatchNotificationDispatch>(entity =>
        {
            entity.ToTable("routarr_notification_dispatches");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DriverPersonId).HasMaxLength(128);
            entity.Property(x => x.RelatedEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DispatchStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.WebhookHost).HasMaxLength(256);
            entity.Property(x => x.ErrorMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DispatchStatus, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.EventKind, x.RelatedEntityType, x.RelatedEntityId });
        });

        modelBuilder.Entity<TenantTripCompletionRollupSettings>(entity =>
        {
            entity.ToTable("routarr_tenant_trip_completion_rollup_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<TripCompletionRollup>(entity =>
        {
            entity.ToTable("routarr_trip_completion_rollups");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TripNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DispatchStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.AssignedDriverPersonId).HasMaxLength(128);
            entity.Property(x => x.VehicleRefKey).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TripId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.DispatchStatus, x.CompletedAt });
        });

        modelBuilder.Entity<TripCompletionEvent>(entity =>
        {
            entity.ToTable("routarr_trip_completion_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Detail).HasMaxLength(1024);
            entity.Property(x => x.SourceEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceEntityId).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.SequenceNumber });
            entity.HasOne(x => x.Rollup)
                .WithMany(x => x.Events)
                .HasForeignKey(x => x.RollupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripCompletionRollupRun>(entity =>
        {
            entity.ToTable("routarr_trip_completion_rollup_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<TenantDispatchBoardState>(entity =>
        {
            entity.ToTable("routarr_tenant_dispatch_board_state");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DefaultScope).HasMaxLength(16).IsRequired();
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<StaffarrPersonRef>(entity =>
        {
            entity.ToTable("routarr_staffarr_person_refs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId }).IsUnique();
        });

        modelBuilder.Entity<RoutarrVehicleRef>(entity =>
        {
            entity.ToTable("routarr_vehicle_refs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.VehicleRefKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DisplayLabel).HasMaxLength(256).IsRequired();
            entity.Property(x => x.AssetTag).HasMaxLength(128);
            entity.Property(x => x.SourceProduct).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.VehicleRefKey }).IsUnique();
        });

        modelBuilder.Entity<TripProofRecord>(entity =>
        {
            entity.ToTable("routarr_trip_proof_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProofType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CapturedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.VehicleRefKey).HasMaxLength(128);
            entity.Property(x => x.ReferenceKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ReviewStatus).HasMaxLength(32).HasDefaultValue(TripProofReviewStatuses.PendingReview).IsRequired();
            entity.Property(x => x.ReviewedByPersonId).HasMaxLength(128);
            entity.Property(x => x.ReviewNotes).HasMaxLength(1024).HasDefaultValue(string.Empty).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TripId });
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.CapturedAt });
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.ReviewStatus });
            entity.HasOne(x => x.Trip)
                .WithMany()
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripDvirInspection>(entity =>
        {
            entity.ToTable("routarr_trip_dvir_inspections");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Phase).HasMaxLength(32).IsRequired();
            entity.Property(x => x.VehicleRefKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Result).HasMaxLength(32).IsRequired();
            entity.Property(x => x.DefectNotes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.MaintainarrEventRouteStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SubmittedByPersonId).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TripId });
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.Phase }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.MaintainarrDefectId })
                .HasDatabaseName("IX_routarr_trip_dvir_inspections_maintainarr_defect");
            entity.HasOne(x => x.Trip)
                .WithMany()
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripCaptureAttachment>(entity =>
        {
            entity.ToTable("routarr_trip_capture_attachments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SubjectType).HasMaxLength(16).IsRequired();
            entity.Property(x => x.AttachmentKind).HasMaxLength(32).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024);
            entity.Property(x => x.CapturedByPersonId).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TripId });
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.SubjectType, x.SubjectId });
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.AttachmentKind, x.CreatedAt });
            entity.HasOne(x => x.Trip)
                .WithMany()
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenantAttachmentRetentionSettings>(entity =>
        {
            entity.ToTable("routarr_tenant_attachment_retention_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<AttachmentRetentionRun>(entity =>
        {
            entity.ToTable("routarr_attachment_retention_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Outcome).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SkipReason).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ProcessedAt });
        });

        modelBuilder.Entity<DispatchException>(entity =>
        {
            entity.ToTable("routarr_dispatch_exceptions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExceptionKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.IncidentType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IncidentSeverity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.IncidentReviewStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.IncidentRoutedProduct).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StaffarrIncidentRouteStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.TrainarrIncidentRouteStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.MaintainarrIncidentRouteStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CompliancecoreIncidentRouteStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ResolutionTemplateKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ResolutionNotes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ExceptionKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.IncidentType, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.IncidentReviewStatus, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.StaffarrPersonnelIncidentId })
                .HasDatabaseName("IX_routarr_dispatch_exceptions_staffarr_incident");
            entity.HasIndex(x => new { x.TenantId, x.TrainarrIncidentRemediationId })
                .HasDatabaseName("IX_routarr_dispatch_exceptions_trainarr_remediation");
            entity.HasIndex(x => new { x.TenantId, x.MaintainarrDefectId })
                .HasDatabaseName("IX_routarr_dispatch_exceptions_maintainarr_defect");
            entity.HasIndex(x => new { x.TenantId, x.CompliancecoreFactPublicationId })
                .HasDatabaseName("IX_routarr_dispatch_exceptions_compliancecore_publication");
            entity.HasIndex(x => new { x.TenantId, x.TripId });
            entity.HasIndex(x => new { x.TenantId, x.AssignedToUserId });
            entity.HasIndex(x => new { x.TenantId, x.SlaDueAt });
        });

        modelBuilder.Entity<DispatchMessage>(entity =>
        {
            entity.ToTable("routarr_dispatch_messages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SenderPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SenderRole).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Body).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.AcknowledgedByPersonId).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.RequiresAcknowledgement, x.AcknowledgedAt });
            entity.HasOne(x => x.Trip)
                .WithMany(x => x.DispatchMessages)
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditPackageGenerationJob>(entity =>
        {
            entity.ToTable("routarr_audit_package_generation_jobs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Format).HasMaxLength(16).IsRequired();
            entity.Property(x => x.FilterJson).HasMaxLength(4096);
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
            entity.Property(x => x.ArtifactZip);
            entity.Property(x => x.ArtifactJson);
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
            entity.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<TenantIntegrationEventSettings>(entity =>
        {
            entity.ToTable("routarr_tenant_integration_event_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<IntegrationOutboxEvent>(entity =>
        {
            entity.ToTable("routarr_integration_outbox_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.RelatedEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PayloadJson).IsRequired();
            entity.Property(x => x.ProcessingStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ProcessingStatus, x.NextRetryAt });
        });

        ConfigureTmsRuntime(modelBuilder);
    }

    private static void ConfigureTmsRuntime(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransportationDemand>(entity =>
        {
            entity.ToTable("routarr_transportation_demands");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DemandNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceObjectType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceObjectId).HasMaxLength(128);
            entity.Property(x => x.SourceObjectNumber).HasMaxLength(128);
            entity.Property(x => x.OriginLocationRef).HasMaxLength(512).IsRequired();
            entity.Property(x => x.DestinationLocationRef).HasMaxLength(512).IsRequired();
            entity.Property(x => x.TransportMode).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ServiceLevel).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EquipmentRequirement).HasMaxLength(256).IsRequired();
            entity.Property(x => x.HandlingRequirementsJson).IsRequired();
            entity.Property(x => x.CustomerRefsJson).IsRequired();
            entity.Property(x => x.OrderRefsJson).IsRequired();
            entity.Property(x => x.SupplierRefsJson).IsRequired();
            entity.Property(x => x.RequirementRefsJson).IsRequired();
            entity.Property(x => x.PlanningStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.TenderStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RatingStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.VisibilityStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.FreshnessState).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CancelReason).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DemandNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct, x.SourceObjectId });
            entity.HasIndex(x => new { x.TenantId, x.TripId });
            entity.HasIndex(x => new { x.TenantId, x.RouteId });
        });

        modelBuilder.Entity<TransportationDemandLine>(entity =>
        {
            entity.ToTable("routarr_transportation_demand_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceProduct).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256);
            entity.Property(x => x.DescriptionSnapshot).HasMaxLength(512).IsRequired();
            entity.Property(x => x.QuantitySnapshot).HasPrecision(18, 4);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(32).IsRequired();
            entity.Property(x => x.WeightSnapshot).HasPrecision(18, 4);
            entity.Property(x => x.VolumeSnapshot).HasPrecision(18, 4);
            entity.Property(x => x.HandlingRequirementSnapshot).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TransportationDemandId, x.LineNumber }).IsUnique();
            entity.HasOne(x => x.TransportationDemand)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.TransportationDemandId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TransportationDemandRequirement>(entity =>
        {
            entity.ToTable("routarr_transportation_demand_requirements");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequirementType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceRequirementRef).HasMaxLength(256);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.EvidenceRefsJson).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TransportationDemandId, x.RequirementType, x.Status });
            entity.HasOne(x => x.TransportationDemand)
                .WithMany(x => x.Requirements)
                .HasForeignKey(x => x.TransportationDemandId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TransportationDemandSourceRef>(entity =>
        {
            entity.ToTable("routarr_transportation_demand_source_refs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceProduct).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceObjectType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceObjectId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SourceObjectNumber).HasMaxLength(128);
            entity.Property(x => x.DisplayNameSnapshot).HasMaxLength(256).IsRequired();
            entity.Property(x => x.StatusSnapshot).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FreshnessState).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TransportationDemandId });
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct, x.SourceObjectType, x.SourceObjectId });
            entity.HasOne(x => x.TransportationDemand)
                .WithMany(x => x.SourceRefs)
                .HasForeignKey(x => x.TransportationDemandId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CarrierTender>(entity =>
        {
            entity.ToTable("routarr_carrier_tenders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenderNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CarrierSupplierRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CarrierSnapshotJson).IsRequired();
            entity.Property(x => x.TenderMethod).HasMaxLength(32).IsRequired();
            entity.Property(x => x.DeclineReason).HasMaxLength(512);
            entity.Property(x => x.CounterSummary).HasMaxLength(1024);
            entity.Property(x => x.ProposedAlternative).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TenderNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.TransportationDemandId, x.Status });
        });

        modelBuilder.Entity<RoutingGuideStep>(entity =>
        {
            entity.ToTable("routarr_routing_guide_steps");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CarrierSupplierRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CarrierSnapshotJson).IsRequired();
            entity.Property(x => x.TenderMethod).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ServiceLevel).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EquipmentRequirement).HasMaxLength(256).IsRequired();
            entity.Property(x => x.LaneSnapshot).HasMaxLength(512).IsRequired();
            entity.Property(x => x.RateAgreementSnapshotRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.FallbackType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TransportationDemandId, x.Sequence }).IsUnique();
        });

        modelBuilder.Entity<FreightRating>(entity =>
        {
            entity.ToTable("routarr_freight_ratings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RatingNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.BuyRateEstimate).HasPrecision(18, 2);
            entity.Property(x => x.SellRateEstimate).HasPrecision(18, 2);
            entity.Property(x => x.PlannedFreightCost).HasPrecision(18, 2);
            entity.Property(x => x.ActualFreightCost).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.RateSourceSnapshot).HasMaxLength(512).IsRequired();
            entity.Property(x => x.FuelSurcharge).HasPrecision(18, 2);
            entity.Property(x => x.AccessorialTotal).HasPrecision(18, 2);
            entity.Property(x => x.VarianceAmount).HasPrecision(18, 2);
            entity.Property(x => x.VarianceReason).HasMaxLength(512);
            entity.Property(x => x.AllocationSnapshotJson).IsRequired();
            entity.Property(x => x.AuditStatus).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RatingNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.TransportationDemandId, x.Status });
        });

        modelBuilder.Entity<FreightAccessorial>(entity =>
        {
            entity.ToTable("routarr_freight_accessorials");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AccessorialType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceEventRef).HasMaxLength(256);
            entity.Property(x => x.EvidenceRefsJson).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.FreightRatingId });
        });

        modelBuilder.Entity<TransportationVisibilityEvent>(entity =>
        {
            entity.ToTable("routarr_transportation_visibility_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(64).IsRequired();
            entity.Property(x => x.NormalizedStatus).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Latitude).HasPrecision(10, 6);
            entity.Property(x => x.Longitude).HasPrecision(10, 6);
            entity.Property(x => x.EtaConfidence).HasMaxLength(32).IsRequired();
            entity.Property(x => x.FreshnessState).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ReviewStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RawExternalRef).HasMaxLength(256);
            entity.Property(x => x.Summary).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RawExternalRef }).IsUnique()
                .HasFilter("\"RawExternalRef\" IS NOT NULL");
            entity.HasIndex(x => new { x.TenantId, x.TransportationDemandId, x.ReceivedAt });
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.ReceivedAt });
        });

        modelBuilder.Entity<TransportationTrackingSnapshot>(entity =>
        {
            entity.ToTable("routarr_transportation_tracking_snapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CurrentStatus).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CurrentLatitude).HasPrecision(10, 6);
            entity.Property(x => x.CurrentLongitude).HasPrecision(10, 6);
            entity.Property(x => x.EtaConfidence).HasMaxLength(32).IsRequired();
            entity.Property(x => x.TrackingSource).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FreshnessState).HasMaxLength(32).IsRequired();
            entity.Property(x => x.StaleReason).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TransportationDemandId }).IsUnique()
                .HasFilter("\"TransportationDemandId\" IS NOT NULL");
            entity.HasIndex(x => new { x.TenantId, x.TripId });
        });

        modelBuilder.Entity<TransportationPlanningScenario>(entity =>
        {
            entity.ToTable("routarr_transportation_planning_scenarios");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScenarioNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Objective).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DemandRefsJson).IsRequired();
            entity.Property(x => x.RouteRefsJson).IsRequired();
            entity.Property(x => x.TripRefsJson).IsRequired();
            entity.Property(x => x.HardBlockersJson).IsRequired();
            entity.Property(x => x.WarningsJson).IsRequired();
            entity.Property(x => x.ServiceRiskEstimate).HasPrecision(18, 4);
            entity.Property(x => x.CostEstimate).HasPrecision(18, 2);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ScenarioNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
        });

        modelBuilder.Entity<TransportationPlanningSuggestion>(entity =>
        {
            entity.ToTable("routarr_transportation_planning_suggestions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SuggestionType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.HardBlockersJson).IsRequired();
            entity.Property(x => x.SoftWarningsJson).IsRequired();
            entity.Property(x => x.EstimatedCost).HasPrecision(18, 2);
            entity.Property(x => x.EstimatedMiles).HasPrecision(18, 2);
            entity.Property(x => x.EstimatedServiceRisk).HasPrecision(18, 4);
            entity.Property(x => x.AffectedDemandRefsJson).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PlanningScenarioId, x.Status });
        });

        modelBuilder.Entity<DriverCapacitySnapshot>(entity =>
        {
            entity.ToTable("routarr_driver_capacity_snapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DomicileLocationRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.FeasibilityStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.BlockerSummary).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.FreshnessState).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.SnapshotAt });
        });

        modelBuilder.Entity<TransportationYardEvent>(entity =>
        {
            entity.ToTable("routarr_transportation_yard_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TrailerAssetRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.TractorAssetRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.StaffarrYardLocationRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.StaffarrDockLocationRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.LoadedEmptyStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SealNumber).HasMaxLength(128);
            entity.Property(x => x.Source).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EvidenceRefsJson).IsRequired();
            entity.Property(x => x.DispatchImpact).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TransportationDemandId, x.OccurredAt });
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.OccurredAt });
        });

        modelBuilder.Entity<PortalCollaborationSubmission>(entity =>
        {
            entity.ToTable("routarr_portal_collaboration_submissions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExternalActorType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExternalActorRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ActionType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SubmittedDataSummary).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.UploadedRecordRefsJson).IsRequired();
            entity.Property(x => x.ReviewedByPersonId).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TransportationDemandId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.TenderId, x.Status });
        });

        modelBuilder.Entity<FreightClaim>(entity =>
        {
            entity.ToTable("routarr_freight_claims");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ClaimNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ClaimAgainstPartyType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ClaimReason).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ClaimAmount).HasPrecision(18, 2);
            entity.Property(x => x.RecoveryAmount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.EvidenceRefsJson).IsRequired();
            entity.Property(x => x.AssurarrNonconformanceRef).HasMaxLength(256);
            entity.Property(x => x.SupplyarrPerformanceImpactRef).HasMaxLength(256);
            entity.Property(x => x.OrdarrCloseoutImpactRef).HasMaxLength(256);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ClaimNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.TransportationDemandId, x.Status });
        });

        modelBuilder.Entity<TransportationDocumentPacketRequest>(entity =>
        {
            entity.ToTable("routarr_transportation_document_packet_requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PacketType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RequiredDocumentTypesJson).IsRequired();
            entity.Property(x => x.SourceFactsJson).IsRequired();
            entity.Property(x => x.RecordPackageRef).HasMaxLength(256);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TransportationDemandId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.Status });
        });

        modelBuilder.Entity<TransportationAppointmentClock>(entity =>
        {
            entity.ToTable("routarr_transportation_appointment_clocks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ClockType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TransportationDemandId, x.ClockType, x.Status });
        });

        modelBuilder.Entity<ModeSpecificRequirementRef>(entity =>
        {
            entity.ToTable("routarr_mode_specific_requirement_refs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TransportMode).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RequirementType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceRequirementRef).HasMaxLength(256);
            entity.Property(x => x.SummarySnapshot).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.DocumentRequirementRefsJson).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TransportationDemandId, x.TransportMode });
        });

        modelBuilder.Entity<TransportationFinancePacketContribution>(entity =>
        {
            entity.ToTable("routarr_transportation_finance_packet_contributions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ContributionNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ContributionType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TargetProduct).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.OperationalSummary).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.CostSnapshotJson).IsRequired();
            entity.Property(x => x.AccessorialRefsJson).IsRequired();
            entity.Property(x => x.ProofRefsJson).IsRequired();
            entity.Property(x => x.DocumentPacketRefsJson).IsRequired();
            entity.Property(x => x.ClaimRefsJson).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ContributionNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.TransportationDemandId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.TargetProduct, x.Status });
        });

        modelBuilder.Entity<RoutArrTenantSettings>(entity =>
        {
            entity.ToTable("routarr_tenant_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsRequired();
            entity.Property(x => x.CreatedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.UpdatedByPersonId).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<RoutArrTenantSettingValue>(entity =>
        {
            entity.ToTable("routarr_tenant_setting_values");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SettingGroup).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SettingKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ValueKind).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.DecimalValue).HasPrecision(18, 6);
            entity.Property(x => x.TextValue).HasMaxLength(2048);
            entity.Property(x => x.EnumValue).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SettingGroup, x.SettingKey }).IsUnique();
            entity.HasOne(x => x.TenantSettings)
                .WithMany(x => x.Values)
                .HasForeignKey(x => x.TenantSettingsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoutArrTenantSettingListItem>(entity =>
        {
            entity.ToTable("routarr_tenant_setting_list_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SettingGroup).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SettingKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ItemKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DisplayLabel).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SettingGroup, x.SettingKey, x.ItemKey }).IsUnique();
            entity.HasOne(x => x.TenantSettings)
                .WithMany(x => x.ListItems)
                .HasForeignKey(x => x.TenantSettingsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoutArrTenantSettingOverride>(entity =>
        {
            entity.ToTable("routarr_tenant_setting_overrides");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PublicKey).HasMaxLength(96).IsRequired();
            entity.Property(x => x.ScopeType).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.ScopeSourceProduct).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ScopeEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ScopeStableId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ScopeDisplayLabelSnapshot).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ScopeStatusSnapshot).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SettingGroup).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SettingKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ValueKind).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.DecimalValue).HasPrecision(18, 6);
            entity.Property(x => x.TextValue).HasMaxLength(2048);
            entity.Property(x => x.EnumValue).HasMaxLength(128);
            entity.Property(x => x.Reason).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Version).IsRequired();
            entity.Property(x => x.CreatedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.UpdatedByPersonId).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PublicKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ScopeType, x.ScopeSourceProduct, x.ScopeEntityType, x.ScopeStableId });
            entity.HasIndex(x => new { x.TenantId, x.SettingGroup, x.SettingKey });
            entity.HasOne(x => x.TenantSettings)
                .WithMany(x => x.Overrides)
                .HasForeignKey(x => x.TenantSettingsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoutArrTenantSettingOverrideListItem>(entity =>
        {
            entity.ToTable("routarr_tenant_setting_override_list_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ItemKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DisplayLabel).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.OverrideId, x.ItemKey }).IsUnique();
            entity.HasOne(x => x.Override)
                .WithMany(x => x.ListItems)
                .HasForeignKey(x => x.OverrideId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoutArrTenantSettingAuditEntry>(entity =>
        {
            entity.ToTable("routarr_tenant_setting_audit_entries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PublicKey).HasMaxLength(96).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SettingGroup).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ChangedKeys).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.ChangedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AffectedScopeType).HasMaxLength(32);
            entity.Property(x => x.AffectedScopeRef).HasMaxLength(256);
            entity.Property(x => x.Summary).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.PreviousSummary).HasMaxLength(2048);
            entity.Property(x => x.NewSummary).HasMaxLength(2048);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PublicKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SettingGroup, x.ChangedAt });
        });
    }
}
