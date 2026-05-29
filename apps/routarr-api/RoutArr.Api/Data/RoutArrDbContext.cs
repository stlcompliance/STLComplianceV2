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

    public DbSet<DispatchRoute> Routes => Set<DispatchRoute>();

    public DbSet<RouteStop> RouteStops => Set<RouteStop>();

    public DbSet<DriverAvailability> DriverAvailabilities => Set<DriverAvailability>();

    public DbSet<EquipmentAvailability> EquipmentAvailabilities => Set<EquipmentAvailability>();

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
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TripNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.DispatchStatus, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.AssignedDriverPersonId });
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

        modelBuilder.Entity<RouteStop>(entity =>
        {
            entity.ToTable("routarr_route_stops");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StopKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(256).IsRequired();
            entity.Property(x => x.AddressLabel).HasMaxLength(512).IsRequired();
            entity.Property(x => x.StopType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.StopStatus).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RouteId });
            entity.HasIndex(x => new { x.TenantId, x.RouteId, x.StopKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RouteId, x.SequenceNumber }).IsUnique();
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
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TripId });
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.CapturedAt });
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
            entity.Property(x => x.SubmittedByPersonId).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TripId });
            entity.HasIndex(x => new { x.TenantId, x.TripId, x.Phase }).IsUnique();
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
            entity.Property(x => x.ResolutionTemplateKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ResolutionNotes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ExceptionKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.TripId });
            entity.HasIndex(x => new { x.TenantId, x.AssignedToUserId });
            entity.HasIndex(x => new { x.TenantId, x.SlaDueAt });
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
    }
}
