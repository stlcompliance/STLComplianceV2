using NexArr.Api.Contracts;
using NexArr.Api.Entities;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tenants").WithTags("Tenants").RequireAuthorization();
        var v1 = app.MapGroup("/api/v1/tenants").WithTags("Tenants").RequireAuthorization();

        static async Task<IResult> ListTenantsEndpoint(
            HttpContext context,
            TenantAdminService service,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var result = await service.ListAsync(context.User, page == 0 ? 1 : page, pageSize == 0 ? 50 : pageSize, cancellationToken);
            return Results.Ok(result);
        }

        group.MapGet("/", ListTenantsEndpoint)
        .WithName("ListTenants");

        v1.MapGet("/", ListTenantsEndpoint)
        .WithName("ListTenantsV1");

        static async Task<IResult> GetTenantEndpoint(
            Guid tenantId,
            HttpContext context,
            TenantAdminService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetAsync(context.User, tenantId, cancellationToken));

        group.MapGet("/{tenantId:guid}", GetTenantEndpoint)
        .WithName("GetTenant");

        v1.MapGet("/{tenantId:guid}", GetTenantEndpoint)
        .WithName("GetTenantV1");

        static async Task<IResult> CreateTenantEndpoint(
            CreateTenantRequest request,
            HttpContext context,
            TenantAdminService service,
            CancellationToken cancellationToken)
        {
            var created = await service.CreateAsync(context.User, request, cancellationToken);
            return Results.Created($"/api/tenants/{created.TenantId}", created);
        }

        group.MapPost("/", CreateTenantEndpoint)
        .WithName("CreateTenant");

        v1.MapPost("/", CreateTenantEndpoint)
        .WithName("CreateTenantV1");

        static async Task<IResult> UpdateTenantEndpoint(
            Guid tenantId,
            UpdateTenantRequest request,
            HttpContext context,
            TenantAdminService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateAsync(context.User, tenantId, request, cancellationToken));

        group.MapPut("/{tenantId:guid}", UpdateTenantEndpoint)
        .WithName("UpdateTenant");

        v1.MapPatch("/{tenantId:guid}", UpdateTenantEndpoint)
        .WithName("UpdateTenantV1");

        static async Task<IResult> UpdateTenantStatusEndpoint(
            Guid tenantId,
            UpdateTenantStatusRequest request,
            HttpContext context,
            TenantAdminService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateStatusAsync(context.User, tenantId, request, cancellationToken));

        group.MapPatch("/{tenantId:guid}/status", UpdateTenantStatusEndpoint)
        .WithName("UpdateTenantStatus");

        v1.MapPost("/{tenantId:guid}/disable", async (
            Guid tenantId,
            HttpContext context,
            TenantAdminService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateStatusAsync(
                context.User,
                tenantId,
                new UpdateTenantStatusRequest(TenantStatuses.Suspended),
                cancellationToken)))
        .WithName("DisableTenantV1");

        v1.MapPost("/{tenantId:guid}/enable", async (
            Guid tenantId,
            HttpContext context,
            TenantAdminService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateStatusAsync(
                context.User,
                tenantId,
                new UpdateTenantStatusRequest(TenantStatuses.Active),
                cancellationToken)))
        .WithName("EnableTenantV1");

        static async Task<IResult> ListTenantMembersEndpoint(
            Guid tenantId,
            HttpContext context,
            TenantMembershipAdminService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ListMembersAsync(context.User, tenantId, cancellationToken));

        group.MapGet("/{tenantId:guid}/members", ListTenantMembersEndpoint)
        .WithName("ListTenantMembers");

        v1.MapGet("/{tenantId:guid}/members", ListTenantMembersEndpoint)
        .WithName("ListTenantMembersV1");

        static async Task<IResult> AddTenantMemberEndpoint(
            Guid tenantId,
            AddTenantMemberRequest request,
            HttpContext context,
            TenantMembershipAdminService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.AddMemberAsync(context.User, tenantId, request, cancellationToken));

        group.MapPost("/{tenantId:guid}/members", AddTenantMemberEndpoint)
        .WithName("AddTenantMember");

        v1.MapPost("/{tenantId:guid}/members", AddTenantMemberEndpoint)
        .WithName("AddTenantMemberV1");

        static async Task<IResult> RemoveTenantMemberEndpoint(
            Guid tenantId,
            Guid userId,
            HttpContext context,
            TenantMembershipAdminService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.RemoveMemberAsync(context.User, tenantId, userId, cancellationToken));

        group.MapDelete("/{tenantId:guid}/members/{userId:guid}", RemoveTenantMemberEndpoint)
        .WithName("RemoveTenantMember");

        v1.MapDelete("/{tenantId:guid}/members/{personId:guid}", async (
            Guid tenantId,
            Guid personId,
            HttpContext context,
            TenantMembershipAdminService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.RemoveMemberAsync(context.User, tenantId, personId, cancellationToken)))
        .WithName("RemoveTenantMemberV1");
    }
}
