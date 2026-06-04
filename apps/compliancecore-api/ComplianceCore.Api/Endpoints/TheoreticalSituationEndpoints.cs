using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class TheoreticalSituationEndpoints
{
    public static void MapComplianceCoreTheoreticalSituationEndpoints(this WebApplication app)
    {
        var situations = app.MapGroup("/api/v1/theoretical-situations")
            .WithTags("TheoreticalSituations")
            .RequireAuthorization();

        situations.MapGet("/options/situation-kinds", async (
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetSituationKindsAsync());
        })
        .WithName("GetTheoreticalSituationKindsV1");

        situations.MapGet("/options/context-fields", async (
            string? situationKind,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetContextFieldsAsync(situationKind));
        })
        .WithName("GetTheoreticalContextFieldsV1");

        situations.MapGet("/options/context-values", async (
            string contextKey,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetContextValuesAsync(contextKey));
        })
        .WithName("GetTheoreticalContextValuesV1");

        situations.MapGet("/options/document-types", async (
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetDocumentTypesAsync(context.User.GetTenantId(), cancellationToken));
        })
        .WithName("GetTheoreticalDocumentTypesV1");

        situations.MapGet("/options/evidence-states", async (
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetEvidenceStatesAsync());
        })
        .WithName("GetTheoreticalEvidenceStatesV1");

        situations.MapGet("/options/evidence-options", async (
            string? requirementKey,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetEvidenceOptionsAsync(context.User.GetTenantId(), requirementKey, cancellationToken));
        })
        .WithName("GetTheoreticalEvidenceOptionsV1");

        situations.MapGet("/options/exception-exemptions", async (
            string? requirementKey,
            string? packKey,
            string? citationKey,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetExceptionExemptionOptionsAsync(
                context.User.GetTenantId(),
                requirementKey,
                packKey,
                citationKey,
                cancellationToken));
        })
        .WithName("GetTheoreticalExceptionExemptionOptionsV1");

        situations.MapGet("/options/incidents", async (
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetIncidentOptionsAsync());
        })
        .WithName("GetTheoreticalIncidentOptionsV1");

        situations.MapGet("/options/material-classes", async (
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetMaterialClassesAsync());
        })
        .WithName("GetTheoreticalMaterialClassesV1");

        situations.MapGet("/options/systems", async (
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetSystemOptionsAsync(context.User.GetTenantId(), cancellationToken));
        })
        .WithName("GetTheoreticalSystemsV1");

        situations.MapGet("/options/parts", async (
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetPartOptionsAsync(context.User.GetTenantId(), cancellationToken));
        })
        .WithName("GetTheoreticalPartsV1");

        situations.MapGet("/options/assets", async (
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetAssetOptionsAsync(context.User.GetTenantId(), cancellationToken));
        })
        .WithName("GetTheoreticalAssetsV1");

        situations.MapPost("/", async (
            CreateTheoreticalSituationRequest request,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationCreate(context.User);
            return Results.Ok(await service.CreateAsync(context.User.GetTenantId(), context.User.GetPersonId(), request, cancellationToken));
        })
        .WithName("CreateTheoreticalSituationV1");

        situations.MapGet("/", async (
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.ListAsync(context.User.GetTenantId(), cancellationToken));
        })
        .WithName("ListTheoreticalSituationsV1");

        situations.MapGet("/{id:guid}", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("GetTheoreticalSituationV1");

        situations.MapPatch("/{id:guid}", async (
            Guid id,
            UpdateTheoreticalSituationRequest request,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationCreate(context.User);
            return Results.Ok(await service.UpdateAsync(context.User.GetTenantId(), id, request, cancellationToken));
        })
        .WithName("UpdateTheoreticalSituationV1");

        situations.MapDelete("/{id:guid}", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationCreate(context.User);
            await service.DeleteAsync(context.User.GetTenantId(), id, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteTheoreticalSituationV1");

        situations.MapGet("/{id:guid}/options/next-context", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetNextContextAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("GetTheoreticalSituationNextContextV1");

        situations.MapPost("/{id:guid}/context", async (
            Guid id,
            TheoreticalSituationContextRequest request,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationCreate(context.User);
            return Results.Ok(await service.UpsertContextAsync(context.User.GetTenantId(), id, request, cancellationToken));
        })
        .WithName("SetTheoreticalSituationContextV1");

        situations.MapPost("/{id:guid}/facts", async (
            Guid id,
            TheoreticalSituationFactRequest request,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationCreate(context.User);
            return Results.Ok(await service.UpsertFactsAsync(context.User.GetTenantId(), id, request, cancellationToken));
        })
        .WithName("SetTheoreticalSituationFactsV1");

        situations.MapPost("/{id:guid}/incidents", async (
            Guid id,
            TheoreticalSituationIncidentRequest request,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationCreate(context.User);
            return Results.Ok(await service.UpsertIncidentsAsync(context.User.GetTenantId(), id, request, cancellationToken));
        })
        .WithName("SetTheoreticalSituationIncidentsV1");

        situations.MapPost("/{id:guid}/resolve-applicability", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationEvaluate(context.User);
            return Results.Ok(await service.ResolveApplicabilityAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("ResolveTheoreticalSituationApplicabilityV1");

        situations.MapGet("/{id:guid}/applicability-results", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetApplicabilityResultsAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("GetTheoreticalSituationApplicabilityResultsV1");

        situations.MapPost("/{id:guid}/evaluate", async (
            Guid id,
            TheoreticalEvaluateRequest request,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationEvaluate(context.User);
            return Results.Ok(await service.EvaluateAsync(context.User.GetTenantId(), id, context.User.GetPersonId(), request, cancellationToken));
        })
        .WithName("EvaluateTheoreticalSituationV1");

        situations.MapGet("/{id:guid}/evaluations", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.ListEvaluationsAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("ListTheoreticalSituationEvaluationsV1");

        situations.MapGet("/{id:guid}/evaluations/{evaluationId:guid}", async (
            Guid id,
            Guid evaluationId,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetEvaluationAsync(context.User.GetTenantId(), id, evaluationId, cancellationToken));
        })
        .WithName("GetTheoreticalSituationEvaluationV1");

        situations.MapGet("/{id:guid}/simulation-report", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationRead(context.User);
            return Results.Ok(await service.GetAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("GetTheoreticalSituationSimulationReportV1");

        situations.MapPost("/{id:guid}/save-template", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationTemplateCreate(context.User);
            return Results.Ok(await service.SaveTemplateAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("SaveTheoreticalSituationTemplateV1");

        situations.MapPost("/from-template/{templateId:guid}", async (
            Guid templateId,
            ComplianceCoreAuthorizationService authorization,
            TheoreticalSituationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSimulationCreate(context.User);
            return Results.Ok(await service.CreateFromTemplateAsync(context.User.GetTenantId(), context.User.GetPersonId(), templateId, cancellationToken));
        })
        .WithName("CreateTheoreticalSituationFromTemplateV1");
    }
}
