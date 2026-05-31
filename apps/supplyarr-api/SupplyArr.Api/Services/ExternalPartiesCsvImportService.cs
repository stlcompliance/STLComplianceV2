using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;

namespace SupplyArr.Api.Services;

public sealed class ExternalPartiesCsvImportService(
    SupplyArrDbContext db,
    ExternalPartyService parties)
{
    private const string ImportType = "external_parties_csv";

    private static readonly string[] Headers =
    [
        "party_key",
        "party_type",
        "display_name",
        "legal_name",
        "tax_identifier",
        "approval_status",
        "status",
        "notes"
    ];

    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "vendor",
        "dealer",
        "supplier",
        "customer"
    };

    private static readonly HashSet<string> AllowedApprovalStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "pending",
        "approved",
        "restricted",
        "inactive"
    };

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "inactive"
    };

    public async Task<ExternalPartiesCsvImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        ExternalPartiesCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<ExternalPartiesCsvImportIssue>();
        var rows = Parse(request.Csv, issues);
        if (issues.Count > 0)
        {
            return BuildResponse(request.DryRun, rows.Count, 0, 0, issues);
        }

        var existingKeys = (await db.ExternalParties
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.PartyKey)
            .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var accepted = 0;

        foreach (var row in rows)
        {
            ValidateRow(row, existingKeys, seenKeys, issues);
            if (issues.Any(x => x.LineNumber == row.LineNumber))
            {
                continue;
            }

            accepted++;
        }

        if (issues.Count > 0 || request.DryRun)
        {
            return BuildResponse(request.DryRun, rows.Count, accepted, 0, issues);
        }

        var created = 0;
        foreach (var row in rows)
        {
            var party = await parties.CreateAsync(
                tenantId,
                actorUserId,
                new CreateExternalPartyRequest(
                    row.PartyKey,
                    row.PartyType,
                    row.DisplayName,
                    row.LegalName,
                    row.TaxIdentifier,
                    row.Notes),
                cancellationToken);

            if (!string.Equals(row.ApprovalStatus, "pending", StringComparison.OrdinalIgnoreCase))
            {
                await parties.UpdateApprovalStatusAsync(
                    tenantId,
                    actorUserId,
                    party.PartyId,
                    new UpdateExternalPartyApprovalStatusRequest(row.ApprovalStatus),
                    cancellationToken);
            }

            if (!string.Equals(row.Status, "active", StringComparison.OrdinalIgnoreCase))
            {
                await parties.UpdateStatusAsync(
                    tenantId,
                    actorUserId,
                    party.PartyId,
                    new UpdateExternalPartyStatusRequest(row.Status),
                    cancellationToken);
            }

            created++;
        }

        return BuildResponse(false, rows.Count, accepted, created, issues);
    }

    private static ExternalPartiesCsvImportResponse BuildResponse(
        bool dryRun,
        int rowsRead,
        int partiesAccepted,
        int partiesCreated,
        IReadOnlyList<ExternalPartiesCsvImportIssue> issues) =>
        new(ImportType, dryRun, issues.Count == 0, rowsRead, partiesAccepted, partiesCreated, issues);

    private static IReadOnlyList<ImportRow> Parse(string csv, List<ExternalPartiesCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            issues.Add(new ExternalPartiesCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var headerFields = ParseRow(lines[0]);
        if (headerFields.Count != Headers.Length || !headerFields.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(1, "csv.header", $"Header must be: {string.Join(",", Headers)}"));
            return [];
        }

        var rows = new List<ImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = ParseRow(lines[index]);
            if (fields.Count != Headers.Length)
            {
                issues.Add(new ExternalPartiesCsvImportIssue(
                    index + 1,
                    "csv.columns",
                    $"Expected {Headers.Length} columns but found {fields.Count}."));
                continue;
            }

            rows.Add(new ImportRow(
                index + 1,
                NormalizeKey(fields[0]),
                NormalizeKey(fields[1]),
                NormalizeOptional(fields[2]),
                NormalizeOptional(fields[3]),
                NormalizeNullable(fields[4]),
                NormalizeStatus(fields[5], "pending"),
                NormalizeStatus(fields[6], "active"),
                NormalizeOptional(fields[7])));
        }

        return rows;
    }

    private static void ValidateRow(
        ImportRow row,
        ISet<string> existingKeys,
        ISet<string> seenKeys,
        List<ExternalPartiesCsvImportIssue> issues)
    {
        ValidateLength(row.LineNumber, "party_key", row.PartyKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "display_name", row.DisplayName, 2, 256, issues);
        ValidateMaxLength(row.LineNumber, "legal_name", row.LegalName, 256, issues);
        if (!AllowedTypes.Contains(row.PartyType))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "party.type_invalid", "party_type must be vendor, dealer, supplier, or customer."));
        }

        if (!AllowedApprovalStatuses.Contains(row.ApprovalStatus))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "party.approval_status_invalid", "approval_status must be pending, approved, restricted, or inactive."));
        }

        if (!AllowedStatuses.Contains(row.Status))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "party.status_invalid", "status must be active or inactive."));
        }

        if (existingKeys.Contains(row.PartyKey))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "party.duplicate", "Party key already exists."));
        }

        if (!seenKeys.Add(row.PartyKey))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "party.duplicate_in_file", "Party key appears more than once in the import file."));
        }
    }

    private static void ValidateLength(
        int lineNumber,
        string column,
        string value,
        int minLength,
        int maxLength,
        List<ExternalPartiesCsvImportIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new ExternalPartiesCsvImportIssue(
                lineNumber,
                "csv.validation",
                $"{column} must be between {minLength} and {maxLength} characters."));
        }
    }

    private static void ValidateMaxLength(
        int lineNumber,
        string column,
        string value,
        int maxLength,
        List<ExternalPartiesCsvImportIssue> issues)
    {
        if (value.Length > maxLength)
        {
            issues.Add(new ExternalPartiesCsvImportIssue(
                lineNumber,
                "csv.validation",
                $"{column} must be {maxLength} characters or fewer."));
        }
    }

    private static IReadOnlyList<string> ParseRow(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (inQuotes)
            {
                if (character == '"')
                {
                    if (index + 1 < line.Length && line[index + 1] == '"')
                    {
                        current.Append('"');
                        index++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(character);
                }

                continue;
            }

            if (character == '"')
            {
                inQuotes = true;
                continue;
            }

            if (character == ',')
            {
                fields.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        fields.Add(current.ToString().Trim());
        return fields;
    }

    private static string NormalizeKey(string value) => value.Trim().ToLowerInvariant();

    private static string NormalizeOptional(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string? NormalizeNullable(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeStatus(string value, string defaultValue) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim().ToLowerInvariant();

    private sealed record ImportRow(
        int LineNumber,
        string PartyKey,
        string PartyType,
        string DisplayName,
        string LegalName,
        string? TaxIdentifier,
        string ApprovalStatus,
        string Status,
        string Notes);
}
