using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class ExceptionExemptionEndpoints
{
    public static void MapComplianceCoreExceptionExemptionEndpoints(this WebApplication app)
    {
        var exceptions = app.MapGroup("/api/v1/exception-exemptions")
            .WithTags("ExceptionExemptions")
            .RequireAuthorization();

        exceptions.MapGet("/", async (
            string? type,
            string? packKey,
            string? citationKey,
            bool? includeInactive,
            ComplianceCoreAuthorizationService authorization,
            ComplianceExceptionExemptionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            return Results.Ok(await service.ListAsync(
                context.User.GetTenantId(),
                type,
                packKey,
                citationKey,
                includeInactive ?? false,
                cancellationToken));
        })
        .WithName("ListExceptionExemptionsV1");

        exceptions.MapGet("/options/types", async (
            ComplianceCoreAuthorizationService authorization,
            ComplianceExceptionExemptionService service,
            HttpContext context) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            return Results.Ok(await service.GetTypeOptionsAsync());
        })
        .WithName("ListExceptionExemptionTypesV1");

        exceptions.MapGet("/options/effects", async (
            ComplianceCoreAuthorizationService authorization,
            ComplianceExceptionExemptionService service,
            HttpContext context) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            return Results.Ok(await service.GetEffectOptionsAsync());
        })
        .WithName("ListExceptionExemptionEffectsV1");

        exceptions.MapGet("/{id:guid}", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            ComplianceExceptionExemptionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            return Results.Ok(await service.GetAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("GetExceptionExemptionV1");

        exceptions.MapPost("/", async (
            CreateComplianceExceptionExemptionRequest request,
            ComplianceCoreAuthorizationService authorization,
            ComplianceExceptionExemptionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryManage(context.User);
            var created = await service.CreateAsync(
                context.User.GetTenantId(),
                context.User.GetPersonId(),
                request,
                cancellationToken);
            return Results.Created($"/api/v1/exception-exemptions/{created.ExceptionExemptionId}", created);
        })
        .WithName("CreateExceptionExemptionV1");

        exceptions.MapPatch("/{id:guid}", async (
            Guid id,
            UpdateComplianceExceptionExemptionRequest request,
            ComplianceCoreAuthorizationService authorization,
            ComplianceExceptionExemptionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryManage(context.User);
            return Results.Ok(await service.UpdateAsync(
                context.User.GetTenantId(),
                id,
                context.User.GetPersonId(),
                request,
                cancellationToken));
        })
        .WithName("UpdateExceptionExemptionV1");

        exceptions.MapDelete("/{id:guid}", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            ComplianceExceptionExemptionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryManage(context.User);
            await service.DeactivateAsync(
                context.User.GetTenantId(),
                id,
                context.User.GetPersonId(),
                cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeactivateExceptionExemptionV1");
    }
}
