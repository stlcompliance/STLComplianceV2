using Microsoft.EntityFrameworkCore;

using TrainArr.Api.Contracts;

using TrainArr.Api.Data;

using TrainArr.Api.Entities;



namespace TrainArr.Api.Services;



public sealed class OrphanReferenceSettingsService(

    TrainArrDbContext db,

    ITrainArrAuditService audit)

{

    public async Task<OrphanReferenceSettingsResponse> GetAsync(

        Guid tenantId,

        CancellationToken cancellationToken = default)

    {

        var settings = await db.TenantOrphanReferenceSettings

            .AsNoTracking()

            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);



        return settings is null

            ? new OrphanReferenceSettingsResponse(

                false,

                OrphanReferenceRules.DefaultStalenessHours,

                null)

            : Map(settings);

    }



    public async Task<OrphanReferenceSettingsResponse> UpsertAsync(

        Guid tenantId,

        Guid actorUserId,

        UpsertOrphanReferenceSettingsRequest request,

        CancellationToken cancellationToken = default)

    {

        var now = DateTimeOffset.UtcNow;

        var entity = await db.TenantOrphanReferenceSettings

            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);



        if (entity is null)

        {

            entity = new TenantOrphanReferenceSettings

            {

                Id = Guid.NewGuid(),

                TenantId = tenantId,

                CreatedAt = now,

            };

            db.TenantOrphanReferenceSettings.Add(entity);

        }



        entity.IsEnabled = request.IsEnabled;

        entity.ScanStalenessHours = OrphanReferenceRules.NormalizeStalenessHours(request.ScanStalenessHours);

        entity.UpdatedByUserId = actorUserId;

        entity.UpdatedAt = now;



        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "orphan_reference_settings.upsert",

            tenantId,

            actorUserId,

            "orphan_reference_settings",

            tenantId.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return Map(entity);

    }



    internal async Task<TenantOrphanReferenceSettingsSnapshot?> LoadSnapshotAsync(

        Guid tenantId,

        CancellationToken cancellationToken = default)

    {

        var settings = await db.TenantOrphanReferenceSettings

            .AsNoTracking()

            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);



        return settings is null ? null : ToSnapshot(settings);

    }



    internal static TenantOrphanReferenceSettingsSnapshot ToSnapshot(

        TenantOrphanReferenceSettings settings) =>

        new(settings.IsEnabled, settings.ScanStalenessHours);



    private static OrphanReferenceSettingsResponse Map(TenantOrphanReferenceSettings settings) =>

        new(settings.IsEnabled, settings.ScanStalenessHours, settings.UpdatedAt);

}



public sealed record TenantOrphanReferenceSettingsSnapshot(

    bool IsEnabled,

    int ScanStalenessHours);


