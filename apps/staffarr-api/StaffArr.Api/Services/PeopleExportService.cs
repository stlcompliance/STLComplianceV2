using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class PeopleExportService(
    StaffArrDbContext db,
    IStaffArrAuditService auditService,
    StaffArrTenantSettingsService tenantSettingsService)
{
    public const string CsvHeader =
        "givenName,familyName,primaryEmail,employmentStatus,jobTitle,managerEmail,primaryOrgUnitId,personId";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public PersonExportManifestResponse GetManifest() =>
        new(
            PackageVersion: "1",
            CsvHeader: CsvHeader,
            Formats:
            [
                new("csv", "text/csv", "people.csv", "Workforce directory CSV compatible with bulk import."),
                new("json", "application/json", "people.json", "Structured workforce directory export."),
                new("zip", "application/zip", "staffarr-people-export.zip", "ZIP bundle with manifest.json and people.csv."),
            ]);

    public async Task<PersonExportResponse> BuildExportAsync(
        Guid tenantId,
        Guid? actorUserId,
        string? employmentStatus,
        Guid? orgUnitId,
        CancellationToken cancellationToken = default)
    {
        await EnsureExportEnabledAsync(tenantId, cancellationToken);
        var people = await LoadPeopleAsync(tenantId, employmentStatus, orgUnitId, cancellationToken);
        var exportId = Guid.NewGuid();

        await auditService.WriteAsync(
            "person.export",
            tenantId,
            actorUserId,
            "person_export",
            exportId.ToString(),
            "success",
            reasonCode: $"{people.Count}",
            cancellationToken: cancellationToken);

        return new PersonExportResponse(
            exportId,
            tenantId,
            DateTimeOffset.UtcNow,
            people.Count,
            people);
    }

    public async Task<string> ExportCsvAsync(
        Guid tenantId,
        Guid? actorUserId,
        string? employmentStatus,
        Guid? orgUnitId,
        CancellationToken cancellationToken = default)
    {
        var package = await BuildExportAsync(tenantId, actorUserId, employmentStatus, orgUnitId, cancellationToken);
        return BuildCsv(package.People);
    }

    public async Task<byte[]> ExportZipAsync(
        Guid tenantId,
        Guid? actorUserId,
        string? employmentStatus,
        Guid? orgUnitId,
        CancellationToken cancellationToken = default)
    {
        var package = await BuildExportAsync(tenantId, actorUserId, employmentStatus, orgUnitId, cancellationToken);
        var csv = BuildCsv(package.People);

        await using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteTextEntry(archive, "manifest.json", JsonSerializer.Serialize(new
            {
                package.ExportId,
                package.TenantId,
                package.GeneratedAt,
                package.PersonCount,
                PackageVersion = "1",
                CsvHeader,
            }, JsonOptions));

            WriteTextEntry(archive, "people.csv", csv);
        }

        return memory.ToArray();
    }

    internal static string BuildCsv(IReadOnlyList<PersonExportRowItem> people)
    {
        var builder = new StringBuilder();
        builder.AppendLine(CsvHeader);

        foreach (var person in people)
        {
            builder.Append(EscapeCsv(person.GivenName));
            builder.Append(',');
            builder.Append(EscapeCsv(person.FamilyName));
            builder.Append(',');
            builder.Append(EscapeCsv(person.PrimaryEmail));
            builder.Append(',');
            builder.Append(EscapeCsv(person.EmploymentStatus));
            builder.Append(',');
            builder.Append(EscapeCsv(person.JobTitle ?? string.Empty));
            builder.Append(',');
            builder.Append(EscapeCsv(person.ManagerEmail ?? string.Empty));
            builder.Append(',');
            builder.Append(EscapeCsv(person.PrimaryOrgUnitId?.ToString() ?? string.Empty));
            builder.Append(',');
            builder.Append(EscapeCsv(person.PersonId.ToString()));
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private async Task<IReadOnlyList<PersonExportRowItem>> LoadPeopleAsync(
        Guid tenantId,
        string? employmentStatus,
        Guid? orgUnitId,
        CancellationToken cancellationToken)
    {
        var peopleQuery = db.People.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(employmentStatus))
        {
            var normalizedStatus = employmentStatus.Trim().ToLowerInvariant();
            peopleQuery = peopleQuery.Where(x => x.EmploymentStatus == normalizedStatus);
        }

        if (orgUnitId is Guid requestedOrgUnitId)
        {
            peopleQuery = peopleQuery.Where(x => x.PrimaryOrgUnitId == requestedOrgUnitId);
        }

        var people = await peopleQuery
            .OrderBy(x => x.DisplayName)
            .Select(x => new
            {
                x.Id,
                x.GivenName,
                x.FamilyName,
                x.PrimaryEmail,
                x.EmploymentStatus,
                x.JobTitle,
                x.ManagerPersonId,
                x.PrimaryOrgUnitId,
                PrimaryOrgUnitName = x.PrimaryOrgUnit != null ? x.PrimaryOrgUnit.Name : null,
                x.CreatedAt,
                x.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        var managerIds = people
            .Where(x => x.ManagerPersonId.HasValue)
            .Select(x => x.ManagerPersonId!.Value)
            .Distinct()
            .ToList();

        var managerEmails = managerIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.People
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && managerIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.PrimaryEmail, cancellationToken);

        return people
            .Select(x => new PersonExportRowItem(
                x.Id,
                x.GivenName,
                x.FamilyName,
                x.PrimaryEmail,
                x.EmploymentStatus,
                x.JobTitle,
                x.ManagerPersonId is Guid managerId && managerEmails.TryGetValue(managerId, out var managerEmail)
                    ? managerEmail
                    : null,
                x.PrimaryOrgUnitId,
                x.PrimaryOrgUnitName,
                x.CreatedAt,
                x.UpdatedAt))
            .ToList();
    }

    private async Task EnsureExportEnabledAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var settings = await tenantSettingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (!settings.ExportEnabled)
        {
            throw new StlApiException(
                "person_export.disabled",
                "StaffArr exports are disabled for this tenant.",
                409);
        }
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',', StringComparison.Ordinal)
            || value.Contains('"', StringComparison.Ordinal)
            || value.Contains('\n', StringComparison.Ordinal)
            || value.Contains('\r', StringComparison.Ordinal))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }

    private static void WriteTextEntry(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
        using var stream = entry.Open();
        var bytes = Encoding.UTF8.GetBytes(content);
        stream.Write(bytes, 0, bytes.Length);
    }
}
