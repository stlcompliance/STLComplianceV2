using AssurArr.Api.Data;
using AssurArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.SmartImport;

namespace AssurArr.Api.Services;

public sealed class AssurArrSmartImportCommitHandler(AssurArrDbContext db) : ISmartImportDestinationCommitHandler
{
    private static readonly Guid SystemPersonId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public string ProductKey => "assurarr";

    public async Task<SmartImportDestinationCommitResponse> CommitAsync(
        string entityType,
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SmartImportDestinationCommitResponses.IsCreateOperation(request.Operation))
        {
            return SmartImportDestinationCommitResponses.ReviewRequired(
                "assurarr.smart_import.operation_not_supported",
                "AssurArr Smart Import commits currently support reviewed create operations only.");
        }

        if (entityType.Contains("capa", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("corrective", StringComparison.OrdinalIgnoreCase))
        {
            return await CommitCapaAsync(request, cancellationToken);
        }

        if (entityType.Contains("nonconformance", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("quality", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("case", StringComparison.OrdinalIgnoreCase))
        {
            return await CommitNonconformanceAsync(request, cancellationToken);
        }

        return SmartImportDestinationCommitResponses.ReviewRequired(
            "assurarr.smart_import.entity_type_not_supported",
            $"AssurArr does not have a Smart Import commit handler for entity type '{entityType}'.");
    }

    private async Task<SmartImportDestinationCommitResponse> CommitNonconformanceAsync(
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.Nonconformances.FirstOrDefaultAsync(
            nonconformance => nonconformance.TenantId == request.TenantId && nonconformance.Id == request.CommitStepId,
            cancellationToken);
        if (existing is not null)
        {
            return Committed(existing.Id, existing.Title);
        }

        var payload = request.DeterministicPayload;
        var shortId = SmartImportPayloadReader.ShortId(request.CommitStepId);
        var number = SmartImportPayloadReader.FirstNonEmpty(
            SmartImportPayloadReader.GetString(payload, "number", "caseNumber", "nonconformanceNumber"),
            $"SI-NC-{shortId}");
        var duplicate = await db.Nonconformances.FirstOrDefaultAsync(
            nonconformance => nonconformance.TenantId == request.TenantId && nonconformance.Number == number,
            cancellationToken);
        if (duplicate is not null)
        {
            return Committed(duplicate.Id, duplicate.Title);
        }

        var now = DateTimeOffset.UtcNow;
        var title = SmartImportPayloadReader.DisplayName(payload, $"Imported nonconformance {shortId}");
        var recordRef = SmartImportPayloadReader.RecordArrRecordId(payload, request.RecordArrSourceRecordId);
        var nonconformanceEntity = new AssurArrNonconformance
        {
            Id = request.CommitStepId,
            TenantId = request.TenantId,
            Number = SmartImportPayloadReader.Truncate(number, 64),
            Title = SmartImportPayloadReader.Truncate(title, 256),
            Description = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "description", "notes") ?? "Created by reviewed Smart Import commit.",
                4000),
            Severity = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "severity") ?? "moderate",
                32),
            Status = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "status") ?? "draft",
                32),
            SourceProduct = "nexarr",
            SourceObjectRef = request.ImportBatchId.ToString("D"),
            DiscoveredAt = SmartImportPayloadReader.GetDateTimeOffset(payload, "discoveredAt", "occurredAt") ?? now,
            DiscoveredByPersonId = SmartImportPayloadReader.GetGuid(payload, "discoveredByPersonId", "reportedByPersonId"),
            RecordRefs = string.IsNullOrWhiteSpace(recordRef) ? [] : [recordRef],
            AuditTrail = [$"Created by Smart Import commit plan {request.CommitPlanId:D}."],
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByPersonId = SystemPersonId,
            UpdatedByPersonId = SystemPersonId,
            NonconformanceType = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "nonconformanceType", "type") ?? "other",
                64),
            Category = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "category") ?? "other",
                64),
            CustomerImpact = SmartImportPayloadReader.Truncate(SmartImportPayloadReader.GetString(payload, "customerImpact"), 4000),
            SupplierImpact = SmartImportPayloadReader.Truncate(SmartImportPayloadReader.GetString(payload, "supplierImpact"), 4000),
            SafetyImpact = SmartImportPayloadReader.Truncate(SmartImportPayloadReader.GetString(payload, "safetyImpact"), 4000),
            ComplianceImpact = SmartImportPayloadReader.Truncate(SmartImportPayloadReader.GetString(payload, "complianceImpact"), 4000),
            RecurrenceFlag = SmartImportPayloadReader.GetBool(payload, false, "recurrenceFlag", "isRepeat"),
            DueAt = SmartImportPayloadReader.GetDateTimeOffset(payload, "dueAt", "dueDate")
        };

        db.Nonconformances.Add(nonconformanceEntity);
        db.TimelineEvents.Add(new AssurArrTimelineEvent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            SubjectType = "nonconformance",
            SubjectId = nonconformanceEntity.Id,
            EventType = "smart_import.committed",
            Details = "Created by reviewed Smart Import commit.",
            OccurredAt = now
        });
        await db.SaveChangesAsync(cancellationToken);
        return Committed(nonconformanceEntity.Id, nonconformanceEntity.Title);
    }

    private async Task<SmartImportDestinationCommitResponse> CommitCapaAsync(
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.Capas.FirstOrDefaultAsync(
            capa => capa.TenantId == request.TenantId && capa.Id == request.CommitStepId,
            cancellationToken);
        if (existing is not null)
        {
            return Committed(existing.Id, existing.Title);
        }

        var payload = request.DeterministicPayload;
        var shortId = SmartImportPayloadReader.ShortId(request.CommitStepId);
        var number = SmartImportPayloadReader.FirstNonEmpty(
            SmartImportPayloadReader.GetString(payload, "number", "capaNumber"),
            $"SI-CAPA-{shortId}");
        var duplicate = await db.Capas.FirstOrDefaultAsync(
            capa => capa.TenantId == request.TenantId && capa.Number == number,
            cancellationToken);
        if (duplicate is not null)
        {
            return Committed(duplicate.Id, duplicate.Title);
        }

        var now = DateTimeOffset.UtcNow;
        var title = SmartImportPayloadReader.DisplayName(payload, $"Imported CAPA {shortId}");
        var recordRef = SmartImportPayloadReader.RecordArrRecordId(payload, request.RecordArrSourceRecordId);
        var capaEntity = new AssurArrCapa
        {
            Id = request.CommitStepId,
            TenantId = request.TenantId,
            Number = SmartImportPayloadReader.Truncate(number, 64),
            Title = SmartImportPayloadReader.Truncate(title, 256),
            Description = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "description", "notes") ?? "Created by reviewed Smart Import commit.",
                4000),
            Severity = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "severity") ?? "moderate",
                32),
            Status = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "status") ?? "draft",
                32),
            SourceProduct = "nexarr",
            SourceObjectRef = request.ImportBatchId.ToString("D"),
            SourceRefs = string.IsNullOrWhiteSpace(recordRef) ? [] : [recordRef],
            RecordRefs = string.IsNullOrWhiteSpace(recordRef) ? [] : [recordRef],
            AuditTrail = [$"Created by Smart Import commit plan {request.CommitPlanId:D}."],
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByPersonId = SystemPersonId,
            UpdatedByPersonId = SystemPersonId,
            OpenedAt = now,
            CapaType = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "capaType", "type") ?? "corrective",
                64),
            SourceType = "smart_import",
            RootCauseSummary = SmartImportPayloadReader.Truncate(SmartImportPayloadReader.GetString(payload, "rootCauseSummary"), 4000),
            DueAt = SmartImportPayloadReader.GetDateTimeOffset(payload, "dueAt", "dueDate")
        };

        db.Capas.Add(capaEntity);
        db.TimelineEvents.Add(new AssurArrTimelineEvent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            SubjectType = "capa",
            SubjectId = capaEntity.Id,
            EventType = "smart_import.committed",
            Details = "Created by reviewed Smart Import commit.",
            OccurredAt = now
        });
        await db.SaveChangesAsync(cancellationToken);
        return Committed(capaEntity.Id, capaEntity.Title);
    }

    private static SmartImportDestinationCommitResponse Committed(Guid id, string displayName) =>
        SmartImportDestinationCommitResponses.Committed(id.ToString("D"), displayName);
}
