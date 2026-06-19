using System.Security.Claims;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Options;
using Npgsql;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Operations;

namespace NexArr.Api.Services;

public sealed class ProductDatabaseNukeService(
    IConfiguration configuration,
    IHostEnvironment environment,
    IOptionsSnapshot<ProductDatabaseNukeOptions> options,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public const string ConfirmationHeaderValue = "DATABASE-NUKE";

    private const string AuditTargetType = "product_databases";
    private const string ReadyStatus = "ready";
    private const string MissingConnectionStatus = "missing_connection";
    private const string ErrorStatus = "error";
    private const string ExecutedStatus = "executed";
    private const string NoOpStatus = "no_op";
    private const string SkippedMissingConnectionStatus = "skipped_missing_connection";

    public async Task<DatabaseNukePreviewResponse> GetPreviewAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformOwnerAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var preview = await BuildPreviewAsync(cancellationToken);

        await audit.WriteAsync(
            "platform_admin.database_nuke.preview",
            AuditTargetType,
            "preview",
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return preview;
    }

    public async Task<DatabaseNukeExecutionResponse> ExecuteAsync(
        ClaimsPrincipal principal,
        ExecuteDatabaseNukeRequest request,
        string? confirmationToken,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformOwnerAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var settings = CurrentOptions;

        if (!settings.IsEnabled)
        {
            await WriteDeniedAuditAsync(actorUserId, "database_nuke_disabled", cancellationToken);
            throw new StlApiException(
                "database_nuke.disabled",
                "Database nuke is disabled for this environment.",
                409);
        }

        if (!string.Equals(confirmationToken?.Trim(), ConfirmationHeaderValue, StringComparison.Ordinal))
        {
            await WriteDeniedAuditAsync(actorUserId, "confirmation_header_required", cancellationToken);
            throw new StlApiException(
                "database_nuke.confirmation_header_required",
                "Database nuke confirmation header is required.",
                409);
        }

        if (!string.Equals(request.ConfirmationPhrase?.Trim(), settings.ConfirmationPhrase, StringComparison.Ordinal))
        {
            await WriteDeniedAuditAsync(actorUserId, "confirmation_phrase_required", cancellationToken);
            throw new StlApiException(
                "database_nuke.confirmation_phrase_required",
                "Database nuke confirmation phrase is required.",
                409);
        }

        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length < 10)
        {
            await WriteDeniedAuditAsync(actorUserId, "reason_required", cancellationToken);
            throw new StlApiException(
                "database_nuke.reason_required",
                "A reason of at least 10 characters is required.",
                400);
        }

        var runId = Guid.NewGuid();
        var startedAt = DateTimeOffset.UtcNow;
        var preview = await BuildPreviewAsync(cancellationToken);
        var previewErrors = preview.Targets
            .Where(t => t.ConnectionConfigured && string.Equals(t.Status, ErrorStatus, StringComparison.Ordinal))
            .ToList();
        if (previewErrors.Count > 0)
        {
            await audit.WriteAsync(
                "platform_admin.database_nuke.execute",
                AuditTargetType,
                runId.ToString(),
                "Failed",
                actorUserId: actorUserId,
                reasonCode: "preview_not_ready",
                cancellationToken: cancellationToken);
            throw new StlApiException(
                "database_nuke.preview_not_ready",
                "One or more configured product databases could not be previewed.",
                409);
        }

        var readyTargetsWithTables = preview.Targets
            .Where(t => string.Equals(t.Status, ReadyStatus, StringComparison.Ordinal) && t.TablesToTruncate.Count > 0)
            .ToList();
        if (readyTargetsWithTables.Count == 0)
        {
            await audit.WriteAsync(
                "platform_admin.database_nuke.execute",
                AuditTargetType,
                runId.ToString(),
                "Failed",
                actorUserId: actorUserId,
                reasonCode: "no_truncatable_tables",
                cancellationToken: cancellationToken);
            throw new StlApiException(
                "database_nuke.no_truncatable_tables",
                "No truncatable tables were found in the configured product databases.",
                409);
        }

        var results = new List<DatabaseNukeTargetExecutionResponse>();
        var hadFailure = false;
        foreach (var target in preview.Targets)
        {
            if (string.Equals(target.Status, MissingConnectionStatus, StringComparison.Ordinal))
            {
                results.Add(new DatabaseNukeTargetExecutionResponse(
                    target.ProductDatabase,
                    SkippedMissingConnectionStatus,
                    0,
                    target.PreserveTableCount,
                    0,
                    target.ErrorCode,
                    target.ErrorMessage));
                continue;
            }

            if (!string.Equals(target.Status, ReadyStatus, StringComparison.Ordinal))
            {
                results.Add(new DatabaseNukeTargetExecutionResponse(
                    target.ProductDatabase,
                    "skipped",
                    0,
                    target.PreserveTableCount,
                    0,
                    target.ErrorCode,
                    target.ErrorMessage));
                continue;
            }

            if (target.TablesToTruncate.Count == 0)
            {
                results.Add(new DatabaseNukeTargetExecutionResponse(
                    target.ProductDatabase,
                    NoOpStatus,
                    0,
                    target.PreserveTableCount,
                    0,
                    null,
                    null));
                continue;
            }

            try
            {
                var connectionString = ResolveConnectionString(target.ProductDatabase);
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    results.Add(new DatabaseNukeTargetExecutionResponse(
                        target.ProductDatabase,
                        SkippedMissingConnectionStatus,
                        0,
                        target.PreserveTableCount,
                        0,
                        "connection_missing",
                        "No connection string is configured for this product database."));
                    continue;
                }

                await TruncateTablesAsync(connectionString, target.TablesToTruncate, cancellationToken);
                results.Add(new DatabaseNukeTargetExecutionResponse(
                    target.ProductDatabase,
                    ExecutedStatus,
                    target.TruncateTableCount,
                    target.PreserveTableCount,
                    target.EstimatedRowsToDelete,
                    null,
                    null));
            }
            catch (Exception ex) when (ex is NpgsqlException or TimeoutException or ArgumentException or FormatException or InvalidOperationException)
            {
                hadFailure = true;
                results.Add(new DatabaseNukeTargetExecutionResponse(
                    target.ProductDatabase,
                    ErrorStatus,
                    0,
                    target.PreserveTableCount,
                    0,
                    "truncate_failed",
                    ex.Message));
            }
        }

        var completedAt = DateTimeOffset.UtcNow;
        await audit.WriteAsync(
            "platform_admin.database_nuke.execute",
            AuditTargetType,
            runId.ToString(),
            hadFailure ? "Failed" : "Success",
            actorUserId: actorUserId,
            reasonCode: hadFailure ? "truncate_failed" : "operator_confirmed",
            cancellationToken: cancellationToken);

        return new DatabaseNukeExecutionResponse(
            runId,
            results,
            results.Sum(r => r.TruncatedTableCount),
            preview.Targets.Sum(t => t.PreserveTableCount),
            results.Sum(r => r.EstimatedRowsDeleted),
            startedAt,
            completedAt);
    }

    private ProductDatabaseNukeOptions CurrentOptions => options.Value;

    private async Task<DatabaseNukePreviewResponse> BuildPreviewAsync(CancellationToken cancellationToken)
    {
        var targets = new List<DatabaseNukeTargetPreviewResponse>();
        foreach (var productDatabase in StlProductDatabaseCatalog.All)
        {
            targets.Add(await BuildTargetPreviewAsync(productDatabase, cancellationToken));
        }

        var settings = CurrentOptions;
        return new DatabaseNukePreviewResponse(
            settings.IsEnabled,
            settings.ConfirmationPhrase,
            targets,
            DateTimeOffset.UtcNow);
    }

    private async Task<DatabaseNukeTargetPreviewResponse> BuildTargetPreviewAsync(
        string productDatabase,
        CancellationToken cancellationToken)
    {
        string? connectionString;
        try
        {
            connectionString = ResolveConnectionString(productDatabase);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or InvalidOperationException)
        {
            return new DatabaseNukeTargetPreviewResponse(
                productDatabase,
                ErrorStatus,
                true,
                0,
                0,
                0,
                0,
                0,
                [],
                [],
                "connection_invalid",
                ex.Message);
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new DatabaseNukeTargetPreviewResponse(
                productDatabase,
                MissingConnectionStatus,
                false,
                0,
                0,
                0,
                0,
                0,
                [],
                [],
                "connection_missing",
                "No connection string is configured for this product database.");
        }

        try
        {
            var tables = await LoadTablesAsync(productDatabase, connectionString, cancellationToken);
            var truncateTables = tables
                .Where(t => string.Equals(t.Disposition, ProductDatabaseNukeTableDispositions.Truncate, StringComparison.Ordinal))
                .ToList();
            var preservedTables = tables
                .Where(t => string.Equals(t.Disposition, ProductDatabaseNukeTableDispositions.Preserve, StringComparison.Ordinal))
                .ToList();

            return new DatabaseNukeTargetPreviewResponse(
                productDatabase,
                ReadyStatus,
                true,
                tables.Count,
                truncateTables.Count,
                preservedTables.Count,
                truncateTables.Sum(t => t.EstimatedRows),
                preservedTables.Sum(t => t.EstimatedRows),
                truncateTables,
                preservedTables,
                null,
                null);
        }
        catch (Exception ex) when (ex is NpgsqlException or TimeoutException or ArgumentException or FormatException or InvalidOperationException)
        {
            return new DatabaseNukeTargetPreviewResponse(
                productDatabase,
                ErrorStatus,
                true,
                0,
                0,
                0,
                0,
                0,
                [],
                [],
                "preview_failed",
                ex.Message);
        }
    }

    private async Task<IReadOnlyList<DatabaseNukeTablePreviewResponse>> LoadTablesAsync(
        string productDatabase,
        string connectionString,
        CancellationToken cancellationToken)
    {
        var tables = new List<DatabaseNukeTablePreviewResponse>();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandTimeout = Math.Max(1, CurrentOptions.CommandTimeoutSeconds);
        command.CommandText = """
            SELECT n.nspname,
                   c.relname,
                   GREATEST(c.reltuples::bigint, 0) AS estimated_rows
            FROM pg_class c
            JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE c.relkind = 'r'
              AND n.nspname NOT IN ('pg_catalog', 'information_schema')
            ORDER BY n.nspname, c.relname;
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var schema = reader.GetString(0);
            var table = reader.GetString(1);
            var estimatedRows = reader.GetInt64(2);
            var classification = ProductDatabaseNukeTableClassifier.Classify(productDatabase, schema, table);
            tables.Add(new DatabaseNukeTablePreviewResponse(
                schema,
                table,
                classification.Disposition,
                classification.Reason,
                estimatedRows));
        }

        return tables;
    }

    private async Task TruncateTablesAsync(
        string connectionString,
        IReadOnlyList<DatabaseNukeTablePreviewResponse> tables,
        CancellationToken cancellationToken)
    {
        if (tables.Count == 0)
        {
            return;
        }

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandTimeout = Math.Max(1, CurrentOptions.CommandTimeoutSeconds);
        command.CommandText = $"TRUNCATE TABLE {string.Join(", ", tables.Select(QuoteTableName))} RESTART IDENTITY;";

        await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private string? ResolveConnectionString(string productDatabase)
    {
        var configured = ResolveConfiguredConnectionString(productDatabase);
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        var nexArrConnectionString = StlDatabaseConnection.Resolve(configuration);
        if (productDatabase.Equals(StlProductDatabaseCatalog.NexArr, StringComparison.OrdinalIgnoreCase))
        {
            return nexArrConnectionString;
        }

        if (!CurrentOptions.AllowLocalDatabaseNameFallback || !IsLocalEnvironment())
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(nexArrConnectionString))
        {
            return null;
        }

        var builder = new NpgsqlConnectionStringBuilder(nexArrConnectionString)
        {
            Database = productDatabase,
        };
        return builder.ConnectionString;
    }

    private string? ResolveConfiguredConnectionString(string productDatabase)
    {
        if (CurrentOptions.ProductDatabases is not null
            && CurrentOptions.ProductDatabases.TryGetValue(productDatabase, out var target)
            && !string.IsNullOrWhiteSpace(target.ConnectionString))
        {
            return StlDatabaseConnection.Normalize(target.ConnectionString);
        }

        var configured = configuration[$"{ProductDatabaseNukeOptions.SectionName}:ProductDatabases:{productDatabase}:ConnectionString"]
            ?? configuration[$"{ProductDatabaseNukeOptions.SectionName}__ProductDatabases__{productDatabase}__ConnectionString"];
        return StlDatabaseConnection.Normalize(configured);
    }

    private bool IsLocalEnvironment() =>
        environment.IsDevelopment()
        || string.Equals(environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase);

    private Task WriteDeniedAuditAsync(
        Guid actorUserId,
        string reasonCode,
        CancellationToken cancellationToken) =>
        audit.WriteAsync(
            "platform_admin.database_nuke.execute",
            AuditTargetType,
            null,
            "Denied",
            actorUserId: actorUserId,
            reasonCode: reasonCode,
            cancellationToken: cancellationToken);

    private static string QuoteTableName(DatabaseNukeTablePreviewResponse table) =>
        $"{QuoteIdentifier(table.Schema)}.{QuoteIdentifier(table.Table)}";

    private static string QuoteIdentifier(string value) =>
        $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
