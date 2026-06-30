using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class SupplierOnboardingEndpoints
{
    public static void MapSupplyArrSupplierOnboardingEndpoints(this WebApplication app)
    {
        static void MapOnboardingRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("SupplierOnboarding").RequireAuthorization();

            group.MapGet("/document-requirements", async (
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOnboardingService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.GetDocumentRequirementsAsync(tenantId, cancellationToken));
            })
            .WithName($"GetSupplierOnboardingDocumentRequirements{nameSuffix}");

            group.MapPut("/document-requirements", async (
                UpsertSupplierOnboardingDocumentRequirementsRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOnboardingService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.UpsertDocumentRequirementsAsync(
                    tenantId,
                    actorUserId,
                    request,
                    cancellationToken));
            })
            .WithName($"UpsertSupplierOnboardingDocumentRequirements{nameSuffix}");

            group.MapGet("/pending", async (
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOnboardingService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingReview(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.ListPendingReviewAsync(tenantId, cancellationToken));
            })
            .WithName($"ListPendingSupplierOnboarding{nameSuffix}");

            group.MapPost("/start", async (
                StartSupplierOnboardingRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOnboardingService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var created = await service.StartOnboardingAsync(tenantId, actorUserId, request, cancellationToken);
                return Results.Created($"/api/v1/supplier-onboarding/suppliers/{created.SupplierId}", created);
            })
            .WithName($"StartSupplierOnboarding{nameSuffix}");

            group.MapGet("/parties/{partyId:guid}", async (
                Guid partyId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOnboardingService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.GetByPartyAsync(tenantId, partyId, cancellationToken));
            })
            .WithName($"GetSupplierOnboardingByParty{nameSuffix}");

            group.MapGet("/suppliers/{supplierId:guid}", async (
                Guid supplierId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOnboardingService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.GetBySupplierAsync(tenantId, supplierId, cancellationToken));
            })
            .WithName($"GetSupplierOnboardingBySupplier{nameSuffix}");

            group.MapPost("/parties/{partyId:guid}/submit", async (
                Guid partyId,
                SubmitSupplierOnboardingForReviewRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOnboardingService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.SubmitForReviewByPartyAsync(
                    tenantId,
                    actorUserId,
                    partyId,
                    request,
                    cancellationToken));
            })
            .WithName($"SubmitSupplierOnboardingForReview{nameSuffix}");

            group.MapPost("/suppliers/{supplierId:guid}/submit", async (
                Guid supplierId,
                SubmitSupplierOnboardingForReviewRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOnboardingService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.SubmitForReviewBySupplierAsync(
                    tenantId,
                    actorUserId,
                    supplierId,
                    request,
                    cancellationToken));
            })
            .WithName($"SubmitSupplierOnboardingForSupplierReview{nameSuffix}");

            group.MapPost("/suppliers/{supplierId:guid}/approve", async (
                Guid supplierId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOnboardingService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingReview(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.ApproveSupplierAsync(tenantId, actorUserId, supplierId, cancellationToken));
            })
            .WithName($"ApproveSupplierOnboardingBySupplier{nameSuffix}");

            group.MapPost("/parties/{partyId:guid}/approve", async (
                Guid partyId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOnboardingService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingReview(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.ApproveByPartyAsync(tenantId, actorUserId, partyId, cancellationToken));
            })
            .WithName($"ApproveSupplierOnboarding{nameSuffix}");

            group.MapPost("/parties/{partyId:guid}/reject", async (
                Guid partyId,
                RejectSupplierOnboardingRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOnboardingService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingReview(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.RejectByPartyAsync(tenantId, actorUserId, partyId, request, cancellationToken));
            })
            .WithName($"RejectSupplierOnboarding{nameSuffix}");

            group.MapPost("/suppliers/{supplierId:guid}/reject", async (
                Guid supplierId,
                RejectSupplierOnboardingRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOnboardingService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingReview(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.RejectSupplierAsync(tenantId, actorUserId, supplierId, request, cancellationToken));
            })
            .WithName($"RejectSupplierOnboardingBySupplier{nameSuffix}");

            group.MapPost("/parties/{partyId:guid}/suspend", async (
                Guid partyId,
                SuspendSupplierOnboardingRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOnboardingService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingReview(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.SuspendByPartyAsync(tenantId, actorUserId, partyId, request, cancellationToken));
            })
            .WithName($"SuspendSupplierOnboarding{nameSuffix}");

            group.MapPost("/suppliers/{supplierId:guid}/suspend", async (
                Guid supplierId,
                SuspendSupplierOnboardingRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOnboardingService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingReview(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.SuspendSupplierAsync(tenantId, actorUserId, supplierId, request, cancellationToken));
            })
            .WithName($"SuspendSupplierOnboardingBySupplier{nameSuffix}");
        }

        static void MapComplianceDocumentRoutes(RouteGroupBuilder partyDocs, string nameSuffix)
        {
            partyDocs = partyDocs.WithTags("PartyComplianceDocuments").RequireAuthorization();

            partyDocs.MapGet("/", async (
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
            .WithName($"ListPartyComplianceDocuments{nameSuffix}");

            partyDocs.MapPost("/", async (
                Guid partyId,
                SupplierComplianceDocumentRegistrationRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                PartyComplianceDocumentService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var created = await service.RegisterAsync(tenantId, actorUserId, partyId, request, cancellationToken);
                return Results.Created($"/api/suppliers/{partyId}/compliance-documents/{created.DocumentId}", created);
            })
            .WithName($"RegisterPartyComplianceDocument{nameSuffix}");

            partyDocs.MapPost("/{documentId:guid}/approve", async (
                Guid partyId,
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
            .WithName($"ApprovePartyComplianceDocument{nameSuffix}");

            partyDocs.MapGet("/{documentId:guid}/content", async (
                Guid partyId,
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
            .WithName($"DownloadPartyComplianceDocument{nameSuffix}");

            partyDocs.MapPost("/{documentId:guid}/reject", async (
                Guid partyId,
                Guid documentId,
                RejectSupplierComplianceDocumentRequest request,
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
            .WithName($"RejectPartyComplianceDocument{nameSuffix}");
        }

        static void MapSupplierComplianceDocumentRoutes(RouteGroupBuilder supplierDocs, string nameSuffix)
        {
            supplierDocs = supplierDocs.WithTags("SupplierComplianceDocuments").RequireAuthorization();

            supplierDocs.MapGet("/", async (
                Guid supplierId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                PartyComplianceDocumentService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.ListForSupplierAsync(tenantId, supplierId, cancellationToken));
            })
            .WithName($"ListSupplierComplianceDocuments{nameSuffix}");

            supplierDocs.MapPost("/", async (
                Guid supplierId,
                SupplierComplianceDocumentRegistrationRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                PartyComplianceDocumentService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var created = await service.RegisterForSupplierAsync(tenantId, actorUserId, supplierId, request, cancellationToken);
                return Results.Created($"/api/suppliers/{supplierId}/compliance-documents/{created.DocumentId}", created);
            })
            .WithName($"RegisterSupplierComplianceDocument{nameSuffix}");

            supplierDocs.MapPost("/{documentId:guid}/approve", async (
                Guid supplierId,
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
            .WithName($"ApproveSupplierComplianceDocument{nameSuffix}");

            supplierDocs.MapGet("/{documentId:guid}/content", async (
                Guid supplierId,
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
            .WithName($"DownloadSupplierComplianceDocument{nameSuffix}");

            supplierDocs.MapPost("/{documentId:guid}/reject", async (
                Guid supplierId,
                Guid documentId,
                RejectSupplierComplianceDocumentRequest request,
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
            .WithName($"RejectSupplierComplianceDocument{nameSuffix}");
        }

        MapOnboardingRoutes(app.MapGroup("/api/supplier-onboarding"), string.Empty);
        MapOnboardingRoutes(app.MapGroup("/api/v1/supplier-onboarding"), "V1");

        MapComplianceDocumentRoutes(app.MapGroup("/api/parties/{partyId:guid}/compliance-documents"), string.Empty);
        MapComplianceDocumentRoutes(app.MapGroup("/api/v1/parties/{partyId:guid}/compliance-documents"), "V1");
        MapSupplierComplianceDocumentRoutes(app.MapGroup("/api/suppliers/{supplierId:guid}/compliance-documents"), "Supplier");
        MapSupplierComplianceDocumentRoutes(app.MapGroup("/api/v1/suppliers/{supplierId:guid}/compliance-documents"), "V1Supplier");
    }
}
