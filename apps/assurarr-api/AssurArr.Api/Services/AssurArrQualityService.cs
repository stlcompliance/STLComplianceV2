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

    public async Task EnsureDemoDataAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Nonconformances.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var tenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        db.Nonconformances.Add(new AssurArrNonconformance
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Number = "NCR-000001",
            Title = "Missing incoming inspection tag",
            Description = "A receiving lot was found without the required inspection tag and must be investigated.",
            Severity = "high",
            Status = "containment",
            SourceProduct = "loadarr",
            SourceObjectRef = "RECEIPT-RR-24018",
            AffectedObjectRefs = ["loadarr:receiving:RR-24018"],
            OwnerPersonId = null,
            RecordRefs = ["recordarr:doc:inspection-photo-1"],
            CreatedAt = now,
            UpdatedAt = now,
            DueAt = now.AddDays(3),
            NonconformanceType = "receiving",
            Category = "failed_inspection",
            CustomerImpact = "Delayed release to the warehouse floor",
            SupplierImpact = "Supplier documentation review required",
            SafetyImpact = "none",
            ComplianceImpact = "Missing evidence for release",
            RecurrenceFlag = false,
        });

        db.QualityHolds.Add(new AssurArrQualityHold
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Number = "HOLD-000001",
            Title = "Quarantine incoming lot",
            Description = "Hold the receiving lot until inspection evidence is complete.",
            Severity = "high",
            Status = "active",
            SourceProduct = "loadarr",
            SourceObjectRef = "RECEIPT-RR-24018",
            AffectedObjectRefs = ["loadarr:inventory:LOT-991"],
            RecordRefs = ["recordarr:doc:inspection-photo-1"],
            CreatedAt = now,
            UpdatedAt = now,
            HoldType = "inventory",
            HoldScope = "full",
            HoldReason = "Inspection evidence missing",
            QuantityHeld = 14,
            UnitOfMeasure = "each",
            PlacedAt = now,
        });

        db.Capas.Add(new AssurArrCapa
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Number = "CAPA-000001",
            Title = "Standardize incoming inspection release steps",
            Description = "Create a standard containment and inspection release checklist for incoming quality holds.",
            Severity = "moderate",
            Status = "action_plan",
            SourceProduct = "assurarr",
            SourceObjectRef = "NCR-000001",
            AffectedObjectRefs = ["loadarr:receiving:RR-24018"],
            RecordRefs = ["recordarr:doc:capa-plan-1"],
            CreatedAt = now,
            UpdatedAt = now,
            CapaType = "corrective_and_preventive",
            SourceType = "nonconformance",
            SponsorPersonId = null,
            RootCauseSummary = "Release steps were not visible in the receiving workflow.",
            DueAt = now.AddDays(14),
            RelatedNonconformanceRefs = ["NCR-000001"],
        });

        db.QualityAudits.Add(new AssurArrQualityAudit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Number = "AUD-000001",
            Title = "Monthly receiving quality audit",
            Description = "Review incoming inspection evidence and quarantine practice at the north yard.",
            Severity = "moderate",
            Status = "findings_review",
            SourceProduct = "assurarr",
            SourceObjectRef = "workflow:audits:monthly-receiving",
            AffectedObjectRefs = ["loadarr:location:north-yard"],
            RecordRefs = ["recordarr:doc:audit-checklist-1"],
            CreatedAt = now,
            UpdatedAt = now,
            AuditType = "process",
            AuditScope = "Receiving, inspection, and release evidence",
            AuditorPersonIds = ["person:auditor-1"],
            LeadAuditorPersonId = null,
            StaffArrSiteId = null,
            StaffArrLocationId = null,
            PlannedStartAt = now.AddDays(-1),
            PlannedEndAt = now,
        });

        db.AuditFindings.Add(new AssurArrAuditFinding
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Number = "FIND-000001",
            Title = "Inspection checklist missing release signature",
            Description = "The inspector completed the checklist but the release signature line remained blank.",
            Severity = "high",
            Status = "nonconformance_created",
            SourceProduct = "assurarr",
            SourceObjectRef = "AUD-000001",
            AffectedObjectRefs = ["loadarr:receiving:RR-24018"],
            RecordRefs = ["recordarr:doc:audit-photo-1"],
            CreatedAt = now,
            UpdatedAt = now,
            FindingType = "major_nonconformance",
            AuditRef = "AUD-000001",
            NonconformanceRef = "NCR-000001",
            DueAt = now.AddDays(10),
        });

        db.QualityStatusSnapshots.Add(new AssurArrQualityStatusSnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Number = "QS-000001",
            Title = "LoadArr inventory quality status",
            Description = "Current quality posture for the receiving lot and related inventory.",
            Severity = "moderate",
            Status = "under_review",
            SourceProduct = "assurarr",
            SourceObjectRef = "loadarr:inventory:LOT-991",
            AffectedObjectRefs = ["loadarr:inventory:LOT-991"],
            RecordRefs = ["recordarr:doc:inspection-photo-1"],
            CreatedAt = now,
            UpdatedAt = now,
            TargetProduct = "loadarr",
            TargetObjectRef = "loadarr:inventory:LOT-991",
            QualityStatus = "on_hold",
            ActiveHoldRefs = ["HOLD-000001"],
            OpenNonconformanceRefs = ["NCR-000001"],
            OpenCapaRefs = ["CAPA-000001"],
            OpenFindingRefs = ["FIND-000001"],
            LastReviewedAt = now,
        });

        db.QualityScorecards.Add(new AssurArrQualityScorecard
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Number = "SCORE-000001",
            Title = "Receiving quality scorecard",
            Description = "Summary scorecard for incoming inspection health.",
            Severity = "moderate",
            Status = "active",
            SourceProduct = "assurarr",
            SourceObjectRef = "loadarr:site:north-yard",
            AffectedObjectRefs = ["loadarr:site:north-yard"],
            RecordRefs = ["recordarr:doc:scorecard-1"],
            CreatedAt = now,
            UpdatedAt = now,
            TargetType = "site",
            TargetRef = "loadarr:site:north-yard",
            PeriodStart = now.AddDays(-30),
            PeriodEnd = now,
            OverallScore = 84,
            QualityStatus = "warning",
            Trend = "worsening",
            GeneratedAt = now,
            GeneratedBy = "system",
            ReviewedByPersonId = null,
            ReviewedAt = null,
            MetricRefs = ["metric:open-nc-count", "metric:hold-aging"],
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AssurArrDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var openNcCount = await db.Nonconformances.CountAsync(x => x.Status != "closed" && x.Status != "canceled", cancellationToken);
        var activeHoldCount = await db.QualityHolds.CountAsync(x => x.Status == "active" || x.Status == "release_pending", cancellationToken);
        var openCapaCount = await db.Capas.CountAsync(x => x.Status != "closed" && x.Status != "canceled", cancellationToken);
        var openAuditCount = await db.QualityAudits.CountAsync(x => x.Status != "closed" && x.Status != "canceled", cancellationToken);
        var openFindingCount = await db.AuditFindings.CountAsync(x => x.Status != "closed" && x.Status != "canceled", cancellationToken);
        var openScorecards = await db.QualityScorecards.CountAsync(x => x.Status == "active", cancellationToken);
        var openStatusSnapshots = await db.QualityStatusSnapshots.CountAsync(x => x.Status != "unknown", cancellationToken);

        var cards = new[]
        {
            new AssurArrDashboardCardResponse("nonconformances", "Open nonconformances", "Cases requiring investigation, containment, or closure.", openNcCount, "danger"),
            new AssurArrDashboardCardResponse("holds", "Active holds", "Business decisions that are currently blocking target objects.", activeHoldCount, "warning"),
            new AssurArrDashboardCardResponse("capa", "Open CAPA", "Corrective and preventive actions in progress.", openCapaCount, "accent"),
            new AssurArrDashboardCardResponse("audits", "Open audits", "Quality reviews and audits awaiting closeout.", openAuditCount, "info"),
            new AssurArrDashboardCardResponse("findings", "Open findings", "Issues or opportunities captured during audits.", openFindingCount, "soft"),
            new AssurArrDashboardCardResponse("status", "Status snapshots", "Current quality state published to other products.", openStatusSnapshots, "neutral"),
            new AssurArrDashboardCardResponse("scorecards", "Scorecards", "Active quality scorecards and trend summaries.", openScorecards, "accent"),
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

        return entities.Select(ToNonconformanceResponse).ToList();
    }

    public async Task<AssurArrNonconformanceResponse?> GetNonconformanceAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await db.Nonconformances.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)) is { } entity
            ? ToNonconformanceResponse(entity)
            : null;

    public async Task<AssurArrNonconformanceResponse> CreateNonconformanceAsync(CreateAssurArrNonconformanceRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrNonconformance
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("NCR", db.Nonconformances, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "moderate"),
            Status = "open",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            OwnerPersonId = request.OwnerPersonId,
            RecordRefs = [],
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
            DueAt = request.DueAt,
        };

        db.Nonconformances.Add(entity);
        await AddTimelineAsync("nonconformance", entity.Id, "assurarr.nonconformance.created", entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToNonconformanceResponse(entity);
    }

    public async Task<AssurArrNonconformanceResponse> UpdateNonconformanceStatusAsync(Guid id, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.Nonconformances.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Nonconformance '{id}' was not found.");
        EnsureTransition(entity.Status, request.Status, NonconformanceTransitions, "nonconformance");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
        }
        else if (!string.IsNullOrWhiteSpace(request.ClosureSummary))
        {
            entity.ClosureSummary = request.ClosureSummary;
        }
        await AddTimelineAsync("nonconformance", entity.Id, "assurarr.nonconformance.status_changed", entity.Status, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToNonconformanceResponse(entity);
    }

    public async Task<List<AssurArrQualityHoldResponse>> ListQualityHoldsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.QualityHolds
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToQualityHoldResponse).ToList();
    }

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
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            OwnerPersonId = request.OwnerPersonId,
            RecordRefs = [],
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
        };

        db.QualityHolds.Add(entity);
        await AddTimelineAsync("hold", entity.Id, "assurarr.hold.created", entity.Title, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToQualityHoldResponse(entity);
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
        }
        else if (string.Equals(entity.Status, "rejected", StringComparison.OrdinalIgnoreCase))
        {
            entity.RejectionReason = request.ClosureSummary ?? entity.RejectionReason;
        }
        else if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
        }
        await AddTimelineAsync("hold", entity.Id, "assurarr.hold.status_changed", entity.Status, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToQualityHoldResponse(entity);
    }

    public async Task<List<AssurArrCapaResponse>> ListCapasAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.Capas
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToCapaResponse).ToList();
    }

    public async Task<AssurArrCapaResponse> CreateCapaAsync(CreateAssurArrCapaRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new AssurArrCapa
        {
            Id = Guid.NewGuid(),
            TenantId = DefaultTenantId,
            Number = await GenerateNumberAsync("CAPA", db.Capas, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Severity = Normalize(request.Severity, "moderate"),
            Status = "open",
            SourceProduct = NormalizeNullable(request.SourceProduct),
            SourceObjectRef = NormalizeNullable(request.SourceObjectRef),
            AffectedObjectRefs = request.AffectedObjectRefs ?? [],
            OwnerPersonId = request.OwnerPersonId,
            RecordRefs = [],
            CreatedAt = now,
            UpdatedAt = now,
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
        return ToCapaResponse(entity);
    }

    public async Task<AssurArrCapaResponse> UpdateCapaStatusAsync(Guid id, UpdateAssurArrStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.Capas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"CAPA '{id}' was not found.");
        EnsureTransition(entity.Status, request.Status, CapaTransitions, "CAPA");
        entity.Status = Normalize(request.Status, entity.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
        }
        await AddTimelineAsync("capa", entity.Id, "assurarr.capa.status_changed", entity.Status, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToCapaResponse(entity);
    }

    public async Task<List<AssurArrQualityAuditResponse>> ListAuditsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.QualityAudits
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToAuditResponse).ToList();
    }

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
            AuditorPersonIds = request.AuditorPersonIds ?? [],
            LeadAuditorPersonId = request.LeadAuditorPersonId,
            StaffArrSiteId = request.StaffArrSiteId,
            StaffArrLocationId = request.StaffArrLocationId,
            SupplierRef = NormalizeNullable(request.SupplierRef),
            CustomerRef = NormalizeNullable(request.CustomerRef),
            PlannedStartAt = request.PlannedStartAt,
            PlannedEndAt = request.PlannedEndAt,
            ChecklistRefs = request.ChecklistRefs ?? [],
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
        if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
        }
        await AddTimelineAsync("audit", entity.Id, "assurarr.audit.status_changed", entity.Status, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToAuditResponse(entity);
    }

    public async Task<List<AssurArrAuditFindingResponse>> ListFindingsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.AuditFindings
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToFindingResponse).ToList();
    }

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
            RecordRefs = [],
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
        if (string.Equals(entity.Status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt = entity.UpdatedAt;
            entity.ClosureSummary = request.ClosureSummary ?? entity.ClosureSummary;
        }
        await AddTimelineAsync("finding", entity.Id, "assurarr.finding.status_changed", entity.Status, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToFindingResponse(entity);
    }

    public async Task<List<AssurArrQualityStatusSnapshotResponse>> ListStatusSnapshotsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.QualityStatusSnapshots
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToStatusSnapshotResponse).ToList();
    }

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
        };

        db.QualityStatusSnapshots.Add(entity);
        await AddTimelineAsync("status", entity.Id, "assurarr.quality_status.published", entity.TargetObjectRef, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToStatusSnapshotResponse(entity);
    }

    public async Task<List<AssurArrQualityScorecardResponse>> ListScorecardsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.QualityScorecards
            .AsNoTracking()
            .OrderByDescending(x => x.GeneratedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToScorecardResponse).ToList();
    }

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
        return ToScorecardResponse(entity);
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

    private static Guid DefaultTenantId => Guid.Parse("22222222-2222-2222-2222-222222222222");

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

    private static AssurArrNonconformanceResponse ToNonconformanceResponse(AssurArrNonconformance entity) =>
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
            entity.AffectedObjectRefs,
            entity.OwnerPersonId,
            entity.RecordRefs,
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
            entity.DueAt);

    private static AssurArrQualityHoldResponse ToQualityHoldResponse(AssurArrQualityHold entity) =>
        new(
            entity.Id,
            entity.Number,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Severity,
            entity.HoldType,
            entity.HoldScope,
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
            entity.HoldReason,
            entity.ReleaseReason,
            entity.RejectionReason,
            entity.ConditionalReleaseTerms,
            entity.QuantityHeld,
            entity.UnitOfMeasure,
            entity.LotNumber,
            entity.SerialNumber,
            entity.PlacedAt,
            entity.PlacedByPersonId,
            entity.ReleasedAt,
            entity.ReleasedByPersonId,
            entity.ExpiresAt);

    private static AssurArrCapaResponse ToCapaResponse(AssurArrCapa entity) =>
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
            entity.RecordRefs,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            entity.SponsorPersonId,
            entity.RootCauseSummary,
            entity.DueAt,
            entity.RelatedNonconformanceRefs,
            entity.RelatedAuditFindingRefs);

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
            entity.AuditorPersonIds,
            entity.LeadAuditorPersonId,
            entity.StaffArrSiteId,
            entity.StaffArrLocationId,
            entity.SupplierRef,
            entity.CustomerRef,
            entity.PlannedStartAt,
            entity.PlannedEndAt,
            entity.ActualStartAt,
            entity.ActualEndAt,
            entity.ChecklistRefs,
            entity.FindingRefs);

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
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ClosedAt,
            entity.ClosedByPersonId,
            entity.ClosureSummary,
            entity.AuditRef,
            entity.NonconformanceRef,
            entity.CapaRef,
            entity.DueAt);

    private static AssurArrQualityStatusSnapshotResponse ToStatusSnapshotResponse(AssurArrQualityStatusSnapshot entity) =>
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
            entity.TargetProduct,
            entity.TargetObjectRef,
            entity.QualityStatus,
            entity.ActiveHoldRefs,
            entity.OpenNonconformanceRefs,
            entity.OpenCapaRefs,
            entity.OpenFindingRefs,
            entity.LastReviewedAt,
            entity.ReviewedByPersonId,
            entity.ExpiresAt);

    private static AssurArrQualityScorecardResponse ToScorecardResponse(AssurArrQualityScorecard entity) =>
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
            entity.MetricRefs);
}
