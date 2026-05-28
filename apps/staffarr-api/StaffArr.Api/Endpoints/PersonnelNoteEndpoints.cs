using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class PersonnelNoteEndpoints
{
    public static void MapStaffArrPersonnelNoteEndpoints(this WebApplication app)
    {
        var notes = app.MapGroup("/api/people/{personId:guid}/notes")
            .WithTags("Personnel Notes")
            .RequireAuthorization();

        notes.MapGet("/", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelNoteService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonnelNotesRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            var result = await service.ListNotesAsync(
                tenantId,
                personId,
                note => authorization.CanViewPersonnelNote(context.User, personId, note),
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ListPersonnelNotes");

        notes.MapPost("/", async (
            Guid personId,
            CreatePersonnelNoteRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelNoteService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonnelNotesManageWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateNoteAsync(tenantId, personId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/people/{personId}/notes/{created.NoteId}", created);
        })
        .WithName("CreatePersonnelNote");

        notes.MapGet("/{noteId:guid}", async (
            Guid personId,
            Guid noteId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelNoteService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonnelNotesRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            var detail = await service.GetNoteAsync(
                tenantId,
                personId,
                noteId,
                note => authorization.CanViewPersonnelNote(context.User, personId, note),
                cancellationToken);
            return Results.Ok(detail);
        })
        .WithName("GetPersonnelNote");
    }
}
