using System.Text;
using NexArr.Api.Contracts;

namespace NexArr.Api.Services;

public static class PlatformAuditPackageCsvWriter
{
    public static byte[] WriteAuditEvents(IReadOnlyList<PlatformAuditEventExportItem> events)
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            "audit_event_id,tenant_id,actor_user_id,action,target_type,target_id,result,reason_code,correlation_id,occurred_at");

        foreach (var item in events)
        {
            builder.Append(Escape(item.AuditEventId.ToString()));
            builder.Append(',');
            builder.Append(Escape(item.TenantId?.ToString()));
            builder.Append(',');
            builder.Append(Escape(item.ActorUserId?.ToString()));
            builder.Append(',');
            builder.Append(Escape(item.Action));
            builder.Append(',');
            builder.Append(Escape(item.TargetType));
            builder.Append(',');
            builder.Append(Escape(item.TargetId));
            builder.Append(',');
            builder.Append(Escape(item.Result));
            builder.Append(',');
            builder.Append(Escape(item.ReasonCode));
            builder.Append(',');
            builder.Append(Escape(item.CorrelationId.ToString()));
            builder.Append(',');
            builder.Append(Escape(item.OccurredAt.ToString("O")));
            builder.AppendLine();
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
