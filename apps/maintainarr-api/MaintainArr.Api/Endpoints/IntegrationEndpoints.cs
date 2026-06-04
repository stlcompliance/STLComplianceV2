using System.Text.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Endpoints;

public static class IntegrationEndpoints
{
    public const string SupplyarrDemandStatusIngestActionScope = "maintainarr.demand_status.write";
    public const string RoutarrAssetReadinessDispatchActionScope = "maintainarr.asset_readiness.dispatch_gate";
    public const string AssetReadinessReadActionScope = "maintainarr.asset_readiness.read";
    public const string RoutarrEventIngestActionScope = "maintainarr.routarr_events.ingest";
    public const string StaffarrPersonSyncActionScope = "maintainarr.technician_refs.sync";

    public static void MapMaintainArrIntegrationEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder integrations, string nameSuffix)
        {
            integrations = integrations.WithTags("Integrations");

            integrations.MapGet("/", (HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                var tenantId = ValidateIntegrationToken(context, tokenValidator);
                return Results.Ok(new
                {
                    tenantId,
                    items = new[]
                    {
                    new { key = "assets", path = "/api/v1/integrations/assets" },
                    new { key = "assets-by-id", path = "/api/v1/integrations/assets/{assetId}" },
                    new { key = "asset-readiness-by-id", path = "/api/v1/integrations/assets/{assetId}/readiness" },
                        new { key = "asset-readiness-checks", path = "/api/v1/integrations/asset-readiness-checks" },
                        new { key = "work-orders", path = "/api/v1/integrations/work-orders" },
                        new { key = "work-orders-by-id", path = "/api/v1/integrations/work-orders/{workOrderId}" },
                        new { key = "work-order-status-updates", path = "/api/v1/integrations/work-orders/{workOrderId}/status-updates" },
                        new { key = "work-order-blockers", path = "/api/v1/integrations/work-orders/{workOrderId}/blockers" },
                        new { key = "work-order-closeout", path = "/api/v1/integrations/work-orders/{workOrderId}/closeout" },
                        new { key = "defects", path = "/api/v1/integrations/defects" },
                        new { key = "defects-by-id", path = "/api/v1/integrations/defects/{defectId}" },
                        new { key = "defect-status-updates", path = "/api/v1/integrations/defects/{defectId}/status-updates" },
                        new { key = "inspections", path = "/api/v1/integrations/inspections" },
                        new { key = "inspections-by-id", path = "/api/v1/integrations/inspections/{inspectionId}" },
                        new { key = "inspection-answers", path = "/api/v1/integrations/inspections/{inspectionId}/answers" },
                    new { key = "route-exceptions", path = "/api/v1/integrations/route-exceptions" },
                    new { key = "quality-holds", path = "/api/v1/integrations/quality-holds" },
                    new { key = "quality-hold-releases", path = "/api/v1/integrations/quality-hold-releases" },
                    new { key = "part-demand-status-updates", path = "/api/v1/integrations/part-demand-status-updates" },
                    new { key = "part-issue-events", path = "/api/v1/integrations/part-issue-events" },
                    new { key = "supplier-work-status", path = "/api/v1/integrations/supplier-work-status" },
                    }
                });
            })
            .WithName($"ListMaintainArrIntegrationEndpoints{nameSuffix}");

            integrations.MapGet("/routarr-asset-readiness", async (
                Guid tenantId,
                Guid? assetId,
                string? vehicleRefKey,
                string? assetTag,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                AssetReadinessService service,
                CancellationToken cancellationToken) =>
            {
                tokenValidator.ValidateOrThrow(
                    ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                    new ServiceTokenRequirements
                    {
                        ExpectedSourceProduct = "routarr",
                        RequiredTargetProduct = "maintainarr",
                        TenantId = tenantId,
                        RequiredActionScope = RoutarrAssetReadinessDispatchActionScope
                    });

                var result = await service.GetByDispatchRefAsync(
                    tenantId,
                    assetId,
                    vehicleRefKey,
                    assetTag,
                    cancellationToken);
                return Results.Ok(result);
            })
            .WithName($"RoutarrAssetReadinessCheck{nameSuffix}")
            .ExcludeFromDescription();

            integrations.MapGet("/asset-readiness", async (
                Guid tenantId,
                Guid? assetId,
                string? vehicleRefKey,
                string? assetTag,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                AssetReadinessService service,
                CancellationToken cancellationToken) =>
            {
                ValidateAssetReadinessServiceToken(tokenValidator, context, tenantId);

                var result = await service.GetByDispatchRefAsync(
                    tenantId,
                    assetId,
                    vehicleRefKey,
                    assetTag,
                    cancellationToken);
                return Results.Ok(result);
            })
            .WithName($"AssetReadinessIntegration{nameSuffix}")
            .ExcludeFromDescription();

            integrations.MapPost("/asset-readiness-checks", (JsonElement request, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                var tenantId = ValidateIntegrationToken(context, tokenValidator);
                var response = new MaintainArrIntegrationAssetReadinessCheckResponse(
                    $"arc-{Guid.NewGuid():N}"[..13],
                    ReadOptionalString(request, "assetId") ?? "asset-001",
                    ReadOptionalString(request, "sourceProduct") ?? "maintainarr",
                    ReadOptionalString(request, "requestedBy") ?? "system",
                    ReadOptionalString(request, "status") ?? "ready",
                    tenantId);
                return Results.Created($"/api/v1/integrations/asset-readiness-checks/{response.AssetReadinessCheckId}", response);
            })
            .WithName($"CheckMaintainArrAssetReadiness{nameSuffix}");

            integrations.MapGet("/assets", (HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var assets = CreateMaintainArrIntegrationAssets();
                return Results.Ok(new MaintainArrIntegrationListResponse<MaintainArrIntegrationAssetResponse>(assets, assets.Length));
            })
            .WithName($"ListMaintainArrIntegrationAssets{nameSuffix}");

            integrations.MapGet("/assets/{assetId}", (string assetId, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var asset = ResolveIntegrationAsset(assetId);
                return asset is null ? Results.NotFound() : Results.Ok(asset);
            })
            .WithName($"GetMaintainArrIntegrationAsset{nameSuffix}");

            integrations.MapGet("/assets/{assetId}/readiness", (string assetId, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var readiness = ResolveIntegrationAssetReadiness(assetId);
                return readiness is null ? Results.NotFound() : Results.Ok(readiness);
            })
            .WithName($"GetMaintainArrIntegrationAssetReadiness{nameSuffix}");

            integrations.MapGet("/work-orders/{workOrderId}", (string workOrderId, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var workOrder = ResolveIntegrationWorkOrder(workOrderId);
                return workOrder is null ? Results.NotFound() : Results.Ok(workOrder);
            })
            .WithName($"GetMaintainArrIntegrationWorkOrder{nameSuffix}");

            integrations.MapPost("/work-orders", (JsonElement request, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var response = new MaintainArrIntegrationWorkOrderResponse(
                    $"wo-{Guid.NewGuid():N}"[..13],
                    ReadOptionalString(request, "workOrderNumber") ?? "WO-001",
                    ReadOptionalString(request, "status") ?? "requested",
                    ReadOptionalString(request, "sourceProduct") ?? "maintainarr",
                    ReadOptionalString(request, "sourceObjectRef") ?? "obj-001");
                return Results.Created($"/api/v1/integrations/work-orders/{response.WorkOrderId}", response);
            })
            .WithName($"CreateMaintainArrIntegrationWorkOrder{nameSuffix}");

            integrations.MapPost("/work-orders/{workOrderId}/status-updates", (string workOrderId, JsonElement request, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var status = ReadOptionalString(request, "status") ?? "updated";
                var response = new MaintainArrIntegrationWorkOrderStatusUpdateResponse(workOrderId, status, TimestampUtc());
                return Results.Ok(response);
            })
            .WithName($"UpdateMaintainArrIntegrationWorkOrderStatus{nameSuffix}");

            integrations.MapPost("/work-orders/{workOrderId}/blockers", (string workOrderId, JsonElement request, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var blocker = new MaintainArrIntegrationWorkOrderBlockerResponse(
                    $"wb-{Guid.NewGuid():N}"[..13],
                    workOrderId,
                    ReadOptionalString(request, "blockerType") ?? "safety",
                    ReadOptionalString(request, "status") ?? "active",
                    TimestampUtc());
                return Results.Ok(blocker);
            })
            .WithName($"CreateMaintainArrIntegrationWorkOrderBlocker{nameSuffix}");

            integrations.MapPost("/work-orders/{workOrderId}/closeout", (string workOrderId, JsonElement request, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var reason = ReadOptionalString(request, "reason") ?? "completed";
                return Results.Ok(new MaintainArrIntegrationWorkOrderCloseoutResponse(workOrderId, reason, TimestampUtc()));
            })
            .WithName($"CloseMaintainArrIntegrationWorkOrder{nameSuffix}");

            integrations.MapPost("/defects", (JsonElement request, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var response = new MaintainArrIntegrationDefectResponse(
                    $"def-{Guid.NewGuid():N}"[..13],
                    ReadOptionalString(request, "assetId") ?? "asset-001",
                    ReadOptionalString(request, "severity") ?? "medium",
                    ReadOptionalString(request, "status") ?? "open",
                    TimestampUtc());
                return Results.Created($"/api/v1/integrations/defects/{response.DefectId}", response);
            })
            .WithName($"CreateMaintainArrIntegrationDefect{nameSuffix}");

            integrations.MapGet("/defects/{defectId}", (string defectId, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var defect = ResolveIntegrationDefect(defectId);
                return defect is null ? Results.NotFound() : Results.Ok(defect);
            })
            .WithName($"GetMaintainArrIntegrationDefect{nameSuffix}");

            integrations.MapPost("/defects/{defectId}/status-updates", (string defectId, JsonElement request, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var status = ReadOptionalString(request, "status") ?? "investigating";
                return Results.Ok(new MaintainArrIntegrationDefectStatusUpdateResponse(defectId, status, TimestampUtc()));
            })
            .WithName($"UpdateMaintainArrIntegrationDefectStatus{nameSuffix}");

            integrations.MapPost("/inspections", (JsonElement request, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var response = new MaintainArrIntegrationInspectionResponse(
                    $"ins-{Guid.NewGuid():N}"[..13],
                    ReadOptionalString(request, "assetId") ?? "asset-001",
                    ReadOptionalString(request, "status") ?? "scheduled",
                    ReadOptionalString(request, "inspectionType") ?? "routine",
                    TimestampUtc());
                return Results.Created($"/api/v1/integrations/inspections/{response.InspectionId}", response);
            })
            .WithName($"CreateMaintainArrIntegrationInspection{nameSuffix}");

            integrations.MapGet("/inspections/{inspectionId}", (string inspectionId, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var inspection = ResolveIntegrationInspection(inspectionId);
                return inspection is null ? Results.NotFound() : Results.Ok(inspection);
            })
            .WithName($"GetMaintainArrIntegrationInspection{nameSuffix}");

            integrations.MapPost("/inspections/{inspectionId}/answers", (string inspectionId, JsonElement request, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                return Results.Ok(new MaintainArrIntegrationInspectionAnswerResponse(
                    inspectionId,
                    ReadOptionalString(request, "answer") ?? "acknowledged",
                    TimestampUtc()));
            })
            .WithName($"AnswerMaintainArrIntegrationInspection{nameSuffix}");

            integrations.MapPost("/route-exceptions", (JsonElement request, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var response = new MaintainArrIntegrationRouteExceptionResponse(
                    $"re-{Guid.NewGuid():N}"[..14],
                    ReadOptionalString(request, "routeId") ?? "route-001",
                    ReadOptionalString(request, "status") ?? "reported",
                    TimestampUtc());
                return Results.Ok(response);
            })
            .WithName($"ReportMaintainArrIntegrationRouteException{nameSuffix}");

            integrations.MapPost("/quality-holds", (JsonElement request, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var hold = new MaintainArrIntegrationQualityHoldResponse(
                    $"qh-{Guid.NewGuid():N}"[..13],
                    ReadOptionalString(request, "assetId") ?? "asset-001",
                    ReadOptionalString(request, "holdType") ?? "quarantine",
                    ReadOptionalString(request, "status") ?? "active",
                    TimestampUtc());
                return Results.Ok(hold);
            })
            .WithName($"CreateMaintainArrIntegrationQualityHold{nameSuffix}");

            integrations.MapPost("/quality-hold-releases", (JsonElement request, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var holdId = ReadOptionalString(request, "holdId") ?? "qh-001";
                return Results.Ok(new MaintainArrIntegrationQualityHoldReleaseResponse(
                    holdId,
                    "released",
                    TimestampUtc()));
            })
            .WithName($"ReleaseMaintainArrIntegrationQualityHold{nameSuffix}");

            integrations.MapPost("/routarr-events", async (
                IngestRoutarrEventRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                RoutarrEventIngestionService service,
                CancellationToken cancellationToken) =>
            {
                tokenValidator.ValidateOrThrow(
                    ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                    new ServiceTokenRequirements
                    {
                        ExpectedSourceProduct = "routarr",
                        RequiredTargetProduct = "maintainarr",
                        TenantId = request.TenantId,
                        RequiredActionScope = RoutarrEventIngestActionScope
                    });

                var result = await service.IngestAsync(request, cancellationToken);
                return Results.Ok(result);
            })
            .WithName($"IngestRoutarrEvent{nameSuffix}")
            .ExcludeFromDescription();

            integrations.MapPost("/supplyarr-demand-status", async (
                IngestSupplyarrDemandStatusRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderPartsDemandStatusIngestionService service,
                CancellationToken cancellationToken) =>
            {
                tokenValidator.ValidateOrThrow(
                    ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                    new ServiceTokenRequirements
                    {
                        ExpectedSourceProduct = "supplyarr",
                        RequiredTargetProduct = "maintainarr",
                        TenantId = request.TenantId,
                        RequiredActionScope = SupplyarrDemandStatusIngestActionScope
                    });

                var result = await service.IngestAsync(request, cancellationToken);
                return Results.Ok(result);
            })
            .WithName($"IngestSupplyarrDemandStatus{nameSuffix}")
            .ExcludeFromDescription();

            integrations.MapPost("/staffarr-person-sync", async (
                IngestStaffarrPersonSyncRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                StaffarrPersonSyncIngestionService service,
                CancellationToken cancellationToken) =>
            {
                tokenValidator.ValidateOrThrow(
                    ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                    new ServiceTokenRequirements
                    {
                        ExpectedSourceProduct = "staffarr",
                        RequiredTargetProduct = "maintainarr",
                        TenantId = request.TenantId,
                        RequiredActionScope = StaffarrPersonSyncActionScope
                    });

                var result = await service.IngestAsync(request, cancellationToken);
                return Results.Ok(result);
            })
            .WithName($"IngestStaffarrPersonSync{nameSuffix}")
            .ExcludeFromDescription();

            integrations.MapPost("/part-demand-status-updates", (JsonElement request, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var response = new MaintainArrIntegrationPartDemandStatusUpdateResponse(
                    ReadOptionalString(request, "workOrderDemandId") ?? "dmd-001",
                    ReadOptionalString(request, "status") ?? "reserved",
                    TimestampUtc());
                return Results.Ok(response);
            })
            .WithName($"IngestMaintainArrPartDemandStatusUpdate{nameSuffix}");

            integrations.MapPost("/part-issue-events", (JsonElement request, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var response = new MaintainArrIntegrationPartIssueEventResponse(
                    ReadOptionalString(request, "workOrderId") ?? "wo-001",
                    ReadOptionalString(request, "issueType") ?? "issued",
                    ReadOptionalString(request, "itemId") ?? "item-001",
                    TimestampUtc());
                return Results.Ok(response);
            })
            .WithName($"CreateMaintainArrPartIssueEvent{nameSuffix}");

            integrations.MapPost("/supplier-work-status", (JsonElement request, HttpContext context, StlServiceTokenValidator tokenValidator) =>
            {
                ValidateIntegrationToken(context, tokenValidator);
                var response = new MaintainArrIntegrationSupplierWorkStatusResponse(
                    ReadOptionalString(request, "workOrderId") ?? "wo-001",
                    ReadOptionalString(request, "status") ?? "in_progress",
                    TimestampUtc());
                return Results.Ok(response);
            })
            .WithName($"ReportMaintainArrSupplierWorkStatus{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/integrations"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/integrations"), "V1");
    }

    private static Guid ValidateIntegrationToken(HttpContext context, StlServiceTokenValidator tokenValidator)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString());
        var preview = tokenValidator.TryValidate(bearer)
            ?? throw new StlApiException(
                "auth.service_token_invalid",
                "Service token is invalid.",
                401);

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = preview.SourceProductKey,
                RequiredTargetProduct = "maintainarr",
                TenantId = preview.TenantScope ?? Guid.Empty
            });

        return preview.TenantScope ?? Guid.Empty;
    }

    private static void ValidateAssetReadinessServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        Guid tenantId)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());
        var preview = tokenValidator.TryValidate(bearer)
            ?? throw new StlApiException(
                "auth.service_token_invalid",
                "Service token is invalid.",
                401);

        var source = preview.SourceProductKey?.Trim().ToLowerInvariant();
        if (source is not "maintainarr" and not "routarr" and not "staffarr" and not "supplyarr" and not "trainarr" and not "compliancecore")
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for asset readiness reads.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = source,
                RequiredTargetProduct = "maintainarr",
                TenantId = tenantId,
                RequiredActionScope = AssetReadinessReadActionScope
            });
    }

    private static string TimestampUtc() => DateTimeOffset.UtcNow.ToString("O");

    private static string? ReadOptionalString(JsonElement payload, string propertyName)
    {
        return payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static MaintainArrIntegrationAssetResponse[] CreateMaintainArrIntegrationAssets() =>
        new[]
        {
            new MaintainArrIntegrationAssetResponse("asset-001", "Pump-AX-01", "active", "maintenance-bay-01"),
            new MaintainArrIntegrationAssetResponse("asset-002", "Compressor-BX-77", "ready", "maintenance-bay-02")
        };

    private static MaintainArrIntegrationAssetResponse? ResolveIntegrationAsset(string id) =>
        CreateMaintainArrIntegrationAssets().SingleOrDefault(asset =>
            string.Equals(asset.AssetId, id, StringComparison.OrdinalIgnoreCase));

    private static MaintainArrIntegrationAssetReadinessResponse? ResolveIntegrationAssetReadiness(string assetId) =>
        new MaintainArrIntegrationAssetReadinessResponse(
            $"ar-{assetId}",
            assetId,
            "ready",
            Array.Empty<string>());

    private static MaintainArrIntegrationWorkOrderResponse[] CreateMaintainArrIntegrationWorkOrders() =>
        new[]
        {
            new MaintainArrIntegrationWorkOrderResponse("wo-001", "WO-1001", "requested", "maintainarr", "asset-001"),
            new MaintainArrIntegrationWorkOrderResponse("wo-002", "WO-1002", "in_progress", "maintainarr", "asset-002")
        };

    private static MaintainArrIntegrationWorkOrderResponse? ResolveIntegrationWorkOrder(string id) =>
        CreateMaintainArrIntegrationWorkOrders().SingleOrDefault(order =>
            string.Equals(order.WorkOrderId, id, StringComparison.OrdinalIgnoreCase));

    private static MaintainArrIntegrationDefectResponse[] CreateMaintainArrIntegrationDefects() =>
        new[]
        {
            new MaintainArrIntegrationDefectResponse("def-001", "asset-001", "medium", "open", TimestampUtc()),
            new MaintainArrIntegrationDefectResponse("def-002", "asset-002", "high", "in_progress", TimestampUtc())
        };

    private static MaintainArrIntegrationDefectResponse? ResolveIntegrationDefect(string id) =>
        CreateMaintainArrIntegrationDefects().SingleOrDefault(defect =>
            string.Equals(defect.DefectId, id, StringComparison.OrdinalIgnoreCase));

    private static MaintainArrIntegrationInspectionResponse[] CreateMaintainArrIntegrationInspections() =>
        new[]
        {
            new MaintainArrIntegrationInspectionResponse("ins-001", "asset-001", "scheduled", "routine", TimestampUtc()),
            new MaintainArrIntegrationInspectionResponse("ins-002", "asset-002", "completed", "safety", TimestampUtc())
        };

    private static MaintainArrIntegrationInspectionResponse? ResolveIntegrationInspection(string id) =>
        CreateMaintainArrIntegrationInspections().SingleOrDefault(inspection =>
            string.Equals(inspection.InspectionId, id, StringComparison.OrdinalIgnoreCase));
}

