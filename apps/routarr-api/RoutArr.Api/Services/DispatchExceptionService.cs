using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class DispatchExceptionService(
    RoutArrDbContext db,
    RoutArrAuthorizationService authorization,
    IRoutArrAuditService audit,
    IntegrationOutboxEnqueueService integrationOutbox)
{
    public const string ListAction = "dispatch_exception.list";
    public const string CreateAction = "dispatch_exception.create";
    public const string AssignAction = "dispatch_exception.assign";
    public const string ResolveAction = "dispatch_exception.resolve";
    public const string LinkTripAction = "dispatch_exception.link_trip";
    public const string BulkAssignAction = "dispatch_exception.bulk_assign";
    public const string BulkResolveAction = "dispatch_exception.bulk_resolve";
    public const string IncidentCreateAction = "routarr.incident.created";

    public IReadOnlyList<DispatchExceptionResolutionTemplateResponse> ListResolutionTemplates() =>
        DispatchExceptionResolutionTemplates.All
            .Select(x => new DispatchExceptionResolutionTemplateResponse(
                x.TemplateKey,
                x.Label,
                x.DefaultResolutionNotes))
            .ToList();

    public async Task<DispatchExceptionListResponse> ListOpenAsync(
        ClaimsPrincipal principal,
        string? statusFilter,
        bool overdueOnly,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDispatchExceptionRead(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var viewAll = authorization.CanViewAllTrips(principal);
        var asOf = DateTimeOffset.UtcNow;

        var statuses = ResolveStatusFilter(statusFilter);
        var query = db.DispatchExceptions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && statuses.Contains(x.Status));

        if (!viewAll)
        {
            query = query.Where(x =>
                x.CreatedByUserId == actorUserId
                || x.AssignedToUserId == actorUserId);
        }

        if (overdueOnly)
        {
            query = query.Where(x =>
                x.SlaDueAt != null
                && x.SlaDueAt < asOf
                && DispatchExceptionStatuses.OpenQueue.Contains(x.Status));
        }

        var entities = await (overdueOnly
                ? query.OrderBy(x => x.SlaDueAt).ThenByDescending(x => x.UpdatedAt)
                : query.OrderByDescending(x => x.UpdatedAt))
            .Take(200)
            .ToListAsync(cancellationToken);

        var tripIds = entities
            .Where(x => x.TripId.HasValue)
            .Select(x => x.TripId!.Value)
            .Distinct()
            .ToList();

        var trips = tripIds.Count == 0
            ? new Dictionary<Guid, Trip>()
            : await db.Trips
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && tripIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        var openCount = await db.DispatchExceptions
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId && DispatchExceptionStatuses.OpenQueue.Contains(x.Status),
                cancellationToken);

        var overdueCount = await db.DispatchExceptions
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId
                     && x.SlaDueAt != null
                     && x.SlaDueAt < asOf
                     && DispatchExceptionStatuses.OpenQueue.Contains(x.Status),
                cancellationToken);

        await audit.WriteAsync(
            ListAction,
            tenantId,
            actorUserId,
            "dispatch_exception_queue",
            overdueOnly ? "overdue" : statusFilter ?? "open",
            entities.Count.ToString(),
            cancellationToken: cancellationToken);

        return new DispatchExceptionListResponse(
            entities.Count,
            openCount,
            overdueCount,
            entities.Select(x =>
            {
                Trip? trip = null;
                if (x.TripId.HasValue)
                {
                    trips.TryGetValue(x.TripId.Value, out trip);
                }

                return MapSummary(x, trip, asOf);
            }).ToList());
    }

    public async Task<DispatchExceptionSummaryResponse> CreateAsync(
        ClaimsPrincipal principal,
        CreateDispatchExceptionRequest request,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDispatchExceptionTriage(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();

        ValidateTitle(request.Title);
        var category = NormalizeCategory(request.Category);
        Trip? trip = null;
        if (request.TripId.HasValue)
        {
            trip = await RequireTripAsync(tenantId, request.TripId.Value, cancellationToken);
            authorization.RequireTripAccess(
                principal,
                trip.CreatedByUserId,
                trip.AssignedDriverPersonId);
        }

        if (request.AssignedToUserId is { } assignee)
        {
            DispatchExceptionRules.EnsureAssignee(assignee);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new DispatchException
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExceptionKey = await GenerateExceptionKeyAsync(tenantId, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Category = category,
            IncidentType = DispatchIncidentTypes.OperationalException,
            IncidentSeverity = DispatchIncidentSeverities.Medium,
            IncidentReviewStatus = DispatchIncidentReviewStatuses.Open,
            IncidentRoutedProduct = DispatchIncidentRoutedProducts.RoutArr,
            Status = request.AssignedToUserId.HasValue
                ? DispatchExceptionStatuses.Assigned
                : DispatchExceptionStatuses.Open,
            TripId = trip?.Id,
            AssignedToUserId = request.AssignedToUserId,
            AssignedAt = request.AssignedToUserId.HasValue ? now : null,
            SlaDueAt = DispatchExceptionRules.NormalizeSlaDueAt(request.SlaDueAt, category, now),
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.DispatchExceptions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            CreateAction,
            tenantId,
            actorUserId,
            "dispatch_exception",
            entity.Id.ToString(),
            entity.ExceptionKey,
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueExceptionCreatedAsync(entity, trip, cancellationToken);
        await integrationOutbox.TryEnqueueIncidentCreatedAsync(entity, trip, cancellationToken);
        await integrationOutbox.TryEnqueueComplianceHoldCreatedAsync(entity, trip, cancellationToken);

        return MapSummary(entity, trip);
    }

    public async Task<DispatchExceptionSummaryResponse> CreateIncidentAsync(
        ClaimsPrincipal principal,
        CreateDispatchIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDispatchExceptionTriage(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();

        ValidateTitle(request.Title);
        var incidentType = NormalizeIncidentType(request.IncidentType);
        var severity = NormalizeIncidentSeverity(request.IncidentSeverity);
        var routedProduct = NormalizeIncidentRoutedProduct(request.RoutedProduct, incidentType);
        var category = CategoryForIncidentType(incidentType);

        Trip? trip = null;
        if (request.TripId.HasValue)
        {
            trip = await RequireTripAsync(tenantId, request.TripId.Value, cancellationToken);
            authorization.RequireTripAccess(
                principal,
                trip.CreatedByUserId,
                trip.AssignedDriverPersonId);
        }

        if (request.AssignedToUserId is { } assignee)
        {
            DispatchExceptionRules.EnsureAssignee(assignee);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new DispatchException
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExceptionKey = await GenerateExceptionKeyAsync(tenantId, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Category = category,
            IncidentType = incidentType,
            IncidentSeverity = severity,
            IncidentReviewStatus = string.Equals(routedProduct, DispatchIncidentRoutedProducts.RoutArr, StringComparison.OrdinalIgnoreCase)
                ? DispatchIncidentReviewStatuses.Open
                : DispatchIncidentReviewStatuses.Routed,
            IncidentRoutedProduct = routedProduct,
            Status = request.AssignedToUserId.HasValue
                ? DispatchExceptionStatuses.Assigned
                : DispatchExceptionStatuses.Open,
            TripId = trip?.Id,
            AssignedToUserId = request.AssignedToUserId,
            AssignedAt = request.AssignedToUserId.HasValue ? now : null,
            SlaDueAt = DispatchExceptionRules.NormalizeSlaDueAt(request.SlaDueAt, category, now),
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.DispatchExceptions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            IncidentCreateAction,
            tenantId,
            actorUserId,
            "dispatch_incident",
            entity.Id.ToString(),
            $"{incidentType}:{severity}:{routedProduct}",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueIncidentCreatedAsync(entity, trip, cancellationToken);
        await integrationOutbox.TryEnqueueExceptionCreatedAsync(entity, trip, cancellationToken);
        await integrationOutbox.TryEnqueueComplianceHoldCreatedAsync(entity, trip, cancellationToken);

        return MapSummary(entity, trip);
    }

    public async Task<DispatchExceptionSummaryResponse> AssignAsync(
        ClaimsPrincipal principal,
        Guid exceptionId,
        AssignDispatchExceptionRequest request,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDispatchExceptionTriage(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();

        DispatchExceptionRules.EnsureAssignee(request.AssignedToUserId);

        var entity = await RequireExceptionAsync(tenantId, exceptionId, cancellationToken);
        EnsureNotTerminal(entity);

        entity.Status = DispatchExceptionStatuses.Assigned;
        entity.AssignedToUserId = request.AssignedToUserId;
        entity.AssignedAt = DateTimeOffset.UtcNow;
        if (request.SlaDueAt is not null)
        {
            entity.SlaDueAt = request.SlaDueAt;
        }

        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            AssignAction,
            tenantId,
            actorUserId,
            "dispatch_exception",
            entity.Id.ToString(),
            request.AssignedToUserId.ToString(),
            cancellationToken: cancellationToken);

        return await MapWithTripAsync(tenantId, entity, cancellationToken);
    }

    public async Task<DispatchExceptionSummaryResponse> ResolveAsync(
        ClaimsPrincipal principal,
        Guid exceptionId,
        ResolveDispatchExceptionRequest request,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDispatchExceptionTriage(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();

        var entity = await RequireExceptionAsync(tenantId, exceptionId, cancellationToken);
        EnsureNotTerminal(entity);

        var templateKey = DispatchExceptionRules.NormalizeResolutionTemplateKey(request.ResolutionTemplateKey);
        entity.Status = DispatchExceptionStatuses.Resolved;
        entity.IncidentReviewStatus = DispatchIncidentReviewStatuses.Closed;
        entity.ResolutionTemplateKey = templateKey;
        entity.ResolutionNotes = DispatchExceptionRules.BuildResolutionNotes(
            string.IsNullOrWhiteSpace(templateKey) ? null : templateKey,
            request.ResolutionNotes);
        entity.ResolvedByUserId = actorUserId;
        entity.ResolvedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            ResolveAction,
            tenantId,
            actorUserId,
            "dispatch_exception",
            entity.Id.ToString(),
            string.IsNullOrWhiteSpace(templateKey) ? "resolved" : templateKey,
            cancellationToken: cancellationToken);

        var trip = entity.TripId.HasValue
            ? await db.Trips
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == entity.TripId.Value, cancellationToken)
            : null;

        await integrationOutbox.TryEnqueueExceptionResolvedAsync(entity, trip, cancellationToken);
        await integrationOutbox.TryEnqueueComplianceHoldReleasedAsync(entity, trip, cancellationToken);

        return await MapWithTripAsync(tenantId, entity, cancellationToken);
    }

    public async Task<DispatchExceptionSummaryResponse> LinkTripAsync(
        ClaimsPrincipal principal,
        Guid exceptionId,
        LinkDispatchExceptionTripRequest request,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDispatchExceptionTriage(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();

        if (request.TripId == Guid.Empty)
        {
            throw new StlApiException(
                "dispatch_exception.trip_required",
                "Trip id is required.",
                400);
        }

        var entity = await RequireExceptionAsync(tenantId, exceptionId, cancellationToken);
        EnsureNotTerminal(entity);

        var trip = await RequireTripAsync(tenantId, request.TripId, cancellationToken);
        authorization.RequireTripAccess(
            principal,
            trip.CreatedByUserId,
            trip.AssignedDriverPersonId);

        entity.TripId = trip.Id;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            LinkTripAction,
            tenantId,
            actorUserId,
            "dispatch_exception",
            entity.Id.ToString(),
            trip.TripNumber,
            cancellationToken: cancellationToken);

        return MapSummary(entity, trip);
    }

    public async Task<BulkDispatchExceptionActionResponse> BulkAssignAsync(
        ClaimsPrincipal principal,
        BulkAssignDispatchExceptionsRequest request,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDispatchExceptionTriage(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        DispatchExceptionRules.EnsureAssignee(request.AssignedToUserId);
        var exceptionIds = DispatchExceptionRules.ValidateBulkExceptionIds(request.ExceptionIds);

        var results = new List<BulkDispatchExceptionActionResult>();
        foreach (var exceptionId in exceptionIds)
        {
            try
            {
                var mapped = await AssignAsync(
                    principal,
                    exceptionId,
                    new AssignDispatchExceptionRequest(request.AssignedToUserId, request.SlaDueAt),
                    cancellationToken);
                results.Add(new BulkDispatchExceptionActionResult(
                    exceptionId,
                    true,
                    null,
                    null,
                    mapped));
            }
            catch (StlApiException ex)
            {
                results.Add(new BulkDispatchExceptionActionResult(
                    exceptionId,
                    false,
                    ex.Code,
                    ex.Message,
                    null));
            }
        }

        var successCount = results.Count(x => x.Success);
        await audit.WriteAsync(
            BulkAssignAction,
            tenantId,
            actorUserId,
            "dispatch_exception_bulk",
            request.AssignedToUserId.ToString(),
            $"{successCount}/{results.Count}",
            cancellationToken: cancellationToken);

        return new BulkDispatchExceptionActionResponse(
            results.Count,
            successCount,
            results.Count - successCount,
            results);
    }

    public async Task<BulkDispatchExceptionActionResponse> BulkResolveAsync(
        ClaimsPrincipal principal,
        BulkResolveDispatchExceptionsRequest request,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDispatchExceptionTriage(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var exceptionIds = DispatchExceptionRules.ValidateBulkExceptionIds(request.ExceptionIds);
        DispatchExceptionRules.NormalizeResolutionTemplateKey(request.ResolutionTemplateKey);

        var results = new List<BulkDispatchExceptionActionResult>();
        foreach (var exceptionId in exceptionIds)
        {
            try
            {
                var mapped = await ResolveAsync(
                    principal,
                    exceptionId,
                    new ResolveDispatchExceptionRequest(
                        request.ResolutionNotes,
                        request.ResolutionTemplateKey),
                    cancellationToken);
                results.Add(new BulkDispatchExceptionActionResult(
                    exceptionId,
                    true,
                    null,
                    null,
                    mapped));
            }
            catch (StlApiException ex)
            {
                results.Add(new BulkDispatchExceptionActionResult(
                    exceptionId,
                    false,
                    ex.Code,
                    ex.Message,
                    null));
            }
        }

        var successCount = results.Count(x => x.Success);
        await audit.WriteAsync(
            BulkResolveAction,
            tenantId,
            actorUserId,
            "dispatch_exception_bulk",
            request.ResolutionTemplateKey ?? "manual",
            $"{successCount}/{results.Count}",
            cancellationToken: cancellationToken);

        return new BulkDispatchExceptionActionResponse(
            results.Count,
            successCount,
            results.Count - successCount,
            results);
    }

    private static IReadOnlySet<string> ResolveStatusFilter(string? statusFilter)
    {
        if (string.IsNullOrWhiteSpace(statusFilter)
            || string.Equals(statusFilter, "open", StringComparison.OrdinalIgnoreCase))
        {
            return DispatchExceptionStatuses.OpenQueue;
        }

        var normalized = statusFilter.Trim().ToLowerInvariant();
        if (!DispatchExceptionStatuses.All.Contains(normalized))
        {
            throw new StlApiException(
                "dispatch_exception.invalid_status",
                "Status filter must be open, assigned, resolved, or cancelled.",
                400);
        }

        return new HashSet<string>([normalized], StringComparer.OrdinalIgnoreCase);
    }

    private async Task<DispatchException> RequireExceptionAsync(
        Guid tenantId,
        Guid exceptionId,
        CancellationToken cancellationToken)
    {
        var entity = await db.DispatchExceptions
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == exceptionId, cancellationToken);

        if (entity is null)
        {
            throw new StlApiException(
                "dispatch_exception.not_found",
                "Dispatch exception was not found.",
                404);
        }

        return entity;
    }

    private async Task<Trip> RequireTripAsync(
        Guid tenantId,
        Guid tripId,
        CancellationToken cancellationToken)
    {
        var trip = await db.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken);

        if (trip is null)
        {
            throw new StlApiException("trip.not_found", "Trip was not found.", 404);
        }

        return trip;
    }

    private static void EnsureNotTerminal(DispatchException entity)
    {
        if (DispatchExceptionStatuses.IsTerminal(entity.Status))
        {
            throw new StlApiException(
                "dispatch_exception.terminal",
                "Dispatch exception is already closed.",
                409);
        }
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new StlApiException(
                "dispatch_exception.title_required",
                "Exception title is required.",
                400);
        }

        if (title.Trim().Length > 256)
        {
            throw new StlApiException(
                "dispatch_exception.title_too_long",
                "Exception title must be 256 characters or fewer.",
                400);
        }
    }

    private static string NormalizeCategory(string? category)
    {
        var normalized = string.IsNullOrWhiteSpace(category)
            ? DispatchExceptionCategories.Other
            : category.Trim().ToLowerInvariant();

        if (!DispatchExceptionCategories.All.Contains(normalized))
        {
            throw new StlApiException(
                "dispatch_exception.invalid_category",
                "Exception category is not valid.",
                400);
        }

        return normalized;
    }

    private static string NormalizeIncidentType(string? incidentType)
    {
        var normalized = string.IsNullOrWhiteSpace(incidentType)
            ? DispatchIncidentTypes.OperationalException
            : incidentType.Trim().ToLowerInvariant();

        if (!DispatchIncidentTypes.All.Contains(normalized))
        {
            throw new StlApiException(
                "dispatch_incident.invalid_type",
                "Incident type is not valid.",
                400);
        }

        return normalized;
    }

    private static string NormalizeIncidentSeverity(string? severity)
    {
        var normalized = string.IsNullOrWhiteSpace(severity)
            ? DispatchIncidentSeverities.Medium
            : severity.Trim().ToLowerInvariant();

        if (!DispatchIncidentSeverities.All.Contains(normalized))
        {
            throw new StlApiException(
                "dispatch_incident.invalid_severity",
                "Incident severity must be low, medium, high, or critical.",
                400);
        }

        return normalized;
    }

    private static string NormalizeIncidentRoutedProduct(string? routedProduct, string incidentType)
    {
        var normalized = string.IsNullOrWhiteSpace(routedProduct)
            ? DefaultRoutedProductForIncidentType(incidentType)
            : routedProduct.Trim().ToLowerInvariant();

        if (!DispatchIncidentRoutedProducts.All.Contains(normalized))
        {
            throw new StlApiException(
                "dispatch_incident.invalid_routed_product",
                "Incident routed product is not valid.",
                400);
        }

        return normalized;
    }

    private static string DefaultRoutedProductForIncidentType(string incidentType) =>
        incidentType switch
        {
            DispatchIncidentTypes.EquipmentAbuse => DispatchIncidentRoutedProducts.MaintainArr,
            DispatchIncidentTypes.TrainingRelated => DispatchIncidentRoutedProducts.TrainArr,
            DispatchIncidentTypes.ComplianceRelated => DispatchIncidentRoutedProducts.ComplianceCore,
            DispatchIncidentTypes.Injury => DispatchIncidentRoutedProducts.StaffArr,
            _ => DispatchIncidentRoutedProducts.RoutArr,
        };

    private static string CategoryForIncidentType(string incidentType) =>
        incidentType switch
        {
            DispatchIncidentTypes.EquipmentAbuse => DispatchExceptionCategories.Vehicle,
            DispatchIncidentTypes.ComplianceRelated => DispatchExceptionCategories.Compliance,
            DispatchIncidentTypes.TrainingRelated => DispatchExceptionCategories.Driver,
            DispatchIncidentTypes.Injury => DispatchExceptionCategories.Driver,
            _ => DispatchExceptionCategories.Other,
        };

    private async Task<string> GenerateExceptionKeyAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var datePart = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            var candidate = $"DEX-{datePart}-{suffix}";
            var exists = await db.DispatchExceptions.AnyAsync(
                x => x.TenantId == tenantId && x.ExceptionKey == candidate,
                cancellationToken);
            if (!exists)
            {
                return candidate;
            }
        }

        return $"DEX-{datePart}-{Guid.NewGuid():N}".ToUpperInvariant();
    }

    private async Task<DispatchExceptionSummaryResponse> MapWithTripAsync(
        Guid tenantId,
        DispatchException entity,
        CancellationToken cancellationToken)
    {
        Trip? trip = null;
        if (entity.TripId.HasValue)
        {
            trip = await db.Trips
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == entity.TripId, cancellationToken);
        }

        return MapSummary(entity, trip);
    }

    private static DispatchExceptionSummaryResponse MapSummary(
        DispatchException entity,
        Trip? trip,
        DateTimeOffset? asOfUtc = null) =>
        new(
            entity.Id,
            entity.ExceptionKey,
            entity.Title,
            entity.Description,
            entity.Category,
            entity.Status,
            entity.IncidentType,
            entity.IncidentSeverity,
            entity.IncidentReviewStatus,
            entity.IncidentRoutedProduct,
            entity.StaffarrPersonnelIncidentId,
            entity.StaffarrIncidentRoutedAt,
            entity.StaffarrIncidentRouteStatus,
            entity.TrainarrIncidentRemediationId,
            entity.TrainarrIncidentRoutedAt,
            entity.TrainarrIncidentRouteStatus,
            entity.MaintainarrInboundEventId,
            entity.MaintainarrDefectId,
            entity.MaintainarrIncidentRoutedAt,
            entity.MaintainarrIncidentRouteStatus,
            entity.CompliancecoreFactPublicationId,
            entity.CompliancecoreIncidentRoutedAt,
            entity.CompliancecoreIncidentRouteStatus,
            entity.TripId,
            trip?.TripNumber,
            trip?.Title,
            entity.AssignedToUserId,
            entity.SlaDueAt,
            DispatchExceptionRules.IsSlaBreached(entity, asOfUtc ?? DateTimeOffset.UtcNow),
            entity.ResolutionTemplateKey,
            entity.ResolutionNotes,
            entity.CreatedByUserId,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.AssignedAt,
            entity.ResolvedAt);
}
