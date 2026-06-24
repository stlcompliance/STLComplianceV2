using AssurArr.Api.Contracts;
using AssurArr.Api.Data;
using AssurArr.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssurArr.Api.Services;

public sealed class AssurArrQualityService(AssurArrDbContext db)
{
    private static readonly IReadOnlyDictionary<string, string[]> NonconformanceTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["draft"] = ["open", "canceled"],
        ["open"] = ["containment", "investigation", "canceled"],
        ["containment"] = ["investigation", "disposition_pending", "canceled"],
        ["investigation"] = ["disposition_pending", "corrective_action", "canceled"],
        ["disposition_pending"] = ["corrective_action", "verification", "canceled"],
        ["corrective_action"] = ["verification", "release_pending", "canceled"],
        ["verification"] = ["release_pending", "closed", "canceled"],
        ["release_pending"] = ["closed", "canceled"],
        ["closed"] = [],
        ["canceled"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string[]> HoldTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["draft"] = ["active", "canceled"],
        ["active"] = ["release_pending", "released", "rejected", "canceled", "expired"],
        ["release_pending"] = ["released", "rejected", "canceled"],
        ["released"] = [],
        ["rejected"] = [],
        ["canceled"] = [],
        ["expired"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string[]> CapaTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["draft"] = ["open", "canceled"],
        ["open"] = ["root_cause", "canceled"],
        ["root_cause"] = ["action_plan", "canceled"],
        ["action_plan"] = ["implementation", "canceled"],
        ["implementation"] = ["verification", "canceled"],
        ["verification"] = ["effective", "ineffective", "canceled"],
        ["effective"] = ["closed"],
        ["ineffective"] = ["open", "root_cause", "action_plan"],
        ["closed"] = [],
        ["canceled"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string[]> CapaActionTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["open"] = ["assigned", "in_progress", "blocked", "completed", "rejected", "canceled"],
        ["assigned"] = ["in_progress", "blocked", "completed", "rejected", "canceled"],
        ["in_progress"] = ["blocked", "completed", "verified", "rejected", "canceled"],
        ["blocked"] = ["assigned", "in_progress", "completed", "rejected", "canceled"],
        ["completed"] = ["verified", "rejected", "canceled"],
        ["verified"] = [],
        ["rejected"] = [],
        ["canceled"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string[]> CapaActionBlockerTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["active"] = ["resolved", "overridden"],
        ["resolved"] = [],
        ["overridden"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string[]> VerificationPlanTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["draft"] = ["approved", "canceled"],
        ["approved"] = ["active", "canceled"],
        ["active"] = ["completed", "canceled"],
        ["completed"] = [],
        ["canceled"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string[]> EffectivenessVerificationTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["scheduled"] = ["in_progress", "effective", "ineffective", "inconclusive", "canceled"],
        ["in_progress"] = ["effective", "ineffective", "inconclusive", "canceled"],
        ["effective"] = [],
        ["ineffective"] = ["scheduled", "in_progress", "canceled"],
        ["inconclusive"] = ["scheduled", "in_progress", "canceled"],
        ["canceled"] = [],
    };

    private static readonly HashSet<string> RiskProfileTargetTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "supplier",
        "customer",
        "process",
        "site",
        "asset",
        "inventory_item",
        "order",
        "route",
    };

    private static readonly HashSet<string> RiskLevels = new(StringComparer.OrdinalIgnoreCase)
    {
        "low",
        "moderate",
        "high",
        "critical",
        "unknown",
    };

    private static readonly IReadOnlyDictionary<string, string[]> AuditTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["draft"] = ["planned", "canceled"],
        ["planned"] = ["in_progress", "canceled"],
        ["in_progress"] = ["findings_review", "canceled"],
        ["findings_review"] = ["corrective_action", "verification", "closed", "canceled"],
        ["corrective_action"] = ["verification", "closed", "canceled"],
        ["verification"] = ["closed", "canceled"],
        ["closed"] = [],
        ["canceled"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string[]> ChecklistTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["draft"] = ["active", "completed", "canceled"],
        ["active"] = ["completed", "canceled"],
        ["completed"] = [],
        ["canceled"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string[]> FindingTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["open"] = ["accepted", "disputed", "nonconformance_created", "corrective_action", "verified", "closed", "canceled"],
        ["accepted"] = ["nonconformance_created", "corrective_action", "verified", "closed", "canceled"],
        ["disputed"] = ["accepted", "closed", "canceled"],
        ["nonconformance_created"] = ["corrective_action", "verified", "closed"],
        ["corrective_action"] = ["verified", "closed"],
        ["verified"] = ["closed"],
        ["closed"] = [],
        ["canceled"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string[]> ReviewTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["pending"] = ["in_review", "canceled"],
        ["in_review"] = ["approved", "rejected", "changes_requested", "canceled"],
        ["approved"] = ["changes_requested"],
        ["rejected"] = [],
        ["changes_requested"] = ["in_review", "approved", "rejected", "canceled"],
        ["canceled"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string[]> ReleaseTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["requested"] = ["pending_review", "canceled"],
        ["pending_review"] = ["approved", "rejected", "canceled"],
        ["approved"] = ["executed", "rejected", "canceled"],
        ["rejected"] = [],
        ["executed"] = [],
        ["canceled"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string[]> ContainmentTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["open"] = ["assigned", "in_progress", "canceled"],
        ["assigned"] = ["in_progress", "completed", "canceled"],
        ["in_progress"] = ["completed", "verified", "canceled"],
        ["completed"] = ["verified", "canceled"],
        ["verified"] = [],
        ["canceled"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string[]> DispositionTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["proposed"] = ["pending_approval", "approved", "rejected", "canceled"],
        ["pending_approval"] = ["approved", "rejected", "canceled"],
        ["approved"] = ["executed", "rejected", "canceled"],
        ["executed"] = [],
        ["rejected"] = [],
        ["canceled"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string[]> SupplierQualityTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["open"] = ["supplier_notified", "canceled"],
        ["supplier_notified"] = ["response_pending", "under_review", "canceled"],
        ["response_pending"] = ["under_review", "corrective_action", "resolved", "canceled"],
        ["under_review"] = ["corrective_action", "resolved", "canceled"],
        ["corrective_action"] = ["resolved", "closed", "canceled"],
        ["resolved"] = ["closed"],
        ["closed"] = [],
        ["canceled"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string[]> ScarTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["draft"] = ["sent", "canceled"],
        ["sent"] = ["acknowledged", "supplier_response_pending", "response_received", "under_review", "accepted", "rejected", "closed", "canceled"],
        ["acknowledged"] = ["supplier_response_pending", "response_received", "under_review", "accepted", "rejected", "closed", "canceled"],
        ["supplier_response_pending"] = ["response_received", "under_review", "accepted", "rejected", "closed", "canceled"],
        ["response_received"] = ["under_review", "accepted", "rejected", "closed", "canceled"],
        ["under_review"] = ["accepted", "rejected", "closed", "canceled"],
        ["accepted"] = ["closed"],
        ["rejected"] = ["closed"],
        ["closed"] = [],
        ["canceled"] = [],
    };

    private static readonly IReadOnlyDictionary<string, string[]> CustomerComplaintTransitions = new Dictionary<string, string[]>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["received"] = ["triage", "canceled"],
        ["triage"] = ["investigating", "containment", "response_pending", "canceled"],
        ["investigating"] = ["containment", "response_pending", "corrective_action", "resolved", "canceled"],
        ["containment"] = ["response_pending", "corrective_action", "resolved", "canceled"],
        ["response_pending"] = ["corrective_action", "resolved", "closed", "canceled"],
        ["corrective_action"] = ["resolved", "closed", "canceled"],
        ["resolved"] = ["closed"],
        ["closed"] = [],
        ["canceled"] = [],
    };

    public async Task<AssurArrDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var openNcCount = await db.Nonconformances.CountAsync(x => x.Status != "closed" && x.Status != "canceled", cancellationToken);
        var criticalNcCount = await db.Nonconformances.CountAsync(x => x.Severity == "critical" && x.Status != "closed" && x.Status != "canceled", cancellationToken);
        var activeHoldCount = await db.QualityHolds.CountAsync(x => x.Status == "active" || x.Status == "release_pending", cancellationToken);
        var openCapaCount = await db.Capas.CountAsync(x => x.Status != "closed" && x.Status != "canceled", cancellationToken);
        var overdueCapaCount = await db.Capas.CountAsync(x => x.DueAt != null && x.DueAt < now && x.Status != "closed" && x.Status != "canceled" && x.Status != "effective", cancellationToken);
        var openAuditCount = await db.QualityAudits.CountAsync(x => x.Status != "closed" && x.Status != "canceled", cancellationToken);
        var openFindingCount = await db.AuditFindings.CountAsync(x => x.Status != "closed" && x.Status != "canceled", cancellationToken);
        var auditFindingCount = await db.AuditFindings.CountAsync(cancellationToken);
        var repeatIssueCount = await db.Nonconformances.CountAsync(x => x.RecurrenceFlag || x.RepeatOfNonconformanceRef != null, cancellationToken);
        var pendingReviewCount = await db.QualityReviews.CountAsync(x => x.Status == "pending" || x.Status == "in_review", cancellationToken);
        var pendingReleaseCount = await db.QualityReleases.CountAsync(x => x.Status == "requested" || x.Status == "pending_review", cancellationToken);
        var openContainmentCount = await db.ContainmentActions.CountAsync(x => x.Status != "verified" && x.Status != "canceled", cancellationToken);
        var openDispositionCount = await db.Dispositions.CountAsync(x => x.Status != "executed" && x.Status != "rejected" && x.Status != "canceled", cancellationToken);
        var openSupplierIssueCount = await db.SupplierQualityIssues.CountAsync(x => x.Status != "closed" && x.Status != "canceled", cancellationToken);
        var openScarCount = await db.SupplierCorrectiveActionRequests.CountAsync(x => x.Status != "closed" && x.Status != "canceled", cancellationToken);
        var openComplaintCount = await db.CustomerComplaintQualityCases.CountAsync(x => x.Status != "closed" && x.Status != "canceled", cancellationToken);
        var effectiveCapaCount = await db.Capas.CountAsync(x => x.Status == "effective", cancellationToken);
        var recentlyReleasedHoldCount = await db.QualityHolds.CountAsync(x => x.Status == "released" && x.ReleasedAt != null && x.ReleasedAt >= now.AddDays(-30), cancellationToken);
        var openScorecards = await db.QualityScorecards.CountAsync(x => x.Status == "active", cancellationToken);
        var highRiskProfileCount = await db.QualityRiskProfiles.CountAsync(x => x.RiskLevel == "high" || x.RiskLevel == "critical", cancellationToken);
        var siteRiskCount = await db.QualityRiskProfiles.CountAsync(x => (x.RiskLevel == "high" || x.RiskLevel == "critical") && x.TargetType == "site", cancellationToken);
        var supplierRiskCount = await db.QualityRiskProfiles.CountAsync(x => (x.RiskLevel == "high" || x.RiskLevel == "critical") && x.TargetType == "supplier", cancellationToken);
        var processRiskCount = await db.QualityRiskProfiles.CountAsync(x => (x.RiskLevel == "high" || x.RiskLevel == "critical") && x.TargetType == "process", cancellationToken);
        var openStatusSnapshots = await db.QualityStatusSnapshots.CountAsync(x => x.Status != "unknown", cancellationToken);

        var cards = new[]
        {
            new AssurArrDashboardCardResponse("nonconformances", "Open nonconformances", "Cases requiring investigation, containment, or closure.", openNcCount, "danger"),
            new AssurArrDashboardCardResponse("critical-nonconformances", "Critical nonconformances", "High-severity cases that need immediate attention.", criticalNcCount, "danger"),
            new AssurArrDashboardCardResponse("holds", "Active holds", "Business decisions that are currently blocking target objects.", activeHoldCount, "warning"),
            new AssurArrDashboardCardResponse("capa", "Open CAPA", "Corrective and preventive actions in progress.", openCapaCount, "accent"),
            new AssurArrDashboardCardResponse("overdue-capas", "Overdue CAPAs", "CAPA records that have passed their due date without completion.", overdueCapaCount, "warning"),
            new AssurArrDashboardCardResponse("audits", "Open audits", "Quality reviews and audits awaiting closeout.", openAuditCount, "info"),
            new AssurArrDashboardCardResponse("findings", "Open findings", "Issues or opportunities captured during audits.", openFindingCount, "soft"),
            new AssurArrDashboardCardResponse("audit-findings", "Audit findings", "Findings captured across active and historical audits.", auditFindingCount, "info"),
            new AssurArrDashboardCardResponse("repeat-issues", "Repeat issues", "Nonconformances marked as recurring or repeated.", repeatIssueCount, "warning"),
            new AssurArrDashboardCardResponse("reviews", "Quality reviews", "Evidence reviews and decision gates in progress.", pendingReviewCount, "info"),
            new AssurArrDashboardCardResponse("releases", "Quality releases", "Release requests waiting on approval or execution.", pendingReleaseCount, "warning"),
            new AssurArrDashboardCardResponse("containment", "Containment actions", "Immediate quality actions in flight or pending verification.", openContainmentCount, "accent"),
            new AssurArrDashboardCardResponse("dispositions", "Dispositions", "Pending or active disposition decisions.", openDispositionCount, "warning"),
            new AssurArrDashboardCardResponse("supplier-quality", "Supplier quality issues", "Supplier-responsible quality problems under review.", openSupplierIssueCount, "danger"),
            new AssurArrDashboardCardResponse("scars", "SCARs", "Supplier corrective action requests sent or in flight.", openScarCount, "danger"),
            new AssurArrDashboardCardResponse("customer-complaints", "Customer complaint cases", "Customer-facing complaint quality workflows in progress.", openComplaintCount, "warning"),
            new AssurArrDashboardCardResponse("status", "Status snapshots", "Current quality state published to other products.", openStatusSnapshots, "neutral"),
            new AssurArrDashboardCardResponse("scorecards", "Scorecards", "Active quality scorecards and trend summaries.", openScorecards, "accent"),
            new AssurArrDashboardCardResponse("capa-effectiveness", "CAPA effectiveness", "CAPAs that have been verified effective.", effectiveCapaCount, "success"),
            new AssurArrDashboardCardResponse("recently-released-holds", "Recently released holds", "Holds released in the last 30 days.", recentlyReleasedHoldCount, "info"),
            new AssurArrDashboardCardResponse("risk-profiles", "Quality risk profiles", "Sites, suppliers, customers, and processes with elevated quality risk.", highRiskProfileCount, "warning"),
            new AssurArrDashboardCardResponse("risk-by-site", "Quality risk by site", "High-risk sites with elevated quality impact.", siteRiskCount, "warning"),
            new AssurArrDashboardCardResponse("risk-by-supplier", "Quality risk by supplier", "High-risk suppliers with current quality concerns.", supplierRiskCount, "danger"),
            new AssurArrDashboardCardResponse("risk-by-process", "Quality risk by process", "High-risk business processes requiring attention.", processRiskCount, "warning"),
        };

        var events = await db.TimelineEvents
            .OrderByDescending(x => x.OccurredAt)
            .Take(8)
            .Select(x => new AssurArrTimelineEventResponse(x.Id, x.SubjectType, x.SubjectId, x.EventType, x.Details, x.OccurredAt))
            .ToListAsync(cancellationToken);

        return new AssurArrDashboardResponse(now, cards, events);
    }

    public async Task<List<AssurArrNonconformanceResponse>> ListNonconformancesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.Nonconformances
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var responses = new List<AssurArrNonconformanceResponse>(entities.Count);
        foreach (var entity in entities)
        {
            responses.Add(await ToNonconformanceResponseAsync(entity, cancellationToken));
        }

        return responses;
    }

    public async Task<AssurArrNonconformanceResponse?> GetNonconformanceAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await db.Nonconformances.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)) is { } entity
            ? await ToNonconformanceResponseAsync(entity, cancellationToken)
            : null;

    public async Task<AssurArrNonconformanceResponse> CreateNonconformanceAsync(CreateAssurArrNonconformanceRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrNonconformance
        {
            Id = Guid.NewGuid(),
            TenantId = db.CurrentTenantId,
            Number = await GenerateNumberAsync("NCR", db.Nonconformances, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "moderate"),
            Status = (request.BlockerRefs?.Length ?? 0) > 0 ? "containment" : "open",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            DiscoveredAt = request.DiscoveredAt ?? now,
            DiscoveredByPersonId = db.CurrentPersonId,
            StaffArrSiteId = request.StaffArrSiteId,
            StaffArrLocationId = request.StaffArrLocationId,
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            OwnerPersonId = request.OwnerPersonId,
            RecordRefs = [],
            ContainmentRefs = request.ContainmentRefs ?? [],
            HoldRefs = request.HoldRefs ?? [],
            AffectedItemRefs = request.AffectedItemRefs ?? [],
            AffectedAssetRefs = request.AffectedAssetRefs ?? [],
            AffectedOrderRefs = request.AffectedOrderRefs ?? [],
            AffectedSupplierRefs = request.AffectedSupplierRefs ?? [],
            AffectedCustomerRefs = request.AffectedCustomerRefs ?? [],
            AffectedShipmentRefs = request.AffectedShipmentRefs ?? [],
            DispositionRefs = request.DispositionRefs ?? [],
            CapaRefs = request.CapaRefs ?? [],
            ComplianceRefs = request.ComplianceRefs ?? [],
            FinancialImpactSnapshot = NormalizeNullable(request.FinancialImpactSnapshot),
            AuditTrail = [CreateAuditTrailEntry("created", now, request.Title)],
            BlockerRefs = request.BlockerRefs ?? [],
            CreatedAt = now,
            UpdatedAt = now,
            NonconformanceType = Normalize(request.NonconformanceType, "other"),
            Category = Normalize(request.Category, "other"),
            CustomerImpact = NormalizeNullable(request.CustomerImpact),
            SupplierImpact = NormalizeNullable(request.SupplierImpact),
            SafetyImpact = NormalizeNullable(request.SafetyImpact),
            ComplianceImpact = NormalizeNullable(request.ComplianceImpact),
            RecurrenceFlag = request.RecurrenceFlag,
            RepeatOfNonconformanceRef = NormalizeNullable(request.RepeatOfNonconformanceRef),
            RootCauseRef = NormalizeNullable(request.RootCauseRef),
            DueAt = request.DueAt,
        };

        db.Nonconformances.Add(entity);
        await AddTimelineAsync("nonconformance", entity.Id, "assurarr.nonconformance.created", entity.Title, cancellationToken);
        await PublishQualityStatusSnapshotAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToNonconformanceResponseAsync(entity, cancellationToken);
    }

    public async Task<AssurArrNonconformanceResponse> UpdateNonconformanceStatusAsync(Guid id, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.Nonconformances.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Nonconformance '{id}' was not found.");
        EnsureTransition(entity.Status, request.Status, NonconformanceTransitions, "nonconformance");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(entity.Status, "action_plan", StringComparison.OrdinalIgnoreCase))
        {
            await AddTimelineAsync("capa", entity.Id, "assurarr.capa.root_cause_completed", entity.Title, cancellationToken);
            await AddTimelineAsync("capa", entity.Id, "assurarr.capa.action_plan_created", entity.Title, cancellationToken);
        }
        else if (string.Equals(entity.Status, "verification", StringComparison.OrdinalIgnoreCase))
        {
            await AddTimelineAsync("capa", entity.Id, "assurarr.capa.verification_started", entity.Title, cancellationToken);
        }
        else if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            await AddTimelineAsync("capa", entity.Id, "assurarr.capa.closed", entity.Title, cancellationToken);
        }
        if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
            entity.AuditTrail = AppendAuditTrail(entity.AuditTrail, "closed", entity.Status, entity.UpdatedAt);
        }
        else if (!string.IsNullOrWhiteSpace(request.ClosureSummary))
        {
            entity.ClosureSummary = request.ClosureSummary;
        }
        entity.AuditTrail = AppendAuditTrail(entity.AuditTrail, "status_changed", entity.Status, entity.UpdatedAt);
        await AddTimelineAsync("nonconformance", entity.Id, "assurarr.nonconformance.status_changed", entity.Status, cancellationToken);
        if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            await AddTimelineAsync("nonconformance", entity.Id, "assurarr.nonconformance.closed", entity.Status, cancellationToken);
        }
        await PublishQualityStatusSnapshotAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToNonconformanceResponseAsync(entity, cancellationToken);
    }

    private async Task PublishQualityStatusSnapshotAsync(AssurArrNonconformance entity, CancellationToken cancellationToken)
    {
        var targetProduct = NormalizeNullable(entity.SourceProduct) ?? "assurarr";
        var targetObjectRef = NormalizeNullable(entity.SourceObjectRef) ?? $"assurarr:nonconformance:{entity.Number}";
        var qualityStatus = DetermineQualityStatus(entity.Status, entity.Severity);
        string[] openNonconformanceRefs = string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "canceled", StringComparison.OrdinalIgnoreCase)
            ? Array.Empty<string>()
            : [entity.Number];

        await CreateStatusSnapshotAsync(
            new CreateAssurArrQualityStatusSnapshotRequest(
                targetProduct,
                targetObjectRef,
                qualityStatus,
                entity.Severity,
                entity.Title,
                entity.Description,
                entity.SourceProduct,
                entity.SourceObjectRef,
                entity.AffectedObjectRefs.ToArray(),
                entity.OwnerPersonId,
                [],
                openNonconformanceRefs,
                [],
                [],
                entity.DueAt),
            cancellationToken);
    }

    private static string DetermineQualityStatus(string status, string severity) =>
        string.Equals(status, "closed", StringComparison.OrdinalIgnoreCase) ? "acceptable"
        : string.Equals(status, "canceled", StringComparison.OrdinalIgnoreCase) ? "unknown"
        : string.Equals(status, "release_pending", StringComparison.OrdinalIgnoreCase) ? "conditional_release"
        : string.Equals(status, "containment", StringComparison.OrdinalIgnoreCase) ? "on_hold"
        : string.Equals(status, "verification", StringComparison.OrdinalIgnoreCase) ? "under_review"
        : string.Equals(status, "disposition_pending", StringComparison.OrdinalIgnoreCase) ? "under_review"
        : string.Equals(status, "corrective_action", StringComparison.OrdinalIgnoreCase) ? "under_review"
        : string.Equals(status, "investigation", StringComparison.OrdinalIgnoreCase) ? "under_review"
        : (string.Equals(severity, "critical", StringComparison.OrdinalIgnoreCase) || string.Equals(severity, "high", StringComparison.OrdinalIgnoreCase))
            ? "warning"
            : "under_review";

    public async Task<List<AssurArrRootCauseAnalysisResponse>> ListRootCauseAnalysesAsync(Guid nonconformanceId, CancellationToken cancellationToken = default)
    {
        var entities = await db.RootCauseAnalyses
            .AsNoTracking()
            .Where(x => x.NonconformanceId == nonconformanceId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToRootCauseAnalysisResponse).ToList();
    }

    public async Task<AssurArrRootCauseAnalysisResponse> GetRootCauseAnalysisAsync(Guid nonconformanceId, Guid rootCauseId, CancellationToken cancellationToken = default)
    {
        var entity = await db.RootCauseAnalyses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == rootCauseId && x.NonconformanceId == nonconformanceId, cancellationToken)
            ?? throw new InvalidOperationException($"Root cause analysis '{rootCauseId}' was not found.");

        return ToRootCauseAnalysisResponse(entity);
    }

