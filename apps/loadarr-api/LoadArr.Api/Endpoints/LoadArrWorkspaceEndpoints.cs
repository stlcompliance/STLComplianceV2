using System.Globalization;
using LoadArr.Api.Services;
using LoadArr.Api.Settings;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace LoadArr.Api.Endpoints;

public static partial class LoadArrWorkspaceEndpoints
{
    private static IResult ReferenceDependencyUnavailable(string surface, string? message = null) =>
        Results.Json(
            new LoadArrProblemResponse(
                "dependency_unavailable",
                message ?? $"{surface} is unavailable because LoadArr does not yet have synchronized StaffArr and SupplyArr reference data for this tenant."),
            statusCode: StatusCodes.Status503ServiceUnavailable);

    private static IResult WorkspaceReadModelUnavailable(string surface) =>
        Results.Json(
            new LoadArrProblemResponse(
                "dependency_unavailable",
                $"{surface} is unavailable because LoadArr does not yet have an authoritative warehouse read model for this tenant."),
            statusCode: StatusCodes.Status503ServiceUnavailable);

    private static IResult WorkflowAuditUnavailable(string surface) =>
        Results.Json(
            new LoadArrProblemResponse(
                "dependency_unavailable",
                $"{surface} is unavailable because LoadArr does not yet persist authoritative cancellation audit state for this tenant."),
            statusCode: StatusCodes.Status503ServiceUnavailable);

    private static IResult ClientRequestConflict(string surface) =>
        Results.Json(
            new LoadArrProblemResponse(
                "client_request_conflict",
                $"{surface} could not be retried because the client request id was already used for a different payload."),
            statusCode: StatusCodes.Status409Conflict);

    private static IResult ConflictProblem(string errorCode, string message) =>
        Results.Json(
            new LoadArrProblemResponse(errorCode, message),
            statusCode: StatusCodes.Status409Conflict);

    private static void ApplyWorkspaceReadAuthorization(RouteGroupBuilder group)
    {
        group.AddEndpointFilterFactory((_, next) => async invocationContext =>
        {
            var authorization = invocationContext.HttpContext.RequestServices.GetRequiredService<LoadArrAuthorizationService>();
            authorization.RequireWorkspaceRead(invocationContext.HttpContext.User);
            return await next(invocationContext);
        });
    }

    private static void ApplyOperationalAuthorization(RouteGroupBuilder group)
    {
        group.AddEndpointFilterFactory((_, next) => async invocationContext =>
        {
            var authorization = invocationContext.HttpContext.RequestServices.GetRequiredService<LoadArrAuthorizationService>();
            var requestMethod = invocationContext.HttpContext.Request.Method;

            if (HttpMethods.IsGet(requestMethod))
            {
                authorization.RequireWorkspaceRead(invocationContext.HttpContext.User);
            }
            else
            {
                authorization.RequireOperationalWrite(invocationContext.HttpContext.User);
            }

            return await next(invocationContext);
        });
    }

    public static void MapLoadArrWorkspaceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/workspace")
            .WithTags("Workspace")
            .RequireAuthorization();
        ApplyWorkspaceReadAuthorization(group);

