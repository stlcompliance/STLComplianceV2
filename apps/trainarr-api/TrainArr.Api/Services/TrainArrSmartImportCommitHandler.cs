using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.SmartImport;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class TrainArrSmartImportCommitHandler(TrainArrDbContext db) : ISmartImportDestinationCommitHandler
{
    public string ProductKey => "trainarr";

    public async Task<SmartImportDestinationCommitResponse> CommitAsync(
        string entityType,
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SmartImportDestinationCommitResponses.IsCreateOperation(request.Operation))
        {
            return SmartImportDestinationCommitResponses.ReviewRequired(
                "trainarr.smart_import.operation_not_supported",
                "TrainArr Smart Import commits currently support reviewed create operations only.");
        }

        if (entityType.Contains("assignment", StringComparison.OrdinalIgnoreCase))
        {
            return await CommitAssignmentAsync(request, cancellationToken);
        }

        if (entityType.Contains("training", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("cert", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("qualification", StringComparison.OrdinalIgnoreCase))
        {
            return await CommitTrainingDefinitionAsync(request, cancellationToken);
        }

        return SmartImportDestinationCommitResponses.ReviewRequired(
            "trainarr.smart_import.entity_type_not_supported",
            $"TrainArr does not have a Smart Import commit handler for entity type '{entityType}'.");
    }

    private async Task<SmartImportDestinationCommitResponse> CommitTrainingDefinitionAsync(
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.TrainingDefinitions.FirstOrDefaultAsync(
            definition => definition.TenantId == request.TenantId && definition.Id == request.CommitStepId,
            cancellationToken);
        if (existing is not null)
        {
            return Committed(existing.Id, existing.Name);
        }

        var payload = request.DeterministicPayload;
        var shortId = SmartImportPayloadReader.ShortId(request.CommitStepId);
        var name = SmartImportPayloadReader.DisplayName(payload, $"Imported training {shortId}");
        var definitionKey = SmartImportPayloadReader.SlugKey(
            SmartImportPayloadReader.GetString(payload, "definitionKey", "trainingKey", "qualificationKey", "certificateKey")
            ?? name,
            $"si_training_{shortId}",
            64);
        var duplicate = await db.TrainingDefinitions.FirstOrDefaultAsync(
            definition => definition.TenantId == request.TenantId && definition.DefinitionKey == definitionKey,
            cancellationToken);
        if (duplicate is not null)
        {
            return Committed(duplicate.Id, duplicate.Name);
        }

        var now = DateTimeOffset.UtcNow;
        var qualificationKey = SmartImportPayloadReader.SlugKey(
            SmartImportPayloadReader.GetString(payload, "qualificationKey", "certificationKey", "certificateKey")
            ?? definitionKey,
            definitionKey,
            128);
        var definitionEntity = new TrainingDefinition
        {
            Id = request.CommitStepId,
            TenantId = request.TenantId,
            DefinitionKey = definitionKey,
            Name = SmartImportPayloadReader.Truncate(name, 128),
            Description = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "description", "notes") ?? "Created by reviewed Smart Import commit.",
                1024),
            QualificationKey = qualificationKey,
            QualificationName = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "qualificationName", "certificateName") ?? name,
                128),
            Status = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "status") ?? "active",
                32),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.TrainingDefinitions.Add(definitionEntity);
        AddAudit(request, "smart_import.training_definition_created", "training_definition", definitionEntity.Id.ToString("D"), now);
        await db.SaveChangesAsync(cancellationToken);
        return Committed(definitionEntity.Id, definitionEntity.Name);
    }

    private async Task<SmartImportDestinationCommitResponse> CommitAssignmentAsync(
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.TrainingAssignments.FirstOrDefaultAsync(
            assignment => assignment.TenantId == request.TenantId && assignment.Id == request.CommitStepId,
            cancellationToken);
        if (existing is not null)
        {
            var definitionName = await db.TrainingDefinitions
                .Where(definition => definition.TenantId == request.TenantId && definition.Id == existing.TrainingDefinitionId)
                .Select(definition => definition.Name)
                .FirstOrDefaultAsync(cancellationToken);
            return Committed(existing.Id, definitionName ?? "Training assignment");
        }

        var payload = request.DeterministicPayload;
        var staffarrPersonId = SmartImportPayloadReader.GetGuid(payload, "staffarrPersonId", "personId", "employeePersonId");
        var trainingDefinitionId = SmartImportPayloadReader.GetGuid(payload, "trainingDefinitionId", "definitionId");
        if (staffarrPersonId is null || trainingDefinitionId is null)
        {
            return SmartImportDestinationCommitResponses.ReviewRequired(
                "trainarr.smart_import.assignment_references_required",
                "TrainArr assignment imports require staffarrPersonId and trainingDefinitionId in the approved payload.");
        }

        var definition = await db.TrainingDefinitions.FirstOrDefaultAsync(
            candidate => candidate.TenantId == request.TenantId && candidate.Id == trainingDefinitionId.Value,
            cancellationToken);
        if (definition is null)
        {
            return SmartImportDestinationCommitResponses.ReviewRequired(
                "trainarr.smart_import.training_definition_not_found",
                "TrainArr assignment imports require a trainingDefinitionId that resolves to an existing TrainArr definition.");
        }

        var now = DateTimeOffset.UtcNow;
        var assignmentEntity = new TrainingAssignment
        {
            Id = request.CommitStepId,
            TenantId = request.TenantId,
            StaffarrPersonId = staffarrPersonId.Value,
            TrainingDefinitionId = definition.Id,
            AssignmentReason = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "assignmentReason", "reason") ?? "smart_import",
                64),
            Status = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "status") ?? "assigned",
                32),
            DueAt = SmartImportPayloadReader.GetDateTimeOffset(payload, "dueAt", "dueDate", "expirationDate"),
            AssignedByUserId = request.ApprovedByPersonId,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.TrainingAssignments.Add(assignmentEntity);
        AddAudit(request, "smart_import.training_assignment_created", "training_assignment", assignmentEntity.Id.ToString("D"), now);
        await db.SaveChangesAsync(cancellationToken);
        return Committed(assignmentEntity.Id, definition.Name);
    }

    private void AddAudit(
        SmartImportDestinationCommitRequest request,
        string action,
        string targetType,
        string targetId,
        DateTimeOffset occurredAt)
    {
        db.AuditEvents.Add(new TrainArrAuditEvent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            ActorUserId = request.ApprovedByPersonId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Result = "success",
            ReasonCode = "smart_import",
            CorrelationId = request.CommitPlanId,
            OccurredAt = occurredAt
        });
    }

    private static SmartImportDestinationCommitResponse Committed(Guid id, string displayName) =>
        SmartImportDestinationCommitResponses.Committed(id.ToString("D"), displayName);
}
