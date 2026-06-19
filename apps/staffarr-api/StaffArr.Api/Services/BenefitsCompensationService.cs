using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class BenefitsCompensationService(StaffArrDbContext db, IStaffArrAuditService audit)
{
    public async Task<IReadOnlyList<BenefitEnrollmentResponse>> ListBenefitEnrollmentsAsync(Guid tenantId, Guid? personId, CancellationToken cancellationToken)
    {
        var query = db.BenefitEnrollments.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }

        return await query.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreatedAt).Take(200).Select(x => MapEnrollment(x)).ToListAsync(cancellationToken);
    }

    public async Task<BenefitEnrollmentResponse> UpsertBenefitEnrollmentAsync(Guid tenantId, Guid? actorUserId, Guid? id, UpsertBenefitEnrollmentRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        var entity = id.HasValue
            ? await db.BenefitEnrollments.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id.Value, cancellationToken)
            : null;
        if (id.HasValue && entity is null)
        {
            throw new StlApiException("staffarr.benefits.enrollment_not_found", "Benefit enrollment was not found.", 404);
        }

        entity ??= new BenefitEnrollment { TenantId = tenantId, PersonId = request.PersonId, CreatedAt = DateTimeOffset.UtcNow };
        entity.PersonId = request.PersonId;
        entity.BenefitType = Require(request.BenefitType, "Benefit type is required.", 64);
        entity.PlanName = Require(request.PlanName, "Plan name is required.", 128);
        entity.BenefitClass = Require(request.BenefitClass, "Benefit class is required.", 128);
        entity.CoverageLevel = NormalizeEnum(request.CoverageLevel, ["employee", "employee_plus_spouse", "employee_plus_children", "family", "waived", "other"], "Coverage level");
        entity.EligibilityStatus = NormalizeEnum(request.EligibilityStatus, ["eligible", "pending", "ineligible", "cobra", "retiree"], "Eligibility status");
        entity.EnrollmentStatus = NormalizeEnum(request.EnrollmentStatus, ["pending", "enrolled", "waived", "terminated", "ended"], "Enrollment status");
        entity.CarrierExportStatus = NormalizeEnum(request.CarrierExportStatus, ["not_exported", "queued", "exported", "error"], "Carrier export status");
        entity.CarrierMemberId = Optional(request.CarrierMemberId, 128);
        entity.CarrierGroupId = Optional(request.CarrierGroupId, 128);
        entity.EffectiveStartDate = request.EffectiveStartDate;
        entity.EffectiveEndDate = request.EffectiveEndDate;
        entity.OpenEnrollmentYear = request.OpenEnrollmentYear;
        entity.SourceProductKey = Optional(request.SourceProductKey, 64);
        entity.SourceRef = Optional(request.SourceRef, 256);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        if (db.Entry(entity).State == EntityState.Detached)
        {
            db.BenefitEnrollments.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("benefits.enrollment.upsert", tenantId, actorUserId, "benefit_enrollment", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapEnrollment(entity);
    }

    public async Task<IReadOnlyList<BenefitDependentResponse>> ListDependentsAsync(Guid tenantId, Guid? personId, CancellationToken cancellationToken)
    {
        var query = db.BenefitDependents.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }

        return await query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).Select(x => MapDependent(x)).ToListAsync(cancellationToken);
    }

    public async Task<BenefitDependentResponse> UpsertDependentAsync(Guid tenantId, Guid? actorUserId, Guid? id, UpsertBenefitDependentRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        var entity = id.HasValue
            ? await db.BenefitDependents.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id.Value, cancellationToken)
            : null;
        if (id.HasValue && entity is null)
        {
            throw new StlApiException("staffarr.benefits.dependent_not_found", "Benefit dependent was not found.", 404);
        }

        entity ??= new BenefitDependent { TenantId = tenantId, PersonId = request.PersonId, CreatedAt = DateTimeOffset.UtcNow };
        entity.PersonId = request.PersonId;
        entity.FirstName = Require(request.FirstName, "Dependent first name is required.", 100);
        entity.LastName = Require(request.LastName, "Dependent last name is required.", 100);
        entity.Relationship = Require(request.Relationship, "Dependent relationship is required.", 64);
        entity.DateOfBirth = request.DateOfBirth;
        entity.IsStudent = request.IsStudent;
        entity.IsDisabled = request.IsDisabled;
        entity.CoverageStatus = NormalizeEnum(request.CoverageStatus, ["eligible", "enrolled", "pending", "terminated", "ineligible"], "Coverage status");
        entity.CoverageStartDate = request.CoverageStartDate;
        entity.CoverageEndDate = request.CoverageEndDate;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        if (db.Entry(entity).State == EntityState.Detached)
        {
            db.BenefitDependents.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("benefits.dependent.upsert", tenantId, actorUserId, "benefit_dependent", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapDependent(entity);
    }

    public async Task<IReadOnlyList<BenefitBeneficiaryResponse>> ListBeneficiariesAsync(Guid tenantId, Guid? personId, CancellationToken cancellationToken)
    {
        var query = db.BenefitBeneficiaries.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }

        return await query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).Select(x => MapBeneficiary(x)).ToListAsync(cancellationToken);
    }

    public async Task<BenefitBeneficiaryResponse> UpsertBeneficiaryAsync(Guid tenantId, Guid? actorUserId, Guid? id, UpsertBenefitBeneficiaryRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        var entity = id.HasValue
            ? await db.BenefitBeneficiaries.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id.Value, cancellationToken)
            : null;
        if (id.HasValue && entity is null)
        {
            throw new StlApiException("staffarr.benefits.beneficiary_not_found", "Benefit beneficiary was not found.", 404);
        }

        entity ??= new BenefitBeneficiary { TenantId = tenantId, PersonId = request.PersonId, CreatedAt = DateTimeOffset.UtcNow };
        entity.PersonId = request.PersonId;
        entity.FirstName = Require(request.FirstName, "Beneficiary first name is required.", 100);
        entity.LastName = Require(request.LastName, "Beneficiary last name is required.", 100);
        entity.Relationship = Require(request.Relationship, "Beneficiary relationship is required.", 64);
        entity.AllocationPercent = Math.Clamp(request.AllocationPercent, 0m, 100m);
        entity.DesignationType = Optional(request.DesignationType, 64);
        entity.Status = NormalizeEnum(request.Status, ["active", "inactive", "pending"], "Beneficiary status");
        entity.EffectiveStartDate = request.EffectiveStartDate;
        entity.EffectiveEndDate = request.EffectiveEndDate;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        if (db.Entry(entity).State == EntityState.Detached)
        {
            db.BenefitBeneficiaries.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("benefits.beneficiary.upsert", tenantId, actorUserId, "benefit_beneficiary", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapBeneficiary(entity);
    }

    public async Task<IReadOnlyList<CompensationProfileResponse>> ListCompensationProfilesAsync(Guid tenantId, Guid? personId, CancellationToken cancellationToken)
    {
        var query = db.CompensationProfiles.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }

        return await query.OrderByDescending(x => x.EffectiveStartDate).Select(x => MapCompensationProfile(x)).ToListAsync(cancellationToken);
    }

    public async Task<CompensationProfileResponse> UpsertCompensationProfileAsync(Guid tenantId, Guid? actorUserId, Guid? id, UpsertCompensationProfileRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        var entity = id.HasValue
            ? await db.CompensationProfiles.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id.Value, cancellationToken)
            : null;
        if (id.HasValue && entity is null)
        {
            throw new StlApiException("staffarr.compensation.profile_not_found", "Compensation profile was not found.", 404);
        }

        entity ??= new CompensationProfile { TenantId = tenantId, PersonId = request.PersonId, CreatedAt = DateTimeOffset.UtcNow };
        entity.PersonId = request.PersonId;
        entity.PayBasis = NormalizeEnum(request.PayBasis, ["salary", "hourly", "commission", "stipend", "contract", "piece_rate"], "Pay basis");
        entity.PayGrade = Require(request.PayGrade, "Pay grade is required.", 64);
        entity.PayBand = Require(request.PayBand, "Pay band is required.", 64);
        entity.StepProgression = Optional(request.StepProgression, 64);
        entity.BaseRate = request.BaseRate;
        entity.AnnualSalary = request.AnnualSalary;
        entity.CurrencyCode = Require(request.CurrencyCode, "Currency code is required.", 8).ToUpperInvariant();
        entity.OvertimeEligible = request.OvertimeEligible;
        entity.ShiftDifferentialEligible = request.ShiftDifferentialEligible;
        entity.BonusEligible = request.BonusEligible;
        entity.AllowanceEligible = request.AllowanceEligible;
        entity.Status = NormalizeEnum(request.Status, ["active", "pending", "inactive", "frozen"], "Compensation status");
        entity.EffectiveStartDate = request.EffectiveStartDate;
        entity.EffectiveEndDate = request.EffectiveEndDate;
        entity.SourceProductKey = Optional(request.SourceProductKey, 64);
        entity.SourceRef = Optional(request.SourceRef, 256);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        if (db.Entry(entity).State == EntityState.Detached)
        {
            db.CompensationProfiles.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("compensation.profile.upsert", tenantId, actorUserId, "compensation_profile", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapCompensationProfile(entity);
    }

    public async Task<IReadOnlyList<CompensationChangeRequestResponse>> ListCompensationChangeRequestsAsync(Guid tenantId, Guid? personId, CancellationToken cancellationToken)
    {
        var query = db.CompensationChangeRequests.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }

        return await query.OrderByDescending(x => x.CreatedAt).Take(200).Select(x => MapChangeRequest(x)).ToListAsync(cancellationToken);
    }

    public async Task<CompensationChangeRequestResponse> CreateCompensationChangeRequestAsync(Guid tenantId, Guid? actorUserId, CreateCompensationChangeRequestRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        var entity = new CompensationChangeRequest
        {
            TenantId = tenantId,
            PersonId = request.PersonId,
            RequestType = NormalizeEnum(request.RequestType, ["raise", "promotion", "step_progression", "market_adjustment", "bonus", "allowance", "salary_review", "other"], "Request type"),
            Status = NormalizeEnum(request.Status, ["draft", "pending", "approved", "rejected", "cancelled"], "Change request status"),
            ReasonCode = Require(request.ReasonCode, "Reason code is required.", 64),
            ReasonText = Require(request.ReasonText, "Reason text is required.", 2048),
            OldSnapshot = Require(request.OldSnapshot, "Old snapshot is required.", 4096),
            NewSnapshot = Require(request.NewSnapshot, "New snapshot is required.", 4096),
            RequestedByPersonId = request.RequestedByPersonId,
            EffectiveDate = request.EffectiveDate,
            SourceProductKey = Optional(request.SourceProductKey, 64),
            SourceRef = Optional(request.SourceRef, 256),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        db.CompensationChangeRequests.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("compensation.change_request.create", tenantId, actorUserId, "compensation_change_request", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapChangeRequest(entity);
    }

    public async Task<CompensationChangeRequestResponse> ReviewCompensationChangeRequestAsync(Guid tenantId, Guid? actorUserId, Guid id, ReviewCompensationChangeRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.CompensationChangeRequests.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken)
            ?? throw new StlApiException("staffarr.compensation.change_request_not_found", "Compensation change request was not found.", 404);
        entity.Status = NormalizeEnum(request.Status, ["approved", "rejected", "pending", "cancelled"], "Change request status");
        entity.ApprovedByPersonId = actorUserId;
        entity.ReviewedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync($"compensation.change_request.{entity.Status}", tenantId, actorUserId, "compensation_change_request", id.ToString(), "success", cancellationToken: cancellationToken);
        return MapChangeRequest(entity);
    }

    private async Task EnsurePersonAsync(Guid tenantId, Guid personId, CancellationToken cancellationToken)
    {
        var exists = await db.People.AnyAsync(x => x.TenantId == tenantId && x.Id == personId, cancellationToken);
        if (!exists)
        {
            throw new StlApiException("staffarr.benefits.person_not_found", "Person record was not found.", 404);
        }
    }

    private static string Require(string? value, string message, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("staffarr.benefits.validation", message, 400);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException("staffarr.benefits.validation", $"{message.TrimEnd('.')} must be {maxLength} characters or less.", 400);
        }

        return normalized;
    }

    private static string? Optional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException("staffarr.benefits.validation", $"Value must be {maxLength} characters or less.", 400);
        }

        return normalized;
    }

    private static string NormalizeEnum(string value, IReadOnlyCollection<string> allowed, string fieldName)
    {
        var normalized = Require(value, $"{fieldName} is required.", 64).ToLowerInvariant();
        if (!allowed.Contains(normalized))
        {
            throw new StlApiException("staffarr.benefits.validation", $"{fieldName} is invalid.", 400);
        }

        return normalized;
    }

    private static BenefitEnrollmentResponse MapEnrollment(BenefitEnrollment x) =>
        new(x.Id, x.PersonId, x.BenefitType, x.PlanName, x.BenefitClass, x.CoverageLevel, x.EligibilityStatus, x.EnrollmentStatus, x.CarrierExportStatus, x.CarrierMemberId, x.CarrierGroupId, x.EffectiveStartDate, x.EffectiveEndDate, x.OpenEnrollmentYear, x.SourceProductKey, x.SourceRef, x.CreatedAt, x.UpdatedAt);

    private static BenefitDependentResponse MapDependent(BenefitDependent x) =>
        new(x.Id, x.PersonId, x.FirstName, x.LastName, x.Relationship, x.DateOfBirth, x.IsStudent, x.IsDisabled, x.CoverageStatus, x.CoverageStartDate, x.CoverageEndDate, x.CreatedAt, x.UpdatedAt);

    private static BenefitBeneficiaryResponse MapBeneficiary(BenefitBeneficiary x) =>
        new(x.Id, x.PersonId, x.FirstName, x.LastName, x.Relationship, x.AllocationPercent, x.DesignationType, x.Status, x.EffectiveStartDate, x.EffectiveEndDate, x.CreatedAt, x.UpdatedAt);

    private static CompensationProfileResponse MapCompensationProfile(CompensationProfile x) =>
        new(x.Id, x.PersonId, x.PayBasis, x.PayGrade, x.PayBand, x.StepProgression, x.BaseRate, x.AnnualSalary, x.CurrencyCode, x.OvertimeEligible, x.ShiftDifferentialEligible, x.BonusEligible, x.AllowanceEligible, x.Status, x.EffectiveStartDate, x.EffectiveEndDate, x.SourceProductKey, x.SourceRef, x.CreatedAt, x.UpdatedAt);

    private static CompensationChangeRequestResponse MapChangeRequest(CompensationChangeRequest x) =>
        new(x.Id, x.PersonId, x.RequestType, x.Status, x.ReasonCode, x.ReasonText, x.OldSnapshot, x.NewSnapshot, x.RequestedByPersonId, x.ApprovedByPersonId, x.ReviewedAt, x.EffectiveDate, x.SourceProductKey, x.SourceRef, x.CreatedAt, x.UpdatedAt);
}
