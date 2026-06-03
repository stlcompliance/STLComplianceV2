using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class VendorDocumentEndpoints
{
    public static void MapSupplyArrVendorDocumentEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group.MapGet("/", async (
                Guid partyId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                PartyComplianceDocumentService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.ListForPartyAsync(tenantId, partyId, cancellationToken));
            })
            .WithName($"ListVendorDocuments{nameSuffix}");

            group.MapPost("/", async (
                RegisterVendorDocumentRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                PartyComplianceDocumentService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var created = await service.RegisterAsync(
                    tenantId,
                    actorUserId,
                    request.PartyId,
                    new RegisterPartyComplianceDocumentRequest(
                        request.DocumentKey,
                        request.DocumentTypeKey,
                        request.Title,
                        request.EffectiveAt,
                        request.ExpiresAt,
                        request.FileName,
                        request.ContentType,
                        request.SizeBytes,
                        request.StorageUri),
                    cancellationToken);
                return Results.Created($"/api/v1/vendor-documents/{created.DocumentId}", created);
            })
            .WithName($"RegisterVendorDocument{nameSuffix}");

            group.MapPost("/{documentId:guid}/approve", async (
                Guid documentId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                PartyComplianceDocumentService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingReview(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.ApproveAsync(tenantId, actorUserId, documentId, cancellationToken));
            })
            .WithName($"ApproveVendorDocument{nameSuffix}");

            group.MapGet("/{documentId:guid}/content", async (
                Guid documentId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                PartyComplianceDocumentService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingRead(context.User);
                var tenantId = context.User.GetTenantId();
                var (metadata, stream) = await service.OpenDocumentContentAsync(tenantId, documentId, cancellationToken);
                return Results.File(stream, metadata.ContentType, metadata.FileName);
            })
            .WithName($"DownloadVendorDocument{nameSuffix}");

            group.MapPost("/{documentId:guid}/reject", async (
                Guid documentId,
                RejectPartyComplianceDocumentRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                PartyComplianceDocumentService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingReview(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.RejectAsync(tenantId, actorUserId, documentId, request, cancellationToken));
            })
            .WithName($"RejectVendorDocument{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/vendor-documents").WithTags("VendorDocuments").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/vendor-documents").WithTags("VendorDocuments").RequireAuthorization(), "V1");
    }
}
