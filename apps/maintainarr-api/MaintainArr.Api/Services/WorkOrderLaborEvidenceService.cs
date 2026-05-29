using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class WorkOrderLaborEvidenceService(
    MaintainArrDbContext db,
    MaintainArrEvidenceStorageService storage,
    IMaintainArrAuditService audit,
    TechnicianRefService technicianRefService)
{
    private const long MaxEvidenceBytes = 10 * 1024 * 1024;
    private const decimal MaxLaborHours = 24m;

    public async Task<IReadOnlyList<WorkOrderTaskLineResponse>> ListTasksAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        await EnsureWorkOrderExistsAsync(tenantId, workOrderId, cancellationToken);

        return await db.WorkOrderTaskLines
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .Select(x => MapTaskResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkOrderTaskLineResponse> CreateTaskAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        CreateWorkOrderTaskLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await GetEditableWorkOrderAsync(tenantId, workOrderId, cancellationToken);
        var title = NormalizeTaskTitle(request.Title);
        var description = request.Description?.Trim() ?? string.Empty;
        if (description.Length > 1024)
        {
            throw new StlApiException(
                "work_order_task.description_too_long",
                "Task description must be 1024 characters or fewer.",
                400);
        }

        var sortOrder = request.SortOrder ?? await GetNextTaskSortOrderAsync(tenantId, workOrderId, cancellationToken);
        if (sortOrder < 0)
        {
            throw new StlApiException("work_order_task.invalid_sort", "Sort order must be zero or greater.", 400);
        }

        var entity = new WorkOrderTaskLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WorkOrderId = workOrderId,
            Title = title,
            Description = description,
            SortOrder = sortOrder,
            Status = WorkOrderTaskStatuses.Pending,
            CreatedByUserId = actorUserId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.WorkOrderTaskLines.Add(entity);
        workOrder.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "work_order_task.create",
            tenantId,
            actorUserId,
            "work_order_task",
            entity.Id.ToString(),
            workOrderId.ToString(),
            cancellationToken: cancellationToken);

        return MapTaskResponse(entity);
    }

    public async Task<IReadOnlyList<WorkOrderLaborEntryResponse>> ListLaborAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        await EnsureWorkOrderExistsAsync(tenantId, workOrderId, cancellationToken);

        return await db.WorkOrderLaborEntries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId)
            .OrderByDescending(x => x.LoggedAt)
            .Select(x => MapLaborResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkOrderLaborEntryResponse> LogLaborAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        CreateWorkOrderLaborEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await GetEditableWorkOrderAsync(tenantId, workOrderId, cancellationToken);
        var personId = NormalizePersonId(request.PersonId);
        var laborTypeKey = NormalizeLaborTypeKey(request.LaborTypeKey);
        var hoursWorked = ValidateHoursWorked(request.HoursWorked);
        var notes = NormalizeNotes(request.Notes);

        if (request.WorkOrderTaskLineId.HasValue)
        {
            await EnsureTaskBelongsToWorkOrderAsync(
                tenantId,
                workOrderId,
                request.WorkOrderTaskLineId.Value,
                cancellationToken);
        }

        var entity = new WorkOrderLaborEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WorkOrderId = workOrderId,
            WorkOrderTaskLineId = request.WorkOrderTaskLineId,
            PersonId = personId,
            HoursWorked = hoursWorked,
            LaborTypeKey = laborTypeKey,
            Notes = notes,
            LoggedByUserId = actorUserId,
            LoggedAt = DateTimeOffset.UtcNow,
        };

        db.WorkOrderLaborEntries.Add(entity);
        workOrder.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "work_order_labor.log",
            tenantId,
            actorUserId,
            "work_order_labor",
            entity.Id.ToString(),
            workOrderId.ToString(),
            cancellationToken: cancellationToken);

        await technicianRefService.UpsertFromAssignmentAsync(
            tenantId,
            actorUserId,
            personId,
            null,
            cancellationToken);

        return MapLaborResponse(entity);
    }

    public async Task<IReadOnlyList<WorkOrderEvidenceResponse>> ListEvidenceAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        await EnsureWorkOrderExistsAsync(tenantId, workOrderId, cancellationToken);

        return await db.WorkOrderEvidence
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapEvidenceResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkOrderEvidenceResponse> UploadEvidenceAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        CreateWorkOrderEvidenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await GetEditableWorkOrderAsync(tenantId, workOrderId, cancellationToken);
        var evidenceTypeKey = NormalizeEvidenceTypeKey(request.EvidenceTypeKey);
        var fileName = NormalizeFileName(request.FileName);
        var contentType = NormalizeContentType(request.ContentType);
        var notes = NormalizeNotes(request.Notes);
        var contentBytes = DecodeContent(request.ContentBase64);

        if (contentBytes.Length == 0)
        {
            throw new StlApiException("evidence.validation", "Evidence content is required.", 400);
        }

        if (contentBytes.Length > MaxEvidenceBytes)
        {
            throw new StlApiException(
                "evidence.validation",
                $"Evidence file must be {MaxEvidenceBytes / (1024 * 1024)} MB or smaller.",
                400);
        }

        var evidenceId = Guid.NewGuid();
        await using var contentStream = new MemoryStream(contentBytes);
        var storageKey = await storage.SaveWorkOrderEvidenceAsync(
            tenantId,
            workOrderId,
            evidenceId,
            fileName,
            contentStream,
            cancellationToken);

        var entity = new WorkOrderEvidence
        {
            Id = evidenceId,
            TenantId = tenantId,
            WorkOrderId = workOrderId,
            EvidenceTypeKey = evidenceTypeKey,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = contentBytes.Length,
            StorageKey = storageKey,
            Notes = notes,
            UploadedByUserId = actorUserId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.WorkOrderEvidence.Add(entity);
        workOrder.UpdatedAt = DateTimeOffset.UtcNow;

        if (string.Equals(workOrder.Status, WorkOrderStatuses.Open, StringComparison.OrdinalIgnoreCase))
        {
            workOrder.Status = WorkOrderStatuses.InProgress;
            workOrder.StartedAt ??= DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "work_order_evidence.create",
            tenantId,
            actorUserId,
            "work_order_evidence",
            entity.Id.ToString(),
            workOrderId.ToString(),
            cancellationToken: cancellationToken);

        return MapEvidenceResponse(entity);
    }

    private async Task EnsureWorkOrderExistsAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken)
    {
        var exists = await db.WorkOrders.AnyAsync(
            x => x.TenantId == tenantId && x.Id == workOrderId,
            cancellationToken);

        if (!exists)
        {
            throw new StlApiException("work_order.not_found", "Work order was not found.", 404);
        }
    }

    private async Task<WorkOrder> GetEditableWorkOrderAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken)
    {
        var workOrder = await db.WorkOrders.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == workOrderId,
            cancellationToken);

        if (workOrder is null)
        {
            throw new StlApiException("work_order.not_found", "Work order was not found.", 404);
        }

        if (!WorkOrderStatuses.Active.Contains(workOrder.Status))
        {
            throw new StlApiException(
                "work_order.not_editable",
                "Tasks, labor, and evidence can only be added to open or in-progress work orders.",
                409);
        }

        return workOrder;
    }

    private async Task EnsureTaskBelongsToWorkOrderAsync(
        Guid tenantId,
        Guid workOrderId,
        Guid taskLineId,
        CancellationToken cancellationToken)
    {
        var exists = await db.WorkOrderTaskLines.AnyAsync(
            x => x.TenantId == tenantId && x.WorkOrderId == workOrderId && x.Id == taskLineId,
            cancellationToken);

        if (!exists)
        {
            throw new StlApiException("work_order_task.not_found", "Work order task line was not found.", 404);
        }
    }

    private async Task<int> GetNextTaskSortOrderAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken)
    {
        var maxSort = await db.WorkOrderTaskLines
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync(cancellationToken);

        return (maxSort ?? -1) + 1;
    }

    private static WorkOrderTaskLineResponse MapTaskResponse(WorkOrderTaskLine entity) =>
        new(
            entity.Id,
            entity.WorkOrderId,
            entity.Title,
            entity.Description,
            entity.SortOrder,
            entity.Status,
            entity.CreatedByUserId,
            entity.CreatedAt,
            entity.CompletedAt);

    private static WorkOrderLaborEntryResponse MapLaborResponse(WorkOrderLaborEntry entity) =>
        new(
            entity.Id,
            entity.WorkOrderId,
            entity.WorkOrderTaskLineId,
            entity.PersonId,
            entity.HoursWorked,
            entity.LaborTypeKey,
            entity.Notes,
            entity.LoggedByUserId,
            entity.LoggedAt);

    private static WorkOrderEvidenceResponse MapEvidenceResponse(WorkOrderEvidence entity) =>
        new(
            entity.Id,
            entity.WorkOrderId,
            entity.EvidenceTypeKey,
            entity.FileName,
            entity.ContentType,
            entity.SizeBytes,
            entity.Notes,
            entity.UploadedByUserId,
            entity.CreatedAt);

    private static string NormalizeTaskTitle(string title)
    {
        var trimmed = title.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new StlApiException("work_order_task.title_required", "Task title is required.", 400);
        }

        if (trimmed.Length > 256)
        {
            throw new StlApiException(
                "work_order_task.title_too_long",
                "Task title must be 256 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string NormalizePersonId(string personId)
    {
        var trimmed = personId.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new StlApiException("work_order_labor.person_required", "Person id is required.", 400);
        }

        if (trimmed.Length > 128)
        {
            throw new StlApiException(
                "work_order_labor.person_too_long",
                "Person id must be 128 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeLaborTypeKey(string laborTypeKey)
    {
        var normalized = laborTypeKey.Trim().ToLowerInvariant();
        if (!WorkOrderLaborTypes.All.Contains(normalized))
        {
            throw new StlApiException(
                "work_order_labor.invalid_type",
                "Labor type must be regular, overtime, or travel.",
                400);
        }

        return normalized;
    }

    private static decimal ValidateHoursWorked(decimal hoursWorked)
    {
        if (hoursWorked <= 0 || hoursWorked > MaxLaborHours)
        {
            throw new StlApiException(
                "work_order_labor.invalid_hours",
                $"Hours worked must be greater than zero and at most {MaxLaborHours}.",
                400);
        }

        return decimal.Round(hoursWorked, 2, MidpointRounding.AwayFromZero);
    }

    private static byte[] DecodeContent(string contentBase64)
    {
        if (string.IsNullOrWhiteSpace(contentBase64))
        {
            return [];
        }

        var payload = contentBase64.Trim();
        var commaIndex = payload.IndexOf(',');
        if (commaIndex >= 0 && payload[..commaIndex].Contains("base64", StringComparison.OrdinalIgnoreCase))
        {
            payload = payload[(commaIndex + 1)..];
        }

        try
        {
            return Convert.FromBase64String(payload);
        }
        catch (FormatException)
        {
            throw new StlApiException("evidence.validation", "Evidence content must be valid base64.", 400);
        }
    }

    private static string NormalizeEvidenceTypeKey(string evidenceTypeKey)
    {
        var normalized = evidenceTypeKey.Trim().ToLowerInvariant();
        if (normalized.Length < 3 || normalized.Length > 64)
        {
            throw new StlApiException(
                "evidence.validation",
                "Evidence type key must be between 3 and 64 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeFileName(string fileName)
    {
        var trimmed = fileName.Trim();
        if (trimmed.Length < 1 || trimmed.Length > 255)
        {
            throw new StlApiException(
                "evidence.validation",
                "File name must be between 1 and 255 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeContentType(string contentType)
    {
        var trimmed = contentType.Trim().ToLowerInvariant();
        if (trimmed.Length < 3 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "evidence.validation",
                "Content type must be between 3 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string? NormalizeNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        var trimmed = notes.Trim();
        if (trimmed.Length > 1024)
        {
            throw new StlApiException("evidence.validation", "Notes must be 1024 characters or fewer.", 400);
        }

        return trimmed;
    }
}
