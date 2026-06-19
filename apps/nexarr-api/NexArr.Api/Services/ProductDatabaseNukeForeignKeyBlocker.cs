using NexArr.Api.Contracts;

namespace NexArr.Api.Services;

public sealed record ProductDatabaseNukeForeignKeyReference(
    string ChildSchema,
    string ChildTable,
    string ParentSchema,
    string ParentTable);

public sealed record ProductDatabaseNukeForeignKeyBlocker(
    string ParentSchema,
    string ParentTable,
    IReadOnlyList<ProductDatabaseNukeForeignKeyReference> ReferencingTables);

public static class ProductDatabaseNukeForeignKeyBlockerFinder
{
    public static IReadOnlyList<ProductDatabaseNukeForeignKeyBlocker> FindBlockers(
        IReadOnlyList<DatabaseNukeTablePreviewResponse> tables,
        IReadOnlyList<ProductDatabaseNukeForeignKeyReference> foreignKeys)
    {
        var truncateTables = tables
            .Where(table => string.Equals(table.Disposition, ProductDatabaseNukeTableDispositions.Truncate, StringComparison.Ordinal))
            .Select(table => TableKey(table.Schema, table.Table))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var preservedTables = tables
            .Where(table => string.Equals(table.Disposition, ProductDatabaseNukeTableDispositions.Preserve, StringComparison.Ordinal))
            .Select(table => TableKey(table.Schema, table.Table))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var blockers = foreignKeys
            .Where(foreignKey =>
                truncateTables.Contains(TableKey(foreignKey.ParentSchema, foreignKey.ParentTable))
                && preservedTables.Contains(TableKey(foreignKey.ChildSchema, foreignKey.ChildTable)))
            .GroupBy(foreignKey => TableKey(foreignKey.ParentSchema, foreignKey.ParentTable), StringComparer.OrdinalIgnoreCase)
            .Select(group =>
                new ProductDatabaseNukeForeignKeyBlocker(
                    group.First().ParentSchema,
                    group.First().ParentTable,
                    group.OrderBy(item => item.ChildSchema, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(item => item.ChildTable, StringComparer.OrdinalIgnoreCase)
                        .ToList()))
            .OrderBy(blocker => blocker.ParentSchema, StringComparer.OrdinalIgnoreCase)
            .ThenBy(blocker => blocker.ParentTable, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return blockers;
    }

    public static string BuildBlockerMessage(
        string productDatabase,
        IReadOnlyList<ProductDatabaseNukeForeignKeyBlocker> blockers)
    {
        if (blockers.Count == 0)
        {
            return string.Empty;
        }

        var snippets = blockers
            .Take(5)
            .Select(blocker =>
            {
                var sources = string.Join(
                    ", ",
                    blocker.ReferencingTables
                        .Take(3)
                        .Select(reference => FormatTable(reference.ChildSchema, reference.ChildTable)));

                if (blocker.ReferencingTables.Count > 3)
                {
                    sources = $"{sources} and {blocker.ReferencingTables.Count - 3} more";
                }

                return $"{FormatTable(blocker.ParentSchema, blocker.ParentTable)} is referenced by {sources}";
            })
            .ToList();

        if (blockers.Count > 5)
        {
            snippets.Add($"{blockers.Count - 5} additional blocker(s) omitted");
        }

        return $"Foreign key blockers prevent truncation in {productDatabase}: {string.Join("; ", snippets)}.";
    }

    private static string TableKey(string schema, string table) => $"{schema}.{table}";

    private static string FormatTable(string schema, string table) => $"{schema}.{table}";
}
