using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LoadArr.Api.Data;
using LoadArr.Api.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace LoadArr.Api.Services;

public sealed class LoadArrOperationalWorkflowStore(LoadArrDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<LoadArrReceivingSessionResponse?> GetReceivingSessionAsync(
        Guid tenantId,
        string sessionId,
        CancellationToken cancellationToken)
    {
        var entity = await db.LoadArrReceivingSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId
                    && string.Equals(x.SessionId, sessionId, StringComparison.OrdinalIgnoreCase),
                cancellationToken);

        return entity is null ? null : DeserializeReceivingSession(entity.PayloadJson);
    }

    public async Task<(LoadArrReceivingSessionResponse? Session, string? RequestFingerprint)> GetReceivingSessionByClientRequestIdAsync(
        Guid tenantId,
        string clientRequestId,
        CancellationToken cancellationToken)
    {
        var entity = await db.LoadArrReceivingSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.ClientRequestId != null
                    && string.Equals(x.ClientRequestId, clientRequestId, StringComparison.OrdinalIgnoreCase),
                cancellationToken);

        return entity is null
            ? (null, null)
            : (DeserializeReceivingSession(entity.PayloadJson), entity.RequestFingerprint);
    }

    public async Task<IReadOnlyCollection<LoadArrReceivingSessionResponse>> ListReceivingSessionsAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var entities = await db.LoadArrReceivingSessions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.StartedAtUtc)
            .ToListAsync(cancellationToken);

        return entities
            .Select(entity => DeserializeReceivingSession(entity.PayloadJson))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<LoadArrInventoryBalanceResponse>> ListInventoryBalancesAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var entities = await db.LoadArrInventoryBalances
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.LocationId)
            .ThenBy(x => x.SupplyarrItemId)
            .ThenBy(x => x.LotCode)
            .ThenBy(x => x.SerialCode)
            .ToListAsync(cancellationToken);

        return entities
            .Select(entity => DeserializeInventoryBalance(entity.PayloadJson))
            .ToArray();
    }

    public async Task<LoadArrInventoryBalanceResponse?> GetInventoryBalanceAsync(
        Guid tenantId,
        string balanceId,
        CancellationToken cancellationToken)
    {
        var entity = await db.LoadArrInventoryBalances
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId
                    && string.Equals(x.BalanceId, balanceId, StringComparison.OrdinalIgnoreCase),
                cancellationToken);

        return entity is null ? null : DeserializeInventoryBalance(entity.PayloadJson);
    }

    public async Task<IReadOnlyCollection<LoadArrWarehouseTaskResponse>> ListWarehouseTasksAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var entities = await db.LoadArrWarehouseTasks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.DueAtUtc)
            .ThenBy(x => x.TaskId)
            .ToListAsync(cancellationToken);

        return entities
            .Select(entity => DeserializeWarehouseTask(entity.PayloadJson))
            .ToArray();
    }

    public async Task<LoadArrWarehouseTaskResponse?> GetWarehouseTaskAsync(
        Guid tenantId,
        string taskId,
        CancellationToken cancellationToken)
    {
        var entity = await db.LoadArrWarehouseTasks
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId
                    && string.Equals(x.TaskId, taskId, StringComparison.OrdinalIgnoreCase),
                cancellationToken);

        return entity is null ? null : DeserializeWarehouseTask(entity.PayloadJson);
    }

    public async Task<IReadOnlyCollection<LoadArrInventoryMovementResponse>> ListInventoryMovementsAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var entities = await db.LoadArrInventoryMovements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities
            .Select(entity => DeserializeInventoryMovement(entity.PayloadJson))
            .ToArray();
    }

    public async Task<LoadArrInventoryMovementResponse?> GetInventoryMovementAsync(
        Guid tenantId,
        string movementId,
        CancellationToken cancellationToken)
    {
        var entity = await db.LoadArrInventoryMovements
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId
                    && string.Equals(x.MovementId, movementId, StringComparison.OrdinalIgnoreCase),
                cancellationToken);

        return entity is null ? null : DeserializeInventoryMovement(entity.PayloadJson);
    }

    public async Task SaveReceivingSessionAsync(
        Guid tenantId,
        LoadArrReceivingSessionResponse session,
        string? clientRequestId = null,
        string? requestFingerprint = null,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.LoadArrReceivingSessions.SingleOrDefaultAsync(
            x => x.TenantId == tenantId
                && string.Equals(x.SessionId, session.Id, StringComparison.OrdinalIgnoreCase),
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new LoadArrReceivingSessionEntity
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SessionId = session.Id,
                CreatedAtUtc = now
            };
            db.LoadArrReceivingSessions.Add(entity);
        }

        entity.ReceivingNumber = session.ReceivingNumber;
        entity.ReceivingType = session.ReceivingType;
        entity.Status = session.Status;
        entity.ClientRequestId = clientRequestId ?? entity.ClientRequestId;
        entity.RequestFingerprint = requestFingerprint ?? entity.RequestFingerprint;
        entity.CompletedByPersonId = session.CompletedByPersonId;
        entity.StartedAtUtc = ParseTimestamp(session.StartedAtUtc, nameof(session.StartedAtUtc));
        entity.CompletedAtUtc = string.IsNullOrWhiteSpace(session.CompletedAtUtc)
            ? null
            : ParseTimestamp(session.CompletedAtUtc, nameof(session.CompletedAtUtc));
        entity.PayloadJson = JsonSerializer.Serialize(session, JsonOptions);
        entity.UpdatedAtUtc = now;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(LoadArrTransferOrderResponse? Order, string? RequestFingerprint)> GetTransferOrderByClientRequestIdAsync(
        Guid tenantId,
        string clientRequestId,
        CancellationToken cancellationToken)
    {
        var entity = await db.LoadArrTransferOrders
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.ClientRequestId != null
                    && string.Equals(x.ClientRequestId, clientRequestId, StringComparison.OrdinalIgnoreCase),
                cancellationToken);

        return entity is null
            ? (null, null)
            : (DeserializeTransferOrder(entity.PayloadJson), entity.RequestFingerprint);
    }

    public async Task<LoadArrTransferOrderResponse?> GetTransferOrderAsync(
        Guid tenantId,
        string orderId,
        CancellationToken cancellationToken)
    {
        var entity = await db.LoadArrTransferOrders
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId
                    && string.Equals(x.OrderId, orderId, StringComparison.OrdinalIgnoreCase),
                cancellationToken);

        return entity is null ? null : DeserializeTransferOrder(entity.PayloadJson);
    }

    public async Task<IReadOnlyCollection<LoadArrTransferOrderResponse>> ListTransferOrdersAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var entities = await db.LoadArrTransferOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities
            .Select(entity => DeserializeTransferOrder(entity.PayloadJson))
            .ToArray();
    }

    public async Task SaveTransferOrderAsync(
        Guid tenantId,
        LoadArrTransferOrderResponse order,
        string? clientRequestId = null,
        string? requestFingerprint = null,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.LoadArrTransferOrders.SingleOrDefaultAsync(
            x => x.TenantId == tenantId
                && string.Equals(x.OrderId, order.Id, StringComparison.OrdinalIgnoreCase),
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new LoadArrTransferOrderEntity
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId
            };
            db.LoadArrTransferOrders.Add(entity);
        }

        entity.OrderId = order.Id;
        entity.TransferNumber = order.TransferNumber;
        entity.TransferType = order.TransferType;
        entity.Status = order.Status;
        entity.ClientRequestId = clientRequestId ?? entity.ClientRequestId;
        entity.RequestFingerprint = requestFingerprint ?? entity.RequestFingerprint;
        entity.CompletedByPersonId = order.CompletedByPersonId;
        entity.CreatedAtUtc = ParseTimestamp(order.CreatedAtUtc, nameof(order.CreatedAtUtc));
        entity.CompletedAtUtc = string.IsNullOrWhiteSpace(order.CompletedAtUtc)
            ? null
            : ParseTimestamp(order.CompletedAtUtc, nameof(order.CompletedAtUtc));
        entity.PayloadJson = JsonSerializer.Serialize(order, JsonOptions);
        entity.UpdatedAtUtc = now;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<LoadArrReceivingCompletionResponse?> GetReceivingCompletionAsync(
        Guid tenantId,
        string sessionId,
        CancellationToken cancellationToken)
    {
        var session = await GetReceivingSessionAsync(tenantId, sessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        var line = session.Lines.SingleOrDefault();
        if (line is null)
        {
            return null;
        }

        var movementEntity = await db.LoadArrInventoryMovements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && string.Equals(x.RelatedObjectType, "receiving_session", StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.RelatedObjectId, sessionId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        if (movementEntity is null)
        {
            return null;
        }

        var movement = DeserializeInventoryMovement(movementEntity.PayloadJson);
        if (string.IsNullOrWhiteSpace(movement.InventoryOriginEventId))
        {
            return null;
        }

        var originEntity = await db.LoadArrInventoryOriginEvents
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId
                    && string.Equals(x.OriginEventId, movement.InventoryOriginEventId, StringComparison.OrdinalIgnoreCase),
                cancellationToken);
        if (originEntity is null)
        {
            return null;
        }

        var taskEntity = await db.LoadArrWarehouseTasks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && string.Equals(x.SourceObjectType, "receiving_session", StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.SourceObjectId, sessionId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.TaskType, "putaway", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        if (taskEntity is null)
        {
            return null;
        }

        var balanceEntity = await db.LoadArrInventoryBalances
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId
                    && string.Equals(x.SupplyarrItemId, line.SupplyarrItemId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.LocationId, line.WarehouseLocationId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.LotCode, NormalizeBalanceKeyPart(line.LotCode), StringComparison.Ordinal)
                    && string.Equals(x.SerialCode, NormalizeBalanceKeyPart(line.SerialCode), StringComparison.Ordinal),
                cancellationToken);
        if (balanceEntity is null)
        {
            return null;
        }

        return new LoadArrReceivingCompletionResponse(
            session,
            DeserializeInventoryOriginEvent(originEntity.PayloadJson),
            movement,
            DeserializeInventoryBalance(balanceEntity.PayloadJson),
            DeserializeWarehouseTask(taskEntity.PayloadJson));
    }

    public async Task<LoadArrReceivingCompletionResponse> CompleteReceivingSessionAsync(
        Guid tenantId,
        LoadArrReceivingSessionResponse session,
        string completedByPersonId,
        string? complianceEvaluationId,
        string? evidenceSummary,
        CancellationToken cancellationToken)
    {
        var existingCompletion = await GetReceivingCompletionAsync(tenantId, session.Id, cancellationToken);
        if (existingCompletion is not null)
        {
            return existingCompletion;
        }

        var line = session.Lines.Single();
        var now = DateTimeOffset.UtcNow;
        var completedAtUtc = now.ToString("O");
        var sessionStatus = line.ReceivedQuantity < line.ExpectedQuantity ? "partial" : "completed";
        var lineStatus = sessionStatus == "partial" ? "partial" : "received";
        var finalEvidenceSummary = string.IsNullOrWhiteSpace(evidenceSummary)
            ? line.EvidenceSummary
            : evidenceSummary.Trim();
        var originEventId = CreateCompletionRecordId("origin", session.Id);
        var movementId = CreateCompletionRecordId("move", session.Id);
        var putawayTaskId = CreateCompletionRecordId("task", session.Id);

        var completedLine = line with
        {
            Status = lineStatus,
            EvidenceSummary = finalEvidenceSummary
        };
        var completedSession = session with
        {
            Status = sessionStatus,
            CompletedByPersonId = completedByPersonId,
            CompletedAtUtc = completedAtUtc,
            Lines = new[] { completedLine }
        };

        var originEvent = new LoadArrInventoryOriginEventResponse(
            originEventId,
            session.ReceivingType,
            session.SourceProductKey,
            session.SourceObjectType,
            session.SourceObjectId,
            session.StaffarrSiteOrgUnitId,
            session.StaffarrSiteNameSnapshot,
            completedLine.WarehouseLocationId,
            completedLine.LocationNameSnapshot,
            completedLine.SupplyarrItemId,
            completedLine.ItemNameSnapshot,
            completedLine.ReceivedQuantity,
            completedLine.UnitOfMeasure,
            completedLine.LotCode,
            completedLine.SerialCode,
            completedLine.Condition,
            sessionStatus,
            completedByPersonId,
            string.IsNullOrWhiteSpace(complianceEvaluationId) ? null : complianceEvaluationId.Trim(),
            finalEvidenceSummary,
            completedAtUtc);

        var movement = new LoadArrInventoryMovementResponse(
            movementId,
            "receive",
            session.StaffarrSiteOrgUnitId,
            null,
            completedLine.WarehouseLocationId,
            completedLine.SupplyarrItemId,
            completedLine.ItemNameSnapshot,
            completedLine.ReceivedQuantity,
            completedLine.UnitOfMeasure,
            "inbound",
            "available",
            "loadarr",
            "receiving_session",
            session.Id,
            "receiving_complete",
            completedByPersonId,
            originEvent.Id,
            completedAtUtc);

        var balanceEntity = await db.LoadArrInventoryBalances.SingleOrDefaultAsync(
            x => x.TenantId == tenantId
                && string.Equals(x.SupplyarrItemId, completedLine.SupplyarrItemId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.LocationId, completedLine.WarehouseLocationId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.LotCode, NormalizeBalanceKeyPart(completedLine.LotCode), StringComparison.Ordinal)
                && string.Equals(x.SerialCode, NormalizeBalanceKeyPart(completedLine.SerialCode), StringComparison.Ordinal),
            cancellationToken);

        var existingBalance = balanceEntity is null
            ? null
            : DeserializeInventoryBalance(balanceEntity.PayloadJson);
        var updatedTraceTags = new HashSet<string>(existingBalance?.TraceTags ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase)
        {
            $"receiving:{completedSession.ReceivingNumber}",
            $"origin:{originEvent.Id}"
        };
        if (!string.IsNullOrWhiteSpace(completedLine.LotCode))
        {
            updatedTraceTags.Add($"lot:{completedLine.LotCode.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(completedLine.SerialCode))
        {
            updatedTraceTags.Add($"serial:{completedLine.SerialCode.Trim()}");
        }

        var balance = new LoadArrInventoryBalanceResponse(
            balanceEntity?.BalanceId ?? $"bal-{Guid.NewGuid():N}"[..15],
            completedLine.SupplyarrItemId,
            completedLine.ItemNameSnapshot,
            completedLine.UnitOfMeasure,
            "available",
            completedLine.WarehouseLocationId,
            completedLine.LocationNameSnapshot,
            (existingBalance?.QuantityOnHand ?? 0m) + completedLine.ReceivedQuantity,
            existingBalance?.QuantityReserved ?? 0m,
            existingBalance?.QuantityAllocated ?? 0m,
            existingBalance?.QuantityBlocked ?? 0m,
            originEvent.OriginType,
            $"{originEvent.OriginObjectType}:{originEvent.OriginObjectId}",
            updatedTraceTags.OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase).ToArray(),
            $"Received through {completedSession.ReceivingNumber}.");

        var putawayTask = new LoadArrWarehouseTaskResponse(
            putawayTaskId,
            "putaway",
            $"Put away {completedLine.ItemNameSnapshot}",
            sessionStatus == "partial" ? "high" : "normal",
            "ready",
            completedLine.LocationNameSnapshot,
            "Warehouse Associate",
            completedLine.SupplyarrItemId,
            completedLine.ReceivedQuantity,
            now.AddHours(4).ToString("O"),
            new[] { "origin_event_created", "movement_recorded", "location_scan_required" });

        var sessionEntity = await db.LoadArrReceivingSessions.SingleAsync(
            x => x.TenantId == tenantId
                && string.Equals(x.SessionId, session.Id, StringComparison.OrdinalIgnoreCase),
            cancellationToken);
        sessionEntity.Status = completedSession.Status;
        sessionEntity.CompletedByPersonId = completedSession.CompletedByPersonId;
        sessionEntity.CompletedAtUtc = ParseTimestamp(completedSession.CompletedAtUtc, nameof(completedSession.CompletedAtUtc));
        sessionEntity.PayloadJson = JsonSerializer.Serialize(completedSession, JsonOptions);
        sessionEntity.UpdatedAtUtc = now;

        db.LoadArrInventoryOriginEvents.Add(new LoadArrInventoryOriginEventEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OriginEventId = originEvent.Id,
            OriginType = originEvent.OriginType,
            OriginProductKey = originEvent.OriginProductKey,
            OriginObjectType = originEvent.OriginObjectType,
            OriginObjectId = originEvent.OriginObjectId,
            WarehouseLocationId = originEvent.WarehouseLocationId,
            SupplyarrItemId = originEvent.SupplyarrItemId,
            PersonId = originEvent.PersonId,
            CreatedAtUtc = ParseTimestamp(originEvent.CreatedAtUtc, nameof(originEvent.CreatedAtUtc)),
            PayloadJson = JsonSerializer.Serialize(originEvent, JsonOptions)
        });

        db.LoadArrInventoryMovements.Add(new LoadArrInventoryMovementEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MovementId = movement.Id,
            MovementType = movement.MovementType,
            FromLocationId = movement.FromLocationId,
            ToLocationId = movement.ToLocationId,
            SupplyarrItemId = movement.SupplyarrItemId,
            PersonId = movement.PersonId,
            RelatedObjectType = movement.RelatedObjectType,
            RelatedObjectId = movement.RelatedObjectId,
            InventoryOriginEventId = movement.InventoryOriginEventId,
            CreatedAtUtc = ParseTimestamp(movement.CreatedAtUtc, nameof(movement.CreatedAtUtc)),
            PayloadJson = JsonSerializer.Serialize(movement, JsonOptions)
        });

        if (balanceEntity is null)
        {
            balanceEntity = new LoadArrInventoryBalanceEntity
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                BalanceId = balance.Id,
                SupplyarrItemId = balance.SupplyarrItemId,
                LocationId = balance.LocationId,
                LotCode = NormalizeBalanceKeyPart(completedLine.LotCode),
                SerialCode = NormalizeBalanceKeyPart(completedLine.SerialCode),
                CreatedAtUtc = now
            };
            db.LoadArrInventoryBalances.Add(balanceEntity);
        }

        balanceEntity.State = balance.State;
        balanceEntity.QuantityOnHand = balance.QuantityOnHand;
        balanceEntity.QuantityReserved = balance.QuantityReserved;
        balanceEntity.QuantityAllocated = balance.QuantityAllocated;
        balanceEntity.QuantityBlocked = balance.QuantityBlocked;
        balanceEntity.PayloadJson = JsonSerializer.Serialize(balance, JsonOptions);
        balanceEntity.UpdatedAtUtc = now;

        db.LoadArrWarehouseTasks.Add(new LoadArrWarehouseTaskEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TaskId = putawayTask.Id,
            TaskType = putawayTask.TaskType,
            Status = putawayTask.Status,
            SupplyarrItemId = putawayTask.SupplyarrItemId,
            SourceObjectType = "receiving_session",
            SourceObjectId = session.Id,
            DueAtUtc = ParseTimestamp(putawayTask.DueAtUtc, nameof(putawayTask.DueAtUtc)),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            PayloadJson = JsonSerializer.Serialize(putawayTask, JsonOptions)
        });

        await db.SaveChangesAsync(cancellationToken);

        return new LoadArrReceivingCompletionResponse(
            completedSession,
            originEvent,
            movement,
            balance,
            putawayTask);
    }

    private static LoadArrReceivingSessionResponse DeserializeReceivingSession(string payloadJson) =>
        JsonSerializer.Deserialize<LoadArrReceivingSessionResponse>(payloadJson, JsonOptions)
        ?? throw new InvalidOperationException("Persisted receiving session payload could not be deserialized.");

    private static LoadArrTransferOrderResponse DeserializeTransferOrder(string payloadJson) =>
        JsonSerializer.Deserialize<LoadArrTransferOrderResponse>(payloadJson, JsonOptions)
        ?? throw new InvalidOperationException("Persisted transfer order payload could not be deserialized.");

    private static LoadArrInventoryOriginEventResponse DeserializeInventoryOriginEvent(string payloadJson) =>
        JsonSerializer.Deserialize<LoadArrInventoryOriginEventResponse>(payloadJson, JsonOptions)
        ?? throw new InvalidOperationException("Persisted inventory origin event payload could not be deserialized.");

    private static LoadArrInventoryMovementResponse DeserializeInventoryMovement(string payloadJson) =>
        JsonSerializer.Deserialize<LoadArrInventoryMovementResponse>(payloadJson, JsonOptions)
        ?? throw new InvalidOperationException("Persisted inventory movement payload could not be deserialized.");

    private static LoadArrInventoryBalanceResponse DeserializeInventoryBalance(string payloadJson) =>
        JsonSerializer.Deserialize<LoadArrInventoryBalanceResponse>(payloadJson, JsonOptions)
        ?? throw new InvalidOperationException("Persisted inventory balance payload could not be deserialized.");

    private static LoadArrWarehouseTaskResponse DeserializeWarehouseTask(string payloadJson) =>
        JsonSerializer.Deserialize<LoadArrWarehouseTaskResponse>(payloadJson, JsonOptions)
        ?? throw new InvalidOperationException("Persisted warehouse task payload could not be deserialized.");

    private static string NormalizeBalanceKeyPart(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string CreateCompletionRecordId(string prefix, string sessionId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(sessionId));
        return $"{prefix}-{Convert.ToHexString(hash[..10]).ToLowerInvariant()}";
    }

    private static DateTimeOffset ParseTimestamp(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value) || !DateTimeOffset.TryParse(value, out var parsed))
        {
            throw new InvalidOperationException($"Persisted operational workflow field '{fieldName}' is not a valid timestamp.");
        }

        return parsed;
    }
}
