using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.SmartImport;

namespace StaffArr.Api.Services;

public sealed class StaffArrSmartImportCommitHandler(StaffArrDbContext db) : ISmartImportDestinationCommitHandler
{
    public string ProductKey => "staffarr";

    public async Task<SmartImportDestinationCommitResponse> CommitAsync(
        string entityType,
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SmartImportDestinationCommitResponses.IsCreateOperation(request.Operation))
        {
            return SmartImportDestinationCommitResponses.ReviewRequired(
                "staffarr.smart_import.operation_not_supported",
                "StaffArr Smart Import commits currently support reviewed create operations only.");
        }

        if (entityType.Contains("location", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("site", StringComparison.OrdinalIgnoreCase))
        {
            return await CommitLocationAsync(entityType, request, cancellationToken);
        }

        if (entityType.Contains("person", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("employee", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("worker", StringComparison.OrdinalIgnoreCase))
        {
            return await CommitPersonAsync(entityType, request, cancellationToken);
        }

        return SmartImportDestinationCommitResponses.ReviewRequired(
            "staffarr.smart_import.entity_type_not_supported",
            $"StaffArr does not have a Smart Import commit handler for entity type '{entityType}'.");
    }

    private async Task<SmartImportDestinationCommitResponse> CommitPersonAsync(
        string entityType,
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.People.FirstOrDefaultAsync(
            person => person.TenantId == request.TenantId && person.Id == request.CommitStepId,
            cancellationToken);
        if (existing is not null)
        {
            return Committed(existing.Id, existing.DisplayName);
        }

        var payload = request.DeterministicPayload;
        var shortId = SmartImportPayloadReader.ShortId(request.CommitStepId);
        var displayName = SmartImportPayloadReader.DisplayName(payload, $"Imported person {shortId}");
        var givenName = SmartImportPayloadReader.FirstNonEmpty(
            SmartImportPayloadReader.GetString(payload, "givenName", "firstName", "legalFirstName"),
            SplitDisplayName(displayName).given,
            "Imported");
        var familyName = SmartImportPayloadReader.FirstNonEmpty(
            SmartImportPayloadReader.GetString(payload, "familyName", "lastName", "legalLastName"),
            SplitDisplayName(displayName).family,
            shortId);
        var primaryEmail = SmartImportPayloadReader.FirstNonEmpty(
            SmartImportPayloadReader.GetString(payload, "primaryEmail", "email", "workEmail"),
            $"smart-import-{shortId}@import.invalid").ToLowerInvariant();
        var now = DateTimeOffset.UtcNow;

        var person = new StaffPerson
        {
            Id = request.CommitStepId,
            TenantId = request.TenantId,
            GivenName = SmartImportPayloadReader.Truncate(givenName, 100),
            FamilyName = SmartImportPayloadReader.Truncate(familyName, 100),
            LegalFirstName = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "legalFirstName") ?? givenName,
                100),
            LegalLastName = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "legalLastName") ?? familyName,
                100),
            DisplayName = SmartImportPayloadReader.Truncate(displayName, 200),
            PrimaryEmail = SmartImportPayloadReader.Truncate(primaryEmail, 320),
            PrimaryPhone = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "primaryPhone", "phone", "mobilePhone"),
                32),
            EmploymentStatus = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "employmentStatus", "status") ?? "active",
                32),
            WorkRelationshipType = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "workRelationshipType", "relationshipType"),
                32),
            EmploymentType = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "employmentType"),
                32),
            JobTitle = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "jobTitle", "title"),
                128),
            StartDate = SmartImportPayloadReader.GetDateTimeOffset(payload, "startDate", "hireDate"),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.People.Add(person);
        AddAudit(request, "smart_import.person_created", "staff_person", person.Id.ToString("D"), now);
        await db.SaveChangesAsync(cancellationToken);
        return Committed(person.Id, person.DisplayName);
    }

    private async Task<SmartImportDestinationCommitResponse> CommitLocationAsync(
        string entityType,
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.InternalLocations.FirstOrDefaultAsync(
            location => location.TenantId == request.TenantId && location.Id == request.CommitStepId,
            cancellationToken);
        if (existing is not null)
        {
            return Committed(existing.Id, existing.Name);
        }

        var payload = request.DeterministicPayload;
        var shortId = SmartImportPayloadReader.ShortId(request.CommitStepId);
        var locationNumber = SmartImportPayloadReader.FirstNonEmpty(
            SmartImportPayloadReader.GetString(payload, "locationNumber", "siteNumber", "code", "locationCode"),
            $"SI-LOC-{shortId}");
        var duplicate = await db.InternalLocations.FirstOrDefaultAsync(
            location => location.TenantId == request.TenantId
                && location.ParentLocationId == null
                && location.LocationNumber == locationNumber,
            cancellationToken);
        if (duplicate is not null)
        {
            return Committed(duplicate.Id, duplicate.Name);
        }

        var displayName = SmartImportPayloadReader.DisplayName(payload, $"Imported location {shortId}");
        var now = DateTimeOffset.UtcNow;
        var locationEntity = new InternalLocation
        {
            Id = request.CommitStepId,
            TenantId = request.TenantId,
            LocationNumber = SmartImportPayloadReader.Truncate(locationNumber, 64),
            Name = SmartImportPayloadReader.Truncate(displayName, 128),
            Description = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "description", "notes"),
                512),
            LocationType = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "locationType", "siteType", "type") ?? "other",
                64),
            Status = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "status") ?? "planned",
                32),
            AllowedProductUsage = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "allowedProductUsage") ?? "all",
                64),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.InternalLocations.Add(locationEntity);
        AddAudit(request, "smart_import.location_created", "internal_location", locationEntity.Id.ToString("D"), now);
        await db.SaveChangesAsync(cancellationToken);
        return Committed(locationEntity.Id, locationEntity.Name);
    }

    private void AddAudit(
        SmartImportDestinationCommitRequest request,
        string action,
        string targetType,
        string targetId,
        DateTimeOffset occurredAt)
    {
        db.AuditEvents.Add(new StaffArrAuditEvent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            ActorUserId = request.ApprovedByPersonId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Result = "success",
            ReasonCode = "smart_import",
            CorrelationId = request.CommitPlanId,
            OccurredAt = occurredAt
        });
    }

    private static SmartImportDestinationCommitResponse Committed(Guid id, string displayName) =>
        SmartImportDestinationCommitResponses.Committed(id.ToString("D"), displayName);

    private static (string given, string family) SplitDisplayName(string displayName)
    {
        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return (string.Empty, string.Empty);
        }

        if (parts.Length == 1)
        {
            return (parts[0], string.Empty);
        }

        return (parts[0], parts[^1]);
    }
}
