using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class QuestionnaireEndpoints
{
    public static void MapComplianceCoreQuestionnaireEndpoints(this WebApplication app)
    {
        var questionnaires = app.MapGroup("/api/v1/questionnaires")
            .WithTags("Questionnaires")
            .RequireAuthorization();

        questionnaires.MapPost("/resolve", async (
            QuestionnaireResolveRequest request,
            ComplianceCoreAuthorizationService authorization,
            QuestionnaireService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireComplianceCoreRuntimeAccess(context.User);
            return Results.Ok(await service.ResolveAsync(context.User.GetTenantId(), request, cancellationToken));
        })
        .WithName("ResolveComplianceCoreQuestionnaireV1");

        questionnaires.MapGet("/{id:guid}", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            QuestionnaireService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireComplianceCoreRuntimeAccess(context.User);
            return Results.Ok(await service.GetAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("GetComplianceCoreQuestionnaireV1");

        questionnaires.MapPost("/{id:guid}/submit", async (
            Guid id,
            QuestionnaireSubmitRequest request,
            ComplianceCoreAuthorizationService authorization,
            QuestionnaireService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireComplianceCoreRuntimeAccess(context.User);
            return Results.Ok(await service.SubmitAsync(context.User.GetTenantId(), id, request, cancellationToken));
        })
        .WithName("SubmitComplianceCoreQuestionnaireV1");
    }
}
