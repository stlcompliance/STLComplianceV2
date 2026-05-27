using Microsoft.EntityFrameworkCore;

using StaffArr.Api.Contracts;

using StaffArr.Api.Data;

using StaffArr.Api.Entities;

using STLCompliance.Shared.Contracts;



namespace StaffArr.Api.Services;



public sealed class PermissionProjectionService(

    StaffArrDbContext db,

    RoleTemplateService roleTemplateService,

    IStaffArrAuditService audit)

{

    public const string ProjectPermissionsActionScope = "staffarr.permissions.project";



    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f4");



    public async Task<EffectivePermissionProjectionResponse> GetEffectivePermissionProjectionAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var asOf = DateTimeOffset.UtcNow;
        var materialized = await TryGetMaterializedProjectionAsync(
            tenantId,
            personId,
            asOf,
            PermissionProjectionRules.DefaultReadStalenessHours,
            cancellationToken);

        if (materialized is not null)
        {
            return materialized;
        }

        return await roleTemplateService.ComputeEffectivePermissionProjectionAsync(
            tenantId,
            personId,
            cancellationToken);
    }

    public async Task<EffectivePermissionProjectionResponse?> TryGetMaterializedProjectionAsync(

        Guid tenantId,

        Guid personId,

        DateTimeOffset asOfUtc,

        int stalenessHours,

        CancellationToken cancellationToken = default)

    {

        var projection = await db.PersonPermissionProjections.AsNoTracking()

            .Include(x => x.Entries)

            .FirstOrDefaultAsync(

                x => x.TenantId == tenantId && x.PersonId == personId,

                cancellationToken);



        if (projection is null

            || PermissionProjectionRules.IsStale(projection.ComputedAt, asOfUtc, stalenessHours))

        {

            return null;

        }



        return MapProjectionResponse(projection);

    }



    public async Task<PendingPermissionProjectionsResponse> ListPendingAsync(

        Guid? tenantId,

        DateTimeOffset? asOfUtc,

        int? batchSize,

        int? stalenessHours,

        CancellationToken cancellationToken = default)

    {

        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;

        var normalizedBatchSize = PermissionProjectionRules.NormalizeBatchSize(batchSize);

        var normalizedStalenessHours = PermissionProjectionRules.NormalizeStalenessHours(stalenessHours);

        var candidates = await LoadPendingCandidatesAsync(

            tenantId,

            asOf,

            normalizedStalenessHours,

            normalizedBatchSize,

            cancellationToken);



        var items = candidates

            .Select(x => new PendingPermissionProjectionItem(

                x.PersonId,

                x.DisplayName,

                x.LastComputedAt))

            .ToList();



        return new PendingPermissionProjectionsResponse(

            asOf,

            normalizedStalenessHours,

            normalizedBatchSize,

            items);

    }



    public async Task<ProcessPermissionProjectionsResponse> ProcessBatchAsync(

        ProcessPermissionProjectionsRequest request,

        CancellationToken cancellationToken = default)

    {

        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;

        var batchSize = PermissionProjectionRules.NormalizeBatchSize(request.BatchSize);

        var stalenessHours = PermissionProjectionRules.NormalizeStalenessHours(request.StalenessHours);

        var candidates = await LoadPendingCandidatesAsync(

            request.TenantId,

            asOf,

            stalenessHours,

            batchSize,

            cancellationToken);



        var refreshed = new List<PersonPermissionProjectionSummaryResponse>();

        var skipped = new List<PermissionProjectionRefreshSkip>();



        foreach (var candidate in candidates)

        {

            try

            {

                var summary = await RefreshProjectionAsync(

                    candidate.TenantId,

                    candidate.PersonId,

                    asOf,

                    cancellationToken);

                refreshed.Add(summary);

            }

            catch (Exception ex) when (ex is not OperationCanceledException)

            {

                skipped.Add(new PermissionProjectionRefreshSkip(candidate.PersonId, ex.Message));

            }

        }



        if (refreshed.Count > 0 && request.TenantId is Guid tenantId)

        {

            await audit.WriteAsync(

                "permission_projection.refresh.batch",

                tenantId,

                WorkerActorUserId,

                "permission_projection",

                $"{refreshed.Count}",

                "Succeeded",

                cancellationToken: cancellationToken);

        }



        return new ProcessPermissionProjectionsResponse(

            asOf,

            batchSize,

            stalenessHours,

            candidates.Count,

            refreshed.Count,

            skipped.Count,

            refreshed,

            skipped);

    }



    public async Task<PersonPermissionProjectionSummaryResponse> RefreshProjectionAsync(

        Guid tenantId,

        Guid personId,

        DateTimeOffset asOfUtc,

        CancellationToken cancellationToken = default)

    {

        var computed = await roleTemplateService.ComputeEffectivePermissionProjectionAsync(

            tenantId,

            personId,

            cancellationToken);



        var existing = await db.PersonPermissionProjections

            .Include(x => x.Entries)

            .FirstOrDefaultAsync(

                x => x.TenantId == tenantId && x.PersonId == personId,

                cancellationToken);



        var now = DateTimeOffset.UtcNow;

        if (existing is null)

        {

            existing = new PersonPermissionProjection

            {

                Id = Guid.NewGuid(),

                TenantId = tenantId,

                PersonId = personId,

                CreatedAt = now

            };

            db.PersonPermissionProjections.Add(existing);

        }

        else

        {

            db.PersonPermissionProjectionEntries.RemoveRange(existing.Entries);

            existing.Entries.Clear();

        }



        existing.PermissionCount = computed.Permissions.Count;

        existing.ComputedAt = asOfUtc;

        existing.UpdatedAt = now;



        foreach (var permission in computed.Permissions)

        {

            existing.Entries.Add(new PersonPermissionProjectionEntry

            {

                Id = Guid.NewGuid(),

                TenantId = tenantId,

                PersonId = personId,

                ProjectionId = existing.Id,

                PermissionKey = permission.PermissionKey,

                PermissionName = permission.PermissionName,

                ScopeType = permission.ScopeType,

                ScopeValue = permission.ScopeValue

            });

        }



        await db.SaveChangesAsync(cancellationToken);



        return new PersonPermissionProjectionSummaryResponse(

            personId,

            existing.PermissionCount,

            existing.ComputedAt,

            computed.Permissions);

    }



    private async Task<IReadOnlyList<PendingProjectionCandidate>> LoadPendingCandidatesAsync(

        Guid? tenantId,

        DateTimeOffset asOfUtc,

        int stalenessHours,

        int batchSize,

        CancellationToken cancellationToken)

    {

        var peopleQuery = db.People.AsNoTracking()

            .Where(x => x.EmploymentStatus == "active");



        if (tenantId is Guid scopedTenantId)

        {

            peopleQuery = peopleQuery.Where(x => x.TenantId == scopedTenantId);

        }



        var people = await peopleQuery

            .OrderBy(x => x.TenantId)

            .ThenBy(x => x.DisplayName)

            .ToListAsync(cancellationToken);



        var projectionLookup = await db.PersonPermissionProjections.AsNoTracking()

            .Where(x => tenantId == null || x.TenantId == tenantId)

            .ToDictionaryAsync(

                x => (x.TenantId, x.PersonId),

                x => x,

                cancellationToken);



        var pending = new List<PendingProjectionCandidate>();

        foreach (var person in people)

        {

            projectionLookup.TryGetValue((person.TenantId, person.Id), out var projection);

            if (!PermissionProjectionRules.IsStale(projection?.ComputedAt, asOfUtc, stalenessHours))

            {

                continue;

            }



            pending.Add(new PendingProjectionCandidate(

                person.TenantId,

                person.Id,

                person.DisplayName,

                projection?.ComputedAt));

        }



        return pending

            .OrderBy(x => x.LastComputedAt.HasValue ? 1 : 0)

            .ThenBy(x => x.LastComputedAt)

            .Take(batchSize)

            .ToList();

    }



    private static EffectivePermissionProjectionResponse MapProjectionResponse(PersonPermissionProjection projection)

    {

        var permissions = projection.Entries

            .GroupBy(x => PermissionProjectionRules.BuildPermissionIdentity(

                x.PermissionKey,

                x.ScopeType,

                x.ScopeValue))

            .OrderBy(x => x.Key)

            .Select(group =>

            {

                var first = group.First();

                return new EffectivePermissionResponse(

                    first.PermissionKey,

                    first.PermissionName,

                    first.ScopeType,

                    first.ScopeValue,

                    []);

            })

            .ToList();



        return new EffectivePermissionProjectionResponse(

            projection.PersonId,

            projection.ComputedAt,

            permissions);

    }



    private sealed record PendingProjectionCandidate(

        Guid TenantId,

        Guid PersonId,

        string DisplayName,

        DateTimeOffset? LastComputedAt);

}


