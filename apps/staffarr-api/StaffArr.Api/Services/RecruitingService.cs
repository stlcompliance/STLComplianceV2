using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class RecruitingService(
    StaffArrDbContext db,
    IStaffArrAuditService audit,
    PeopleService peopleService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<RecruitingRequisitionResponse>> ListRequisitionsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var requisitions = await db.RecruitingRequisitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.UpdatedAt)
            .ThenBy(x => x.RequisitionNumber)
            .ToListAsync(cancellationToken);

        return requisitions.Select(MapRequisition).ToArray();
    }

    public async Task<RecruitingRequisitionResponse> UpsertRequisitionAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid? requisitionId,
        UpsertRecruitingRequisitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var requisitionNumber = NormalizeRequiredText(request.RequisitionNumber, 64, "requisition number");
        var now = DateTimeOffset.UtcNow;
        var requisition = requisitionId is Guid existingId
            ? await db.RecruitingRequisitions.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == existingId, cancellationToken)
            : null;

        if (await db.RecruitingRequisitions.AnyAsync(
                x => x.TenantId == tenantId
                    && x.RequisitionNumber == requisitionNumber
                    && (!requisitionId.HasValue || x.Id != requisitionId.Value),
                cancellationToken))
        {
            throw new StlApiException("recruiting.requisition.duplicate", "A requisition with this number already exists.", 409);
        }

        if (requisition is null)
        {
            requisition = new RecruitingRequisition
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.RecruitingRequisitions.Add(requisition);
        }

        var before = requisitionId is Guid ? MapRequisition(requisition) : null;
        requisition.RequisitionNumber = requisitionNumber;
        requisition.Title = NormalizeRequiredText(request.Title, 200, "title");
        requisition.JobCode = NormalizeRequiredText(request.JobCode, 64, "job code");
        requisition.JobFamily = NormalizeRequiredText(request.JobFamily, 128, "job family");
        requisition.DepartmentRef = NormalizeOptionalText(request.DepartmentRef, 256);
        requisition.SiteRef = NormalizeOptionalText(request.SiteRef, 256);
        requisition.LocationRef = NormalizeOptionalText(request.LocationRef, 256);
        requisition.HiringManagerPersonId = request.HiringManagerPersonId;
        requisition.RecruiterPersonId = request.RecruiterPersonId;
        requisition.Status = NormalizeRequiredText(request.Status, 32, "status").ToLowerInvariant();
        requisition.HeadcountRequested = Math.Max(1, request.HeadcountRequested);
        requisition.FilledCount = Math.Max(0, request.FilledCount);
        requisition.OpenDate = request.OpenDate;
        requisition.TargetStartDate = request.TargetStartDate;
        requisition.SourceProductKey = NormalizeOptionalText(request.SourceProductKey, 64);
        requisition.SourceRef = NormalizeOptionalText(request.SourceRef, 256);
        requisition.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteWithMetadataAsync(
            requisitionId is null ? "staffarr.recruiting.requisition.create" : "staffarr.recruiting.requisition.update",
            tenantId,
            actorUserId,
            "recruiting_requisition",
            requisition.Id.ToString(),
            "success",
            JsonSerializer.Serialize(new { before, after = MapRequisition(requisition) }, JsonOptions),
            cancellationToken: cancellationToken);

        return MapRequisition(requisition);
    }

    public async Task<RecruitingRequisitionResponse> ArchiveRequisitionAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid requisitionId,
        CancellationToken cancellationToken = default)
    {
        var requisition = await db.RecruitingRequisitions.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == requisitionId, cancellationToken);
        if (requisition is null)
        {
            throw new StlApiException("recruiting.requisition.not_found", "Recruiting requisition was not found.", 404);
        }

        var now = DateTimeOffset.UtcNow;
        var before = MapRequisition(requisition);
        requisition.Status = "archived";
        requisition.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteWithMetadataAsync(
            "staffarr.recruiting.requisition.archive",
            tenantId,
            actorUserId,
            "recruiting_requisition",
            requisition.Id.ToString(),
            "success",
            JsonSerializer.Serialize(new { before, after = MapRequisition(requisition) }, JsonOptions),
            cancellationToken: cancellationToken);

        return MapRequisition(requisition);
    }

    public async Task<IReadOnlyList<RecruitingCandidateResponse>> ListCandidatesAsync(
        Guid tenantId,
        Guid? recruitingRequisitionId,
        CancellationToken cancellationToken = default)
    {
        var query = db.RecruitingCandidates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (recruitingRequisitionId is Guid requisitionId)
        {
            query = query.Where(x => x.RecruitingRequisitionId == requisitionId);
        }

        var candidates = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ThenBy(x => x.CandidateName)
            .ToListAsync(cancellationToken);

        return candidates.Select(MapCandidate).ToArray();
    }

    public async Task<RecruitingCandidateResponse> UpsertCandidateAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid? candidateId,
        UpsertRecruitingCandidateRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var candidate = candidateId is Guid existingId
            ? await db.RecruitingCandidates.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == existingId, cancellationToken)
            : null;

        if (candidate is null)
        {
            candidate = new RecruitingCandidate
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.RecruitingCandidates.Add(candidate);
        }

        var before = candidateId is Guid ? MapCandidate(candidate) : null;
        candidate.RecruitingRequisitionId = request.RecruitingRequisitionId;
        candidate.EmploymentApplicationSubmissionId = request.EmploymentApplicationSubmissionId;
        candidate.PersonId = request.PersonId;
        candidate.CandidateName = NormalizeRequiredText(request.CandidateName, 200, "candidate name");
        candidate.CandidateEmail = NormalizeRequiredText(request.CandidateEmail, 320, "candidate email");
        candidate.CandidatePhone = NormalizeOptionalText(request.CandidatePhone, 32);
        candidate.SourceType = NormalizeRequiredText(request.SourceType, 32, "source type").ToLowerInvariant();
        candidate.Stage = NormalizeRequiredText(request.Stage, 32, "stage").ToLowerInvariant();
        candidate.Status = NormalizeRequiredText(request.Status, 32, "status").ToLowerInvariant();
        candidate.BackgroundCheckStatus = NormalizeOptionalText(request.BackgroundCheckStatus, 32);
        candidate.DrugScreenStatus = NormalizeOptionalText(request.DrugScreenStatus, 32);
        candidate.PhysicalStatus = NormalizeOptionalText(request.PhysicalStatus, 32);
        candidate.OfferStatus = NormalizeOptionalText(request.OfferStatus, 32);
        candidate.Score = request.Score;
        candidate.Notes = NormalizeOptionalText(request.Notes, 2048);
        candidate.SourceProductKey = NormalizeOptionalText(request.SourceProductKey, 64);
        candidate.SourceRef = NormalizeOptionalText(request.SourceRef, 256);
        candidate.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteWithMetadataAsync(
            candidateId is null ? "staffarr.recruiting.candidate.create" : "staffarr.recruiting.candidate.update",
            tenantId,
            actorUserId,
            "recruiting_candidate",
            candidate.Id.ToString(),
            "success",
            JsonSerializer.Serialize(new { before, after = MapCandidate(candidate) }, JsonOptions),
            cancellationToken: cancellationToken);

        return MapCandidate(candidate);
    }

    public async Task<RecruitingCandidateResponse> ArchiveCandidateAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid candidateId,
        CancellationToken cancellationToken = default)
    {
        var candidate = await db.RecruitingCandidates.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == candidateId, cancellationToken);
        if (candidate is null)
        {
            throw new StlApiException("recruiting.candidate.not_found", "Recruiting candidate was not found.", 404);
        }

        var now = DateTimeOffset.UtcNow;
        var before = MapCandidate(candidate);
        candidate.Status = "archived";
        candidate.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteWithMetadataAsync(
            "staffarr.recruiting.candidate.archive",
            tenantId,
            actorUserId,
            "recruiting_candidate",
            candidate.Id.ToString(),
            "success",
            JsonSerializer.Serialize(new { before, after = MapCandidate(candidate) }, JsonOptions),
            cancellationToken: cancellationToken);

        return MapCandidate(candidate);
    }

    public async Task<RecruitingCandidateResponse> ConvertSubmissionToCandidateAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid submissionId,
        Guid? recruitingRequisitionId,
        CancellationToken cancellationToken = default)
    {
        var submission = await db.EmploymentApplicationSubmissions
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == submissionId, cancellationToken);

        if (submission is null)
        {
            throw new StlApiException("recruiting.candidate.submission_not_found", "Employment application submission was not found.", 404);
        }

        if (submission.CreatedCandidateId is Guid existingCandidateId)
        {
            var existingCandidate = await db.RecruitingCandidates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == existingCandidateId, cancellationToken);
            if (existingCandidate is not null)
            {
                return MapCandidate(existingCandidate);
            }
        }

        var createRequest = TryReadCreateRequest(submission.CreateRequestJson);
        var candidate = new RecruitingCandidate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecruitingRequisitionId = recruitingRequisitionId,
            EmploymentApplicationSubmissionId = submission.Id,
            PersonId = null,
            CandidateName = submission.ApplicantDisplayName,
            CandidateEmail = submission.ApplicantEmail,
            CandidatePhone = createRequest.CandidatePhone,
            SourceType = "application",
            Stage = "applied",
            Status = "active",
            SourceProductKey = "staffarr.hiring",
            SourceRef = submission.TemplateKey,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        db.RecruitingCandidates.Add(candidate);
        submission.CreatedCandidateId = candidate.Id;
        submission.Status = "candidate_created";
        submission.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteWithMetadataAsync(
            "staffarr.hiring.candidate.convert_from_application",
            tenantId,
            actorUserId,
            "employment_application_submission",
            submission.Id.ToString(),
            "success",
            JsonSerializer.Serialize(new
            {
                submissionId,
                candidateId = candidate.Id,
                requisitionId = recruitingRequisitionId,
                personId = candidate.PersonId,
            }, JsonOptions),
            cancellationToken: cancellationToken);

        return MapCandidate(candidate);
    }

    public async Task<RecruitingCandidateResponse> HireCandidateAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid candidateId,
        CreateStaffPersonRequest request,
        CancellationToken cancellationToken = default)
    {
        var candidate = await db.RecruitingCandidates.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == candidateId,
            cancellationToken);

        if (candidate is null)
        {
            throw new StlApiException("recruiting.candidate.not_found", "Recruiting candidate was not found.", 404);
        }

        if (candidate.PersonId is Guid existingPersonId)
        {
            await audit.WriteWithMetadataAsync(
                "staffarr.hiring.candidate.hire",
                tenantId,
                actorUserId,
                "recruiting_candidate",
                candidate.Id.ToString(),
                "already_linked",
                JsonSerializer.Serialize(new { candidateId = candidate.Id, personId = existingPersonId }, JsonOptions),
                cancellationToken: cancellationToken);

            return MapCandidate(candidate);
        }

        var now = DateTimeOffset.UtcNow;
        var person = await peopleService.CreateAsync(tenantId, actorUserId, request, cancellationToken);
        var submission = candidate.EmploymentApplicationSubmissionId is Guid submissionId
            ? await db.EmploymentApplicationSubmissions.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == submissionId,
                cancellationToken)
            : null;
        var requisition = candidate.RecruitingRequisitionId is Guid requisitionId
            ? await db.RecruitingRequisitions.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == requisitionId,
                cancellationToken)
            : null;

        candidate.PersonId = person.PersonId;
        candidate.Stage = "hired";
        candidate.Status = "hired";
        candidate.UpdatedAt = now;

        if (submission is not null)
        {
            submission.CreatedPersonId = person.PersonId;
            submission.Status = "hired";
            submission.UpdatedAt = now;
        }

        if (requisition is not null)
        {
            requisition.FilledCount = Math.Max(0, requisition.FilledCount + 1);
            requisition.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteWithMetadataAsync(
            "staffarr.hiring.candidate.hire",
            tenantId,
            actorUserId,
            "recruiting_candidate",
            candidate.Id.ToString(),
            "success",
            JsonSerializer.Serialize(new
            {
                candidateId = candidate.Id,
                personId = person.PersonId,
                submissionId = candidate.EmploymentApplicationSubmissionId,
                requisitionId = candidate.RecruitingRequisitionId,
            }, JsonOptions),
            cancellationToken: cancellationToken);

        return MapCandidate(candidate);
    }

    public async Task<IReadOnlyList<RecruitingInterviewStageResponse>> ListInterviewStagesAsync(
        Guid tenantId,
        Guid? recruitingCandidateId,
        CancellationToken cancellationToken = default)
    {
        var query = db.RecruitingInterviewStages
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (recruitingCandidateId is Guid candidateId)
        {
            query = query.Where(x => x.RecruitingCandidateId == candidateId);
        }

        var stages = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.StageName)
            .ToListAsync(cancellationToken);

        return stages.Select(MapInterviewStage).ToArray();
    }

    public async Task<RecruitingInterviewStageResponse> UpsertInterviewStageAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid? stageId,
        UpsertRecruitingInterviewStageRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var stage = stageId is Guid existingId
            ? await db.RecruitingInterviewStages.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == existingId, cancellationToken)
            : null;

        if (stage is null)
        {
            stage = new RecruitingInterviewStage
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.RecruitingInterviewStages.Add(stage);
        }

        var before = stageId is Guid ? MapInterviewStage(stage) : null;
        stage.RecruitingCandidateId = request.RecruitingCandidateId;
        stage.StageName = NormalizeRequiredText(request.StageName, 128, "stage name");
        stage.Status = NormalizeRequiredText(request.Status, 32, "status").ToLowerInvariant();
        stage.ScheduledAt = request.ScheduledAt;
        stage.CompletedAt = request.CompletedAt;
        stage.InterviewerPersonId = request.InterviewerPersonId;
        stage.Score = request.Score;
        stage.Recommendation = NormalizeOptionalText(request.Recommendation, 64);
        stage.Notes = NormalizeOptionalText(request.Notes, 2048);
        stage.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteWithMetadataAsync(
            stageId is null ? "staffarr.recruiting.interview_stage.create" : "staffarr.recruiting.interview_stage.update",
            tenantId,
            actorUserId,
            "recruiting_interview_stage",
            stage.Id.ToString(),
            "success",
            JsonSerializer.Serialize(new { before, after = MapInterviewStage(stage) }, JsonOptions),
            cancellationToken: cancellationToken);

        return MapInterviewStage(stage);
    }

    public async Task<RecruitingInterviewStageResponse> ArchiveInterviewStageAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid stageId,
        CancellationToken cancellationToken = default)
    {
        var stage = await db.RecruitingInterviewStages.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == stageId, cancellationToken);
        if (stage is null)
        {
            throw new StlApiException("recruiting.interview_stage.not_found", "Recruiting interview stage was not found.", 404);
        }

        var now = DateTimeOffset.UtcNow;
        var before = MapInterviewStage(stage);
        stage.Status = "archived";
        stage.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteWithMetadataAsync(
            "staffarr.recruiting.interview_stage.archive",
            tenantId,
            actorUserId,
            "recruiting_interview_stage",
            stage.Id.ToString(),
            "success",
            JsonSerializer.Serialize(new { before, after = MapInterviewStage(stage) }, JsonOptions),
            cancellationToken: cancellationToken);

        return MapInterviewStage(stage);
    }

    public async Task<IReadOnlyList<RecruitingOfferResponse>> ListOffersAsync(
        Guid tenantId,
        Guid? recruitingCandidateId,
        CancellationToken cancellationToken = default)
    {
        var query = db.RecruitingOffers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (recruitingCandidateId is Guid candidateId)
        {
            query = query.Where(x => x.RecruitingCandidateId == candidateId);
        }

        var offers = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return offers.Select(MapOffer).ToArray();
    }

    public async Task<RecruitingOfferResponse> UpsertOfferAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid? offerId,
        UpsertRecruitingOfferRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var offer = offerId is Guid existingId
            ? await db.RecruitingOffers.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == existingId, cancellationToken)
            : null;

        if (offer is null)
        {
            offer = new RecruitingOffer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.RecruitingOffers.Add(offer);
        }

        var before = offerId is Guid ? MapOffer(offer) : null;
        offer.RecruitingCandidateId = request.RecruitingCandidateId;
        offer.Status = NormalizeRequiredText(request.Status, 32, "status").ToLowerInvariant();
        offer.Title = NormalizeRequiredText(request.Title, 200, "title");
        offer.PayBasis = NormalizeRequiredText(request.PayBasis, 32, "pay basis").ToLowerInvariant();
        offer.AnnualSalary = request.AnnualSalary;
        offer.HourlyRate = request.HourlyRate;
        offer.StartDate = request.StartDate;
        offer.ApprovedAt = request.ApprovedAt;
        offer.ApprovedByPersonId = request.ApprovedByPersonId;
        offer.AcceptedAt = request.AcceptedAt;
        offer.DeclinedAt = request.DeclinedAt;
        offer.Notes = NormalizeOptionalText(request.Notes, 2048);
        offer.SourceProductKey = NormalizeOptionalText(request.SourceProductKey, 64);
        offer.SourceRef = NormalizeOptionalText(request.SourceRef, 256);
        offer.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteWithMetadataAsync(
            offerId is null ? "staffarr.recruiting.offer.create" : "staffarr.recruiting.offer.update",
            tenantId,
            actorUserId,
            "recruiting_offer",
            offer.Id.ToString(),
            "success",
            JsonSerializer.Serialize(new { before, after = MapOffer(offer) }, JsonOptions),
            cancellationToken: cancellationToken);

        return MapOffer(offer);
    }

    public async Task<RecruitingOfferResponse> ArchiveOfferAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid offerId,
        CancellationToken cancellationToken = default)
    {
        var offer = await db.RecruitingOffers.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == offerId, cancellationToken);
        if (offer is null)
        {
            throw new StlApiException("recruiting.offer.not_found", "Recruiting offer was not found.", 404);
        }

        var now = DateTimeOffset.UtcNow;
        var before = MapOffer(offer);
        offer.Status = "archived";
        offer.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteWithMetadataAsync(
            "staffarr.recruiting.offer.archive",
            tenantId,
            actorUserId,
            "recruiting_offer",
            offer.Id.ToString(),
            "success",
            JsonSerializer.Serialize(new { before, after = MapOffer(offer) }, JsonOptions),
            cancellationToken: cancellationToken);

        return MapOffer(offer);
    }

    private static RecruitingRequisitionResponse MapRequisition(RecruitingRequisition requisition) =>
        new(
            requisition.Id,
            requisition.RequisitionNumber,
            requisition.Title,
            requisition.JobCode,
            requisition.JobFamily,
            requisition.DepartmentRef,
            requisition.SiteRef,
            requisition.LocationRef,
            requisition.HiringManagerPersonId,
            requisition.RecruiterPersonId,
            requisition.Status,
            requisition.HeadcountRequested,
            requisition.FilledCount,
            requisition.OpenDate,
            requisition.TargetStartDate,
            requisition.SourceProductKey,
            requisition.SourceRef,
            requisition.CreatedAt,
            requisition.UpdatedAt);

    private static RecruitingCandidateResponse MapCandidate(RecruitingCandidate candidate) =>
        new(
            candidate.Id,
            candidate.RecruitingRequisitionId,
            candidate.EmploymentApplicationSubmissionId,
            candidate.PersonId,
            candidate.CandidateName,
            candidate.CandidateEmail,
            candidate.CandidatePhone,
            candidate.SourceType,
            candidate.Stage,
            candidate.Status,
            candidate.BackgroundCheckStatus,
            candidate.DrugScreenStatus,
            candidate.PhysicalStatus,
            candidate.OfferStatus,
            candidate.Score,
            candidate.Notes,
            candidate.SourceProductKey,
            candidate.SourceRef,
            candidate.CreatedAt,
            candidate.UpdatedAt);

    private static RecruitingInterviewStageResponse MapInterviewStage(RecruitingInterviewStage stage) =>
        new(
            stage.Id,
            stage.RecruitingCandidateId,
            stage.StageName,
            stage.Status,
            stage.ScheduledAt,
            stage.CompletedAt,
            stage.InterviewerPersonId,
            stage.Score,
            stage.Recommendation,
            stage.Notes,
            stage.CreatedAt,
            stage.UpdatedAt);

    private static RecruitingOfferResponse MapOffer(RecruitingOffer offer) =>
        new(
            offer.Id,
            offer.RecruitingCandidateId,
            offer.Status,
            offer.Title,
            offer.PayBasis,
            offer.AnnualSalary,
            offer.HourlyRate,
            offer.StartDate,
            offer.ApprovedAt,
            offer.ApprovedByPersonId,
            offer.AcceptedAt,
            offer.DeclinedAt,
            offer.Notes,
            offer.SourceProductKey,
            offer.SourceRef,
            offer.CreatedAt,
            offer.UpdatedAt);

    private static string NormalizeRequiredText(string value, int maxLength, string fieldName)
    {
        var normalized = NormalizeOptionalText(value, maxLength);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException("recruiting.validation", $"{fieldName} is required.", 400);
        }

        return normalized!;
    }

    private static string? NormalizeOptionalText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static CreateStaffPersonRequestSnapshot TryReadCreateRequest(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            return new CreateStaffPersonRequestSnapshot(
                GetOptional(root, "primaryPhone"),
                GetOptional(root, "primaryEmail"));
        }
        catch
        {
            return new CreateStaffPersonRequestSnapshot(null, null);
        }
    }

    private static string? GetOptional(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = property.GetString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record CreateStaffPersonRequestSnapshot(string? CandidatePhone, string? PrimaryEmail);
}
