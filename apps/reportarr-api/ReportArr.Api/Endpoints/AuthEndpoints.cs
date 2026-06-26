using ReportArr.Api.Contracts;
using ReportArr.Api.Data;
using ReportArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace ReportArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapReportArrAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/api/auth").WithTags("Auth");

        static async Task<IResult> RedeemHandoffAsync(
            RedeemHandoffRequest request,
            HandoffAuthService service,
            CancellationToken cancellationToken)
        {
            return Results.Ok(await service.RedeemAsync(request, cancellationToken));
        }

        auth.MapPost("/handoff/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .WithName("ReportArrRedeemHandoff");
        auth.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        var authV1 = app.MapGroup("/api/v1/auth").WithTags("Auth");
        authV1.MapPost("/handoff/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .WithName("ReportArrRedeemHandoffV1");
        authV1.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        var session = app.MapGroup("/api/session").WithTags("Session").RequireAuthorization();
        session.MapGet("/", (HttpContext context, ReportArrStore store) =>
        {
            return Results.Ok(store.BuildSession(
                context.User.GetUserId().ToString(),
                context.User.GetPersonId().ToString(),
                context.User.GetTenantId().ToString(),
                context.User.GetTenantRoleKey(),
                context.User.IsPlatformAdmin(),
                context.User.GetLaunchableProductKeys()));
        }).WithName("ReportArrGetSessionBootstrap");

        var sessionV1 = app.MapGroup("/api/v1/session").WithTags("Session").RequireAuthorization();
        sessionV1.MapGet("/", (HttpContext context, ReportArrStore store) =>
        {
            return Results.Ok(store.BuildSession(
                context.User.GetUserId().ToString(),
                context.User.GetPersonId().ToString(),
                context.User.GetTenantId().ToString(),
                context.User.GetTenantRoleKey(),
                context.User.IsPlatformAdmin(),
                context.User.GetLaunchableProductKeys()));
        }).WithName("ReportArrGetSessionBootstrapV1");

        var me = app.MapGroup("/api/me").WithTags("Me").RequireAuthorization();
        me.MapGet("/", (HttpContext context, ReportArrStore store) =>
        {
            return Results.Ok(store.GetMe(context.User));
        }).WithName("ReportArrGetMe");

        var meV1 = app.MapGroup("/api/v1/me").WithTags("Me").RequireAuthorization();
        meV1.MapGet("/", (HttpContext context, ReportArrStore store) =>
        {
            return Results.Ok(store.GetMe(context.User));
        }).WithName("ReportArrGetMeV1");
    }
}

