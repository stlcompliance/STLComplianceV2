using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class PmProgramService(
    MaintainArrDbContext db,
    AssetTypeService assetTypeService,
    AssetService assetService,
    IMaintainArrAuditService audit)
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> EmptyFieldLookup =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        PmProgramStatuses.Draft,
        PmProgramStatuses.Active,
        PmProgramStatuses.Paused,
        PmProgramStatuses.Retired,
        PmProgramStatuses.Inactive
    };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };

    public async Task<IReadOnlyList<PmProgramSummaryResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var programs = await db.PmPrograms
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Include(x => x.AssetType)
            .Include(x => x.Asset)
            .Include(x => x.InspectionTemplate)
            .Include(x => x.ProgramSchedules)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.ProgramKey)
            .ToListAsync(cancellationToken);

        return programs.Select(MapSummary).ToList();
    }

    public async Task<PmProgramDetailResponse> GetAsync(
        Guid tenantId,
        Guid pmProgramId,
        CancellationToken cancellationToken = default)
    {
        var program = await LoadProgramAsync(tenantId, pmProgramId, cancellationToken);
        return MapDetail(program);
    }

    public async Task<PmProgramDetailResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreatePmProgramRequest request,
        string? actorPersonId = null,
        CancellationToken cancellationToken = default)
    {
        var programKey = NormalizeProgramKey(request.ProgramKey);
        var name = NormalizeName(request.Name);
        var description = NormalizeDescription(request.Description);
        var richProgram = IsRichProgramRequest(request);
        var scopeType = NormalizeScopeType(request.ScopeType);
        var categoryKey = NormalizeCatalogKey(request.CategoryKey, "standard_fleet", "category", richProgram);
        var workTypeKey = NormalizeCatalogKey(request.WorkTypeKey, "preventive", "work_type", richProgram);
        var priorityKey = NormalizeCatalogKey(request.PriorityKey, "normal", "priority", richProgram);
        var owningSiteRef = NormalizeReference(request.OwningSiteRef);
        var owningTeamRef = NormalizeReference(request.OwningTeamRef);
        var owningDepartmentRef = NormalizeReference(request.OwningDepartmentRef);
        var ownerPersonId = NormalizePersonReference(request.OwnerPersonId);
        var ownerRoleKey = NormalizeReference(request.OwnerRoleKey);
        var tags = NormalizeTags(request.Tags);
        var (assetTypeId, assetId) = await ResolveScopeAsync(
            tenantId,
            scopeType,
            request.AssetTypeId,
            request.AssetId,
            cancellationToken);
        var defaultWorkOrderTemplateRef = NormalizeTemplateRef(request.DefaultWorkOrderTemplateRef);
        var inspectionTemplateId = request.InspectionTemplateId
            ?? request.InspectionDefinition?.InspectionTemplateId;
        var inspectionTemplate = await ResolveInspectionTemplateAsync(
            tenantId,
            scopeType,
            assetTypeId,
            assetId,
            !richProgram && request.AutoGenerateInspection,
            inspectionTemplateId,
            cancellationToken);

        var exists = await db.PmPrograms.AnyAsync(
            x => x.TenantId == tenantId && x.ProgramKey == programKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "pm_program.duplicate_key",
                "A PM program with this key already exists.",
                409);
        }

        var scheduleIds = request.PmScheduleIds ?? [];
        var schedules = scheduleIds.Count > 0
            ? await LoadSchedulesForScopeAsync(
                tenantId,
                scopeType,
                assetTypeId,
                assetId,
                scheduleIds,
                excludeProgramId: null,
                cancellationToken)
            : [];

        var now = DateTimeOffset.UtcNow;
        var program = new PmProgram
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProgramKey = programKey,
            Name = name,
            Description = description,
            ScopeType = scopeType,
            AssetTypeId = assetTypeId,
            AssetId = assetId,
            CategoryKey = categoryKey,
            WorkTypeKey = workTypeKey,
            PriorityKey = priorityKey,
            OwningSiteRef = owningSiteRef,
            OwningTeamRef = owningTeamRef,
            OwningDepartmentRef = owningDepartmentRef,
            OwnerPersonId = ownerPersonId,
            OwnerRoleKey = ownerRoleKey,
            TagsJson = SerializeJson(tags),
            ScopeDefinitionJson = SerializeJson(request.ScopeDefinition),
            DueTriggerDefinitionJson = SerializeJson(request.DueDefinition),
            WorkPackageDefinitionJson = SerializeJson(request.WorkPackageDefinition),
            InspectionDefinitionJson = SerializeJson(request.InspectionDefinition),
            ComplianceDefinitionJson = SerializeJson(request.ComplianceDefinition),
            AutomationDefinitionJson = SerializeJson(request.AutomationDefinition),
            AutoGenerateWorkOrder = request.AutoGenerateWorkOrder,
            DefaultWorkOrderTemplateRef = defaultWorkOrderTemplateRef,
            AutoGenerateInspection = request.AutoGenerateInspection,
            InspectionTemplateId = inspectionTemplate?.Id ?? inspectionTemplateId,
            Status = PmProgramStatuses.Draft,
            CreatedByPersonId = ownerPersonId ?? actorPersonId,
            UpdatedByPersonId = ownerPersonId ?? actorPersonId,
            CreatedAt = now,
            UpdatedAt = now,
            ProgramSchedules = schedules
                .Select((schedule, index) => new PmProgramSchedule
                {
                    PmProgramId = default,
                    PmScheduleId = schedule.Id,
                    SortOrder = index
                })
                .ToList()
        };

        foreach (var link in program.ProgramSchedules)
        {
            link.PmProgramId = program.Id;
        }

        db.PmPrograms.Add(program);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "pm_program.create",
            tenantId,
            actorUserId,
            actorPersonId: actorPersonId,
            targetType: "pm_program",
            targetId: program.Id.ToString(),
            result: "Succeeded",
            cancellationToken: cancellationToken);

        return MapDetail(await LoadProgramAsync(tenantId, program.Id, cancellationToken));
    }

    public async Task<PmProgramDetailResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid pmProgramId,
        UpdatePmProgramRequest request,
        string? actorPersonId = null,
        CancellationToken cancellationToken = default)
    {
        var program = await LoadProgramAsync(tenantId, pmProgramId, cancellationToken, tracking: true);
        var status = NormalizeStatus(request.Status);
        var defaultWorkOrderTemplateRef = NormalizeTemplateRef(request.DefaultWorkOrderTemplateRef);
        var inspectionTemplate = await ResolveInspectionTemplateAsync(
            tenantId,
            program.ScopeType,
            program.AssetTypeId,
            program.AssetId,
            request.AutoGenerateInspection,
            request.InspectionTemplateId,
            cancellationToken);

        if (string.Equals(status, PmProgramStatuses.Active, StringComparison.OrdinalIgnoreCase)
            && program.ProgramSchedules.Count == 0
            && !string.Equals(program.ScopeType, PmProgramScopeTypes.Custom, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "pm_program.no_schedules",
                "At least one PM schedule must be assigned before activating a program.",
                400);
        }

        program.Name = NormalizeName(request.Name);
        program.Description = NormalizeDescription(request.Description);
        program.AutoGenerateWorkOrder = request.AutoGenerateWorkOrder;
        program.DefaultWorkOrderTemplateRef = defaultWorkOrderTemplateRef;
        program.AutoGenerateInspection = request.AutoGenerateInspection;
        program.InspectionTemplateId = inspectionTemplate?.Id;
        if (request.CategoryKey is not null)
        {
            program.CategoryKey = NormalizeCatalogKey(request.CategoryKey, program.CategoryKey ?? "standard_fleet", "category", false);
        }

        if (request.WorkTypeKey is not null)
        {
            program.WorkTypeKey = NormalizeCatalogKey(request.WorkTypeKey, program.WorkTypeKey ?? "preventive", "work_type", false);
        }

        if (request.PriorityKey is not null)
        {
            program.PriorityKey = NormalizeCatalogKey(request.PriorityKey, program.PriorityKey ?? "normal", "priority", false);
        }

        if (request.OwningSiteRef is not null)
        {
            program.OwningSiteRef = NormalizeReference(request.OwningSiteRef);
        }

        if (request.OwningTeamRef is not null)
        {
            program.OwningTeamRef = NormalizeReference(request.OwningTeamRef);
        }

        if (request.OwningDepartmentRef is not null)
        {
            program.OwningDepartmentRef = NormalizeReference(request.OwningDepartmentRef);
        }

        if (request.OwnerPersonId is not null)
        {
            program.OwnerPersonId = NormalizePersonReference(request.OwnerPersonId);
        }

        if (request.OwnerRoleKey is not null)
        {
            program.OwnerRoleKey = NormalizeReference(request.OwnerRoleKey);
        }

        if (request.Tags is not null)
        {
            program.TagsJson = SerializeJson(NormalizeTags(request.Tags));
        }

        if (request.ScopeDefinition is not null)
        {
            program.ScopeDefinitionJson = SerializeJson(request.ScopeDefinition);
        }

        if (request.DueDefinition is not null)
        {
            program.DueTriggerDefinitionJson = SerializeJson(request.DueDefinition);
        }

        if (request.WorkPackageDefinition is not null)
        {
            program.WorkPackageDefinitionJson = SerializeJson(request.WorkPackageDefinition);
        }

        if (request.InspectionDefinition is not null)
        {
            program.InspectionDefinitionJson = SerializeJson(request.InspectionDefinition);
        }

        if (request.ComplianceDefinition is not null)
        {
            program.ComplianceDefinitionJson = SerializeJson(request.ComplianceDefinition);
        }

        if (request.AutomationDefinition is not null)
        {
            program.AutomationDefinitionJson = SerializeJson(request.AutomationDefinition);
        }

        program.Status = status;
        program.UpdatedAt = DateTimeOffset.UtcNow;
        program.UpdatedByPersonId = NormalizePersonReference(actorPersonId) ?? program.UpdatedByPersonId;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            status == PmProgramStatuses.Active ? "pm_program.activate" : "pm_program.update",
            tenantId,
            actorUserId,
            actorPersonId: actorPersonId,
            targetType: "pm_program",
            targetId: program.Id.ToString(),
            result: "Succeeded",
            cancellationToken: cancellationToken);

        return MapDetail(await LoadProgramAsync(tenantId, program.Id, cancellationToken));
    }

    public async Task<PmProgramDetailResponse> UpdateStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid pmProgramId,
        UpdatePmProgramStatusRequest request,
        string? actorPersonId = null,
        CancellationToken cancellationToken = default)
    {
        var program = await LoadProgramAsync(tenantId, pmProgramId, cancellationToken, tracking: true);
        var status = NormalizeStatus(request.Status);

        if (string.Equals(status, PmProgramStatuses.Active, StringComparison.OrdinalIgnoreCase)
            && program.ProgramSchedules.Count == 0
            && !string.Equals(program.ScopeType, PmProgramScopeTypes.Custom, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "pm_program.no_schedules",
                "At least one PM schedule must be assigned before activating a program.",
                400);
        }

        program.Status = status;
        program.UpdatedAt = DateTimeOffset.UtcNow;
        var normalizedActorPersonId = NormalizePersonReference(actorPersonId);
        if (string.Equals(status, PmProgramStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            program.ActivatedAt = program.UpdatedAt;
            program.ActivatedByPersonId = normalizedActorPersonId;
        }
        else if (string.Equals(status, PmProgramStatuses.Paused, StringComparison.OrdinalIgnoreCase))
        {
            program.PausedAt = program.UpdatedAt;
            program.PausedByPersonId = normalizedActorPersonId;
        }
        else if (string.Equals(status, PmProgramStatuses.Retired, StringComparison.OrdinalIgnoreCase))
        {
            program.RetiredAt = program.UpdatedAt;
            program.RetiredByPersonId = normalizedActorPersonId;
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            status == PmProgramStatuses.Active ? "pm_program.activate" : "pm_program.status.update",
            tenantId,
            actorUserId,
            actorPersonId: actorPersonId,
            targetType: "pm_program",
            targetId: program.Id.ToString(),
            result: "Succeeded",
            cancellationToken: cancellationToken);

        return MapDetail(await LoadProgramAsync(tenantId, program.Id, cancellationToken));
    }

    public async Task<PmProgramDetailResponse> ReplaceSchedulesAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid pmProgramId,
        ReplacePmProgramSchedulesRequest request,
        string? actorPersonId = null,
        CancellationToken cancellationToken = default)
    {
        var program = await LoadProgramAsync(tenantId, pmProgramId, cancellationToken, tracking: true);
        var schedules = await LoadSchedulesForScopeAsync(
            tenantId,
            program.ScopeType,
            program.AssetTypeId,
            program.AssetId,
            request.PmScheduleIds,
            program.Id,
            cancellationToken);

        db.PmProgramSchedules.RemoveRange(program.ProgramSchedules);
        program.ProgramSchedules = schedules
            .Select((schedule, index) => new PmProgramSchedule
            {
                PmProgramId = program.Id,
                PmScheduleId = schedule.Id,
                SortOrder = index
            })
            .ToList();
        program.UpdatedAt = DateTimeOffset.UtcNow;
        program.UpdatedByPersonId = NormalizePersonReference(actorPersonId) ?? program.UpdatedByPersonId;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "pm_program.schedules.replace",
            tenantId,
            actorUserId,
            actorPersonId: actorPersonId,
            targetType: "pm_program",
            targetId: program.Id.ToString(),
            result: "Succeeded",
            cancellationToken: cancellationToken);

        return MapDetail(await LoadProgramAsync(tenantId, program.Id, cancellationToken));
    }

    public async Task<PmProgramScopePreviewResponse> PreviewScopeAsync(
        Guid tenantId,
        CreatePmProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        var candidate = await BuildScopePreviewAsync(tenantId, request, cancellationToken);
        return new PmProgramScopePreviewResponse(
            candidate.MatchedAssets.Count,
            candidate.ExcludedCount,
            candidate.MatchedAssets.Select(MapPreviewAsset).ToList(),
            candidate.Warnings,
            candidate.MatchedAssets.Count > 0);
    }

    public async Task<PmProgramDuePreviewResponse> PreviewDueAsync(
        Guid tenantId,
        CreatePmProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        var scopePreview = await BuildScopePreviewAsync(tenantId, request, cancellationToken);
        var dueDefinition = request.DueDefinition;
        var items = new List<PmProgramDuePreviewItemResponse>();
        var warnings = scopePreview.Warnings.ToList();

        if (dueDefinition is null || dueDefinition.Triggers.Count == 0)
        {
            warnings.Add("Add at least one due trigger to calculate a due preview.");
            return new PmProgramDuePreviewResponse(
                "any",
                items,
                warnings,
                true);
        }

        foreach (var asset in scopePreview.MatchedAssets.Take(5))
        {
            var duePreview = BuildAssetDuePreview(asset, request);
            items.Add(new PmProgramDuePreviewItemResponse(
                asset.AssetId,
                asset.AssetTag,
                asset.AssetName,
                duePreview.TriggerSummary,
                duePreview.EstimatedNextDueDate,
                duePreview.EstimatedNextDueReading,
                duePreview.DueState));
        }

        return new PmProgramDuePreviewResponse(
            dueDefinition.MatchLogic.Trim().ToLowerInvariant(),
            items,
            warnings,
            scopePreview.MatchedAssets.Count == 0);
    }

    public async Task<PmProgramDetailResponse> ActivateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid pmProgramId,
        ActivatePmProgramRequest request,
        string? actorPersonId = null,
        CancellationToken cancellationToken = default)
    {
        var program = await LoadProgramAsync(tenantId, pmProgramId, cancellationToken, tracking: true);
        var richProgram = string.Equals(program.ScopeType, PmProgramScopeTypes.Custom, StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(program.ScopeDefinitionJson)
            || !string.IsNullOrWhiteSpace(program.DueTriggerDefinitionJson)
            || !string.IsNullOrWhiteSpace(program.WorkPackageDefinitionJson)
            || !string.IsNullOrWhiteSpace(program.InspectionDefinitionJson)
            || !string.IsNullOrWhiteSpace(program.ComplianceDefinitionJson)
            || !string.IsNullOrWhiteSpace(program.AutomationDefinitionJson);

        if (string.Equals(program.Status, PmProgramStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            return MapDetail(program);
        }

        if (!richProgram && program.ProgramSchedules.Count == 0)
        {
            throw new StlApiException(
                "pm_program.no_schedules",
                "At least one PM schedule must be assigned before activating a program.",
                400);
        }

        if (richProgram)
        {
            await ValidateRichActivationAsync(program, request, cancellationToken);
        }

        program.Status = PmProgramStatuses.Active;
        program.ActivatedAt = DateTimeOffset.UtcNow;
        program.ActivatedByPersonId = NormalizePersonReference(actorPersonId);
        program.UpdatedAt = program.ActivatedAt.Value;
        program.UpdatedByPersonId = NormalizePersonReference(actorPersonId) ?? program.UpdatedByPersonId;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "pm_program.activate",
            tenantId,
            actorUserId,
            actorPersonId: actorPersonId,
            targetType: "pm_program",
            targetId: program.Id.ToString(),
            result: "Succeeded",
            cancellationToken: cancellationToken);

        return MapDetail(await LoadProgramAsync(tenantId, program.Id, cancellationToken));
    }

    private sealed record AssetPreviewCandidate(
        Asset Asset,
        AssetType AssetType,
        AssetClass AssetClass,
        AssetReadinessState? ReadinessState,
        IReadOnlyList<string> ComplianceCategoryKeys,
        AssetLocationHistory? LocationHistory,
        IReadOnlyDictionary<string, IReadOnlyList<string>> CustomValues,
        IReadOnlyDictionary<string, IReadOnlyList<string>> SpecValues,
        IReadOnlyList<AssetMeter> Meters,
        PmSchedule? LatestSchedule)
    {
        public Guid AssetId => Asset.Id;
        public string AssetTag => Asset.AssetTag;
        public string AssetName => Asset.Name;
        public string AssetTypeName => AssetType.Name;
        public string SiteName => !string.IsNullOrWhiteSpace(Asset.StaffarrSiteNameSnapshot)
            ? Asset.StaffarrSiteNameSnapshot
            : Asset.SiteRef ?? "Unassigned";
        public string? DueStatus => LatestSchedule?.DueStatus;
        public DateTimeOffset? LastPmAt => LatestSchedule?.LastCompletedAt;
    }

    private sealed record ScopePreviewResult(
        IReadOnlyList<AssetPreviewCandidate> MatchedAssets,
        int ExcludedCount,
        IReadOnlyList<string> Warnings);

    private sealed record AssetDuePreviewResult(
        string TriggerSummary,
        string? EstimatedNextDueDate,
        string? EstimatedNextDueReading,
        string DueState);

    private async Task<ScopePreviewResult> BuildScopePreviewAsync(
        Guid tenantId,
        CreatePmProgramRequest request,
        CancellationToken cancellationToken)
    {
        var scopeType = NormalizeScopeType(request.ScopeType);
        var scope = request.ScopeDefinition;
        var hasScopeRules = !string.Equals(scopeType, PmProgramScopeTypes.Custom, StringComparison.OrdinalIgnoreCase)
            || scope is not null
            || (request.PmScheduleIds?.Count ?? 0) > 0;

        if (!hasScopeRules)
        {
            return new ScopePreviewResult([], 0, ["Define an asset scope or include at least one asset to preview matches."]);
        }

        var assetRows = await (
            from asset in db.Assets.AsNoTracking().Where(x => x.TenantId == tenantId)
            join assetType in db.AssetTypes.AsNoTracking() on asset.AssetTypeId equals assetType.Id
            join assetClass in db.AssetClasses.AsNoTracking() on assetType.AssetClassId equals assetClass.Id
            join readiness in db.AssetReadinessStates.AsNoTracking() on asset.Id equals readiness.AssetId into readinessJoin
            from readiness in readinessJoin.DefaultIfEmpty()
            select new { asset, assetType, assetClass, readiness })
            .ToListAsync(cancellationToken);

        var assetIds = assetRows.Select(x => x.asset.Id).ToList();
        var complianceRows = await db.AssetComplianceStates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .ToListAsync(cancellationToken);
        var customRows = await db.AssetCustomFieldValues
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .ToListAsync(cancellationToken);
        var specRows = await db.AssetSpecs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .ToListAsync(cancellationToken);
        var meterRows = await db.AssetMeters
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .ToListAsync(cancellationToken);
        var scheduleRows = await db.PmSchedules
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .ToListAsync(cancellationToken);
        var locationRows = await db.AssetLocationHistory
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .OrderByDescending(x => x.EffectiveAt)
            .ToListAsync(cancellationToken);

        var customLookup = customRows
            .GroupBy(x => x.AssetId)
            .ToDictionary(g => g.Key, g => BuildFieldLookup(g.Select(row => (row.FieldKey, row.ValueJson))));
        var complianceLookup = complianceRows
            .GroupBy(x => x.AssetId)
            .ToDictionary(g => g.Key, g => DeserializeJson<List<string>>(g.OrderByDescending(row => row.UpdatedAt).First().ComplianceCategoryKeysJson) ?? []);
        var specLookup = specRows
            .GroupBy(x => x.AssetId)
            .ToDictionary(g => g.Key, g => BuildFieldLookup(g.Select(row => (row.SpecKey, row.ValueJson))));
        var meterLookup = meterRows
            .GroupBy(x => x.AssetId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<AssetMeter>)g.ToList());
        var scheduleLookup = scheduleRows
            .GroupBy(x => x.AssetId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(row => row.UpdatedAt).First());
        var locationLookup = locationRows
            .GroupBy(x => x.AssetId)
            .ToDictionary(g => g.Key, g => g.First());

        var excludedCount = 0;
        var matched = new List<AssetPreviewCandidate>();
        foreach (var row in assetRows)
        {
            var customValues = customLookup.TryGetValue(row.asset.Id, out var custom) ? custom : EmptyFieldLookup;
            var complianceCategoryKeys = complianceLookup.TryGetValue(row.asset.Id, out var complianceCategories)
                ? complianceCategories
                : [];
            var specValues = specLookup.TryGetValue(row.asset.Id, out var spec) ? spec : EmptyFieldLookup;
            var meters = meterLookup.TryGetValue(row.asset.Id, out var assetMeters) ? assetMeters : [];
            var latestSchedule = scheduleLookup.TryGetValue(row.asset.Id, out var schedule) ? schedule : null;
            var location = locationLookup.TryGetValue(row.asset.Id, out var assetLocation) ? assetLocation : null;

            var candidate = new AssetPreviewCandidate(
                row.asset,
                row.assetType,
                row.assetClass,
                row.readiness,
                complianceCategoryKeys,
                location,
                customValues,
                specValues,
                meters,
                latestSchedule);

            if (MatchesScope(candidate, request))
            {
                matched.Add(candidate);
            }
            else
            {
                excludedCount++;
            }
        }

        var warnings = new List<string>();
        if (string.Equals(scopeType, PmProgramScopeTypes.Custom, StringComparison.OrdinalIgnoreCase)
            && scope is null
            && (request.PmScheduleIds?.Count ?? 0) == 0)
        {
            warnings.Add("The scope is not fully defined yet. Add rules or included assets to preview matches.");
        }

        if (scope?.DepartmentRefs?.Count > 0 || scope?.LocationRefs?.Count > 0)
        {
            warnings.Add("Department and location filters depend on the site/location references available on matching assets.");
        }

        return new ScopePreviewResult(matched, excludedCount, warnings);
    }

    private bool MatchesScope(AssetPreviewCandidate candidate, CreatePmProgramRequest request)
    {
        var scopeType = NormalizeScopeType(request.ScopeType);
        if (string.Equals(scopeType, PmProgramScopeTypes.Asset, StringComparison.OrdinalIgnoreCase))
        {
            return request.AssetId.HasValue && candidate.Asset.Id == request.AssetId.Value;
        }

        if (string.Equals(scopeType, PmProgramScopeTypes.AssetType, StringComparison.OrdinalIgnoreCase))
        {
            return request.AssetTypeId.HasValue && candidate.Asset.AssetTypeId == request.AssetTypeId.Value;
        }

        var scope = request.ScopeDefinition;
        if (scope is null)
        {
            return (request.PmScheduleIds?.Count ?? 0) > 0;
        }

        if ((scope.IncludedAssetIds?.Count ?? 0) > 0 && !scope.IncludedAssetIds!.Contains(candidate.Asset.Id))
        {
            return false;
        }

        if ((scope.ExcludedAssetIds?.Count ?? 0) > 0 && scope.ExcludedAssetIds!.Contains(candidate.Asset.Id))
        {
            return false;
        }

        if (!MatchesAny(scope.AssetTypeIds, candidate.Asset.AssetTypeId))
        {
            return false;
        }

        if (!MatchesAny(scope.AssetClassKeys, candidate.AssetClass.ClassKey))
        {
            return false;
        }

        if (!MatchesAny(scope.AssetCategoryKeys, candidate.ComplianceCategoryKeys))
        {
            return false;
        }

        if (!MatchesAny(scope.AssetStatusKeys, candidate.Asset.LifecycleStatus))
        {
            return false;
        }

        if (!MatchesAny(scope.ReadinessStateKeys, candidate.ReadinessState?.ReadinessStatusKey))
        {
            return false;
        }

        if (!MatchesAny(scope.SiteRefs, candidate.Asset.SiteRef, candidate.Asset.StaffarrSiteOrgUnitId?.ToString("D"), candidate.Asset.StaffarrSiteNameSnapshot))
        {
            return false;
        }

        if (!MatchesAny(scope.MakeKeys, GetFieldValues(candidate.SpecValues, "make", "manufacturer")))
        {
            return false;
        }

        if (!MatchesAny(scope.ModelKeys, GetFieldValues(candidate.SpecValues, "model")))
        {
            return false;
        }

        if (scope.YearFrom.HasValue || scope.YearTo.HasValue)
        {
            var yearValues = GetFieldValues(candidate.SpecValues, "modelYear", "year");
            if (yearValues.Count == 0)
            {
                return false;
            }

            int? year = null;
            foreach (var value in yearValues)
            {
                if (int.TryParse(value, out var parsed))
                {
                    year = parsed;
                    break;
                }
            }

            if (!year.HasValue)
            {
                return false;
            }

            if (scope.YearFrom.HasValue && year.Value < scope.YearFrom.Value)
            {
                return false;
            }

            if (scope.YearTo.HasValue && year.Value > scope.YearTo.Value)
            {
                return false;
            }
        }

        if (!MatchesAny(scope.FuelTypeKeys, GetFieldValues(candidate.SpecValues, "fuelType")))
        {
            return false;
        }

        var tagValues = GetFieldValues(candidate.CustomValues, "tags")
            .Concat(GetFieldValues(candidate.SpecValues, "tags"))
            .ToList();
        if (!MatchesAny(scope.Tags, tagValues))
        {
            return false;
        }

        if (!MatchesAny(scope.DepartmentRefs, candidate.LocationHistory?.HomeLocationId, candidate.LocationHistory?.CurrentLocationId))
        {
            return false;
        }

        if (!MatchesAny(scope.LocationRefs, candidate.LocationHistory?.CurrentLocationId, candidate.LocationHistory?.HomeLocationId))
        {
            return false;
        }

        return true;
    }

    private async Task ValidateRichActivationAsync(
        PmProgram program,
        ActivatePmProgramRequest request,
        CancellationToken cancellationToken)
    {
        var scopeRequest = new CreatePmProgramRequest(
            program.ProgramKey,
            program.Name,
            program.Description,
            program.ScopeType,
            program.AssetTypeId,
            program.AssetId,
            null,
            program.AutoGenerateWorkOrder,
            program.DefaultWorkOrderTemplateRef,
            program.AutoGenerateInspection,
            program.InspectionTemplateId,
            program.CategoryKey,
            program.WorkTypeKey,
            program.PriorityKey,
            program.OwningSiteRef,
            program.OwningTeamRef,
            program.OwningDepartmentRef,
            program.OwnerPersonId,
            program.OwnerRoleKey,
            DeserializeJson<List<string>>(program.TagsJson) ?? [],
            DeserializeJson<PmProgramScopeDefinitionRequest>(program.ScopeDefinitionJson),
            DeserializeJson<PmProgramDueDefinitionRequest>(program.DueTriggerDefinitionJson),
            DeserializeJson<PmProgramWorkPackageDefinitionRequest>(program.WorkPackageDefinitionJson),
            DeserializeJson<PmProgramInspectionDefinitionRequest>(program.InspectionDefinitionJson),
            DeserializeJson<PmProgramComplianceDefinitionRequest>(program.ComplianceDefinitionJson),
            DeserializeJson<PmProgramAutomationDefinitionRequest>(program.AutomationDefinitionJson));

        var scopePreview = await BuildScopePreviewAsync(program.TenantId, scopeRequest, cancellationToken);
        if (scopePreview.MatchedAssets.Count == 0 && !request.ConfirmZeroMatch)
        {
            throw new StlApiException(
                "pm_program.scope_empty",
                "This PM program does not match any assets. Save it as a draft or confirm activation explicitly.",
                400);
        }

        if (string.IsNullOrWhiteSpace(program.CategoryKey)
            || string.IsNullOrWhiteSpace(program.WorkTypeKey)
            || string.IsNullOrWhiteSpace(program.PriorityKey))
        {
            throw new StlApiException(
                "pm_program.basics_incomplete",
                "Category, work type, and priority must be defined before activation.",
                400);
        }

        var dueDefinition = DeserializeJson<PmProgramDueDefinitionRequest>(program.DueTriggerDefinitionJson);
        if ((dueDefinition is null || dueDefinition.Triggers.Count == 0)
            && program.ProgramSchedules.Count == 0)
        {
            throw new StlApiException(
                "pm_program.due_missing",
                "At least one due trigger must be configured before activation.",
                400);
        }

        if (program.AutoGenerateWorkOrder)
        {
            var workPackage = DeserializeJson<PmProgramWorkPackageDefinitionRequest>(program.WorkPackageDefinitionJson);
            if (workPackage is null || !workPackage.GenerateWorkOrder)
            {
                throw new StlApiException(
                    "pm_program.work_package_missing",
                    "Work order generation requires a work package definition.",
                    400);
            }
        }

        InspectionTemplate? inspectionTemplate = null;
        if (program.InspectionTemplateId.HasValue)
        {
            inspectionTemplate = await ResolveInspectionTemplateAsync(
                program.TenantId,
                program.ScopeType,
                program.AssetTypeId,
                program.AssetId,
                true,
                program.InspectionTemplateId,
                cancellationToken);
        }
        else if (program.AutoGenerateInspection)
        {
            throw new StlApiException(
                "pm_program.inspection_template_required",
                "An active inspection template must be attached before activation.",
                400);
        }

        if (inspectionTemplate is not null
            && string.Equals(program.ScopeType, PmProgramScopeTypes.Custom, StringComparison.OrdinalIgnoreCase)
            && scopePreview.MatchedAssets.Count > 0)
        {
            var linkedAssetTypeIds = inspectionTemplate.AssetTypeLinks.Select(x => x.AssetTypeId).ToHashSet();
            if (linkedAssetTypeIds.Count > 0
                && scopePreview.MatchedAssets.Any(candidate => !linkedAssetTypeIds.Contains(candidate.Asset.AssetTypeId)))
            {
                throw new StlApiException(
                    "pm_program.inspection_template_scope_mismatch",
                    "Inspection template does not apply to the program's matched asset scope.",
                    400);
            }
        }

        var compliance = DeserializeJson<PmProgramComplianceDefinitionRequest>(program.ComplianceDefinitionJson);
        if (compliance is not null
            && !string.Equals(compliance.ReadinessImpact, "no_impact", StringComparison.OrdinalIgnoreCase)
            && !request.ConfirmReadinessImpact)
        {
            throw new StlApiException(
                "pm_program.readiness_confirmation_required",
                "Readiness-impacting PM programs require explicit confirmation before activation.",
                400);
        }

        if (compliance is not null
            && compliance.IsComplianceRelated
            && !request.ConfirmComplianceImpact)
        {
            throw new StlApiException(
                "pm_program.compliance_confirmation_required",
                "Compliance-related PM programs require explicit confirmation before activation.",
                400);
        }

        var automation = DeserializeJson<PmProgramAutomationDefinitionRequest>(program.AutomationDefinitionJson);
        if (automation is not null && automation.DuplicatePreventionWindowDays < 0)
        {
            throw new StlApiException(
                "pm_program.duplicate_window_invalid",
                "Duplicate prevention window must be zero or greater.",
                400);
        }
    }

    private static AssetDuePreviewResult BuildAssetDuePreview(
        AssetPreviewCandidate candidate,
        CreatePmProgramRequest request)
    {
        var dueDefinition = request.DueDefinition;
        if (dueDefinition is null || dueDefinition.Triggers.Count == 0)
        {
            return new AssetDuePreviewResult(
                "No due trigger configured",
                null,
                null,
                "upcoming");
        }

        var triggerSummaries = new List<string>();
        var dueDates = new List<DateTimeOffset>();
        var meterReadings = new List<decimal>();
        var triggerStates = new List<string>();

        foreach (var trigger in dueDefinition.Triggers)
        {
            var triggerType = trigger.TriggerType.Trim().ToLowerInvariant();
            switch (triggerType)
            {
                case "calendar":
                case "time":
                {
                    var calendar = trigger.Calendar;
                    if (calendar is null || calendar.IntervalValue <= 0)
                    {
                        triggerSummaries.Add("Calendar trigger");
                        triggerStates.Add("upcoming");
                        break;
                    }

                    var dueDate = calendar.FirstDueDate.HasValue
                        ? new DateTimeOffset(calendar.FirstDueDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))
                        : ResolveCalendarDueDate(candidate, calendar);

                    dueDates.Add(dueDate);
                    triggerSummaries.Add($"Every {calendar.IntervalValue} {calendar.IntervalUnit}");
                    triggerStates.Add(dueDate <= DateTimeOffset.UtcNow ? "due" : "upcoming");
                    break;
                }
                case "meter":
                case "mileage":
                case "engine_hours":
                case "cycles":
                {
                    var meterTrigger = trigger.Meter;
                    if (meterTrigger is null || meterTrigger.IntervalValue <= 0)
                    {
                        triggerSummaries.Add(triggerType);
                        triggerStates.Add("upcoming");
                        break;
                    }

                    var meter = ResolvePreviewMeter(candidate, meterTrigger.IntervalUnit);
                    if (meter is null)
                    {
                        triggerSummaries.Add($"Usage trigger ({meterTrigger.IntervalValue} {meterTrigger.IntervalUnit})");
                        triggerStates.Add("upcoming");
                        break;
                    }

                    var dueReading = meterTrigger.FirstDueReading
                        ?? meterTrigger.AnchorReading
                        ?? meter.CurrentReading + meterTrigger.IntervalValue;
                    meterReadings.Add(dueReading);
                    triggerSummaries.Add($"Every {meterTrigger.IntervalValue} {meterTrigger.IntervalUnit}");
                    triggerStates.Add(meter.CurrentReading >= dueReading ? "due" : "upcoming");
                    break;
                }
                case "one_time":
                {
                    var oneTime = trigger.OneTime;
                    if (oneTime is null)
                    {
                        triggerSummaries.Add("One-time trigger");
                        triggerStates.Add("upcoming");
                        break;
                    }

                    var dueDate = new DateTimeOffset(oneTime.DueDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
                    dueDates.Add(dueDate);
                    triggerSummaries.Add($"Due {oneTime.DueDate:yyyy-MM-dd}");
                    triggerStates.Add(dueDate <= DateTimeOffset.UtcNow ? "due" : "upcoming");
                    break;
                }
                case "manual":
                    triggerSummaries.Add("Manual-only");
                    triggerStates.Add("upcoming");
                    break;
                default:
                    triggerSummaries.Add(triggerType);
                    triggerStates.Add("upcoming");
                    break;
            }
        }

        var matchLogic = string.Equals(dueDefinition.MatchLogic, "all", StringComparison.OrdinalIgnoreCase)
            ? "all"
            : "any";
        var dueState = matchLogic == "all"
            ? (triggerStates.All(state => state == "due") ? "due" : "upcoming")
            : (triggerStates.Contains("due") ? "due" : "upcoming");
        DateTimeOffset? nextDueDate = dueDates.Count == 0
            ? null
            : (matchLogic == "all" ? dueDates.Max() : dueDates.Min());
        decimal? nextDueReading = meterReadings.Count == 0
            ? null
            : (matchLogic == "all" ? meterReadings.Max() : meterReadings.Min());

        return new AssetDuePreviewResult(
            string.Join(" or ", triggerSummaries),
            nextDueDate?.ToString("yyyy-MM-dd"),
            nextDueReading?.ToString("0.##"),
            dueState);
    }

    private static AssetMeter? ResolvePreviewMeter(AssetPreviewCandidate candidate, string? unit)
    {
        if (candidate.Meters.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(unit))
        {
            var meter = candidate.Meters.FirstOrDefault(x => string.Equals(x.Unit, unit, StringComparison.OrdinalIgnoreCase));
            if (meter is not null)
            {
                return meter;
            }
        }

        return candidate.Meters.FirstOrDefault();
    }

    private static DateTimeOffset ResolveCalendarDueDate(
        AssetPreviewCandidate candidate,
        PmProgramCalendarTriggerRequest calendar)
    {
        var baseDate = calendar.AnchorDate.HasValue
            ? new DateTimeOffset(calendar.AnchorDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))
            : candidate.LatestSchedule?.LastCompletedAt ?? DateTimeOffset.UtcNow;

        var intervalValue = Math.Max(1, calendar.IntervalValue);
        return calendar.IntervalUnit.Trim().ToLowerInvariant() switch
        {
            "weeks" => baseDate.AddDays(intervalValue * 7),
            "months" => baseDate.AddMonths(intervalValue),
            "years" => baseDate.AddYears(intervalValue),
            _ => baseDate.AddDays(intervalValue),
        };
    }

    private static bool MatchesAny(IReadOnlyList<Guid>? values, Guid currentValue) =>
        values is null || values.Count == 0 || values.Contains(currentValue);

    private static bool MatchesAny(IReadOnlyList<string>? values, params string?[] candidates)
    {
        if (values is null || values.Count == 0)
        {
            return true;
        }

        var normalizedCandidates = candidates
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Select(candidate => candidate!.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return values.Any(value => normalizedCandidates.Contains(value.Trim()));
    }

    private static bool MatchesAny(IReadOnlyList<string>? values, IReadOnlyList<string> candidates)
    {
        if (values is null || values.Count == 0)
        {
            return true;
        }

        if (candidates.Count == 0)
        {
            return false;
        }

        var normalizedCandidates = candidates
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Select(candidate => candidate.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return values.Any(value => normalizedCandidates.Contains(value.Trim()));
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildFieldLookup(
        IEnumerable<(string FieldKey, string ValueJson)> rows)
    {
        var lookup = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (fieldKey, valueJson) in rows)
        {
            lookup[fieldKey.Trim()] = ExtractStringValues(valueJson);
        }

        return lookup;
    }

    private static IReadOnlyList<string> GetFieldValues(
        IReadOnlyDictionary<string, IReadOnlyList<string>> lookup,
        params string[] keys)
    {
        foreach (var key in keys)
        {
            if (lookup.TryGetValue(key, out var values) && values.Count > 0)
            {
                return values;
            }
        }

        return [];
    }

    private static IReadOnlyList<string> ExtractStringValues(string valueJson)
    {
        if (string.IsNullOrWhiteSpace(valueJson))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(valueJson);
            return ExtractStringValues(document.RootElement).ToList();
        }
        catch
        {
            return [valueJson.Trim().Trim('"')];
        }
    }

    private static IEnumerable<string> ExtractStringValues(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Array => element.EnumerateArray().SelectMany(ExtractStringValues),
            JsonValueKind.Object => element.EnumerateObject().SelectMany(property => ExtractStringValues(property.Value)),
            JsonValueKind.String => string.IsNullOrWhiteSpace(element.GetString())
                ? []
                : [element.GetString()!],
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => [element.ToString()],
            _ => [],
        };
    }

    private async Task<PmProgram> LoadProgramAsync(
        Guid tenantId,
        Guid pmProgramId,
        CancellationToken cancellationToken,
        bool tracking = false)
    {
        var query = db.PmPrograms
            .Include(x => x.AssetType)
            .Include(x => x.Asset)
            .Include(x => x.InspectionTemplate)
            .Include(x => x.ProgramSchedules)
            .ThenInclude(x => x.PmSchedule)
            .ThenInclude(x => x.Asset)
            .Where(x => x.TenantId == tenantId && x.Id == pmProgramId);

        var program = tracking
            ? await query.FirstOrDefaultAsync(cancellationToken)
            : await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        if (program is null)
        {
            throw new StlApiException("pm_program.not_found", "PM program was not found.", 404);
        }

        return program;
    }

    private async Task<IReadOnlyList<PmSchedule>> LoadSchedulesForScopeAsync(
        Guid tenantId,
        string scopeType,
        Guid? assetTypeId,
        Guid? assetId,
        IReadOnlyList<Guid> pmScheduleIds,
        Guid? excludeProgramId,
        CancellationToken cancellationToken)
    {
        if (pmScheduleIds.Count == 0)
        {
            return [];
        }

        var distinctIds = pmScheduleIds.Distinct().ToList();
        var schedules = await db.PmSchedules
            .Include(x => x.Asset)
            .Where(x => x.TenantId == tenantId && distinctIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (schedules.Count != distinctIds.Count)
        {
            throw new StlApiException(
                "pm_program.schedule_not_found",
                "One or more PM schedules were not found.",
                400);
        }

        var alreadyAssignedQuery = db.PmProgramSchedules
            .AsNoTracking()
            .Where(x => distinctIds.Contains(x.PmScheduleId));
        if (excludeProgramId.HasValue)
        {
            alreadyAssignedQuery = alreadyAssignedQuery.Where(x => x.PmProgramId != excludeProgramId.Value);
        }

        var alreadyAssigned = await alreadyAssignedQuery
            .Select(x => x.PmScheduleId)
            .ToListAsync(cancellationToken);

        if (alreadyAssigned.Count > 0)
        {
            throw new StlApiException(
                "pm_program.schedule_already_assigned",
                "One or more PM schedules are already assigned to another program.",
                409);
        }

        foreach (var schedule in schedules)
        {
            EnsureScheduleMatchesScope(scopeType, assetTypeId, assetId, schedule);
        }

        return distinctIds
            .Select(id => schedules.Single(s => s.Id == id))
            .ToList();
    }

    private static void EnsureScheduleMatchesScope(
        string scopeType,
        Guid? assetTypeId,
        Guid? assetId,
        PmSchedule schedule)
    {
        if (string.Equals(scopeType, PmProgramScopeTypes.Asset, StringComparison.OrdinalIgnoreCase))
        {
            if (schedule.AssetId != assetId)
            {
                throw new StlApiException(
                    "pm_program.schedule_scope_mismatch",
                    $"PM schedule '{schedule.ScheduleKey}' does not belong to the program's asset scope.",
                    400);
            }

            return;
        }

        if (schedule.Asset.AssetTypeId != assetTypeId)
        {
            throw new StlApiException(
                "pm_program.schedule_scope_mismatch",
                $"PM schedule '{schedule.ScheduleKey}' does not belong to the program's asset type scope.",
                400);
        }
    }

    private async Task<(Guid? AssetTypeId, Guid? AssetId)> ResolveScopeAsync(
        Guid tenantId,
        string scopeType,
        Guid? assetTypeId,
        Guid? assetId,
        CancellationToken cancellationToken)
    {
        if (string.Equals(scopeType, PmProgramScopeTypes.Custom, StringComparison.OrdinalIgnoreCase))
        {
            if (assetId.HasValue)
            {
                _ = await assetService.GetAsync(tenantId, assetId.Value, cancellationToken);
            }

            if (assetTypeId.HasValue)
            {
                _ = await assetTypeService.GetActiveTypeAsync(tenantId, assetTypeId.Value, cancellationToken);
            }

            return (assetTypeId, assetId);
        }

        if (string.Equals(scopeType, PmProgramScopeTypes.Asset, StringComparison.OrdinalIgnoreCase))
        {
            if (!assetId.HasValue)
            {
                throw new StlApiException(
                    "pm_program.asset_required",
                    "Asset scope requires assetId.",
                    400);
            }

            _ = await assetService.GetAsync(tenantId, assetId.Value, cancellationToken);
            return (null, assetId.Value);
        }

        if (!assetTypeId.HasValue)
        {
            throw new StlApiException(
                "pm_program.asset_type_required",
                "Asset type scope requires assetTypeId.",
                400);
        }

        _ = await assetTypeService.GetActiveTypeAsync(tenantId, assetTypeId.Value, cancellationToken);
        return (assetTypeId.Value, null);
    }

    private async Task<InspectionTemplate?> ResolveInspectionTemplateAsync(
        Guid tenantId,
        string scopeType,
        Guid? assetTypeId,
        Guid? assetId,
        bool autoGenerateInspection,
        Guid? inspectionTemplateId,
        CancellationToken cancellationToken)
    {
        if (!autoGenerateInspection)
        {
            if (!inspectionTemplateId.HasValue)
            {
                return null;
            }
        }
        else if (!inspectionTemplateId.HasValue)
        {
            throw new StlApiException(
                "pm_program.inspection_template_required",
                "Inspection template is required when auto-generate inspection is enabled.",
                400);
        }

        if (!inspectionTemplateId.HasValue)
        {
            return null;
        }

        var template = await db.InspectionTemplates
            .Include(x => x.AssetTypeLinks)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == inspectionTemplateId.Value,
                cancellationToken);

        if (template is null)
        {
            throw new StlApiException(
                "pm_program.inspection_template_not_found",
                "Inspection template was not found.",
                404);
        }

        if (!string.Equals(template.Status, InspectionTemplateStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "pm_program.inspection_template_not_active",
                "Inspection template must be active before it can be assigned to a PM program.",
                400);
        }

        var linkedAssetTypeIds = template.AssetTypeLinks.Select(x => x.AssetTypeId).ToHashSet();
        if (linkedAssetTypeIds.Count > 0)
        {
            if (string.Equals(scopeType, PmProgramScopeTypes.Asset, StringComparison.OrdinalIgnoreCase))
            {
                if (!assetId.HasValue)
                {
                    throw new StlApiException(
                        "pm_program.asset_required",
                        "Asset scope requires assetId.",
                        400);
                }

                var asset = await assetService.GetAsync(tenantId, assetId.Value, cancellationToken);
                if (!linkedAssetTypeIds.Contains(asset.AssetTypeId))
                {
                    throw new StlApiException(
                        "pm_program.inspection_template_scope_mismatch",
                        "Inspection template does not apply to the program's asset scope.",
                        400);
                }
            }
            else if (assetTypeId.HasValue && !linkedAssetTypeIds.Contains(assetTypeId.Value))
            {
                throw new StlApiException(
                    "pm_program.inspection_template_scope_mismatch",
                    "Inspection template does not apply to the program's asset type scope.",
                    400);
            }
        }

        return template;
    }

    private static PmProgramSummaryResponse MapSummary(PmProgram program)
    {
        var tags = DeserializeJson<List<string>>(program.TagsJson) ?? [];
        return new PmProgramSummaryResponse(
            program.Id,
            program.ProgramKey,
            program.Name,
            program.ScopeType,
            program.AssetTypeId,
            program.AssetType?.Name,
            program.AssetId,
            program.Asset?.AssetTag,
            program.Status,
            program.AutoGenerateWorkOrder,
            program.DefaultWorkOrderTemplateRef,
            program.AutoGenerateInspection,
            program.InspectionTemplateId,
            program.InspectionTemplate?.TemplateKey,
            program.InspectionTemplate?.Name,
            program.ProgramSchedules.Count,
            program.CreatedAt,
            program.UpdatedAt,
            program.CategoryKey,
            program.WorkTypeKey,
            program.PriorityKey,
            program.OwningSiteRef,
            program.OwningTeamRef,
            program.OwningDepartmentRef,
            program.OwnerPersonId,
            program.OwnerRoleKey,
            tags,
            program.ActivatedAt,
            program.PausedAt,
            program.RetiredAt,
            program.ScopeType == PmProgramScopeTypes.Custom ? null : program.ProgramSchedules.Count,
            BuildScopeSummary(program),
            BuildDueSummary(program),
            BuildWorkPackageSummary(program),
            BuildInspectionSummary(program),
            BuildComplianceSummary(program),
            BuildAutomationSummary(program));
    }

    private static PmProgramPreviewAssetResponse MapPreviewAsset(AssetPreviewCandidate candidate) =>
        new(
            candidate.AssetId,
            candidate.AssetTag,
            candidate.AssetName,
            candidate.AssetTypeName,
            candidate.SiteName,
            candidate.Asset.LifecycleStatus,
            candidate.ReadinessState?.ReadinessStatusKey,
            candidate.DueStatus,
            candidate.LastPmAt,
            null);

    private static PmProgramDetailResponse MapDetail(PmProgram program)
    {
        var tags = DeserializeJson<List<string>>(program.TagsJson) ?? [];
        return new PmProgramDetailResponse(
            program.Id,
            program.ProgramKey,
            program.Name,
            program.Description,
            program.ScopeType,
            program.AssetTypeId,
            program.AssetType?.TypeKey,
            program.AssetType?.Name,
            program.AssetId,
            program.Asset?.AssetTag,
            program.Asset?.Name,
            program.Status,
            program.AutoGenerateWorkOrder,
            program.DefaultWorkOrderTemplateRef,
            program.AutoGenerateInspection,
            program.InspectionTemplateId,
            program.InspectionTemplate?.TemplateKey,
            program.InspectionTemplate?.Name,
            program.ProgramSchedules
                .OrderBy(x => x.SortOrder)
                .Select(x => new PmProgramScheduleLinkResponse(
                    x.PmScheduleId,
                    x.PmSchedule.ScheduleKey,
                    x.PmSchedule.Name,
                    x.PmSchedule.Asset.AssetTag,
                    x.PmSchedule.Asset.Name,
                    x.PmSchedule.DueStatus,
                    x.PmSchedule.Status,
                    x.SortOrder))
                .ToList(),
            program.CreatedAt,
            program.UpdatedAt,
            program.CategoryKey,
            program.WorkTypeKey,
            program.PriorityKey,
            program.OwningSiteRef,
            program.OwningTeamRef,
            program.OwningDepartmentRef,
            program.OwnerPersonId,
            program.OwnerRoleKey,
            tags,
            program.ActivatedAt,
            program.ActivatedByPersonId,
            program.PausedAt,
            program.PausedByPersonId,
            program.RetiredAt,
            program.RetiredByPersonId,
            program.ScopeType == PmProgramScopeTypes.Custom ? null : program.ProgramSchedules.Count,
            BuildScopeSummary(program),
            BuildDueSummary(program),
            BuildWorkPackageSummary(program),
            BuildInspectionSummary(program),
            BuildComplianceSummary(program),
            BuildAutomationSummary(program));
    }

    private static string NormalizeProgramKey(string programKey)
    {
        var normalized = programKey.Trim().ToLowerInvariant();
        if (normalized.Length < 3 || normalized.Length > 128)
        {
            throw new StlApiException(
                "pm_program.invalid_key",
                "Program key must be between 3 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeName(string name)
    {
        var trimmed = name.Trim();
        if (trimmed.Length < 3 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "pm_program.invalid_name",
                "Program name must be between 3 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeDescription(string description)
    {
        var trimmed = description.Trim();
        if (trimmed.Length > 512)
        {
            throw new StlApiException(
                "pm_program.invalid_description",
                "Program description must be 512 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string? NormalizeTemplateRef(string? templateRef)
    {
        if (string.IsNullOrWhiteSpace(templateRef))
        {
            return null;
        }

        var trimmed = templateRef.Trim();
        if (trimmed.Length > 128)
        {
            throw new StlApiException(
                "pm_program.invalid_template_ref",
                "Template reference must be 128 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static bool IsRichProgramRequest(CreatePmProgramRequest request) =>
        string.Equals(request.ScopeType, PmProgramScopeTypes.Custom, StringComparison.OrdinalIgnoreCase)
        || request.ScopeDefinition is not null
        || request.DueDefinition is not null
        || request.WorkPackageDefinition is not null
        || request.InspectionDefinition is not null
        || request.ComplianceDefinition is not null
        || request.AutomationDefinition is not null
        || !string.IsNullOrWhiteSpace(request.CategoryKey)
        || !string.IsNullOrWhiteSpace(request.WorkTypeKey)
        || !string.IsNullOrWhiteSpace(request.PriorityKey)
        || !string.IsNullOrWhiteSpace(request.OwningSiteRef)
        || !string.IsNullOrWhiteSpace(request.OwningTeamRef)
        || !string.IsNullOrWhiteSpace(request.OwningDepartmentRef)
        || !string.IsNullOrWhiteSpace(request.OwnerPersonId)
        || !string.IsNullOrWhiteSpace(request.OwnerRoleKey)
        || ((request.Tags?.Count ?? 0) > 0);

    private static string NormalizeCatalogKey(
        string? value,
        string fallback,
        string fieldName,
        bool required)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required)
            {
                throw new StlApiException(
                    $"pm_program.{fieldName}_required",
                    $"PM program {fieldName} is required.",
                    400);
            }

            return fallback;
        }

        return value.Trim().ToLowerInvariant();
    }

    private static string? NormalizeReference(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    private static string? NormalizePersonReference(string? value)
    {
        var normalized = NormalizeReference(value);
        if (normalized is null)
        {
            return null;
        }

        return normalized;
    }

    private static IReadOnlyList<string> NormalizeTags(IReadOnlyList<string>? tags) =>
        tags is null
            ? []
            : tags
                .Select(tag => tag.Trim())
                .Where(tag => tag.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

    private static string SerializeJson<T>(T? value) =>
        JsonSerializer.Serialize(value, JsonOptions);

    private static T? DeserializeJson<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    private static string BuildScopeSummary(PmProgram program)
    {
        if (string.Equals(program.ScopeType, PmProgramScopeTypes.Asset, StringComparison.OrdinalIgnoreCase))
        {
            return program.Asset is null
                ? "Single asset scope"
                : $"Applies to asset {program.Asset.AssetTag} at {program.Asset.Name}";
        }

        if (string.Equals(program.ScopeType, PmProgramScopeTypes.AssetType, StringComparison.OrdinalIgnoreCase))
        {
            return program.AssetType is null
                ? "Asset type scope"
                : $"Applies to {program.AssetType.Name} assets";
        }

        var scope = DeserializeJson<PmProgramScopeDefinitionRequest>(program.ScopeDefinitionJson);
        if (scope is null)
        {
            return "Custom asset scope";
        }

        var filters = new List<string>();
        if ((scope.AssetTypeIds?.Count ?? 0) > 0) filters.Add($"{scope.AssetTypeIds!.Count} asset type(s)");
        if ((scope.AssetClassKeys?.Count ?? 0) > 0) filters.Add($"{scope.AssetClassKeys!.Count} class key(s)");
        if ((scope.AssetCategoryKeys?.Count ?? 0) > 0) filters.Add($"{scope.AssetCategoryKeys!.Count} category(s)");
        if ((scope.SiteRefs?.Count ?? 0) > 0) filters.Add($"{scope.SiteRefs!.Count} site(s)");
        if ((scope.DepartmentRefs?.Count ?? 0) > 0) filters.Add($"{scope.DepartmentRefs!.Count} department(s)");
        if ((scope.MakeKeys?.Count ?? 0) > 0) filters.Add($"{scope.MakeKeys!.Count} make(s)");
        if ((scope.ModelKeys?.Count ?? 0) > 0) filters.Add($"{scope.ModelKeys!.Count} model(s)");
        if (scope.YearFrom.HasValue || scope.YearTo.HasValue) filters.Add("year range");
        if ((scope.IncludedAssetIds?.Count ?? 0) > 0) filters.Add($"{scope.IncludedAssetIds!.Count} included asset(s)");
        if ((scope.ExcludedAssetIds?.Count ?? 0) > 0) filters.Add($"{scope.ExcludedAssetIds!.Count} excluded asset(s)");
        return filters.Count == 0 ? "Custom asset scope" : string.Join(", ", filters);
    }

    private static string BuildDueSummary(PmProgram program)
    {
        var due = DeserializeJson<PmProgramDueDefinitionRequest>(program.DueTriggerDefinitionJson);
        if (due is null || due.Triggers.Count == 0)
        {
            return "No due trigger configured";
        }

        var triggerLabels = due.Triggers.Select(trigger =>
        {
            var triggerType = trigger.TriggerType.Trim().ToLowerInvariant();
            return triggerType switch
            {
                "calendar" or "time" => trigger.Calendar is null
                    ? "Calendar trigger"
                    : $"Every {trigger.Calendar.IntervalValue} {trigger.Calendar.IntervalUnit}",
                "meter" or "mileage" or "engine_hours" or "cycles" => trigger.Meter is null
                    ? "Usage trigger"
                    : $"Every {trigger.Meter.IntervalValue} {trigger.Meter.IntervalUnit}",
                "one_time" => trigger.OneTime is null ? "One-time trigger" : $"Due {trigger.OneTime.DueDate:yyyy-MM-dd}",
                "manual" => "Manual-only",
                _ => triggerType,
            };
        }).ToList();

        var logic = string.Equals(due.MatchLogic, "all", StringComparison.OrdinalIgnoreCase)
            ? "all"
            : "any";
        return $"{string.Join(" + ", triggerLabels)} ({logic})";
    }

    private static string BuildWorkPackageSummary(PmProgram program)
    {
        if (!program.AutoGenerateWorkOrder)
        {
            return "No work order generated";
        }

        var package = DeserializeJson<PmProgramWorkPackageDefinitionRequest>(program.WorkPackageDefinitionJson);
        var title = package?.WorkOrderTitleTemplate?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Generates a work order";
        }

        return $"Generates work orders titled '{title}'";
    }

    private static string BuildInspectionSummary(PmProgram program)
    {
        if (!program.AutoGenerateInspection)
        {
            return "No inspection attached";
        }

        var inspection = DeserializeJson<PmProgramInspectionDefinitionRequest>(program.InspectionDefinitionJson);
        if (!string.IsNullOrWhiteSpace(program.InspectionTemplate?.Name))
        {
            return $"Attached inspection template {program.InspectionTemplate.Name}";
        }

        if (inspection?.InspectionTemplateId is Guid templateId)
        {
            return $"Attached inspection template {templateId:D}";
        }

        return "Attached inspection template";
    }

    private static string BuildComplianceSummary(PmProgram program)
    {
        var compliance = DeserializeJson<PmProgramComplianceDefinitionRequest>(program.ComplianceDefinitionJson);
        if (compliance is null || !compliance.IsComplianceRelated)
        {
            return "No compliance impact";
        }

        return string.Equals(compliance.ReadinessImpact, "no_impact", StringComparison.OrdinalIgnoreCase)
            ? "Compliance-related, no readiness impact"
            : $"Compliance-related, readiness impact: {compliance.ReadinessImpact.Replace('_', ' ')}";
    }

    private static string BuildAutomationSummary(PmProgram program)
    {
        var automation = DeserializeJson<PmProgramAutomationDefinitionRequest>(program.AutomationDefinitionJson);
        if (automation is null)
        {
            return "Manual generation";
        }

        return $"Lead {automation.LeadTimeDays} days, duplicate window {automation.DuplicatePreventionWindowDays} days";
    }

    private static int? CountMatchedAssets(PmProgram program, IReadOnlyList<PmProgramPreviewAssetResponse>? previewAssets = null) =>
        previewAssets?.Count ?? program.ProgramSchedules.Count;

    private static string NormalizeScopeType(string scopeType)
    {
        var normalized = scopeType.Trim().ToLowerInvariant();
        if (!string.Equals(normalized, PmProgramScopeTypes.AssetType, StringComparison.Ordinal)
            && !string.Equals(normalized, PmProgramScopeTypes.Asset, StringComparison.Ordinal)
            && !string.Equals(normalized, PmProgramScopeTypes.Custom, StringComparison.Ordinal))
        {
            throw new StlApiException(
                "pm_program.invalid_scope",
                $"Scope type must be '{PmProgramScopeTypes.AssetType}', '{PmProgramScopeTypes.Asset}', or '{PmProgramScopeTypes.Custom}'.",
                400);
        }

        return normalized;
    }

    private static string NormalizeStatus(string status)
    {
        var normalized = status.Trim().ToLowerInvariant();
        if (!AllowedStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "pm_program.invalid_status",
                $"Program status must be one of: {string.Join(", ", AllowedStatuses.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }
}
