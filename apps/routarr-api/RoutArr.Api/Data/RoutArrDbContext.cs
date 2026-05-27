using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Data;

namespace RoutArr.Api.Data;

public sealed class RoutArrDbContext(DbContextOptions<RoutArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<Trip> Trips => Set<Trip>();

    public DbSet<TripLoad> TripLoads => Set<TripLoad>();

    public DbSet<DispatchRoute> Routes => Set<DispatchRoute>();

    public DbSet<RouteStop> RouteStops => Set<RouteStop>();

    public DbSet<DriverAvailability> DriverAvailabilities => Set<DriverAvailability>();

    public DbSet<EquipmentAvailability> EquipmentAvailabilities => Set<EquipmentAvailability>();

    public DbSet<RoutArrAuditEvent> AuditEvents => Set<RoutArrAuditEvent>();

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
    }
}
