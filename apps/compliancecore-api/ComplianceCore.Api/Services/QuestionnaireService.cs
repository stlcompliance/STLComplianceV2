using System.Text.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class QuestionnaireService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<QuestionnaireResolutionResponse> ResolveAsync(
        Guid tenantId,
        QuestionnaireResolveRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeRequest(tenantId, request);
        var template = QuestionnaireCatalog.Resolve(normalized.ProductKey, normalized.WorkflowKey, normalized.SubjectType);
        var run = await LoadOrCreateRunAsync(normalized, template, cancellationToken);
        var questions = QuestionnaireCatalog.ResolveQuestions(template, normalized);
        var profile = QuestionnaireCatalog.BuildProfile(template, normalized);
        var summary = QuestionnaireCatalog.BuildSummary(template, normalized, questions, Array.Empty<QuestionnaireAnswerModel>());

        run.TemplateKey = template.TemplateKey;
        run.Status = QuestionnaireRunStatuses.Draft;
        run.SummaryJson = JsonSerializer.Serialize(summary, JsonOptions);
        run.UpdatedAt = DateTimeOffset.UtcNow;
        run.ResolvedAt = DateTimeOffset.UtcNow;
        db.QuestionnaireRuns.Update(run);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "questionnaire.resolve",
            tenantId,
            null,
            "questionnaire_run",
            run.QuestionnaireRunId.ToString(),
            "success",
            reasonCode: normalized.WorkflowKey,
            cancellationToken: cancellationToken);

        return new QuestionnaireResolutionResponse(
            MapRun(run),
            questions.Select(question => MapQuestion(question, normalized)).ToList(),
            profile,
            summary);
    }

    public async Task<QuestionnaireSubmissionResponse> SubmitAsync(
        Guid tenantId,
        Guid questionnaireRunId,
        QuestionnaireSubmitRequest request,
        CancellationToken cancellationToken = default)
    {
        var run = await db.QuestionnaireRuns.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.QuestionnaireRunId == questionnaireRunId,
            cancellationToken);

        if (run is null)
        {
            throw new StlApiException("questionnaires.run_not_found", "Questionnaire run was not found.", 404);
        }

        var normalized = NormalizeRun(run, request);
        var template = QuestionnaireCatalog.Resolve(run.ProductKey, run.WorkflowKey, run.SubjectType);
        var questions = QuestionnaireCatalog.ResolveQuestions(template, normalized).ToList();
        var questionLookup = questions.ToDictionary(x => x.QuestionKey, StringComparer.OrdinalIgnoreCase);
        var knownFacts = DeserializeFacts(run.KnownFactsJson);
        var sourceContext = request.SourceRecordContext ?? DeserializeFacts(run.SourceRecordContextJson);
        var now = DateTimeOffset.UtcNow;
        var answers = new List<QuestionnaireAnswerModel>();
        var createdFacts = new List<QuestionnaireFactResponse>();

        foreach (var requestAnswer in request.Answers)
        {
            if (!questionLookup.TryGetValue(requestAnswer.QuestionKey, out var question))
            {
                continue;
            }

            var normalizedAnswer = QuestionnaireCatalog.NormalizeAnswer(question, requestAnswer, sourceContext);
            var reviewStatus = normalizedAnswer.ReviewStatus;
            var knownFactValue = knownFacts.TryGetValue(question.FactKey, out var existingKnown) ? existingKnown : null;
            if (!string.IsNullOrWhiteSpace(knownFactValue) &&
                !string.Equals(knownFactValue, normalizedAnswer.NormalizedFactValue, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(reviewStatus, QuestionnaireReviewStatuses.Unknown, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(reviewStatus, QuestionnaireReviewStatuses.Deferred, StringComparison.OrdinalIgnoreCase))
            {
                reviewStatus = QuestionnaireReviewStatuses.Conflict;
            }

            var evidenceReference = await TryCreateEvidenceReferenceAsync(
                tenantId,
                run,
                question,
                normalizedAnswer,
                sourceContext,
                requestAnswer,
                cancellationToken);

            var answer = new QuestionnaireAnswer
            {
                QuestionnaireAnswerId = Guid.NewGuid(),
                TenantId = tenantId,
                QuestionnaireRunId = run.QuestionnaireRunId,
                QuestionKey = question.QuestionKey,
                QuestionLabel = question.Prompt,
                SectionKey = question.SectionKey,
                SectionLabel = question.SectionLabel,
                AnswerKind = question.AnswerKind,
                SelectedOptionKey = normalizedAnswer.SelectedOptionKey,
                AnswerText = normalizedAnswer.AnswerText,
                DocumentUrl = normalizedAnswer.DocumentUrl,
                StorageKey = normalizedAnswer.StorageKey,
                FileName = normalizedAnswer.FileName,
                FileHash = normalizedAnswer.FileHash,
                NormalizedFactKey = question.FactKey,
                NormalizedFactValue = normalizedAnswer.NormalizedFactValue,
                NormalizedFactValueType = normalizedAnswer.NormalizedFactValueType,
                SourceProduct = run.ProductKey,
                WorkflowKey = run.WorkflowKey,
                SubjectType = run.SubjectType,
                SubjectId = run.SubjectId,
                SourceRecordId = run.SourceRecordId,
                ReviewStatus = reviewStatus,
                Confidence = normalizedAnswer.Confidence,
                EffectiveAt = requestAnswer.EffectiveAt ?? now,
                CreatedAt = now,
                UpdatedAt = now,
                EvidenceReferenceId = evidenceReference?.Id,
                EvidenceId = evidenceReference?.EvidenceId,
                SourceContextJson = JsonSerializer.Serialize(sourceContext, JsonOptions),
            };

            answers.Add(MapAnswerModel(answer));
            db.QuestionnaireAnswers.Add(answer);

            var factAssertion = new FactAssertion
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                FactKey = question.FactKey,
                SubjectKind = run.SubjectType,
                SubjectId = string.IsNullOrWhiteSpace(run.SubjectId) ? run.SourceRecordId : run.SubjectId,
                Value = normalizedAnswer.NormalizedFactValue,
                ValueType = normalizedAnswer.NormalizedFactValueType,
                SourceProduct = run.ProductKey,
                SourceRecordId = run.SourceRecordId,
                EvidenceReferenceId = evidenceReference?.Id,
                EvidenceId = evidenceReference?.EvidenceId,
                AssertedAt = now,
                EffectiveAt = requestAnswer.EffectiveAt ?? now,
                ExpiresAt = null,
                CreatedAt = now,
            };
            db.FactAssertions.Add(factAssertion);

            createdFacts.Add(
                new QuestionnaireFactResponse(
                    factAssertion.Id,
                    factAssertion.FactKey,
                    factAssertion.SubjectKind,
                    factAssertion.SubjectId,
                    factAssertion.Value,
                    factAssertion.ValueType,
                    factAssertion.SourceProduct,
                    factAssertion.SourceRecordId,
                    reviewStatus,
                    normalizedAnswer.Confidence,
                    factAssertion.AssertedAt,
                    factAssertion.EffectiveAt,
                    factAssertion.ExpiresAt));
        }

        run.Status = QuestionnaireRunStatuses.Submitted;
        run.SubmittedAt = now;
        run.UpdatedAt = now;
        run.SummaryJson = JsonSerializer.Serialize(
            QuestionnaireCatalog.BuildSummary(template, normalized, questions, answers),
            JsonOptions);

        await db.SaveChangesAsync(cancellationToken);

        var summary = QuestionnaireCatalog.BuildSummary(template, normalized, questions, answers);
        var profile = QuestionnaireCatalog.BuildProfile(template, normalized, answers);

        await auditService.WriteAsync(
            "questionnaire.submit",
            tenantId,
            null,
            "questionnaire_run",
            run.QuestionnaireRunId.ToString(),
            "success",
            reasonCode: run.WorkflowKey,
            cancellationToken: cancellationToken);

        return new QuestionnaireSubmissionResponse(
            MapRun(run),
            profile,
            summary,
            answers.Select(MapAnswerResponse).ToList(),
            createdFacts);
    }

    public async Task<QuestionnaireResolutionResponse> GetAsync(
        Guid tenantId,
        Guid questionnaireRunId,
        CancellationToken cancellationToken = default)
    {
        var run = await db.QuestionnaireRuns.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.QuestionnaireRunId == questionnaireRunId, cancellationToken);

        if (run is null)
        {
            throw new StlApiException("questionnaires.run_not_found", "Questionnaire run was not found.", 404);
        }

        var normalized = new QuestionnaireRequestContext(
            tenantId,
            run.ProductKey,
            run.WorkflowKey,
            run.SubjectType,
            run.SubjectId,
            run.SourceRecordId,
            run.SourceEntity,
            DeserializeFacts(run.KnownFactsJson),
            DeserializeFacts(run.SourceRecordContextJson),
            run.TemplateKey);

        var template = QuestionnaireCatalog.Resolve(normalized.ProductKey, normalized.WorkflowKey, normalized.SubjectType);
        var questions = QuestionnaireCatalog.ResolveQuestions(template, normalized);
        var profile = QuestionnaireCatalog.BuildProfile(template, normalized);
        var summary = QuestionnaireCatalog.BuildSummary(template, normalized, questions, Array.Empty<QuestionnaireAnswerModel>());

        return new QuestionnaireResolutionResponse(
            MapRun(run),
            questions.Select(question => MapQuestion(question, normalized)).ToList(),
            profile,
            summary);
    }

    private async Task<QuestionnaireRun> LoadOrCreateRunAsync(
        QuestionnaireRequestContext normalized,
        QuestionnaireTemplate template,
        CancellationToken cancellationToken)
    {
        var run = await db.QuestionnaireRuns.FirstOrDefaultAsync(
            x => x.TenantId == normalized.TenantId
                && x.ProductKey == normalized.ProductKey
                && x.WorkflowKey == normalized.WorkflowKey
                && x.SubjectType == normalized.SubjectType
                && x.SubjectId == normalized.SubjectId
                && x.SourceRecordId == normalized.SourceRecordId,
            cancellationToken);

        if (run is not null)
        {
            return run;
        }

        run = new QuestionnaireRun
        {
            QuestionnaireRunId = Guid.NewGuid(),
            TenantId = normalized.TenantId,
            ProductKey = normalized.ProductKey,
            WorkflowKey = normalized.WorkflowKey,
            SubjectType = normalized.SubjectType,
            SubjectId = normalized.SubjectId,
            SourceRecordId = normalized.SourceRecordId,
            SourceEntity = normalized.SourceEntity,
            SourceRecordContextJson = JsonSerializer.Serialize(normalized.SourceRecordContext, JsonOptions),
            KnownFactsJson = JsonSerializer.Serialize(normalized.KnownFacts, JsonOptions),
            TemplateKey = template.TemplateKey,
            Status = QuestionnaireRunStatuses.Draft,
            SummaryJson = "{}",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        db.QuestionnaireRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);
        return run;
    }

    private static QuestionnaireRequestContext NormalizeRequest(Guid tenantId, QuestionnaireResolveRequest request)
    {
        if (tenantId != request.TenantId)
        {
            throw new StlApiException("questionnaires.tenant_mismatch", "Questionnaire tenant did not match the authenticated tenant.", 403);
        }

        return new QuestionnaireRequestContext(
            tenantId,
            NormalizeToken(request.ProductKey),
            NormalizeToken(request.WorkflowKey),
            NormalizeToken(request.SubjectType),
            NormalizeToken(request.SubjectId),
            NormalizeToken(request.SourceRecordId),
            NormalizeToken(request.SourceEntity),
            NormalizeFacts(request.KnownFacts),
            NormalizeFacts(request.SourceRecordContext),
            string.Empty);
    }

    private static QuestionnaireRequestContext NormalizeRun(QuestionnaireRun run, QuestionnaireSubmitRequest request) =>
        new(
            run.TenantId,
            run.ProductKey,
            run.WorkflowKey,
            run.SubjectType,
            run.SubjectId,
            run.SourceRecordId,
            run.SourceEntity,
            DeserializeFacts(run.KnownFactsJson),
            request.SourceRecordContext ?? DeserializeFacts(run.SourceRecordContextJson),
            run.TemplateKey);

    private async Task<EvidenceReference?> TryCreateEvidenceReferenceAsync(
        Guid tenantId,
        QuestionnaireRun run,
        QuestionnaireQuestionDefinition question,
        QuestionnaireNormalizedAnswer normalizedAnswer,
        IReadOnlyDictionary<string, string> sourceContext,
        QuestionnaireAnswerRequest requestAnswer,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(normalizedAnswer.DocumentUrl)
            && string.IsNullOrWhiteSpace(normalizedAnswer.StorageKey)
            && string.IsNullOrWhiteSpace(normalizedAnswer.FileHash))
        {
            return null;
        }

        var existingEvidenceId = requestAnswer.EvidenceId?.Trim();
        if (!string.IsNullOrWhiteSpace(existingEvidenceId))
        {
            return await db.EvidenceReferences.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.EvidenceId == existingEvidenceId,
                cancellationToken);
        }

        var evidence = new EvidenceReference
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EvidenceId = $"q-{Guid.NewGuid():N}",
            FactKey = question.FactKey,
            SourceProduct = run.ProductKey,
            SourceEntity = run.SourceEntity,
            SourceRecordId = run.SourceRecordId,
            SourceField = question.QuestionKey,
            DocumentType = sourceContext.TryGetValue("document_type", out var documentType) ? documentType : question.QuestionKey,
            DocumentUrl = normalizedAnswer.DocumentUrl,
            StorageKey = normalizedAnswer.StorageKey,
            FileHash = normalizedAnswer.FileHash,
            CapturedAt = requestAnswer.EffectiveAt ?? DateTimeOffset.UtcNow,
            EffectiveAt = requestAnswer.EffectiveAt,
            ExpiresAt = null,
            CreatedByPersonId = null,
            ReviewedByPersonId = null,
            ReviewStatus = normalizedAnswer.ReviewStatus,
            Notes = normalizedAnswer.AnswerText,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.EvidenceReferences.Add(evidence);
        return evidence;
    }

    private static QuestionnaireRunResponse MapRun(QuestionnaireRun run) =>
        new(
            run.QuestionnaireRunId,
            run.ProductKey,
            run.WorkflowKey,
            run.SubjectType,
            run.SubjectId,
            run.SourceRecordId,
            run.SourceEntity,
            run.Status,
            run.TemplateKey,
            run.CreatedAt,
            run.UpdatedAt,
            run.ResolvedAt,
            run.SubmittedAt);

    private static QuestionnaireQuestionResponse MapQuestion(
        QuestionnaireQuestionDefinition question,
        QuestionnaireRequestContext context)
    {
        var defaultOption = question.Options.FirstOrDefault(option => IsDefaultOption(question, option, context));
        return new QuestionnaireQuestionResponse(
            question.QuestionKey,
            question.SectionKey,
            question.SectionLabel,
            question.Prompt,
            question.HelpText,
            question.WhyItMatters,
            question.AnswerKind,
            question.FactKey,
            question.FactValueType,
            question.Required,
            question.Priority,
            defaultOption?.Key,
            question.Options.Select(option => new QuestionnaireAnswerOptionResponse(
                    option.Key,
                    option.Label,
                    option.Description,
                    option.AnswerKind,
                    option.IsDefault))
                .ToList(),
            question.ApplicableAreas,
            question.RecommendedNextActions);
    }

    private static QuestionnaireAnswerResponse MapAnswerResponse(QuestionnaireAnswerModel answer) =>
        new(
            answer.QuestionnaireAnswerId,
            answer.QuestionKey,
            answer.SelectedOptionKey,
            answer.AnswerText,
            answer.DocumentUrl,
            answer.StorageKey,
            answer.FileName,
            answer.FileHash,
            answer.NormalizedFactKey,
            answer.NormalizedFactValue,
            answer.NormalizedFactValueType,
            answer.ReviewStatus,
            answer.Confidence,
            answer.EffectiveAt,
            answer.EvidenceReferenceId,
            answer.EvidenceId);

    private static QuestionnaireAnswerModel MapAnswerModel(QuestionnaireAnswer answer) =>
        new(
            answer.QuestionnaireAnswerId,
            answer.QuestionKey,
            answer.SelectedOptionKey,
            answer.AnswerText,
            answer.DocumentUrl,
            answer.StorageKey,
            answer.FileName,
            answer.FileHash,
            answer.NormalizedFactKey,
            answer.NormalizedFactValue,
            answer.NormalizedFactValueType,
            answer.ReviewStatus,
            answer.Confidence,
            answer.EffectiveAt,
            answer.EvidenceReferenceId,
            answer.EvidenceId);

    private static Dictionary<string, string> NormalizeFacts(IReadOnlyDictionary<string, string>? values)
    {
        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (values is null)
        {
            return normalized;
        }

        foreach (var (key, value) in values)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            normalized[NormalizeToken(key)] = NormalizeToken(value);
        }

        return normalized;
    }

    private static Dictionary<string, string> DeserializeFacts(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var values = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions);
        return values is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeToken(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();

    private static bool IsDefaultOption(QuestionnaireQuestionDefinition question, QuestionnaireAnswerOptionDefinition option, QuestionnaireRequestContext context)
    {
        if (!string.IsNullOrWhiteSpace(question.DefaultOptionKey) &&
            string.Equals(question.DefaultOptionKey, option.Key, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(question.FactKey) &&
            context.KnownFacts.TryGetValue(question.FactKey, out var knownValue) &&
            string.Equals(knownValue, option.NormalizedValue, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetProfileValue(IReadOnlyDictionary<string, string> facts, string key, string fallback) =>
        facts.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;

    private sealed record QuestionnaireRequestContext(
        Guid TenantId,
        string ProductKey,
        string WorkflowKey,
        string SubjectType,
        string SubjectId,
        string SourceRecordId,
        string SourceEntity,
        IReadOnlyDictionary<string, string> KnownFacts,
        IReadOnlyDictionary<string, string> SourceRecordContext,
        string TemplateKey);

    private sealed record QuestionnaireAnswerModel(
        Guid QuestionnaireAnswerId,
        string QuestionKey,
        string SelectedOptionKey,
        string AnswerText,
        string DocumentUrl,
        string StorageKey,
        string FileName,
        string FileHash,
        string NormalizedFactKey,
        string NormalizedFactValue,
        string NormalizedFactValueType,
        string ReviewStatus,
        decimal Confidence,
        DateTimeOffset EffectiveAt,
        Guid? EvidenceReferenceId,
        string? EvidenceId);

    private sealed record QuestionnaireNormalizedAnswer(
        string SelectedOptionKey,
        string AnswerText,
        string DocumentUrl,
        string StorageKey,
        string FileName,
        string FileHash,
        string NormalizedFactValue,
        string NormalizedFactValueType,
        string ReviewStatus,
        decimal Confidence);

    private sealed record QuestionnaireAnswerOptionDefinition(
        string Key,
        string Label,
        string Description,
        string NormalizedValue,
        string NormalizedValueType,
        string AnswerKind,
        string ReviewStatus,
        decimal Confidence,
        bool IsDefault = false);

    private sealed record QuestionnaireQuestionDefinition(
        string QuestionKey,
        string SectionKey,
        string SectionLabel,
        string Prompt,
        string? HelpText,
        string? WhyItMatters,
        string AnswerKind,
        string FactKey,
        string FactValueType,
        bool Required,
        int Priority,
        string? DefaultOptionKey,
        IReadOnlyList<QuestionnaireAnswerOptionDefinition> Options,
        IReadOnlyList<string> ApplicableAreas,
        IReadOnlyList<string> RecommendedNextActions,
        bool AlwaysAsk = false,
        Func<QuestionnaireRequestContext, bool>? AppliesWhen = null);

    private sealed record QuestionnaireTemplate(
        string TemplateKey,
        string Title,
        string Description,
        string ProductKey,
        string WorkflowKey,
        string SubjectType,
        IReadOnlyList<QuestionnaireQuestionDefinition> Questions,
        IReadOnlyList<string> LikelyAreas,
        IReadOnlyList<string> RecommendedNextActions,
        IReadOnlyList<QuestionnaireExceptionResponse> GeneratedExceptions,
        bool RiskSensitive = false);

    private sealed record QuestionnaireProfileBuilder(
        string BusinessProfile,
        IReadOnlyList<string> TransportationExposure,
        IReadOnlyList<string> WorkforceExposure,
        IReadOnlyList<string> LocationExposure,
        IReadOnlyList<string> MaterialHazmatExposure,
        string RecordDocumentMaturity,
        IReadOnlyList<string> LikelyRulePacks,
        IReadOnlyList<string> InitialAssumptions,
        IReadOnlyList<string> SetupChecklist);

    private static class QuestionnaireCatalog
    {
        private static readonly QuestionnaireAnswerOptionDefinition Yes = new("yes", "Yes", "Confirm this applies.", "true", FactValueTypes.Boolean, "choice", QuestionnaireReviewStatuses.Confirmed, 1m, true);
        private static readonly QuestionnaireAnswerOptionDefinition No = new("no", "No", "Confirm this does not apply.", "false", FactValueTypes.Boolean, "choice", QuestionnaireReviewStatuses.Confirmed, 1m);
        private static readonly QuestionnaireAnswerOptionDefinition Sometimes = new("sometimes", "Sometimes", "Applies part of the time.", "sometimes", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 0.8m);
        private static readonly QuestionnaireAnswerOptionDefinition NotSure = new("not_sure", "Not sure", "Keep this open for review.", "unknown", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Unknown, 0.2m);
        private static readonly QuestionnaireAnswerOptionDefinition Skip = new("skip_for_now", "Skip for now", "Leave this open and continue.", "deferred", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Deferred, 0m);
        private static readonly QuestionnaireAnswerOptionDefinition Document = new("document_upload", "Document upload", "Attach a supporting file.", "document_upload", FactValueTypes.String, "document", QuestionnaireReviewStatuses.EvidencePending, 1m);

        public static QuestionnaireTemplate Resolve(string productKey, string workflowKey, string subjectType)
        {
            var key = $"{productKey}:{workflowKey}:{subjectType}";
            return key switch
            {
                "compliancecore:tenant_onboarding:tenant" => BuildOnboardingTemplate(productKey, workflowKey, subjectType),
                "maintainarr:asset_create:asset" => BuildAssetTemplate(productKey, workflowKey, subjectType),
                "staffarr:person_create:person" => BuildPersonTemplate(productKey, workflowKey, subjectType),
                "staffarr:location_create:location" => BuildLocationTemplate(productKey, workflowKey, subjectType),
                "supplyarr:vendor_create:vendor" => BuildVendorTemplate(productKey, workflowKey, subjectType),
                "supplyarr:material_create:material" => BuildMaterialTemplate(productKey, workflowKey, subjectType),
                "supplyarr:route_order_create:trip" => BuildRouteTemplate(productKey, workflowKey, subjectType),
                "routarr:route_order_create:trip" => BuildRouteTemplate(productKey, workflowKey, subjectType),
                "routarr:order_create:trip" => BuildRouteTemplate(productKey, workflowKey, subjectType),
                "routarr:trip_create:trip" => BuildRouteTemplate(productKey, workflowKey, subjectType),
                "compliancecore:document_upload:document" => BuildDocumentTemplate(productKey, workflowKey, subjectType),
                _ => BuildFallbackTemplate(productKey, workflowKey, subjectType),
            };
        }

        public static IReadOnlyList<QuestionnaireQuestionDefinition> ResolveQuestions(
            QuestionnaireTemplate template,
            QuestionnaireRequestContext context)
        {
            return template.Questions
                .Where(question => question.AppliesWhen?.Invoke(context) ?? true)
                .Where(question => question.AlwaysAsk || !context.KnownFacts.ContainsKey(question.FactKey))
                .OrderBy(question => question.Priority)
                .ToList();
        }

        public static QuestionnaireTenantProfileResponse BuildProfile(
            QuestionnaireTemplate template,
            QuestionnaireRequestContext context,
            IReadOnlyList<QuestionnaireAnswerModel>? answers = null)
        {
            var facts = MergeFacts(context.KnownFacts, answers);
            var businessProfile = GetProfileValue(facts, "tenant.profile.business_profile", "Not captured yet");
            var transportation = SplitProfileList(facts, "tenant.profile.transportation_exposure");
            var workforce = SplitProfileList(facts, "tenant.profile.workforce_exposure");
            var locations = SplitProfileList(facts, "tenant.profile.location_exposure");
            var materials = SplitProfileList(facts, "tenant.profile.material_hazmat_exposure");
            var documentMaturity = GetProfileValue(facts, "tenant.profile.record_document_maturity", "not captured");
            var likelyRulePacks = SplitProfileList(facts, "tenant.profile.likely_rule_packs");
            if (likelyRulePacks.Count == 0)
            {
                likelyRulePacks = template.LikelyAreas.Select(area => area.ToLowerInvariant()).ToList();
            }

            var assumptions = SplitProfileList(facts, "tenant.profile.initial_assumptions");
            if (assumptions.Count == 0)
            {
                assumptions = template.RecommendedNextActions.ToList();
            }

            var checklist = SplitProfileList(facts, "tenant.profile.setup_checklist");
            if (checklist.Count == 0)
            {
                checklist = template.RecommendedNextActions.ToList();
            }

            return new QuestionnaireTenantProfileResponse(
                businessProfile,
                transportation,
                workforce,
                locations,
                materials,
                documentMaturity,
                likelyRulePacks,
                assumptions,
                checklist);
        }

        public static QuestionnaireResultSummaryResponse BuildSummary(
            QuestionnaireTemplate template,
            QuestionnaireRequestContext context,
            IReadOnlyList<QuestionnaireQuestionDefinition> questions,
            IReadOnlyList<QuestionnaireAnswerModel> answers)
        {
            var answerMap = answers.ToDictionary(x => x.NormalizedFactKey, x => x, StringComparer.OrdinalIgnoreCase);
            var missingFacts = questions
                .Where(question => !answerMap.ContainsKey(question.FactKey))
                .Select(question => question.FactKey)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var openAnswerFacts = answers
                .Where(answer =>
                    string.Equals(answer.ReviewStatus, QuestionnaireReviewStatuses.Unknown, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(answer.ReviewStatus, QuestionnaireReviewStatuses.Deferred, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(answer.ReviewStatus, QuestionnaireReviewStatuses.Conflict, StringComparison.OrdinalIgnoreCase))
                .Select(answer => answer.NormalizedFactKey)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var fact in openAnswerFacts)
            {
                if (!missingFacts.Any(existing => string.Equals(existing, fact, StringComparison.OrdinalIgnoreCase)))
                {
                    missingFacts.Add(fact);
                }
            }

            var likelyAreas = template.LikelyAreas
                .Concat(questions.SelectMany(question => question.ApplicableAreas))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var recommendedActions = template.RecommendedNextActions
                .Concat(questions.SelectMany(question => question.RecommendedNextActions))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var generatedExceptions = new List<QuestionnaireExceptionResponse>(template.GeneratedExceptions);
            if (context.WorkflowKey.Contains("hazmat", StringComparison.OrdinalIgnoreCase) && missingFacts.Count > 0)
            {
                generatedExceptions.Add(new QuestionnaireExceptionResponse(
                    "hazmat_missing_information",
                    "Hazmat review needed",
                    "Hazmat-related workflow questions still need review before the record is considered ready.",
                    "high"));
            }

            if (answers.Any(answer => string.Equals(answer.ReviewStatus, QuestionnaireReviewStatuses.Conflict, StringComparison.OrdinalIgnoreCase)))
            {
                generatedExceptions.Add(new QuestionnaireExceptionResponse(
                    "conflicting_facts",
                    "Conflicting facts",
                    "One or more questionnaire answers conflict with existing compliance facts and need review.",
                    "high"));
            }

            var followUps = missingFacts
                .Take(5)
                .Select((fact, index) => new QuestionnaireFollowUpResponse(
                    $"follow_up_{index + 1}",
                    FollowUpPromptForFact(fact),
                    FollowUpReasonForFact(fact, answers),
                    fact,
                    index == 0 ? "high" : "normal"))
                .ToList();

            var requiresMoreFacts = missingFacts.Count > 0;
            var riskGateStatus = template.RiskSensitive && requiresMoreFacts ? "blocked" : requiresMoreFacts ? "warning" : "ready";
            var summary = template.Title + (requiresMoreFacts ? " needs a few more facts." : " is ready to proceed.");

            return new QuestionnaireResultSummaryResponse(
                summary,
                likelyAreas,
                missingFacts,
                recommendedActions,
                generatedExceptions,
                followUps,
                requiresMoreFacts,
                riskGateStatus);
        }

        public static QuestionnaireNormalizedAnswer NormalizeAnswer(
            QuestionnaireQuestionDefinition question,
            QuestionnaireAnswerRequest answer,
            IReadOnlyDictionary<string, string> sourceContext)
        {
            if (string.Equals(question.AnswerKind, "document", StringComparison.OrdinalIgnoreCase))
            {
                return new QuestionnaireNormalizedAnswer(
                    "document_upload",
                    string.Empty,
                    NormalizeToken(answer.DocumentUrl),
                    NormalizeToken(answer.StorageKey),
                    NormalizeToken(answer.FileName),
                    NormalizeToken(answer.FileHash),
                    NormalizeToken(answer.DocumentUrl ?? answer.StorageKey ?? answer.FileHash),
                    FactValueTypes.String,
                    QuestionnaireReviewStatuses.EvidencePending,
                    1m);
            }

            var selectedOption = question.Options.FirstOrDefault(option =>
                string.Equals(option.Key, NormalizeToken(answer.SelectedOptionKey), StringComparison.OrdinalIgnoreCase));

            if (selectedOption is not null)
            {
                var reviewStatus = selectedOption.ReviewStatus;
                var normalizedValue = selectedOption.NormalizedValue;
                var normalizedValueType = selectedOption.NormalizedValueType;
                if (string.Equals(selectedOption.Key, "skip_for_now", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedValue = "deferred";
                    normalizedValueType = FactValueTypes.String;
                }

                return new QuestionnaireNormalizedAnswer(
                    selectedOption.Key,
                    NormalizeToken(answer.AnswerText),
                    NormalizeToken(answer.DocumentUrl),
                    NormalizeToken(answer.StorageKey),
                    NormalizeToken(answer.FileName),
                    NormalizeToken(answer.FileHash),
                    normalizedValue,
                    normalizedValueType,
                    reviewStatus,
                    selectedOption.Confidence);
            }

            var rawText = NormalizeToken(answer.AnswerText);
            if (string.Equals(rawText, "not sure", StringComparison.OrdinalIgnoreCase))
            {
                return new QuestionnaireNormalizedAnswer(
                    "not_sure",
                    rawText,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    "unknown",
                    FactValueTypes.String,
                    QuestionnaireReviewStatuses.Unknown,
                    0.2m);
            }

            if (string.IsNullOrWhiteSpace(rawText))
            {
                return new QuestionnaireNormalizedAnswer(
                    "skip_for_now",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    "deferred",
                    FactValueTypes.String,
                    QuestionnaireReviewStatuses.Deferred,
                    0m);
            }

            return new QuestionnaireNormalizedAnswer(
                "freeform",
                rawText,
                NormalizeToken(answer.DocumentUrl),
                NormalizeToken(answer.StorageKey),
                NormalizeToken(answer.FileName),
                NormalizeToken(answer.FileHash),
                rawText,
                question.FactValueType,
                QuestionnaireReviewStatuses.Confirmed,
                0.8m);
        }

        private static QuestionnaireTemplate BuildOnboardingTemplate(string productKey, string workflowKey, string subjectType)
        {
            var questions = new List<QuestionnaireQuestionDefinition>
            {
                ChoiceQuestion(
                    "business_profile",
                    "profile",
                    "Business profile",
                    "What best describes the business?",
                    "Choose the closest plain-language description for the tenant.",
                    "This anchors the starting compliance profile and rulepack suggestions.",
                    "tenant.profile.business_profile",
                    "string",
                    10,
                    true,
                    "not_sure",
                    areas: new[] { "Operations", "Coverage" },
                    nextActions: new[] { "Review the tenant profile after answers are submitted." },
                    options: new[]
                    {
                        new QuestionnaireAnswerOptionDefinition("fleet_operator", "Fleet operator", "Moves company vehicles or equipment.", "fleet_operator", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m, true),
                        new QuestionnaireAnswerOptionDefinition("carrier", "Carrier", "Ships or transports goods for others.", "carrier", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                        new QuestionnaireAnswerOptionDefinition("shop_or_service", "Shop or service business", "Mainly repairs, services, or supports equipment.", "shop_or_service", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                        new QuestionnaireAnswerOptionDefinition("warehouse_or_yard", "Warehouse or yard business", "Stores, stages, or moves materials.", "warehouse_or_yard", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                        NotSure,
                    Skip,
                    }),
                ChoiceQuestion(
                    "transportation_exposure",
                    "profile",
                    "Transportation exposure",
                    "Does the business move people or property in vehicles or through vendors?",
                    "This helps decide which transportation-related rulepacks may matter.",
                    "Transportation exposure drives route, driver, and hazmat follow-up questions.",
                    "tenant.profile.transportation_exposure",
                    "string",
                    20,
                    true,
                    "not_sure",
                    areas: new[] { "Transportation", "Driver compliance" },
                    nextActions: new[] { "Confirm whether transportation is company-operated, vendor-operated, or brokered." },
                    options: new[]
                    {
                        Yes,
                        No,
                        Sometimes,
                        NotSure,
                        Skip,
                    }),
                ChoiceQuestion(
                    "workforce_exposure",
                    "profile",
                    "Workforce exposure",
                    "Does the business have drivers, maintenance staff, or other safety-sensitive workers?",
                    "This helps decide whether person-level questions need to stay open.",
                    "Workforce exposure drives employee and supervisor setup defaults.",
                    "tenant.profile.workforce_exposure",
                    "string",
                    30,
                    true,
                    "not_sure",
                    areas: new[] { "Workforce", "Safety-sensitive work" },
                    nextActions: new[] { "Review employee, supervisor, and maintenance exposure." },
                    options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion(
                    "location_exposure",
                    "profile",
                    "Location exposure",
                    "Do the business locations include yards, shops, docks, warehouses, or customer sites?",
                    "This helps choose location questions and storage-related follow-ups.",
                    "Location exposure affects site-level checks, storage, and access control.",
                    "tenant.profile.location_exposure",
                    "string",
                    40,
                    true,
                    "not_sure",
                    areas: new[] { "Locations", "Storage" },
                    nextActions: new[] { "Confirm the main operating sites." },
                    options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion(
                    "material_hazmat_exposure",
                    "profile",
                    "Material and hazmat exposure",
                    "Does the business handle chemicals, fuel, batteries, or other regulated materials?",
                    "This steers hazmat, SDS, and special storage follow-ups.",
                    "Material exposure determines whether SDS, storage, and transport questions appear.",
                    "tenant.profile.material_hazmat_exposure",
                    "string",
                    50,
                    true,
                    "not_sure",
                    areas: new[] { "Hazmat", "Materials" },
                    nextActions: new[] { "Confirm whether SDS and special storage are needed." },
                    options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion(
                    "record_document_maturity",
                    "profile",
                    "Record and document maturity",
                    "How mature are the business records and document files today?",
                    "This helps Compliance Core decide how much follow-up evidence is needed.",
                    "Record maturity shapes the setup checklist and evidence expectations.",
                    "tenant.profile.record_document_maturity",
                    "string",
                    60,
                    true,
                    "not_sure",
                    areas: new[] { "Evidence", "Documentation" },
                    nextActions: new[] { "Review document capture and retention setup." },
                    options: new[]
                    {
                        new QuestionnaireAnswerOptionDefinition("light", "Light", "Paper and ad hoc files.", "light", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                        new QuestionnaireAnswerOptionDefinition("basic", "Basic", "Some organized files, but not consistently.", "basic", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m, true),
                        new QuestionnaireAnswerOptionDefinition("mature", "Mature", "Mostly consistent records and retention.", "mature", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                        NotSure,
                        Skip,
                    }),
            };

            return new QuestionnaireTemplate(
                "tenant_onboarding",
                "Tenant onboarding questionnaire",
                "Build the first tenant compliance profile using plain operational facts.",
                productKey,
                workflowKey,
                subjectType,
                questions,
                new[] { "Transportation", "Hazmat", "Workforce", "Locations", "Evidence" },
                new[]
                {
                    "Review the suggested rulepacks after onboarding.",
                    "Upload supporting documents where the answers were not sure.",
                },
                new[]
                {
                    new QuestionnaireExceptionResponse("onboarding_unknowns", "Onboarding unknowns", "Some tenant profile facts were left open for review.", "medium"),
                },
                false);
        }

        private static QuestionnaireTemplate BuildAssetTemplate(string productKey, string workflowKey, string subjectType)
        {
            var questions = new List<QuestionnaireQuestionDefinition>
            {
                ChoiceQuestion("asset_kind", "asset", "Asset type", "What kind of asset is this?", null, "This narrows the compliance defaults for the asset record.", "asset.kind", "string", 10, true, "not_sure", areas: new[] { "Asset profile" }, nextActions: new[] { "Confirm the primary asset type." }, options: new[]
                {
                    new QuestionnaireAnswerOptionDefinition("truck", "Truck", "A road-going truck or tractor.", "truck", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m, true),
                    new QuestionnaireAnswerOptionDefinition("trailer", "Trailer", "A trailer or trailer-like asset.", "trailer", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("forklift", "Forklift", "A powered industrial truck or forklift.", "forklift", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("passenger_vehicle", "Passenger vehicle", "A sedan, van, or SUV used for transport.", "passenger_vehicle", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("other", "Other", "Some other asset type.", "other", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    NotSure,
                    Skip,
                }, alwaysAsk: true),
                ChoiceQuestion("asset_use", "asset", "Asset use", "How is the asset used most of the time?", null, "Usage determines which facts matter next.", "asset.use", "string", 20, true, "not_sure", areas: new[] { "Operations" }, nextActions: new[] { "Confirm operational use and duty cycle." }, options: new[]
                {
                    new QuestionnaireAnswerOptionDefinition("road_use", "Road use", "Regularly goes on public roads.", "road_use", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m, true),
                    new QuestionnaireAnswerOptionDefinition("yard_use", "Yard or site use", "Stays mostly on company property.", "yard_use", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("mixed_use", "Mixed use", "Moves both on-site and on-road.", "mixed_use", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    NotSure,
                    Skip,
                }),
                ChoiceQuestion("asset_leaves_property", "asset", "Road exposure", "Does the asset ever leave company property?", null, "This is a major driver for transportation compliance needs.", "asset.leaves_company_property", FactValueTypes.Boolean, 30, true, "not_sure", areas: new[] { "Transportation" }, nextActions: new[] { "Confirm whether road exposure exists." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("asset_tows_trailer", "asset", "Towing", "Does the asset tow trailers?", null, "Trailer towing changes route and equipment expectations.", "asset.tows_trailers", FactValueTypes.Boolean, 40, true, "not_sure", areas: new[] { "Transportation", "Equipment" }, nextActions: new[] { "Confirm trailer towing if applicable." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("asset_passengers", "asset", "Passengers", "Does the asset carry passengers?", null, "Passenger transport often changes driver and safety follow-ups.", "asset.carries_passengers", FactValueTypes.Boolean, 50, true, "not_sure", areas: new[] { "Passenger transport" }, nextActions: new[] { "Confirm passenger capacity and intended use." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("asset_hazmat", "asset", "Hazmat exposure", "Can the asset handle placarded hazmat?", null, "This determines whether hazmat-specific review is needed.", "asset.handles_placarded_hazmat", FactValueTypes.Boolean, 60, true, "not_sure", areas: new[] { "Hazmat" }, nextActions: new[] { "Confirm hazmat handling and placarding." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("asset_base_location", "asset", "Base location", "Where is the asset mainly based?", null, "Base location affects site-level compliance facts.", "asset.base_location", "string", 70, false, "not_sure", areas: new[] { "Locations" }, nextActions: new[] { "Confirm the home base or yard location." }, options: new[]
                {
                    new QuestionnaireAnswerOptionDefinition("office", "Office", "Based at an office.", "office", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("yard", "Yard", "Based at a yard or lot.", "yard", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m, true),
                    new QuestionnaireAnswerOptionDefinition("shop", "Shop", "Based at a shop or service bay.", "shop", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("offsite", "Offsite", "Based somewhere else.", "offsite", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    NotSure,
                    Skip,
                }),
            };

            return new QuestionnaireTemplate(
                "asset_create",
                "Asset create questionnaire",
                "Ask the minimum operational questions needed to classify the asset.",
                productKey,
                workflowKey,
                subjectType,
                questions,
                new[] { "Transportation", "Passenger transport", "Hazmat", "Sites" },
                new[]
                {
                    "Review the asset and route questions together before dispatching.",
                },
                Array.Empty<QuestionnaireExceptionResponse>(),
                true);
        }

        private static QuestionnaireTemplate BuildPersonTemplate(string productKey, string workflowKey, string subjectType)
        {
            var questions = new List<QuestionnaireQuestionDefinition>
            {
                ChoiceQuestion("person_work", "person", "Work performed", "What kind of work does this person do?", null, "This determines which follow-up questions matter.", "person.work", "string", 10, true, "not_sure", areas: new[] { "Workforce" }, nextActions: new[] { "Confirm the person's main tasks." }, options: new[]
                {
                    new QuestionnaireAnswerOptionDefinition("drives", "Drives", "Drives vehicles or equipment on public roads.", "drives", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m, true),
                    new QuestionnaireAnswerOptionDefinition("equipment", "Operates equipment", "Operates powered equipment or machinery.", "equipment", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("maintenance", "Maintenance", "Performs repair or maintenance work.", "maintenance", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("supervision", "Supervision", "Supervises safety-sensitive work.", "supervision", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("office", "Office or admin", "Primarily office or admin work.", "office", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    NotSure,
                    Skip,
                }, alwaysAsk: true),
                ChoiceQuestion("person_drives", "person", "Driving", "Does this person drive for work?", null, "Driving drives the downstream compliance profile.", "person.drives", FactValueTypes.Boolean, 20, true, "not_sure", areas: new[] { "Driver compliance" }, nextActions: new[] { "Confirm driver status and vehicle exposure." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("person_equipment", "person", "Equipment use", "Does this person operate equipment or machinery?", null, "Equipment work can change training and safety follow-ups.", "person.operates_equipment", FactValueTypes.Boolean, 30, true, "not_sure", areas: new[] { "Equipment", "Safety" }, nextActions: new[] { "Confirm equipment exposure if any." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("person_supervision", "person", "Supervision", "Does this person supervise safety-sensitive work?", null, "Supervisors may need different follow-up questions.", "person.safety_sensitive_supervision", FactValueTypes.Boolean, 40, true, "not_sure", areas: new[] { "Management" }, nextActions: new[] { "Confirm supervisory responsibilities." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("person_maintenance", "person", "Maintenance", "Does this person do repair or maintenance work?", null, "Maintenance work can affect equipment and shop safety defaults.", "person.maintenance_work", FactValueTypes.Boolean, 50, true, "not_sure", areas: new[] { "Maintenance" }, nextActions: new[] { "Confirm maintenance tasks." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("person_hazmat", "person", "Hazmat handling", "Does this person handle hazardous materials?", null, "Hazmat handling changes material and training follow-ups.", "person.handles_hazmat", FactValueTypes.Boolean, 60, true, "not_sure", areas: new[] { "Hazmat" }, nextActions: new[] { "Confirm hazmat handling or material contact." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
            };

            return new QuestionnaireTemplate(
                "person_create",
                "Person create questionnaire",
                "Capture workforce facts without asking the user to classify rules.",
                productKey,
                workflowKey,
                subjectType,
                questions,
                new[] { "Workforce", "Driver compliance", "Maintenance", "Hazmat" },
                new[] { "Review driver and hazmat exposure before granting permissions." },
                Array.Empty<QuestionnaireExceptionResponse>(),
                true);
        }

        private static QuestionnaireTemplate BuildLocationTemplate(string productKey, string workflowKey, string subjectType)
        {
            var questions = new List<QuestionnaireQuestionDefinition>
            {
                ChoiceQuestion("location_kind", "location", "Location type", "What type of location is this?", null, "Location type drives the follow-up facts.", "location.kind", "string", 10, true, "not_sure", areas: new[] { "Locations" }, nextActions: new[] { "Confirm the primary site type." }, options: new[]
                {
                    new QuestionnaireAnswerOptionDefinition("office", "Office", "Administrative office space.", "office", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m, true),
                    new QuestionnaireAnswerOptionDefinition("yard", "Yard", "Outdoor yard or lot.", "yard", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("shop", "Shop", "Maintenance or repair shop.", "shop", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("warehouse", "Warehouse", "Storage or warehouse space.", "warehouse", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("dock", "Dock", "Loading or receiving dock.", "dock", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("customer_site", "Customer or vendor site", "A site owned by a customer or vendor.", "customer_site", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    NotSure,
                    Skip,
                }, alwaysAsk: true),
                ChoiceQuestion("location_activities", "location", "Location activities", "What happens at this location most often?", null, "This determines the operational follow-up questions.", "location.activities", "string", 20, true, "not_sure", areas: new[] { "Operations" }, nextActions: new[] { "Confirm the main activities on site." }, options: new[]
                {
                    new QuestionnaireAnswerOptionDefinition("administrative", "Administrative work", "Mostly office work.", "administrative", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m, true),
                    new QuestionnaireAnswerOptionDefinition("storage", "Storage", "Stores materials or equipment.", "storage", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("maintenance", "Maintenance", "Repair or maintenance work happens here.", "maintenance", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("loading", "Loading or unloading", "Vehicles or equipment are loaded or unloaded here.", "loading", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("mixed", "Mixed use", "A mix of activities happens here.", "mixed", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    NotSure,
                    Skip,
                }),
                ChoiceQuestion("location_hazmat_storage", "location", "Special storage", "Is anything special stored here, like fuel, chemicals, or batteries?", null, "Storage facts drive hazmat and fire-related follow-ups.", "location.special_storage", FactValueTypes.Boolean, 30, true, "not_sure", areas: new[] { "Hazmat", "Storage" }, nextActions: new[] { "Confirm any special storage requirements." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
            };

            return new QuestionnaireTemplate(
                "location_create",
                "Location create questionnaire",
                "Capture the key site facts that affect downstream compliance checks.",
                productKey,
                workflowKey,
                subjectType,
                questions,
                new[] { "Locations", "Storage", "Operations" },
                new[] { "Review site access, storage, and loading facts next." },
                Array.Empty<QuestionnaireExceptionResponse>(),
                false);
        }

        private static QuestionnaireTemplate BuildVendorTemplate(string productKey, string workflowKey, string subjectType)
        {
            var questions = new List<QuestionnaireQuestionDefinition>
            {
                ChoiceQuestion("vendor_transport", "vendor", "Transportation", "Does this vendor transport goods for you?", null, "Vendor transport changes the carrier and trip facts.", "vendor.transports_goods", FactValueTypes.Boolean, 10, true, "not_sure", areas: new[] { "Transportation" }, nextActions: new[] { "Confirm whether the vendor is a carrier or broker." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }, alwaysAsk: true),
                ChoiceQuestion("vendor_service", "vendor", "Service work", "Does this vendor repair or inspect assets?", null, "Service vendors can create equipment and evidence follow-ups.", "vendor.repairs_or_inspects", FactValueTypes.Boolean, 20, true, "not_sure", areas: new[] { "Maintenance" }, nextActions: new[] { "Confirm vendor service work and inspection scope." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("vendor_materials", "vendor", "Materials", "Does this vendor supply chemicals or other materials?", null, "Material supply drives SDS and storage follow-ups.", "vendor.supplies_chemicals", FactValueTypes.Boolean, 30, true, "not_sure", areas: new[] { "Materials", "Hazmat" }, nextActions: new[] { "Confirm whether SDS tracking is needed." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("vendor_tracking", "vendor", "Certificates", "Do you need insurance or certificates tracked for this vendor?", null, "This indicates vendor oversight and document tracking.", "vendor.tracked_certs", FactValueTypes.Boolean, 40, true, "not_sure", areas: new[] { "Evidence", "Vendor management" }, nextActions: new[] { "Confirm certificate and insurance tracking needs." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
            };

            return new QuestionnaireTemplate(
                "vendor_create",
                "Vendor create questionnaire",
                "Ask about vendor operational exposure without jumping to conclusions.",
                productKey,
                workflowKey,
                subjectType,
                questions,
                new[] { "Vendor management", "Transportation", "Materials", "Evidence" },
                new[] { "Review carrier, service, and certificate tracking needs." },
                Array.Empty<QuestionnaireExceptionResponse>(),
                false);
        }

        private static QuestionnaireTemplate BuildMaterialTemplate(string productKey, string workflowKey, string subjectType)
        {
            var questions = new List<QuestionnaireQuestionDefinition>
            {
                ChoiceQuestion("material_kind", "material", "Material type", "What kind of material is this?", null, "Material type helps choose the right downstream facts.", "material.kind", "string", 10, true, "not_sure", areas: new[] { "Materials" }, nextActions: new[] { "Confirm the material family." }, options: new[]
                {
                    new QuestionnaireAnswerOptionDefinition("chemical", "Chemical", "A chemical or mixture.", "chemical", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m, true),
                    new QuestionnaireAnswerOptionDefinition("fuel", "Fuel", "Fuel or combustible liquid.", "fuel", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("battery", "Battery", "Battery, cell, or energy storage unit.", "battery", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("regulated", "Regulated material", "A regulated or controlled material.", "regulated", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("other", "Other", "Some other material type.", "other", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    NotSure,
                    Skip,
                }, alwaysAsk: true),
                ChoiceQuestion("material_sds", "material", "SDS", "Is there an SDS or similar product sheet for it?", null, "SDS-backed materials are easier to review and trace.", "material.sds_backed", FactValueTypes.Boolean, 20, true, "not_sure", areas: new[] { "Evidence", "SDS" }, nextActions: new[] { "Upload the SDS if one exists." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("material_storage", "material", "Storage", "Does it need special storage?", null, "Special storage often means extra handling or location facts.", "material.special_storage", FactValueTypes.Boolean, 30, true, "not_sure", areas: new[] { "Storage", "Hazmat" }, nextActions: new[] { "Confirm any special storage conditions." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("material_transport", "material", "Transport", "Could it be placarded during transport?", null, "Transport placarding changes the hazmat follow-up path.", "material.placarded_during_transport", FactValueTypes.Boolean, 40, true, "not_sure", areas: new[] { "Hazmat", "Transportation" }, nextActions: new[] { "Confirm transport placarding needs." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
            };

            return new QuestionnaireTemplate(
                "material_create",
                "Material create questionnaire",
                "Capture plain-language material facts for SDS and hazmat follow-up.",
                productKey,
                workflowKey,
                subjectType,
                questions,
                new[] { "Materials", "SDS", "Hazmat", "Storage" },
                new[] { "Review SDS upload and storage handling next." },
                Array.Empty<QuestionnaireExceptionResponse>(),
                false);
        }

        private static QuestionnaireTemplate BuildRouteTemplate(string productKey, string workflowKey, string subjectType)
        {
            var questions = new List<QuestionnaireQuestionDefinition>
            {
                ChoiceQuestion("route_company_operated", "route", "Operating model", "Is this movement company-operated?", null, "This helps separate company, vendor, and brokered moves.", "route.company_operated", FactValueTypes.Boolean, 10, true, "not_sure", areas: new[] { "Transportation" }, nextActions: new[] { "Confirm who will actually run the move." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }, alwaysAsk: true),
                ChoiceQuestion("route_vendor_operated", "route", "Vendor operated", "Is a vendor or carrier operating the movement?", null, "Vendor-operated moves need different follow-up facts.", "route.vendor_operated", FactValueTypes.Boolean, 20, true, "not_sure", areas: new[] { "Carrier" }, nextActions: new[] { "Confirm the operating carrier or vendor." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("route_brokered", "route", "Brokered", "Is this brokered rather than directly operated?", null, "Brokered moves can require extra carrier review.", "route.brokered", FactValueTypes.Boolean, 30, true, "not_sure", areas: new[] { "Brokered moves" }, nextActions: new[] { "Confirm broker involvement if any." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("route_interstate", "route", "Interstate", "Does the move cross state lines?", null, "Interstate moves often trigger broader transportation coverage.", "route.interstate", FactValueTypes.Boolean, 40, true, "not_sure", areas: new[] { "Transportation" }, nextActions: new[] { "Confirm the route crosses state lines or not." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("route_passenger", "route", "Passenger movement", "Does the move carry passengers?", null, "Passenger movement may need special review.", "route.passenger", FactValueTypes.Boolean, 50, true, "not_sure", areas: new[] { "Passenger transport" }, nextActions: new[] { "Confirm any passenger exposure." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("route_property", "route", "Property movement", "Does the move carry property or cargo?", null, "Property movement affects cargo and hazmat facts.", "route.property", FactValueTypes.Boolean, 60, true, "not_sure", areas: new[] { "Cargo" }, nextActions: new[] { "Confirm property or cargo exposure." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("route_hazmat", "route", "Hazmat", "Could the trip involve hazmat?", null, "Hazmat trips need a stronger fact set before dispatch.", "route.hazmat", FactValueTypes.Boolean, 70, true, "not_sure", areas: new[] { "Hazmat" }, nextActions: new[] { "Confirm whether hazmat is present or not." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
                ChoiceQuestion("route_driver", "route", "Driver or carrier", "Which driver, asset, or carrier is involved?", null, "This links the trip to the operational records.", "route.driver_or_carrier", "string", 80, false, "not_sure", areas: new[] { "Linkage" }, nextActions: new[] { "Connect the trip to the responsible asset or carrier." }, options: new[]
                {
                    new QuestionnaireAnswerOptionDefinition("recorded", "Recorded elsewhere", "Already captured on the source record.", "recorded", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m, true),
                    NotSure,
                    Skip,
                }),
            };

            return new QuestionnaireTemplate(
                "route_order_create",
                "Route or order questionnaire",
                "Confirm the movement facts before dispatch or order closeout.",
                productKey,
                workflowKey,
                subjectType,
                questions,
                new[] { "Transportation", "Carrier", "Cargo", "Hazmat" },
                new[] { "Confirm driver, carrier, interstate, and hazmat exposure before dispatch." },
                new[] { new QuestionnaireExceptionResponse("dispatch_blocker", "Dispatch blocker", "The move is not ready until the route facts are complete.", "high") },
                true);
        }

        private static QuestionnaireTemplate BuildDocumentTemplate(string productKey, string workflowKey, string subjectType)
        {
            var questions = new List<QuestionnaireQuestionDefinition>
            {
                ChoiceQuestion("document_type", "document", "Document type", "What kind of document is being uploaded?", null, "The document type helps route the evidence.", "document.document_type", "string", 10, true, "not_sure", areas: new[] { "Evidence" }, nextActions: new[] { "Attach the document to the supporting record." }, options: new[]
                {
                    new QuestionnaireAnswerOptionDefinition("sds", "SDS", "Safety data sheet or product sheet.", "sds", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m, true),
                    new QuestionnaireAnswerOptionDefinition("registration", "Registration or certificate", "A registration or certificate document.", "registration", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("photo", "Photo", "A supporting photo.", "photo", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("other", "Other", "Some other document type.", "other", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    NotSure,
                    Skip,
                }, alwaysAsk: true),
                ChoiceQuestion("document_subject", "document", "Supports what?", "What is this document supporting?", null, "This links the upload to the relevant operational record.", "document.supports_subject", "string", 20, true, "not_sure", areas: new[] { "Linkage", "Evidence" }, nextActions: new[] { "Link the document to the subject record." }, options: new[]
                {
                    new QuestionnaireAnswerOptionDefinition("asset", "Asset", "Supports an asset record.", "asset", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m, true),
                    new QuestionnaireAnswerOptionDefinition("person", "Person", "Supports a person record.", "person", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("location", "Location", "Supports a location record.", "location", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("vendor", "Vendor", "Supports a vendor record.", "vendor", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("material", "Material", "Supports a material record.", "material", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    new QuestionnaireAnswerOptionDefinition("route", "Route or order", "Supports a route or order record.", "route", FactValueTypes.String, "choice", QuestionnaireReviewStatuses.Confirmed, 1m),
                    NotSure,
                    Skip,
                }),
                ChoiceQuestion("document_upload", "document", "File upload", "Upload the document now.", "Attach the file directly if the product has the upload widget.", "Document uploads become reviewable evidence references.", "document.evidence_url", "string", 30, true, "not_sure", areas: new[] { "Evidence" }, nextActions: new[] { "Review the uploaded file and the supporting record." }, options: new[] { Document, NotSure, Skip }),
            };

            return new QuestionnaireTemplate(
                "document_upload",
                "Document upload questionnaire",
                "Capture a reviewable evidence record with a plain-language prompt.",
                productKey,
                workflowKey,
                subjectType,
                questions,
                new[] { "Evidence", "Linkage" },
                new[] { "Confirm the uploaded document is attached to the right subject." },
                Array.Empty<QuestionnaireExceptionResponse>(),
                false);
        }

        private static QuestionnaireTemplate BuildFallbackTemplate(string productKey, string workflowKey, string subjectType)
        {
            var questions = new[]
            {
                ChoiceQuestion("fallback_question", "general", "General question", "What is the main operational fact we need to confirm?", null, "This keeps the engine usable even if a template is missing.", "general.fact", "string", 10, true, "not_sure", areas: new[] { "General" }, nextActions: new[] { "Capture the missing operational fact." }, options: new[] { Yes, No, Sometimes, NotSure, Skip }),
            };

            return new QuestionnaireTemplate(
                $"fallback:{productKey}:{workflowKey}:{subjectType}",
                "Questionnaire",
                "A fallback questionnaire for unsupported workflows.",
                productKey,
                workflowKey,
                subjectType,
                questions,
                Array.Empty<string>(),
                new[] { "Review the workflow mapping after submission." },
                Array.Empty<QuestionnaireExceptionResponse>(),
                false);
        }

        private static QuestionnaireQuestionDefinition ChoiceQuestion(
            string questionKey,
            string sectionKey,
            string sectionLabel,
            string prompt,
            string? helpText,
            string? whyItMatters,
            string factKey,
            string factValueType,
            int priority,
            bool required,
            string? defaultOptionKey,
            IReadOnlyList<string> areas,
            IReadOnlyList<string> nextActions,
            IReadOnlyList<QuestionnaireAnswerOptionDefinition> options,
            bool alwaysAsk = false,
            Func<QuestionnaireRequestContext, bool>? appliesWhen = null) =>
            new(
                questionKey,
                sectionKey,
                sectionLabel,
                prompt,
                helpText,
                whyItMatters,
                "choice",
                factKey,
                factValueType,
                required,
                priority,
                defaultOptionKey,
                options,
                areas,
                nextActions,
                alwaysAsk,
                appliesWhen);

        private static Dictionary<string, string> MergeFacts(
            IReadOnlyDictionary<string, string> knownFacts,
            IReadOnlyList<QuestionnaireAnswerModel>? answers)
        {
            var facts = new Dictionary<string, string>(knownFacts, StringComparer.OrdinalIgnoreCase);
            if (answers is null)
            {
                return facts;
            }

            foreach (var answer in answers)
            {
                facts[answer.NormalizedFactKey] = answer.NormalizedFactValue;
            }

            return facts;
        }

        private static IReadOnlyList<string> SplitProfileList(IReadOnlyDictionary<string, string> facts, string key)
        {
            if (!facts.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<string>();
            }

            return value.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string FollowUpPromptForFact(string factKey) =>
            factKey switch
            {
                "tenant.profile.business_profile" => "What best describes the business?",
                "tenant.profile.transportation_exposure" => "Does the business move people or property in vehicles or through vendors?",
                "tenant.profile.workforce_exposure" => "Does the business have drivers, maintenance staff, or other safety-sensitive workers?",
                "tenant.profile.location_exposure" => "Do the business locations include yards, shops, docks, warehouses, or customer sites?",
                "tenant.profile.material_hazmat_exposure" => "Does the business handle chemicals, fuel, batteries, or other regulated materials?",
                "tenant.profile.record_document_maturity" => "How mature are the business records and document files today?",
                "asset.kind" => "What kind of asset is this?",
                "asset.use" => "How is the asset used most of the time?",
                "asset.leaves_company_property" => "Does the asset ever leave company property?",
                "asset.tows_trailers" => "Does the asset tow trailers?",
                "asset.carries_passengers" => "Does the asset carry passengers?",
                "asset.handles_placarded_hazmat" => "Can the asset handle placarded hazmat?",
                "asset.base_location" => "Where is the asset mainly based?",
                "person.work" => "What kind of work does this person do?",
                "person.drives" => "Does this person drive for work?",
                "person.operates_equipment" => "Does this person operate equipment or machinery?",
                "person.safety_sensitive_supervision" => "Does this person supervise safety-sensitive work?",
                "person.maintenance_work" => "Does this person do repair or maintenance work?",
                "person.handles_hazmat" => "Does this person handle hazardous materials?",
                "location.kind" => "What type of location is this?",
                "location.activities" => "What happens at this location most often?",
                "location.special_storage" => "Is anything special stored here, like fuel, chemicals, or batteries?",
                "vendor.operates_transport" => "Does this vendor transport goods or people?",
                "vendor.repairs_or_inspects" => "Does this vendor repair or inspect assets?",
                "vendor.supplies_chemicals" => "Does this vendor supply chemicals or other materials?",
                "vendor.tracked_certs" => "Do you need insurance or certificates tracked for this vendor?",
                "material.kind" => "What kind of material is this?",
                "material.sds_backed" => "Is there an SDS or similar product sheet for it?",
                "material.special_storage" => "Does it need special storage?",
                "material.placarded_during_transport" => "Could it be placarded during transport?",
                "route.company_operated" => "Is this movement company-operated?",
                "route.vendor_operated" => "Is a vendor or carrier operating the movement?",
                "route.brokered" => "Is this brokered rather than directly operated?",
                "route.interstate" => "Does the move cross state lines?",
                "route.passenger" => "Does the move carry passengers?",
                "route.property" => "Does the move carry property or cargo?",
                "route.hazmat" => "Could the trip involve hazmat?",
                "route.driver_or_carrier" => "Which driver, asset, or carrier is involved?",
                "document.document_type" => "What kind of document is being uploaded?",
                "document.supports_subject" => "What is this document supporting?",
                "document.evidence_url" => "Upload the document now.",
                _ => $"Please confirm the operational fact for {factKey}.",
            };

        private static string FollowUpReasonForFact(string factKey, IReadOnlyList<QuestionnaireAnswerModel> answers)
        {
            var answer = answers.FirstOrDefault(item => string.Equals(item.NormalizedFactKey, factKey, StringComparison.OrdinalIgnoreCase));
            if (answer is null)
            {
                return "The answer was missing, deferred, or uncertain.";
            }

            return answer.ReviewStatus switch
            {
                var status when string.Equals(status, QuestionnaireReviewStatuses.Conflict, StringComparison.OrdinalIgnoreCase) =>
                    "The answer conflicts with an existing compliance fact and needs review.",
                var status when string.Equals(status, QuestionnaireReviewStatuses.Unknown, StringComparison.OrdinalIgnoreCase) =>
                    "The answer was marked not sure and needs review.",
                var status when string.Equals(status, QuestionnaireReviewStatuses.Deferred, StringComparison.OrdinalIgnoreCase) =>
                    "The answer was skipped for now and can be revisited later.",
                _ => "The answer needs review.",
            };
        }
    }
}
