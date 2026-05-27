using Npgsql;

namespace STLCompliance.Shared.Operations;

public sealed record StlDrRestoreValidationResult(
    string Database,
    bool Connected,
    int MigrationHistoryCount,
    bool PlatformMetadataTableExists,
    int PlatformMetadataCount,
    IReadOnlyList<string> Errors)
{
    public bool IsValid => Connected && MigrationHistoryCount > 0 && Errors.Count == 0;
}

/// <summary>
/// Post-restore validation queries used by DR restore drill scripts and automated tests.
/// </summary>
public static class StlDrRestoreDrillValidator
{
    public static async Task<StlDrRestoreValidationResult> ValidateRestoredDatabaseAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var errors = new List<string>();
        var connected = false;
        var migrationCount = 0;
        var platformMetadataExists = false;
        var platformMetadataCount = 0;

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            connected = true;

            migrationCount = await ScalarIntAsync(
                connection,
                "SELECT COUNT(*) FROM \"__EFMigrationsHistory\"",
                cancellationToken);

            if (migrationCount <= 0)
            {
                errors.Add("Expected at least one EF migration history row.");
            }

            platformMetadataExists = await TableExistsAsync(connection, "platform_metadata", cancellationToken);
            if (!platformMetadataExists)
            {
                errors.Add("Expected platform_metadata table to exist.");
            }
            else
            {
                platformMetadataCount = await ScalarIntAsync(
                    connection,
                    "SELECT COUNT(*) FROM platform_metadata",
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            errors.Add(ex.Message);
        }

        return new StlDrRestoreValidationResult(
            builder.Database ?? string.Empty,
            connected,
            migrationCount,
            platformMetadataExists,
            platformMetadataCount,
            errors);
    }

    private static async Task<bool> TableExistsAsync(
        NpgsqlConnection connection,
        string tableName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT EXISTS (
              SELECT 1
              FROM information_schema.tables
              WHERE table_schema = 'public'
                AND table_name = @tableName
            )
            """;
        command.Parameters.AddWithValue("tableName", tableName);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is bool exists && exists;
    }

    private static async Task<int> ScalarIntAsync(
        NpgsqlConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result switch
        {
            int value => value,
            long longValue => (int)longValue,
            decimal decimalValue => (int)decimalValue,
            _ => 0,
        };
    }
}
