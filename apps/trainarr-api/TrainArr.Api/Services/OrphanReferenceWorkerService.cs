using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class OrphanReferenceWorkerService(
    TrainArrDbContext db,
    OrphanReferenceSettingsService settingsService,
    StaffArrPersonLookupClient staffArrPersonLookupClient,
    ComplianceCoreCitationClient complianceCoreCitationClient,
    ComplianceCoreRulePackClient complianceCoreRulePackClient,
    ITrainArrAuditService audit)
{
    public const string ProcessOrphanReferenceScansActionScope = "trainarr.orphan_references.scan";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f7");

    public async Task<PendingOrphanReferenceScansResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        int? stalenessHours,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = OrphanReferenceRules.NormalizeBatchSize(batchSize);
        var normalizedStalenessHours = OrphanReferenceRules.NormalizeStalenessHours(stalenessHours);
        var items = await LoadPendingTenantsAsync(
            tenantId,
            asOf,
            normalizedBatchSize,
            normalizedStalenessHours,
            cancellationToken);

        var responseItems = items
            .Select(x => new PendingOrphanReferenceScanItem(x.TenantId, x.LastScannedAt))
            .ToList();

        return new PendingOrphanReferenceScansResponse(
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            responseItems);
    }

    public async Task<ProcessOrphanReferenceScansResponse> ProcessBatchAsync(
        ProcessOrphanReferenceScansRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = OrphanReferenceRules.NormalizeBatchSize(request.BatchSize);
        var stalenessHours = OrphanReferenceRules.NormalizeStalenessHours(request.StalenessHours);
        var pendingTenants = await LoadPendingTenantsAsync(
            request.TenantId,
            asOf,
            batchSize,
            stalenessHours,
            cancellationToken);

        var scannedTenantIds = new List<Guid>();
        var skipped = new List<OrphanReferenceScanSkip>();
        var referencesChecked = 0;
        var findingsDetected = 0;
        var findingsResolved = 0;

        foreach (var tenant in pendingTenants)
        {
            try
            {
                var scanResult = await ScanTenantAsync(tenant.TenantId, asOf, cancellationToken);
                referencesChecked += scanResult.ReferencesChecked;
                findingsDetected += scanResult.FindingsDetected;
                findingsResolved += scanResult.FindingsResolved;
                scannedTenantIds.Add(tenant.TenantId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new OrphanReferenceScanSkip(tenant.TenantId, ex.Message));
            }
        }

        if (scannedTenantIds.Count > 0 && request.TenantId is Guid scopedTenantId)
        {
            await audit.WriteAsync(
                "orphan_reference.batch",
                scopedTenantId,
                WorkerActorUserId,
                "orphan_reference_scan",
                $"{scannedTenantIds.Count}",
                findingsDetected > 0 ? "AttentionRequired" : "Succeeded",
                cancellationToken: cancellationToken);
        }

        return new ProcessOrphanReferenceScansResponse(
            asOf,
            batchSize,
            scannedTenantIds.Count,
            referencesChecked,
            findingsDetected,
            findingsResolved,
            skipped.Count,
            scannedTenantIds,
            skipped);
    }

    public async Task<OrphanReferenceFindingsResponse> ListActiveFindingsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = OrphanReferenceRules.NormalizeFindingListLimit(limit);
        var rows = await db.OrphanReferenceFindings
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .OrderByDescending(x => x.LastDetectedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new OrphanReferenceFindingsResponse(rows.Select(MapFinding).ToList());
    }

    public async Task<OrphanReferenceRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = OrphanReferenceRules.NormalizeRunListLimit(limit);
        var rows = await db.OrphanReferenceRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ProcessedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new OrphanReferenceRunItem(
                x.Id,
                x.Outcome,
                x.ReferencesCheckedCount,
                x.FindingsDetectedCount,
                x.FindingsResolvedCount,
                x.SkippedCount,
                x.SkipReason,
                x.ProcessedAt))
            .ToList();

        return new OrphanReferenceRunsResponse(items);
    }

    private async Task<TenantScanResult> ScanTenantAsync(
        Guid tenantId,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        var references = await CollectReferencesAsync(tenantId, cancellationToken);
        var orphanKeys = await DetectOrphanKeysAsync(tenantId, references, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var detectedCount = 0;
        var resolvedCount = 0;

        foreach (var orphan in orphanKeys)
        {
            var referenceKey = orphan.ReferenceKey;
            var existing = await db.OrphanReferenceFindings.FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                     && x.ReferenceKind == orphan.ReferenceKind
                     && x.ReferenceKey == referenceKey,
                cancellationToken);

            if (existing is null)
            {
                db.OrphanReferenceFindings.Add(new OrphanReferenceFinding
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReferenceKind = orphan.ReferenceKind,
                    ReferenceKey = referenceKey,
                    SampleSourceEntityType = orphan.SampleSourceEntityType,
                    SampleSourceEntityId = orphan.SampleSourceEntityId,
                    AffectedSourceCount = orphan.AffectedSourceCount,
                    IsActive = true,
                    FirstDetectedAt = now,
                    LastDetectedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now,
                });
                detectedCount++;
                continue;
            }

            existing.SampleSourceEntityType = orphan.SampleSourceEntityType;
            existing.SampleSourceEntityId = orphan.SampleSourceEntityId;
            existing.AffectedSourceCount = orphan.AffectedSourceCount;
            existing.LastDetectedAt = now;
            existing.UpdatedAt = now;

            if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.ResolvedAt = null;
                detectedCount++;
            }
        }

        var activeFindings = await db.OrphanReferenceFindings
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var finding in activeFindings)
        {
            if (orphanKeys.Any(x =>
                    x.ReferenceKind == finding.ReferenceKind
                    && x.ReferenceKey == finding.ReferenceKey))
            {
                continue;
            }

            finding.IsActive = false;
            finding.ResolvedAt = now;
            finding.UpdatedAt = now;
            resolvedCount++;
        }

        var outcome = orphanKeys.Count > 0
            ? "found"
            : resolvedCount > 0
                ? "resolved"
                : "clean";

        db.OrphanReferenceRuns.Add(new OrphanReferenceRun
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Outcome = outcome,
            ReferencesCheckedCount = references.Count,
            FindingsDetectedCount = orphanKeys.Count,
            FindingsResolvedCount = resolvedCount,
            SkippedCount = 0,
            ProcessedAt = now,
            CreatedAt = now,
        });

        await db.SaveChangesAsync(cancellationToken);

        return new TenantScanResult(references.Count, detectedCount, resolvedCount);
    }

    private async Task<IReadOnlyList<DetectedOrphanReference>> DetectOrphanKeysAsync(
        Guid tenantId,
        IReadOnlyList<CollectedReference> references,
        CancellationToken cancellationToken)
    {
        var grouped = references
            .GroupBy(x => (x.ReferenceKind, x.ReferenceKey))
            .Select(group => new CollectedReference(
                group.Key.ReferenceKind,
                group.Key.ReferenceKey,
                group.First().SourceEntityType,
                group.First().SourceEntityId,
                group.Count()))
            .ToList();

        var orphans = new List<DetectedOrphanReference>();

        var personIds = grouped
            .Where(x => x.ReferenceKind == OrphanReferenceRules.ReferenceKindStaffarrPerson)
            .Select(x => Guid.Parse(x.ReferenceKey))
            .Distinct()
            .ToList();

        foreach (var personId in personIds)
        {
            var exists = await staffArrPersonLookupClient.PersonExistsAsync(tenantId, personId, cancellationToken);
            if (!exists)
            {
                var sample = grouped.First(x =>
                    x.ReferenceKind == OrphanReferenceRules.ReferenceKindStaffarrPerson
                    && x.ReferenceKey == OrphanReferenceRules.BuildStaffarrPersonReferenceKey(personId));
                orphans.Add(new DetectedOrphanReference(
                    OrphanReferenceRules.ReferenceKindStaffarrPerson,
                    sample.ReferenceKey,
                    sample.SourceEntityType,
                    sample.SourceEntityId,
                    sample.AffectedSourceCount));
            }
        }

        var citationIds = grouped
            .Where(x => x.ReferenceKind == OrphanReferenceRules.ReferenceKindComplianceCoreCitation)
            .Select(x => Guid.Parse(x.ReferenceKey))
            .Distinct()
            .ToList();

        if (citationIds.Count > 0)
        {
            var foundCitations = await complianceCoreCitationClient.LookupAsync(
                new ComplianceCoreCitationLookupPayload(tenantId, citationIds),
                cancellationToken);
            var foundIds = foundCitations.Select(x => x.CitationId).ToHashSet();

            foreach (var citationId in citationIds.Where(id => !foundIds.Contains(id)))
            {
                var sample = grouped.First(x =>
                    x.ReferenceKind == OrphanReferenceRules.ReferenceKindComplianceCoreCitation
                    && x.ReferenceKey == OrphanReferenceRules.BuildComplianceCoreCitationReferenceKey(citationId));
                orphans.Add(new DetectedOrphanReference(
                    OrphanReferenceRules.ReferenceKindComplianceCoreCitation,
                    sample.ReferenceKey,
                    sample.SourceEntityType,
                    sample.SourceEntityId,
                    sample.AffectedSourceCount));
            }
        }

        var rulePackKeys = grouped
            .Where(x => x.ReferenceKind == OrphanReferenceRules.ReferenceKindComplianceCoreRulePack)
            .Select(x => x.ReferenceKey)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (rulePackKeys.Count > 0)
        {
            var foundRulePacks = await complianceCoreRulePackClient.LookupAsync(
                new ComplianceCoreRulePackLookupPayload(tenantId, rulePackKeys),
                cancellationToken);
            var foundKeys = foundRulePacks
                .Select(x => x.RulePackKey)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var rulePackKey in rulePackKeys.Where(key => !foundKeys.Contains(key)))
            {
                var sample = grouped.First(x =>
                    x.ReferenceKind == OrphanReferenceRules.ReferenceKindComplianceCoreRulePack
                    && string.Equals(x.ReferenceKey, rulePackKey, StringComparison.OrdinalIgnoreCase));
                orphans.Add(new DetectedOrphanReference(
                    OrphanReferenceRules.ReferenceKindComplianceCoreRulePack,
                    sample.ReferenceKey,
                    sample.SourceEntityType,
                    sample.SourceEntityId,
                    sample.AffectedSourceCount));
            }
        }

        return orphans;
    }

    private async Task<IReadOnlyList<CollectedReference>> CollectReferencesAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var references = new List<CollectedReference>();

        var assignments = await db.TrainingAssignments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.Id, x.StaffarrPersonId })
            .ToListAsync(cancellationToken);
        references.AddRange(assignments.Select(x => new CollectedReference(
            OrphanReferenceRules.ReferenceKindStaffarrPerson,
            OrphanReferenceRules.BuildStaffarrPersonReferenceKey(x.StaffarrPersonId),
            "training_assignment",
            x.Id,
            1)));

        var qualifications = await db.QualificationIssues
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.Id, x.StaffarrPersonId })
            .ToListAsync(cancellationToken);
        references.AddRange(qualifications.Select(x => new CollectedReference(
            OrphanReferenceRules.ReferenceKindStaffarrPerson,
            OrphanReferenceRules.BuildStaffarrPersonReferenceKey(x.StaffarrPersonId),
            "qualification_issue",
            x.Id,
            1)));

        var publications = await db.CertificationPublications
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.Id, x.StaffarrPersonId })
            .ToListAsync(cancellationToken);
        references.AddRange(publications.Select(x => new CollectedReference(
            OrphanReferenceRules.ReferenceKindStaffarrPerson,
            OrphanReferenceRules.BuildStaffarrPersonReferenceKey(x.StaffarrPersonId),
            "certification_publication",
            x.Id,
            1)));

        var remediations = await db.StaffarrIncidentRemediations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.Id, x.StaffarrPersonId })
            .ToListAsync(cancellationToken);
        references.AddRange(remediations.Select(x => new CollectedReference(
            OrphanReferenceRules.ReferenceKindStaffarrPerson,
            OrphanReferenceRules.BuildStaffarrPersonReferenceKey(x.StaffarrPersonId),
            "staffarr_incident_remediation",
            x.Id,
            1)));

        var domainEvents = await db.TrainingDomainEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.Id, x.StaffarrPersonId })
            .ToListAsync(cancellationToken);
        references.AddRange(domainEvents.Select(x => new CollectedReference(
            OrphanReferenceRules.ReferenceKindStaffarrPerson,
            OrphanReferenceRules.BuildStaffarrPersonReferenceKey(x.StaffarrPersonId),
            "training_domain_event",
            x.Id,
            1)));

        var historyEntries = await db.PersonTrainingHistoryEntries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.Id, x.StaffarrPersonId })
            .ToListAsync(cancellationToken);
        references.AddRange(historyEntries.Select(x => new CollectedReference(
            OrphanReferenceRules.ReferenceKindStaffarrPerson,
            OrphanReferenceRules.BuildStaffarrPersonReferenceKey(x.StaffarrPersonId),
            "person_training_history_entry",
            x.Id,
            1)));

        var dispatches = await db.TrainingNotificationDispatches
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.Id, x.StaffarrPersonId })
            .ToListAsync(cancellationToken);
        references.AddRange(dispatches.Select(x => new CollectedReference(
            OrphanReferenceRules.ReferenceKindStaffarrPerson,
            OrphanReferenceRules.BuildStaffarrPersonReferenceKey(x.StaffarrPersonId),
            "training_notification_dispatch",
            x.Id,
            1)));

        var deliveries = await db.StaffarrPublicationDeliveries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.Id, x.StaffarrPersonId })
            .ToListAsync(cancellationToken);
        references.AddRange(deliveries.Select(x => new CollectedReference(
            OrphanReferenceRules.ReferenceKindStaffarrPerson,
            OrphanReferenceRules.BuildStaffarrPersonReferenceKey(x.StaffarrPersonId),
            "staffarr_publication_delivery",
            x.Id,
            1)));

        var recalculationStates = await db.QualificationRecalculationStates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.Id, x.StaffarrPersonId })
            .ToListAsync(cancellationToken);
        references.AddRange(recalculationStates.Select(x => new CollectedReference(
            OrphanReferenceRules.ReferenceKindStaffarrPerson,
            OrphanReferenceRules.BuildStaffarrPersonReferenceKey(x.StaffarrPersonId),
            "qualification_recalculation_state",
            x.Id,
            1)));

        var citations = await db.TrainingCitationAttachments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.Id, x.EntityType, x.EntityId, x.ComplianceCoreCitationId })
            .ToListAsync(cancellationToken);
        references.AddRange(citations.Select(x => new CollectedReference(
            OrphanReferenceRules.ReferenceKindComplianceCoreCitation,
            OrphanReferenceRules.BuildComplianceCoreCitationReferenceKey(x.ComplianceCoreCitationId),
            $"training_citation_attachment:{x.EntityType}",
            x.Id,
            1)));

        var rulePackRequirements = await db.TrainingRulePackRequirements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.Id, x.EntityType, x.EntityId, x.RulePackKey })
            .ToListAsync(cancellationToken);
        references.AddRange(rulePackRequirements.Select(x => new CollectedReference(
            OrphanReferenceRules.ReferenceKindComplianceCoreRulePack,
            OrphanReferenceRules.BuildComplianceCoreRulePackReferenceKey(x.RulePackKey),
            $"training_rule_pack_requirement:{x.EntityType}",
            x.Id,
            1)));

        return references;
    }

    private async Task<IReadOnlyList<PendingTenantCandidate>> LoadPendingTenantsAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        int stalenessHours,
        CancellationToken cancellationToken)
    {
        var settingsQuery = db.TenantOrphanReferenceSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled);

        if (tenantId is Guid scopedTenantId)
        {
            settingsQuery = settingsQuery.Where(x => x.TenantId == scopedTenantId);
        }

        var tenantSettings = await settingsQuery.ToListAsync(cancellationToken);
        var results = new List<PendingTenantCandidate>();

        foreach (var settings in tenantSettings)
        {
            if (results.Count >= batchSize)
            {
                break;
            }

            var tenantStalenessHours = OrphanReferenceRules.NormalizeStalenessHours(settings.ScanStalenessHours);
            var effectiveStalenessHours = tenantId is null
                ? tenantStalenessHours
                : OrphanReferenceRules.NormalizeStalenessHours(stalenessHours);

            var lastRun = await db.OrphanReferenceRuns
                .AsNoTracking()
                .Where(x => x.TenantId == settings.TenantId)
                .OrderByDescending(x => x.ProcessedAt)
                .Select(x => (DateTimeOffset?)x.ProcessedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (!OrphanReferenceRules.IsStale(lastRun, asOfUtc, effectiveStalenessHours))
            {
                continue;
            }

            results.Add(new PendingTenantCandidate(settings.TenantId, lastRun));
        }

        return results;
    }

    private static OrphanReferenceFindingItem MapFinding(OrphanReferenceFinding finding) =>
        new(
            finding.Id,
            finding.ReferenceKind,
            finding.ReferenceKey,
            finding.SampleSourceEntityType,
            finding.SampleSourceEntityId,
            finding.AffectedSourceCount,
            finding.IsActive,
            finding.FirstDetectedAt,
            finding.LastDetectedAt,
            finding.ResolvedAt);

    private sealed record CollectedReference(
        string ReferenceKind,
        string ReferenceKey,
        string SourceEntityType,
        Guid SourceEntityId,
        int AffectedSourceCount);

    private sealed record DetectedOrphanReference(
        string ReferenceKind,
        string ReferenceKey,
        string SampleSourceEntityType,
        Guid SampleSourceEntityId,
        int AffectedSourceCount);

    private sealed record PendingTenantCandidate(
        Guid TenantId,
        DateTimeOffset? LastScannedAt);

    private sealed record TenantScanResult(
        int ReferencesChecked,
        int FindingsDetected,
        int FindingsResolved);
}
