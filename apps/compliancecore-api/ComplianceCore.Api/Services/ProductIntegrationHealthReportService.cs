using System.Text;
using ComplianceCore.Api.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class ProductIntegrationHealthReportService(
    FactSourceSyncHealthService syncHealthService)
{
    public const string CsvHeader =
        "factSourceId,sourceKey,factKey,sourceType,productKey,scopeKey,healthStatus,lastAttemptAt,lastSuccessAt,lastFailureAt,lastErrorMessage,consecutiveFailureCount";

    public async Task<ProductIntegrationHealthReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var health = await syncHealthService.GetHealthAsync(tenantId, cancellationToken);
        return new ProductIntegrationHealthReportSummaryResponse(
            health.TenantId,
            health.WorkerEnabled,
            health.IntervalMinutes,
            health.LastBatchRunAt,
            health.ProductApiSourceCount,
            health.HealthyCount,
            health.StaleCount,
            health.FailedCount,
            health.PendingCount,
            health.Sources);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(CsvHeader);

        foreach (var source in summary.Sources)
        {
            builder.Append(CsvEscape(source.FactSourceId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(source.SourceKey));
            builder.Append(',');
            builder.Append(CsvEscape(source.FactKey));
            builder.Append(',');
            builder.Append(CsvEscape(source.SourceType));
            builder.Append(',');
            builder.Append(CsvEscape(source.ProductKey ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(source.ScopeKey));
            builder.Append(',');
            builder.Append(CsvEscape(source.HealthStatus));
            builder.Append(',');
            builder.Append(CsvEscape(source.LastAttemptAt?.ToString("O") ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(source.LastSuccessAt?.ToString("O") ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(source.LastFailureAt?.ToString("O") ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(source.LastErrorMessage ?? string.Empty));
            builder.AppendLine(CsvEscape(source.ConsecutiveFailureCount.ToString()));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-product-integration-health-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
