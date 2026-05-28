using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class TrainarrPersonTrainingHistoryService(
    StaffArrDbContext db,
    TrainArrPersonTrainingHistoryClient trainArrClient,
    IStaffArrAuditService audit)
{
    public const string ReadAction = "staffarr.trainarr_training_history.read";

    public async Task<TrainarrPersonTrainingHistoryResponse> GetForPersonAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);

        var payload = await trainArrClient.GetForPersonAsync(tenantId, personId, limit, cancellationToken);

        await audit.WriteAsync(
            ReadAction,
            tenantId,
            actorUserId,
            "trainarr_person_training_history",
            personId.ToString(),
            payload.TotalCount.ToString(),
            cancellationToken: cancellationToken);

        return new TrainarrPersonTrainingHistoryResponse(
            personId,
            SourceProduct: "trainarr",
            SourceNote: "Read-through from TrainArr materialized person training history (not StaffArr-owned truth).",
            payload.TotalCount,
            payload.Items.Select(x => new TrainarrPersonTrainingHistoryEntryItem(
                x.EntryId,
                x.EventKind,
                x.Summary,
                x.RelatedEntityType,
                x.RelatedEntityId,
                x.OccurredAt)).ToList());
    }

    private async Task EnsurePersonExistsAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        var exists = await db.People
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.Id == personId, cancellationToken);
        if (!exists)
        {
            throw new StlApiException("people.person_not_found", "Person was not found.", 404);
        }
    }
}
