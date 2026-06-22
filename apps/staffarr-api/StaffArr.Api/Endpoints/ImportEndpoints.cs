using System.Text;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class ImportEndpoints
{
    private const string ProductKey = "staffarr";
    private const string ImportTypeKey = "people";
    private const string TemplateVersion = "2026-06";
    private const string ImportPermission = "staffarr.import.people";
    private const string TemplateFileName = "staffarr-people-import-template-v2026-06.csv";
    private const string TemplateCsv =
        """
        givenName,familyName,primaryEmail,employmentStatus,jobTitle,managerEmail
        Jane,Doe,jane.doe@example.com,active,Technician,
        John,Smith,john.smith@example.com,active,Lead,jane.doe@example.com
        """;

    public static void MapStaffArrImportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/imports")
            .WithTags("Imports")
            .RequireAuthorization();

        group.MapGet("/manifests", (
            HttpContext context,
            StaffArrAuthorizationService authorization) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(new[] { BuildManifest() });
        })
        .WithName("ListStaffArrImportManifestsV1");

        group.MapGet("/manifests/{importTypeKey}/template", (
            string importTypeKey,
            HttpContext context,
            StaffArrAuthorizationService authorization) =>
        {
            authorization.RequirePeopleRead(context.User);
            if (!string.Equals(importTypeKey, ImportTypeKey, StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound();
            }

            return Results.File(
                Encoding.UTF8.GetBytes(TemplateCsv),
                "text/csv",
                TemplateFileName);
        })
        .WithName("DownloadStaffArrImportTemplateV1");

        group.MapGet("/history", async (
            int? limit,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            StaffArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            var tenantId = context.User.GetTenantId();
            var take = Math.Clamp(limit ?? 25, 1, 100);

            var events = await db.AuditEvents
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.TargetType == "person_import")
                .OrderByDescending(x => x.OccurredAt)
                .Take(take)
                .ToListAsync(cancellationToken);

            return Results.Ok(new ProductImportHistoryListResponse(events.Select(MapHistoryItem).ToList()));
        })
        .WithName("ListStaffArrImportHistoryV1");

        group.MapPost("/people", async (
            BulkPersonImportRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PeopleBulkImportService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ImportAsync(tenantId, actorUserId, request, cancellationToken));
        })
        .WithName("ImportStaffArrPeopleV1");
    }

    private static ProductImportManifestResponse BuildManifest() =>
        new(
            ProductKey,
            ImportTypeKey,
            "People",
            "Create StaffArr people records from a deterministic CSV template owned by the workforce product.",
            ["csv"],
            TemplateVersion,
            ImportPermission,
            "staff_person",
            ["create"],
            ["givenName", "familyName", "primaryEmail"],
            ["employmentStatus", "jobTitle", "managerEmail"],
            ["employmentStatus"],
            ["managerEmail"],
            ["primaryEmail"],
            ["primaryEmail", "givenName+familyName when tenant policy allows"],
            [
                "Required field missing",
                "Invalid email format",
                "Invalid employment status",
                "Unknown manager reference",
                "Duplicate email in import file",
                "Duplicate email against existing people"
            ],
            ["givenName", "familyName", "primaryEmail", "employmentStatus", "jobTitle", "managerEmail"],
            "Uses StaffArr people services and emits normal person lifecycle events after reviewable validation.",
            ["staffarr.person.created"],
            false,
            "staffarr.people.import");

    private static ProductImportHistoryItemResponse MapHistoryItem(StaffArrAuditEvent auditEvent)
    {
        var metrics = ParseReasonCode(auditEvent.ReasonCode);
        return new ProductImportHistoryItemResponse(
            auditEvent.Id,
            ImportTypeKey,
            "People",
            auditEvent.Result,
            metrics.DryRun,
            metrics.RowCount,
            metrics.SuccessCount,
            metrics.ErrorCount,
            auditEvent.ActorUserId,
            null,
            auditEvent.OccurredAt,
            metrics.DryRun
                ? $"Validated {metrics.SuccessCount} of {metrics.RowCount} rows."
                : $"Imported {metrics.SuccessCount} of {metrics.RowCount} rows.");
    }

    private static (bool DryRun, int RowCount, int SuccessCount, int ErrorCount) ParseReasonCode(string? reasonCode)
    {
        var rowCount = 0;
        var createdCount = 0;
        var validatedCount = 0;
        var errorCount = 0;
        var dryRun = false;

        if (!string.IsNullOrWhiteSpace(reasonCode))
        {
            foreach (var part in reasonCode.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var separatorIndex = part.IndexOf(':', StringComparison.Ordinal);
                if (separatorIndex <= 0 || separatorIndex >= part.Length - 1)
                {
                    continue;
                }

                var key = part[..separatorIndex];
                var value = part[(separatorIndex + 1)..];
                switch (key.ToLowerInvariant())
                {
                    case "rows" when int.TryParse(value, out var parsedRows):
                        rowCount = parsedRows;
                        break;
                    case "created" when int.TryParse(value, out var parsedCreated):
                        createdCount = parsedCreated;
                        break;
                    case "validated" when int.TryParse(value, out var parsedValidated):
                        validatedCount = parsedValidated;
                        break;
                    case "errors" when int.TryParse(value, out var parsedErrors):
                        errorCount = parsedErrors;
                        break;
                    case "dryrun" when bool.TryParse(value, out var parsedDryRun):
                        dryRun = parsedDryRun;
                        break;
                }
            }
        }

        return (dryRun, rowCount, dryRun ? validatedCount : createdCount, errorCount);
    }
}
