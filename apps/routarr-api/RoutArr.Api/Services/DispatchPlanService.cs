using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class DispatchPlanService(
    RoutArrDbContext db,
    IRoutArrAuditService audit)
{
    public const string CreateAction = "dispatch_plan.create";
    public const string ReadAction = "dispatch_plan.read";

    public async Task<IReadOnlyList<DispatchPlanSummaryResponse>> ListAsync(
        Guid tenantId,
        bool viewAll,
        string? actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var query = db.DispatchPlans
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!viewAll && !string.IsNullOrWhiteSpace(actorPersonId))
        {
            var personId = actorPersonId.Trim();
            query = query.Where(x =>
                x.CreatedByPersonId == personId
                || x.PlannerPersonId == personId
                || x.DispatcherPersonId == personId);
        }

        var items = await query
            .OrderByDescending(x => x.DispatchDate)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return items.Select(MapSummary).ToList();
    }

    public async Task<DispatchPlanDetailResponse> GetAsync(
        Guid tenantId,
        Guid dispatchPlanId,
        CancellationToken cancellationToken = default)
    {
        var entity = await RequireAsync(tenantId, dispatchPlanId, cancellationToken);
        return MapDetail(entity);
    }

    public async Task<DispatchPlanDetailResponse> CreateAsync(
        ClaimsPrincipal principal,
        CreateDispatchPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var actorPersonId = principal.GetPersonId().ToString();

        Validate(request);

        var now = DateTimeOffset.UtcNow;
        var hasExplicitPlanningRefs = (request.RouteRefs?.Count ?? 0) > 0 || (request.TripRefs?.Count ?? 0) > 0;
        var entity = new DispatchPlan
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DispatchNumber = await GenerateDispatchNumberAsync(tenantId, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            DispatchDate = request.DispatchDate,
            DispatchType = NormalizeType(request.DispatchType),
            Status = hasExplicitPlanningRefs ? DispatchPlanStatuses.Planning : DispatchPlanStatuses.Draft,
            PlannerPersonId = NormalizeOptionalPersonId(request.PlannerPersonId),
            DispatcherPersonId = NormalizeOptionalPersonId(request.DispatcherPersonId),
            StaffarrSiteId = request.StaffarrSiteId,
            RouteRefsJson = SerializeRefs(request.RouteRefs),
            TripRefsJson = SerializeRefs(request.TripRefs),
            BlockerRefsJson = SerializeRefs(request.BlockerRefs),
            Notes = request.Notes?.Trim() ?? string.Empty,
            CreatedByPersonId = actorPersonId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.DispatchPlans.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            CreateAction,
            tenantId,
            actorUserId,
            "dispatch_plan",
            entity.Id.ToString(),
            entity.DispatchNumber,
            cancellationToken: cancellationToken);

        return MapDetail(entity);
    }

    private async Task<DispatchPlan> RequireAsync(
        Guid tenantId,
        Guid dispatchPlanId,
        CancellationToken cancellationToken)
    {
        var entity = await db.DispatchPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == dispatchPlanId, cancellationToken);

        if (entity is null)
        {
            throw new StlApiException("dispatch_plan.not_found", "Dispatch plan was not found.", 404);
        }

        return entity;
    }

    private static void Validate(CreateDispatchPlanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new StlApiException("dispatch_plan.title_required", "Dispatch plan title is required.", 400);
        }

        if (!DispatchPlanTypes.All.Contains(request.DispatchType?.Trim() ?? string.Empty))
        {
            throw new StlApiException(
                "dispatch_plan.invalid_type",
                "Dispatch type is not valid.",
                400);
        }
    }

    private static string NormalizeType(string dispatchType) =>
        dispatchType.Trim().ToLowerInvariant();

    private static string? NormalizeOptionalPersonId(string? personId) =>
        string.IsNullOrWhiteSpace(personId) ? null : personId.Trim();

    private async Task<string> GenerateDispatchNumberAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var datePart = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            var candidate = $"DP-{datePart}-{suffix}";
            var exists = await db.DispatchPlans.AnyAsync(
                x => x.TenantId == tenantId && x.DispatchNumber == candidate,
                cancellationToken);
            if (!exists)
            {
                return candidate;
            }
        }

        return $"DP-{datePart}-{Guid.NewGuid():N}".ToUpperInvariant();
    }

    private static string SerializeRefs(IReadOnlyList<string>? refs) =>
        JsonSerializer.Serialize(
            (refs ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList());

    private static IReadOnlyList<string> DeserializeRefs(string refsJson)
    {
        if (string.IsNullOrWhiteSpace(refsJson))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(refsJson) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static DispatchPlanSummaryResponse MapSummary(DispatchPlan entity) =>
        new(
            entity.Id,
            entity.DispatchNumber,
            entity.Title,
            entity.Description,
            entity.DispatchDate,
            entity.DispatchType,
            entity.Status,
            entity.PlannerPersonId,
            entity.DispatcherPersonId,
            entity.StaffarrSiteId,
            DeserializeRefs(entity.RouteRefsJson),
            DeserializeRefs(entity.TripRefsJson),
            DeserializeRefs(entity.BlockerRefsJson),
            entity.Notes,
            entity.CreatedByPersonId,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ReleasedAt,
            entity.ReleasedByPersonId,
            entity.CompletedAt,
            entity.CanceledAt,
            entity.CancelReason);

    private static DispatchPlanDetailResponse MapDetail(DispatchPlan entity) =>
        new(
            entity.Id,
            entity.DispatchNumber,
            entity.Title,
            entity.Description,
            entity.DispatchDate,
            entity.DispatchType,
            entity.Status,
            entity.PlannerPersonId,
            entity.DispatcherPersonId,
            entity.StaffarrSiteId,
            DeserializeRefs(entity.RouteRefsJson),
            DeserializeRefs(entity.TripRefsJson),
            DeserializeRefs(entity.BlockerRefsJson),
            entity.Notes,
            entity.CreatedByPersonId,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ReleasedAt,
            entity.ReleasedByPersonId,
            entity.CompletedAt,
            entity.CanceledAt,
            entity.CancelReason);
}
