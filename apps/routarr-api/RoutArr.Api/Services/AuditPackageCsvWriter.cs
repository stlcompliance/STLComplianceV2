using System.Text;
using RoutArr.Api.Contracts;

namespace RoutArr.Api.Services;

public static class AuditPackageCsvWriter
{
    public static byte[] WriteAuditEvents(IReadOnlyList<AuditEventExportItem> events)
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            "audit_event_id,actor_user_id,action,target_type,target_id,result,reason_code,correlation_id,occurred_at");

        foreach (var item in events)
        {
            builder.Append(Escape(item.AuditEventId.ToString()));
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

    public static byte[] WriteProofRecords(IReadOnlyList<ProofRecordExportItem> records)
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            "proof_id,trip_id,trip_number,proof_type,captured_by_person_id,vehicle_ref_key,reference_key,notes,captured_at,created_at,updated_at,evidence_hash");

        foreach (var item in records)
        {
            AppendRow(
                builder,
                item.ProofId,
                item.TripId,
                item.TripNumber,
                item.ProofType,
                item.CapturedByPersonId,
                item.VehicleRefKey,
                item.ReferenceKey,
                item.Notes,
                item.CapturedAt,
                item.CreatedAt,
                item.UpdatedAt,
                item.EvidenceHash);
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public static byte[] WriteDvirInspections(IReadOnlyList<DvirInspectionExportItem> inspections)
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            "dvir_id,trip_id,trip_number,phase,vehicle_ref_key,result,odometer_reading,defect_notes,submitted_by_person_id,submitted_at,created_at,updated_at,evidence_hash");

        foreach (var item in inspections)
        {
            AppendRow(
                builder,
                item.DvirId,
                item.TripId,
                item.TripNumber,
                item.Phase,
                item.VehicleRefKey,
                item.Result,
                item.OdometerReading?.ToString(),
                item.DefectNotes,
                item.SubmittedByPersonId,
                item.SubmittedAt,
                item.CreatedAt,
                item.UpdatedAt,
                item.EvidenceHash);
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public static byte[] WriteCaptureAttachments(IReadOnlyList<CaptureAttachmentExportItem> attachments)
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            "attachment_id,trip_id,trip_number,subject_type,subject_id,attachment_kind,file_name,content_type,size_bytes,storage_key,notes,captured_by_person_id,created_at,evidence_hash");

        foreach (var item in attachments)
        {
            AppendRow(
                builder,
                item.AttachmentId,
                item.TripId,
                item.TripNumber,
                item.SubjectType,
                item.SubjectId,
                item.AttachmentKind,
                item.FileName,
                item.ContentType,
                item.SizeBytes,
                item.StorageKey,
                item.Notes,
                item.CapturedByPersonId,
                item.CreatedAt,
                item.EvidenceHash);
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static void AppendRow(StringBuilder builder, params object?[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(',');
            }

            builder.Append(Escape(FormatValue(values[i])));
        }

        builder.AppendLine();
    }

    private static string? FormatValue(object? value) =>
        value switch
        {
            null => null,
            DateTimeOffset dateTime => dateTime.ToString("O"),
            _ => value.ToString(),
        };

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
