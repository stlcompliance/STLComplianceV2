using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class IncidentEndpoints
{
    public static void MapStaffArrIncidentEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/incidents", Suffix: string.Empty),
            (Route: "/api/v1/incidents", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var incidents = app.MapGroup(route)
                .WithTags("Incidents")
                .RequireAuthorization();

            incidents.MapGet("/", async (
                Guid? personId,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                IncidentService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireIncidentsRead(context.User, personId);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.ListIncidentsAsync(tenantId, personId, cancellationToken));
            })
            .WithName($"ListPersonnelIncidents{suffix}");

            incidents.MapPost("/", async (
                CreatePersonnelIncidentRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                IncidentService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireIncidentsManageWrite(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var created = await service.CreateIncidentAsync(tenantId, actorUserId, request, cancellationToken);
                return Results.Created($"{route}/{created.IncidentId}", created);
            })
            .WithName($"CreatePersonnelIncident{suffix}");

            incidents.MapGet("/{incidentId:guid}", async (
                Guid incidentId,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                IncidentService service,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var detail = await service.GetIncidentAsync(tenantId, incidentId, cancellationToken);
                authorization.RequireIncidentsRead(context.User, detail.PersonId);
                return Results.Ok(detail);
            })
            .WithName($"GetPersonnelIncident{suffix}");

            incidents.MapPost("/{incidentId:guid}/route-to-trainarr", async (
                Guid incidentId,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                IncidentRoutingService routingService,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireIncidentsManageWrite(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var result = await routingService.RouteToTrainarrAsync(
                    tenantId,
                    actorUserId,
                    incidentId,
                    cancellationToken);
                return Results.Ok(result);
            })
            .WithName($"RoutePersonnelIncidentToTrainarr{suffix}");

            incidents.MapPatch("/{incidentId:guid}/status", async (
                Guid incidentId,
                UpdatePersonnelIncidentStatusRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                IncidentService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireIncidentsManageWrite(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.UpdateIncidentStatusAsync(
                    tenantId,
                    actorUserId,
                    incidentId,
                    request,
                cancellationToken));
            })
            .WithName($"UpdatePersonnelIncidentStatus{suffix}");

            incidents.MapPost("/{incidentId:guid}/notes", async (
                Guid incidentId,
                CreateIncidentNoteRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                IncidentService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireIncidentsManageWrite(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var detail = await service.CreateIncidentNoteAsync(
                    tenantId,
                    actorUserId,
                    incidentId,
                    request,
                    cancellationToken);
                return Results.Ok(detail);
            })
            .WithName($"CreatePersonnelIncidentNote{suffix}");

            incidents.MapPatch("/{incidentId:guid}/notes/{noteId:guid}/status", async (
                Guid incidentId,
                Guid noteId,
                UpdateIncidentNoteStatusRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                IncidentService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireIncidentsManageWrite(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var detail = await service.UpdateIncidentNoteStatusAsync(
                    tenantId,
                    actorUserId,
                    incidentId,
                    noteId,
                    request,
                    cancellationToken);
                return Results.Ok(detail);
            })
            .WithName($"UpdatePersonnelIncidentNoteStatus{suffix}");

            incidents.MapPost("/{incidentId:guid}/attachments", async (
                Guid incidentId,
                CreateIncidentAttachmentRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                IncidentService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireIncidentsManageWrite(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var detail = await service.CreateIncidentAttachmentAsync(
                    tenantId,
                    actorUserId,
                    incidentId,
                    request,
                    cancellationToken);
                return Results.Ok(detail);
            })
            .WithName($"CreatePersonnelIncidentAttachment{suffix}");

            incidents.MapGet("/{incidentId:guid}/attachments/{attachmentId:guid}/content", async (
                Guid incidentId,
                Guid attachmentId,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                IncidentService service,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var detail = await service.GetIncidentAsync(tenantId, incidentId, cancellationToken);
                authorization.RequireIncidentsRead(context.User, detail.PersonId);
                var (metadata, stream) = await service.OpenIncidentAttachmentContentAsync(
                    tenantId,
                    incidentId,
                    attachmentId,
                    cancellationToken);
                return Results.File(stream, metadata.ContentType, metadata.FileName);
            })
            .WithName($"DownloadPersonnelIncidentAttachmentContent{suffix}");
        }
    }
}
