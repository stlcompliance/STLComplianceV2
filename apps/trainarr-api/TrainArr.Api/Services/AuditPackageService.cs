using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class AuditPackageService(
    TrainArrDbContext db,
    ITrainArrAuditService auditService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public AuditPackageManifestResponse GetManifest() =>
        new(
            PackageVersion: "1",
            Sections:
            [
                new("audit_events", "audit_events.json", "Audit events", "Tenant-scoped TrainArr audit trail."),
                new("training_definitions", "training_definitions.json", "Training definitions", "Training definition catalog for the tenant."),
                new("training_programs", "training_programs.json", "Training programs", "Training program builder records."),
                new("training_program_definitions", "training_program_definitions.json", "Program definitions", "Training program to definition links."),
                new("training_rule_pack_requirements", "training_rule_pack_requirements.json", "Rule pack requirements", "Compliance Core rule pack requirements on definitions and programs."),
                new("training_assignments", "training_assignments.json", "Training assignments", "Person training assignment records."),
                new("training_evidence", "training_evidence.json", "Training evidence", "Evidence metadata for assignments (file content excluded)."),
                new("training_evaluations", "training_evaluations.json", "Training evaluations", "Trainer evaluation records."),
                new("training_signoffs", "training_signoffs.json", "Training signoffs", "Trainee and trainer signoff records."),
                new("qualification_issues", "qualification_issues.json", "Qualifications", "Issued qualification lifecycle records."),
                new("certification_publications", "certification_publications.json", "StaffArr publications", "TrainArr publication records sent to StaffArr."),
                new("person_training_history", "person_training_history.json", "Person training history", "Aggregated person training history entries."),
            ]);

    public async Task<AuditPackageExportResponse> BuildExportAsync(
        Guid tenantId,
        Guid? actorUserId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var package = await MaterializeExportAsync(tenantId, from, to, cancellationToken);

        await auditService.WriteAsync(
            "audit_package.export",
            tenantId,
            actorUserId,
            "audit_package",
            package.PackageId.ToString(),
            "success",
            reasonCode: BuildDateRangeReasonCode(from, to),
            cancellationToken: cancellationToken);

        return package;
    }

    public async Task<byte[]> ExportZipAsync(
        Guid tenantId,
        Guid? actorUserId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);
        var package = await MaterializeExportAsync(tenantId, from, to, cancellationToken);
        var zipBytes = await CreateZipBytesAsync(package, cancellationToken);

        await auditService.WriteAsync(
            "audit_package.export",
            tenantId,
            actorUserId,
            "audit_package",
            package.PackageId.ToString(),
            "success",
            reasonCode: BuildDateRangeReasonCode(from, to),
            cancellationToken: cancellationToken);

        return zipBytes;
    }

    public async Task<AuditPackageExportResponse> MaterializeExportAsync(
        Guid tenantId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);
        var package = await LoadPackageDataAsync(tenantId, from, to, cancellationToken);
        return package with { PackageId = Guid.NewGuid() };
    }

    public async Task<byte[]> CreateZipBytesAsync(
        AuditPackageExportResponse package,
        CancellationToken cancellationToken = default)
    {
        await using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            await WriteJsonEntryAsync(archive, "manifest.json", new
            {
                package.PackageId,
                package.TenantId,
                package.GeneratedAt,
                package.DateRange,
                package.Counts,
                PackageVersion = "1",
            }, cancellationToken);

            await WriteJsonEntryAsync(archive, "audit_events.json", package.AuditEvents, cancellationToken);
            await WriteJsonEntryAsync(archive, "training_definitions.json", package.TrainingDefinitions, cancellationToken);
            await WriteJsonEntryAsync(archive, "training_programs.json", package.TrainingPrograms, cancellationToken);
            await WriteJsonEntryAsync(archive, "training_program_definitions.json", package.TrainingProgramDefinitions, cancellationToken);
            await WriteJsonEntryAsync(archive, "training_rule_pack_requirements.json", package.TrainingRulePackRequirements, cancellationToken);
            await WriteJsonEntryAsync(archive, "training_assignments.json", package.TrainingAssignments, cancellationToken);
            await WriteJsonEntryAsync(archive, "training_evidence.json", package.TrainingEvidence, cancellationToken);
            await WriteJsonEntryAsync(archive, "training_evaluations.json", package.TrainingEvaluations, cancellationToken);
            await WriteJsonEntryAsync(archive, "training_signoffs.json", package.TrainingSignoffs, cancellationToken);
            await WriteJsonEntryAsync(archive, "qualification_issues.json", package.QualificationIssues, cancellationToken);
            await WriteJsonEntryAsync(archive, "certification_publications.json", package.CertificationPublications, cancellationToken);
            await WriteJsonEntryAsync(archive, "person_training_history.json", package.PersonTrainingHistory, cancellationToken);
        }

        return memory.ToArray();
    }

    private async Task<AuditPackageExportResponse> LoadPackageDataAsync(
        Guid tenantId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var auditEventsQuery = db.AuditEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
        auditEventsQuery = ApplyOccurredAtFilter(auditEventsQuery, from, to);

        var definitionsQuery = db.TrainingDefinitions.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            definitionsQuery = definitionsQuery.Where(x => x.UpdatedAt >= from);
        }

        if (to is not null)
        {
            definitionsQuery = definitionsQuery.Where(x => x.UpdatedAt <= to);
        }

        var programsQuery = db.TrainingPrograms.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            programsQuery = programsQuery.Where(x => x.UpdatedAt >= from);
        }

        if (to is not null)
        {
            programsQuery = programsQuery.Where(x => x.UpdatedAt <= to);
        }

        var programDefinitionsQuery = db.TrainingProgramDefinitions.AsNoTracking()
            .Join(
                db.TrainingPrograms.AsNoTracking().Where(x => x.TenantId == tenantId),
                link => link.TrainingProgramId,
                program => program.Id,
                (link, program) => link);
        if (from is not null)
        {
            programDefinitionsQuery = programDefinitionsQuery.Where(x =>
                db.TrainingPrograms.Any(p => p.Id == x.TrainingProgramId && p.UpdatedAt >= from));
        }

        if (to is not null)
        {
            programDefinitionsQuery = programDefinitionsQuery.Where(x =>
                db.TrainingPrograms.Any(p => p.Id == x.TrainingProgramId && p.UpdatedAt <= to));
        }

        var requirementsQuery = db.TrainingRulePackRequirements.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            requirementsQuery = requirementsQuery.Where(x => x.UpdatedAt >= from);
        }

        if (to is not null)
        {
            requirementsQuery = requirementsQuery.Where(x => x.UpdatedAt <= to);
        }

        var assignmentsQuery = db.TrainingAssignments.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            assignmentsQuery = assignmentsQuery.Where(x => x.UpdatedAt >= from);
        }

        if (to is not null)
        {
            assignmentsQuery = assignmentsQuery.Where(x => x.UpdatedAt <= to);
        }

        var evidenceQuery = db.TrainingEvidence.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            evidenceQuery = evidenceQuery.Where(x => x.CreatedAt >= from);
        }

        if (to is not null)
        {
            evidenceQuery = evidenceQuery.Where(x => x.CreatedAt <= to);
        }

        var evaluationsQuery = db.TrainingEvaluations.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            evaluationsQuery = evaluationsQuery.Where(x => x.EvaluatedAt >= from);
        }

        if (to is not null)
        {
            evaluationsQuery = evaluationsQuery.Where(x => x.EvaluatedAt <= to);
        }

        var signoffsQuery = db.TrainingSignoffs.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            signoffsQuery = signoffsQuery.Where(x => x.SignedAt >= from);
        }

        if (to is not null)
        {
            signoffsQuery = signoffsQuery.Where(x => x.SignedAt <= to);
        }

        var qualificationsQuery = db.QualificationIssues.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            qualificationsQuery = qualificationsQuery.Where(x => x.UpdatedAt >= from);
        }

        if (to is not null)
        {
            qualificationsQuery = qualificationsQuery.Where(x => x.UpdatedAt <= to);
        }

        var publicationsQuery = db.CertificationPublications.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            publicationsQuery = publicationsQuery.Where(x => x.PublishedAt >= from);
        }

        if (to is not null)
        {
            publicationsQuery = publicationsQuery.Where(x => x.PublishedAt <= to);
        }

        var historyQuery = db.PersonTrainingHistoryEntries.AsNoTracking().Where(x => x.TenantId == tenantId);
        historyQuery = ApplyHistoryOccurredAtFilter(historyQuery, from, to);

        var auditEvents = await auditEventsQuery
            .OrderBy(x => x.OccurredAt)
            .Select(x => new TrainArrAuditEventExportItem(
                x.Id,
                x.ActorUserId,
                x.Action,
                x.TargetType,
                x.TargetId,
                x.Result,
                x.ReasonCode,
                x.CorrelationId,
                x.OccurredAt))
            .ToListAsync(cancellationToken);

        var definitions = await definitionsQuery
            .OrderBy(x => x.DefinitionKey)
            .Select(x => new AuditPackageTrainingDefinitionItem(
                x.Id,
                x.DefinitionKey,
                x.Name,
                x.QualificationKey,
                x.QualificationName,
                x.Status,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var programs = await programsQuery
            .OrderBy(x => x.ProgramKey)
            .Select(x => new AuditPackageTrainingProgramItem(
                x.Id,
                x.ProgramKey,
                x.Name,
                x.Status,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var programDefinitions = await programDefinitionsQuery
            .OrderBy(x => x.TrainingProgramId)
            .ThenBy(x => x.SortOrder)
            .Select(x => new AuditPackageTrainingProgramDefinitionItem(
                x.TrainingProgramId,
                x.TrainingDefinitionId,
                x.SortOrder))
            .ToListAsync(cancellationToken);

        var requirements = await requirementsQuery
            .OrderBy(x => x.EntityType)
            .ThenBy(x => x.RulePackKey)
            .Select(x => new AuditPackageTrainingRulePackRequirementItem(
                x.Id,
                x.EntityType,
                x.EntityId,
                x.RulePackKey,
                x.KnownVersionNumber,
                x.KnownStatus,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var assignments = await assignmentsQuery
            .OrderBy(x => x.CreatedAt)
            .Select(x => new AuditPackageTrainingAssignmentItem(
                x.Id,
                x.StaffarrPersonId,
                x.TrainingDefinitionId,
                x.StaffarrIncidentRemediationId,
                x.SourceQualificationIssueId,
                x.AssignmentReason,
                x.Status,
                x.DueAt,
                x.CompletedAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var evidence = await evidenceQuery
            .OrderBy(x => x.CreatedAt)
            .Select(x => new AuditPackageTrainingEvidenceItem(
                x.Id,
                x.TrainingAssignmentId,
                x.EvidenceTypeKey,
                x.FileName,
                x.ContentType,
                x.SizeBytes,
                x.StorageKey,
                x.UploadedByUserId,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        var evaluations = await evaluationsQuery
            .OrderBy(x => x.EvaluatedAt)
            .Select(x => new AuditPackageTrainingEvaluationItem(
                x.Id,
                x.TrainingAssignmentId,
                x.Result,
                x.Score,
                x.EvaluatorUserId,
                x.EvaluatedAt,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        var signoffs = await signoffsQuery
            .OrderBy(x => x.SignedAt)
            .Select(x => new AuditPackageTrainingSignoffItem(
                x.Id,
                x.TrainingAssignmentId,
                x.SignoffRole,
                x.SignedByUserId,
                x.SignedAt,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        var qualifications = await qualificationsQuery
            .OrderBy(x => x.IssuedAt)
            .Select(x => new AuditPackageQualificationIssueItem(
                x.Id,
                x.TrainingAssignmentId,
                x.StaffarrPersonId,
                x.QualificationKey,
                x.QualificationName,
                x.Status,
                x.IssuedAt,
                x.ExpiresAt,
                x.StatusChangedAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var publications = await publicationsQuery
            .OrderBy(x => x.PublishedAt)
            .Select(x => new AuditPackageCertificationPublicationItem(
                x.Id,
                x.StaffarrPersonId,
                x.QualificationKey,
                x.QualificationName,
                x.PublicationType,
                x.BlockerType,
                x.Status,
                x.PublishedAt,
                x.ExpiresAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var history = await historyQuery
            .OrderBy(x => x.OccurredAt)
            .Select(x => new AuditPackagePersonTrainingHistoryItem(
                x.Id,
                x.StaffarrPersonId,
                x.EventKind,
                x.Summary,
                x.RelatedEntityType,
                x.RelatedEntityId,
                x.OccurredAt,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new AuditPackageExportResponse(
            PackageId: Guid.Empty,
            TenantId: tenantId,
            GeneratedAt: DateTimeOffset.UtcNow,
            DateRange: from is null && to is null
                ? null
                : new AuditPackageDateRangeResponse(from, to),
            Counts: new AuditPackageCountsResponse(
                auditEvents.Count,
                definitions.Count,
                programs.Count,
                programDefinitions.Count,
                requirements.Count,
                assignments.Count,
                evidence.Count,
                evaluations.Count,
                signoffs.Count,
                qualifications.Count,
                publications.Count,
                history.Count),
            AuditEvents: auditEvents,
            TrainingDefinitions: definitions,
            TrainingPrograms: programs,
            TrainingProgramDefinitions: programDefinitions,
            TrainingRulePackRequirements: requirements,
            TrainingAssignments: assignments,
            TrainingEvidence: evidence,
            TrainingEvaluations: evaluations,
            TrainingSignoffs: signoffs,
            QualificationIssues: qualifications,
            CertificationPublications: publications,
            PersonTrainingHistory: history);
    }

    private static IQueryable<TrainArrAuditEvent> ApplyOccurredAtFilter(
        IQueryable<TrainArrAuditEvent> query,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        if (from is not null)
        {
            query = query.Where(x => x.OccurredAt >= from);
        }

        if (to is not null)
        {
            query = query.Where(x => x.OccurredAt <= to);
        }

        return query;
    }

    private static IQueryable<PersonTrainingHistoryEntry> ApplyHistoryOccurredAtFilter(
        IQueryable<PersonTrainingHistoryEntry> query,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        if (from is not null)
        {
            query = query.Where(x => x.OccurredAt >= from);
        }

        if (to is not null)
        {
            query = query.Where(x => x.OccurredAt <= to);
        }

        return query;
    }

    private static void ValidateDateRange(DateTimeOffset? from, DateTimeOffset? to)
    {
        if (from is not null && to is not null && from > to)
        {
            throw new StlApiException(
                "audit_package.invalid_date_range",
                "The 'from' date must be before or equal to the 'to' date.",
                400);
        }
    }

    private static string? BuildDateRangeReasonCode(DateTimeOffset? from, DateTimeOffset? to) =>
        from is null && to is null ? "all_time" : "date_filtered";

    private static async Task WriteJsonEntryAsync<T>(
        ZipArchive archive,
        string entryName,
        T payload,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using var entryStream = entry.Open();
        await JsonSerializer.SerializeAsync(entryStream, payload, JsonOptions, cancellationToken);
    }
}