public sealed record MaintainArrIntegrationListResponse<TItem>(
    IReadOnlyCollection<TItem> Items,
    int Total);

public sealed record MaintainArrIntegrationAssetResponse(
    string AssetId,
    string AssetName,
    string Status,
    string LocationId);

public sealed record MaintainArrIntegrationAssetReadinessResponse(
    string AssetReadinessId,
    string AssetId,
    string Status,
    IReadOnlyCollection<string> Blockers);

public sealed record MaintainArrIntegrationAssetReadinessCheckResponse(
    string AssetReadinessCheckId,
    string AssetId,
    string SourceProduct,
    string RequestedBy,
    string Status,
    Guid TenantId);

public sealed record MaintainArrIntegrationWorkOrderResponse(
    string WorkOrderId,
    string WorkOrderNumber,
    string Status,
    string SourceProduct,
    string AssetId);

public sealed record MaintainArrIntegrationWorkOrderStatusUpdateResponse(
    string WorkOrderId,
    string Status,
    string UpdatedAtUtc);

public sealed record MaintainArrIntegrationWorkOrderBlockerResponse(
    string BlockerId,
    string WorkOrderId,
    string BlockerType,
    string Status,
    string RecordedAtUtc);

public sealed record MaintainArrIntegrationWorkOrderCloseoutResponse(
    string WorkOrderId,
    string Reason,
    string ClosedAtUtc);