        group.MapGet("/site-sources", async (
            HttpContext context,
            ILoadArrSiteSourceService siteSources,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var items = await siteSources.ListSitesAsync(
                    context.User.GetTenantId(),
                    cancellationToken);

                return Results.Ok(new LoadArrListResponse<LoadArrSiteSourceResponse>(items, items.Count));
            }
            catch (StlApiException ex)
            {
                return ReferenceDependencyUnavailable("LoadArr site reference metadata", ex.Message);
            }
        })
        .WithName("GetLoadArrSiteSources");

        group.MapGet("/summary", () => WorkspaceReadModelUnavailable("LoadArr workspace summary"))
            .WithName("GetLoadArrWorkspaceSummary");

        group.MapGet("/locations", async (
            HttpContext context,
            ILoadArrLocationReferenceService locationReferences,
            string? staffarrSiteOrgUnitId,
            string? locationType,
            bool? active,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var items = await locationReferences.ListLocationsAsync(
                    context.User.GetTenantId(),
                    staffarrSiteOrgUnitId,
                    locationType,
                    active,
                    cancellationToken);

                return Results.Ok(new LoadArrListResponse<LoadArrLocationResponse>(items, items.Count));
            }
            catch (StlApiException ex)
            {
                return ReferenceDependencyUnavailable("LoadArr location references", ex.Message);
            }
        })
        .WithName("ListLoadArrLocations");

        group.MapGet("/locations/{id}", async (
            HttpContext context,
            ILoadArrLocationReferenceService locationReferences,
            string id,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var location = await locationReferences.GetLocationAsync(
                    context.User.GetTenantId(),
                    id,
                    cancellationToken);

                return location is null ? Results.NotFound() : Results.Ok(location);
            }
            catch (StlApiException ex)
            {
                return ReferenceDependencyUnavailable("LoadArr location references", ex.Message);
            }
        })
        .WithName("GetLoadArrLocation");

        group.MapGet("/locations/tree", async (
            HttpContext context,
            ILoadArrLocationReferenceService locationReferences,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var nodes = await locationReferences.GetLocationTreeAsync(
                    context.User.GetTenantId(),
                    cancellationToken);

                return Results.Ok(nodes);
            }
            catch (StlApiException ex)
            {
                return ReferenceDependencyUnavailable("LoadArr location references", ex.Message);
            }
        })
        .WithName("GetLoadArrLocationTree");

        group.MapGet("/inventory", async (
            HttpContext context,
            LoadArrOperationalWorkflowStore store,
            string? query,
            string? state,
            string? locationId,
            CancellationToken cancellationToken) =>
        {
            var balances = (await store.ListInventoryBalancesAsync(context.User.GetTenantId(), cancellationToken))
                .Where(item => string.IsNullOrWhiteSpace(query) || InventoryMatchesQuery(item, query))
                .Where(item => string.IsNullOrWhiteSpace(state)
                    || string.Equals(item.State, state, StringComparison.OrdinalIgnoreCase))
                .Where(item => string.IsNullOrWhiteSpace(locationId)
                    || string.Equals(item.LocationId, locationId, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrInventoryBalanceResponse>(balances, balances.Length));
        })
        .WithName("ListLoadArrInventory");

        group.MapGet("/supplyarr-item-references", async (
            HttpContext context,
            ILoadArrSupplyArrItemReferenceService itemReferences,
            string? query,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var items = await itemReferences.ListItemReferencesAsync(
                    context.User.GetTenantId(),
                    query,
                    cancellationToken);

                return Results.Ok(new LoadArrListResponse<LoadArrSupplyArrItemReferenceResponse>(items, items.Count));
            }
            catch (StlApiException ex)
            {
                return ReferenceDependencyUnavailable("LoadArr SupplyArr item references", ex.Message);
            }
        })
        .WithName("ListLoadArrSupplyArrItemReferences");

        group.MapGet("/tasks", async (
            HttpContext context,
            LoadArrOperationalWorkflowStore store,
            string? status,
            string? priority,
            string? taskType,
            CancellationToken cancellationToken) =>
        {
            var tasks = (await store.ListWarehouseTasksAsync(context.User.GetTenantId(), cancellationToken))
                .Where(task => string.IsNullOrWhiteSpace(status)
                    || string.Equals(task.Status, status, StringComparison.OrdinalIgnoreCase))
                .Where(task => string.IsNullOrWhiteSpace(priority)
                    || string.Equals(task.Priority, priority, StringComparison.OrdinalIgnoreCase))
                .Where(task => string.IsNullOrWhiteSpace(taskType)
                    || string.Equals(task.TaskType, taskType, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrWarehouseTaskResponse>(tasks, tasks.Length));
        })
        .WithName("ListLoadArrWarehouseTasks");

        group.MapGet("/holds", (string? status, string? holdType) =>
            WorkspaceReadModelUnavailable("LoadArr inventory holds"))
        .WithName("ListLoadArrHolds");

        group.MapGet("/route-handoffs", (string? targetProduct, string? status) =>
            WorkspaceReadModelUnavailable("LoadArr route handoffs"))
        .WithName("ListLoadArrRouteHandoffs");

        group.MapGet("/evidence", (string? evidenceType, string? locationNameSnapshot) =>
            WorkspaceReadModelUnavailable("LoadArr warehouse evidence"))
        .WithName("ListLoadArrEvidence");

        group.MapGet("/unexplained-inventory", (string? status, string? locationId) =>
            WorkspaceReadModelUnavailable("LoadArr unexplained inventory"))
        .WithName("ListLoadArrWorkspaceUnexplainedInventory");

        var receiving = app.MapGroup("/api/v1/receiving")
            .WithTags("Receiving")
            .RequireAuthorization();
        ApplyOperationalAuthorization(receiving);

        receiving.MapGet("/", async (
            HttpContext context,
            LoadArrOperationalWorkflowStore store,
            string? status,
            string? receivingType,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var sessions = (await store.ListReceivingSessionsAsync(tenantId, cancellationToken))
                .Where(session => status is null
                    || string.Equals(session.Status, status, StringComparison.OrdinalIgnoreCase))
                .Where(session => receivingType is null
                    || string.Equals(session.ReceivingType, receivingType, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(session => session.StartedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrReceivingSessionResponse>(sessions, sessions.Length));
        })
        .WithName("ListLoadArrReceivingSessions");

        receiving.MapGet("/{id}", async (
            HttpContext context,
            LoadArrOperationalWorkflowStore store,
            string id,
            CancellationToken cancellationToken) =>
        {
            var session = await store.GetReceivingSessionAsync(context.User.GetTenantId(), id, cancellationToken);

            return session is null ? Results.NotFound() : Results.Ok(session);
        })
        .WithName("GetLoadArrReceivingSession");

        receiving.MapPost("/", async (
            HttpContext context,
            LoadArrOperationalWorkflowStore store,
            ILoadArrLocationReferenceService locationReferences,
            ILoadArrSupplyArrItemReferenceService itemReferences,
            CreateLoadArrReceivingSessionRequest request,
            CancellationToken cancellationToken) =>
        {
            try
            {
                return await CreateReceivingSessionDraftAsync(
                    context.User.GetTenantId(),
                    request,
                    store,
                    locationReferences,
                    itemReferences,
                    cancellationToken);
            }
            catch (StlApiException ex)
            {
                return ReferenceDependencyUnavailable("LoadArr receiving creation", ex.Message);
            }
        })
        .WithName("CreateLoadArrReceivingSession");

        receiving.MapPost("/{id}/lines", (
            HttpContext context,
            LoadArrOperationalWorkflowStore store,
            string id,
            AddLoadArrReceivingLineRequest request,
            CancellationToken cancellationToken) =>
            ReferenceDependencyUnavailable("LoadArr receiving line updates"))
        .WithName("AddLoadArrReceivingLine");

        receiving.MapPost("/draft/complete", (
            HttpContext context,
            LoadArrOperationalWorkflowStore store,
            CompleteLoadArrReceivingSessionRequest request,
            CancellationToken cancellationToken) => ReferenceDependencyUnavailable("LoadArr receiving completion"))
        .WithName("CompleteLoadArrReceivingDraft");

        receiving.MapPost("/{id}/complete", async (
            HttpContext context,
            LoadArrOperationalWorkflowStore store,
            ILoadArrLocationReferenceService locationReferences,
            ILoadArrSupplyArrItemReferenceService itemReferences,
            string id,
            CompleteLoadArrReceivingSessionRequest request,
            CancellationToken cancellationToken) =>
        {
            try
            {
                return await CompleteReceivingSessionAsync(
                    context.User.GetTenantId(),
                    id,
                    request,
                    store,
                    locationReferences,
                    itemReferences,
                    cancellationToken);
            }
            catch (StlApiException ex)
            {
                return ReferenceDependencyUnavailable("LoadArr receiving completion", ex.Message);
            }
        })
        .WithName("CompleteLoadArrReceivingSession");

        receiving.MapPost("/{id}/cancel", (
            HttpContext context,
            LoadArrOperationalWorkflowStore store,
            string id,
            CancelLoadArrReceivingSessionRequest request,
            CancellationToken cancellationToken) => WorkflowAuditUnavailable("LoadArr receiving cancellation"))
        .WithName("CancelLoadArrReceivingSession");

        var transfers = app.MapGroup("/api/v1/transfers")
            .WithTags("Transfers")
            .RequireAuthorization();
        ApplyOperationalAuthorization(transfers);

        transfers.MapGet("/", async (
            HttpContext context,
            LoadArrOperationalWorkflowStore store,
            string? status,
            string? transferType,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var orders = (await store.ListTransferOrdersAsync(tenantId, cancellationToken))
                .Where(order => status is null
                    || string.Equals(order.Status, status, StringComparison.OrdinalIgnoreCase))
                .Where(order => transferType is null
                    || string.Equals(order.TransferType, transferType, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(order => order.CreatedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrTransferOrderResponse>(orders, orders.Length));
        })
        .WithName("ListLoadArrTransferOrders");

        transfers.MapGet("/{id}", async (
            HttpContext context,
            LoadArrOperationalWorkflowStore store,
            string id,
            CancellationToken cancellationToken) =>
        {
            var order = await store.GetTransferOrderAsync(context.User.GetTenantId(), id, cancellationToken);

            return order is null ? Results.NotFound() : Results.Ok(order);
        })
        .WithName("GetLoadArrTransferOrder");

        transfers.MapPost("/", async (
            HttpContext context,
            LoadArrOperationalWorkflowStore store,
            ILoadArrLocationReferenceService locationReferences,
            ILoadArrSupplyArrItemReferenceService itemReferences,
            CreateLoadArrTransferOrderRequest request,
            CancellationToken cancellationToken) =>
        {
            try
            {
                return await CreateTransferOrderDraftAsync(
                    context.User.GetTenantId(),
                    request,
                    store,
                    locationReferences,
                    itemReferences,
                    cancellationToken);
            }
            catch (StlApiException ex)
            {
                return ReferenceDependencyUnavailable("LoadArr transfer creation", ex.Message);
            }
        })
        .WithName("CreateLoadArrTransferOrder");

        transfers.MapPost("/draft/complete", (
            HttpContext context,
            LoadArrOperationalWorkflowStore store,
            CompleteLoadArrTransferOrderRequest request,
            CancellationToken cancellationToken) => WorkspaceReadModelUnavailable("LoadArr transfer completion"))
        .WithName("CompleteLoadArrTransferDraft");

        transfers.MapPost("/{id}/complete", (
            HttpContext context,
            LoadArrOperationalWorkflowStore store,
            string id,
            CompleteLoadArrTransferOrderRequest request,
            CancellationToken cancellationToken) => WorkspaceReadModelUnavailable("LoadArr transfer completion"))
        .WithName("CompleteLoadArrTransferOrder");

        transfers.MapPost("/{id}/cancel", (
            HttpContext context,
            LoadArrOperationalWorkflowStore store,
            string id,
            CancelLoadArrTransferOrderRequest request,
            CancellationToken cancellationToken) => WorkflowAuditUnavailable("LoadArr transfer cancellation"))
        .WithName("CancelLoadArrTransferOrder");

        var holds = app.MapGroup("/api/v1/holds")
            .WithTags("Holds")
            .RequireAuthorization();
        ApplyOperationalAuthorization(holds);

        holds.MapGet("/", (string? status, string? holdType) =>
            WorkspaceReadModelUnavailable("LoadArr inventory holds"))
        .WithName("ListLoadArrInventoryHolds");

        holds.MapGet("/{id}", (string id) =>
            WorkspaceReadModelUnavailable("LoadArr inventory hold detail"))
        .WithName("GetLoadArrInventoryHold");

        holds.MapPost("/", (CreateLoadArrInventoryHoldRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr inventory hold workflow"))
        .WithName("CreateLoadArrInventoryHold");

        holds.MapPost("/{id}/release", (string id, ReleaseLoadArrInventoryHoldRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr inventory hold workflow"))
        .WithName("ReleaseLoadArrInventoryHold");

        var unexplainedInventory = app.MapGroup("/api/v1/unexplained-inventory")
            .WithTags("Unexplained Inventory")
            .RequireAuthorization();
        ApplyOperationalAuthorization(unexplainedInventory);

        unexplainedInventory.MapGet("/", (string? status, string? locationId) =>
            WorkspaceReadModelUnavailable("LoadArr unexplained inventory"))
        .WithName("ListLoadArrUnexplainedInventory");

        unexplainedInventory.MapGet("/{id}", (string id) =>
            WorkspaceReadModelUnavailable("LoadArr unexplained inventory detail"))
        .WithName("GetLoadArrUnexplainedInventory");

        unexplainedInventory.MapPost("/", (CreateLoadArrUnexplainedInventoryRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr unexplained inventory workflow"))
        .WithName("CreateLoadArrUnexplainedInventory");

        unexplainedInventory.MapPost("/{id}/resolve", (string id, ResolveLoadArrUnexplainedInventoryRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr unexplained inventory workflow"))
        .WithName("ResolveLoadArrUnexplainedInventory");

        unexplainedInventory.MapPost("/{id}/quarantine", (string id, QuarantineLoadArrUnexplainedInventoryRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr unexplained inventory workflow"))
        .WithName("QuarantineLoadArrUnexplainedInventory");

        unexplainedInventory.MapPost("/{id}/scrap", (string id, ScrapLoadArrUnexplainedInventoryRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr unexplained inventory workflow"))
        .WithName("ScrapLoadArrUnexplainedInventory");
    }

    private static bool InventoryMatchesQuery(LoadArrInventoryBalanceResponse item, string query)
    {
        var normalizedQuery = query.Trim();
        if (normalizedQuery.Length == 0)
        {
            return true;
        }

        return Contains(item.SupplyarrItemId)
            || Contains(item.ItemNameSnapshot)
            || Contains(item.LocationNameSnapshot)
            || Contains(item.OriginReference)
            || item.TraceTags.Any(Contains);

        bool Contains(string value) =>
            value.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase);
    }

    private static bool SupplyArrItemReferenceMatchesQuery(
        LoadArrSupplyArrItemReferenceResponse item,
        string query)
    {
        var normalizedQuery = query.Trim();
        if (normalizedQuery.Length == 0)
        {
            return true;
        }

        return item.SupplyarrItemId.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
            || item.ItemNumberSnapshot.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
            || item.ItemNameSnapshot.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
            || item.ItemTypeSnapshot.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase);
    }

    private static LoadArrSupplyArrItemReferenceResponse? ResolveSupplyArrItemReference(string supplyarrItemId) =>
        CreateSupplyArrItemReferences()
            .FirstOrDefault(item => string.Equals(item.SupplyarrItemId, supplyarrItemId, StringComparison.OrdinalIgnoreCase));

    private static LoadArrLocationResponse? ResolveLocation(string locationId) =>
        CreateWorkspaceSummary().Locations
            .FirstOrDefault(location => string.Equals(location.Id, locationId, StringComparison.OrdinalIgnoreCase));

    private static async Task<IResult> CreateReceivingSessionDraftAsync(
        Guid tenantId,
        CreateLoadArrReceivingSessionRequest request,
        LoadArrOperationalWorkflowStore store,
        ILoadArrLocationReferenceService locationReferences,
        ILoadArrSupplyArrItemReferenceService itemReferences,
        CancellationToken cancellationToken)
    {
        var validation = await ValidateReceivingDraftRequestAsync(
            tenantId,
            request,
            locationReferences,
            itemReferences,
            cancellationToken);
        if (validation.Result is not null)
        {
            return validation.Result;
        }

        var clientRequestId = request.ClientRequestId!.Trim();
        var requestFingerprint = CreateReceivingDraftFingerprint(request);
        var existing = await store.GetReceivingSessionByClientRequestIdAsync(
            tenantId,
            clientRequestId,
            cancellationToken);
        if (existing.Session is not null)
        {
            return BuildIdempotentCreateResult(
                existing.Session,
                existing.RequestFingerprint,
                requestFingerprint,
                "LoadArr receiving draft");
        }

        var now = DateTimeOffset.UtcNow;
        var location = validation.Location!;
        var item = validation.Item!;
        var requiresInspection = string.Equals(request.Condition, "pending_inspection", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(request.DiscrepancyReasonCode);
        var sessionId = CreateReceivingSessionId();
        var session = new LoadArrReceivingSessionResponse(
            sessionId,
            CreateReceivingNumber(now),
            NormalizeRequiredOrDefault(request.ReceivingType, "manual"),
            requiresInspection ? "inspection_required" : "open",
            location.StaffarrSiteOrgUnitId,
            location.StaffarrSiteNameSnapshot,
            NormalizeRequiredOrDefault(request.SourceProductKey, "loadarr"),
            NormalizeRequiredOrDefault(request.SourceObjectType, "manual_receipt"),
            ResolveReceivingSourceObjectId(request, sessionId),
            NormalizeOptional(request.SupplierNameSnapshot) ?? string.Empty,
            request.StartedByPersonId.Trim(),
            null,
            now.ToString("O"),
            null,
            new[]
            {
                new LoadArrReceivingLineResponse(
                    $"line-{Guid.NewGuid():N}"[..13],
                    item.SupplyarrItemId,
                    item.ItemNameSnapshot,
                    request.ExpectedQuantity,
                    request.ReceivedQuantity,
                    item.UnitOfMeasureSnapshot,
                    location.Id,
                    location.Name,
                    NormalizeOptional(request.LotCode),
                    NormalizeOptional(request.SerialCode),
                    NormalizeRequiredOrDefault(request.Condition, "new"),
                    requiresInspection ? "needs_review" : "ready_to_complete",
                    NormalizeOptional(request.DiscrepancyReasonCode),
                    NormalizeOptional(request.EvidenceSummary))
            });

        try
        {
            await store.SaveReceivingSessionAsync(
                tenantId,
                session,
                clientRequestId,
                requestFingerprint,
                cancellationToken);
        }
        catch (DbUpdateException)
        {
            var existingAfterConflict = await store.GetReceivingSessionByClientRequestIdAsync(
                tenantId,
                clientRequestId,
                cancellationToken);
            if (existingAfterConflict.Session is not null)
            {
                return BuildIdempotentCreateResult(
                    existingAfterConflict.Session,
                    existingAfterConflict.RequestFingerprint,
                    requestFingerprint,
                    "LoadArr receiving draft");
            }

            throw;
        }

        return Results.Created($"/api/v1/receiving/{Uri.EscapeDataString(session.Id)}", session);
    }

    private static async Task<IResult> CreateTransferOrderDraftAsync(
        Guid tenantId,
        CreateLoadArrTransferOrderRequest request,
        LoadArrOperationalWorkflowStore store,
        ILoadArrLocationReferenceService locationReferences,
        ILoadArrSupplyArrItemReferenceService itemReferences,
        CancellationToken cancellationToken)
    {
        var validation = await ValidateTransferDraftRequestAsync(
            tenantId,
            request,
            locationReferences,
            itemReferences,
            cancellationToken);
        if (validation.Result is not null)
        {
            return validation.Result;
        }

        var clientRequestId = request.ClientRequestId!.Trim();
        var requestFingerprint = CreateTransferDraftFingerprint(request);
        var existing = await store.GetTransferOrderByClientRequestIdAsync(
            tenantId,
            clientRequestId,
            cancellationToken);
        if (existing.Order is not null)
        {
            return BuildIdempotentCreateResult(
                existing.Order,
                existing.RequestFingerprint,
                requestFingerprint,
                "LoadArr transfer draft");
        }

        var now = DateTimeOffset.UtcNow;
        var fromLocation = validation.FromLocation!;
        var toLocation = validation.ToLocation!;
        var item = validation.Item!;
        var order = new LoadArrTransferOrderResponse(
            CreateTransferOrderId(),
            CreateTransferNumber(now),
            "draft",
            NormalizeRequiredOrDefault(request.TransferType, "bin_to_bin"),
            fromLocation.StaffarrSiteOrgUnitId,
            fromLocation.StaffarrSiteNameSnapshot,
            fromLocation.Id,
            fromLocation.Name,
            toLocation.Id,
            toLocation.Name,
            request.RequestedByPersonId.Trim(),
            null,
            request.ReasonCode.Trim(),
            now.ToString("O"),
            null,
            new[]
            {
                new LoadArrTransferLineResponse(
                    $"xfer-line-{Guid.NewGuid():N}"[..18],
                    item.SupplyarrItemId,
                    item.ItemNameSnapshot,
                    request.Quantity,
                    item.UnitOfMeasureSnapshot,
                    NormalizeOptional(request.LotCode),
                    NormalizeOptional(request.SerialCode),
                    "draft")
            });

        try
        {
            await store.SaveTransferOrderAsync(
                tenantId,
                order,
                clientRequestId,
                requestFingerprint,
                cancellationToken);
        }
        catch (DbUpdateException)
        {
            var existingAfterConflict = await store.GetTransferOrderByClientRequestIdAsync(
                tenantId,
                clientRequestId,
                cancellationToken);
            if (existingAfterConflict.Order is not null)
            {
                return BuildIdempotentCreateResult(
                    existingAfterConflict.Order,
                    existingAfterConflict.RequestFingerprint,
                    requestFingerprint,
                    "LoadArr transfer draft");
            }

            throw;
        }

        return Results.Created($"/api/v1/transfers/{Uri.EscapeDataString(order.Id)}", order);
    }

    private static async Task<IResult> CompleteReceivingSessionAsync(
        Guid tenantId,
        string sessionId,
        CompleteLoadArrReceivingSessionRequest request,
        LoadArrOperationalWorkflowStore store,
        ILoadArrLocationReferenceService locationReferences,
        ILoadArrSupplyArrItemReferenceService itemReferences,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CompletedByPersonId))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_completed_by",
                "Receiving completion requires the completing person reference."));
        }

        var session = await store.GetReceivingSessionAsync(tenantId, sessionId, cancellationToken);
        if (session is null)
        {
            return Results.NotFound();
        }

        var line = session.Lines.SingleOrDefault();
        if (line is null || session.Lines.Count != 1)
        {
            return ReferenceDependencyUnavailable(
                "LoadArr receiving completion",
                "Receiving completion is unavailable because this saved draft is not in the authoritative single-line format supported by the current rollout slice.");
        }

        if (string.Equals(session.Status, "canceled", StringComparison.OrdinalIgnoreCase))
        {
            return ConflictProblem(
                "receiving_session_canceled",
                "This receiving draft was canceled and can no longer be completed.");
        }

        if (string.Equals(session.Status, "completed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(session.Status, "partial", StringComparison.OrdinalIgnoreCase))
        {
            var existingCompletion = await store.GetReceivingCompletionAsync(tenantId, sessionId, cancellationToken);
            return existingCompletion is null
                ? ReferenceDependencyUnavailable(
                    "LoadArr receiving completion",
                    "Receiving completion cannot be retried because the persisted warehouse completion record for this draft is incomplete.")
                : Results.Ok(existingCompletion);
        }

        if (!string.Equals(session.Status, "open", StringComparison.OrdinalIgnoreCase))
        {
            return ConflictProblem(
                "receiving_session_not_completable",
                $"This receiving draft is in '{session.Status}' and cannot be completed from the current LoadArr rollout slice.");
        }

        if (!ReceivingCompletionRequestMatchesSession(session, request))
        {
            return ConflictProblem(
                "stale_receiving_draft",
                "Receiving completion no longer matches the saved draft. Refresh the draft and retry so LoadArr completes the authoritative server version.");
        }

        if (line.ReceivedQuantity <= 0m)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_received_quantity",
                "Receiving completion requires a received quantity greater than zero."));
        }

        if (ReceivingCompletionRequiresInspectionHold(session, line))
        {
            return ReferenceDependencyUnavailable(
                "LoadArr receiving completion",
                "Receiving drafts that require inspection or discrepancy review cannot be completed until LoadArr has authoritative inspection and hold workflow truth for this tenant.");
        }

        var location = await locationReferences.GetLocationAsync(
            tenantId,
            line.WarehouseLocationId,
            cancellationToken);
        if (location is null || !location.Active)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_receiving_location",
                "Receiving completion requires the saved draft to reference an active StaffArr-owned location."));
        }

        var item = await itemReferences.GetItemReferenceAsync(
            tenantId,
            line.SupplyarrItemId,
            cancellationToken);
        if (item is null)
        {
            return ReferenceDependencyUnavailable(
                "LoadArr receiving completion",
                "Receiving completion is unavailable because the saved SupplyArr item reference is no longer available for this tenant.");
        }

        if (item.RequiresTraceabilityCapture
            && string.IsNullOrWhiteSpace(line.LotCode)
            && string.IsNullOrWhiteSpace(line.SerialCode))
        {
            return ConflictProblem(
                "missing_traceability_capture",
                "This receiving draft cannot be completed until a lot code or serial code is captured for the SupplyArr-tracked item.");
        }

        try
        {
            var completion = await store.CompleteReceivingSessionAsync(
                tenantId,
                session,
                request.CompletedByPersonId.Trim(),
                NormalizeOptional(request.ComplianceEvaluationId),
                NormalizeOptional(request.EvidenceSummary),
                cancellationToken);

            return Results.Ok(completion);
        }
        catch (DbUpdateException)
        {
            var existingCompletion = await store.GetReceivingCompletionAsync(tenantId, sessionId, cancellationToken);
            if (existingCompletion is not null)
            {
                return Results.Ok(existingCompletion);
            }

            throw;
        }
    }

    private static async Task<(IResult? Result, LoadArrLocationResponse? Location, LoadArrSupplyArrItemReferenceResponse? Item)>
        ValidateReceivingDraftRequestAsync(
            Guid tenantId,
            CreateLoadArrReceivingSessionRequest request,
            ILoadArrLocationReferenceService locationReferences,
            ILoadArrSupplyArrItemReferenceService itemReferences,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ClientRequestId))
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "missing_client_request_id",
                "Receiving drafts require a client request id so LoadArr can safely handle retries.")), null, null);
        }

        if (string.IsNullOrWhiteSpace(request.StartedByPersonId))
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "missing_started_by",
                "Receiving drafts require the starting person reference.")), null, null);
        }

        if (request.ExpectedQuantity <= 0m)
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "invalid_expected_quantity",
                "Receiving drafts require an expected quantity greater than zero.")), null, null);
        }

        if (request.ReceivedQuantity < 0m)
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "invalid_received_quantity",
                "Received quantity cannot be negative.")), null, null);
        }

        if (string.IsNullOrWhiteSpace(request.Condition))
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "missing_condition",
                "Receiving drafts require a recorded condition.")), null, null);
        }

        if (!string.Equals(request.ReceivingType, "manual", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(request.SourceObjectId))
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "missing_source_reference",
                "Non-manual receiving drafts require a source reference.")), null, null);
        }

        var location = await locationReferences.GetLocationAsync(
            tenantId,
            request.WarehouseLocationId,
            cancellationToken);
        if (location is null || !location.Active)
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "invalid_receiving_location",
                "Receiving drafts require an active StaffArr-owned receiving location.")), null, null);
        }

        var item = await itemReferences.GetItemReferenceAsync(
            tenantId,
            request.SupplyarrItemId,
            cancellationToken);
        if (item is null)
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "invalid_supplyarr_item_reference",
                "Receiving drafts require an item reference that exists in SupplyArr.")), null, null);
        }

        return (null, location, item);
    }

    private static async Task<(IResult? Result, LoadArrLocationResponse? FromLocation, LoadArrLocationResponse? ToLocation, LoadArrSupplyArrItemReferenceResponse? Item)>
        ValidateTransferDraftRequestAsync(
            Guid tenantId,
            CreateLoadArrTransferOrderRequest request,
            ILoadArrLocationReferenceService locationReferences,
            ILoadArrSupplyArrItemReferenceService itemReferences,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ClientRequestId))
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "missing_client_request_id",
                "Transfer drafts require a client request id so LoadArr can safely handle retries.")), null, null, null);
        }

        if (string.IsNullOrWhiteSpace(request.RequestedByPersonId))
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "missing_requested_by",
                "Transfer drafts require the requesting person reference.")), null, null, null);
        }

        if (string.IsNullOrWhiteSpace(request.ReasonCode))
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "missing_reason_code",
                "Transfer drafts require a controlled reason code.")), null, null, null);
        }

        if (request.Quantity <= 0m)
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "invalid_transfer_quantity",
                "Transfer quantity must be greater than zero.")), null, null, null);
        }

        var fromLocation = await locationReferences.GetLocationAsync(
            tenantId,
            request.FromLocationId,
            cancellationToken);
        var toLocation = await locationReferences.GetLocationAsync(
            tenantId,
            request.ToLocationId,
            cancellationToken);

        if (fromLocation is null || !fromLocation.Active || toLocation is null || !toLocation.Active)
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "invalid_transfer_location",
                "Transfer drafts require active StaffArr-owned source and destination locations.")), null, null, null);
        }

        if (string.Equals(fromLocation.Id, toLocation.Id, StringComparison.OrdinalIgnoreCase))
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "same_transfer_location",
                "Transfer source and destination must be different StaffArr locations.")), null, null, null);
        }

        if (!string.Equals(request.TransferType, "site_to_site", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(fromLocation.StaffarrSiteOrgUnitId, toLocation.StaffarrSiteOrgUnitId, StringComparison.OrdinalIgnoreCase))
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "site_transfer_type_required",
                "Transfers across StaffArr sites must use the site_to_site transfer type.")), null, null, null);
        }

        var item = await itemReferences.GetItemReferenceAsync(
            tenantId,
            request.SupplyarrItemId,
            cancellationToken);
        if (item is null)
        {
            return (Results.BadRequest(new LoadArrProblemResponse(
                "invalid_supplyarr_item_reference",
                "Transfer drafts require an item reference that exists in SupplyArr.")), null, null, null);
        }

        return (null, fromLocation, toLocation, item);
    }

    private static bool ReceivingCompletionRequestMatchesSession(
        LoadArrReceivingSessionResponse session,
        CompleteLoadArrReceivingSessionRequest request)
    {
        var line = session.Lines.SingleOrDefault();
        if (line is null)
        {
            return false;
        }

        return string.Equals(
                NormalizeRequiredOrDefault(request.ReceivingType, "manual"),
                session.ReceivingType,
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                NormalizeRequiredOrDefault(request.SourceProductKey, "loadarr"),
                session.SourceProductKey,
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                NormalizeRequiredOrDefault(request.SourceObjectType, "manual_receipt"),
                session.SourceObjectType,
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                NormalizeRequiredOrDefault(request.SourceObjectId, string.Empty),
                session.SourceObjectId,
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                NormalizeOptional(request.SupplierNameSnapshot) ?? string.Empty,
                session.SupplierNameSnapshot,
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                NormalizeRequiredOrDefault(request.SupplyarrItemId, string.Empty),
                line.SupplyarrItemId,
                StringComparison.OrdinalIgnoreCase)
            && request.ExpectedQuantity == line.ExpectedQuantity
            && request.ReceivedQuantity == line.ReceivedQuantity
            && string.Equals(
                NormalizeRequiredOrDefault(request.WarehouseLocationId, string.Empty),
                line.WarehouseLocationId,
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                NormalizeOptional(request.LotCode),
                NormalizeOptional(line.LotCode),
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                NormalizeOptional(request.SerialCode),
                NormalizeOptional(line.SerialCode),
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                NormalizeRequiredOrDefault(request.Condition, "new"),
                line.Condition,
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                NormalizeOptional(request.DiscrepancyReasonCode),
                NormalizeOptional(line.DiscrepancyReasonCode),
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool ReceivingCompletionRequiresInspectionHold(
        LoadArrReceivingSessionResponse session,
        LoadArrReceivingLineResponse line) =>
        string.Equals(session.Status, "inspection_required", StringComparison.OrdinalIgnoreCase)
        || string.Equals(line.Status, "needs_review", StringComparison.OrdinalIgnoreCase)
        || string.Equals(line.Condition, "pending_inspection", StringComparison.OrdinalIgnoreCase)
        || !string.IsNullOrWhiteSpace(line.DiscrepancyReasonCode);

    private static string CreateReceivingSessionId() => $"recv-{Guid.NewGuid():N}"[..13];

    private static string CreateTransferOrderId() => $"xfer-{Guid.NewGuid():N}"[..13];

    private static string CreateReceivingNumber(DateTimeOffset createdAtUtc) =>
        $"RCV-{createdAtUtc:yyMMdd-HHmmss}-{Guid.NewGuid():N}"[..22];

    private static string CreateTransferNumber(DateTimeOffset createdAtUtc) =>
        $"TRF-{createdAtUtc:yyMMdd-HHmmss}-{Guid.NewGuid():N}"[..22];

    private static string ResolveReceivingSourceObjectId(CreateLoadArrReceivingSessionRequest request, string sessionId)
    {
        var normalized = NormalizeOptional(request.SourceObjectId);
        if (normalized is not null)
        {
            return normalized;
        }

        return string.Equals(request.ReceivingType, "manual", StringComparison.OrdinalIgnoreCase)
            ? $"manual:{sessionId}"
            : sessionId;
    }

    private static string NormalizeRequiredOrDefault(string? value, string fallback) =>
        NormalizeOptional(value) ?? fallback;

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static IResult BuildIdempotentCreateResult<TResponse>(
        TResponse response,
        string? existingRequestFingerprint,
        string requestFingerprint,
        string surface)
    {
        if (!string.Equals(existingRequestFingerprint, requestFingerprint, StringComparison.Ordinal))
        {
            return ClientRequestConflict(surface);
        }

        return Results.Ok(response);
    }

    private static string CreateReceivingDraftFingerprint(CreateLoadArrReceivingSessionRequest request) =>
        string.Join('|',
            NormalizeRequiredOrDefault(request.ReceivingType, "manual"),
            NormalizeRequiredOrDefault(request.SourceProductKey, "loadarr"),
            NormalizeRequiredOrDefault(request.SourceObjectType, "manual_receipt"),
            NormalizeOptional(request.SourceObjectId) ?? string.Empty,
            NormalizeOptional(request.SupplierNameSnapshot) ?? string.Empty,
            request.StartedByPersonId.Trim(),
            NormalizeRequiredOrDefault(request.SupplyarrItemId, string.Empty),
            request.ExpectedQuantity.ToString(CultureInfo.InvariantCulture),
            request.ReceivedQuantity.ToString(CultureInfo.InvariantCulture),
            NormalizeRequiredOrDefault(request.WarehouseLocationId, string.Empty),
            NormalizeOptional(request.LotCode) ?? string.Empty,
            NormalizeOptional(request.SerialCode) ?? string.Empty,
            NormalizeRequiredOrDefault(request.Condition, "new"),
            NormalizeOptional(request.DiscrepancyReasonCode) ?? string.Empty,
            NormalizeOptional(request.EvidenceSummary) ?? string.Empty);

    private static string CreateTransferDraftFingerprint(CreateLoadArrTransferOrderRequest request) =>
        string.Join('|',
            NormalizeRequiredOrDefault(request.TransferType, "bin_to_bin"),
            NormalizeRequiredOrDefault(request.FromLocationId, string.Empty),
            NormalizeRequiredOrDefault(request.ToLocationId, string.Empty),
            request.RequestedByPersonId.Trim(),
            NormalizeRequiredOrDefault(request.SupplyarrItemId, string.Empty),
            request.Quantity.ToString(CultureInfo.InvariantCulture),
            NormalizeOptional(request.LotCode) ?? string.Empty,
            NormalizeOptional(request.SerialCode) ?? string.Empty,
            NormalizeRequiredOrDefault(request.ReasonCode, string.Empty));

    private static LoadArrWorkspaceSummaryResponse CreateWorkspaceSummary()
    {
        var generatedAt = DateTimeOffset.UtcNow;

        var locations = new[]
        {
            new LoadArrLocationResponse(
                "loc-dock-01",
                "STL North Yard",
                "staff-site-stl-north",
                "Receiving Dock 1",
                "dock",
                "STL North Yard / Main Warehouse / Dock 1",
                true,
                new[] { "ambient", "forklift" },
                78,
                "Open for receipts and outbound staging"),
            new LoadArrLocationResponse(
                "loc-haz-01",
                "STL North Yard",
                "staff-site-stl-north",
                "Hazmat Cage A",
                "hazmat_cage",
                "STL North Yard / Secure Storage / Hazmat Cage A",
                true,
                new[] { "hazmat", "controlled_access", "inspection_required" },
                63,
                "Authorized staff and current hazmat training required"),
            new LoadArrLocationResponse(
                "loc-quarantine-01",
                "STL North Yard",
                "staff-site-stl-north",
                "Quarantine Bay",
                "quarantine_area",
                "STL North Yard / Quality / Quarantine Bay",
                true,
                new[] { "quality_hold", "blocked" },
                41,
                "Blocked from allocation until investigation closes"),
            new LoadArrLocationResponse(
                "loc-truck-17",
                "South Service Depot",
                "staff-site-south-depot",
                "Truck Stock 17",
                "service_truck",
                "South Service Depot / Mobile Stock / Truck 17",
                true,
                new[] { "mobile_stock", "route_ready" },
                55,
                "Assigned to field maintenance route handoff")
        };

        var inventory = new[]
        {
            new LoadArrInventoryBalanceResponse(
                "bal-valve-kit-a",
                "SUP-VALVE-KIT-A",
                "Valve repair kit A",
                "each",
                "available",
                "loc-dock-01",
                "Receiving Dock 1",
                38,
                11,
                4,
                0,
                "purchase_receipt",
                "PO-10492 / RR-24018",
                new[] { "lot:L2405-77", "supplyarr:part:SUP-VALVE-KIT-A" },
                "Ready for putaway"),
            new LoadArrInventoryBalanceResponse(
                "bal-adhesive-haz",
                "SUP-ADH-49",
                "Regulated adhesive cartridge",
                "case",
                "pending_inspection",
                "loc-haz-01",
                "Hazmat Cage A",
                14,
                0,
                0,
                14,
                "vendor_consignment_receipt",
                "ASN-8834",
                new[] { "hazmat", "sds:current", "lot:ADH-991" },
                "SDS and label check required"),
            new LoadArrInventoryBalanceResponse(
                "bal-brake-rotor",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                "each",
                "reserved",
                "loc-truck-17",
                "Truck Stock 17",
                12,
                2,
                6,
                0,
                "route_return",
                "RoutArr trip RT-7781",
                new[] { "maintainarr:work-order:WO-5530", "truck_stock" },
                "Reserved for maintenance work orders")
        };

        var tasks = new[]
        {
            new LoadArrWarehouseTaskResponse(
                "task-receive-24018",
                "receive",
                "Receive PO-10492",
                "high",
                "ready",
                "Receiving Dock 1",
                "Inventory Clerk",
                "SUP-VALVE-KIT-A",
                38,
                "2026-06-03T15:00:00Z",
                new[] { "purchase_receipt", "packing_slip_attached" }),
            new LoadArrWarehouseTaskResponse(
                "task-inspect-adh-49",
                "quality_inspection",
                "Inspect regulated adhesive lot",
                "urgent",
                "blocked_by_training",
                "Hazmat Cage A",
                "Hazmat-qualified reviewer",
                "SUP-ADH-49",
                14,
                "2026-06-03T18:00:00Z",
                new[] { "hazmat", "trainarr_required", "compliancecore_sds_check" }),
            new LoadArrWarehouseTaskResponse(
                "task-pick-wo-5530",
                "pick",
                "Pick parts for WO-5530",
                "normal",
                "in_progress",
                "Truck Stock 17",
                "Route Stock Lead",
                "SUP-BR-ROTOR-22",
                6,
                "2026-06-04T13:30:00Z",
                new[] { "maintainarr", "route_ready" })
        };

        var holds = new[]
        {
            new LoadArrHoldResponse(
                "hold-adh-49",
                "quality_hold",
                "SUP-ADH-49",
                "Hazmat Cage A",
                "Open",
                "SDS label mismatch requires Compliance Core review",
                "ComplianceCore rule title49.hazmat.labeling",
                "2026-06-02T21:10:00Z"),
            new LoadArrHoldResponse(
                "hold-count-rotor",
                "investigation",
                "SUP-BR-ROTOR-22",
                "Truck Stock 17",
                "Review",
                "Cycle count variance above mobile-stock threshold",
                "Count CC-8021",
                "2026-06-02T19:40:00Z")
        };

        var handoffs = new[]
        {
            new LoadArrRouteHandoffResponse(
                "handoff-rt-7781",
                "RoutArr",
                "RT-7781",
                "Truck Stock 17",
                "ready",
                6,
                "WO-5530 parts staged for mobile maintenance route"),
            new LoadArrRouteHandoffResponse(
                "handoff-out-1204",
                "SupplyArr",
                "OUT-1204",
                "Receiving Dock 1",
                "waiting_on_pick",
                11,
                "Outbound stock movement pending pick confirmation")
        };

        var evidence = new[]
        {
            new LoadArrEvidenceResponse(
                "ev-rr-24018-photo",
                "photo",
                "Receiving Dock 1",
                "Dock receipt photo attached to RR-24018",
                "2026-06-02T20:16:00Z",
                "Inventory Clerk"),
            new LoadArrEvidenceResponse(
                "ev-adh-sds-check",
                "rule_evaluation",
                "Hazmat Cage A",
                "SDS and label rule check opened",
                "2026-06-02T21:12:00Z",
                "ComplianceCore"),
            new LoadArrEvidenceResponse(
                "ev-count-8021",
                "cycle_count",
                "Truck Stock 17",
                "Mobile stock variance captured for review",
                "2026-06-02T19:42:00Z",
                "Route Stock Lead")
        };

        var unexplainedInventory = CreateUnexplainedInventoryRecords();

        return new LoadArrWorkspaceSummaryResponse(
            generatedAt,
            new LoadArrWorkspaceMetricsResponse(
                locations.Length,
                inventory.Sum(item => item.QuantityOnHand),
                inventory.Sum(item => item.QuantityReserved + item.QuantityAllocated),
                inventory.Sum(item => item.QuantityBlocked),
                tasks.Count(task => task.Status is "ready" or "in_progress"),
                holds.Count(hold => hold.Status is "Open" or "Review"),
                unexplainedInventory.Count(record => record.Status is "needs_review" or "needs_approval" or "needs_quarantine")),
            locations,
            inventory,
            tasks,
            holds,
            handoffs,
            evidence,
            unexplainedInventory);
    }

    internal static IReadOnlyCollection<LoadArrReceivingSessionResponse> CreateReceivingSessions() =>
        new[]
        {
            new LoadArrReceivingSessionResponse(
                "recv-24018",
                "RCV-24018",
                "purchase_order",
                "open",
                "staff-site-stl-north",
                "STL North Yard",
                "supplyarr",
                "purchase_order",
                "PO-10492",
                "Midwest Fleet Supply",
                "person-inventory-clerk",
                null,
                "2026-06-02T20:10:00Z",
                null,
                new[]
                {
                    new LoadArrReceivingLineResponse(
                        "line-24018-1",
                        "SUP-VALVE-KIT-A",
                        "Valve repair kit A",
                        38,
                        38,
                        "each",
                        "loc-dock-01",
                        "Receiving Dock 1",
                        "L2405-77",
                        null,
                        "new",
                        "ready_to_complete",
                        null,
                        "Dock receipt photo attached")
                }),
            new LoadArrReceivingSessionResponse(
                "recv-8834",
                "RCV-8834",
                "vendor_consignment",
                "inspection_required",
                "staff-site-stl-north",
                "STL North Yard",
                "supplyarr",
                "asn",
                "ASN-8834",
                "Applied Chemical Partners",
                "person-hazmat-reviewer",
                null,
                "2026-06-02T21:05:00Z",
                null,
                new[]
                {
                    new LoadArrReceivingLineResponse(
                        "line-8834-1",
                        "SUP-ADH-49",
                        "Regulated adhesive cartridge",
                        14,
                        14,
                        "case",
                        "loc-haz-01",
                        "Hazmat Cage A",
                        "ADH-991",
                        null,
                        "pending_inspection",
                        "blocked_by_compliance",
                        "label_mismatch",
                        "SDS and label check opened")
                })
        };

    private static IReadOnlyCollection<LoadArrTransferOrderResponse> CreateTransferOrders() =>
        new[]
        {
            new LoadArrTransferOrderResponse(
                "xfer-24018-putaway",
                "TRF-24018",
                "ready",
                "bin_to_bin",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "loc-quarantine-01",
                "Quarantine Bay",
                "person-inventory-clerk",
                null,
                "quality_inspection",
                "2026-06-03T14:15:00Z",
                null,
                new[]
                {
                    new LoadArrTransferLineResponse(
                        "xfer-line-24018",
                        "SUP-VALVE-KIT-A",
                        "Valve repair kit A",
                        4,
                        "each",
                        "L2405-77",
                        null,
                        "ready")
                }),
            new LoadArrTransferOrderResponse(
                "xfer-truck-17",
                "TRF-TRUCK-17",
                "completed",
                "warehouse_to_truck",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "loc-dock-01",
                "Receiving Dock 1",
                "person-route-stock-lead",
                "person-route-stock-lead",
                "route_replenishment",
                "2026-06-02T18:00:00Z",
                "2026-06-02T18:35:00Z",
                new[]
                {
                    new LoadArrTransferLineResponse(
                        "xfer-line-truck-17",
                        "SUP-BR-ROTOR-22",
                        "Brake rotor assembly",
                        2,
                        "each",
                        null,
                        "BR-SN-7781",
                        "completed")
                })
        };

    private static IReadOnlyCollection<LoadArrInventoryHoldResponse> CreateInventoryHolds() =>
        new[]
        {
            new LoadArrInventoryHoldResponse(
                "hold-adh-49",
                "HLD-ADH-49",
                "open",
                "quality",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-haz-01",
                "Hazmat Cage A",
                "SUP-ADH-49",
                "Regulated adhesive cartridge",
                "bal-adhesive-haz",
                14,
                "case",
                "sds_label_mismatch",
                "SDS label mismatch requires Compliance Core review",
                "person-hazmat-reviewer",
                null,
                "cc-eval-adh-49",
                "SDS and label rule check opened",
                "2026-06-02T21:10:00Z",
                null),
            new LoadArrInventoryHoldResponse(
                "hold-count-rotor",
                "HLD-ROTOR-22",
                "review",
                "investigation",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                "bal-brake-rotor",
                2,
                "each",
                "cycle_count_variance",
                "Cycle count variance above mobile-stock threshold",
                "person-route-stock-lead",
                null,
                null,
                "Mobile stock variance captured for review",
                "2026-06-02T19:40:00Z",
                null)
        };

    private static IReadOnlyCollection<LoadArrUnexplainedInventoryRecordResponse> CreateUnexplainedInventoryRecords() =>
        new[]
        {
            new LoadArrUnexplainedInventoryRecordResponse(
                "unexplained-count-8021",
                "UNX-8021",
                "needs_approval",
                "cycle_count_variance",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                10,
                12,
                2,
                "each",
                null,
                "BR-SN-7781",
                "person-route-stock-lead",
                "cycle_count_variance",
                "Positive mobile-stock variance captured; stock is not trusted available until supervisor approval.",
                null,
                "not_trusted_available",
                "2026-06-02T19:45:00Z",
                null),
            new LoadArrUnexplainedInventoryRecordResponse(
                "unexplained-dock-adh",
                "UNX-ADH-49",
                "needs_quarantine",
                "damaged_freight_receipt",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-quarantine-01",
                "Quarantine Bay",
                "SUP-ADH-49",
                "Regulated adhesive cartridge",
                0,
                1,
                1,
                "case",
                "ADH-991",
                null,
                "person-inventory-clerk",
                "unknown_origin_review",
                "One extra case found beside freight paperwork; moved to StaffArr quarantine location for review.",
                "cc-eval-adh-extra",
                "quarantined_untrusted",
                "2026-06-02T20:50:00Z",
                null)
        };

    private static IReadOnlyCollection<LoadArrSupplyArrItemReferenceResponse> CreateSupplyArrItemReferences() =>
        new[]
        {
            new LoadArrSupplyArrItemReferenceResponse(
                "SUP-VALVE-KIT-A",
                "VALVE-KIT-A",
                "Valve repair kit A",
                "each",
                "maintenance_part",
                true,
                false,
                false,
                false,
                "2026-06-01T12:00:00Z"),
            new LoadArrSupplyArrItemReferenceResponse(
                "SUP-ADH-49",
                "ADH-49",
                "Regulated adhesive cartridge",
                "case",
                "regulated_consumable",
                true,
                false,
                true,
                true,
                "2026-06-01T12:00:00Z"),
            new LoadArrSupplyArrItemReferenceResponse(
                "SUP-BR-ROTOR-22",
                "BR-ROTOR-22",
                "Brake rotor assembly",
                "each",
                "maintenance_part",
                false,
                true,
                false,
                false,
                "2026-06-01T12:00:00Z")
        };
}

public sealed record LoadArrWorkspaceSummaryResponse(
    DateTimeOffset GeneratedAt,
    LoadArrWorkspaceMetricsResponse Metrics,
    IReadOnlyCollection<LoadArrLocationResponse> Locations,
    IReadOnlyCollection<LoadArrInventoryBalanceResponse> Inventory,
    IReadOnlyCollection<LoadArrWarehouseTaskResponse> Tasks,
    IReadOnlyCollection<LoadArrHoldResponse> Holds,
    IReadOnlyCollection<LoadArrRouteHandoffResponse> RouteHandoffs,
    IReadOnlyCollection<LoadArrEvidenceResponse> Evidence,
    IReadOnlyCollection<LoadArrUnexplainedInventoryRecordResponse> UnexplainedInventory);

public sealed record LoadArrListResponse<TItem>(
    IReadOnlyCollection<TItem> Items,
    int Total);

public sealed record LoadArrSiteSourceResponse(
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string Status,
    bool Active,
    string Notes);

public sealed record LoadArrLocationTreeNodeResponse(
    string Id,
    string Label,
    string NodeType,
    string? LocationId,
    IReadOnlyCollection<LoadArrLocationTreeNodeResponse> Children);

public sealed record LoadArrSupplyArrItemReferenceResponse(
    string SupplyarrItemId,
    string ItemNumberSnapshot,
    string ItemNameSnapshot,
    string UnitOfMeasureSnapshot,
    string ItemTypeSnapshot,
    bool IsLotControlled,
    bool IsSerialControlled,
    bool IsHazardous,
    bool RequiresSds,
    string UpdatedAtUtc,
    bool RequiresTraceabilityCapture = false);

public sealed record LoadArrWorkspaceMetricsResponse(
    int ActiveLocations,
    decimal QuantityOnHand,
    decimal QuantityCommitted,
    decimal QuantityBlocked,
    int OpenTasks,
    int OpenHolds,
    int UnexplainedInventory);

public sealed record LoadArrLocationResponse(
    string Id,
    string StaffarrSiteNameSnapshot,
    string StaffarrSiteOrgUnitId,
    string Name,
    string LocationType,
    string Path,
    bool Active,
    IReadOnlyCollection<string> ComplianceRestrictions,
    int CapacityPercent,
    string Notes);

public sealed record LoadArrInventoryBalanceResponse(
    string Id,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    string UnitOfMeasureSnapshot,
    string State,
    string LocationId,
    string LocationNameSnapshot,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAllocated,
    decimal QuantityBlocked,
    string OriginEventType,
    string OriginReference,
    IReadOnlyCollection<string> TraceTags,
    string Notes);

public sealed record LoadArrWarehouseTaskResponse(
    string Id,
    string TaskType,
    string Title,
    string Priority,
    string Status,
    string LocationNameSnapshot,
    string AssignedRole,
    string SupplyarrItemId,
    decimal Quantity,
    string DueAtUtc,
    IReadOnlyCollection<string> RequiredSignals);

public sealed record LoadArrReceivingSessionResponse(
    string Id,
    string ReceivingNumber,
    string ReceivingType,
    string Status,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string SourceProductKey,
    string SourceObjectType,
    string SourceObjectId,
    string SupplierNameSnapshot,
    string StartedByPersonId,
    string? CompletedByPersonId,
    string StartedAtUtc,
    string? CompletedAtUtc,
    IReadOnlyCollection<LoadArrReceivingLineResponse> Lines);

public sealed record LoadArrReceivingLineResponse(
    string Id,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal ExpectedQuantity,
    decimal ReceivedQuantity,
    string UnitOfMeasure,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string? LotCode,
    string? SerialCode,
    string Condition,
    string Status,
    string? DiscrepancyReasonCode,
    string? EvidenceSummary);

public sealed record CreateLoadArrReceivingSessionRequest(
    string? ClientRequestId,
    string ReceivingType,
    string SourceProductKey,
    string SourceObjectType,
    string SourceObjectId,
    string SupplierNameSnapshot,
    string StartedByPersonId,
    string SupplyarrItemId,
    decimal ExpectedQuantity,
    decimal ReceivedQuantity,
    string WarehouseLocationId,
    string? LotCode,
    string? SerialCode,
    string Condition,
    string? DiscrepancyReasonCode,
    string? EvidenceSummary);

public sealed record AddLoadArrReceivingLineRequest(
    string SupplyarrItemId,
    decimal ExpectedQuantity,
    decimal ReceivedQuantity,
    string WarehouseLocationId,
    string? LotCode,
    string? SerialCode,
    string Condition,
    string? DiscrepancyReasonCode,
    string? EvidenceSummary);

public sealed record CompleteLoadArrReceivingSessionRequest(
    string ReceivingType,
    string SourceProductKey,
    string SourceObjectType,
    string SourceObjectId,
    string SupplierNameSnapshot,
    string CompletedByPersonId,
    string SupplyarrItemId,
    decimal ExpectedQuantity,
    decimal ReceivedQuantity,
    string WarehouseLocationId,
    string? LotCode,
    string? SerialCode,
    string Condition,
    string? DiscrepancyReasonCode,
    string? ComplianceEvaluationId,
    string? EvidenceSummary);

public sealed record CancelLoadArrReceivingSessionRequest(
    string CanceledByPersonId,
    string ReasonCode,
    string? Notes);

public interface ILoadArrTransferMutationRequest
{
    string TransferType { get; }
    string FromLocationId { get; }
    string ToLocationId { get; }
    string SupplyarrItemId { get; }
    decimal Quantity { get; }
    string ReasonCode { get; }
}

public sealed record LoadArrTransferOrderResponse(
    string Id,
    string TransferNumber,
    string Status,
    string TransferType,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string FromLocationId,
    string FromLocationNameSnapshot,
    string ToLocationId,
    string ToLocationNameSnapshot,
    string RequestedByPersonId,
    string? CompletedByPersonId,
    string ReasonCode,
    string CreatedAtUtc,
    string? CompletedAtUtc,
    IReadOnlyCollection<LoadArrTransferLineResponse> Lines);

public sealed record LoadArrTransferLineResponse(
    string Id,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    string? LotCode,
    string? SerialCode,
    string Status);

public sealed record CreateLoadArrTransferOrderRequest(
    string? ClientRequestId,
    string TransferType,
    string FromLocationId,
    string ToLocationId,
    string RequestedByPersonId,
    string SupplyarrItemId,
    decimal Quantity,
    string? LotCode,
    string? SerialCode,
    string ReasonCode) : ILoadArrTransferMutationRequest;

public sealed record CompleteLoadArrTransferOrderRequest(
    string TransferType,
    string FromLocationId,
    string ToLocationId,
    string CompletedByPersonId,
    string SupplyarrItemId,
    decimal Quantity,
    string? LotCode,
    string? SerialCode,
    string ReasonCode,
    string? ComplianceEvaluationId,
    string? EvidenceSummary) : ILoadArrTransferMutationRequest;

public sealed record CancelLoadArrTransferOrderRequest(
    string CanceledByPersonId,
    string ReasonCode,
    string? Notes);

public sealed record LoadArrTransferCompletionResponse(
    LoadArrTransferOrderResponse Transfer,
    LoadArrInventoryMovementResponse Movement,
    LoadArrInventoryBalanceResponse SourceBalance,
    LoadArrInventoryBalanceResponse DestinationBalance,
    LoadArrWarehouseTaskResponse TransferTask);

public sealed record LoadArrInventoryHoldResponse(
    string Id,
    string HoldNumber,
    string Status,
    string HoldType,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    string InventoryBalanceId,
    decimal Quantity,
    string UnitOfMeasure,
    string ReasonCode,
    string Description,
    string CreatedByPersonId,
    string? ReleasedByPersonId,
    string? ComplianceEvaluationId,
    string? EvidenceSummary,
    string CreatedAtUtc,
    string? ReleasedAtUtc);

public sealed record CreateLoadArrInventoryHoldRequest(
    string HoldType,
    string WarehouseLocationId,
    string SupplyarrItemId,
    decimal Quantity,
    string ReasonCode,
    string Description,
    string CreatedByPersonId,
    string? ComplianceEvaluationId,
    string? EvidenceSummary);

public sealed record ReleaseLoadArrInventoryHoldRequest(
    string ReleasedByPersonId,
    string ReasonCode,
    string? EvidenceSummary);

public sealed record LoadArrHoldMutationResponse(
    LoadArrInventoryHoldResponse Hold,
    LoadArrInventoryMovementResponse Movement,
    LoadArrInventoryBalanceResponse Balance);

public sealed record LoadArrUnexplainedInventoryRecordResponse(
    string Id,
    string RecordNumber,
    string Status,
    string DiscoverySource,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal ExpectedQuantity,
    decimal Quantity,
    decimal VarianceQuantity,
    string UnitOfMeasure,
    string? LotCode,
    string? SerialCode,
    string DiscoveredByPersonId,
    string ReasonCode,
    string EvidenceSummary,
    string? ComplianceEvaluationId,
    string ResolutionState,
    string DiscoveredAtUtc,
    string? ResolvedAtUtc);

public sealed record CreateLoadArrUnexplainedInventoryRequest(
    string DiscoverySource,
    string WarehouseLocationId,
    string SupplyarrItemId,
    decimal ExpectedQuantity,
    decimal Quantity,
    string? LotCode,
    string? SerialCode,
    string DiscoveredByPersonId,
    string ReasonCode,
    string EvidenceSummary,
    string? ComplianceEvaluationId);

public sealed record ResolveLoadArrUnexplainedInventoryRequest(
    string ApprovedByPersonId,
    string ReasonCode,
    string? ComplianceEvaluationId,
    string? EvidenceSummary);

public sealed record QuarantineLoadArrUnexplainedInventoryRequest(
    string QuarantineLocationId,
    string QuarantinedByPersonId,
    string ReasonCode,
    string? EvidenceSummary);

public sealed record ScrapLoadArrUnexplainedInventoryRequest(
    string ScrappedByPersonId,
    string ReasonCode,
    string? EvidenceSummary);

public sealed record LoadArrUnexplainedInventoryMutationResponse(
    LoadArrUnexplainedInventoryRecordResponse Record,
    LoadArrInventoryOriginEventResponse? OriginEvent,
    LoadArrInventoryMovementResponse? Movement,
    LoadArrWarehouseTaskResponse? ReviewTask);

public sealed record LoadArrReceivingCompletionResponse(
    LoadArrReceivingSessionResponse Session,
    LoadArrInventoryOriginEventResponse OriginEvent,
    LoadArrInventoryMovementResponse Movement,
    LoadArrInventoryBalanceResponse Balance,
    LoadArrWarehouseTaskResponse PutawayTask);

public sealed record LoadArrInventoryOriginEventResponse(
    string Id,
    string OriginType,
    string OriginProductKey,
    string OriginObjectType,
    string OriginObjectId,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    string? LotCode,
    string? SerialCode,
    string Condition,
    string Status,
    string PersonId,
    string? ComplianceEvaluationId,
    string? EvidenceSummary,
    string CreatedAtUtc);

public sealed record LoadArrInventoryMovementResponse(
    string Id,
    string MovementType,
    string StaffarrSiteOrgUnitId,
    string? FromLocationId,
    string ToLocationId,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    string? StatusBefore,
    string StatusAfter,
    string RelatedProductKey,
    string RelatedObjectType,
    string RelatedObjectId,
    string ReasonCode,
    string PersonId,
    string? InventoryOriginEventId,
    string CreatedAtUtc);

public sealed record LoadArrHoldResponse(
    string Id,
    string HoldType,
    string SupplyarrItemId,
    string LocationNameSnapshot,
    string Status,
    string Reason,
    string SourceReference,
    string OpenedAtUtc);

public sealed record LoadArrRouteHandoffResponse(
    string Id,
    string TargetProduct,
    string TargetReference,
    string LocationNameSnapshot,
    string Status,
    decimal Quantity,
    string Notes);

public sealed record LoadArrEvidenceResponse(
    string Id,
    string EvidenceType,
    string LocationNameSnapshot,
    string Summary,
    string CapturedAtUtc,
    string CapturedBy);

public sealed record LoadArrProblemResponse(
    string ErrorCode,
    string Message);
