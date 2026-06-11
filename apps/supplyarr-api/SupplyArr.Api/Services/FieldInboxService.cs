using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class FieldInboxService
{
    public Task<FieldInboxResponse> GetAsync(
        Guid tenantId,
        Guid actorUserId,
        bool viewAll,
        CancellationToken cancellationToken = default)
    {
        _ = tenantId;
        _ = actorUserId;
        _ = viewAll;
        _ = cancellationToken;

        // LoadArr owns receiving execution and therefore owns field/mobile receiving tasks.
        // SupplyArr keeps the endpoint for compatibility, but it no longer emits receiving work.
        return Task.FromResult(FieldInboxRules.BuildProductResponse([]));
    }
}
