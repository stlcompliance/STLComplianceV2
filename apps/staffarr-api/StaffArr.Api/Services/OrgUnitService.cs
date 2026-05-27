using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;

namespace StaffArr.Api.Services;

public sealed class OrgUnitService(StaffArrDbContext db)
{
    public async Task<IReadOnlyList<OrgUnitResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.OrgUnits
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.UnitType)
            .ThenBy(x => x.Name)
            .Select(x => new OrgUnitResponse(
                x.Id,
                x.UnitType,
                x.Name,
                x.ParentOrgUnitId))
            .ToListAsync(cancellationToken);
    }
}
