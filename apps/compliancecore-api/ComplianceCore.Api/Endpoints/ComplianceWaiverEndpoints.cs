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

        group.MapGet("/{waiverId:guid}", async (
            Guid waiverId,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, waiverId, cancellationToken));
        })
        .WithName("GetComplianceWaiver");

        v1.MapGet("/{waiverId:guid}", async (
            Guid waiverId,
            ComplianceCoreAuthorizationService authorization,
            ComplianceWaiverService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, waiverId, cancellationToken));
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

        v1.MapPatch("/{waiverId:guid}", async (
            Guid waiverId,
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
                return await service.ApproveAsync(tenantId, actorUserId, waiverId, cancellationToken);
            }

            async Task<ComplianceWaiverResponse> RejectAsync()
            {
                authorization.RequireWaiverApprove(context.User);
                return await service.RejectAsync(
                    tenantId,
                    actorUserId,
                    waiverId,
                    new RejectComplianceWaiverRequest(request.Notes),
                    cancellationToken);
            }

            async Task<ComplianceWaiverResponse> RevokeAsync()
            {
                authorization.RequireWaiverManage(context.User);
                return await service.RevokeAsync(
                    tenantId,
                    actorUserId,
                    waiverId,
                    new RevokeComplianceWaiverRequest(request.Notes),
                    cancellationToken);
            }
        })
        .WithName("UpdateComplianceWaiverV1");

        group.MapPost("/{waiverId:guid}/approve", async (
            Guid waiverId,
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
                waiverId,
                cancellationToken));
        })
        .WithName("ApproveComplianceWaiver");

        v1.MapPost("/{waiverId:guid}/approve", async (
            Guid waiverId,
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
                waiverId,
                cancellationToken));
        })
        .WithName("ApproveComplianceWaiverV1");

        group.MapPost("/{waiverId:guid}/reject", async (
            Guid waiverId,
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
                waiverId,
                request,
                cancellationToken));
        })
        .WithName("RejectComplianceWaiver");

        v1.MapPost("/{waiverId:guid}/reject", async (
            Guid waiverId,
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
                waiverId,
                request,
                cancellationToken));
        })
        .WithName("RejectComplianceWaiverV1");

        group.MapPost("/{waiverId:guid}/revoke", async (
            Guid waiverId,
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
                waiverId,
                request,
                cancellationToken));
        })
        .WithName("RevokeComplianceWaiver");

        v1.MapPost("/{waiverId:guid}/revoke", async (
            Guid waiverId,
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
                waiverId,
                request,
                cancellationToken));
        })
        .WithName("RevokeComplianceWaiverV1");

        v1.MapPost("/{waiverId:guid}/renew", async (
            Guid waiverId,
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
                waiverId,
                request,
                cancellationToken));
        })
        .WithName("RenewComplianceWaiverV1");
    }
}
