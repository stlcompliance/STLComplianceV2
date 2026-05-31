using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class ComplianceWaiverEndpoints
{
    public static void MapComplianceCoreWaiverEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/waivers")
            .WithTags("Waivers")
            .RequireAuthorization();
        var v1 = app.MapGroup("/api/v1/waivers")
            .WithTags("Waivers")
            .RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            string? packKey,
            string? scopeKey,
            int? limit,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                status,
                packKey,
                scopeKey,
                limit,
                cancellationToken));
        })
        .WithName("ListComplianceWaivers");

        v1.MapGet("/", async (
            string? status,
            string? packKey,
            string? scopeKey,
            int? limit,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                status,
                packKey,
                scopeKey,
                limit,
                cancellationToken));
        })
        .WithName("ListComplianceWaiversV1");

        group.MapGet("/{id:guid}", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, id, cancellationToken));
        })
        .WithName("GetComplianceWaiver");

        v1.MapGet("/{id:guid}", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, id, cancellationToken));
        })
        .WithName("GetComplianceWaiverV1");

        group.MapPost("/", async (
            CreateComplianceWaiverRequest request,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverManage(context.User);
            var tenantId = context.User.GetTenantId();
            var created = await service.CreateAsync(
                tenantId,
                context.User.GetUserId(),
                request,
                cancellationToken);
            return Results.Created($"/api/waivers/{created.WaiverId}", created);
        })
        .WithName("CreateComplianceWaiver");

        v1.MapPost("/", async (
            CreateComplianceWaiverRequest request,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverManage(context.User);
            var tenantId = context.User.GetTenantId();
            var created = await service.CreateAsync(
                tenantId,
                context.User.GetUserId(),
                request,
                cancellationToken);
            return Results.Created($"/api/v1/waivers/{created.WaiverId}", created);
        })
        .WithName("CreateComplianceWaiverV1");

        v1.MapPatch("/{id:guid}", async (
            Guid id,
            UpdateComplianceWaiverRequest request,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var status = request.Status.Trim().ToLowerInvariant();

            return status switch
            {
                "approved" => Results.Ok(await ApproveAsync()),
                "rejected" => Results.Ok(await RejectAsync()),
                "revoked" => Results.Ok(await RevokeAsync()),
                _ => Results.BadRequest(new { error = "waivers.invalid_status", message = "Status must be approved, rejected, or revoked." }),
            };

            async Task<ComplianceWaiverResponse> ApproveAsync()
            {
                authorization.RequireWaiverApprove(context.User);
                return await service.ApproveAsync(tenantId, actorUserId, id, cancellationToken);
            }

            async Task<ComplianceWaiverResponse> RejectAsync()
            {
                authorization.RequireWaiverApprove(context.User);
                return await service.RejectAsync(
                    tenantId,
                    actorUserId,
                    id,
                    new RejectComplianceWaiverRequest(request.Notes),
                    cancellationToken);
            }

            async Task<ComplianceWaiverResponse> RevokeAsync()
            {
                authorization.RequireWaiverManage(context.User);
                return await service.RevokeAsync(
                    tenantId,
                    actorUserId,
                    id,
                    new RevokeComplianceWaiverRequest(request.Notes),
                    cancellationToken);
            }
        })
        .WithName("UpdateComplianceWaiverV1");

        group.MapPost("/{id:guid}/approve", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverApprove(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ApproveAsync(
                tenantId,
                context.User.GetUserId(),
                id,
                cancellationToken));
        })
        .WithName("ApproveComplianceWaiver");

        v1.MapPost("/{id:guid}/approve", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverApprove(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ApproveAsync(
                tenantId,
                context.User.GetUserId(),
                id,
                cancellationToken));
        })
        .WithName("ApproveComplianceWaiverV1");

        group.MapPost("/{id:guid}/reject", async (
            Guid id,
            RejectComplianceWaiverRequest? request,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverApprove(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.RejectAsync(
                tenantId,
                context.User.GetUserId(),
                id,
                request,
                cancellationToken));
        })
        .WithName("RejectComplianceWaiver");

        v1.MapPost("/{id:guid}/reject", async (
            Guid id,
            RejectComplianceWaiverRequest? request,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverApprove(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.RejectAsync(
                tenantId,
                context.User.GetUserId(),
                id,
                request,
                cancellationToken));
        })
        .WithName("RejectComplianceWaiverV1");

        group.MapPost("/{id:guid}/revoke", async (
            Guid id,
            RevokeComplianceWaiverRequest? request,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.RevokeAsync(
                tenantId,
                context.User.GetUserId(),
                id,
                request,
                cancellationToken));
        })
        .WithName("RevokeComplianceWaiver");

        v1.MapPost("/{id:guid}/revoke", async (
            Guid id,
            RevokeComplianceWaiverRequest? request,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.RevokeAsync(
                tenantId,
                context.User.GetUserId(),
                id,
                request,
                cancellationToken));
        })
        .WithName("RevokeComplianceWaiverV1");

        v1.MapPost("/{id:guid}/renew", async (
            Guid id,
            RenewComplianceWaiverRequest request,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverApprove(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.RenewAsync(
                tenantId,
                context.User.GetUserId(),
                id,
                request,
                cancellationToken));
        })
        .WithName("RenewComplianceWaiverV1");
    }
}