public sealed record MaintainArrIntegrationDefectResponse(
    string DefectId,
    string AssetId,
    string Severity,
    string Status,
    string CreatedAtUtc);

public sealed record MaintainArrIntegrationDefectStatusUpdateResponse(
    string DefectId,
    string Status,
    string UpdatedAtUtc);

public sealed record MaintainArrIntegrationInspectionResponse(
    string InspectionId,
    string AssetId,
    string Status,
    string InspectionType,
    string UpdatedAtUtc);

public sealed record MaintainArrIntegrationInspectionAnswerResponse(
    string InspectionId,
    string Answer,
    string AnsweredAtUtc);

public sealed record MaintainArrIntegrationRouteExceptionResponse(
    string RouteExceptionId,
    string RouteId,
    string Status,
    string ReportedAtUtc);

public sealed record MaintainArrIntegrationQualityHoldResponse(
    string HoldId,
    string AssetId,
    string HoldType,
    string Status,
    string CreatedAtUtc);

public sealed record MaintainArrIntegrationQualityHoldReleaseResponse(
    string HoldId,
    string Status,
    string ReleasedAtUtc);

public sealed record MaintainArrIntegrationPartDemandStatusUpdateResponse(
    string PartDemandId,
    string Status,
    string UpdatedAtUtc);

public sealed record MaintainArrIntegrationPartIssueEventResponse(
    string WorkOrderId,
    string IssueType,
    string ItemId,
    string IssuedAtUtc);

public sealed record MaintainArrIntegrationSupplierWorkStatusResponse(
    string WorkOrderId,
    string Status,
    string UpdatedAtUtc);
