using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class PerformanceService(StaffArrDbContext db, IStaffArrAuditService audit)
{
    public async Task<IReadOnlyList<PerformanceReviewCycleResponse>> ListReviewCyclesAsync(Guid tenantId, Guid? personId, CancellationToken cancellationToken)
    {
        var query = db.PerformanceReviewCycles.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }

        return await query
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.CreatedAt)
            .Take(200)
            .Select(x => MapCycle(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<PerformanceReviewCycleResponse> GetReviewCycleAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.PerformanceReviewCycles.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken)
            ?? throw new StlApiException("staffarr.performance.review_cycle_not_found", "Performance review cycle was not found.", 404);
        return MapCycle(entity);
    }

    public async Task<PerformanceReviewCycleResponse> UpsertReviewCycleAsync(Guid tenantId, Guid? actorUserId, Guid? id, UpsertPerformanceReviewCycleRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        if (request.ManagerPersonId.HasValue)
        {
            await EnsurePersonAsync(tenantId, request.ManagerPersonId.Value, cancellationToken);
        }

        var entity = id.HasValue
            ? await db.PerformanceReviewCycles.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id.Value, cancellationToken)
            : null;

        if (id.HasValue && entity is null)
        {
            throw new StlApiException("staffarr.performance.review_cycle_not_found", "Performance review cycle was not found.", 404);
        }

        entity ??= new PerformanceReviewCycle { TenantId = tenantId, PersonId = request.PersonId, CreatedAt = DateTimeOffset.UtcNow };
        entity.PersonId = request.PersonId;
        entity.CycleName = Require(request.CycleName, "Cycle name is required.", 128);
        entity.CycleType = NormalizeEnum(request.CycleType, ["annual", "midyear", "quarterly", "probation", "project", "1on1", "other"], "Cycle type");
        entity.Status = NormalizeEnum(request.Status, ["planned", "active", "in_review", "complete", "closed", "cancelled"], "Cycle status");
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.SelfReviewDueDate = request.SelfReviewDueDate;
        entity.ManagerReviewDueDate = request.ManagerReviewDueDate;
        entity.ManagerPersonId = request.ManagerPersonId;
        entity.SelfReviewCompletedAt = request.SelfReviewCompletedAt;
        entity.ManagerReviewCompletedAt = request.ManagerReviewCompletedAt;
        entity.OverallRating = Optional(request.OverallRating, 64);
        entity.PromotionReady = request.PromotionReady;
        entity.SuccessionReady = request.SuccessionReady;
        entity.NextCheckInAt = request.NextCheckInAt;
        entity.Summary = Optional(request.Summary, 2048);
        entity.DevelopmentPlan = Optional(request.DevelopmentPlan, 4096);
        entity.SourceProductKey = Optional(request.SourceProductKey, 64);
        entity.SourceRef = Optional(request.SourceRef, 256);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        if (db.Entry(entity).State == EntityState.Detached)
        {
            db.PerformanceReviewCycles.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("performance.review_cycle.upsert", tenantId, actorUserId, "performance_review_cycle", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapCycle(entity);
    }

    public async Task<IReadOnlyList<PerformanceGoalResponse>> ListGoalsAsync(Guid tenantId, Guid? personId, Guid? cycleId, CancellationToken cancellationToken)
    {
        var query = db.PerformanceGoals.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }
        if (cycleId.HasValue)
        {
            query = query.Where(x => x.PerformanceReviewCycleId == cycleId.Value);
        }

        return await query
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.CreatedAt)
            .Take(250)
            .Select(x => MapGoal(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<PerformanceGoalResponse> UpsertGoalAsync(Guid tenantId, Guid? actorUserId, Guid? id, UpsertPerformanceGoalRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        if (request.PerformanceReviewCycleId.HasValue)
        {
            _ = await db.PerformanceReviewCycles.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.PerformanceReviewCycleId.Value, cancellationToken)
                ?? throw new StlApiException("staffarr.performance.review_cycle_not_found", "Performance review cycle was not found.", 404);
        }

        if (request.OwnerPersonId.HasValue)
        {
            await EnsurePersonAsync(tenantId, request.OwnerPersonId.Value, cancellationToken);
        }

        var entity = id.HasValue
            ? await db.PerformanceGoals.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id.Value, cancellationToken)
            : null;
        if (id.HasValue && entity is null)
        {
            throw new StlApiException("staffarr.performance.goal_not_found", "Performance goal was not found.", 404);
        }

        entity ??= new PerformanceGoal { TenantId = tenantId, PersonId = request.PersonId, CreatedAt = DateTimeOffset.UtcNow };
        entity.PersonId = request.PersonId;
        entity.PerformanceReviewCycleId = request.PerformanceReviewCycleId;
        entity.GoalTitle = Require(request.GoalTitle, "Goal title is required.", 200);
        entity.GoalType = NormalizeEnum(request.GoalType, ["strategic", "operational", "development", "competency", "probation", "pip", "other"], "Goal type");
        entity.Status = NormalizeEnum(request.Status, ["not_started", "in_progress", "at_risk", "blocked", "completed", "cancelled"], "Goal status");
        entity.Priority = NormalizeEnum(request.Priority, ["low", "medium", "high", "critical"], "Goal priority");
        entity.ProgressPercent = Math.Clamp(request.ProgressPercent, 0m, 100m);
        entity.StartDate = request.StartDate;
        entity.TargetDate = request.TargetDate;
        entity.CompletedAt = request.CompletedAt;
        entity.OwnerPersonId = request.OwnerPersonId;
        entity.SuccessMetric = Optional(request.SuccessMetric, 1024);
        entity.Summary = Optional(request.Summary, 2048);
        entity.ResultSummary = Optional(request.ResultSummary, 2048);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        if (db.Entry(entity).State == EntityState.Detached)
        {
            db.PerformanceGoals.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("performance.goal.upsert", tenantId, actorUserId, "performance_goal", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapGoal(entity);
    }

    public async Task<IReadOnlyList<PerformanceCompetencyAssessmentResponse>> ListCompetenciesAsync(Guid tenantId, Guid? personId, Guid? cycleId, CancellationToken cancellationToken)
    {
        var query = db.PerformanceCompetencyAssessments.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }
        if (cycleId.HasValue)
        {
            query = query.Where(x => x.PerformanceReviewCycleId == cycleId.Value);
        }

        return await query
            .OrderBy(x => x.CompetencyName)
            .Select(x => MapCompetency(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<PerformanceCompetencyAssessmentResponse> UpsertCompetencyAsync(Guid tenantId, Guid? actorUserId, Guid? id, UpsertPerformanceCompetencyAssessmentRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        if (request.PerformanceReviewCycleId.HasValue)
        {
            _ = await db.PerformanceReviewCycles.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.PerformanceReviewCycleId.Value, cancellationToken)
                ?? throw new StlApiException("staffarr.performance.review_cycle_not_found", "Performance review cycle was not found.", 404);
        }

        var entity = id.HasValue
            ? await db.PerformanceCompetencyAssessments.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id.Value, cancellationToken)
            : null;
        if (id.HasValue && entity is null)
        {
            throw new StlApiException("staffarr.performance.competency_not_found", "Performance competency assessment was not found.", 404);
        }

        entity ??= new PerformanceCompetencyAssessment { TenantId = tenantId, PersonId = request.PersonId, CreatedAt = DateTimeOffset.UtcNow };
        entity.PersonId = request.PersonId;
        entity.PerformanceReviewCycleId = request.PerformanceReviewCycleId;
        entity.CompetencyKey = Require(request.CompetencyKey, "Competency key is required.", 128).ToLowerInvariant();
        entity.CompetencyName = Require(request.CompetencyName, "Competency name is required.", 200);
        entity.ExpectedLevel = NormalizeEnum(request.ExpectedLevel, ["beginner", "developing", "proficient", "advanced", "expert"], "Expected level");
        entity.CurrentLevel = NormalizeEnum(request.CurrentLevel, ["beginner", "developing", "proficient", "advanced", "expert"], "Current level");
        entity.Rating = NormalizeEnum(request.Rating, ["below", "needs_improvement", "meets", "exceeds", "exceptional"], "Rating");
        entity.Status = NormalizeEnum(request.Status, ["draft", "in_progress", "complete", "final"], "Competency status");
        entity.Notes = Optional(request.Notes, 2048);
        entity.AssessedByPersonId = request.AssessedByPersonId;
        entity.AssessedAt = request.AssessedAt;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        if (db.Entry(entity).State == EntityState.Detached)
        {
            db.PerformanceCompetencyAssessments.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("performance.competency.upsert", tenantId, actorUserId, "performance_competency_assessment", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapCompetency(entity);
    }

    public async Task<IReadOnlyList<PerformanceFeedbackEntryResponse>> ListFeedbackAsync(Guid tenantId, Guid? personId, Guid? cycleId, CancellationToken cancellationToken)
    {
        var query = db.PerformanceFeedbackEntries.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }
        if (cycleId.HasValue)
        {
            query = query.Where(x => x.PerformanceReviewCycleId == cycleId.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(250)
            .Select(x => MapFeedback(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<PerformanceFeedbackEntryResponse> CreateFeedbackAsync(Guid tenantId, Guid? actorUserId, CreatePerformanceFeedbackEntryRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        if (request.PerformanceReviewCycleId.HasValue)
        {
            _ = await db.PerformanceReviewCycles.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.PerformanceReviewCycleId.Value, cancellationToken)
                ?? throw new StlApiException("staffarr.performance.review_cycle_not_found", "Performance review cycle was not found.", 404);
        }

        var entity = new PerformanceFeedbackEntry
        {
            TenantId = tenantId,
            PersonId = request.PersonId,
            PerformanceReviewCycleId = request.PerformanceReviewCycleId,
            FeedbackType = NormalizeEnum(request.FeedbackType, ["self_review", "manager_review", "peer_feedback", "one_on_one", "calibration_note", "promotion_readiness", "succession_readiness"], "Feedback type"),
            Visibility = NormalizeEnum(request.Visibility, ["employee", "manager", "hr", "restricted"], "Feedback visibility"),
            Subject = Require(request.Subject, "Feedback subject is required.", 200),
            Body = Require(request.Body, "Feedback body is required.", 4096),
            Sentiment = Optional(request.Sentiment, 32),
            AuthorPersonId = request.AuthorPersonId,
            RelatedPersonId = request.RelatedPersonId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        db.PerformanceFeedbackEntries.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("performance.feedback.create", tenantId, actorUserId, "performance_feedback_entry", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapFeedback(entity);
    }

    public async Task<IReadOnlyList<PerformanceImprovementPlanResponse>> ListImprovementPlansAsync(Guid tenantId, Guid? personId, CancellationToken cancellationToken)
    {
        var query = db.PerformanceImprovementPlans.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }

        return await query
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.CreatedAt)
            .Take(100)
            .Select(x => MapPlan(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<PerformanceImprovementPlanResponse> UpsertImprovementPlanAsync(Guid tenantId, Guid? actorUserId, Guid? id, UpsertPerformanceImprovementPlanRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        if (request.ManagerPersonId.HasValue)
        {
            await EnsurePersonAsync(tenantId, request.ManagerPersonId.Value, cancellationToken);
        }

        var entity = id.HasValue
            ? await db.PerformanceImprovementPlans.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id.Value, cancellationToken)
            : null;
        if (id.HasValue && entity is null)
        {
            throw new StlApiException("staffarr.performance.plan_not_found", "Performance improvement plan was not found.", 404);
        }

        entity ??= new PerformanceImprovementPlan { TenantId = tenantId, PersonId = request.PersonId, CreatedAt = DateTimeOffset.UtcNow };
        entity.PersonId = request.PersonId;
        entity.PlanName = Require(request.PlanName, "Plan name is required.", 200);
        entity.Status = NormalizeEnum(request.Status, ["draft", "active", "paused", "completed", "closed"], "Plan status");
        entity.StartDate = request.StartDate;
        entity.TargetDate = request.TargetDate;
        entity.CheckInCadence = Optional(request.CheckInCadence, 64);
        entity.NextCheckInAt = request.NextCheckInAt;
        entity.ManagerPersonId = request.ManagerPersonId;
        entity.HrOwnerPersonId = request.HrOwnerPersonId;
        entity.Summary = Optional(request.Summary, 2048);
        entity.Expectations = Optional(request.Expectations, 4096);
        entity.SuccessCriteria = Optional(request.SuccessCriteria, 4096);
        entity.SourceProductKey = Optional(request.SourceProductKey, 64);
        entity.SourceRef = Optional(request.SourceRef, 256);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        if (db.Entry(entity).State == EntityState.Detached)
        {
            db.PerformanceImprovementPlans.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("performance.plan.upsert", tenantId, actorUserId, "performance_improvement_plan", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapPlan(entity);
    }

    private async Task EnsurePersonAsync(Guid tenantId, Guid personId, CancellationToken cancellationToken)
    {
        var exists = await db.People.AnyAsync(x => x.TenantId == tenantId && x.Id == personId, cancellationToken);
        if (!exists)
        {
            throw new StlApiException("staffarr.performance.person_not_found", "Person record was not found.", 404);
        }
    }

    private static string Require(string? value, string message, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("staffarr.performance.validation", message, 400);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException("staffarr.performance.validation", $"{message.TrimEnd('.')} must be {maxLength} characters or less.", 400);
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
            throw new StlApiException("staffarr.performance.validation", $"Value must be {maxLength} characters or less.", 400);
        }

        return normalized;
    }

    private static string NormalizeEnum(string value, IReadOnlyCollection<string> allowed, string fieldName)
    {
        var normalized = Require(value, $"{fieldName} is required.", 64).ToLowerInvariant();
        if (!allowed.Contains(normalized))
        {
            throw new StlApiException("staffarr.performance.validation", $"{fieldName} is invalid.", 400);
        }

        return normalized;
    }

    private static PerformanceReviewCycleResponse MapCycle(PerformanceReviewCycle x) =>
        new(x.Id, x.PersonId, x.CycleName, x.CycleType, x.Status, x.StartDate, x.EndDate, x.SelfReviewDueDate, x.ManagerReviewDueDate, x.ManagerPersonId, x.SelfReviewCompletedAt, x.ManagerReviewCompletedAt, x.OverallRating, x.PromotionReady, x.SuccessionReady, x.NextCheckInAt, x.Summary, x.DevelopmentPlan, x.SourceProductKey, x.SourceRef, x.CreatedAt, x.UpdatedAt);

    private static PerformanceGoalResponse MapGoal(PerformanceGoal x) =>
        new(x.Id, x.PersonId, x.PerformanceReviewCycleId, x.GoalTitle, x.GoalType, x.Status, x.Priority, x.ProgressPercent, x.StartDate, x.TargetDate, x.CompletedAt, x.OwnerPersonId, x.SuccessMetric, x.Summary, x.ResultSummary, x.CreatedAt, x.UpdatedAt);

    private static PerformanceCompetencyAssessmentResponse MapCompetency(PerformanceCompetencyAssessment x) =>
        new(x.Id, x.PersonId, x.PerformanceReviewCycleId, x.CompetencyKey, x.CompetencyName, x.ExpectedLevel, x.CurrentLevel, x.Rating, x.Status, x.Notes, x.AssessedByPersonId, x.AssessedAt, x.CreatedAt, x.UpdatedAt);

    private static PerformanceFeedbackEntryResponse MapFeedback(PerformanceFeedbackEntry x) =>
        new(x.Id, x.PersonId, x.PerformanceReviewCycleId, x.FeedbackType, x.Visibility, x.Subject, x.Body, x.Sentiment, x.AuthorPersonId, x.RelatedPersonId, x.CreatedAt, x.UpdatedAt);

    private static PerformanceImprovementPlanResponse MapPlan(PerformanceImprovementPlan x) =>
        new(x.Id, x.PersonId, x.PlanName, x.Status, x.StartDate, x.TargetDate, x.CheckInCadence, x.NextCheckInAt, x.ManagerPersonId, x.HrOwnerPersonId, x.Summary, x.Expectations, x.SuccessCriteria, x.SourceProductKey, x.SourceRef, x.CreatedAt, x.UpdatedAt);
}
