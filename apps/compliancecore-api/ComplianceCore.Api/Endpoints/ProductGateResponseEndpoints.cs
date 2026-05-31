using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Endpoints;

public static class ProductGateResponseEndpoints
{
    public static void MapComplianceCoreProductGateResponseEndpoints(this WebApplication app)
    {
        app.MapPost("/api/gates/responses", async (
            CreateProductGateResponseRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            ProductGateResponseService service,
            CancellationToken cancellationToken) =>
        {
            var validated = ValidateServiceToken(tokenValidator, context, request.TenantId);
            return Results.Ok(await service.CreateAsync(request, validated.SourceProductKey, cancellationToken));
        })
        .WithTags("ProductGates")
        .WithName("RecordProductGateResponse");

        app.MapPost("/api/v1/gates/responses", async (
            CreateProductGateResponseRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            ProductGateResponseService service,
            CancellationToken cancellationToken) =>
        {
            var validated = ValidateServiceToken(tokenValidator, context, request.TenantId);
            return Results.Ok(await service.CreateAsync(request, validated.SourceProductKey, cancellationToken));
        })
        .WithTags("ProductGates")
        .WithName("RecordProductGateResponseV1");

        app.MapGet("/api/v1/gates/responses", async (
            Guid checkResultId,
            ComplianceCoreAuthorizationService authorization,
            ProductGateResponseService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, checkResultId, cancellationToken));
        })
        .WithTags("ProductGates")
        .RequireAuthorization()
        .WithName("ListProductGateResponsesV1");

        app.MapGet("/api/v1/gates/events", async (
            Guid? checkResultId,
            DateTimeOffset? from,
            DateTimeOffset? to,
            int? page,
            int? pageSize,
            ComplianceCoreAuthorizationService authorization,
            ProductGateEventService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                checkResultId,
                from,
                to,
                page,
                pageSize,
                cancellationToken));
        })
        .WithTags("ProductGates")
        .RequireAuthorization()
        .WithName("ListProductGateEventsV1");
    }

    private static ValidatedServiceToken ValidateServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        Guid tenantId)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());

        var preview = tokenValidator.TryValidate(bearer);
        if (preview is null)
        {
            throw new StlApiException("auth.service_token_invalid", "Service token is invalid.", 401);
        }

        return tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = preview.SourceProductKey,
                RequiredTargetProduct = "compliancecore",
                TenantId = tenantId,
                RequiredActionScope = ProductGateResponseService.RecordResponseActionScope,
            });
    }
}
