using MaintainArr.Api.Contracts;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class PmProgramEndpoints
{
    public static void MapMaintainArrPmProgramEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            app.MapGroup("/api/preventive-maintenance/programs"),
            app.MapGroup("/api/v1/preventive-maintenance/programs"),
            app.MapGroup("/api/v1/pm-programs"),
        };

        foreach (var group in groups)
        {
            group.WithTags("PmPrograms").RequireAuthorization();

            group.MapGet("/", async (
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                PmProgramService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePmProgramsRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
            });

            group.MapGet("/{pmProgramId:guid}", async (
                Guid pmProgramId,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                PmProgramService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePmProgramsRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.GetAsync(tenantId, pmProgramId, cancellationToken));
            });

            group.MapPost("/", async (
                CreatePmProgramRequest request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                PmProgramService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePmProgramsCreate(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var actorPersonId = context.User.GetPersonId().ToString("D");
                var created = await service.CreateAsync(tenantId, actorUserId, request, actorPersonId, cancellationToken);
                return Results.Created($"/api/v1/pm-programs/{created.PmProgramId}", created);
            });

            group.MapPost("/preview-scope", async (
                CreatePmProgramRequest request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                PmProgramService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePmProgramsPreview(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.PreviewScopeAsync(tenantId, request, cancellationToken));
            });

            group.MapPost("/preview-due", async (
                CreatePmProgramRequest request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                PmProgramService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePmProgramsPreview(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.PreviewDueAsync(tenantId, request, cancellationToken));
            });

            group.MapPost("/{pmProgramId:guid}/activate", async (
                Guid pmProgramId,
                ActivatePmProgramRequest request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                PmProgramService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePmProgramsActivate(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var actorPersonId = context.User.GetPersonId().ToString("D");
                var updated = await service.ActivateAsync(tenantId, actorUserId, pmProgramId, request, actorPersonId, cancellationToken);
                return Results.Ok(updated);
            });

            group.MapPut("/{pmProgramId:guid}", async (
                Guid pmProgramId,
                UpdatePmProgramRequest request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                PmProgramService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePmProgramsUpdate(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var actorPersonId = context.User.GetPersonId().ToString("D");
                var updated = await service.UpdateAsync(tenantId, actorUserId, pmProgramId, request, actorPersonId, cancellationToken);
                return Results.Ok(updated);
            });

            group.MapPatch("/{pmProgramId:guid}/status", async (
                Guid pmProgramId,
                UpdatePmProgramStatusRequest request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                PmProgramService service,
                CancellationToken cancellationToken) =>
            {
                var normalizedStatus = request.Status.Trim().ToLowerInvariant();
                if (normalizedStatus == PmProgramStatuses.Active)
                {
                    authorization.RequirePmProgramsActivate(context.User);
                }
                else if (normalizedStatus == PmProgramStatuses.Paused)
                {
                    authorization.RequirePmProgramsPause(context.User);
                }
                else if (normalizedStatus == PmProgramStatuses.Retired)
                {
                    authorization.RequirePmProgramsRetire(context.User);
                }
                else
                {
                    authorization.RequirePmProgramsUpdate(context.User);
                }

                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var actorPersonId = context.User.GetPersonId().ToString("D");
                var updated = await service.UpdateStatusAsync(tenantId, actorUserId, pmProgramId, request, actorPersonId, cancellationToken);
                return Results.Ok(updated);
            });

            group.MapPut("/{pmProgramId:guid}/schedules", async (
                Guid pmProgramId,
                ReplacePmProgramSchedulesRequest request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                PmProgramService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePmProgramsUpdate(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var actorPersonId = context.User.GetPersonId().ToString("D");
                var updated = await service.ReplaceSchedulesAsync(tenantId, actorUserId, pmProgramId, request, actorPersonId, cancellationToken);
                return Results.Ok(updated);
            });
        }
    }
}
