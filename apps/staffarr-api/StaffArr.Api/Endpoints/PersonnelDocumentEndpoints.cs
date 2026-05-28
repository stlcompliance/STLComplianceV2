using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class PersonnelDocumentEndpoints
{
    public static void MapStaffArrPersonnelDocumentEndpoints(this WebApplication app)
    {
        var documents = app.MapGroup("/api/people/{personId:guid}/documents")
            .WithTags("Personnel Documents")
            .RequireAuthorization();

        documents.MapGet("/", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelDocumentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonnelDocumentsRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListDocumentsAsync(tenantId, personId, cancellationToken));
        })
        .WithName("ListPersonnelDocuments");

        documents.MapPost("/", async (
            Guid personId,
            CreatePersonnelDocumentRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelDocumentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonnelDocumentsManageWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateDocumentAsync(
                tenantId,
                personId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Created($"/api/people/{personId}/documents/{created.DocumentId}", created);
        })
        .WithName("CreatePersonnelDocument");

        documents.MapGet("/{documentId:guid}", async (
            Guid personId,
            Guid documentId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelDocumentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonnelDocumentsRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetDocumentAsync(tenantId, personId, documentId, cancellationToken));
        })
        .WithName("GetPersonnelDocument");

        documents.MapGet("/{documentId:guid}/content", async (
            Guid personId,
            Guid documentId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelDocumentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonnelDocumentsRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            var (metadata, stream) = await service.OpenDocumentContentAsync(
                tenantId,
                personId,
                documentId,
                cancellationToken);
            return Results.File(stream, metadata.ContentType, metadata.FileName);
        })
        .WithName("DownloadPersonnelDocumentContent");
    }
}
