using System.Text.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class WorkOrderDiscussionService(MaintainArrDbContext db, IMaintainArrAuditService audit)
{
    public async Task<IReadOnlyList<WorkOrderCommentResponse>> ListCommentsAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        await EnsureWorkOrderExistsAsync(tenantId, workOrderId, cancellationToken);

        return await db.WorkOrderComments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId)
            .OrderByDescending(x => x.Pinned)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => MapCommentResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkOrderCommentResponse> AddCommentAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid workOrderId,
        CreateWorkOrderCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await db.WorkOrders
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrderId, cancellationToken);

        if (workOrder is null)
        {
            throw new StlApiException("work_order.not_found", "Work order was not found.", 404);
        }

        var body = NormalizeBody(request.Body);
        var visibility = NormalizeVisibility(request.Visibility);
        var pinned = request.Pinned ?? false;
        var now = DateTimeOffset.UtcNow;

        var entity = new WorkOrderComment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WorkOrderId = workOrderId,
            Body = body,
            Visibility = visibility,
            CreatedAt = now,
            CreatedByPersonId = NormalizePersonId(actorPersonId),
            Pinned = pinned,
        };

        db.WorkOrderComments.Add(entity);
        workOrder.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "work_order_comment.create",
            tenantId,
            actorUserId,
            "work_order_comment",
            entity.Id.ToString(),
            workOrderId.ToString(),
            cancellationToken: cancellationToken);

        await RecordTimelineEventAsync(
            tenantId,
            workOrderId,
            "maintainarr.work_order.comment_added",
            now,
            actorPersonId,
            null,
            $"Comment added to work order {workOrder.WorkOrderNumber}.",
            "maintainarr",
            entity.Id.ToString("D"),
            null,
            JsonSerializer.Serialize(new
            {
                commentId = entity.Id,
                entity.WorkOrderId,
                entity.Body,
                entity.Visibility,
                entity.Pinned,
                entity.CreatedAt,
                entity.CreatedByPersonId,
            }),
            cancellationToken);

        return MapCommentResponse(entity);
    }

    public async Task<IReadOnlyList<WorkOrderTimelineEventResponse>> ListTimelineAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        await EnsureWorkOrderExistsAsync(tenantId, workOrderId, cancellationToken);

        return await db.WorkOrderTimelineEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId)
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id)
            .Select(x => MapTimelineResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task RecordTimelineEventAsync(
        Guid tenantId,
        Guid workOrderId,
        string eventType,
        DateTimeOffset occurredAt,
        string? actorPersonId,
        string? actorServiceClientId,
        string summary,
        string? sourceProduct,
        string? sourceObjectRef,
        string? beforeSnapshot,
        string? afterSnapshot,
        CancellationToken cancellationToken = default)
    {
        var exists = await db.WorkOrders.AnyAsync(
            x => x.TenantId == tenantId && x.Id == workOrderId,
            cancellationToken);

        if (!exists)
        {
            throw new StlApiException("work_order.not_found", "Work order was not found.", 404);
        }

        var entity = new WorkOrderTimelineEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WorkOrderId = workOrderId,
            EventType = eventType,
            OccurredAt = occurredAt,
            ActorPersonId = NormalizePersonId(actorPersonId),
            ActorServiceClientId = NormalizeOptional(actorServiceClientId),
            Summary = summary,
            SourceProduct = NormalizeOptional(sourceProduct),
            SourceObjectRef = NormalizeOptional(sourceObjectRef),
            BeforeSnapshot = NormalizeOptional(beforeSnapshot),
            AfterSnapshot = NormalizeOptional(afterSnapshot),
        };

        db.WorkOrderTimelineEvents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
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

    private static WorkOrderCommentResponse MapCommentResponse(WorkOrderComment entity) =>
        new(
            entity.Id,
            entity.WorkOrderId,
            entity.Body,
            entity.Visibility,
            entity.CreatedAt,
            entity.CreatedByPersonId,
            entity.EditedAt,
            entity.EditedByPersonId,
            entity.Pinned);

    private static WorkOrderTimelineEventResponse MapTimelineResponse(WorkOrderTimelineEvent entity) =>
        new(
            entity.Id,
            entity.WorkOrderId,
            entity.EventType,
            entity.OccurredAt,
            entity.ActorPersonId,
            entity.ActorServiceClientId,
            entity.Summary,
            entity.SourceProduct,
            entity.SourceObjectRef,
            entity.BeforeSnapshot,
            entity.AfterSnapshot);

    private static string NormalizeBody(string body)
    {
        var trimmed = body.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new StlApiException("work_order.comment_required", "Comment body is required.", 400);
        }

        if (trimmed.Length > 2000)
        {
            throw new StlApiException("work_order.comment_too_long", "Comment body must be 2000 characters or fewer.", 400);
        }

        return trimmed;
    }

    private static string NormalizeVisibility(string? visibility)
    {
        var normalized = string.IsNullOrWhiteSpace(visibility)
            ? WorkOrderCommentVisibility.Internal
            : visibility.Trim().ToLowerInvariant();

        if (!WorkOrderCommentVisibility.All.Contains(normalized))
        {
            throw new StlApiException(
                "work_order.comment_visibility_invalid",
                "Comment visibility must be internal, supervisor_only, auditor_visible, or vendor_visible.",
                400);
        }

        return normalized;
    }

    private static string? NormalizePersonId(string? personId)
    {
        if (string.IsNullOrWhiteSpace(personId))
        {
            return null;
        }

        var trimmed = personId.Trim();
        return trimmed.Length > 128 ? throw new StlApiException("work_order.person_id_too_long", "Person id must be 128 characters or fewer.", 400) : trimmed;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
