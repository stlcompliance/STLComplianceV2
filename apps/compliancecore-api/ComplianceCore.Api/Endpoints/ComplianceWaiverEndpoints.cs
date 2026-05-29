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
    }
}
