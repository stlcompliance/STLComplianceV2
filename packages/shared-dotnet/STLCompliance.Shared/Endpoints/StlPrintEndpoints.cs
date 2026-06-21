using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using STLCompliance.Shared.Print;

namespace STLCompliance.Shared.Endpoints;

public static class StlPrintEndpoints
{
    public static void MapStlPrintEndpoints(this WebApplication app)
    {
        MapGroup(app.MapGroup("/api/print").WithTags("Print").RequireAuthorization(), string.Empty);
        MapGroup(app.MapGroup("/api/v1/print").WithTags("Print").RequireAuthorization(), "V1");
    }

    private static void MapGroup(RouteGroupBuilder group, string suffix)
    {
        group.MapGet("/templates", (ClaimsPrincipal principal, StlPrintLogService service) =>
                Results.Ok(service.GetTemplateCatalog(principal)))
            .WithName($"GetPrintTemplates{suffix}");

        group.MapGet("/templates/{templateKey}", (ClaimsPrincipal principal, string templateKey, StlPrintLogService service) =>
                Results.Ok(service.GetTemplate(principal, templateKey)))
            .WithName($"GetPrintTemplate{suffix}");

        group.MapGet("/history", async (
                ClaimsPrincipal principal,
                string sourceEntityType,
                string sourceEntityId,
                int? limit,
                StlPrintLogService service,
                CancellationToken cancellationToken) =>
                Results.Ok(await service.GetHistoryAsync(
                    principal,
                    sourceEntityType,
                    sourceEntityId,
                    limit,
                    cancellationToken)))
            .WithName($"GetPrintHistory{suffix}");

        group.MapPost("/preview", async (
                ClaimsPrincipal principal,
                StlPrintDocumentRequest request,
                StlPrintLogService service,
                CancellationToken cancellationToken) =>
                Results.Ok(await service.PreviewAsync(principal, request, cancellationToken)))
            .WithName($"CreatePrintPreview{suffix}");

        group.MapPost("/pdf", async (
                ClaimsPrincipal principal,
                StlPrintDocumentRequest request,
                StlPrintLogService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var response = await service.GeneratePdfAsync(principal, request, cancellationToken);
                context.Response.Headers.Append("X-Stl-Print-LogId", response.LogId.ToString());
                return Results.File(
                    response.File.Content,
                    response.File.ContentType,
                    response.File.FileName);
            })
            .WithName($"CreatePrintPdf{suffix}");

        group.MapPost("/archive", async (
                ClaimsPrincipal principal,
                StlPrintDocumentRequest request,
                StlPrintLogService service,
                CancellationToken cancellationToken) =>
                Results.Ok(await service.ArchiveOfficialAsync(principal, request, cancellationToken)))
            .WithName($"CreatePrintArchive{suffix}");

        group.MapPost("/browser-print-log", async (
                ClaimsPrincipal principal,
                StlBrowserPrintLogRequest request,
                StlPrintLogService service,
                CancellationToken cancellationToken) =>
                Results.Ok(await service.LogBrowserPrintAsync(principal, request, cancellationToken)))
            .WithName($"CreateBrowserPrintLog{suffix}");

        group.MapPost("/reprint", async (
                ClaimsPrincipal principal,
                StlReprintRequest request,
                StlPrintLogService service,
                CancellationToken cancellationToken) =>
                Results.Ok(await service.LogReprintAsync(principal, request, cancellationToken)))
            .WithName($"CreateReprintLog{suffix}");
    }
}
