using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class StaffarrIncidentRemediationService(
    TrainArrDbContext db,
    ITrainArrAuditService audit)
{
    private static readonly HashSet<string> AllowedReasonCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "training_compliance"
    };

    private static readonly HashSet<string> AllowedSeverities = new(StringComparer.OrdinalIgnoreCase)
    {
        "low",
        "medium",
        "high",
        "critical"
    };

    public async Task<StaffarrIncidentRemediationResponse> IngestAsync(
        IngestStaffarrIncidentRemediationRequest request,
        CancellationToken cancellationToken = default)
    {
        var reasonCategoryKey = NormalizeReasonCategoryKey(request.ReasonCategoryKey);
        var severity = NormalizeSeverity(request.Severity);
        var title = NormalizeTitle(request.Title);
        var description = NormalizeDescription(request.Description);

        var existing = await db.StaffarrIncidentRemediations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == request.TenantId && x.StaffarrIncidentId == request.StaffarrIncidentId,
                cancellationToken);

        if (existing is not null)
        {
            return MapResponse(existing);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new StaffarrIncidentRemediation
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            StaffarrIncidentId = request.StaffarrIncidentId,
            StaffarrPersonId = request.StaffarrPersonId,
            ReasonCategoryKey = reasonCategoryKey,
            Severity = severity,
            Title = title,
            Description = description,
            OccurredAt = request.OccurredAt,
            ReportedAt = request.ReportedAt,
            Status = "intake_received",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.StaffarrIncidentRemediations.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "incident_remediation.intake",
            request.TenantId,
            null,
            "staffarr_incident_remediation",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    private static StaffarrIncidentRemediationResponse MapResponse(StaffarrIncidentRemediation entity) =>
        new(
            entity.Id,
            entity.TenantId,
            entity.StaffarrIncidentId,
            entity.StaffarrPersonId,
            entity.ReasonCategoryKey,
            entity.Status,
            entity.CreatedAt);

    private static string NormalizeReasonCategoryKey(string reasonCategoryKey)
    {
        var normalized = reasonCategoryKey.Trim().ToLowerInvariant();
        if (!AllowedReasonCategories.Contains(normalized))
        {
            throw new StlApiException(
                "incident_remediations.validation",
                $"Reason category must be one of: {string.Join(", ", AllowedReasonCategories.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeSeverity(string severity)
    {
        var normalized = severity.Trim().ToLowerInvariant();
        if (!AllowedSeverities.Contains(normalized))
        {
            throw new StlApiException(
                "incident_remediations.validation",
                $"Severity must be one of: {string.Join(", ", AllowedSeverities.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeTitle(string title)
    {
        var trimmed = title.Trim();
        if (trimmed.Length < 4 || trimmed.Length > 200)
        {
            throw new StlApiException(
                "incident_remediations.validation",
                "Incident title must be between 4 and 200 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeDescription(string description)
    {
        var trimmed = description.Trim();
        if (trimmed.Length < 16 || trimmed.Length > 4096)
        {
            throw new StlApiException(
                "incident_remediations.validation",
                "Incident description must be between 16 and 4096 characters.",
                400);
        }

        return trimmed;
    }
}
