using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;

namespace TrainArr.Api.Services;

public sealed class PersonTrainingHistoryService(TrainArrDbContext db)
{
    public const string IntegrationReadActionScope = "trainarr.person_training_history.read";

    public async Task<PersonTrainingHistoryResponse> GetForPersonAsync(
        Guid tenantId,
        Guid staffarrPersonId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = EventProcessingRules.NormalizeHistoryListLimit(limit);
        var query = db.PersonTrainingHistoryEntries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.StaffarrPersonId == staffarrPersonId);

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PersonTrainingHistoryResponse(
            staffarrPersonId,
            totalCount,
            rows.Select(x => new PersonTrainingHistoryEntryItem(
                x.Id,
                x.EventKind,
                x.Summary,
                x.RelatedEntityType,
                x.RelatedEntityId,
                x.OccurredAt)).ToList());
    }
}
