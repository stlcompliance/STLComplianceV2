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
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        PmProgramStatuses.Draft,
        PmProgramStatuses.Active,
        PmProgramStatuses.Inactive
    };

    public async Task<IReadOnlyList<PmProgramSummaryResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from program in db.PmPrograms.AsNoTracking().Where(x => x.TenantId == tenantId)
            join assetType in db.AssetTypes.AsNoTracking()
                on program.AssetTypeId equals assetType.Id into assetTypes
            from assetType in assetTypes.DefaultIfEmpty()
            join asset in db.Assets.AsNoTracking()
                on program.AssetId equals asset.Id into assets
            from asset in assets.DefaultIfEmpty()
            orderby program.Name, program.ProgramKey
            select new PmProgramSummaryResponse(
                program.Id,
                program.ProgramKey,
                program.Name,
                program.ScopeType,
                program.AssetTypeId,
                assetType != null ? assetType.Name : null,
                program.AssetId,
                asset != null ? asset.AssetTag : null,
                program.Status,
                program.ProgramSchedules.Count,
                program.CreatedAt,
                program.UpdatedAt))
            .ToListAsync(cancellationToken);
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
        CancellationToken cancellationToken = default)
    {
        var programKey = NormalizeProgramKey(request.ProgramKey);
        var name = NormalizeName(request.Name);
        var description = NormalizeDescription(request.Description);
        var scopeType = NormalizeScopeType(request.ScopeType);
        var (assetTypeId, assetId) = await ResolveScopeAsync(
            tenantId,
            scopeType,
            request.AssetTypeId,
            request.AssetId,
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
        var schedules = await LoadSchedulesForScopeAsync(
            tenantId,
            scopeType,
            assetTypeId,
            assetId,
            scheduleIds,
            excludeProgramId: null,
            cancellationToken);

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
            Status = PmProgramStatuses.Draft,
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
            "pm_program",
            program.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapDetail(await LoadProgramAsync(tenantId, program.Id, cancellationToken));
    }

    public async Task<PmProgramDetailResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid pmProgramId,
        UpdatePmProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        var program = await LoadProgramAsync(tenantId, pmProgramId, cancellationToken, tracking: true);
        var status = NormalizeStatus(request.Status);

        if (string.Equals(status, PmProgramStatuses.Active, StringComparison.OrdinalIgnoreCase)
            && program.ProgramSchedules.Count == 0)
        {
            throw new StlApiException(
                "pm_program.no_schedules",
                "At least one PM schedule must be assigned before activating a program.",
                400);
        }

        program.Name = NormalizeName(request.Name);
        program.Description = NormalizeDescription(request.Description);
        program.Status = status;
        program.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            status == PmProgramStatuses.Active ? "pm_program.activate" : "pm_program.update",
            tenantId,
            actorUserId,
            "pm_program",
            program.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapDetail(await LoadProgramAsync(tenantId, program.Id, cancellationToken));
    }

    public async Task<PmProgramDetailResponse> UpdateStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid pmProgramId,
        UpdatePmProgramStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var program = await LoadProgramAsync(tenantId, pmProgramId, cancellationToken, tracking: true);
        var status = NormalizeStatus(request.Status);

        if (string.Equals(status, PmProgramStatuses.Active, StringComparison.OrdinalIgnoreCase)
            && program.ProgramSchedules.Count == 0)
        {
            throw new StlApiException(
                "pm_program.no_schedules",
                "At least one PM schedule must be assigned before activating a program.",
                400);
        }

        program.Status = status;
        program.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            status == PmProgramStatuses.Active ? "pm_program.activate" : "pm_program.status.update",
            tenantId,
            actorUserId,
            "pm_program",
            program.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapDetail(await LoadProgramAsync(tenantId, program.Id, cancellationToken));
    }

    public async Task<PmProgramDetailResponse> ReplaceSchedulesAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid pmProgramId,
        ReplacePmProgramSchedulesRequest request,
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

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "pm_program.schedules.replace",
            tenantId,
            actorUserId,
            "pm_program",
            program.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapDetail(await LoadProgramAsync(tenantId, program.Id, cancellationToken));
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

    private static PmProgramDetailResponse MapDetail(PmProgram program) =>
        new(
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
            program.UpdatedAt);

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

    private static string NormalizeScopeType(string scopeType)
    {
        var normalized = scopeType.Trim().ToLowerInvariant();
        if (!string.Equals(normalized, PmProgramScopeTypes.AssetType, StringComparison.Ordinal)
            && !string.Equals(normalized, PmProgramScopeTypes.Asset, StringComparison.Ordinal))
        {
            throw new StlApiException(
                "pm_program.invalid_scope",
                $"Scope type must be '{PmProgramScopeTypes.AssetType}' or '{PmProgramScopeTypes.Asset}'.",
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
