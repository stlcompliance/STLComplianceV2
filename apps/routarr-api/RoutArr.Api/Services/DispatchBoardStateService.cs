using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class DispatchBoardStateService(
    RoutArrDbContext db,
    RoutArrAuthorizationService authorization,
    IRoutArrAuditService audit)
{
    public async Task<DispatchBoardStateResponse> GetAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDispatchBoardRead(principal);
        var tenantId = principal.GetTenantId();
        var entity = await LoadOrCreateAsync(tenantId, cancellationToken);
        return Map(entity);
    }

    public async Task<DispatchBoardStateResponse> UpsertAsync(
        ClaimsPrincipal principal,
        UpsertDispatchBoardStateRequest request,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireTripsAssign(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var scope = NormalizeScope(request.DefaultScope);

        var entity = await db.TenantDispatchBoardStates
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            entity = new TenantDispatchBoardState
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
            };
            db.TenantDispatchBoardStates.Add(entity);
        }

        entity.DefaultScope = scope;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedByUserId = actorUserId;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "dispatch_board_state.update",
            tenantId,
            actorUserId,
            "dispatch_board_state",
            entity.Id.ToString(),
            scope,
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    internal async Task<TenantDispatchBoardState> LoadOrCreateAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantDispatchBoardStates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return entity ?? new TenantDispatchBoardState
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DefaultScope = DispatchBoardScopes.Daily,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    private static string NormalizeScope(string? scope)
    {
        var normalized = scope?.Trim().ToLowerInvariant() ?? DispatchBoardScopes.Daily;
        if (!DispatchBoardScopes.All.Contains(normalized))
        {
            throw new StlApiException(
                "dispatch_board.invalid_scope",
                "Board scope must be daily or weekly.",
                400);
        }

        return normalized;
    }

    private static DispatchBoardStateResponse Map(TenantDispatchBoardState entity) =>
        new(entity.DefaultScope, entity.UpdatedAt, entity.UpdatedByUserId);
}
