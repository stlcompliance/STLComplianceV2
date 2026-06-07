using System.Text.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Endpoints;

public static class IntegrationEndpoints
{
    public const string SupplyarrDemandStatusIngestActionScope = "maintainarr.demand_status.write";
    public const string SupplyarrIssueEventIngestActionScope = "maintainarr.demand_status.write";
    public const string SupplyarrVendorWorkStatusIngestActionScope = "maintainarr.demand_status.write";
    public const string RoutarrAssetReadinessDispatchActionScope = "maintainarr.asset_readiness.dispatch_gate";
    public const string AssetReadinessReadActionScope = "maintainarr.asset_readiness.read";
    public const string RoutarrEventIngestActionScope = "maintainarr.routarr_events.ingest";
    public const string StaffarrPersonSyncActionScope = "maintainarr.technician_refs.sync";
    public const string AssurarrQualityHoldIngestActionScope = "maintainarr.quality_holds.write";

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

            integrations.MapPost("/asset-readiness-checks", async (
                CreateAssetReadinessCheckRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                AssetReadinessCheckService service,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                var response = await service.CreateAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    request,
                    cancellationToken);
                return Results.Created($"/api/v1/integrations/asset-readiness-checks/{response.AssetReadinessCheckId}", response);
            })
            .WithName($"CheckMaintainArrAssetReadiness{nameSuffix}");

            integrations.MapGet("/assets", async (
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                AssetService assetService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                var assets = await assetService.ListAsync(token.TenantScope ?? Guid.Empty, cancellationToken);
                return Results.Ok(new MaintainArrIntegrationListResponse<AssetResponse>(assets, assets.Count));
            })
            .WithName($"ListMaintainArrIntegrationAssets{nameSuffix}");

            integrations.MapGet("/assets/{assetId}", async (
                string assetId,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                AssetService assetService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(assetId, out var parsedAssetId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "assetId must be a valid identifier."
                    });
                }

                return Results.Ok(await assetService.GetAsync(token.TenantScope ?? Guid.Empty, parsedAssetId, cancellationToken));
            })
            .WithName($"GetMaintainArrIntegrationAsset{nameSuffix}");

            integrations.MapGet("/assets/{assetId}/readiness", async (
                string assetId,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                AssetReadinessService assetReadinessService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(assetId, out var parsedAssetId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "assetId must be a valid identifier."
                    });
                }

                return Results.Ok(await assetReadinessService.GetAsync(token.TenantScope ?? Guid.Empty, parsedAssetId, cancellationToken));
            })
            .WithName($"GetMaintainArrIntegrationAssetReadiness{nameSuffix}");

            integrations.MapGet("/work-orders", async (
                Guid? assetId,
                Guid? defectId,
                string? status,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderService service,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                return Results.Ok(await service.ListAsync(
                    token.TenantScope ?? Guid.Empty,
                    true,
                    null,
                    null,
                    assetId,
                    defectId,
                    status,
                    cancellationToken));
            })
            .WithName($"ListMaintainArrIntegrationWorkOrders{nameSuffix}");

            integrations.MapGet("/work-orders/{workOrderId}", async (
                string workOrderId,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderService workOrderService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(workOrderId, out var parsedWorkOrderId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "workOrderId must be a valid identifier."
                    });
                }

                return Results.Ok(await workOrderService.GetAsync(
                    token.TenantScope ?? Guid.Empty,
                    parsedWorkOrderId,
                    cancellationToken));
            })
            .WithName($"GetMaintainArrIntegrationWorkOrder{nameSuffix}");

            integrations.MapPost("/work-orders", async (
                CreateWorkOrderRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderService workOrderService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                var created = await workOrderService.CreateAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    request,
                    cancellationToken);
                return Results.Created($"/api/v1/integrations/work-orders/{created.WorkOrderId}", created);
            })
            .WithName($"CreateMaintainArrIntegrationWorkOrder{nameSuffix}");

            integrations.MapPost("/work-orders/drafts", async (
                CreateWorkOrderRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderService workOrderService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                var created = await workOrderService.CreateDraftAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    request,
                    cancellationToken);
                return Results.Created($"/api/v1/integrations/work-orders/{created.WorkOrderId}", created);
            })
            .WithName($"CreateMaintainArrIntegrationWorkOrderDraft{nameSuffix}");

            integrations.MapPatch("/work-orders/{workOrderId}/draft", async (
                string workOrderId,
                CreateWorkOrderRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderService workOrderService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(workOrderId, out var parsedWorkOrderId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "workOrderId must be a valid identifier."
                    });
                }

                var updated = await workOrderService.UpdateDraftAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    parsedWorkOrderId,
                    request,
                    cancellationToken);
                return Results.Ok(updated);
            })
            .WithName($"UpdateMaintainArrIntegrationWorkOrderDraft{nameSuffix}");

            integrations.MapPost("/work-orders/{workOrderId}/validate", async (
                string workOrderId,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderService workOrderService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(workOrderId, out var parsedWorkOrderId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "workOrderId must be a valid identifier."
                    });
                }

                return Results.Ok(await workOrderService.ValidateDraftAsync(
                    token.TenantScope ?? Guid.Empty,
                    parsedWorkOrderId,
                    cancellationToken));
            })
            .WithName($"ValidateMaintainArrIntegrationWorkOrderDraft{nameSuffix}");

            integrations.MapPost("/work-orders/{workOrderId}/preview", async (
                string workOrderId,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderService workOrderService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(workOrderId, out var parsedWorkOrderId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "workOrderId must be a valid identifier."
                    });
                }

                return Results.Ok(await workOrderService.PreviewDraftAsync(
                    token.TenantScope ?? Guid.Empty,
                    parsedWorkOrderId,
                    token.TokenId.ToString("D"),
                    cancellationToken));
            })
            .WithName($"PreviewMaintainArrIntegrationWorkOrderDraft{nameSuffix}");

            integrations.MapPost("/work-orders/{workOrderId}/duplicates", async (
                string workOrderId,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderService workOrderService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(workOrderId, out var parsedWorkOrderId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "workOrderId must be a valid identifier."
                    });
                }

                return Results.Ok(await workOrderService.CheckDuplicateDraftAsync(
                    token.TenantScope ?? Guid.Empty,
                    parsedWorkOrderId,
                    cancellationToken));
            })
            .WithName($"CheckMaintainArrIntegrationWorkOrderDraftDuplicates{nameSuffix}");

            integrations.MapPost("/work-orders/{workOrderId}/open", async (
                string workOrderId,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderService workOrderService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(workOrderId, out var parsedWorkOrderId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "workOrderId must be a valid identifier."
                    });
                }

                return Results.Ok(await workOrderService.OpenDraftAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    parsedWorkOrderId,
                    token.TokenId.ToString("D"),
                    cancellationToken));
            })
            .WithName($"OpenMaintainArrIntegrationWorkOrderDraft{nameSuffix}");

            integrations.MapPost("/work-orders/{workOrderId}/schedule", async (
                string workOrderId,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderService workOrderService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(workOrderId, out var parsedWorkOrderId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "workOrderId must be a valid identifier."
                    });
                }

                return Results.Ok(await workOrderService.ScheduleDraftAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    parsedWorkOrderId,
                    token.TokenId.ToString("D"),
                    cancellationToken));
            })
            .WithName($"ScheduleMaintainArrIntegrationWorkOrderDraft{nameSuffix}");

            integrations.MapPost("/work-orders/{workOrderId}/start", async (
                string workOrderId,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderService workOrderService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(workOrderId, out var parsedWorkOrderId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "workOrderId must be a valid identifier."
                    });
                }

                return Results.Ok(await workOrderService.StartDraftAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    parsedWorkOrderId,
                    token.TokenId.ToString("D"),
                    cancellationToken));
            })
            .WithName($"StartMaintainArrIntegrationWorkOrderDraft{nameSuffix}");

            integrations.MapPost("/work-orders/{workOrderId}/status-updates", async (
                string workOrderId,
                UpdateWorkOrderStatusRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderService workOrderService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(workOrderId, out var parsedWorkOrderId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "workOrderId must be a valid identifier."
                    });
                }

                var updated = await workOrderService.UpdateStatusAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    parsedWorkOrderId,
                    request,
                    canCloseAny: true,
                    token.TokenId.ToString("D"),
                    cancellationToken);
                return Results.Ok(updated);
            })
            .WithName($"UpdateMaintainArrIntegrationWorkOrderStatus{nameSuffix}");

            integrations.MapPost("/work-orders/{workOrderId}/blockers", async (
                string workOrderId,
                CreateWorkOrderBlockerRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderService workOrderService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(workOrderId, out var parsedWorkOrderId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "workOrderId must be a valid identifier."
                    });
                }

                var blocker = await workOrderService.CreateBlockerAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    parsedWorkOrderId,
                    request,
                    cancellationToken);
                return Results.Ok(blocker);
            })
            .WithName($"CreateMaintainArrIntegrationWorkOrderBlocker{nameSuffix}");

            integrations.MapPost("/work-orders/{workOrderId}/closeout", async (
                string workOrderId,
                CreateWorkOrderCloseoutRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderService workOrderService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(workOrderId, out var parsedWorkOrderId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "workOrderId must be a valid identifier."
                    });
                }

                var closeout = await workOrderService.CreateCloseoutAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    parsedWorkOrderId,
                    request,
                    cancellationToken);
                return Results.Ok(closeout);
            })
            .WithName($"CloseMaintainArrIntegrationWorkOrder{nameSuffix}");

            integrations.MapPost("/defects", async (
                CreateDefectRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                DefectService defectService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                var created = await defectService.CreateManualAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    request,
                    cancellationToken);
                return Results.Created($"/api/v1/integrations/defects/{created.DefectId}", created);
            })
            .WithName($"CreateMaintainArrIntegrationDefect{nameSuffix}");

            integrations.MapGet("/defects", async (
                Guid? assetId,
                Guid? inspectionRunId,
                string? status,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                DefectService service,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                return Results.Ok(await service.ListAsync(
                    token.TenantScope ?? Guid.Empty,
                    true,
                    null,
                    assetId,
                    inspectionRunId,
                    status,
                    cancellationToken));
            })
            .WithName($"ListMaintainArrIntegrationDefects{nameSuffix}");

            integrations.MapGet("/defects/{defectId}", async (
                string defectId,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                DefectService defectService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(defectId, out var parsedDefectId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "defectId must be a valid identifier."
                    });
                }

                return Results.Ok(await defectService.GetAsync(
                    token.TenantScope ?? Guid.Empty,
                    parsedDefectId,
                    cancellationToken));
            })
            .WithName($"GetMaintainArrIntegrationDefect{nameSuffix}");

            integrations.MapPost("/defects/{defectId}/status-updates", async (
                string defectId,
                UpdateDefectStatusRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                DefectService defectService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(defectId, out var parsedDefectId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "defectId must be a valid identifier."
                    });
                }

                var updated = await defectService.UpdateStatusAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    parsedDefectId,
                    request,
                    cancellationToken);
                return Results.Ok(updated);
            })
            .WithName($"UpdateMaintainArrIntegrationDefectStatus{nameSuffix}");

            integrations.MapPost("/inspections", async (
                StartInspectionRunRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                InspectionRunService inspectionRunService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                var created = await inspectionRunService.StartAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    request,
                    cancellationToken);
                return Results.Created($"/api/v1/integrations/inspections/{created.InspectionRunId}", created);
            })
            .WithName($"CreateMaintainArrIntegrationInspection{nameSuffix}");

            integrations.MapGet("/inspections", async (
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                InspectionRunService service,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                return Results.Ok(await service.ListAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    true,
                    cancellationToken));
            })
            .WithName($"ListMaintainArrIntegrationInspections{nameSuffix}");

            integrations.MapGet("/inspections/{inspectionId}", async (
                string inspectionId,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                InspectionRunService inspectionRunService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(inspectionId, out var parsedInspectionId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "inspectionId must be a valid identifier."
                    });
                }

                return Results.Ok(await inspectionRunService.GetAsync(
                    token.TenantScope ?? Guid.Empty,
                    parsedInspectionId,
                    cancellationToken));
            })
            .WithName($"GetMaintainArrIntegrationInspection{nameSuffix}");

            integrations.MapPost("/inspections/{inspectionId}/answers", async (
                string inspectionId,
                SubmitInspectionRunAnswersRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                InspectionRunService inspectionRunService,
                CancellationToken cancellationToken) =>
            {
                var token = ValidateIntegrationTokenPreview(context, tokenValidator);
                if (!Guid.TryParse(inspectionId, out var parsedInspectionId))
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "inspectionId must be a valid identifier."
                    });
                }

                var updated = await inspectionRunService.SubmitAnswersAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    parsedInspectionId,
                    request,
                    cancellationToken);
                return Results.Ok(updated);
            })
            .WithName($"AnswerMaintainArrIntegrationInspection{nameSuffix}");

            integrations.MapPost("/route-exceptions", async (
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
            .WithName($"ReportMaintainArrIntegrationRouteException{nameSuffix}");

            integrations.MapPost("/quality-holds", async (
                CreateAssetQualityHoldRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                AssetQualityHoldService assetQualityHoldService,
                CancellationToken cancellationToken) =>
            {
                var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString());
                var token = tokenValidator.TryValidate(bearer)
                    ?? throw new StlApiException(
                        "auth.service_token_invalid",
                        "Service token is invalid.",
                        401);

                tokenValidator.ValidateOrThrow(
                    bearer,
                    new ServiceTokenRequirements
                    {
                        ExpectedSourceProduct = "assurarr",
                        RequiredTargetProduct = "maintainarr",
                        TenantId = token.TenantScope ?? Guid.Empty,
                        RequiredActionScope = AssurarrQualityHoldIngestActionScope
                    });

                var hold = await assetQualityHoldService.CreateAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    request,
                    cancellationToken);
                return Results.Ok(hold);
            })
            .WithName($"CreateMaintainArrIntegrationQualityHold{nameSuffix}");

            integrations.MapPost("/quality-hold-releases", async (
                ReleaseAssetQualityHoldRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                AssetQualityHoldService assetQualityHoldService,
                CancellationToken cancellationToken) =>
            {
                var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString());
                var token = tokenValidator.TryValidate(bearer)
                    ?? throw new StlApiException(
                        "auth.service_token_invalid",
                        "Service token is invalid.",
                        401);

                tokenValidator.ValidateOrThrow(
                    bearer,
                    new ServiceTokenRequirements
                    {
                        ExpectedSourceProduct = "assurarr",
                        RequiredTargetProduct = "maintainarr",
                        TenantId = token.TenantScope ?? Guid.Empty,
                        RequiredActionScope = AssurarrQualityHoldIngestActionScope
                    });

                if (request.HoldId == Guid.Empty)
                {
                    return Results.BadRequest(new
                    {
                        code = "integration.validation",
                        message = "holdId must be a valid identifier."
                    });
                }

                var released = await assetQualityHoldService.ReleaseAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    request.HoldId,
                    request,
                    cancellationToken);
                return Results.Ok(released);
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

            integrations.MapPost("/part-demand-status-updates", async (
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
            .WithName($"IngestMaintainArrPartDemandStatusUpdate{nameSuffix}");

            integrations.MapPost("/part-issue-events", async (
                IngestPartIssueEventRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WorkOrderPartsDemandService service,
                CancellationToken cancellationToken) =>
            {
                var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString());
                tokenValidator.ValidateOrThrow(
                    bearer,
                    new ServiceTokenRequirements
                    {
                        ExpectedSourceProduct = "supplyarr",
                        RequiredTargetProduct = "maintainarr",
                        TenantId = request.TenantId,
                        RequiredActionScope = SupplyarrIssueEventIngestActionScope
                    });

                var token = tokenValidator.TryValidate(bearer) ?? throw new StlApiException(
                    "auth.service_token_invalid",
                    "Service token is invalid.",
                    401);

                var response = await service.RecordIssueAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    request,
                    cancellationToken);
                return Results.Ok(response);
            })
            .WithName($"CreateMaintainArrPartIssueEvent{nameSuffix}");

            integrations.MapPost("/supplier-work-status", async (
                IngestSupplierWorkStatusRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                MaintenanceVendorWorkService service,
                CancellationToken cancellationToken) =>
            {
                var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString());
                tokenValidator.ValidateOrThrow(
                    bearer,
                    new ServiceTokenRequirements
                    {
                        ExpectedSourceProduct = "supplyarr",
                        RequiredTargetProduct = "maintainarr",
                        TenantId = request.TenantId,
                        RequiredActionScope = SupplyarrVendorWorkStatusIngestActionScope
                    });

                var token = tokenValidator.TryValidate(bearer) ?? throw new StlApiException(
                    "auth.service_token_invalid",
                    "Service token is invalid.",
                    401);

                var response = await service.UpsertAsync(
                    token.TenantScope ?? Guid.Empty,
                    token.TokenId,
                    request,
                    cancellationToken);
                return Results.Ok(response);
            })
            .WithName($"ReportMaintainArrSupplierWorkStatus{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/integrations"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/integrations"), "V1");
    }

    private static Guid ValidateIntegrationToken(HttpContext context, StlServiceTokenValidator tokenValidator)
    {
        return ValidateIntegrationTokenPreview(context, tokenValidator).TenantScope ?? Guid.Empty;
    }

    private static ValidatedServiceToken ValidateIntegrationTokenPreview(
        HttpContext context,
        StlServiceTokenValidator tokenValidator)
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
        return preview;
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
