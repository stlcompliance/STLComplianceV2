using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Scheduling;

namespace MaintainArr.Api.Endpoints;

public static class SchedulingEndpoints
{
    public static void MapMaintainArrSchedulingEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/scheduling").WithTags("Scheduling").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/scheduling").WithTags("Scheduling").RequireAuthorization(), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/unscheduled", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintainArrSchedulingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSchedulingView(context.User);
            return Results.Ok(await service.ListUnscheduledAsync(context.User.GetTenantId(), cancellationToken));
        })
        .WithName($"ListMaintainArrUnscheduledWork{nameSuffix}");

        group.MapGet("/scheduled", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintainArrSchedulingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSchedulingView(context.User);
            return Results.Ok(await service.ListScheduledAsync(context.User.GetTenantId(), cancellationToken));
        })
        .WithName($"ListMaintainArrScheduledWork{nameSuffix}");

        group.MapGet("/resources", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintainArrSchedulingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSchedulingView(context.User);
            return Results.Ok(await service.ListResourcesAsync(context.User.GetTenantId(), cancellationToken));
        })
        .WithName($"ListMaintainArrSchedulingResources{nameSuffix}");

        group.MapPost("/validate", async (
            StlSchedulingRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintainArrSchedulingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSchedulingView(context.User);
            var canOverride = authorization.CanOverrideScheduling(context.User);
            return Results.Ok(await service.ValidateAsync(
                context.User.GetTenantId(),
                request,
                canOverride,
                cancellationToken));
        })
        .WithName($"ValidateMaintainArrScheduling{nameSuffix}");

        group.MapPost("/schedule", async (
            StlSchedulingRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintainArrSchedulingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSchedulingSchedule(context.User);
            var result = await service.ScheduleAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString(),
                request,
                authorization.CanOverrideScheduling(context.User),
                isReschedule: false,
                cancellationToken);
            return result.Validation.Allowed ? Results.Ok(result) : Results.Conflict(result);
        })
        .WithName($"ScheduleMaintainArrWork{nameSuffix}");

        group.MapPost("/reschedule", async (
            StlSchedulingRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintainArrSchedulingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSchedulingReschedule(context.User);
            var result = await service.ScheduleAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString(),
                request,
                authorization.CanOverrideScheduling(context.User),
                isReschedule: true,
                cancellationToken);
            return result.Validation.Allowed ? Results.Ok(result) : Results.Conflict(result);
        })
        .WithName($"RescheduleMaintainArrWork{nameSuffix}");

        group.MapPost("/unschedule", async (
            StlSchedulingRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintainArrSchedulingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSchedulingUnschedule(context.User);
            return Results.Ok(await service.UnscheduleAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString(),
                request,
                cancellationToken));
        })
        .WithName($"UnscheduleMaintainArrWork{nameSuffix}");

        group.MapPost("/cancel", async (
            StlSchedulingRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintainArrSchedulingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSchedulingCancel(context.User);
            return Results.Ok(await service.CancelAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString(),
                request,
                cancellationToken));
        })
        .WithName($"CancelMaintainArrScheduledWork{nameSuffix}");

        group.MapPost("/complete", async (
            StlSchedulingRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintainArrSchedulingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSchedulingSchedule(context.User);
            return Results.Ok(await service.CompleteAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString(),
                request,
                cancellationToken));
        })
        .WithName($"CompleteMaintainArrScheduledWork{nameSuffix}");
    }
}
