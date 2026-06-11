using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class AiAssistanceEndpoints
{
    public static void MapAiAssistanceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/ai")
            .WithTags("AI Assistance")
            .RequireAuthorization();

        group.MapPost("/assistant/messages", async (
            AiAssistantMessageRequest request,
            HttpContext context,
            AiAssistanceService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.SendMessageAsync(context.User, request, cancellationToken)))
            .WithName("CreateAiAssistantMessage");

        group.MapPost("/assistant/support-drafts", async (
            AiAssistantMessageRequest request,
            HttpContext context,
            AiAssistanceService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.SendMessageAsync(context.User, request with { Category = "drafting", Surface = "support" }, cancellationToken)))
            .WithName("CreateAiSupportDraft");

        group.MapPost("/assistant/admin-diagnostics", async (
            HttpContext context,
            PlatformAuthorizationService authorization,
            AiAssistanceService service,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformReadAccessAsync(context.User, cancellationToken);
            return Results.Ok(service.BuildAdminDiagnostic());
        }).WithName("RunAiAdminDiagnostic");

        group.MapPost("/validation/explain", async (
            AiAssistantMessageRequest request,
            HttpContext context,
            AiAssistanceService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.SendMessageAsync(context.User, request with { Category = "explanation", Surface = "validation" }, cancellationToken)))
            .WithName("ExplainAiValidationError");

        group.MapPost("/actions/preview", async (
            AiActionPreviewRequest request,
            HttpContext context,
            AiAssistanceService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.PreviewActionAsync(context.User, request, cancellationToken)))
            .WithName("PreviewAiAction");

        group.MapPost("/actions/{proposalId:guid}/confirm", async (
            Guid proposalId,
            HttpContext context,
            NexArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var actorPersonId = context.User.GetPersonId();
            var proposal = await db.AiActionProposals.FirstOrDefaultAsync(
                x => x.Id == proposalId && x.TenantId == tenantId,
                cancellationToken);
            if (proposal is null)
            {
                throw new StlApiException("ai.action_proposal_not_found", "AI action proposal was not found.", 404);
            }

            proposal.Status = "blocked_requires_owning_product_api";
            proposal.ConfirmedAt = DateTimeOffset.UtcNow;
            db.AiAuditEvents.Add(new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ActorPersonId = actorPersonId,
                EventType = "ai.action_confirm_blocked",
                TargetType = "ai_action_proposal",
                TargetId = proposal.Id.ToString(),
                Result = "blocked",
                ReasonCode = "owning_product_api_required",
                MetadataJson = "{}",
                OccurredAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
            return Results.Conflict(new
            {
                code = "owning_product_api_required",
                message = "AI prepared the action, but final writes must be completed through the owning product workflow."
            });
        }).WithName("ConfirmAiAction");
    }
}
