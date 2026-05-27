using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class LaunchEndpoints
{
    public static void MapLaunchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/launch").WithTags("Launch").RequireAuthorization();

        group.MapGet("/context", async (
            string productKey,
            HttpContext context,
            LaunchService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetLaunchContextAsync(context.User, productKey, cancellationToken));
        })
        .WithName("GetLaunchContext");

        group.MapPost("/handoff", async (
            CreateHandoffRequest request,
            HttpContext context,
            LaunchService service,
            CancellationToken cancellationToken) =>
        {
            var created = await service.CreateHandoffAsync(context.User, request, cancellationToken);
            return Results.Created($"/api/launch/handoff/{created.HandoffId}", created);
        })
        .WithName("CreateHandoff");

        group.MapPost("/handoff/redeem", async (
            RedeemHandoffRequest request,
            HttpContext context,
            LaunchService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.RedeemHandoffAsync(context.User, request, cancellationToken));
        })
        .AllowAnonymous()
        .WithName("RedeemHandoff");

        group.MapPost("/callback/validate", async (
            ValidateCallbackRequest request,
            HttpContext context,
            LaunchService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ValidateCallbackAsync(context.User, request, cancellationToken));
        })
        .WithName("ValidateLaunchCallback");

        group.MapGet("/callback-allowlist", async (
            string productKey,
            Guid? tenantId,
            HttpContext context,
            CallbackAllowlistAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListAsync(context.User, productKey, tenantId, cancellationToken));
        })
        .WithName("ListCallbackAllowlist");

        group.MapPost("/callback-allowlist", async (
            CreateCallbackAllowlistEntryRequest request,
            HttpContext context,
            CallbackAllowlistAdminService service,
            CancellationToken cancellationToken) =>
        {
            var created = await service.CreateAsync(context.User, request, cancellationToken);
            return Results.Created($"/api/launch/callback-allowlist/{created.EntryId}", created);
        })
        .WithName("CreateCallbackAllowlistEntry");

        group.MapDelete("/callback-allowlist/{entryId:guid}", async (
            Guid entryId,
            HttpContext context,
            CallbackAllowlistAdminService service,
            CancellationToken cancellationToken) =>
        {
            await service.DeleteAsync(context.User, entryId, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteCallbackAllowlistEntry");
    }
}
