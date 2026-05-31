using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;

namespace SupplyArr.Api.Services;

public sealed class ContactsCsvImportService(
    SupplyArrDbContext db,
    ExternalPartyService parties)
{
    private const string ImportType = "contacts_csv";

    private static readonly string[] Headers =
    [
        "party_key",
        "contact_name",
        "email",
        "phone",
        "role_label",
        "is_primary"
    ];

    public async Task<ContactsCsvImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        ContactsCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<ContactsCsvImportIssue>();
        var rows = Parse(request.Csv, issues);
        if (issues.Count > 0)
        {
            return BuildResponse(request.DryRun, rows.Count, 0, 0, issues);
        }

        var partyIdsByKey = await db.ExternalParties
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.PartyKey, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var accepted = 0;

        foreach (var row in rows)
        {
            ValidateRow(row, partyIdsByKey, issues);
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
            await parties.AddContactAsync(
                tenantId,
                actorUserId,
                partyIdsByKey[row.PartyKey],
                new CreatePartyContactRequest(
                    row.ContactName,
                    row.Email,
                    row.Phone,
                    row.RoleLabel,
                    row.IsPrimary),
                cancellationToken);
            created++;
        }

        return BuildResponse(false, rows.Count, accepted, created, issues);
    }

    private static ContactsCsvImportResponse BuildResponse(
        bool dryRun,
        int rowsRead,
        int contactsAccepted,
        int contactsCreated,
        IReadOnlyList<ContactsCsvImportIssue> issues) =>
        new(ImportType, dryRun, issues.Count == 0, rowsRead, contactsAccepted, contactsCreated, issues);

    private static IReadOnlyList<ImportRow> Parse(string csv, List<ContactsCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            issues.Add(new ContactsCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            issues.Add(new ContactsCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var headerFields = ParseRow(lines[0]);
        if (headerFields.Count != Headers.Length || !headerFields.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase))
        {
            issues.Add(new ContactsCsvImportIssue(1, "csv.header", $"Header must be: {string.Join(",", Headers)}"));
            return [];
        }

        var rows = new List<ImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = ParseRow(lines[index]);
            if (fields.Count != Headers.Length)
            {
                issues.Add(new ContactsCsvImportIssue(
                    index + 1,
                    "csv.columns",
                    $"Expected {Headers.Length} columns but found {fields.Count}."));
                continue;
            }

            rows.Add(new ImportRow(
                index + 1,
                NormalizeKey(fields[0]),
                NormalizeOptional(fields[1]),
                NormalizeOptional(fields[2]),
                NormalizeOptional(fields[3]),
                NormalizeOptional(fields[4]),
                ParseBool(fields[5], index + 1, issues)));
        }

        return rows;
    }

    private static void ValidateRow(
        ImportRow row,
        IReadOnlyDictionary<string, Guid> partyIdsByKey,
        List<ContactsCsvImportIssue> issues)
    {
        ValidateLength(row.LineNumber, "party_key", row.PartyKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "contact_name", row.ContactName, 2, 128, issues);
        if (!partyIdsByKey.ContainsKey(row.PartyKey))
        {
            issues.Add(new ContactsCsvImportIssue(row.LineNumber, "party.not_found", "Party key was not found."));
        }
    }

    private static void ValidateLength(
        int lineNumber,
        string column,
        string value,
        int minLength,
        int maxLength,
        List<ContactsCsvImportIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new ContactsCsvImportIssue(
                lineNumber,
                "csv.validation",
                $"{column} must be between {minLength} and {maxLength} characters."));
        }
    }

    private static bool ParseBool(string value, int lineNumber, List<ContactsCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (bool.TryParse(value.Trim(), out var parsed))
        {
            return parsed;
        }

        issues.Add(new ContactsCsvImportIssue(lineNumber, "csv.boolean", "is_primary must be true or false."));
        return false;
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

    private sealed record ImportRow(
        int LineNumber,
        string PartyKey,
        string ContactName,
        string Email,
        string Phone,
        string RoleLabel,
        bool IsPrimary);
}
