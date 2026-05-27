using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Data;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class FieldInboxService(StaffArrDbContext db)
{
    public async Task<FieldInboxResponse> GetAsync(
        Guid tenantId,
        Guid? personId,
        CancellationToken cancellationToken = default)
    {
        var query = db.PersonnelIncidents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == "open");

        if (personId is Guid filterPersonId)
        {
            query = query.Where(x => x.PersonId == filterPersonId);
        }

        var incidents = await query
            .OrderByDescending(x => x.ReportedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        var items = incidents.Select(incident => new FieldInboxTaskItem(
            $"staffarr:incident:{incident.Id:D}",
            "staffarr",
            "incident_acknowledgement",
            incident.Title,
            incident.ReasonCategoryKey,
            incident.Status,
            incident.Severity,
            incident.OccurredAt,
            incident.ReportedAt,
            $"/incidents/{incident.Id:D}")).ToList();

        return FieldInboxRules.BuildProductResponse(items);
    }
}
