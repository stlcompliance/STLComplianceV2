using Microsoft.EntityFrameworkCore;

using TrainArr.Api.Contracts;

using TrainArr.Api.Data;

using TrainArr.Api.Entities;

using STLCompliance.Shared.Contracts;



namespace TrainArr.Api.Services;



public sealed class QualificationCheckService(

    TrainArrDbContext db,

    ComplianceCoreRuleEvaluationClient complianceCoreClient,

    TrainingRulePackRequirementService rulePackRequirementService,

    IntegrationSettingsService integrationSettingsService,

    ITrainArrAuditService auditService)

{

    public const string SingleCheckAction = "qualification_check.run";

    public const string BatchCheckAction = "qualification_check.batch_run";

    public const string SingleCheckSnapshotKind = "qualification_check";

    public const string BatchCheckSnapshotKind = "qualification_check_batch";

    public const int MaxBatchSubjects = 100;

    public const int DefaultHistoryLimit = 25;

    public const int MaxHistoryLimit = 100;

    public static readonly TimeSpan AssignmentCheckValidity = TimeSpan.FromMinutes(30);



    public async Task<QualificationCheckResponse> CheckAsync(

        Guid tenantId,

        Guid? actorUserId,

        CreateQualificationCheckRequest request,

        CancellationToken cancellationToken = default)

    {

        var personId = request.StaffarrPersonId;
        var evaluationAt = request.EffectiveAt ?? DateTimeOffset.UtcNow;

        if (personId == Guid.Empty)

        {

            throw new StlApiException(

                "qualification_checks.validation",

                "StaffArr person id is required.",

                400);

        }



        var qualificationKey = NormalizeQualificationKey(request.QualificationKey);

        var localStates = await LoadLocalQualificationStatesAsync(

            tenantId,

            [personId],

            qualificationKey,

            evaluationAt,

            cancellationToken);

        localStates.TryGetValue(personId, out var localState);

        localState ??= NoLocalQualificationState();



        var rulePackKey = await ResolveRulePackKeyAsync(

            tenantId,

            request.RulePackKey,

            request.TrainingDefinitionId,

            request.TrainingProgramId,

            qualificationKey,

            cancellationToken);



        var complianceSummary = await EvaluateComplianceCoreAsync(

            tenantId,

            rulePackKey,

            request.Context,

            cancellationToken);



        var result = BuildCheckResponse(

            Guid.NewGuid(),

            personId,

            qualificationKey,

            localState,

            complianceSummary);

        result = result with
        {
            AuthorizationGuidance = await BuildAuthorizationGuidanceAsync(
                tenantId,
                personId,
                qualificationKey,
                result,
                request.TrainingDefinitionId,
                request.TrainingProgramId,
                cancellationToken)
        };



        var auditResult = await auditService.WriteAsync(

            SingleCheckAction,

            tenantId,

            actorUserId,

            "qualification_check",

            result.CheckId.ToString(),

            result.Outcome,

            reasonCode: result.ReasonCode,

            cancellationToken: cancellationToken);

        result = result with
        {
            AuditSnapshot = new QualificationCheckAuditSnapshotResponse(
                auditResult.AuditEventId,
                SingleCheckSnapshotKind,
                auditResult.OccurredAt)
        };



        await PersistRecordAsync(

            tenantId,

            actorUserId,

            result,

            rulePackKey,

            request.TrainingDefinitionId,

            request.TrainingProgramId,

            batchId: null,

            cancellationToken);



        return result;

    }



    public async Task<BatchQualificationCheckResponse> CheckBatchAsync(

        Guid tenantId,

        Guid? actorUserId,

        CreateBatchQualificationCheckRequest request,

        CancellationToken cancellationToken = default)

    {

        if (request.Subjects is null || request.Subjects.Count == 0)

        {

            throw new StlApiException(

                "qualification_checks.batch_validation",

                "At least one subject is required for a batch qualification check.",

                400);

        }



        if (request.Subjects.Count > MaxBatchSubjects)

        {

            throw new StlApiException(

                "qualification_checks.batch_validation",

                $"Batch qualification checks are limited to {MaxBatchSubjects} subjects per request.",

                400);

        }



        var qualificationKey = NormalizeQualificationKey(request.QualificationKey);
        var evaluationAt = request.EffectiveAt ?? DateTimeOffset.UtcNow;

        var distinctSubjects = request.Subjects

            .Where(subject => subject.StaffarrPersonId != Guid.Empty)

            .GroupBy(subject => subject.StaffarrPersonId)

            .Select(group => group.Last())

            .ToList();



        if (distinctSubjects.Count == 0)

        {

            throw new StlApiException(

                "qualification_checks.batch_validation",

                "Each subject must include a valid StaffArr person id.",

                400);

        }



        var personIds = distinctSubjects.Select(subject => subject.StaffarrPersonId).ToList();

        var localStates = await LoadLocalQualificationStatesAsync(

            tenantId,

            personIds,

            qualificationKey,

            evaluationAt,

            cancellationToken);



        var batchId = Guid.NewGuid();

        var resolvedRulePackKey = await ResolveRulePackKeyAsync(

            tenantId,

            request.RulePackKey,

            request.TrainingDefinitionId,

            request.TrainingProgramId,

            qualificationKey,

            cancellationToken);

        var hasRulePack = !string.IsNullOrWhiteSpace(resolvedRulePackKey);

        IReadOnlyList<ComplianceCoreInternalEvaluateResult>? complianceEvaluations = null;

        if (hasRulePack)

        {

            var batchEvaluation = await complianceCoreClient.EvaluateRulePackBatchAsync(

                new ComplianceCoreInternalEvaluateBatchPayload(

                    tenantId,

                    distinctSubjects

                        .Select(subject => new ComplianceCoreInternalEvaluateBatchItem(

                            resolvedRulePackKey!,

                            subject.Context))

                        .ToList()),

                cancellationToken);

            complianceEvaluations = batchEvaluation.Results;

        }



        var results = new List<QualificationCheckResponse>(distinctSubjects.Count);

        for (var index = 0; index < distinctSubjects.Count; index++)

        {

            var subject = distinctSubjects[index];

            localStates.TryGetValue(subject.StaffarrPersonId, out var localState);

            localState ??= NoLocalQualificationState();



            ComplianceCoreCheckSummaryResponse? complianceSummary = null;

            if (complianceEvaluations is not null)

            {

                complianceSummary = ToComplianceSummary(complianceEvaluations[index]);

            }



            results.Add(BuildCheckResponse(

                Guid.NewGuid(),

                subject.StaffarrPersonId,

                qualificationKey,

                localState,

                complianceSummary));

            var subjectResult = results[^1];

            subjectResult = subjectResult with
            {
                AuthorizationGuidance = await BuildAuthorizationGuidanceAsync(
                    tenantId,
                    subject.StaffarrPersonId,
                    qualificationKey,
                    subjectResult,
                    request.TrainingDefinitionId,
                    request.TrainingProgramId,
                    cancellationToken)
            };
            results[^1] = subjectResult;

            await PersistRecordAsync(

                tenantId,

                actorUserId,

                subjectResult,

                resolvedRulePackKey,

                request.TrainingDefinitionId,

                request.TrainingProgramId,

                batchId,

                cancellationToken);

        }

        var summary = new BatchQualificationCheckSummary(

            results.Count,

            results.Count(item => item.Outcome == QualificationCheckOutcomes.Allow),

            results.Count(item => item.Outcome == QualificationCheckOutcomes.Warn),

            results.Count(item => item.Outcome == QualificationCheckOutcomes.Block),

            results.Count(item => item.Outcome == QualificationCheckOutcomes.Waived));



        var batchAuditResult = await auditService.WriteAsync(

            BatchCheckAction,

            tenantId,

            actorUserId,

            "qualification_check_batch",

            batchId.ToString(),

            $"total={summary.Total};allow={summary.AllowCount};warn={summary.WarnCount};block={summary.BlockCount}",

            reasonCode: qualificationKey,

            cancellationToken: cancellationToken);



        return new BatchQualificationCheckResponse(
            batchId,
            qualificationKey,
            results,
            summary,
            new QualificationCheckAuditSnapshotResponse(
                batchAuditResult.AuditEventId,
                BatchCheckSnapshotKind,
                batchAuditResult.OccurredAt));

    }

    public async Task<QualificationCheckResponse> EvaluateIssueAsync(
        Guid tenantId,
        QualificationIssue issue,
        Guid? trainingDefinitionId,
        CancellationToken cancellationToken = default)
    {
        var qualificationKey = issue.QualificationKey.Trim().ToLowerInvariant();
        var localState = MapLocalState(issue);
        var rulePackKey = await ResolveRulePackKeyAsync(
            tenantId,
            null,
            trainingDefinitionId,
            null,
            qualificationKey,
            cancellationToken);

        var complianceSummary = await EvaluateComplianceCoreAsync(
            tenantId,
            rulePackKey,
            null,
            cancellationToken);

        var result = BuildCheckResponse(
            Guid.NewGuid(),
            issue.StaffarrPersonId,
            qualificationKey,
            localState,
            complianceSummary);

        return result with
        {
            AuthorizationGuidance = await BuildAuthorizationGuidanceAsync(
                tenantId,
                issue.StaffarrPersonId,
                qualificationKey,
                result,
                trainingDefinitionId,
                null,
                cancellationToken)
        };
    }



    private async Task<string?> ResolveRulePackKeyAsync(

        Guid tenantId,

        string? explicitRulePackKey,

        Guid? trainingDefinitionId,

        Guid? trainingProgramId,

        string qualificationKey,

        CancellationToken cancellationToken)

    {

        if (!string.IsNullOrWhiteSpace(explicitRulePackKey))

        {

            return explicitRulePackKey.Trim();

        }



        return await rulePackRequirementService.ResolveRulePackKeyAsync(

            tenantId,

            trainingDefinitionId,

            trainingProgramId,

            qualificationKey,

            cancellationToken);

    }



    private async Task<ComplianceCoreCheckSummaryResponse?> EvaluateComplianceCoreAsync(

        Guid tenantId,

        string? rulePackKey,

        IReadOnlyDictionary<string, string>? context,

        CancellationToken cancellationToken)

    {

        if (string.IsNullOrWhiteSpace(rulePackKey))

        {

            return null;

        }



        await integrationSettingsService.EnsureComplianceCoreQualificationChecksEnabledAsync(

            tenantId,

            cancellationToken);



        var evaluation = await complianceCoreClient.EvaluateRulePackAsync(

            new ComplianceCoreInternalEvaluatePayload(

                tenantId,

                rulePackKey.Trim(),

                context),

            cancellationToken);



        return ToComplianceSummary(evaluation);

    }



    private static ComplianceCoreCheckSummaryResponse ToComplianceSummary(

        ComplianceCoreInternalEvaluateResult evaluation) =>

        new(

            evaluation.RulePackKey,

            evaluation.Outcome,

            evaluation.ReasonCode,

            evaluation.Message,

            evaluation.EvaluationResult,

            evaluation.UnresolvedFactKeys,

            evaluation.AppliedWaiverId,

            evaluation.AppliedWaiverKey);



    private static QualificationCheckResponse BuildCheckResponse(

        Guid checkId,

        Guid personId,

        string qualificationKey,

        QualificationLocalStateResponse localState,

        ComplianceCoreCheckSummaryResponse? complianceSummary)

    {

        var (outcome, reasonCode, message) = MergeOutcomes(localState, complianceSummary);

        return new QualificationCheckResponse(

            checkId,

            personId,

            qualificationKey,

            outcome,

            reasonCode,

            message,

            localState,

            complianceSummary,

            BuildDependencyFacts(complianceSummary));

    }

    private static IReadOnlyList<QualificationDependencyFactResponse> BuildDependencyFacts(
        ComplianceCoreCheckSummaryResponse? complianceSummary)
    {
        if (complianceSummary is null || complianceSummary.UnresolvedFactKeys.Count == 0)
        {
            return [];
        }

        return complianceSummary.UnresolvedFactKeys
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(factKey => new QualificationDependencyFactResponse(
                factKey,
                "missing",
                $"Compliance dependency fact '{factKey}' is missing or unresolved."))
            .ToList();
    }

    private async Task<QualificationAuthorizationGuidanceResponse?> BuildAuthorizationGuidanceAsync(
        Guid tenantId,
        Guid personId,
        string qualificationKey,
        QualificationCheckResponse result,
        Guid? requestedTrainingDefinitionId,
        Guid? requestedTrainingProgramId,
        CancellationToken cancellationToken)
    {
        if (string.Equals(result.Outcome, QualificationCheckOutcomes.Allow, StringComparison.OrdinalIgnoreCase)
            || string.Equals(result.Outcome, QualificationCheckOutcomes.Waived, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var definition = await ResolveGuidanceDefinitionAsync(
            tenantId,
            qualificationKey,
            requestedTrainingDefinitionId,
            requestedTrainingProgramId,
            cancellationToken);
        var program = await ResolveGuidanceProgramAsync(
            tenantId,
            definition?.Id,
            requestedTrainingProgramId,
            cancellationToken);

        TrainingAssignment? assignment = null;
        if (definition is not null)
        {
            assignment = await db.TrainingAssignments
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId
                    && x.StaffarrPersonId == personId
                    && x.TrainingDefinitionId == definition.Id)
                .OrderByDescending(x => TrainingAssignmentService.ActiveAssignmentStatuses.Contains(x.Status))
                .ThenByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var assignmentStatus = assignment?.Status ?? "not_assigned";
        var missingQualification = result.LocalQualification?.Status switch
        {
            "expired" => $"Qualification '{qualificationKey}' is expired.",
            "suspended" => $"Qualification '{qualificationKey}' is suspended.",
            "revoked" => $"Qualification '{qualificationKey}' is revoked.",
            "issued" => result.ComplianceCore is null
                ? $"Qualification '{qualificationKey}' requires review."
                : $"Qualification '{qualificationKey}' has unresolved compliance requirements.",
            _ => $"Qualification '{qualificationKey}' has not been issued."
        };

        return new QualificationAuthorizationGuidanceResponse(
            BuildGuidanceBlockReason(result),
            missingQualification,
            definition?.Id,
            definition?.Name,
            program?.Id,
            program?.Name,
            assignmentStatus,
            assignment?.Id,
            assignment?.DueAt,
            BuildGuidanceNextAction(assignmentStatus, definition, program, result),
            BuildGuidanceSupervisorAction(assignmentStatus, result),
            BuildGuidanceEstimatedPath(assignmentStatus, assignment, definition, program, result));
    }

    private async Task<TrainingDefinition?> ResolveGuidanceDefinitionAsync(
        Guid tenantId,
        string qualificationKey,
        Guid? requestedTrainingDefinitionId,
        Guid? requestedTrainingProgramId,
        CancellationToken cancellationToken)
    {
        if (requestedTrainingDefinitionId is { } definitionId && definitionId != Guid.Empty)
        {
            return await db.TrainingDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == definitionId, cancellationToken);
        }

        if (requestedTrainingProgramId is { } programId && programId != Guid.Empty)
        {
            var programDefinition = await db.TrainingProgramDefinitions
                .AsNoTracking()
                .Include(x => x.TrainingDefinition)
                .Where(x => x.TrainingProgram.TenantId == tenantId
                    && x.TrainingProgramId == programId
                    && x.TrainingDefinition.QualificationKey == qualificationKey)
                .OrderBy(x => x.SortOrder)
                .FirstOrDefaultAsync(cancellationToken);

            if (programDefinition is not null)
            {
                return programDefinition.TrainingDefinition;
            }
        }

        return await db.TrainingDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.QualificationKey == qualificationKey
                && x.Status == "active")
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<TrainingProgram?> ResolveGuidanceProgramAsync(
        Guid tenantId,
        Guid? trainingDefinitionId,
        Guid? requestedTrainingProgramId,
        CancellationToken cancellationToken)
    {
        if (requestedTrainingProgramId is { } programId && programId != Guid.Empty)
        {
            return await db.TrainingPrograms
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == programId, cancellationToken);
        }

        if (trainingDefinitionId is not { } definitionId)
        {
            return null;
        }

        return await db.TrainingPrograms
            .AsNoTracking()
            .Include(x => x.ProgramDefinitions)
            .Where(x => x.TenantId == tenantId
                && x.Status == "published"
                && x.ProgramDefinitions.Any(link => link.TrainingDefinitionId == definitionId))
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string BuildGuidanceBlockReason(QualificationCheckResponse result)
    {
        if (result.DependencyFacts?.Count > 0)
        {
            return "Missing or stale dependency facts prevent a clean authorization decision.";
        }

        if (result.ComplianceCore is { Outcome: QualificationCheckOutcomes.Block })
        {
            return result.ComplianceCore.Message;
        }

        return result.LocalQualification?.Status switch
        {
            "expired" => "The local qualification is expired.",
            "suspended" => "The local qualification is suspended.",
            "revoked" => "The local qualification is revoked.",
            "none" => "No local qualification has been issued.",
            _ => result.Message
        };
    }

    private static string BuildGuidanceNextAction(
        string assignmentStatus,
        TrainingDefinition? definition,
        TrainingProgram? program,
        QualificationCheckResponse result)
    {
        if (result.DependencyFacts?.Count > 0)
        {
            return "Resolve the missing dependency facts, then rerun the authorization check.";
        }

        if (TrainingAssignmentService.ActiveAssignmentStatuses.Contains(assignmentStatus))
        {
            return "Continue and complete the active training assignment.";
        }

        if (definition is not null)
        {
            var trainingName = program?.Name ?? definition.Name;
            return $"Assign or restart '{trainingName}' for this person.";
        }

        return "Create or select a training definition that issues this qualification, then assign it.";
    }

    private static string BuildGuidanceSupervisorAction(
        string assignmentStatus,
        QualificationCheckResponse result)
    {
        if (result.DependencyFacts?.Count > 0)
        {
            return "Provide the missing facts or confirm the source system can publish them.";
        }

        if (TrainingAssignmentService.ActiveAssignmentStatuses.Contains(assignmentStatus))
        {
            return "Review assignment progress and remove operational access until qualification is restored.";
        }

        if (string.Equals(result.Outcome, QualificationCheckOutcomes.Block, StringComparison.OrdinalIgnoreCase))
        {
            return "Keep the person blocked from this work until training or review restores authorization.";
        }

        return "Review whether a training assignment, waiver, or manual qualification review is appropriate.";
    }

    private static string BuildGuidanceEstimatedPath(
        string assignmentStatus,
        TrainingAssignment? assignment,
        TrainingDefinition? definition,
        TrainingProgram? program,
        QualificationCheckResponse result)
    {
        if (result.DependencyFacts?.Count > 0)
        {
            return "Available after dependency facts are resolved.";
        }

        if (TrainingAssignmentService.ActiveAssignmentStatuses.Contains(assignmentStatus))
        {
            return assignment?.DueAt is DateTimeOffset dueAt
                ? $"Complete active assignment by {dueAt:u}, then qualification can be rechecked."
                : "Complete the active assignment, then qualification can be rechecked.";
        }

        var trainingName = program?.Name ?? definition?.Name;
        return string.IsNullOrWhiteSpace(trainingName)
            ? "Define, assign, complete, and issue the required qualification."
            : $"Assign and complete '{trainingName}', then issue or recalculate the qualification.";
    }



    private async Task<Dictionary<Guid, QualificationLocalStateResponse>> LoadLocalQualificationStatesAsync(

        Guid tenantId,

        IReadOnlyList<Guid> personIds,

        string qualificationKey,

        DateTimeOffset evaluationAt,

        CancellationToken cancellationToken)

    {

        if (personIds.Count == 0)

        {

            return new Dictionary<Guid, QualificationLocalStateResponse>();

        }



        var issues = await db.QualificationIssues

            .AsNoTracking()

            .Where(x =>

                x.TenantId == tenantId

                && personIds.Contains(x.StaffarrPersonId)

                && x.QualificationKey == qualificationKey

                && x.IssuedAt <= evaluationAt)

            .OrderByDescending(x => x.IssuedAt)

            .ToListAsync(cancellationToken);



        var latestByPerson = new Dictionary<Guid, QualificationIssue>();

        foreach (var issue in issues)

        {

            if (!latestByPerson.ContainsKey(issue.StaffarrPersonId))

            {

                latestByPerson[issue.StaffarrPersonId] = issue;

            }

        }



        return latestByPerson.ToDictionary(

            pair => pair.Key,

            pair => MapLocalState(pair.Value, evaluationAt));

    }



    private static QualificationLocalStateResponse NoLocalQualificationState() =>

        new(

            null,

            "none",

            "No TrainArr qualification issue exists for this person and qualification key.");



    private static QualificationLocalStateResponse MapLocalState(
        QualificationIssue issue,
        DateTimeOffset? evaluationAt = null)

    {

        var effectiveAt = evaluationAt ?? DateTimeOffset.UtcNow;
        var status = issue.Status.Trim().ToLowerInvariant();
        if (status == "issued"
            && issue.ExpiresAt is DateTimeOffset expiresAt
            && expiresAt <= effectiveAt)
        {
            status = "expired";
        }

        var message = status switch

        {

            "issued" =>

                $"Active qualification '{issue.QualificationName}' was issued on {issue.IssuedAt:u}.",

            "suspended" =>

                $"Qualification '{issue.QualificationName}' is suspended and cannot authorize work.",

            "revoked" =>

                $"Qualification '{issue.QualificationName}' was revoked and cannot authorize work.",

            "expired" =>

                $"Qualification '{issue.QualificationName}' expired and must be renewed through training.",

            _ =>

                $"Qualification '{issue.QualificationName}' has status '{status}'.",

        };



        return new QualificationLocalStateResponse(issue.Id, status, message);

    }



    private static (string Outcome, string ReasonCode, string Message) MergeOutcomes(

        QualificationLocalStateResponse localState,

        ComplianceCoreCheckSummaryResponse? complianceSummary)

    {

        var localOutcome = MapLocalOutcome(localState.Status);

        var complianceOutcome = complianceSummary?.Outcome;



        var outcome = MergeOutcome(localOutcome, complianceOutcome);

        var reasonCode = outcome switch

        {

            QualificationCheckOutcomes.Block when localOutcome == QualificationCheckOutcomes.Block =>

                $"local_{localState.Status}",

            QualificationCheckOutcomes.Block =>

                complianceSummary?.ReasonCode ?? "compliance_blocked",

            QualificationCheckOutcomes.Warn when localOutcome == QualificationCheckOutcomes.Warn =>

                "local_no_qualification",

            QualificationCheckOutcomes.Warn =>

                complianceSummary?.ReasonCode ?? "compliance_warn",

            _ =>

                complianceSummary?.ReasonCode ?? $"local_{localState.Status}",

        };



        var parts = new List<string> { localState.Message };

        if (complianceSummary is not null)

        {

            parts.Add(complianceSummary.Message);

        }



        var message = outcome switch

        {

            QualificationCheckOutcomes.Allow =>

                "Authorization check passed. " + string.Join(" ", parts),

            QualificationCheckOutcomes.Warn =>

                "Authorization check returned warnings. " + string.Join(" ", parts),

            QualificationCheckOutcomes.Waived =>

                "Authorization check passed with a Compliance Core waiver. " + string.Join(" ", parts),

            _ =>

                "Authorization check blocked. " + string.Join(" ", parts),

        };



        return (outcome, reasonCode, message.Trim());

    }



    private static string MapLocalOutcome(string localStatus) =>

        localStatus switch

        {

            "issued" => QualificationCheckOutcomes.Allow,

            "suspended" or "revoked" or "expired" => QualificationCheckOutcomes.Block,

            _ => QualificationCheckOutcomes.Warn,

        };



    private static string MaxSeverity(string left, string? right)

    {

        var leftRank = SeverityRank(left);

        var rightRank = right is null ? -1 : SeverityRank(right);

        return rightRank > leftRank ? right! : left;

    }

    private static string MergeOutcome(string localOutcome, string? complianceOutcome)
    {
        if (string.Equals(complianceOutcome, QualificationCheckOutcomes.Waived, StringComparison.OrdinalIgnoreCase)
            && string.Equals(localOutcome, QualificationCheckOutcomes.Allow, StringComparison.OrdinalIgnoreCase))
        {
            return QualificationCheckOutcomes.Waived;
        }

        return MaxSeverity(localOutcome, complianceOutcome);
    }



    private static int SeverityRank(string outcome) =>

        outcome switch

        {

            QualificationCheckOutcomes.Block => 2,

            QualificationCheckOutcomes.Warn => 1,

            QualificationCheckOutcomes.Waived => 0,

            _ => 0,

        };



    private static string NormalizeQualificationKey(string qualificationKey)

    {

        var normalized = qualificationKey.Trim().ToLowerInvariant();

        if (normalized.Length == 0)

        {

            throw new StlApiException(

                "qualification_checks.validation",

                "Qualification key is required.",

                400);

        }



        return normalized;

    }



    public async Task<IReadOnlyList<QualificationCheckHistoryItemResponse>> ListRecentAsync(

        Guid tenantId,

        Guid? staffarrPersonId,

        string? qualificationKey,

        int? limit,

        CancellationToken cancellationToken = default)

    {

        var take = Math.Clamp(limit ?? DefaultHistoryLimit, 1, MaxHistoryLimit);

        var query = db.QualificationCheckRecords

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId);



        if (staffarrPersonId is Guid personId)

        {

            query = query.Where(x => x.StaffarrPersonId == personId);

        }



        if (!string.IsNullOrWhiteSpace(qualificationKey))

        {

            var normalizedKey = qualificationKey.Trim().ToLowerInvariant();

            query = query.Where(x => x.QualificationKey == normalizedKey);

        }



        return await query

            .OrderByDescending(x => x.CheckedAt)

            .Take(take)

            .Select(x => new QualificationCheckHistoryItemResponse(

                x.Id,

                x.StaffarrPersonId,

                x.QualificationKey,

                x.Outcome,

                x.ReasonCode,

                x.Message,

                x.RulePackKey,

                x.TrainingDefinitionId,

                x.BatchId,

                x.CheckedAt))

            .ToListAsync(cancellationToken);

    }



    public async Task ValidateForAssignmentAsync(

        Guid tenantId,

        Guid authorizationQualificationCheckId,

        Guid staffarrPersonId,

        string qualificationKey,

        CancellationToken cancellationToken = default)

    {

        var normalizedKey = NormalizeQualificationKey(qualificationKey);

        var record = await db.QualificationCheckRecords

            .AsNoTracking()

            .FirstOrDefaultAsync(

                x => x.TenantId == tenantId && x.Id == authorizationQualificationCheckId,

                cancellationToken);



        if (record is null)

        {

            throw new StlApiException(

                "assignments.qualification_check_not_found",

                "Authorization qualification check was not found.",

                404);

        }



        if (record.StaffarrPersonId != staffarrPersonId)

        {

            throw new StlApiException(

                "assignments.qualification_check_person_mismatch",

                "Authorization qualification check does not match the assignment person.",

                400);

        }



        if (!string.Equals(record.QualificationKey, normalizedKey, StringComparison.Ordinal))

        {

            throw new StlApiException(

                "assignments.qualification_check_key_mismatch",

                "Authorization qualification check does not match the training definition qualification key.",

                400);

        }



        if (DateTimeOffset.UtcNow - record.CheckedAt > AssignmentCheckValidity)

        {

            throw new StlApiException(

                "assignments.qualification_check_expired",

                "Authorization qualification check is too old. Run a new check before creating the assignment.",

                409);

        }



        if (string.Equals(record.Outcome, QualificationCheckOutcomes.Block, StringComparison.OrdinalIgnoreCase))

        {

            throw new StlApiException(

                "assignments.qualification_check_blocked",

                "Assignment creation is blocked because the authorization check outcome is block.",

                409);

        }

    }



    private async Task PersistRecordAsync(

        Guid tenantId,

        Guid? actorUserId,

        QualificationCheckResponse result,

        string? rulePackKey,

        Guid? trainingDefinitionId,

        Guid? trainingProgramId,

        Guid? batchId,

        CancellationToken cancellationToken)

    {

        db.QualificationCheckRecords.Add(new QualificationCheckRecord

        {

            Id = result.CheckId,

            TenantId = tenantId,

            StaffarrPersonId = result.StaffarrPersonId,

            QualificationKey = result.QualificationKey,

            Outcome = result.Outcome,

            ReasonCode = result.ReasonCode,

            Message = result.Message,

            RulePackKey = rulePackKey,

            TrainingDefinitionId = trainingDefinitionId,

            TrainingProgramId = trainingProgramId,

            ActorUserId = actorUserId,

            BatchId = batchId,

            CheckedAt = DateTimeOffset.UtcNow,

        });



        await db.SaveChangesAsync(cancellationToken);

    }

}


