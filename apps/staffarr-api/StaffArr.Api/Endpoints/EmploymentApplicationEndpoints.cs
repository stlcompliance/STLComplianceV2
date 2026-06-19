using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class EmploymentApplicationEndpoints
{
    public static void MapStaffArrEmploymentApplicationEndpoints(this WebApplication app)
    {
        var authenticated = app.MapGroup("/api/v1/employment-applications").WithTags("Employment applications").RequireAuthorization();

        authenticated.MapGet("/templates", async (
            HttpContext context,
            StaffArrAuthorizationService authorization,
            EmploymentApplicationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsView(context.User);
            return Results.Ok(await service.ListTemplatesAsync(context.User.GetTenantId(), cancellationToken));
        })
        .WithName("ListStaffArrEmploymentApplicationTemplatesV1");

        authenticated.MapGet("/templates/{templateId:guid}", async (
            Guid templateId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            EmploymentApplicationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsView(context.User);
            return Results.Ok(await service.GetTemplateAsync(context.User.GetTenantId(), templateId, cancellationToken));
        })
        .WithName("GetStaffArrEmploymentApplicationTemplateV1");

        authenticated.MapPost("/templates", async (
            EmploymentApplicationTemplateCreateRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            EmploymentApplicationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsManage(context.User);
            var created = await service.CreateTemplateAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                request,
                cancellationToken);
            return Results.Created($"/api/v1/employment-applications/templates/{created.EmploymentApplicationTemplateId}", created);
        })
        .WithName("CreateStaffArrEmploymentApplicationTemplateV1");

        authenticated.MapPut("/templates/{templateId:guid}", async (
            Guid templateId,
            EmploymentApplicationTemplateUpsertRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            EmploymentApplicationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsManage(context.User);
            var updated = await service.UpdateTemplateAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                templateId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("UpdateStaffArrEmploymentApplicationTemplateV1");

        authenticated.MapPost("/templates/{templateId:guid}/publish", async (
            Guid templateId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            EmploymentApplicationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsManage(context.User);
            var published = await service.PublishTemplateAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                templateId,
                cancellationToken);
            return Results.Ok(published);
        })
        .WithName("PublishStaffArrEmploymentApplicationTemplateV1");

        authenticated.MapPost("/templates/{templateId:guid}/clone", async (
            Guid templateId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            EmploymentApplicationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsManage(context.User);
            var cloned = await service.CloneTemplateAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                templateId,
                cancellationToken);
            return Results.Created($"/api/v1/employment-applications/templates/{cloned.EmploymentApplicationTemplateId}", cloned);
        })
        .WithName("CloneStaffArrEmploymentApplicationTemplateV1");

        authenticated.MapGet("/submissions", async (
            int? limit,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            EmploymentApplicationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.ListSubmissionsAsync(
                context.User.GetTenantId(),
                limit ?? 20,
                cancellationToken));
        })
        .WithName("ListStaffArrEmploymentApplicationSubmissionsV1");

        var publicGroup = app.MapGroup("/api/public/employment-applications").WithTags("Public employment applications");

        publicGroup.MapGet("/{publicToken}", async (
            string publicToken,
            EmploymentApplicationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetPublicTemplateAsync(publicToken, cancellationToken));
        })
        .WithName("GetPublicEmploymentApplicationTemplate");

        publicGroup.MapPost("/{publicToken}/submissions", async (
            string publicToken,
            SubmitEmploymentApplicationRequest request,
            HttpContext context,
            EmploymentApplicationService service,
            CancellationToken cancellationToken) =>
        {
            var submission = await service.SubmitPublicAsync(
                publicToken,
                request,
                context.Connection.RemoteIpAddress?.ToString(),
                context.Request.Headers.UserAgent.ToString(),
                cancellationToken);

            return Results.Created($"/api/public/employment-applications/{publicToken}/submissions/{submission.EmploymentApplicationSubmissionId}", submission);
        })
        .WithName("SubmitPublicEmploymentApplication");
    }
}
