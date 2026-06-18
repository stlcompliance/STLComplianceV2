using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class TimekeepingEndpoints
{
    public const string LedgArrPayrollReadySnapshotActionScope = TimekeepingService.PayrollReadySnapshotReadActionScope;
    public const string LaborEvidenceIngestActionScope = TimekeepingService.LaborEvidenceWriteActionScope;

    public static void MapStaffArrTimekeepingEndpoints(this WebApplication app)
    {
        var timekeeping = app.MapGroup("/api/v1/timekeeping").WithTags("Timekeeping").RequireAuthorization();
        var integrations = app.MapGroup("/api/v1/integrations").WithTags("Timekeeping Integrations");

        timekeeping.MapGet("/profiles", async (HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingRead(context.User);
            return Results.Ok(await service.ListProfilesAsync(context.User.GetTenantId(), cancellationToken));
        });
        timekeeping.MapGet("/profiles/{personId:guid}", async (Guid personId, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingRead(context.User);
            return Results.Ok(await service.GetProfileAsync(context.User.GetTenantId(), personId, cancellationToken));
        });
        timekeeping.MapPost("/profiles", async (UpsertTimekeepingProfileRequest request, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingManage(context.User);
            return Results.Ok(await service.UpsertProfileAsync(context.User.GetTenantId(), context.User.GetUserId(), request, cancellationToken));
        });
        timekeeping.MapPatch("/profiles/{personId:guid}", async (Guid personId, UpsertTimekeepingProfileRequest request, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingManage(context.User);
            return Results.Ok(await service.UpsertProfileAsync(context.User.GetTenantId(), context.User.GetUserId(), request with { PersonId = personId }, cancellationToken));
        });

        timekeeping.MapGet("/pay-policies", async (HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingRead(context.User);
            return Results.Ok(await service.ListPayPoliciesAsync(context.User.GetTenantId(), cancellationToken));
        });
        timekeeping.MapPost("/pay-policies", async (UpsertPayPolicyRequest request, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingAdmin(context.User);
            return Results.Ok(await service.UpsertPayPolicyAsync(context.User.GetTenantId(), context.User.GetUserId(), null, request, cancellationToken));
        });
        timekeeping.MapPatch("/pay-policies/{id:guid}", async (Guid id, UpsertPayPolicyRequest request, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingAdmin(context.User);
            return Results.Ok(await service.UpsertPayPolicyAsync(context.User.GetTenantId(), context.User.GetUserId(), id, request, cancellationToken));
        });

        timekeeping.MapGet("/pay-codes", async (HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingRead(context.User);
            return Results.Ok(await service.ListPayCodesAsync(context.User.GetTenantId(), cancellationToken));
        });
        timekeeping.MapPost("/pay-codes", async (UpsertPayCodeRequest request, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingAdmin(context.User);
            return Results.Ok(await service.UpsertPayCodeAsync(context.User.GetTenantId(), context.User.GetUserId(), null, request, cancellationToken));
        });
        timekeeping.MapPatch("/pay-codes/{id:guid}", async (Guid id, UpsertPayCodeRequest request, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingAdmin(context.User);
            return Results.Ok(await service.UpsertPayCodeAsync(context.User.GetTenantId(), context.User.GetUserId(), id, request, cancellationToken));
        });

        timekeeping.MapPost("/clock-events", async (CreateClockEventRequest request, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingClock(context.User);
            return Results.Ok(await service.CreateClockEventAsync(context.User.GetTenantId(), context.User.GetUserId(), request, cancellationToken));
        });
        timekeeping.MapGet("/clock-events", async (Guid? personId, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingRead(context.User);
            return Results.Ok(await service.ListClockEventsAsync(context.User.GetTenantId(), personId, cancellationToken));
        });
        timekeeping.MapGet("/fieldcompanion/clock", async (HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingClock(context.User);
            var personId = context.User.GetPersonId();
            return personId == Guid.Empty
                ? Results.Forbid()
                : Results.Ok(await service.GetFieldCompanionClockStatusAsync(context.User.GetTenantId(), personId, cancellationToken));
        });
        timekeeping.MapPost("/fieldcompanion/clock", async (SubmitFieldCompanionClockEventRequest request, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingClock(context.User);
            var personId = context.User.GetPersonId();
            return personId == Guid.Empty
                ? Results.Forbid()
                : Results.Ok(await service.SubmitFieldCompanionClockEventAsync(context.User.GetTenantId(), context.User.GetUserId(), personId, request, cancellationToken));
        });

        timekeeping.MapGet("/work-sessions", async (Guid? personId, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingRead(context.User);
            return Results.Ok(await service.ListWorkSessionsAsync(context.User.GetTenantId(), personId, cancellationToken));
        });
        timekeeping.MapPost("/work-sessions", async (UpsertWorkSessionRequest request, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingManage(context.User);
            return Results.Ok(await service.UpsertWorkSessionAsync(context.User.GetTenantId(), context.User.GetUserId(), null, request, cancellationToken));
        });
        timekeeping.MapPatch("/work-sessions/{id:guid}", async (Guid id, UpsertWorkSessionRequest request, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingManage(context.User);
            return Results.Ok(await service.UpsertWorkSessionAsync(context.User.GetTenantId(), context.User.GetUserId(), id, request, cancellationToken));
        });

        timekeeping.MapGet("/time-entries", async (Guid? personId, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingRead(context.User);
            return Results.Ok(await service.ListTimeEntriesAsync(context.User.GetTenantId(), personId, cancellationToken));
        });
        timekeeping.MapPost("/time-entries", async (UpsertTimeEntryRequest request, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingManualEntry(context.User);
            return Results.Ok(await service.UpsertTimeEntryAsync(context.User.GetTenantId(), context.User.GetUserId(), null, request, cancellationToken));
        });
        timekeeping.MapPatch("/time-entries/{id:guid}", async (Guid id, UpsertTimeEntryRequest request, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingManualEntry(context.User);
            return Results.Ok(await service.UpsertTimeEntryAsync(context.User.GetTenantId(), context.User.GetUserId(), id, request, cancellationToken));
        });

        timekeeping.MapGet("/timesheets", async (Guid? personId, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingRead(context.User);
            return Results.Ok(await service.ListTimesheetsAsync(context.User.GetTenantId(), personId, cancellationToken));
        });
        timekeeping.MapGet("/timesheets/{id:guid}", async (Guid id, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingRead(context.User);
            return Results.Ok(await service.GetTimesheetAsync(context.User.GetTenantId(), id, cancellationToken));
        });
        timekeeping.MapPost("/timesheets", async (CreateTimesheetPeriodRequest request, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingManage(context.User);
            return Results.Ok(await service.CreateTimesheetPeriodAsync(context.User.GetTenantId(), context.User.GetUserId(), request, cancellationToken));
        });
        timekeeping.MapPost("/timesheets/{id:guid}/submit", async (Guid id, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingManage(context.User);
            return Results.Ok(await service.SubmitTimesheetAsync(context.User.GetTenantId(), context.User.GetUserId(), id, cancellationToken));
        });
        timekeeping.MapPost("/timesheets/{id:guid}/approve", async (Guid id, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingApprove(context.User);
            return Results.Ok(await service.ApproveTimesheetAsync(context.User.GetTenantId(), context.User.GetUserId(), id, cancellationToken));
        });
        timekeeping.MapPost("/timesheets/{id:guid}/reject", async (Guid id, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingApprove(context.User);
            return Results.Ok(await service.RejectTimesheetAsync(context.User.GetTenantId(), context.User.GetUserId(), id, cancellationToken));
        });
        timekeeping.MapPost("/timesheets/{id:guid}/reopen", async (Guid id, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingCorrect(context.User);
            return Results.Ok(await service.ReopenTimesheetAsync(context.User.GetTenantId(), context.User.GetUserId(), id, cancellationToken));
        });
        timekeeping.MapPost("/timesheets/{id:guid}/mark-payroll-ready", async (Guid id, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingPayrollReady(context.User);
            return Results.Ok(await service.MarkPayrollReadyAsync(context.User.GetTenantId(), context.User.GetUserId(), id, cancellationToken));
        });

        timekeeping.MapGet("/exceptions", async (Guid? personId, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingRead(context.User);
            return Results.Ok(await service.ListExceptionsAsync(context.User.GetTenantId(), personId, cancellationToken));
        });
        timekeeping.MapPost("/exceptions/{id:guid}/resolve", async (Guid id, ResolveTimeExceptionRequest request, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingApprove(context.User);
            return Results.Ok(await service.ResolveExceptionAsync(context.User.GetTenantId(), context.User.GetUserId(), id, request, cancellationToken));
        });

        timekeeping.MapPost("/corrections", async (CreateTimeCorrectionRequest request, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingCorrect(context.User);
            return Results.Ok(await service.CreateCorrectionAsync(context.User.GetTenantId(), context.User.GetUserId(), request, cancellationToken));
        });
        timekeeping.MapPost("/corrections/{id:guid}/approve", async (Guid id, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingApprove(context.User);
            return Results.Ok(await service.ApproveCorrectionAsync(context.User.GetTenantId(), context.User.GetUserId(), id, true, cancellationToken));
        });
        timekeeping.MapPost("/corrections/{id:guid}/reject", async (Guid id, HttpContext context, StaffArrAuthorizationService authorization, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireTimekeepingApprove(context.User);
            return Results.Ok(await service.ApproveCorrectionAsync(context.User.GetTenantId(), context.User.GetUserId(), id, false, cancellationToken));
        });

        integrations.MapPost("/labor-evidence", async (LaborEvidenceIngestRequest request, HttpContext context, StlServiceTokenValidator tokenValidator, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = request.SourceProductKey,
                    RequiredTargetProduct = "staffarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = LaborEvidenceIngestActionScope,
                });

            return Results.Ok(await service.IngestLaborEvidenceAsync(request, cancellationToken));
        }).WithName("IngestStaffArrLaborEvidence");

        integrations.MapGet("/timekeeping/person/{personId:guid}/payroll-ready-periods", async (Guid personId, Guid tenantId, HttpContext context, StlServiceTokenValidator tokenValidator, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "ledgarr",
                    RequiredTargetProduct = "staffarr",
                    TenantId = tenantId,
                    RequiredActionScope = LedgArrPayrollReadySnapshotActionScope,
                });
            return Results.Ok(await service.GetPayrollReadyPeriodsAsync(tenantId, personId, cancellationToken));
        }).WithName("ListStaffArrPayrollReadyPeriods");

        integrations.MapGet("/timekeeping/payroll-ready-snapshot", async (Guid tenantId, DateOnly? periodStartDate, DateOnly? periodEndDate, HttpContext context, StlServiceTokenValidator tokenValidator, TimekeepingService service, CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "ledgarr",
                    RequiredTargetProduct = "staffarr",
                    TenantId = tenantId,
                    RequiredActionScope = LedgArrPayrollReadySnapshotActionScope,
                });
            return Results.Ok(await service.GetPayrollReadySnapshotAsync(tenantId, periodStartDate, periodEndDate, cancellationToken));
        }).WithName("GetStaffArrPayrollReadySnapshot");
    }
}
