using CustomArr.Api.Data;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.SmartImport;

namespace CustomArr.Api.Services;

public sealed class CustomArrSmartImportCommitHandler(CustomArrDbContext db) : ISmartImportDestinationCommitHandler
{
    private const string OperationKey = "smart_import.customer.create";

    public string ProductKey => "customarr";

    public async Task<SmartImportDestinationCommitResponse> CommitAsync(
        string entityType,
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SmartImportDestinationCommitResponses.IsCreateOperation(request.Operation))
        {
            return SmartImportDestinationCommitResponses.ReviewRequired(
                "customarr.smart_import.operation_not_supported",
                "CustomArr Smart Import commits currently support reviewed create operations only.");
        }

        if (!entityType.Contains("customer", StringComparison.OrdinalIgnoreCase)
            && !entityType.Contains("account", StringComparison.OrdinalIgnoreCase)
            && !entityType.Contains("shipper", StringComparison.OrdinalIgnoreCase)
            && !entityType.Contains("consignee", StringComparison.OrdinalIgnoreCase))
        {
            return SmartImportDestinationCommitResponses.ReviewRequired(
                "customarr.smart_import.entity_type_not_supported",
                $"CustomArr does not have a Smart Import commit handler for entity type '{entityType}'.");
        }

        return await CommitCustomerAsync(request, cancellationToken);
    }

    private async Task<SmartImportDestinationCommitResponse> CommitCustomerAsync(
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var idempotencyRecord = await db.IdempotencyRecords.FirstOrDefaultAsync(
            record => record.TenantId == request.TenantId
                && record.OperationKey == OperationKey
                && record.IdempotencyKey == request.IdempotencyKey,
            cancellationToken);
        if (idempotencyRecord is not null)
        {
            var committed = await db.Customers.FirstOrDefaultAsync(
                customer => customer.TenantId == request.TenantId && customer.CustomerId == idempotencyRecord.ResourceId,
                cancellationToken);
            if (committed is not null)
            {
                return SmartImportDestinationCommitResponses.Committed(committed.CustomerId, committed.DisplayName);
            }
        }

        var payload = request.DeterministicPayload;
        var shortId = SmartImportPayloadReader.ShortId(request.CommitStepId);
        var customerId = $"cust-{shortId}";
        var existing = await db.Customers.FirstOrDefaultAsync(
            customer => customer.TenantId == request.TenantId && customer.CustomerId == customerId,
            cancellationToken);
        if (existing is not null)
        {
            return SmartImportDestinationCommitResponses.Committed(existing.CustomerId, existing.DisplayName);
        }

        var displayName = SmartImportPayloadReader.DisplayName(payload, $"Imported customer {shortId}");
        var customerNumber = SmartImportPayloadReader.FirstNonEmpty(
            SmartImportPayloadReader.GetString(payload, "customerNumber", "accountNumber", "code"),
            $"SI-CUST-{shortId}");
        var customerCode = SmartImportPayloadReader.SlugKey(
            SmartImportPayloadReader.GetString(payload, "customerCode", "accountCode", "code") ?? customerNumber,
            $"si_cust_{shortId}",
            64);
        var duplicate = await db.Customers.FirstOrDefaultAsync(
            customer => customer.TenantId == request.TenantId
                && (customer.CustomerNumber == customerNumber || customer.CustomerCode == customerCode),
            cancellationToken);
        if (duplicate is not null)
        {
            return SmartImportDestinationCommitResponses.Committed(duplicate.CustomerId, duplicate.DisplayName);
        }

        var now = DateTimeOffset.UtcNow;
        var actor = request.ApprovedByPersonId.ToString("D");
        var customerEntity = new CustomArrCustomer
        {
            CustomerId = customerId,
            TenantId = request.TenantId,
            CustomerNumber = SmartImportPayloadReader.Truncate(customerNumber, 64),
            CustomerCode = customerCode,
            LegalName = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "legalName", "name") ?? displayName,
                256),
            DisplayName = SmartImportPayloadReader.Truncate(displayName, 256),
            DbaName = SmartImportPayloadReader.Truncate(SmartImportPayloadReader.GetString(payload, "dbaName", "dba"), 256),
            CustomerTypeKey = SmartImportPayloadReader.SlugKey(
                SmartImportPayloadReader.GetString(payload, "customerTypeKey", "customerType", "type"),
                "business",
                64),
            StatusKey = SmartImportPayloadReader.SlugKey(
                SmartImportPayloadReader.GetString(payload, "statusKey", "status"),
                "prospect",
                64),
            AccountOwnerPersonId = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "accountOwnerPersonId", "ownerPersonId"),
                128),
            SourceKey = "smart_import",
            Notes = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "notes", "description") ?? "Created by reviewed Smart Import commit.",
                4000),
            RiskRatingKey = SmartImportPayloadReader.SlugKey(
                SmartImportPayloadReader.GetString(payload, "riskRatingKey", "riskRating"),
                "low",
                64),
            PortalDisplayName = SmartImportPayloadReader.Truncate(displayName, 256),
            CreatedAt = now,
            CreatedByPersonId = actor,
            UpdatedAt = now,
            UpdatedByPersonId = actor
        };

        db.Customers.Add(customerEntity);
        db.CustomerActivity.Add(new CustomArrCustomerActivity
        {
            ActivityId = $"act-{shortId}",
            TenantId = request.TenantId,
            CustomerId = customerEntity.CustomerId,
            Kind = "smart_import.committed",
            Message = "Customer created by reviewed Smart Import commit.",
            SourceProductKey = "nexarr",
            ActorPersonId = actor,
            OccurredAt = now
        });
        db.IdempotencyRecords.Add(new CustomArrIdempotencyRecord
        {
            IdempotencyRecordId = $"idem-{shortId}",
            TenantId = request.TenantId,
            OperationKey = OperationKey,
            IdempotencyKey = request.IdempotencyKey,
            ResourceId = customerEntity.CustomerId,
            CreatedAt = now
        });

        await db.SaveChangesAsync(cancellationToken);
        return SmartImportDestinationCommitResponses.Committed(customerEntity.CustomerId, customerEntity.DisplayName);
    }
}
