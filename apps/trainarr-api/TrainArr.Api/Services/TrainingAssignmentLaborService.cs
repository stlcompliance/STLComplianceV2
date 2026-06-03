using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class TrainingAssignmentLaborService(
    TrainArrDbContext db,
    ITrainArrAuditService audit)
{
    public async Task<IReadOnlyList<TrainingAssignmentLaborEntryResponse>> ListAsync(
        Guid tenantId,
        Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        await GetAssignmentAsync(tenantId, assignmentId, cancellationToken);

        return await db.TrainingAssignmentLaborEntries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TrainingAssignmentId == assignmentId)
            .OrderByDescending(x => x.LoggedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new TrainingAssignmentLaborEntryResponse(
                x.Id,
                x.TrainingAssignmentId,
                x.LaborTypeKey,
                x.HoursWorked,
                x.CostPerHour,
                Math.Round(x.HoursWorked * x.CostPerHour, 2),
                x.Notes,
                x.LoggedByUserId,
                x.LoggedAt,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<TrainingAssignmentLaborEntryResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assignmentId,
        CreateTrainingAssignmentLaborEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentAsync(tenantId, assignmentId, cancellationToken);

        var laborTypeKey = NormalizeLaborTypeKey(request.LaborTypeKey);
        var hoursWorked = NormalizeHoursWorked(request.HoursWorked);
        var costPerHour = NormalizeCostPerHour(request.CostPerHour);
        var notes = NormalizeNotes(request.Notes);
        var now = DateTimeOffset.UtcNow;
        var entity = new TrainingAssignmentLaborEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrainingAssignmentId = assignmentId,
            LaborTypeKey = laborTypeKey,
            HoursWorked = hoursWorked,
            CostPerHour = costPerHour,
            Notes = notes,
            LoggedByUserId = actorUserId,
            LoggedAt = now,
            CreatedAt = now,
        };

        db.TrainingAssignmentLaborEntries.Add(entity);
        assignment.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "training_assignment_labor.create",
            tenantId,
            actorUserId,
            "training_assignment_labor",
            entity.Id.ToString(),
            assignmentId.ToString(),
            cancellationToken: cancellationToken);

        return new TrainingAssignmentLaborEntryResponse(
            entity.Id,
            entity.TrainingAssignmentId,
            entity.LaborTypeKey,
            entity.HoursWorked,
            entity.CostPerHour,
            Math.Round(entity.HoursWorked * entity.CostPerHour, 2),
            entity.Notes,
            entity.LoggedByUserId,
            entity.LoggedAt,
            entity.CreatedAt);
    }

    public async Task RemoveAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assignmentId,
        Guid laborEntryId,
        CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentAsync(tenantId, assignmentId, cancellationToken);

        var entity = await db.TrainingAssignmentLaborEntries.FirstOrDefaultAsync(
            x => x.TenantId == tenantId
                && x.TrainingAssignmentId == assignmentId
                && x.Id == laborEntryId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException(
                "training_assignment_labor.not_found",
                "Training assignment labor entry was not found.",
                404);
        }

        db.TrainingAssignmentLaborEntries.Remove(entity);
        assignment.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "training_assignment_labor.remove",
            tenantId,
            actorUserId,
            "training_assignment_labor",
            laborEntryId.ToString(),
            assignmentId.ToString(),
            cancellationToken: cancellationToken);
    }

    private async Task<TrainingAssignment> GetAssignmentAsync(
        Guid tenantId,
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var assignment = await db.TrainingAssignments.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == assignmentId,
            cancellationToken);
        if (assignment is null)
        {
            throw new StlApiException(
                "training_assignments.not_found",
                "Training assignment was not found.",
                404);
        }

        return assignment;
    }

    private static string NormalizeLaborTypeKey(string laborTypeKey)
    {
        var normalized = laborTypeKey.Trim().ToLowerInvariant();
        if (!TrainingAssignmentLaborTypeSet.Allowed.Contains(normalized))
        {
            throw new StlApiException(
                "training_assignment_labor.validation",
                $"Labor type must be one of: {string.Join(", ", TrainingAssignmentLaborTypeSet.Allowed.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static decimal NormalizeHoursWorked(decimal hoursWorked)
    {
        if (hoursWorked <= 0)
        {
            throw new StlApiException(
                "training_assignment_labor.validation",
                "Hours worked must be greater than zero.",
                400);
        }

        return Math.Round(hoursWorked, 2);
    }

    private static decimal NormalizeCostPerHour(decimal costPerHour)
    {
        if (costPerHour < 0)
        {
            throw new StlApiException(
                "training_assignment_labor.validation",
                "Cost per hour cannot be negative.",
                400);
        }

        return Math.Round(costPerHour, 2);
    }

    private static string? NormalizeNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        var trimmed = notes.Trim();
        return trimmed.Length > 1024 ? trimmed[..1024] : trimmed;
    }
}

public static class TrainingAssignmentLaborTypeSet
{
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        TrainingAssignmentLaborTypes.Delivery,
        TrainingAssignmentLaborTypes.Preparation,
        TrainingAssignmentLaborTypes.Review,
        TrainingAssignmentLaborTypes.Administration,
        TrainingAssignmentLaborTypes.Travel,
    };
}
