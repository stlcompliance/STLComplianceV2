using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;

namespace StaffArr.Api.Services;

public sealed class StaffArrMaintainArrTechnicianRefSyncService(
    MaintainArrTechnicianRefSyncClient maintainarrClient,
    StaffArrDbContext db)
{
    public async Task TryPublishPersonChangedAsync(
        Guid tenantId,
        Guid personId,
        string eventType,
        CancellationToken cancellationToken = default)
    {
        if (!maintainarrClient.IsConfigured)
        {
            return;
        }

        var person = await db.People
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == personId)
            .Select(x => new
            {
                x.DisplayName,
                x.EmploymentStatus,
                x.PrimaryOrgUnitId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (person is null)
        {
            return;
        }

        string? primarySite = null;
        if (person.PrimaryOrgUnitId is Guid orgUnitId)
        {
            primarySite = await db.OrgUnits
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Id == orgUnitId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var occurredAt = DateTimeOffset.UtcNow;
        try
        {
            await maintainarrClient.TrySyncPersonAsync(
                new MaintainArrIngestStaffarrPersonSyncPayload(
                    tenantId,
                    personId,
                    person.DisplayName,
                    person.EmploymentStatus,
                    primarySite,
                    eventType,
                    occurredAt,
                    $"{eventType}:{personId:D}:{occurredAt:O}"),
                cancellationToken);
        }
        catch
        {
            // Best-effort cross-product mirror refresh; person workflow must not fail.
        }
    }

    public Task TryPublishPersonChangedAsync(
        StaffPerson person,
        string eventType,
        CancellationToken cancellationToken = default) =>
        TryPublishPersonChangedAsync(person.TenantId, person.Id, eventType, cancellationToken);
}