    public async Task<AssurArrRootCauseAnalysisResponse> CreateRootCauseAnalysisAsync(Guid nonconformanceId, CreateAssurArrRootCauseAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        var nonconformance = await db.Nonconformances.FirstOrDefaultAsync(x => x.Id == nonconformanceId, cancellationToken)
            ?? throw new InvalidOperationException($"Nonconformance '{nonconformanceId}' was not found.");

        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrRootCauseAnalysis
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("RCA", db.RootCauseAnalyses, cancellationToken),
            NonconformanceId = nonconformanceId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = Normalize(request.Status, "in_progress"),
            Method = Normalize(request.Method, "manual"),
            PrimaryCauseCategory = Normalize(request.PrimaryCauseCategory, "unknown"),
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            OwnerPersonId = request.OwnerPersonId,
            RecordRefs = request.RecordRefs ?? [],
            CreatedAt = now,
            UpdatedAt = now,
            RootCauseSummary = NormalizeNullable(request.RootCauseSummary),
            ContributingFactors = request.ContributingFactors ?? [],
            AnalyzedByPersonId = db.CurrentPersonId,
            CompletedAt = request.CompletedAt,
            EvidenceRecordRefs = request.EvidenceRecordRefs ?? [],
        };

        if (string.IsNullOrWhiteSpace(nonconformance.RootCauseRef))
        {
            nonconformance.RootCauseRef = entity.Number;
        }

        nonconformance.UpdatedAt = now;
        if (string.Equals(nonconformance.Status, "open", StringComparison.OrdinalIgnoreCase) || string.Equals(nonconformance.Status, "containment", StringComparison.OrdinalIgnoreCase))
        {
            nonconformance.Status = "investigation";
            await AddTimelineAsync("nonconformance", nonconformance.Id, "assurarr.nonconformance.status_changed", nonconformance.Status, cancellationToken);
        }

        db.RootCauseAnalyses.Add(entity);
        await AddTimelineAsync("root_cause", entity.Id, "assurarr.root_cause.started", entity.Title, cancellationToken);
        if (string.Equals(entity.Status, "completed", StringComparison.OrdinalIgnoreCase) || string.Equals(entity.Status, "inconclusive", StringComparison.OrdinalIgnoreCase))
        {
            await AddTimelineAsync("root_cause", entity.Id, "assurarr.root_cause.completed", entity.Status, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        return ToRootCauseAnalysisResponse(entity);
    }

    public async Task<List<AssurArrQualityHoldResponse>> ListQualityHoldsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.QualityHolds
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var responses = new List<AssurArrQualityHoldResponse>(entities.Count);
        foreach (var entity in entities)
        {
            responses.Add(await ToQualityHoldResponseAsync(entity, cancellationToken));
        }

        return responses;
    }

