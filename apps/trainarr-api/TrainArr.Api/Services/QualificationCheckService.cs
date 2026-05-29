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



        await auditService.WriteAsync(

            "qualification_check.run",

            tenantId,

            actorUserId,

            "qualification_check",

            result.CheckId.ToString(),

            result.Outcome,

            reasonCode: result.ReasonCode,

            cancellationToken: cancellationToken);



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

            results.Count(item => item.Outcome == QualificationCheckOutcomes.Block));



        await auditService.WriteAsync(

            "qualification_check.batch_run",

            tenantId,

            actorUserId,

            "qualification_check_batch",

            batchId.ToString(),

            $"total={summary.Total};allow={summary.AllowCount};warn={summary.WarnCount};block={summary.BlockCount}",

            reasonCode: qualificationKey,

            cancellationToken: cancellationToken);



        return new BatchQualificationCheckResponse(batchId, qualificationKey, results, summary);

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

        return BuildCheckResponse(
            Guid.NewGuid(),
            issue.StaffarrPersonId,
            qualificationKey,
            localState,
            complianceSummary);
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

            evaluation.UnresolvedFactKeys);



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

            complianceSummary);

    }



    private async Task<Dictionary<Guid, QualificationLocalStateResponse>> LoadLocalQualificationStatesAsync(

        Guid tenantId,

        IReadOnlyList<Guid> personIds,

        string qualificationKey,

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

                && x.QualificationKey == qualificationKey)

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

            pair => MapLocalState(pair.Value));

    }



    private static QualificationLocalStateResponse NoLocalQualificationState() =>

        new(

            null,

            "none",

            "No TrainArr qualification issue exists for this person and qualification key.");



    private static QualificationLocalStateResponse MapLocalState(QualificationIssue issue)

    {

        var status = issue.Status.Trim().ToLowerInvariant();

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



        var outcome = MaxSeverity(localOutcome, complianceOutcome);

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



    private static int SeverityRank(string outcome) =>

        outcome switch

        {

            QualificationCheckOutcomes.Block => 2,

            QualificationCheckOutcomes.Warn => 1,

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


