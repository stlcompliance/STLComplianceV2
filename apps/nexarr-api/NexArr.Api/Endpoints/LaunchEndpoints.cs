using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class LaunchEndpoints
{
    public static void MapLaunchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/launch").WithTags("Launch").RequireAuthorization();

        static async Task<IResult> GetLaunchContextEndpoint(
            string productKey,
            HttpContext context,
            LaunchService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetLaunchContextAsync(context.User, productKey, cancellationToken));

        group.MapGet("/context", GetLaunchContextEndpoint)
        .WithName("GetLaunchContext");

        app.MapGet("/api/v1/launch/context", GetLaunchContextEndpoint)
            .WithTags("Launch")
            .RequireAuthorization()
            .WithName("GetLaunchContextV1");

        static async Task<IResult> GetLaunchCatalogEndpoint(
            string? currentProductKey,
            HttpContext context,
            LaunchService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetLaunchCatalogAsync(context.User, currentProductKey, cancellationToken));

        group.MapGet("/catalog", GetLaunchCatalogEndpoint)
        .WithName("GetLaunchCatalog");

        app.MapGet("/api/v1/launch/catalog", GetLaunchCatalogEndpoint)
            .WithTags("Launch")
            .RequireAuthorization()
            .WithName("GetLaunchCatalogV1");

        static async Task<IResult> ValidateLaunchEndpoint(
            ValidateLaunchRequest request,
            HttpContext context,
            LaunchService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ValidateLaunchAsync(context.User, request, cancellationToken));

        group.MapPost("/validate", ValidateLaunchEndpoint)
            .WithName("ValidateLaunch");

        app.MapPost("/api/v1/launch/validate", ValidateLaunchEndpoint)
            .WithTags("Launch")
            .RequireAuthorization()
            .WithName("ValidateLaunchV1");

        static async Task<IResult> CreateHandoffEndpoint(
            CreateHandoffRequest request,
            HttpContext context,
            LaunchService service,
            CancellationToken cancellationToken)
        {
            var created = await service.CreateHandoffAsync(context.User, request, cancellationToken);
            return Results.Created($"/api/launch/handoff/{created.HandoffId}", created);
        }

        group.MapPost("/handoff", CreateHandoffEndpoint)
        .WithName("CreateHandoff");

        app.MapPost("/api/v1/launch/handoff", CreateHandoffEndpoint)
            .WithTags("Launch")
            .RequireAuthorization()
            .WithName("CreateHandoffV1");

        static async Task<IResult> RedeemHandoffEndpoint(
            RedeemHandoffRequest request,
            HttpContext context,
            LaunchService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.RedeemHandoffAsync(context.User, request, cancellationToken));

        group.MapPost("/handoff/redeem", RedeemHandoffEndpoint)
        .AllowAnonymous()
        .WithName("RedeemHandoff");

        app.MapPost("/api/v1/handoff/redeem", RedeemHandoffEndpoint)
        .WithTags("Launch")
        .AllowAnonymous()
        .WithName("RedeemHandoffV1");

        app.MapPost("/api/v1/launch/handoff/redeem", RedeemHandoffEndpoint)
        .WithTags("Launch")
        .AllowAnonymous()
        .WithName("RedeemHandoffLaunchV1");

        static async Task<IResult> ValidateCallbackEndpoint(
            ValidateCallbackRequest request,
            HttpContext context,
            LaunchService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ValidateCallbackAsync(context.User, request, cancellationToken));

        group.MapPost("/callback/validate", ValidateCallbackEndpoint)
        .WithName("ValidateLaunchCallback");

        app.MapPost("/api/v1/launch/callback/validate", ValidateCallbackEndpoint)
            .WithTags("Launch")
            .RequireAuthorization()
            .WithName("ValidateLaunchCallbackV1");

        static async Task<IResult> ListCallbackAllowlistEndpoint(
            string productKey,
            Guid? tenantId,
            HttpContext context,
            CallbackAllowlistAdminService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ListAsync(context.User, productKey, tenantId, cancellationToken));

        group.MapGet("/callback-allowlist", ListCallbackAllowlistEndpoint)
        .WithName("ListCallbackAllowlist");

        app.MapGet("/api/v1/launch/callback-allowlist", ListCallbackAllowlistEndpoint)
            .WithTags("Launch")
            .RequireAuthorization()
            .WithName("ListCallbackAllowlistV1");

        static async Task<IResult> CreateCallbackAllowlistEndpoint(
            CreateCallbackAllowlistEntryRequest request,
            HttpContext context,
            CallbackAllowlistAdminService service,
            CancellationToken cancellationToken)
        {
            var created = await service.CreateAsync(context.User, request, cancellationToken);
            return Results.Created($"/api/launch/callback-allowlist/{created.EntryId}", created);
        }

        group.MapPost("/callback-allowlist", CreateCallbackAllowlistEndpoint)
        .WithName("CreateCallbackAllowlistEntry");

        app.MapPost("/api/v1/launch/callback-allowlist", CreateCallbackAllowlistEndpoint)
            .WithTags("Launch")
            .RequireAuthorization()
            .WithName("CreateCallbackAllowlistEntryV1");

        static async Task<IResult> DeleteCallbackAllowlistEndpoint(
            Guid entryId,
            HttpContext context,
            CallbackAllowlistAdminService service,
            CancellationToken cancellationToken)
        {
            await service.DeleteAsync(context.User, entryId, cancellationToken);
            return Results.NoContent();
        }

        group.MapDelete("/callback-allowlist/{entryId:guid}", DeleteCallbackAllowlistEndpoint)
        .WithName("DeleteCallbackAllowlistEntry");

        app.MapDelete("/api/v1/launch/callback-allowlist/{entryId:guid}", DeleteCallbackAllowlistEndpoint)
            .WithTags("Launch")
            .RequireAuthorization()
            .WithName("DeleteCallbackAllowlistEntryV1");
    }
}