    public async Task<AssurArrQualityHoldResponse?> GetQualityHoldAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await db.QualityHolds.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)) is { } entity
            ? await ToQualityHoldResponseAsync(entity, cancellationToken)
            : null;

    public async Task<AssurArrQualityHoldResponse> CreateQualityHoldAsync(CreateAssurArrQualityHoldRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrQualityHold
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("HOLD", db.QualityHolds, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "moderate"),
            Status = "active",
            SourceNonconformanceRef = NormalizeNullable(request.SourceNonconformanceRef),
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            StaffArrSiteId = request.StaffArrSiteId,
            StaffArrLocationId = request.StaffArrLocationId,
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            OwnerPersonId = request.OwnerPersonId,
            RecordRefs = [],
            AuditTrail = [CreateAuditTrailEntry("placed", now, request.Title)],
            ReleaseRequirements = [],
            ReleaseApprovalRefs = [],
            CreatedAt = now,
            UpdatedAt = now,
            HoldType = Normalize(request.HoldType, "other"),
            HoldScope = Normalize(request.HoldScope, "full"),
            HoldReason = NormalizeNullable(request.HoldReason),
            QuantityHeld = request.QuantityHeld,
            UnitOfMeasure = NormalizeNullable(request.UnitOfMeasure),
            LotNumber = NormalizeNullable(request.LotNumber),
            SerialNumber = NormalizeNullable(request.SerialNumber),
            PlacedAt = now,
            PlacedByPersonId = db.CurrentPersonId,
        };

        db.QualityHolds.Add(entity);
        if (!string.IsNullOrWhiteSpace(entity.SourceNonconformanceRef))
        {
            var sourceNonconformance = await db.Nonconformances.FirstOrDefaultAsync(x => x.Number == entity.SourceNonconformanceRef, cancellationToken);
            if (sourceNonconformance is not null && !sourceNonconformance.HoldRefs.Contains(entity.Number, StringComparer.OrdinalIgnoreCase))
            {
                sourceNonconformance.HoldRefs = [.. sourceNonconformance.HoldRefs, entity.Number];
                sourceNonconformance.UpdatedAt = now;
                sourceNonconformance.AuditTrail = AppendAuditTrail(sourceNonconformance.AuditTrail, "hold_linked", entity.Number, now);
            }
        }
        await AddTimelineAsync("hold", entity.Id, "assurarr.hold.placed", entity.Title, cancellationToken);
        await AddTimelineAsync("hold", entity.Id, "assurarr.hold.created", entity.Title, cancellationToken);
        await PublishQualityStatusSnapshotAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToQualityHoldResponseAsync(entity, cancellationToken);
    }

    public async Task<AssurArrQualityHoldResponse> UpdateQualityHoldStatusAsync(Guid id, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.QualityHolds.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Quality hold '{id}' was not found.");
        EnsureTransition(entity.Status, request.Status, HoldTransitions, "quality hold");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(entity.Status, "released", StringComparison.OrdinalIgnoreCase))
        {
            entity.ReleasedAt = entity.UpdatedAt;
            entity.ReleaseReason = request.ClosureSummary ?? entity.ReleaseReason;
            entity.AuditTrail = AppendAuditTrail(entity.AuditTrail, "released", entity.Status, entity.UpdatedAt);
        }
        else if (string.Equals(entity.Status, "rejected", StringComparison.OrdinalIgnoreCase))
        {
            entity.RejectedAt = entity.UpdatedAt;
            entity.RejectedByPersonId = null;
            entity.RejectionReason = request.ClosureSummary ?? entity.RejectionReason;
            entity.AuditTrail = AppendAuditTrail(entity.AuditTrail, "rejected", entity.Status, entity.UpdatedAt);
        }
        else if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
            entity.AuditTrail = AppendAuditTrail(entity.AuditTrail, "closed", entity.Status, entity.UpdatedAt);
        }
        entity.AuditTrail = AppendAuditTrail(entity.AuditTrail, "status_changed", entity.Status, entity.UpdatedAt);
        await AddTimelineAsync("hold", entity.Id, "assurarr.hold.status_changed", entity.Status, cancellationToken);
        if (string.Equals(entity.Status, "canceled", StringComparison.OrdinalIgnoreCase))
        {
            await AddTimelineAsync("hold", entity.Id, "assurarr.hold.canceled", entity.Title, cancellationToken);
        }
        await PublishQualityStatusSnapshotAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToQualityHoldResponseAsync(entity, cancellationToken);
    }

    public async Task<AssurArrQualityReleaseResponse> RequestHoldReleaseAsync(Guid holdId, CreateAssurArrQualityReleaseRequest request, CancellationToken cancellationToken = default)
    {
        var hold = await db.QualityHolds.FirstOrDefaultAsync(x => x.Id == holdId, cancellationToken)
            ?? throw new InvalidOperationException($"Quality hold '{holdId}' was not found.");
        if (!string.Equals(request.HoldRef.Trim(), hold.Number, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Release request hold reference '{request.HoldRef}' does not match hold '{hold.Number}'.");
        }

        var release = new AssurArrQualityRelease
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("QREL", db.QualityReleases, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "none"),
            Status = "requested",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            OwnerPersonId = request.OwnerPersonId,
            RecordRefs = [],
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            HoldRef = hold.Number,
            ReleaseType = Normalize(request.ReleaseType, "full"),
            RequestedByPersonId = db.CurrentPersonId,
            RequestedAt = request.RequestedAt ?? DateTimeOffset.UtcNow,
            Conditions = NormalizeNullable(request.Conditions),
            ExpirationAt = request.ExpirationAt,
            EvidenceRecordRefs = request.EvidenceRecordRefs ?? [],
            Notes = NormalizeNullable(request.Notes),
        };

        hold.Status = "release_pending";
        hold.ReleaseRequirements = request.EvidenceRecordRefs ?? [];
        hold.ReleaseApprovalRefs = [];
        hold.UpdatedAt = release.CreatedAt;
        hold.ReleaseReason = release.Conditions;

        db.QualityReleases.Add(release);
        await AddTimelineAsync("hold", hold.Id, "assurarr.hold.release_requested", hold.Title, cancellationToken);
        await AddTimelineAsync("release", release.Id, "assurarr.quality_release.requested", release.Title, cancellationToken);
        await PublishQualityStatusSnapshotAsync(hold, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToReleaseResponseAsync(release, cancellationToken);
    }

    public async Task<AssurArrQualityReleaseResponse> ApproveHoldReleaseAsync(Guid holdId, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var hold = await db.QualityHolds.FirstOrDefaultAsync(x => x.Id == holdId, cancellationToken)
            ?? throw new InvalidOperationException($"Quality hold '{holdId}' was not found.");

        var release = await db.QualityReleases
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.HoldRef == hold.Number, cancellationToken)
            ?? throw new InvalidOperationException($"No quality release request was found for hold '{hold.Number}'.");

        if (string.Equals(release.Status, "requested", StringComparison.OrdinalIgnoreCase))
        {
            EnsureTransition(release.Status, "pending_review", ReleaseTransitions, "quality release");
            release.Status = "pending_review";
        }
        EnsureTransition(release.Status, "approved", ReleaseTransitions, "quality release");
        release.Status = "approved";
        release.ApprovedAt = release.ApprovedAt ?? DateTimeOffset.UtcNow;
        release.ApprovedByPersonId = db.CurrentPersonId;
        release.UpdatedAt = DateTimeOffset.UtcNow;

        EnsureTransition(hold.Status, "released", HoldTransitions, "quality hold");
        hold.Status = "released";
        hold.ReleasedAt = DateTimeOffset.UtcNow;
        hold.ReleasedByPersonId = null;
        hold.ReleaseReason = request.ClosureSummary ?? hold.ReleaseReason;
        hold.UpdatedAt = release.UpdatedAt;

        await AddTimelineAsync("hold", hold.Id, "assurarr.hold.released", hold.Title, cancellationToken);
        await AddTimelineAsync("release", release.Id, "assurarr.quality_release.approved", release.Title, cancellationToken);
        await PublishQualityStatusSnapshotAsync(hold, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToReleaseResponseAsync(release, cancellationToken);
    }

    public async Task<AssurArrQualityReleaseResponse> RejectHoldReleaseAsync(Guid holdId, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var hold = await db.QualityHolds.FirstOrDefaultAsync(x => x.Id == holdId, cancellationToken)
            ?? throw new InvalidOperationException($"Quality hold '{holdId}' was not found.");

        var release = await db.QualityReleases
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.HoldRef == hold.Number, cancellationToken)
            ?? throw new InvalidOperationException($"No quality release request was found for hold '{hold.Number}'.");

        if (string.Equals(release.Status, "requested", StringComparison.OrdinalIgnoreCase))
        {
            EnsureTransition(release.Status, "pending_review", ReleaseTransitions, "quality release");
            release.Status = "pending_review";
        }
        EnsureTransition(release.Status, "rejected", ReleaseTransitions, "quality release");
        release.Status = "rejected";
        release.ClosedAt = DateTimeOffset.UtcNow;
        release.ClosureSummary = request.ClosureSummary ?? release.ClosureSummary;
        release.UpdatedAt = release.ClosedAt.Value;

        EnsureTransition(hold.Status, "rejected", HoldTransitions, "quality hold");
        hold.Status = "rejected";
        hold.RejectedAt = release.UpdatedAt;
        hold.RejectedByPersonId = null;
        hold.RejectionReason = request.ClosureSummary ?? hold.RejectionReason;
        hold.UpdatedAt = release.UpdatedAt;

        await AddTimelineAsync("hold", hold.Id, "assurarr.hold.rejected", hold.Title, cancellationToken);
        await AddTimelineAsync("release", release.Id, "assurarr.quality_release.rejected", release.Title, cancellationToken);
        await PublishQualityStatusSnapshotAsync(hold, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToReleaseResponseAsync(release, cancellationToken);
    }

    private async Task PublishQualityStatusSnapshotAsync(AssurArrQualityHold entity, CancellationToken cancellationToken)
    {
        var targetProduct = NormalizeNullable(entity.SourceProduct) ?? "assurarr";
        var targetObjectRef = NormalizeNullable(entity.SourceObjectRef) ?? $"assurarr:hold:{entity.Number}";
        var qualityStatus = DetermineHoldQualityStatus(entity.Status);
        string[] activeHoldRefs = string.Equals(entity.Status, "released", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "rejected", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "canceled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "expired", StringComparison.OrdinalIgnoreCase)
            ? Array.Empty<string>()
            : [entity.Number];

        await CreateStatusSnapshotAsync(
            new CreateAssurArrQualityStatusSnapshotRequest(
                targetProduct,
                targetObjectRef,
                qualityStatus,
                entity.Severity,
                entity.Title,
                entity.Description,
                entity.SourceProduct,
                entity.SourceObjectRef,
                entity.AffectedObjectRefs.ToArray(),
                entity.OwnerPersonId,
                activeHoldRefs,
                [],
                [],
                [],
                entity.ExpiresAt),
            cancellationToken);
    }

    private static string DetermineHoldQualityStatus(string status) =>
        string.Equals(status, "released", StringComparison.OrdinalIgnoreCase) ? "acceptable"
        : string.Equals(status, "release_pending", StringComparison.OrdinalIgnoreCase) ? "conditional_release"
        : string.Equals(status, "active", StringComparison.OrdinalIgnoreCase) ? "on_hold"
        : string.Equals(status, "draft", StringComparison.OrdinalIgnoreCase) ? "under_review"
        : "unknown";

    public async Task<List<AssurArrCapaResponse>> ListCapasAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.Capas
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var responses = new List<AssurArrCapaResponse>(entities.Count);
        foreach (var entity in entities)
        {
            responses.Add(await ToCapaResponseAsync(entity, cancellationToken));
        }

        return responses;
    }

    public async Task<AssurArrCapaResponse?> GetCapaAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await db.Capas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)) is { } entity
            ? await ToCapaResponseAsync(entity, cancellationToken)
            : null;

    public async Task<AssurArrCapaResponse> CreateCapaAsync(CreateAssurArrCapaRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var sourceObjectRef = NormalizeNullable(request.SourceObjectRef);
        var openedAt = request.OpenedAt ?? now;
        var entity = new AssurArrCapa
        {
            Id = Guid.NewGuid(),
            TenantId = db.CurrentTenantId,
            Number = await GenerateNumberAsync("CAPA", db.Capas, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "moderate"),
            Status = "open",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = sourceObjectRef,
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            OwnerPersonId = request.OwnerPersonId,
            StaffArrSiteId = request.StaffArrSiteId,
            StaffArrLocationId = request.StaffArrLocationId,
            SourceRefs = request.SourceRefs ?? (sourceObjectRef is { Length: > 0 } ? [sourceObjectRef] : []),
            RecordRefs = request.RecordRefs ?? [],
            ActionPlanRefs = request.ActionPlanRefs ?? [],
            VerificationPlanRef = NormalizeNullable(request.VerificationPlanRef),
            RelatedCustomerComplaintRefs = request.RelatedCustomerComplaintRefs ?? [],
            RelatedSupplierIssueRefs = request.RelatedSupplierIssueRefs ?? [],
            ComplianceRefs = request.ComplianceRefs ?? [],
            AuditTrail = [CreateAuditTrailEntry("created", now, request.Title), CreateAuditTrailEntry("opened", openedAt, request.Title)],
            CreatedAt = now,
            UpdatedAt = now,
            OpenedAt = openedAt,
            CapaType = Normalize(request.CapaType, "corrective"),
            SourceType = Normalize(request.SourceType, "manual"),
            SponsorPersonId = request.SponsorPersonId,
            RootCauseSummary = NormalizeNullable(request.RootCauseSummary),
            DueAt = request.DueAt,
            RelatedNonconformanceRefs = request.RelatedNonconformanceRefs ?? [],
            RelatedAuditFindingRefs = request.RelatedAuditFindingRefs ?? [],
        };

        db.Capas.Add(entity);
        await AddTimelineAsync("capa", entity.Id, "assurarr.capa.created", entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToCapaResponseAsync(entity, cancellationToken);
    }

    public async Task<AssurArrCapaResponse> UpdateCapaStatusAsync(Guid id, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.Capas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"CAPA '{id}' was not found.");
        EnsureTransition(entity.Status, request.Status, CapaTransitions, "CAPA");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(entity.Status, "action_plan", StringComparison.OrdinalIgnoreCase))
        {
            await AddTimelineAsync("capa", entity.Id, "assurarr.capa.root_cause_completed", entity.Title, cancellationToken);
            await AddTimelineAsync("capa", entity.Id, "assurarr.capa.action_plan_created", entity.Title, cancellationToken);
        }
        else if (string.Equals(entity.Status, "verification", StringComparison.OrdinalIgnoreCase))
        {
            await AddTimelineAsync("capa", entity.Id, "assurarr.capa.verification_started", entity.Title, cancellationToken);
        }
        else if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            await AddTimelineAsync("capa", entity.Id, "assurarr.capa.closed", entity.Title, cancellationToken);
        }
        if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
            entity.AuditTrail = AppendAuditTrail(entity.AuditTrail, "closed", entity.Status, entity.UpdatedAt);
        }
        entity.AuditTrail = AppendAuditTrail(entity.AuditTrail, "status_changed", entity.Status, entity.UpdatedAt);
        await AddTimelineAsync("capa", entity.Id, "assurarr.capa.status_changed", entity.Status, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToCapaResponseAsync(entity, cancellationToken);
    }

    public async Task<List<AssurArrCapaActionResponse>> ListCapaActionsAsync(Guid capaId, CancellationToken cancellationToken = default)
    {
        var entities = await db.CapaActions
            .AsNoTracking()
            .Where(x => x.CapaId == capaId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToCapaActionResponse).ToList();
    }

    public async Task<AssurArrCapaActionResponse> GetCapaActionAsync(Guid capaId, Guid actionId, CancellationToken cancellationToken = default)
    {
        var entity = await db.CapaActions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == actionId && x.CapaId == capaId, cancellationToken)
            ?? throw new InvalidOperationException($"CAPA action '{actionId}' was not found.");

        return ToCapaActionResponse(entity);
    }

    public async Task<AssurArrCapaActionResponse> CreateCapaActionAsync(Guid capaId, CreateAssurArrCapaActionRequest request, CancellationToken cancellationToken = default)
    {
        var capa = await db.Capas.FirstOrDefaultAsync(x => x.Id == capaId, cancellationToken)
            ?? throw new InvalidOperationException($"CAPA '{capaId}' was not found.");

        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrCapaAction
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("ACT", db.CapaActions, cancellationToken),
            CapaId = capa.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = "open",
            ActionType = Normalize(request.ActionType, "other"),
            AssignedPersonId = db.CurrentPersonId,
            AssignedTeamRef = NormalizeNullable(request.AssignedTeamRef),
            SourceProductActionRef = NormalizeNullable(request.SourceProductActionRef),
            TargetProduct = Normalize(request.TargetProduct, "manual"),
            TargetObjectRef = NormalizeNullable(request.TargetObjectRef),
            DueAt = request.DueAt,
            StartedAt = null,
            CompletedAt = null,
            CompletedByPersonId = null,
            VerificationRequired = request.VerificationRequired,
            VerifiedAt = null,
            VerifiedByPersonId = null,
            EvidenceRecordRefs = request.EvidenceRecordRefs ?? [],
            BlockerRefs = request.BlockerRefs ?? [],
            Notes = NormalizeNullable(request.Notes),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.CapaActions.Add(entity);
        if (string.Equals(entity.Status, "blocked", StringComparison.OrdinalIgnoreCase))
        {
            await AddTimelineAsync("capa_action", entity.Id, "assurarr.capa.action.blocked", entity.Title, cancellationToken);
        }
        await AddTimelineAsync("capa_action", entity.Id, "assurarr.capa.action.created", entity.Title, cancellationToken);
        capa.UpdatedAt = now;
        await AddTimelineAsync("capa", capa.Id, "assurarr.capa.action_assigned", entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToCapaActionResponse(entity);
    }

    public async Task<AssurArrCapaActionResponse> UpdateCapaActionStatusAsync(Guid capaId, Guid actionId, UpdateAssurArrCapaActionStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.CapaActions.FirstOrDefaultAsync(x => x.Id == actionId && x.CapaId == capaId, cancellationToken)
            ?? throw new InvalidOperationException($"CAPA action '{actionId}' was not found.");
        EnsureTransition(entity.Status, request.Status, CapaActionTransitions, "CAPA action");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(entity.Status, "in_progress", StringComparison.OrdinalIgnoreCase))
        {
            entity.StartedAt ??= entity.UpdatedAt;
        }
        if (string.Equals(entity.Status, "completed", StringComparison.OrdinalIgnoreCase))
        {
            entity.CompletedAt = request.CompletedAt ?? entity.UpdatedAt;
            entity.CompletedByPersonId = db.CurrentPersonId;
            await AddTimelineAsync("capa", capaId, "assurarr.capa.action_completed", entity.Title, cancellationToken);
        }
        if (string.Equals(entity.Status, "verified", StringComparison.OrdinalIgnoreCase))
        {
            entity.VerifiedAt = request.VerifiedAt ?? entity.UpdatedAt;
            entity.VerifiedByPersonId = db.CurrentPersonId;
            await AddTimelineAsync("capa", capaId, "assurarr.capa.action_verified", entity.Title, cancellationToken);
        }
        await AddTimelineAsync("capa_action", entity.Id, $"assurarr.capa.action.{entity.Status}", request.ClosureSummary ?? entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToCapaActionResponse(entity);
    }

    private async Task<AssurArrCapaAction> EnsureCapaActionExistsAsync(Guid capaId, Guid actionId, CancellationToken cancellationToken) =>
        await db.CapaActions.FirstOrDefaultAsync(x => x.Id == actionId && x.CapaId == capaId, cancellationToken)
            ?? throw new InvalidOperationException($"CAPA action '{actionId}' was not found.");

    public async Task<List<AssurArrCapaActionBlockerResponse>> ListCapaActionBlockersAsync(Guid capaId, Guid actionId, CancellationToken cancellationToken = default)
    {
        await EnsureCapaActionExistsAsync(capaId, actionId, cancellationToken);
        var entities = await db.CapaActionBlockers
            .AsNoTracking()
            .Where(x => x.CapaActionId == actionId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToCapaActionBlockerResponse).ToList();
    }

    public async Task<AssurArrCapaActionBlockerResponse> GetCapaActionBlockerAsync(Guid capaId, Guid actionId, Guid blockerId, CancellationToken cancellationToken = default)
    {
        await EnsureCapaActionExistsAsync(capaId, actionId, cancellationToken);
        var entity = await db.CapaActionBlockers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == blockerId && x.CapaActionId == actionId, cancellationToken)
            ?? throw new InvalidOperationException($"CAPA action blocker '{blockerId}' was not found.");

        return ToCapaActionBlockerResponse(entity);
    }

    public async Task<AssurArrCapaActionBlockerResponse> CreateCapaActionBlockerAsync(Guid capaId, Guid actionId, CreateAssurArrCapaActionBlockerRequest request, CancellationToken cancellationToken = default)
    {
        var action = await EnsureCapaActionExistsAsync(capaId, actionId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrCapaActionBlocker
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("BLK", db.CapaActionBlockers, cancellationToken),
            CapaActionId = action.Id,
            BlockerType = Normalize(request.BlockerType, "other"),
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = "active",
            CreatedAt = now,
        };

        db.CapaActionBlockers.Add(entity);
        if (!action.BlockerRefs.Contains(entity.Number, StringComparer.OrdinalIgnoreCase))
        {
            action.BlockerRefs = [.. action.BlockerRefs, entity.Number];
        }

        if (!string.Equals(action.Status, "blocked", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(action.Status, "completed", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(action.Status, "verified", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(action.Status, "canceled", StringComparison.OrdinalIgnoreCase))
        {
            action.Status = "blocked";
            action.UpdatedAt = now;
            await AddTimelineAsync("capa_action", action.Id, "assurarr.capa.action.blocked", action.Title, cancellationToken);
        }

        await AddTimelineAsync("capa_action_blocker", entity.Id, "assurarr.capa.action_blocker.created", entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToCapaActionBlockerResponse(entity);
    }

    public async Task<AssurArrCapaActionBlockerResponse> UpdateCapaActionBlockerStatusAsync(Guid capaId, Guid actionId, Guid blockerId, UpdateAssurArrCapaActionBlockerStatusRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureCapaActionExistsAsync(capaId, actionId, cancellationToken);
        var entity = await db.CapaActionBlockers.FirstOrDefaultAsync(x => x.Id == blockerId && x.CapaActionId == actionId, cancellationToken)
            ?? throw new InvalidOperationException($"CAPA action blocker '{blockerId}' was not found.");
        EnsureTransition(entity.Status, request.Status, CapaActionBlockerTransitions, "CAPA action blocker");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.ResolvedAt = string.Equals(entity.Status, "active", StringComparison.OrdinalIgnoreCase)
            ? entity.ResolvedAt
            : request.ResolvedAt ?? DateTimeOffset.UtcNow;
        entity.ResolvedByPersonId = db.CurrentPersonId;
        await AddTimelineAsync("capa_action_blocker", entity.Id, $"assurarr.capa.action_blocker.{entity.Status}", entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToCapaActionBlockerResponse(entity);
    }

    public async Task<List<AssurArrVerificationPlanResponse>> ListVerificationPlansAsync(Guid capaId, CancellationToken cancellationToken = default)
    {
        var entities = await db.VerificationPlans
            .AsNoTracking()
            .Where(x => x.CapaId == capaId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToVerificationPlanResponse).ToList();
    }

    public async Task<AssurArrVerificationPlanResponse> GetVerificationPlanAsync(Guid capaId, Guid verificationPlanId, CancellationToken cancellationToken = default)
    {
        var entity = await db.VerificationPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == verificationPlanId && x.CapaId == capaId, cancellationToken)
            ?? throw new InvalidOperationException($"Verification plan '{verificationPlanId}' was not found.");

        return ToVerificationPlanResponse(entity);
    }

    public async Task<AssurArrVerificationPlanResponse> CreateVerificationPlanAsync(Guid capaId, CreateAssurArrVerificationPlanRequest request, CancellationToken cancellationToken = default)
    {
        var capa = await db.Capas.FirstOrDefaultAsync(x => x.Id == capaId, cancellationToken)
            ?? throw new InvalidOperationException($"CAPA '{capaId}' was not found.");

        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrVerificationPlan
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("VER", db.VerificationPlans, cancellationToken),
            CapaId = capa.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            VerificationType = Normalize(request.VerificationType, "observation"),
            SuccessCriteria = request.SuccessCriteria.Trim(),
            SampleSize = request.SampleSize,
            ObservationPeriodDays = request.ObservationPeriodDays,
            RequiredEvidenceTypes = request.RequiredEvidenceTypes ?? [],
            ResponsiblePersonId = request.ResponsiblePersonId,
            PlannedVerificationAt = request.PlannedVerificationAt,
            Status = "draft",
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.VerificationPlans.Add(entity);
        capa.UpdatedAt = now;
        await AddTimelineAsync("capa", capa.Id, "assurarr.capa.verification_started", entity.Number, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToVerificationPlanResponse(entity);
    }

    public async Task<AssurArrVerificationPlanResponse> UpdateVerificationPlanStatusAsync(Guid capaId, Guid verificationPlanId, UpdateAssurArrVerificationPlanStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.VerificationPlans.FirstOrDefaultAsync(x => x.Id == verificationPlanId && x.CapaId == capaId, cancellationToken)
            ?? throw new InvalidOperationException($"Verification plan '{verificationPlanId}' was not found.");
        EnsureTransition(entity.Status, request.Status, VerificationPlanTransitions, "verification plan");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await AddTimelineAsync("capa_verification", entity.Id, $"assurarr.capa.verification.{entity.Status}", request.ClosureSummary ?? entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToVerificationPlanResponse(entity);
    }

    public async Task<List<AssurArrEffectivenessVerificationResponse>> ListEffectivenessVerificationsAsync(Guid capaId, CancellationToken cancellationToken = default)
    {
        var entities = await db.EffectivenessVerifications
            .AsNoTracking()
            .Where(x => x.CapaId == capaId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToEffectivenessVerificationResponse).ToList();
    }

    public async Task<AssurArrEffectivenessVerificationResponse> GetEffectivenessVerificationAsync(Guid capaId, Guid verificationId, CancellationToken cancellationToken = default)
    {
        var entity = await db.EffectivenessVerifications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == verificationId && x.CapaId == capaId, cancellationToken)
            ?? throw new InvalidOperationException($"Effectiveness verification '{verificationId}' was not found.");

        return ToEffectivenessVerificationResponse(entity);
    }

    public async Task<AssurArrEffectivenessVerificationResponse> CreateEffectivenessVerificationAsync(Guid capaId, CreateAssurArrEffectivenessVerificationRequest request, CancellationToken cancellationToken = default)
    {
        var capa = await db.Capas.FirstOrDefaultAsync(x => x.Id == capaId, cancellationToken)
            ?? throw new InvalidOperationException($"CAPA '{capaId}' was not found.");

        var now = DateTimeOffset.UtcNow;
        if (!string.Equals(capa.Status, "verification", StringComparison.OrdinalIgnoreCase))
        {
            EnsureTransition(capa.Status, "verification", CapaTransitions, "CAPA");
            capa.Status = "verification";
        }
        var entity = new AssurArrEffectivenessVerification
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("VERIF", db.EffectivenessVerifications, cancellationToken),
            CapaId = capa.Id,
            VerificationPlanId = request.VerificationPlanId,
            Status = Normalize(request.Status, "scheduled"),
            PerformedByPersonId = db.CurrentPersonId,
            PerformedAt = request.PerformedAt,
            ResultSummary = NormalizeNullable(request.ResultSummary),
            EvidenceRecordRefs = request.EvidenceRecordRefs ?? [],
            MetricResults = request.MetricResults ?? [],
            RecurrenceFound = request.RecurrenceFound,
            FollowUpRequired = request.FollowUpRequired,
            ReopenedCapaRef = NormalizeNullable(request.ReopenedCapaRef),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.EffectivenessVerifications.Add(entity);
        capa.EffectivenessVerificationRefs = capa.EffectivenessVerificationRefs.Append(entity.Number).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        capa.UpdatedAt = now;
        await AddTimelineAsync("capa", capa.Id, "assurarr.capa.verification_started", entity.Number, cancellationToken);
        await AddTimelineAsync("capa_verification", entity.Id, "assurarr.capa.verification_started", entity.Number, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToEffectivenessVerificationResponse(entity);
    }

    public async Task<AssurArrEffectivenessVerificationResponse> UpdateEffectivenessVerificationStatusAsync(Guid capaId, Guid verificationId, UpdateAssurArrEffectivenessVerificationStatusRequest request, CancellationToken cancellationToken = default)
    {
        var capa = await db.Capas.FirstOrDefaultAsync(x => x.Id == capaId, cancellationToken)
            ?? throw new InvalidOperationException($"CAPA '{capaId}' was not found.");
        var entity = await db.EffectivenessVerifications.FirstOrDefaultAsync(x => x.Id == verificationId && x.CapaId == capaId, cancellationToken)
            ?? throw new InvalidOperationException($"Effectiveness verification '{verificationId}' was not found.");
        EnsureTransition(entity.Status, request.Status, EffectivenessVerificationTransitions, "effectiveness verification");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.ResultSummary = NormalizeNullable(request.ResultSummary) ?? entity.ResultSummary;
        if (request.RecurrenceFound.HasValue)
        {
            entity.RecurrenceFound = request.RecurrenceFound.Value;
        }
        if (request.FollowUpRequired.HasValue)
        {
            entity.FollowUpRequired = request.FollowUpRequired.Value;
        }
        entity.ReopenedCapaRef = NormalizeNullable(request.ReopenedCapaRef) ?? entity.ReopenedCapaRef;
        if (string.Equals(entity.Status, "effective", StringComparison.OrdinalIgnoreCase))
        {
            EnsureTransition(capa.Status, "effective", CapaTransitions, "CAPA");
            capa.Status = "effective";
            EnsureTransition(capa.Status, "closed", CapaTransitions, "CAPA");
            capa.Status = "closed";
            capa.ClosedAt = entity.UpdatedAt;
            capa.ClosureSummary = entity.ResultSummary ?? capa.ClosureSummary;
            await AddTimelineAsync("capa", capaId, "assurarr.capa.verified_effective", entity.ResultSummary ?? entity.Number, cancellationToken);
            await AddTimelineAsync("capa", capaId, "assurarr.capa.closed", entity.ResultSummary ?? entity.Number, cancellationToken);
        }
        else if (string.Equals(entity.Status, "ineffective", StringComparison.OrdinalIgnoreCase))
        {
            EnsureTransition(capa.Status, "ineffective", CapaTransitions, "CAPA");
            capa.Status = "ineffective";
            capa.ClosedAt = null;
            capa.ClosureSummary = entity.ResultSummary ?? capa.ClosureSummary;
            await AddTimelineAsync("capa", capaId, "assurarr.capa.reopened", entity.ResultSummary ?? entity.Number, cancellationToken);
            await AddTimelineAsync("capa", capaId, "assurarr.capa.verified_ineffective", entity.ResultSummary ?? entity.Number, cancellationToken);
        }
        capa.UpdatedAt = entity.UpdatedAt;
        await AddTimelineAsync("capa_verification", entity.Id, $"assurarr.capa.verification.{entity.Status}", entity.ResultSummary ?? entity.Number, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToEffectivenessVerificationResponse(entity);
    }

    public async Task<List<AssurArrQualityAuditResponse>> ListAuditsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.QualityAudits
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToAuditResponse).ToList();
    }

    public async Task<AssurArrQualityAuditResponse?> GetAuditAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await db.QualityAudits.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)) is { } entity
            ? ToAuditResponse(entity)
            : null;

    public async Task<AssurArrQualityAuditResponse> CreateAuditAsync(CreateAssurArrQualityAuditRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrQualityAudit
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("AUD", db.QualityAudits, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "moderate"),
            Status = "planned",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            OwnerPersonId = request.OwnerPersonId,
            RecordRefs = [],
            CreatedAt = now,
            UpdatedAt = now,
            AuditType = Normalize(request.AuditType, "internal"),
            AuditScope = NormalizeNullable(request.AuditScope),
            StandardRefs = request.StandardRefs ?? [],
            ComplianceRefs = request.ComplianceRefs ?? [],
            AuditorPersonIds = request.AuditorPersonIds ?? [],
            LeadAuditorPersonId = request.LeadAuditorPersonId,
            AuditeeRefs = request.AuditeeRefs ?? [],
            StaffArrSiteId = request.StaffArrSiteId,
            StaffArrLocationId = request.StaffArrLocationId,
            SupplierRef = NormalizeNullable(request.SupplierRef),
            CustomerRef = NormalizeNullable(request.CustomerRef),
            PlannedStartAt = request.PlannedStartAt,
            PlannedEndAt = request.PlannedEndAt,
            ActualStartAt = request.ActualStartAt,
            ActualEndAt = request.ActualEndAt,
            ChecklistRefs = request.ChecklistRefs ?? [],
            AuditTrail = [CreateAuditTrailEntry("created", now, request.Title)],
        };

        db.QualityAudits.Add(entity);
        await AddTimelineAsync("audit", entity.Id, "assurarr.audit.created", entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToAuditResponse(entity);
    }

    public async Task<AssurArrQualityAuditResponse> UpdateAuditStatusAsync(Guid id, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.QualityAudits.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Quality audit '{id}' was not found.");
        EnsureTransition(entity.Status, request.Status, AuditTransitions, "quality audit");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(entity.Status, "in_progress", StringComparison.OrdinalIgnoreCase))
        {
            entity.ActualStartAt ??= entity.UpdatedAt;
            await AddTimelineAsync("audit", entity.Id, "assurarr.audit.started", entity.Title, cancellationToken);
            entity.AuditTrail = AppendAuditTrail(entity.AuditTrail, "started", entity.Status, entity.UpdatedAt);
        }
        if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
            entity.ActualEndAt ??= entity.UpdatedAt;
            entity.AuditTrail = AppendAuditTrail(entity.AuditTrail, "closed", entity.Status, entity.UpdatedAt);
            await AddTimelineAsync("audit", entity.Id, "assurarr.audit.closed", entity.Title, cancellationToken);
        }
        entity.AuditTrail = AppendAuditTrail(entity.AuditTrail, "status_changed", entity.Status, entity.UpdatedAt);
        await AddTimelineAsync("audit", entity.Id, "assurarr.audit.status_changed", entity.Status, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToAuditResponse(entity);
    }

    public async Task<List<AssurArrQualityAuditChecklistResponse>> ListAuditChecklistsAsync(Guid auditId, CancellationToken cancellationToken = default)
    {
        var entities = await db.QualityAuditChecklists
            .AsNoTracking()
            .Where(x => x.AuditId == auditId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToChecklistResponse).ToList();
    }

    public async Task<AssurArrQualityAuditChecklistResponse> GetAuditChecklistAsync(Guid auditId, Guid checklistId, CancellationToken cancellationToken = default)
    {
        var entity = await db.QualityAuditChecklists.AsNoTracking().FirstOrDefaultAsync(x => x.Id == checklistId && x.AuditId == auditId, cancellationToken)
            ?? throw new InvalidOperationException($"Quality audit checklist '{checklistId}' was not found.");

        return ToChecklistResponse(entity);
    }

    public async Task<AssurArrQualityAuditChecklistResponse> CreateAuditChecklistAsync(Guid auditId, CreateAssurArrQualityAuditChecklistRequest request, CancellationToken cancellationToken = default)
    {
        var audit = await db.QualityAudits.FirstOrDefaultAsync(x => x.Id == auditId, cancellationToken)
            ?? throw new InvalidOperationException($"Quality audit '{auditId}' was not found.");

        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrQualityAuditChecklist
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("CHK", db.QualityAuditChecklists, cancellationToken),
            AuditId = audit.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = Normalize(request.Status, "draft"),
            ItemRefs = [],
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.QualityAuditChecklists.Add(entity);
        audit.ChecklistRefs = [.. audit.ChecklistRefs, entity.Number];
        audit.UpdatedAt = now;
        await AddTimelineAsync("audit", audit.Id, "assurarr.audit.checklist_created", entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToChecklistResponse(entity);
    }

    public async Task<AssurArrQualityAuditChecklistResponse> UpdateAuditChecklistStatusAsync(Guid auditId, Guid checklistId, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.QualityAuditChecklists.FirstOrDefaultAsync(x => x.Id == checklistId && x.AuditId == auditId, cancellationToken)
            ?? throw new InvalidOperationException($"Quality audit checklist '{checklistId}' was not found.");
        EnsureTransition(entity.Status, request.Status, ChecklistTransitions, "quality audit checklist");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(entity.Status, "completed", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
        }
        await AddTimelineAsync("audit_checklist", entity.Id, "assurarr.audit.checklist.status_changed", entity.Status, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToChecklistResponse(entity);
    }

    public async Task<List<AssurArrQualityAuditChecklistItemResponse>> ListAuditChecklistItemsAsync(Guid auditId, Guid checklistId, CancellationToken cancellationToken = default)
    {
        var checklist = await db.QualityAuditChecklists.AsNoTracking().FirstOrDefaultAsync(x => x.Id == checklistId && x.AuditId == auditId, cancellationToken)
            ?? throw new InvalidOperationException($"Quality audit checklist '{checklistId}' was not found.");

        var entities = await db.QualityAuditChecklistItems
            .AsNoTracking()
            .Where(x => x.ChecklistId == checklist.Id)
            .OrderBy(x => x.Sequence)
            .ToListAsync(cancellationToken);

        return entities.Select(ToChecklistItemResponse).ToList();
    }

    public async Task<AssurArrQualityAuditChecklistItemResponse> GetAuditChecklistItemAsync(Guid auditId, Guid checklistId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var checklist = await db.QualityAuditChecklists.AsNoTracking().FirstOrDefaultAsync(x => x.Id == checklistId && x.AuditId == auditId, cancellationToken)
            ?? throw new InvalidOperationException($"Quality audit checklist '{checklistId}' was not found.");

        var entity = await db.QualityAuditChecklistItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == itemId && x.ChecklistId == checklist.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Quality audit checklist item '{itemId}' was not found.");

        return ToChecklistItemResponse(entity);
    }

    public async Task<AssurArrQualityAuditChecklistItemResponse> CreateAuditChecklistItemAsync(Guid auditId, Guid checklistId, CreateAssurArrQualityAuditChecklistItemRequest request, CancellationToken cancellationToken = default)
    {
        var checklist = await db.QualityAuditChecklists.FirstOrDefaultAsync(x => x.Id == checklistId && x.AuditId == auditId, cancellationToken)
            ?? throw new InvalidOperationException($"Quality audit checklist '{checklistId}' was not found.");

        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrQualityAuditChecklistItem
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("CHKI", db.QualityAuditChecklistItems, cancellationToken),
            ChecklistId = checklist.Id,
            Sequence = request.Sequence,
            Prompt = request.Prompt.Trim(),
            HelpText = NormalizeNullable(request.HelpText),
            RequirementRef = NormalizeNullable(request.RequirementRef),
            ResponseType = Normalize(request.ResponseType, "pass_fail"),
            Required = request.Required,
            ResponseValue = NormalizeNullable(request.ResponseValue),
            Result = NormalizeNullable(request.Result),
            FindingCreated = request.FindingCreated,
            FindingRef = NormalizeNullable(request.FindingRef),
            EvidenceRecordRefs = request.EvidenceRecordRefs ?? [],
            AnsweredAt = request.AnsweredAt,
            AnsweredByPersonId = db.CurrentPersonId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.QualityAuditChecklistItems.Add(entity);
        checklist.ItemRefs = [.. checklist.ItemRefs, entity.Number];
        checklist.UpdatedAt = now;
        await AddTimelineAsync("audit_checklist", checklist.Id, "assurarr.audit.checklist.item_created", entity.Prompt, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToChecklistItemResponse(entity);
    }

    public async Task<AssurArrQualityAuditChecklistItemResponse> UpdateAuditChecklistItemResponseAsync(
        Guid auditId,
        Guid checklistId,
        Guid itemId,
        UpdateAssurArrQualityAuditChecklistItemResponseRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.QualityAuditChecklistItems
            .FirstOrDefaultAsync(x => x.Id == itemId && x.ChecklistId == checklistId, cancellationToken)
            ?? throw new InvalidOperationException($"Quality audit checklist item '{itemId}' was not found.");

        var checklist = await db.QualityAuditChecklists.FirstOrDefaultAsync(x => x.Id == checklistId && x.AuditId == auditId, cancellationToken)
            ?? throw new InvalidOperationException($"Quality audit checklist '{checklistId}' was not found.");

        entity.ResponseValue = NormalizeNullable(request.ResponseValue);
        entity.Result = NormalizeNullable(request.Result);
        entity.FindingCreated = request.FindingCreated;
        entity.FindingRef = NormalizeNullable(request.FindingRef);
        entity.EvidenceRecordRefs = request.EvidenceRecordRefs ?? [];
        entity.AnsweredAt = request.AnsweredAt ?? DateTimeOffset.UtcNow;
        entity.AnsweredByPersonId = db.CurrentPersonId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        checklist.UpdatedAt = entity.UpdatedAt;
        await AddTimelineAsync("audit_checklist_item", entity.Id, "assurarr.audit.checklist.item_answered", entity.Result ?? entity.ResponseValue, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToChecklistItemResponse(entity);
    }

    public async Task<List<AssurArrAuditFindingResponse>> ListFindingsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.AuditFindings
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToFindingResponse).ToList();
    }

    public async Task<AssurArrAuditFindingResponse?> GetFindingAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await db.AuditFindings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)) is { } entity
            ? ToFindingResponse(entity)
            : null;

    public async Task<AssurArrAuditFindingResponse> CreateFindingAsync(CreateAssurArrAuditFindingRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrAuditFinding
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("FIND", db.AuditFindings, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "moderate"),
            Status = "open",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            OwnerPersonId = request.OwnerPersonId,
            SourceRequirementRef = NormalizeNullable(request.SourceRequirementRef),
            RecordRefs = [],
            EvidenceRecordRefs = request.EvidenceRecordRefs ?? [],
            CreatedAt = now,
            UpdatedAt = now,
            FindingType = Normalize(request.FindingType, "observation"),
            AuditRef = NormalizeNullable(request.AuditRef),
            NonconformanceRef = NormalizeNullable(request.NonconformanceRef),
            CapaRef = NormalizeNullable(request.CapaRef),
            DueAt = request.DueAt,
        };

        db.AuditFindings.Add(entity);
        await AddTimelineAsync("finding", entity.Id, "assurarr.finding.created", entity.Title, cancellationToken);
        if (!string.IsNullOrWhiteSpace(entity.AuditRef))
        {
            await AddTimelineAsync("finding", entity.Id, "assurarr.audit.finding_created", entity.Title, cancellationToken);
        }
        await db.SaveChangesAsync(cancellationToken);
        return ToFindingResponse(entity);
    }

    public async Task<AssurArrAuditFindingResponse> UpdateFindingStatusAsync(Guid id, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.AuditFindings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Audit finding '{id}' was not found.");
        EnsureTransition(entity.Status, request.Status, FindingTransitions, "audit finding");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(entity.Status, "accepted", StringComparison.OrdinalIgnoreCase))
        {
            await AddTimelineAsync("finding", entity.Id, "assurarr.finding.accepted", entity.Title, cancellationToken);
        }
        if (string.Equals(entity.Status, "nonconformance_created", StringComparison.OrdinalIgnoreCase))
        {
            await AddTimelineAsync("finding", entity.Id, "assurarr.finding.nonconformance_created", entity.Title, cancellationToken);
        }
        if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
            await AddTimelineAsync("finding", entity.Id, "assurarr.finding.closed", entity.Title, cancellationToken);
        }
        await AddTimelineAsync("finding", entity.Id, "assurarr.finding.status_changed", entity.Status, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToFindingResponse(entity);
    }

    public async Task<List<AssurArrQualityReviewResponse>> ListQualityReviewsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.QualityReviews
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var responses = new List<AssurArrQualityReviewResponse>(entities.Count);
        foreach (var entity in entities)
        {
            responses.Add(await ToReviewResponseAsync(entity, cancellationToken));
        }

        return responses;
    }

    public async Task<AssurArrQualityReviewResponse?> GetQualityReviewAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await db.QualityReviews.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)) is { } entity
            ? await ToReviewResponseAsync(entity, cancellationToken)
            : null;

    public async Task<AssurArrQualityReviewResponse> CreateQualityReviewAsync(CreateAssurArrQualityReviewRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrQualityReview
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("QREV", db.QualityReviews, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "moderate"),
            Status = "pending",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            OwnerPersonId = request.OwnerPersonId,
            RecordRefs = [],
            CreatedAt = now,
            UpdatedAt = now,
            ReviewType = Normalize(request.ReviewType, "nonconformance_review"),
            SourceReviewRef = NormalizeNullable(request.SourceReviewRef),
            ReviewerPersonId = db.CurrentPersonId,
            RequestedAt = request.RequestedAt ?? now,
            DueAt = request.DueAt,
            DecisionReason = NormalizeNullable(request.DecisionReason),
            RequiredEvidenceRefs = request.RequiredEvidenceRefs ?? [],
            SubmittedEvidenceRefs = request.SubmittedEvidenceRefs ?? [],
            Notes = NormalizeNullable(request.Notes),
        };

        db.QualityReviews.Add(entity);
        await AddTimelineAsync("review", entity.Id, "assurarr.quality_review.requested", entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToReviewResponseAsync(entity, cancellationToken);
    }

    public async Task<AssurArrQualityReviewResponse> UpdateQualityReviewStatusAsync(Guid id, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.QualityReviews.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Quality review '{id}' was not found.");
        EnsureTransition(entity.Status, request.Status, ReviewTransitions, "quality review");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(entity.Status, "approved", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "rejected", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "canceled", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.DecisionAt = entity.DecisionAt ?? entity.UpdatedAt;
            entity.DecisionReason = request.ClosureSummary ?? entity.DecisionReason;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
        }
        if (string.Equals(entity.Status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            await AddTimelineAsync("review", entity.Id, "assurarr.quality_review.approved", entity.Title, cancellationToken);
        }
        else if (string.Equals(entity.Status, "rejected", StringComparison.OrdinalIgnoreCase))
        {
            await AddTimelineAsync("review", entity.Id, "assurarr.quality_review.rejected", entity.Title, cancellationToken);
        }
        await AddTimelineAsync("review", entity.Id, $"assurarr.quality_review.{entity.Status}", entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToReviewResponseAsync(entity, cancellationToken);
    }

    public async Task<List<AssurArrQualityReleaseResponse>> ListQualityReleasesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.QualityReleases
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var responses = new List<AssurArrQualityReleaseResponse>(entities.Count);
        foreach (var entity in entities)
        {
            responses.Add(await ToReleaseResponseAsync(entity, cancellationToken));
        }

        return responses;
    }

    public async Task<AssurArrQualityReleaseResponse?> GetQualityReleaseAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await db.QualityReleases.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)) is { } entity
            ? await ToReleaseResponseAsync(entity, cancellationToken)
            : null;

    public async Task<AssurArrQualityReleaseResponse> CreateQualityReleaseAsync(CreateAssurArrQualityReleaseRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrQualityRelease
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("QREL", db.QualityReleases, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "none"),
            Status = "requested",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            OwnerPersonId = request.OwnerPersonId,
            RecordRefs = [],
            CreatedAt = now,
            UpdatedAt = now,
            HoldRef = request.HoldRef.Trim(),
            ReleaseType = Normalize(request.ReleaseType, "full"),
            RequestedByPersonId = db.CurrentPersonId,
            RequestedAt = request.RequestedAt ?? now,
            Conditions = NormalizeNullable(request.Conditions),
            ExpirationAt = request.ExpirationAt,
            EvidenceRecordRefs = request.EvidenceRecordRefs ?? [],
            Notes = NormalizeNullable(request.Notes),
        };

        db.QualityReleases.Add(entity);
        await AddTimelineAsync("release", entity.Id, "assurarr.quality_release.requested", entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToReleaseResponseAsync(entity, cancellationToken);
    }

    public async Task<AssurArrQualityReleaseResponse> UpdateQualityReleaseStatusAsync(Guid id, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.QualityReleases.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Quality release '{id}' was not found.");
        EnsureTransition(entity.Status, request.Status, ReleaseTransitions, "quality release");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(entity.Status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            entity.ApprovedAt = entity.ApprovedAt ?? entity.UpdatedAt;
            await AddTimelineAsync("release", entity.Id, "assurarr.quality_release.approved", entity.Title, cancellationToken);
        }
        else if (string.Equals(entity.Status, "executed", StringComparison.OrdinalIgnoreCase))
        {
            entity.ApprovedAt ??= entity.UpdatedAt;
            entity.ExecutedAt = entity.UpdatedAt;
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
            await AddTimelineAsync("release", entity.Id, "assurarr.quality_release.executed", entity.Title, cancellationToken);
        }
        else if (string.Equals(entity.Status, "rejected", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "canceled", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
            if (string.Equals(entity.Status, "rejected", StringComparison.OrdinalIgnoreCase))
            {
                await AddTimelineAsync("release", entity.Id, "assurarr.quality_release.rejected", entity.Title, cancellationToken);
            }
        }

        await AddTimelineAsync("release", entity.Id, $"assurarr.quality_release.{entity.Status}", entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToReleaseResponseAsync(entity, cancellationToken);
    }

    public async Task<List<AssurArrContainmentActionResponse>> ListContainmentActionsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.ContainmentActions
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToContainmentActionResponse).ToList();
    }

    public async Task<AssurArrContainmentActionResponse> GetContainmentActionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.ContainmentActions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Containment action '{id}' was not found.");

        return ToContainmentActionResponse(entity);
    }

    public async Task<AssurArrContainmentActionResponse> CreateContainmentActionAsync(CreateAssurArrContainmentActionRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrContainmentAction
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("CONT", db.ContainmentActions, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "moderate"),
            Status = "open",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            NonconformanceRef = NormalizeNullable(request.NonconformanceRef),
            ActionType = Normalize(request.ActionType, "hold_inventory"),
            AssignedPersonId = db.CurrentPersonId,
            AssignedTeamRef = NormalizeNullable(request.AssignedTeamRef),
            SourceProductActionRef = NormalizeNullable(request.SourceProductActionRef),
            DueAt = request.DueAt,
            VerificationRequired = request.VerificationRequired,
            EvidenceRecordRefs = request.EvidenceRecordRefs ?? [],
            Notes = NormalizeNullable(request.Notes),
            CreatedAt = now,
            UpdatedAt = now,
            StartedAt = now,
        };

        db.ContainmentActions.Add(entity);
        await AddTimelineAsync("containment", entity.Id, "assurarr.containment.created", entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToContainmentActionResponse(entity);
    }

    public async Task<AssurArrContainmentActionResponse> UpdateContainmentActionStatusAsync(Guid id, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.ContainmentActions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Containment action '{id}' was not found.");
        EnsureTransition(entity.Status, request.Status, ContainmentTransitions, "containment action");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(entity.Status, "completed", StringComparison.OrdinalIgnoreCase))
        {
            entity.CompletedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
        }
        else if (string.Equals(entity.Status, "verified", StringComparison.OrdinalIgnoreCase))
        {
            entity.VerifiedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
        }
        else if (string.Equals(entity.Status, "canceled", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
        }

        var eventType = entity.Status.ToLowerInvariant() switch
        {
            "assigned" => "assurarr.containment.assigned",
            "completed" => "assurarr.containment.completed",
            "verified" => "assurarr.containment.verified",
            _ => "assurarr.containment.status_changed",
        };

        await AddTimelineAsync("containment", entity.Id, eventType, entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToContainmentActionResponse(entity);
    }

    public async Task<List<AssurArrDispositionResponse>> ListDispositionsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.Dispositions
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToDispositionResponse).ToList();
    }

    public async Task<AssurArrDispositionResponse> GetDispositionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Dispositions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Disposition '{id}' was not found.");

        return ToDispositionResponse(entity);
    }

    public async Task<AssurArrDispositionResponse> CreateDispositionAsync(CreateAssurArrDispositionRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrDisposition
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("DISP", db.Dispositions, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "moderate"),
            Status = "proposed",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            NonconformanceRef = NormalizeNullable(request.NonconformanceRef),
            DispositionType = Normalize(request.DispositionType, "use_as_is"),
            DecisionByPersonId = db.CurrentPersonId,
            DecisionAt = request.DecisionAt ?? now,
            ApprovedByPersonId = db.CurrentPersonId,
            ApprovedAt = request.ApprovedAt,
            Rationale = NormalizeNullable(request.Rationale),
            RequiredActions = request.RequiredActions ?? [],
            ExecutionProduct = NormalizeNullable(request.ExecutionProduct),
            ExecutionObjectRef = NormalizeNullable(request.ExecutionObjectRef),
            EvidenceRecordRefs = request.EvidenceRecordRefs ?? [],
            Notes = NormalizeNullable(request.Notes),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Dispositions.Add(entity);
        await AddTimelineAsync("disposition", entity.Id, "assurarr.disposition.proposed", entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToDispositionResponse(entity);
    }

    public async Task<AssurArrDispositionResponse> UpdateDispositionStatusAsync(Guid id, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.Dispositions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Disposition '{id}' was not found.");
        EnsureTransition(entity.Status, request.Status, DispositionTransitions, "disposition");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(entity.Status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            entity.ApprovedAt ??= entity.UpdatedAt;
        }
        else if (string.Equals(entity.Status, "executed", StringComparison.OrdinalIgnoreCase))
        {
            entity.ApprovedAt ??= entity.UpdatedAt;
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
        }
        else if (string.Equals(entity.Status, "rejected", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "canceled", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
        }

        var eventType = entity.Status.ToLowerInvariant() switch
        {
            "approved" => "assurarr.disposition.approved",
            "executed" => "assurarr.disposition.executed",
            "rejected" => "assurarr.disposition.rejected",
            _ => "assurarr.disposition.status_changed",
        };

        await AddTimelineAsync("disposition", entity.Id, eventType, entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToDispositionResponse(entity);
    }

    public async Task<List<AssurArrSupplierQualityIssueResponse>> ListSupplierQualityIssuesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.SupplierQualityIssues
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToSupplierQualityIssueResponse).ToList();
    }

    public async Task<AssurArrSupplierQualityIssueResponse?> GetSupplierQualityIssueAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.SupplierQualityIssues
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? null : ToSupplierQualityIssueResponse(entity);
    }

    public async Task<AssurArrSupplierQualityIssueResponse> CreateSupplierQualityIssueAsync(CreateAssurArrSupplierQualityIssueRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrSupplierQualityIssue
        {
            Id = Guid.NewGuid(),
            TenantId = db.CurrentTenantId,
            Number = await GenerateNumberAsync("SQA", db.SupplierQualityIssues, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "moderate"),
            Status = "open",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedReceiptRefs = request.AffectedReceiptRefs ?? [],
            AffectedPurchaseOrderRefs = request.AffectedPurchaseOrderRefs ?? [],
            AffectedItemRefs = request.AffectedItemRefs ?? [],
            SupplierRef = NormalizeNullable(request.SupplierRef),
            NonconformanceRef = NormalizeNullable(request.NonconformanceRef),
            ScarRef = NormalizeNullable(request.ScarRef),
            HoldRefs = request.HoldRefs ?? [],
            RecordRefs = request.RecordRefs ?? [],
            CreatedAt = now,
            UpdatedAt = now,
            IssueType = Normalize(request.IssueType, "other"),
            OwnerPersonId = request.OwnerPersonId,
            OpenedAt = request.OpenedAt ?? now,
        };

        if (!string.IsNullOrWhiteSpace(entity.ScarRef))
        {
            await EnsureScarReferenceExistsAsync(entity.ScarRef, cancellationToken);
        }

        db.SupplierQualityIssues.Add(entity);
        await AddTimelineAsync("supplier_quality_issue", entity.Id, "assurarr.supplier_quality_issue.created", entity.Title, cancellationToken);
        await PublishQualityStatusSnapshotAsync(entity, cancellationToken);
        await RecalculateSupplierQualityMetricsAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToSupplierQualityIssueResponse(entity);
    }

    public async Task<AssurArrSupplierQualityIssueResponse> UpdateSupplierQualityIssueStatusAsync(Guid id, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.SupplierQualityIssues.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Supplier quality issue '{id}' was not found.");
        EnsureTransition(entity.Status, request.Status, SupplierQualityTransitions, "supplier quality issue");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "canceled", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
            if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase))
            {
                await AddTimelineAsync("supplier_quality_issue", entity.Id, "assurarr.supplier_quality_issue.closed", entity.Title, cancellationToken);
            }
        }

        await AddTimelineAsync("supplier_quality_issue", entity.Id, "assurarr.supplier_quality_issue.status_changed", entity.Status, cancellationToken);
        await PublishQualityStatusSnapshotAsync(entity, cancellationToken);
        await RecalculateSupplierQualityMetricsAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToSupplierQualityIssueResponse(entity);
    }

    public async Task<List<AssurArrSupplierCorrectiveActionRequestResponse>> ListScarsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.SupplierCorrectiveActionRequests
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToScarResponse).ToList();
    }

    public async Task<AssurArrSupplierCorrectiveActionRequestResponse?> GetScarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.SupplierCorrectiveActionRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? null : ToScarResponse(entity);
    }

    public async Task<AssurArrSupplierCorrectiveActionRequestResponse> CreateScarAsync(CreateAssurArrSupplierCorrectiveActionRequest request, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.FollowUpCapaRef))
        {
            await EnsureCapaReferenceExistsAsync(request.FollowUpCapaRef, cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrSupplierCorrectiveActionRequest
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("SCAR", db.SupplierCorrectiveActionRequests, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "moderate"),
            Status = "draft",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            SupplierRef = NormalizeNullable(request.SupplierRef),
            SourceNonconformanceRef = NormalizeNullable(request.SourceNonconformanceRef),
            SourceCapaRef = NormalizeNullable(request.SourceCapaRef),
            RequestedByPersonId = db.CurrentPersonId,
            RequestedAt = request.RequestedAt ?? now,
            SupplierDueAt = request.SupplierDueAt,
            SupplierResponseRecordRefs = request.SupplierResponseRecordRefs ?? [],
            ReviewPersonId = db.CurrentPersonId,
            ReviewedAt = request.ReviewedAt,
            ReviewDecision = NormalizeNullable(request.ReviewDecision),
            FollowUpCapaRef = NormalizeNullable(request.FollowUpCapaRef),
            RecordRefs = request.RecordRefs ?? [],
            CreatedAt = now,
            UpdatedAt = now,
            OwnerPersonId = request.OwnerPersonId,
        };

        if (!string.IsNullOrWhiteSpace(entity.SourceCapaRef))
        {
            await EnsureCapaReferenceExistsAsync(entity.SourceCapaRef, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(entity.SourceNonconformanceRef))
        {
            await EnsureNonconformanceReferenceExistsAsync(entity.SourceNonconformanceRef, cancellationToken);
        }

        db.SupplierCorrectiveActionRequests.Add(entity);
        await AddTimelineAsync("scar", entity.Id, "assurarr.scar.created", entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToScarResponse(entity);
    }

    public async Task<AssurArrSupplierCorrectiveActionRequestResponse> UpdateScarStatusAsync(Guid id, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.SupplierCorrectiveActionRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Supplier corrective action request '{id}' was not found.");
        EnsureTransition(entity.Status, request.Status, ScarTransitions, "supplier corrective action request");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        if (string.Equals(entity.Status, "sent", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "acknowledged", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "supplier_response_pending", StringComparison.OrdinalIgnoreCase))
        {
            entity.RequestedAt ??= entity.UpdatedAt;
        }

        if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "canceled", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
        }

        var eventType = entity.Status.ToLowerInvariant() switch
        {
            "sent" => "assurarr.scar.sent",
            "response_received" => "assurarr.scar.response_received",
            "accepted" => "assurarr.scar.accepted",
            "rejected" => "assurarr.scar.rejected",
            "closed" => "assurarr.scar.closed",
            _ => "assurarr.scar.status_changed",
        };

        await AddTimelineAsync("scar", entity.Id, eventType, entity.Status, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToScarResponse(entity);
    }

    public async Task<List<AssurArrCustomerComplaintQualityCaseResponse>> ListCustomerComplaintQualityCasesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.CustomerComplaintQualityCases
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToCustomerComplaintQualityCaseResponse).ToList();
    }

    public async Task<AssurArrCustomerComplaintQualityCaseResponse?> GetCustomerComplaintQualityCaseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.CustomerComplaintQualityCases
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? null : ToCustomerComplaintQualityCaseResponse(entity);
    }

    public async Task<AssurArrCustomerComplaintQualityCaseResponse> CreateCustomerComplaintQualityCaseAsync(CreateAssurArrCustomerComplaintQualityCaseRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrCustomerComplaintQualityCase
        {
            Id = Guid.NewGuid(),
            TenantId = db.CurrentTenantId,
            Number = await GenerateNumberAsync("COMP", db.CustomerComplaintQualityCases, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "moderate"),
            Status = "received",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedOrderRefs = request.AffectedOrderRefs ?? [],
            AffectedShipmentRefs = request.AffectedShipmentRefs ?? [],
            AffectedItemRefs = request.AffectedItemRefs ?? [],
            AffectedAssetRefs = request.AffectedAssetRefs ?? [],
            CustomerRef = NormalizeNullable(request.CustomerRef),
            CustomerContactSnapshot = NormalizeNullable(request.CustomerContactSnapshot),
            CustomerLocationRef = NormalizeNullable(request.CustomerLocationRef),
            NonconformanceRef = NormalizeNullable(request.NonconformanceRef),
            HoldRefs = request.HoldRefs ?? [],
            CapaRefs = request.CapaRefs ?? [],
            CustomerResponseRecordRefs = request.CustomerResponseRecordRefs ?? [],
            RecordRefs = request.RecordRefs ?? [],
            CreatedAt = now,
            UpdatedAt = now,
            ComplaintType = Normalize(request.ComplaintType, "other"),
            OwnerPersonId = request.OwnerPersonId,
            ReceivedAt = request.ReceivedAt ?? now,
            ReceivedByPersonId = db.CurrentPersonId,
            CustomerResponseDueAt = request.CustomerResponseDueAt,
        };

        db.CustomerComplaintQualityCases.Add(entity);
        await AddTimelineAsync("customer_complaint", entity.Id, "assurarr.customer_complaint.created", entity.Title, cancellationToken);
        await PublishQualityStatusSnapshotAsync(entity, cancellationToken);
        await RecalculateCustomerComplaintMetricsAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToCustomerComplaintQualityCaseResponse(entity);
    }

    public async Task<AssurArrCustomerComplaintQualityCaseResponse> UpdateCustomerComplaintQualityCaseStatusAsync(Guid id, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.CustomerComplaintQualityCases.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Customer complaint quality case '{id}' was not found.");
        EnsureTransition(entity.Status, request.Status, CustomerComplaintTransitions, "customer complaint quality case");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(entity.Status, "response_pending", StringComparison.OrdinalIgnoreCase))
        {
            await AddTimelineAsync("customer_complaint", entity.Id, "assurarr.customer_complaint.response_sent", entity.Title, cancellationToken);
        }
        if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "canceled", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
            if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase))
            {
                await AddTimelineAsync("customer_complaint", entity.Id, "assurarr.customer_complaint.closed", entity.Title, cancellationToken);
            }
        }

        await AddTimelineAsync("customer_complaint", entity.Id, "assurarr.customer_complaint.status_changed", entity.Status, cancellationToken);
        await PublishQualityStatusSnapshotAsync(entity, cancellationToken);
        await RecalculateCustomerComplaintMetricsAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToCustomerComplaintQualityCaseResponse(entity);
    }

    private async Task PublishQualityStatusSnapshotAsync(AssurArrSupplierQualityIssue entity, CancellationToken cancellationToken)
    {
        var targetProduct = "supplyarr";
        var targetObjectRef = NormalizeNullable(entity.SourceObjectRef) ?? $"assurarr:supplier-quality-issue:{entity.Number}";
        var qualityStatus = DetermineSupplierIssueQualityStatus(entity.Status, entity.Severity);
        var openNonconformanceRefs = string.IsNullOrWhiteSpace(entity.NonconformanceRef)
            || string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "canceled", StringComparison.OrdinalIgnoreCase)
            ? Array.Empty<string>()
            : [entity.NonconformanceRef!];

        await CreateStatusSnapshotAsync(
            new CreateAssurArrQualityStatusSnapshotRequest(
                targetProduct,
                targetObjectRef,
                qualityStatus,
                entity.Severity,
                entity.Title,
                entity.Description,
                entity.SourceProduct,
                entity.SourceObjectRef,
                entity.AffectedItemRefs.ToArray(),
                entity.OwnerPersonId,
                [],
                openNonconformanceRefs,
                [],
                [],
                entity.OpenedAt),
            cancellationToken);
    }

    private static string DetermineSupplierIssueQualityStatus(string status, string severity) =>
        string.Equals(status, "closed", StringComparison.OrdinalIgnoreCase) ? "acceptable"
        : string.Equals(status, "resolved", StringComparison.OrdinalIgnoreCase) ? "acceptable"
        : string.Equals(status, "canceled", StringComparison.OrdinalIgnoreCase) ? "unknown"
        : string.Equals(status, "response_pending", StringComparison.OrdinalIgnoreCase) ? "under_review"
        : string.Equals(status, "under_review", StringComparison.OrdinalIgnoreCase) ? "under_review"
        : string.Equals(status, "corrective_action", StringComparison.OrdinalIgnoreCase) ? "under_review"
        : string.Equals(status, "supplier_notified", StringComparison.OrdinalIgnoreCase) ? "warning"
        : string.Equals(status, "open", StringComparison.OrdinalIgnoreCase) ? "warning"
        : (string.Equals(severity, "critical", StringComparison.OrdinalIgnoreCase) || string.Equals(severity, "high", StringComparison.OrdinalIgnoreCase))
            ? "warning"
            : "under_review";

    private async Task PublishQualityStatusSnapshotAsync(AssurArrCustomerComplaintQualityCase entity, CancellationToken cancellationToken)
    {
        var targetProduct = "customarr";
        var targetObjectRef = NormalizeNullable(entity.SourceObjectRef) ?? $"assurarr:customer-complaint:{entity.Number}";
        var qualityStatus = DetermineCustomerComplaintQualityStatus(entity.Status, entity.Severity);
        var openNonconformanceRefs = string.IsNullOrWhiteSpace(entity.NonconformanceRef)
            || string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "canceled", StringComparison.OrdinalIgnoreCase)
            ? Array.Empty<string>()
            : [entity.NonconformanceRef!];
        var openCapaRefs = string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "canceled", StringComparison.OrdinalIgnoreCase)
            ? Array.Empty<string>()
            : entity.CapaRefs.ToArray();

        await CreateStatusSnapshotAsync(
            new CreateAssurArrQualityStatusSnapshotRequest(
                targetProduct,
                targetObjectRef,
                qualityStatus,
                entity.Severity,
                entity.Title,
                entity.Description,
                entity.SourceProduct,
                entity.SourceObjectRef,
                entity.AffectedItemRefs.Concat(entity.AffectedShipmentRefs).Concat(entity.AffectedOrderRefs).Concat(entity.AffectedAssetRefs).ToArray(),
                entity.OwnerPersonId,
                [],
                openNonconformanceRefs,
                openCapaRefs,
                [],
                entity.CustomerResponseDueAt),
            cancellationToken);
    }

    private static string DetermineCustomerComplaintQualityStatus(string status, string severity) =>
        string.Equals(status, "closed", StringComparison.OrdinalIgnoreCase) ? "acceptable"
        : string.Equals(status, "resolved", StringComparison.OrdinalIgnoreCase) ? "acceptable"
        : string.Equals(status, "canceled", StringComparison.OrdinalIgnoreCase) ? "unknown"
        : string.Equals(status, "response_pending", StringComparison.OrdinalIgnoreCase) ? "under_review"
        : string.Equals(status, "investigation", StringComparison.OrdinalIgnoreCase) ? "under_review"
        : string.Equals(status, "containment", StringComparison.OrdinalIgnoreCase) ? "on_hold"
        : string.Equals(status, "corrective_action", StringComparison.OrdinalIgnoreCase) ? "under_review"
        : string.Equals(status, "triage", StringComparison.OrdinalIgnoreCase) ? "warning"
        : string.Equals(status, "received", StringComparison.OrdinalIgnoreCase) ? "warning"
        : (string.Equals(severity, "critical", StringComparison.OrdinalIgnoreCase) || string.Equals(severity, "high", StringComparison.OrdinalIgnoreCase))
            ? "warning"
            : "under_review";

    public async Task<List<AssurArrQualityStatusSnapshotResponse>> ListStatusSnapshotsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.QualityStatusSnapshots
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var responses = new List<AssurArrQualityStatusSnapshotResponse>(entities.Count);
        foreach (var entity in entities)
        {
            responses.Add(await ToStatusSnapshotResponseAsync(entity, cancellationToken));
        }

        return responses;
    }

    public async Task<AssurArrQualityStatusSnapshotResponse> GetStatusSnapshotAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await db.QualityStatusSnapshots.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)) is { } entity
            ? await ToStatusSnapshotResponseAsync(entity, cancellationToken)
            : throw new InvalidOperationException($"Quality status snapshot '{id}' was not found.");

    public async Task<List<AssurArrQualityStatusSnapshotResponse>> ListQualityStatusAsync(CancellationToken cancellationToken = default) =>
        await ListStatusSnapshotsAsync(cancellationToken);

    public async Task<AssurArrQualityStatusSnapshotResponse?> GetQualityStatusAsync(string targetProduct, string targetObjectId, CancellationToken cancellationToken = default)
    {
        var normalizedTargetProduct = targetProduct.Trim();
        var normalizedTargetObjectId = targetObjectId.Trim();

        var entity = await db.QualityStatusSnapshots
            .AsNoTracking()
            .Where(x => x.TargetProduct == normalizedTargetProduct
                && (x.TargetObjectRef == normalizedTargetObjectId || x.TargetObjectRef.EndsWith($":{normalizedTargetObjectId}", StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : await ToStatusSnapshotResponseAsync(entity, cancellationToken);
    }

    public async Task<AssurArrQualityStatusSnapshotResponse> CreateQualityStatusCheckAsync(CreateAssurArrQualityStatusSnapshotRequest request, CancellationToken cancellationToken = default) =>
        await CreateStatusSnapshotAsync(request, cancellationToken);

    public async Task<AssurArrQualityStatusSnapshotResponse> CreateStatusSnapshotAsync(CreateAssurArrQualityStatusSnapshotRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrQualityStatusSnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("QS", db.QualityStatusSnapshots, cancellationToken),
            Title = request.TargetProduct.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "none"),
            Status = "published",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            OwnerPersonId = request.OwnerPersonId,
            RecordRefs = [],
            CreatedAt = now,
            UpdatedAt = now,
            TargetProduct = request.TargetProduct.Trim(),
            TargetObjectRef = request.TargetObjectRef.Trim(),
            QualityStatus = Normalize(request.QualityStatus, "unknown"),
            ActiveHoldRefs = request.ActiveHoldRefs ?? [],
            OpenNonconformanceRefs = request.OpenNonconformanceRefs ?? [],
            OpenCapaRefs = request.OpenCapaRefs ?? [],
            OpenFindingRefs = request.OpenFindingRefs ?? [],
            LastReviewedAt = now,
            ExpiresAt = request.ExpiresAt,
            Notes = NormalizeNullable(request.Notes),
        };

        db.QualityStatusSnapshots.Add(entity);
        await AddTimelineAsync("status", entity.Id, "assurarr.quality_status.changed", entity.QualityStatus, cancellationToken);
        await AddTimelineAsync("status", entity.Id, "assurarr.quality_status.published", entity.TargetObjectRef, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToStatusSnapshotResponseAsync(entity, cancellationToken);
    }

    public async Task<List<AssurArrQualityScorecardResponse>> ListScorecardsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.QualityScorecards
            .AsNoTracking()
            .OrderByDescending(x => x.GeneratedAt)
            .ToListAsync(cancellationToken);

        var responses = new List<AssurArrQualityScorecardResponse>(entities.Count);
        foreach (var entity in entities)
        {
            responses.Add(await ToScorecardResponseAsync(entity, cancellationToken));
        }

        return responses;
    }

    public async Task<AssurArrQualityScorecardResponse?> GetScorecardAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await db.QualityScorecards.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)) is { } entity
            ? await ToScorecardResponseAsync(entity, cancellationToken)
            : null;

    public async Task<AssurArrQualityScorecardResponse> CreateScorecardAsync(CreateAssurArrQualityScorecardRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrQualityScorecard
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("SCORE", db.QualityScorecards, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "none"),
            Status = "active",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            OwnerPersonId = request.OwnerPersonId,
            RecordRefs = [],
            CreatedAt = now,
            UpdatedAt = now,
            TargetType = Normalize(request.TargetType, "other"),
            TargetRef = request.TargetRef.Trim(),
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            OverallScore = request.OverallScore,
            QualityStatus = Normalize(request.QualityStatus, "unknown"),
            Trend = Normalize(request.Trend, "unknown"),
            GeneratedAt = now,
            GeneratedBy = "system",
            MetricRefs = request.MetricRefs ?? [],
        };

        db.QualityScorecards.Add(entity);
        await AddTimelineAsync("scorecard", entity.Id, "assurarr.scorecard.generated", entity.TargetRef, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToScorecardResponseAsync(entity, cancellationToken);
    }

    public async Task<AssurArrQualityScorecardResponse?> ReviewScorecardAsync(Guid id, ReviewAssurArrQualityScorecardRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.QualityScorecards.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var now = request.ReviewedAt ?? DateTimeOffset.UtcNow;
        entity.ReviewedByPersonId = db.CurrentPersonId;
        entity.ReviewedAt = now;
        entity.UpdatedAt = now;
        await AddTimelineAsync("scorecard", entity.Id, "assurarr.scorecard.reviewed", entity.TargetRef, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToScorecardResponseAsync(entity, cancellationToken);
    }

    public async Task<List<AssurArrQualityRiskProfileResponse>> ListQualityRiskProfilesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.QualityRiskProfiles
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        var responses = new List<AssurArrQualityRiskProfileResponse>(entities.Count);
        foreach (var entity in entities)
        {
            responses.Add(await ToRiskProfileResponseAsync(entity, cancellationToken));
        }

        return responses;
    }

    public async Task<AssurArrQualityRiskProfileResponse?> GetQualityRiskProfileAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await db.QualityRiskProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)) is { } entity
            ? await ToRiskProfileResponseAsync(entity, cancellationToken)
            : null;

    public async Task<AssurArrQualityRiskProfileResponse> CreateQualityRiskProfileAsync(CreateAssurArrQualityRiskProfileRequest request, CancellationToken cancellationToken = default)
    {
        var targetType = Normalize(request.TargetType, "other");
        if (!RiskProfileTargetTypes.Contains(targetType))
        {
            throw new InvalidOperationException($"Unsupported risk profile target type '{request.TargetType}'.");
        }

        var riskLevel = Normalize(request.RiskLevel, "unknown");
        if (!RiskLevels.Contains(riskLevel))
        {
            throw new InvalidOperationException($"Unsupported risk level '{request.RiskLevel}'.");
        }

        var existing = await db.QualityRiskProfiles.FirstOrDefaultAsync(x => x.TargetType == targetType && x.TargetRef == request.TargetRef.Trim(), cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var entity = existing ?? new AssurArrQualityRiskProfile
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            TargetType = targetType,
            TargetRef = request.TargetRef.Trim(),
            CreatedAt = now,
        };

        entity.RiskLevel = riskLevel;
        entity.RiskFactors = request.RiskFactors ?? [];
        entity.OpenIssueCount = request.OpenIssueCount;
        entity.RepeatIssueCount = request.RepeatIssueCount;
        entity.CriticalIssueCount = request.CriticalIssueCount;
        entity.LastIncidentAt = request.LastIncidentAt;
        entity.MitigationActions = request.MitigationActions ?? [];
        entity.ReviewedAt = request.ReviewedAt;
        entity.ReviewedByPersonId = db.CurrentPersonId;
        entity.UpdatedAt = now;

        if (existing is null)
        {
            db.QualityRiskProfiles.Add(entity);
        }

        await AddTimelineAsync("risk-profile", entity.Id, "assurarr.risk_profile.updated", entity.TargetRef, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ToRiskProfileResponseAsync(entity, cancellationToken);
    }

    public async Task<List<AssurArrQualityMetricResponse>> ListQualityMetricsAsync(Guid scorecardId, CancellationToken cancellationToken = default)
    {
        var entities = await db.QualityMetrics
            .AsNoTracking()
            .Where(x => x.ScorecardId == scorecardId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToMetricResponse).ToList();
    }

    public async Task<AssurArrQualityMetricResponse> GetQualityMetricAsync(Guid scorecardId, Guid metricId, CancellationToken cancellationToken = default)
    {
        var entity = await db.QualityMetrics
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == metricId && x.ScorecardId == scorecardId, cancellationToken)
            ?? throw new InvalidOperationException($"Quality metric '{metricId}' was not found.");

        return ToMetricResponse(entity);
    }

    public async Task<AssurArrQualityMetricResponse> CreateQualityMetricAsync(Guid scorecardId, CreateAssurArrQualityMetricRequest request, CancellationToken cancellationToken = default)
    {
        var scorecard = await db.QualityScorecards.FirstOrDefaultAsync(x => x.Id == scorecardId, cancellationToken);
        if (scorecard is null)
        {
            throw new InvalidOperationException($"Scorecard '{scorecardId}' was not found.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrQualityMetric
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            ScorecardId = scorecardId,
            MetricKey = request.MetricKey.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Category = Normalize(request.Category, "other"),
            Value = request.Value,
            Numerator = request.Numerator,
            Denominator = request.Denominator,
            Unit = NormalizeNullable(request.Unit),
            TargetValue = request.TargetValue,
            WarningThreshold = request.WarningThreshold,
            CriticalThreshold = request.CriticalThreshold,
            Status = Normalize(request.Status, "unknown"),
            SourceProductRefs = request.SourceProductRefs ?? [],
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.QualityMetrics.Add(entity);
        if (!scorecard.MetricRefs.Contains(request.MetricKey.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            scorecard.MetricRefs = scorecard.MetricRefs.Append(request.MetricKey.Trim()).ToArray();
            scorecard.UpdatedAt = now;
        }

        await AddTimelineAsync("scorecard", scorecardId, "assurarr.metric.calculated", entity.MetricKey, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToMetricResponse(entity);
    }

    private async Task RecalculateSupplierQualityMetricsAsync(AssurArrSupplierQualityIssue entity, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entity.SupplierRef))
        {
            return;
        }

        var supplierRef = entity.SupplierRef.Trim();
        var openIssueCount = await db.SupplierQualityIssues.CountAsync(x =>
            x.SupplierRef == supplierRef && x.Status != "closed" && x.Status != "canceled", cancellationToken);

        var scorecards = await db.QualityScorecards
            .Where(x => x.TargetType == "supplier" && x.TargetRef == supplierRef)
            .ToListAsync(cancellationToken);

        foreach (var scorecard in scorecards)
        {
            await UpsertCalculatedMetricAsync(
                scorecard,
                "supplier-quality-issue-count",
                "Supplier quality issue count",
                "Open supplier-responsible quality issues for the target supplier.",
                "supplier",
                openIssueCount,
                "count",
                0,
                1,
                3,
                new[] { entity.SourceProduct ?? "assurarr", "supplyarr" },
                cancellationToken);
        }
    }

    private async Task RecalculateCustomerComplaintMetricsAsync(AssurArrCustomerComplaintQualityCase entity, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entity.CustomerRef))
        {
            return;
        }

        var customerRef = entity.CustomerRef.Trim();
        var openComplaintCount = await db.CustomerComplaintQualityCases.CountAsync(x =>
            x.CustomerRef == customerRef && x.Status != "closed" && x.Status != "canceled", cancellationToken);

        var scorecards = await db.QualityScorecards
            .Where(x => x.TargetType == "customer" && x.TargetRef == customerRef)
            .ToListAsync(cancellationToken);

        foreach (var scorecard in scorecards)
        {
            await UpsertCalculatedMetricAsync(
                scorecard,
                "customer-complaint-count",
                "Customer complaint count",
                "Open customer complaint quality cases for the target customer.",
                "customer",
                openComplaintCount,
                "count",
                0,
                1,
                3,
                new[] { entity.SourceProduct ?? "assurarr", "customarr" },
                cancellationToken);
        }
    }

    private async Task UpsertCalculatedMetricAsync(
        AssurArrQualityScorecard scorecard,
        string metricKey,
        string title,
        string description,
        string category,
        decimal value,
        string unit,
        decimal targetValue,
        decimal warningThreshold,
        decimal criticalThreshold,
        string[] sourceProductRefs,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var metric = await db.QualityMetrics.FirstOrDefaultAsync(x => x.ScorecardId == scorecard.Id && x.MetricKey == metricKey, cancellationToken);
        if (metric is null)
        {
            metric = new AssurArrQualityMetric
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                ScorecardId = scorecard.Id,
                MetricKey = metricKey,
                CreatedAt = now,
            };
            db.QualityMetrics.Add(metric);
        }

        metric.Title = title;
        metric.Description = description;
        metric.Category = category;
        metric.Value = value;
        metric.Numerator = value;
        metric.Denominator = 0;
        metric.Unit = unit;
        metric.TargetValue = targetValue;
        metric.WarningThreshold = warningThreshold;
        metric.CriticalThreshold = criticalThreshold;
        metric.Status = value <= targetValue ? "acceptable"
            : value >= criticalThreshold ? "critical"
            : value >= warningThreshold ? "warning"
            : "acceptable";
        metric.SourceProductRefs = sourceProductRefs;
        metric.UpdatedAt = now;

        if (!scorecard.MetricRefs.Contains(metricKey, StringComparer.OrdinalIgnoreCase))
        {
            scorecard.MetricRefs = scorecard.MetricRefs.Append(metricKey).ToArray();
            scorecard.UpdatedAt = now;
        }

        await AddTimelineAsync("scorecard", scorecard.Id, "assurarr.metric.calculated", metricKey, cancellationToken);
    }

    public async Task AddTimelineAsync(string subjectType, Guid subjectId, string eventType, string? details, CancellationToken cancellationToken = default)
    {
        db.TimelineEvents.Add(new AssurArrTimelineEvent
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            SubjectType = subjectType,
            SubjectId = subjectId,
            EventType = eventType,
            Details = details,
            OccurredAt = DateTimeOffset.UtcNow,
        });
        await Task.CompletedTask;
    }

    private Guid DefaultTenantId => db.CurrentTenantId;

    private static async Task<string> GenerateNumberAsync<TEntity>(
        string prefix,
        IQueryable<TEntity> set,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var count = await set.CountAsync(cancellationToken);
        return $"{prefix}-{count + 1:000000}";
    }

    private static void EnsureTransition(
        string currentStatus,
        string nextStatus,
        IReadOnlyDictionary<string, string[]> allowedTransitions,
        string label)
    {
        if (string.Equals(currentStatus, nextStatus, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!allowedTransitions.TryGetValue(currentStatus, out var transitions)
            || !transitions.Any(status => string.Equals(status, nextStatus, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Cannot move {label} from '{currentStatus}' to '{nextStatus}'.");
        }
    }

    private static string Normalize(string value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim().ToLowerInvariant().Replace(' ', '_');

    private static string? NormalizeNullable(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task EnsureScarReferenceExistsAsync(string scarRef, CancellationToken cancellationToken)
    {
        if (await db.SupplierCorrectiveActionRequests.AsNoTracking().AnyAsync(x => x.Number == scarRef, cancellationToken))
        {
            return;
        }

        throw new InvalidOperationException($"Referenced SCAR '{scarRef}' was not found.");
    }

    private async Task EnsureCapaReferenceExistsAsync(string capaRef, CancellationToken cancellationToken)
    {
        if (await db.Capas.AsNoTracking().AnyAsync(x => x.Number == capaRef, cancellationToken))
        {
            return;
        }

        throw new InvalidOperationException($"Referenced CAPA '{capaRef}' was not found.");
    }

    private async Task EnsureNonconformanceReferenceExistsAsync(string nonconformanceRef, CancellationToken cancellationToken)
    {
        if (await db.Nonconformances.AsNoTracking().AnyAsync(x => x.Number == nonconformanceRef, cancellationToken))
        {
            return;
        }

        throw new InvalidOperationException($"Referenced nonconformance '{nonconformanceRef}' was not found.");
    }

    private static AssurArrNonconformanceResponse ToNonconformanceResponse(AssurArrNonconformance entity, IReadOnlyList<string> eventLog) =>
        new(
            entity.Id,
            entity.Number,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Severity,
            entity.NonconformanceType,
            entity.Category,
            entity.SourceProduct,
            entity.SourceObjectRef,
            entity.DiscoveredAt,
            entity.DiscoveredByPersonId,
            entity.StaffArrSiteId,
            entity.StaffArrLocationId,
            entity.AffectedObjectRefs,
            entity.OwnerPersonId,
            entity.RecordRefs,
            entity.ContainmentRefs,
            entity.HoldRefs,
            entity.AffectedItemRefs,
            entity.AffectedAssetRefs,
            entity.AffectedOrderRefs,
            entity.AffectedSupplierRefs,
            entity.AffectedCustomerRefs,
            entity.AffectedShipmentRefs,
            entity.DispositionRefs,
            entity.CapaRefs,
            entity.ComplianceRefs,
            entity.FinancialImpactSnapshot,
            entity.AuditTrail,
            eventLog,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            entity.CustomerImpact,
            entity.SupplierImpact,
            entity.SafetyImpact,
            entity.ComplianceImpact,
            entity.RecurrenceFlag,
            entity.RepeatOfNonconformanceRef,
            entity.RootCauseRef,
            entity.BlockerRefs,
            entity.DueAt,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrQualityHoldResponse ToQualityHoldResponse(AssurArrQualityHold entity, IReadOnlyList<string> eventLog) =>
        new(
            entity.Id,
            entity.Number,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Severity,
            entity.HoldType,
            entity.HoldScope,
            entity.SourceNonconformanceRef,
            entity.SourceProduct,
            entity.SourceObjectRef,
            entity.StaffArrSiteId,
            entity.StaffArrLocationId,
            entity.AffectedObjectRefs,
            entity.OwnerPersonId,
            entity.RecordRefs,
            entity.AuditTrail,
            eventLog,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            entity.HoldReason,
            entity.ReleaseReason,
            entity.RejectionReason,
            entity.ConditionalReleaseTerms,
            entity.ReleaseRequirements,
            entity.ReleaseApprovalRefs,
            entity.QuantityHeld,
            entity.UnitOfMeasure,
            entity.LotNumber,
            entity.SerialNumber,
            entity.PlacedAt,
            entity.PlacedByPersonId,
            entity.ReleasedAt,
            entity.ReleasedByPersonId,
            entity.RejectedAt,
            entity.RejectedByPersonId,
            entity.ExpiresAt,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrCapaResponse ToCapaResponse(AssurArrCapa entity, IReadOnlyList<string> eventLog) =>
        new(
            entity.Id,
            entity.Number,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Severity,
            entity.CapaType,
            entity.SourceType,
            entity.SourceProduct,
            entity.SourceObjectRef,
            entity.AffectedObjectRefs,
            entity.OwnerPersonId,
            entity.StaffArrSiteId,
            entity.StaffArrLocationId,
            entity.SourceRefs,
            entity.RecordRefs,
            entity.ActionPlanRefs,
            entity.VerificationPlanRef,
            entity.RelatedCustomerComplaintRefs,
            entity.RelatedSupplierIssueRefs,
            entity.ComplianceRefs,
            entity.AuditTrail,
            eventLog,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.OpenedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            entity.SponsorPersonId,
            entity.RootCauseSummary,
            entity.DueAt,
            entity.RelatedNonconformanceRefs,
            entity.RelatedAuditFindingRefs,
            entity.EffectivenessVerificationRefs,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private async Task<IReadOnlyList<string>> GetEventLogAsync(string subjectType, Guid subjectId, CancellationToken cancellationToken = default) =>
        await db.TimelineEvents
            .AsNoTracking()
            .Where(x => x.SubjectType == subjectType && x.SubjectId == subjectId)
            .OrderBy(x => x.OccurredAt)
            .Select(x => x.EventType)
            .ToListAsync(cancellationToken);

    private async Task<AssurArrNonconformanceResponse> ToNonconformanceResponseAsync(AssurArrNonconformance entity, CancellationToken cancellationToken = default) =>
        ToNonconformanceResponse(entity, await GetEventLogAsync("nonconformance", entity.Id, cancellationToken));

    private async Task<AssurArrQualityHoldResponse> ToQualityHoldResponseAsync(AssurArrQualityHold entity, CancellationToken cancellationToken = default) =>
        ToQualityHoldResponse(entity, await GetEventLogAsync("hold", entity.Id, cancellationToken));

    private static string CreateAuditTrailEntry(string action, DateTimeOffset timestamp, string subject) =>
        $"{timestamp:O}|{action}|{subject}";

    private static string[] AppendAuditTrail(IEnumerable<string> currentTrail, string action, string subject, DateTimeOffset timestamp) =>
        [.. currentTrail, CreateAuditTrailEntry(action, timestamp, subject)];

    private async Task<AssurArrCapaResponse> ToCapaResponseAsync(AssurArrCapa entity, CancellationToken cancellationToken = default) =>
        ToCapaResponse(entity, await GetEventLogAsync("capa", entity.Id, cancellationToken));

    private static AssurArrEffectivenessVerificationResponse ToEffectivenessVerificationResponse(AssurArrEffectivenessVerification entity) =>
        new(
            entity.Id,
            entity.Number,
            entity.CapaId,
            entity.VerificationPlanId,
            entity.Status,
            entity.PerformedByPersonId,
            entity.PerformedAt,
            entity.ResultSummary,
            entity.EvidenceRecordRefs,
            entity.MetricResults,
            entity.RecurrenceFound,
            entity.FollowUpRequired,
            entity.ReopenedCapaRef,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrCapaActionResponse ToCapaActionResponse(AssurArrCapaAction entity) =>
        new(
            entity.Id,
            entity.Number,
            entity.CapaId,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.ActionType,
            entity.AssignedPersonId,
            entity.AssignedTeamRef,
            entity.SourceProductActionRef,
            entity.TargetProduct,
            entity.TargetObjectRef,
            entity.DueAt,
            entity.StartedAt,
            entity.CompletedAt,
            entity.CompletedByPersonId,
            entity.VerificationRequired,
            entity.VerifiedAt,
            entity.VerifiedByPersonId,
            entity.EvidenceRecordRefs,
            entity.BlockerRefs,
            entity.Notes,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrCapaActionBlockerResponse ToCapaActionBlockerResponse(AssurArrCapaActionBlocker entity) =>
        new(
            entity.Id,
            entity.Number,
            entity.CapaActionId,
            entity.BlockerType,
            entity.SourceProduct,
            entity.SourceObjectRef,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.CreatedAt,
            entity.ResolvedAt,
            entity.ResolvedByPersonId,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrVerificationPlanResponse ToVerificationPlanResponse(AssurArrVerificationPlan entity) =>
        new(
            entity.Id,
            entity.Number,
            entity.CapaId,
            entity.Title,
            entity.Description,
            entity.VerificationType,
            entity.SuccessCriteria,
            entity.SampleSize,
            entity.ObservationPeriodDays,
            entity.RequiredEvidenceTypes,
            entity.ResponsiblePersonId,
            entity.PlannedVerificationAt,
            entity.Status,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrQualityAuditResponse ToAuditResponse(AssurArrQualityAudit entity) =>
        new(
            entity.Id,
            entity.Number,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Severity,
            entity.AuditType,
            entity.AuditScope,
            entity.SourceProduct,
            entity.SourceObjectRef,
            entity.AffectedObjectRefs,
            entity.OwnerPersonId,
            entity.RecordRefs,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            entity.StandardRefs,
            entity.ComplianceRefs,
            entity.AuditorPersonIds,
            entity.LeadAuditorPersonId,
            entity.AuditeeRefs,
            entity.StaffArrSiteId,
            entity.StaffArrLocationId,
            entity.SupplierRef,
            entity.CustomerRef,
            entity.PlannedStartAt,
            entity.PlannedEndAt,
            entity.ActualStartAt,
            entity.ActualEndAt,
            entity.ChecklistRefs,
            entity.FindingRefs,
            entity.AuditTrail,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrQualityAuditChecklistResponse ToChecklistResponse(AssurArrQualityAuditChecklist entity) =>
        new(
            entity.Id,
            entity.Number,
            entity.AuditId,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.ItemRefs,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrQualityAuditChecklistItemResponse ToChecklistItemResponse(AssurArrQualityAuditChecklistItem entity) =>
        new(
            entity.Id,
            entity.Number,
            entity.ChecklistId,
            entity.Sequence,
            entity.Prompt,
            entity.HelpText,
            entity.RequirementRef,
            entity.ResponseType,
            entity.Required,
            entity.ResponseValue,
            entity.Result,
            entity.FindingCreated,
            entity.FindingRef,
            entity.EvidenceRecordRefs,
            entity.AnsweredAt,
            entity.AnsweredByPersonId,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrAuditFindingResponse ToFindingResponse(AssurArrAuditFinding entity) =>
        new(
            entity.Id,
            entity.Number,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Severity,
            entity.FindingType,
            entity.SourceProduct,
            entity.SourceObjectRef,
            entity.AffectedObjectRefs,
            entity.OwnerPersonId,
            entity.RecordRefs,
            entity.SourceRequirementRef,
            entity.EvidenceRecordRefs,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            entity.AuditRef,
            entity.NonconformanceRef,
            entity.CapaRef,
            entity.DueAt,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrRootCauseAnalysisResponse ToRootCauseAnalysisResponse(AssurArrRootCauseAnalysis entity) =>
        new(
            entity.Id,
            entity.Number,
            entity.NonconformanceId,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Method,
            entity.PrimaryCauseCategory,
            entity.SourceProduct,
            entity.SourceObjectRef,
            entity.AffectedObjectRefs,
            entity.OwnerPersonId,
            entity.RecordRefs,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.RootCauseSummary,
            entity.ContributingFactors,
            entity.AnalyzedByPersonId,
            entity.CompletedAt,
            entity.EvidenceRecordRefs,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrQualityStatusSnapshotResponse ToStatusSnapshotResponse(AssurArrQualityStatusSnapshot entity, IReadOnlyList<string> eventLog) =>
        new(
            entity.Id,
            entity.Number,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Severity,
            entity.SourceProduct,
            entity.SourceObjectRef,
            entity.AffectedObjectRefs,
            entity.OwnerPersonId,
            entity.RecordRefs,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            eventLog,
            entity.TargetProduct,
            entity.TargetObjectRef,
            entity.QualityStatus,
            entity.ActiveHoldRefs,
            entity.OpenNonconformanceRefs,
            entity.OpenCapaRefs,
            entity.OpenFindingRefs,
            entity.LastReviewedAt,
            entity.ReviewedByPersonId,
            entity.ExpiresAt,
            entity.Notes,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrQualityReviewResponse ToReviewResponse(AssurArrQualityReview entity, IReadOnlyList<string> eventLog) =>
        new(
            entity.Id,
            entity.Number,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Severity,
            entity.ReviewType,
            entity.SourceProduct,
            entity.SourceObjectRef,
            entity.AffectedObjectRefs,
            entity.OwnerPersonId,
            entity.RecordRefs,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            eventLog,
            entity.SourceReviewRef,
            entity.ReviewerPersonId,
            entity.RequestedAt,
            entity.DueAt,
            entity.DecisionAt,
            entity.DecisionReason,
            entity.RequiredEvidenceRefs,
            entity.SubmittedEvidenceRefs,
            entity.Notes,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrQualityReleaseResponse ToReleaseResponse(AssurArrQualityRelease entity, IReadOnlyList<string> eventLog) =>
        new(
            entity.Id,
            entity.Number,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Severity,
            entity.SourceProduct,
            entity.SourceObjectRef,
            entity.AffectedObjectRefs,
            entity.OwnerPersonId,
            entity.RecordRefs,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            eventLog,
            entity.HoldRef,
            entity.ReleaseType,
            entity.RequestedByPersonId,
            entity.RequestedAt,
            entity.ApprovedByPersonId,
            entity.ApprovedAt,
            entity.ExecutedAt,
            entity.Conditions,
            entity.ExpirationAt,
            entity.EvidenceRecordRefs,
            entity.Notes,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private async Task<AssurArrQualityReviewResponse> ToReviewResponseAsync(AssurArrQualityReview entity, CancellationToken cancellationToken = default) =>
        ToReviewResponse(entity, await GetEventLogAsync("review", entity.Id, cancellationToken));

    private async Task<AssurArrQualityReleaseResponse> ToReleaseResponseAsync(AssurArrQualityRelease entity, CancellationToken cancellationToken = default) =>
        ToReleaseResponse(entity, await GetEventLogAsync("release", entity.Id, cancellationToken));

    private static AssurArrContainmentActionResponse ToContainmentActionResponse(AssurArrContainmentAction entity) =>
        new(
            entity.Id,
            entity.Number,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Severity,
            entity.ActionType,
            entity.SourceProduct,
            entity.SourceObjectRef,
            entity.AffectedObjectRefs,
            entity.NonconformanceRef,
            entity.AssignedPersonId,
            entity.AssignedTeamRef,
            entity.SourceProductActionRef,
            entity.DueAt,
            entity.StartedAt,
            entity.CompletedAt,
            entity.CompletedByPersonId,
            entity.VerificationRequired,
            entity.VerifiedByPersonId,
            entity.VerifiedAt,
            entity.EvidenceRecordRefs,
            entity.Notes,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrDispositionResponse ToDispositionResponse(AssurArrDisposition entity) =>
        new(
            entity.Id,
            entity.Number,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Severity,
            entity.DispositionType,
            entity.SourceProduct,
            entity.SourceObjectRef,
            entity.AffectedObjectRefs,
            entity.NonconformanceRef,
            entity.DecisionByPersonId,
            entity.DecisionAt,
            entity.ApprovedByPersonId,
            entity.ApprovedAt,
            entity.Rationale,
            entity.RequiredActions,
            entity.ExecutionProduct,
            entity.ExecutionObjectRef,
            entity.EvidenceRecordRefs,
            entity.Notes,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrSupplierQualityIssueResponse ToSupplierQualityIssueResponse(AssurArrSupplierQualityIssue entity) =>
        new(
            entity.Id,
            entity.Number,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Severity,
            entity.IssueType,
            entity.SourceProduct,
            entity.SourceObjectRef,
            [
                .. entity.AffectedReceiptRefs,
                .. entity.AffectedPurchaseOrderRefs,
                .. entity.AffectedItemRefs,
            ],
            entity.AffectedReceiptRefs,
            entity.AffectedPurchaseOrderRefs,
            entity.AffectedItemRefs,
            entity.SupplierRef,
            entity.NonconformanceRef,
            entity.ScarRef,
            entity.HoldRefs,
            entity.RecordRefs,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            entity.OwnerPersonId,
            entity.OpenedAt,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrSupplierCorrectiveActionRequestResponse ToScarResponse(AssurArrSupplierCorrectiveActionRequest entity) =>
        new(
            entity.Id,
            entity.Number,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Severity,
            entity.SourceProduct,
            entity.SourceObjectRef,
            entity.AffectedObjectRefs,
            entity.SupplierRef,
            entity.SourceNonconformanceRef,
            entity.SourceCapaRef,
            entity.RequestedByPersonId,
            entity.RequestedAt,
            entity.SupplierDueAt,
            entity.SupplierResponseRecordRefs,
            entity.ReviewPersonId,
            entity.ReviewedAt,
            entity.ReviewDecision,
            entity.FollowUpCapaRef,
            entity.RecordRefs,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            entity.OwnerPersonId,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrCustomerComplaintQualityCaseResponse ToCustomerComplaintQualityCaseResponse(AssurArrCustomerComplaintQualityCase entity) =>
        new(
            entity.Id,
            entity.Number,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Severity,
            entity.ComplaintType,
            entity.SourceProduct,
            entity.SourceObjectRef,
            [
                .. entity.AffectedOrderRefs,
                .. entity.AffectedShipmentRefs,
                .. entity.AffectedItemRefs,
                .. entity.AffectedAssetRefs,
            ],
            entity.AffectedOrderRefs,
            entity.AffectedShipmentRefs,
            entity.AffectedItemRefs,
            entity.AffectedAssetRefs,
            entity.CustomerRef,
            entity.CustomerContactSnapshot,
            entity.CustomerLocationRef,
            entity.NonconformanceRef,
            entity.HoldRefs,
            entity.CapaRefs,
            entity.CustomerResponseRecordRefs,
            entity.RecordRefs,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            entity.OwnerPersonId,
            entity.ReceivedAt,
            entity.ReceivedByPersonId,
            entity.CustomerResponseDueAt,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrQualityScorecardResponse ToScorecardResponse(AssurArrQualityScorecard entity, IReadOnlyList<string> eventLog) =>
        new(
            entity.Id,
            entity.Number,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Severity,
            entity.SourceProduct,
            entity.SourceObjectRef,
            entity.AffectedObjectRefs,
            entity.OwnerPersonId,
            entity.RecordRefs,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            eventLog,
            entity.TargetType,
            entity.TargetRef,
            entity.PeriodStart,
            entity.PeriodEnd,
            entity.OverallScore,
            entity.QualityStatus,
            entity.Trend,
            entity.GeneratedAt,
            entity.GeneratedBy,
            entity.ReviewedByPersonId,
            entity.ReviewedAt,
            entity.MetricRefs,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrQualityMetricResponse ToMetricResponse(AssurArrQualityMetric entity) =>
        new(
            entity.Id,
            entity.ScorecardId,
            entity.MetricKey,
            entity.Title,
            entity.Description,
            entity.Category,
            entity.Value,
            entity.Numerator,
            entity.Denominator,
            entity.Unit,
            entity.TargetValue,
            entity.WarningThreshold,
            entity.CriticalThreshold,
            entity.Status,
            entity.SourceProductRefs,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private static AssurArrQualityRiskProfileResponse ToRiskProfileResponse(AssurArrQualityRiskProfile entity, IReadOnlyList<string> eventLog) =>
        new(
            entity.Id,
            entity.TargetType,
            entity.TargetRef,
            entity.RiskLevel,
            entity.RiskFactors,
            entity.OpenIssueCount,
            entity.RepeatIssueCount,
            entity.CriticalIssueCount,
            entity.LastIncidentAt,
            entity.MitigationActions,
            entity.ReviewedAt,
            entity.ReviewedByPersonId,
            eventLog,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId);

    private async Task<AssurArrQualityStatusSnapshotResponse> ToStatusSnapshotResponseAsync(AssurArrQualityStatusSnapshot entity, CancellationToken cancellationToken = default) =>
        ToStatusSnapshotResponse(entity, await GetEventLogAsync("status", entity.Id, cancellationToken));

    private async Task<AssurArrQualityScorecardResponse> ToScorecardResponseAsync(AssurArrQualityScorecard entity, CancellationToken cancellationToken = default) =>
        ToScorecardResponse(entity, await GetEventLogAsync("scorecard", entity.Id, cancellationToken));

    private async Task<AssurArrQualityRiskProfileResponse> ToRiskProfileResponseAsync(AssurArrQualityRiskProfile entity, CancellationToken cancellationToken = default) =>
        ToRiskProfileResponse(entity, await GetEventLogAsync("risk-profile", entity.Id, cancellationToken));
}
