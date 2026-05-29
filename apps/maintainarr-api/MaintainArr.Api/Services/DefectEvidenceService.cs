using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class DefectEvidenceService(
    MaintainArrDbContext db,
    MaintainArrEvidenceStorageService storage,
    IMaintainArrAuditService audit)
{
    private const long MaxEvidenceBytes = 10 * 1024 * 1024;

    public async Task<IReadOnlyList<DefectEvidenceResponse>> ListDefectEvidenceAsync(
        Guid tenantId,
        Guid defectId,
        CancellationToken cancellationToken = default)
    {
        await EnsureDefectExistsAsync(tenantId, defectId, cancellationToken);

        return await db.DefectEvidence
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DefectId == defectId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapDefectEvidenceResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<DefectEvidenceResponse> UploadDefectEvidenceAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid defectId,
        CreateMaintainArrEvidenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var defect = await GetEditableDefectAsync(tenantId, defectId, cancellationToken);
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
        var storageKey = await storage.SaveDefectEvidenceAsync(
            tenantId,
            defectId,
            evidenceId,
            fileName,
            contentStream,
            cancellationToken);

        var entity = new DefectEvidence
        {
            Id = evidenceId,
            TenantId = tenantId,
            DefectId = defectId,
            EvidenceTypeKey = evidenceTypeKey,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = contentBytes.Length,
            StorageKey = storageKey,
            Notes = notes,
            UploadedByUserId = actorUserId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.DefectEvidence.Add(entity);
        defect.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "defect_evidence.create",
            tenantId,
            actorUserId,
            "defect_evidence",
            entity.Id.ToString(),
            defectId.ToString(),
            cancellationToken: cancellationToken);

        return MapDefectEvidenceResponse(entity);
    }

    public async Task<IReadOnlyList<InspectionRunEvidenceResponse>> ListInspectionRunEvidenceAsync(
        Guid tenantId,
        Guid inspectionRunId,
        CancellationToken cancellationToken = default)
    {
        await EnsureInspectionRunExistsAsync(tenantId, inspectionRunId, cancellationToken);

        return await db.InspectionRunEvidence
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.InspectionRunId == inspectionRunId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapInspectionRunEvidenceResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<InspectionRunEvidenceResponse> UploadInspectionRunEvidenceAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid inspectionRunId,
        CreateMaintainArrEvidenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var run = await GetEditableInspectionRunAsync(tenantId, inspectionRunId, cancellationToken);
        if (request.ChecklistItemId.HasValue)
        {
            await EnsureChecklistItemBelongsToRunAsync(
                tenantId,
                inspectionRunId,
                request.ChecklistItemId.Value,
                cancellationToken);
        }

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
        var storageKey = await storage.SaveInspectionRunEvidenceAsync(
            tenantId,
            inspectionRunId,
            evidenceId,
            fileName,
            contentStream,
            cancellationToken);

        var entity = new InspectionRunEvidence
        {
            Id = evidenceId,
            TenantId = tenantId,
            InspectionRunId = inspectionRunId,
            ChecklistItemId = request.ChecklistItemId,
            EvidenceTypeKey = evidenceTypeKey,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = contentBytes.Length,
            StorageKey = storageKey,
            Notes = notes,
            UploadedByUserId = actorUserId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.InspectionRunEvidence.Add(entity);
        run.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "inspection_run_evidence.create",
            tenantId,
            actorUserId,
            "inspection_run_evidence",
            entity.Id.ToString(),
            inspectionRunId.ToString(),
            cancellationToken: cancellationToken);

        return MapInspectionRunEvidenceResponse(entity);
    }

    private async Task EnsureDefectExistsAsync(
        Guid tenantId,
        Guid defectId,
        CancellationToken cancellationToken)
    {
        var exists = await db.Defects.AnyAsync(
            x => x.TenantId == tenantId && x.Id == defectId,
            cancellationToken);

        if (!exists)
        {
            throw new StlApiException("defect.not_found", "Defect was not found.", 404);
        }
    }

    private async Task<Defect> GetEditableDefectAsync(
        Guid tenantId,
        Guid defectId,
        CancellationToken cancellationToken)
    {
        var defect = await db.Defects.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == defectId,
            cancellationToken);

        if (defect is null)
        {
            throw new StlApiException("defect.not_found", "Defect was not found.", 404);
        }

        if (string.Equals(defect.Status, DefectStatuses.Closed, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(defect.Status, DefectStatuses.Resolved, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "defect_evidence.not_editable",
                "Evidence cannot be added to resolved or closed defects.",
                409);
        }

        return defect;
    }

    private async Task EnsureInspectionRunExistsAsync(
        Guid tenantId,
        Guid inspectionRunId,
        CancellationToken cancellationToken)
    {
        var exists = await db.InspectionRuns.AnyAsync(
            x => x.TenantId == tenantId && x.Id == inspectionRunId,
            cancellationToken);

        if (!exists)
        {
            throw new StlApiException("inspection_run.not_found", "Inspection run was not found.", 404);
        }
    }

    private async Task<InspectionRun> GetEditableInspectionRunAsync(
        Guid tenantId,
        Guid inspectionRunId,
        CancellationToken cancellationToken)
    {
        var run = await db.InspectionRuns.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == inspectionRunId,
            cancellationToken);

        if (run is null)
        {
            throw new StlApiException("inspection_run.not_found", "Inspection run was not found.", 404);
        }

        if (!string.Equals(run.Status, InspectionRunStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "inspection_run_evidence.not_editable",
                "Evidence can only be added to in-progress inspection runs.",
                409);
        }

        return run;
    }

    private async Task EnsureChecklistItemBelongsToRunAsync(
        Guid tenantId,
        Guid inspectionRunId,
        Guid checklistItemId,
        CancellationToken cancellationToken)
    {
        var run = await db.InspectionRuns
            .AsNoTracking()
            .FirstAsync(x => x.TenantId == tenantId && x.Id == inspectionRunId, cancellationToken);

        var belongs = await db.InspectionChecklistItems.AnyAsync(
            x => x.TenantId == tenantId &&
                 x.Id == checklistItemId &&
                 x.InspectionTemplateId == run.InspectionTemplateId,
            cancellationToken);

        if (!belongs)
        {
            throw new StlApiException(
                "inspection_run_evidence.invalid_checklist_item",
                "Checklist item does not belong to this inspection run template.",
                400);
        }
    }

    private static DefectEvidenceResponse MapDefectEvidenceResponse(DefectEvidence entity) =>
        new(
            entity.Id,
            entity.DefectId,
            entity.EvidenceTypeKey,
            entity.FileName,
            entity.ContentType,
            entity.SizeBytes,
            entity.Notes,
            entity.UploadedByUserId,
            entity.CreatedAt);

    private static InspectionRunEvidenceResponse MapInspectionRunEvidenceResponse(InspectionRunEvidence entity) =>
        new(
            entity.Id,
            entity.InspectionRunId,
            entity.ChecklistItemId,
            entity.EvidenceTypeKey,
            entity.FileName,
            entity.ContentType,
            entity.SizeBytes,
            entity.Notes,
            entity.UploadedByUserId,
            entity.CreatedAt);

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
