using SupplyArr.Api.Contracts;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class ProcurementExceptionEndpoints
{
    public static void MapSupplyArrProcurementExceptionEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("ProcurementExceptions").RequireAuthorization();

        group.MapGet("/resolution-templates", (
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            HttpContext context) =>
        {
            authorization.RequirePurchaseRequestRead(context.User);
            return Results.Ok(service.ListResolutionTemplates());
        })
        .WithName($"ListProcurementExceptionResolutionTemplates{nameSuffix}");

        group.MapGet("/", async (
            string? status,
            string? subjectType,
            Guid? subjectId,
            bool? overdueOnly,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                status,
                subjectType,
                subjectId,
                overdueOnly ?? false,
                cancellationToken));
        })
        .WithName($"ListProcurementExceptions{nameSuffix}");

        group.MapGet("/{exceptionId:guid}", async (
            Guid exceptionId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, exceptionId, cancellationToken));
        })
        .WithName($"GetProcurementException{nameSuffix}");

        group.MapPut("/{exceptionId:guid}", async (
            Guid exceptionId,
            UpdateProcurementExceptionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(tenantId, actorUserId, exceptionId, request, cancellationToken));
        })
        .WithName($"UpdateProcurementException{nameSuffix}");

        group.MapPost("/{exceptionId:guid}/assign", async (
            Guid exceptionId,
            AssignProcurementExceptionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.AssignAsync(tenantId, actorUserId, exceptionId, request, cancellationToken));
        })
        .WithName($"AssignProcurementException{nameSuffix}");

        group.MapPut("/{exceptionId:guid}/link-actions", async (
            Guid exceptionId,
            LinkProcurementExceptionActionsRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.LinkActionsAsync(tenantId, actorUserId, exceptionId, request, cancellationToken));
        })
        .WithName($"LinkProcurementExceptionActions{nameSuffix}");

        group.MapPost("/{exceptionId:guid}/start-investigation", async (
            Guid exceptionId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.StartInvestigationAsync(tenantId, actorUserId, exceptionId, cancellationToken));
        })
        .WithName($"StartProcurementExceptionInvestigation{nameSuffix}");

        group.MapPost("/{exceptionId:guid}/resolve", async (
            Guid exceptionId,
            ResolveProcurementExceptionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ResolveAsync(tenantId, actorUserId, exceptionId, request, cancellationToken));
        })
        .WithName($"ResolveProcurementException{nameSuffix}");

        group.MapPost("/{exceptionId:guid}/request-waive", async (
            Guid exceptionId,
            RequestProcurementExceptionWaiveRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.RequestWaiveAsync(tenantId, actorUserId, exceptionId, request, cancellationToken));
        })
        .WithName($"RequestProcurementExceptionWaive{nameSuffix}");

        group.MapPost("/{exceptionId:guid}/approve-waive", async (
            Guid exceptionId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestApprove(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ApproveWaiveAsync(tenantId, actorUserId, exceptionId, cancellationToken));
        })
        .WithName($"ApproveProcurementExceptionWaive{nameSuffix}");

        group.MapPost("/{exceptionId:guid}/reject-waive", async (
            Guid exceptionId,
            RejectProcurementExceptionWaiveRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestApprove(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.RejectWaiveAsync(tenantId, actorUserId, exceptionId, request, cancellationToken));
        })
        .WithName($"RejectProcurementExceptionWaive{nameSuffix}");

        group.MapPost("/{exceptionId:guid}/close", async (
            Guid exceptionId,
            CloseProcurementExceptionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CloseAsync(tenantId, actorUserId, exceptionId, request, cancellationToken));
        })
        .WithName($"CloseProcurementException{nameSuffix}");

        group.MapPost("/{exceptionId:guid}/cancel", async (
            Guid exceptionId,
            CancelProcurementExceptionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CancelAsync(tenantId, actorUserId, exceptionId, request, cancellationToken));
        })
        .WithName($"CancelProcurementException{nameSuffix}");

        group.MapPost("/{exceptionId:guid}/reopen", async (
            Guid exceptionId,
            ReopenProcurementExceptionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ReopenAsync(tenantId, actorUserId, exceptionId, request, cancellationToken));
        })
        .WithName($"ReopenProcurementException{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/procurement-exceptions"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/procurement-exceptions"), "V1");

        MapSubjectCreateEndpoint(
            app,
            "/api/purchase-requests/{subjectId:guid}/procurement-exceptions",
            ProcurementExceptionSubjectTypes.PurchaseRequest);

        MapSubjectCreateEndpoint(
            app,
            "/api/purchase-orders/{subjectId:guid}/procurement-exceptions",
            ProcurementExceptionSubjectTypes.PurchaseOrder);

        MapSubjectCreateEndpoint(
            app,
            "/api/rfqs/{subjectId:guid}/procurement-exceptions",
            ProcurementExceptionSubjectTypes.Rfq);

        MapSubjectListEndpoint(
            app,
            "/api/purchase-requests/{subjectId:guid}/procurement-exceptions",
            ProcurementExceptionSubjectTypes.PurchaseRequest);

        MapSubjectListEndpoint(
            app,
            "/api/purchase-orders/{subjectId:guid}/procurement-exceptions",
            ProcurementExceptionSubjectTypes.PurchaseOrder);

        MapSubjectListEndpoint(
            app,
            "/api/rfqs/{subjectId:guid}/procurement-exceptions",
            ProcurementExceptionSubjectTypes.Rfq);

        MapSubjectCreateEndpoint(
            app,
            "/api/v1/purchase-requests/{subjectId:guid}/procurement-exceptions",
            ProcurementExceptionSubjectTypes.PurchaseRequest);

        MapSubjectCreateEndpoint(
            app,
            "/api/v1/purchase-orders/{subjectId:guid}/procurement-exceptions",
            ProcurementExceptionSubjectTypes.PurchaseOrder);

        MapSubjectCreateEndpoint(
            app,
            "/api/v1/rfqs/{subjectId:guid}/procurement-exceptions",
            ProcurementExceptionSubjectTypes.Rfq);

        MapSubjectListEndpoint(
            app,
            "/api/v1/purchase-requests/{subjectId:guid}/procurement-exceptions",
            ProcurementExceptionSubjectTypes.PurchaseRequest);

        MapSubjectListEndpoint(
            app,
            "/api/v1/purchase-orders/{subjectId:guid}/procurement-exceptions",
            ProcurementExceptionSubjectTypes.PurchaseOrder);

        MapSubjectListEndpoint(
            app,
            "/api/v1/rfqs/{subjectId:guid}/procurement-exceptions",
            ProcurementExceptionSubjectTypes.Rfq);
    }

    private static void MapSubjectCreateEndpoint(WebApplication app, string route, string subjectType)
    {
        app.MapPost(route, async (
            Guid subjectId,
            CreateProcurementExceptionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CreateForSubjectAsync(
                tenantId,
                actorUserId,
                subjectType,
                subjectId,
                request,
                cancellationToken));
        })
        .WithTags("ProcurementExceptions")
        .RequireAuthorization();
    }

    private static void MapSubjectListEndpoint(WebApplication app, string route, string subjectType)
    {
        app.MapGet(route, async (
            Guid subjectId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                subjectType: subjectType,
                subjectId: subjectId,
                cancellationToken: cancellationToken));
        })
        .WithTags("ProcurementExceptions")
        .RequireAuthorization();
    }
}
