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
                return Results.Created($"/api/supplier-onboarding/parties/{created.ExternalPartyId}", created);
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
                return Results.Ok(await service.SubmitForReviewAsync(
                    tenantId,
                    actorUserId,
                    partyId,
                    request,
                    cancellationToken));
            })
            .WithName($"SubmitSupplierOnboardingForReview{nameSuffix}");

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
                return Results.Ok(await service.ApproveAsync(tenantId, actorUserId, partyId, cancellationToken));
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
                return Results.Ok(await service.RejectAsync(tenantId, actorUserId, partyId, request, cancellationToken));
            })
            .WithName($"RejectSupplierOnboarding{nameSuffix}");

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
                return Results.Ok(await service.SuspendAsync(tenantId, actorUserId, partyId, request, cancellationToken));
            })
            .WithName($"SuspendSupplierOnboarding{nameSuffix}");
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
                RegisterPartyComplianceDocumentRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                PartyComplianceDocumentService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOnboardingManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var created = await service.RegisterAsync(tenantId, actorUserId, partyId, request, cancellationToken);
                return Results.Created($"/api/parties/{partyId}/compliance-documents/{created.DocumentId}", created);
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

            partyDocs.MapPost("/{documentId:guid}/reject", async (
                Guid partyId,
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
            .WithName($"RejectPartyComplianceDocument{nameSuffix}");
        }

        MapOnboardingRoutes(app.MapGroup("/api/supplier-onboarding"), string.Empty);
        MapOnboardingRoutes(app.MapGroup("/api/v1/supplier-onboarding"), "V1");

        MapComplianceDocumentRoutes(app.MapGroup("/api/parties/{partyId:guid}/compliance-documents"), string.Empty);
        MapComplianceDocumentRoutes(app.MapGroup("/api/v1/parties/{partyId:guid}/compliance-documents"), "V1");
    }
}
