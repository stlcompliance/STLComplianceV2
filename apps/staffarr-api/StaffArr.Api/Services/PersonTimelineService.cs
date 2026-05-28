using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class PersonTimelineService(StaffArrDbContext db)
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<PersonTimelineEntryResponse>> ListPersonTimelineAsync(
        Guid tenantId,
        Guid personId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var personExists = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.Id == personId,
            cancellationToken);
        if (!personExists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        page = page < 1 ? 1 : page;
        pageSize = pageSize switch
        {
            < 1 => 50,
            > MaxPageSize => MaxPageSize,
            _ => pageSize
        };

        var entries = await PersonTimelineBuilder.BuildTimelineEntriesAsync(
            db,
            tenantId,
            personId,
            cancellationToken);
        var total = entries.Count;
        var items = entries
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.EntryId, StringComparer.Ordinal)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<PersonTimelineEntryResponse>(
            items,
            page,
            pageSize,
            total,
            page * pageSize < total);
    }
}
