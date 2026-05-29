using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class PersonnelUpdateRequestService(
    StaffArrDbContext db,
    IStaffArrAuditService audit,
    StaffArrMaintainArrTechnicianRefSyncService maintainarrTechnicianRefSync)
{
    private static readonly HashSet<string> PendingStatuses =
    [
        PersonnelUpdateRequestStatuses.Submitted,
        PersonnelUpdateRequestStatuses.PendingReview,
    ];

    public async Task<PersonnelUpdateRequestResponse> SubmitAsync(
        Guid tenantId,
        Guid personId,
        Guid actorUserId,
        SubmitPersonnelUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var personExists = await db.People.AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.Id == personId, cancellationToken);
        if (!personExists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        var requestType = PersonnelUpdateRequestRules.NormalizeRequestType(request.RequestType);
        var fieldKey = PersonnelUpdateRequestRules.NormalizeFieldKey(request.FieldKey);
        var requestedValue = PersonnelUpdateRequestRules.NormalizeRequestedValue(request.RequestedValue);
        var currentValue = PersonnelUpdateRequestRules.NormalizeOptionalText(
            request.CurrentValue,
            512,
            "Current value");
        var details = PersonnelUpdateRequestRules.NormalizeOptionalText(request.Details, 2048, "Details");

        var now = DateTimeOffset.UtcNow;
        var record = new PersonnelUpdateRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = personId,
            RequestType = requestType,
            Status = PersonnelUpdateRequestStatuses.Submitted,
            FieldKey = fieldKey,
            CurrentValue = currentValue,
            RequestedValue = requestedValue,
            Details = details,
            SubmittedByUserId = actorUserId,
            SubmittedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.PersonnelUpdateRequests.Add(record);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "personnel_update.submitted",
            tenantId,
            actorUserId,
            "personnel_update_request",
            record.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return Map(record);
    }

    public async Task<IReadOnlyList<PersonnelUpdateRequestResponse>> ListForPersonAsync(
        Guid tenantId,
        Guid personId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var cappedLimit = Math.Clamp(limit, 1, 100);
        var rows = await db.PersonnelUpdateRequests.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .OrderByDescending(x => x.SubmittedAt)
            .Take(cappedLimit)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<PersonnelUpdateRequestResponse>> ListPendingAsync(
        Guid tenantId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var cappedLimit = Math.Clamp(limit, 1, 100);
        var rows = await db.PersonnelUpdateRequests.AsNoTracking()
            .Where(x => x.TenantId == tenantId && PendingStatuses.Contains(x.Status))
            .OrderByDescending(x => x.SubmittedAt)
            .Take(cappedLimit)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).ToList();
    }

    public async Task<PersonnelUpdateRequestResponse> GetByIdAsync(
        Guid tenantId,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var record = await LoadRequestAsync(tenantId, requestId, cancellationToken);
        return Map(record);
    }

    public async Task<PersonnelUpdateRequestReviewResponse> ReviewAsync(
        Guid tenantId,
        Guid requestId,
        Guid reviewerUserId,
        ReviewPersonnelUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var decision = PersonnelUpdateRequestRules.NormalizeReviewDecision(request.Decision);
        var reviewNotes = PersonnelUpdateRequestRules.NormalizeReviewNotes(request.ReviewNotes);

        var record = await LoadRequestAsync(tenantId, requestId, cancellationToken, tracked: true);
        if (!PendingStatuses.Contains(record.Status))
        {
            throw new StlApiException(
                "personnel_update.invalid_state",
                "Only submitted personnel update requests can be reviewed.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var appliedToProfile = false;

        if (decision == "approve")
        {
            record.Status = PersonnelUpdateRequestStatuses.Approved;
            if (request.ApplyToProfile)
            {
                if (!PersonnelUpdateRequestRules.SupportsProfileApply(record.FieldKey))
                {
                    throw new StlApiException(
                        "personnel_update.apply_unsupported",
                        $"Field '{record.FieldKey}' cannot be applied automatically to the workforce profile.",
                        400);
                }

                await ApplyToProfileAsync(tenantId, record, reviewerUserId, cancellationToken);
                appliedToProfile = true;
            }
        }
        else
        {
            record.Status = PersonnelUpdateRequestStatuses.Denied;
        }

        record.ReviewedByUserId = reviewerUserId;
        record.ReviewedAt = now;
        record.ReviewNotes = reviewNotes;
        record.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        var auditAction = decision == "approve"
            ? "personnel_update.approved"
            : "personnel_update.denied";
        await audit.WriteAsync(
            auditAction,
            tenantId,
            reviewerUserId,
            "personnel_update_request",
            record.Id.ToString(),
            appliedToProfile ? "applied" : "success",
            cancellationToken: cancellationToken);

        return new PersonnelUpdateRequestReviewResponse(Map(record), appliedToProfile);
    }

    private async Task ApplyToProfileAsync(
        Guid tenantId,
        PersonnelUpdateRequest record,
        Guid? actorUserId,
        CancellationToken cancellationToken)
    {
        var person = await db.People.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == record.PersonId,
            cancellationToken);
        if (person is null)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        switch (record.FieldKey)
        {
            case "work_phone":
                person.WorkPhone = NormalizeWorkPhone(record.RequestedValue);
                break;
            case "primary_email":
                person.PrimaryEmail = await NormalizeUniqueEmailAsync(
                    tenantId,
                    record.RequestedValue,
                    person.Id,
                    cancellationToken);
                break;
            case "job_title":
                person.JobTitle = NormalizeJobTitle(record.RequestedValue);
                break;
            case "given_name":
                person.GivenName = NormalizeNamePart(record.RequestedValue, "Given name");
                person.DisplayName = BuildDisplayName(person.GivenName, person.FamilyName);
                break;
            case "family_name":
                person.FamilyName = NormalizeNamePart(record.RequestedValue, "Family name");
                person.DisplayName = BuildDisplayName(person.GivenName, person.FamilyName);
                break;
            default:
                throw new StlApiException(
                    "personnel_update.apply_unsupported",
                    $"Field '{record.FieldKey}' cannot be applied automatically to the workforce profile.",
                    400);
        }

        person.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "personnel_update.applied",
            tenantId,
            actorUserId,
            "person",
            person.Id.ToString(),
            record.FieldKey,
            cancellationToken: cancellationToken);

        await maintainarrTechnicianRefSync.TryPublishPersonChangedAsync(
            person,
            "staffarr.person.updated",
            cancellationToken);
    }

    private async Task<PersonnelUpdateRequest> LoadRequestAsync(
        Guid tenantId,
        Guid requestId,
        CancellationToken cancellationToken,
        bool tracked = false)
    {
        var query = tracked
            ? db.PersonnelUpdateRequests.AsQueryable()
            : db.PersonnelUpdateRequests.AsNoTracking();

        var record = await query.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == requestId,
            cancellationToken);
        if (record is null)
        {
            throw new StlApiException(
                "personnel_update.not_found",
                "Personnel update request was not found.",
                404);
        }

        return record;
    }

    private async Task<string> NormalizeUniqueEmailAsync(
        Guid tenantId,
        string requestedValue,
        Guid personId,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = requestedValue.Trim().ToLowerInvariant();
        if (!new EmailAddressAttribute().IsValid(normalizedEmail))
        {
            throw new StlApiException(
                "personnel_update.validation",
                "Requested email is not valid.",
                400);
        }

        var emailTaken = await db.People.AsNoTracking()
            .AnyAsync(
                x => x.TenantId == tenantId
                    && x.PrimaryEmail == normalizedEmail
                    && x.Id != personId,
                cancellationToken);
        if (emailTaken)
        {
            throw new StlApiException(
                "personnel_update.validation",
                "Requested email is already assigned to another person.",
                409);
        }

        return normalizedEmail;
    }

    private static string NormalizeWorkPhone(string requestedValue)
    {
        var trimmed = requestedValue.Trim();
        if (trimmed.Length > 32)
        {
            throw new StlApiException(
                "personnel_update.validation",
                "Work phone must be 32 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string? NormalizeJobTitle(string requestedValue)
    {
        var trimmed = requestedValue.Trim();
        if (trimmed.Length > 128)
        {
            throw new StlApiException(
                "personnel_update.validation",
                "Job title must be 128 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeNamePart(string requestedValue, string fieldName)
    {
        var trimmed = requestedValue.Trim();
        if (trimmed.Length == 0 || trimmed.Length > 100)
        {
            throw new StlApiException(
                "personnel_update.validation",
                $"{fieldName} must be between 1 and 100 characters.",
                400);
        }

        return trimmed;
    }

    private static string BuildDisplayName(string givenName, string familyName) =>
        $"{givenName.Trim()} {familyName.Trim()}".Trim();

    private static PersonnelUpdateRequestResponse Map(PersonnelUpdateRequest record) =>
        new(
            record.Id,
            record.PersonId,
            record.RequestType,
            record.Status,
            record.FieldKey,
            record.CurrentValue,
            record.RequestedValue,
            record.Details,
            record.SubmittedByUserId,
            record.SubmittedAt,
            record.ReviewedByUserId,
            record.ReviewedAt,
            record.ReviewNotes,
            record.CreatedAt,
            record.UpdatedAt);
}
