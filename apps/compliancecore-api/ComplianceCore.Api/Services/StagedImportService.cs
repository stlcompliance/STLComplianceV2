using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Csv;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed record UploadedImportFile(
    string SourceFile,
    string OriginalFilename,
    string Content,
    long ByteLength);

public sealed class StagedImportService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public const string EvidenceReferencesFile = "evidence_references.csv";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> FileHeaders =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [CsvBundleFiles.ControlledVocabulary] =
                ["term_key", "vocabulary_type_key", "label", "description", "active"],
            [CsvBundleFiles.VocabularyAliases] = ["term_key", "alias_text", "active"],
            [CsvBundleFiles.ComplianceKeys] = ["key", "label", "category", "description", "active"],
            [CsvBundleFiles.MaterialKeys] = ["key", "label", "category", "description", "active"],
            [CsvBundleFiles.RulePacks] =
            [
                "pack_key",
                "program_key",
                "version_number",
                "label",
                "description",
                "status",
                "active",
                "rule_content_json"
            ],
            [CsvBundleFiles.RuleRequirements] =
            [
                "citation_key",
                "program_key",
                "pack_key",
                "pack_version",
                "label",
                "source_reference",
                "description",
                "active",
                "supersedes_citation_key"
            ],
            [CsvBundleFiles.RuleFactRequirements] =
            [
                "requirement_key",
                "fact_key",
                "pack_key",
                "pack_version",
                "citation_key",
                "citation_version",
                "applicability_key",
                "source_product",
                "source_entity",
                "source_field_or_record_type",
                "value_type",
                "operator",
                "expected_value",
                "evidence_kind",
                "required_document_type",
                "retention_period",
                "audit_question",
                "failure_severity",
                "automatic_failure_flag",
                "override_allowed",
                "override_permission",
                "remediation_required",
                "label",
                "description",
                "is_required",
                "active"
            ],
            [CsvBundleFiles.RegulatoryMappings] =
            [
                "mapping_key",
                "target_kind",
                "program_key",
                "pack_key",
                "pack_version",
                "citation_key",
                "compliance_key",
                "material_key",
                "fact_key",
                "label",
                "description",
                "active"
            ],
            [CsvBundleFiles.SdsReferences] =
            [
                "sds_key",
                "material_key",
                "product_name",
                "manufacturer",
                "document_url",
                "revision_date",
                "active"
            ],
            [CsvBundleFiles.ExceptionExemptions] =
            [
                "key",
                "label",
                "type",
                "governing_body",
                "program_key",
                "pack_key",
                "citation_key",
                "applicability_key",
                "applies_to_subject_kind",
                "applies_to_source_product",
                "applies_to_source_entity",
                "effect_type",
                "condition_logic_json",
                "required_evidence_option_group_key",
                "issuing_authority",
                "authorization_number",
                "effective_at",
                "expires_at",
                "active",
                "description"
            ],
            [EvidenceReferencesFile] =
            [
                "evidence_id",
                "fact_key",
                "source_product",
                "source_entity",
                "source_record_id",
                "source_field",
                "document_type",
                "document_url",
                "storage_key",
                "file_hash",
                "captured_at",
                "effective_at",
                "expires_at",
                "review_status",
                "notes"
            ]
        };

    private static readonly IReadOnlySet<string> BooleanColumns =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "active",
            "automatic_failure_flag",
            "override_allowed",
            "remediation_required",
            "is_required"
        };

    private static readonly IReadOnlySet<string> KnownSourceEntities =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "asset",
            "certificate",
            "component",
            "defect",
            "document",
            "driver",
            "employee",
            "external_registry",
            "inspection",
            "license",
            "load",
            "material",
            "part",
            "person",
            "repair",
            "route",
            "sds",
            "system",
            "training_completion",
            "trip",
            "vehicle",
            "vendor"
        };

    public static bool IsSupportedSourceFile(string fileName) =>
        FileHeaders.ContainsKey(Path.GetFileName(fileName));

    public static IReadOnlyDictionary<string, IReadOnlyList<string>> SupportedHeaders => FileHeaders;

    public async Task<ImportSessionResponse> CreateSessionAsync(
        Guid tenantId,
        Guid? actorPersonId,
        CreateImportSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var session = new ImportSession
        {
            ImportSessionId = Guid.NewGuid(),
            TenantId = tenantId,
            UploadedByPersonId = actorPersonId,
            SourceFilename = request.SourceFilename?.Trim() ?? string.Empty,
            SourceHash = string.Empty,
            ImportType = string.IsNullOrWhiteSpace(request.ImportType)
                ? ImportSessionImportTypes.ComplianceCoreCsvBundle
                : request.ImportType.Trim(),
            Status = ImportSessionStatuses.Uploaded,
            ValidationStatus = ImportSessionValidationStatuses.NotValidated,
            MappingStatus = ImportSessionMappingStatuses.NotStarted,
            CommitStatus = ImportSessionCommitStatuses.NotCommitted,
            CreatedAt = now,
            Notes = request.Notes?.Trim() ?? string.Empty
        };

        db.ImportSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(session);
    }

    public async Task<ImportUploadResponse> UploadAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid? actorPersonId,
        IReadOnlyList<UploadedImportFile> files,
        CancellationToken cancellationToken = default)
    {
        var session = await LoadSessionAsync(tenantId, importSessionId, cancellationToken);
        if (files.Count == 0)
        {
            throw new StlApiException("import_sessions.no_files", "Upload at least one supported CSV file or ZIP bundle.", 400);
        }

        await ClearSessionWorkAsync(importSessionId, includeSourceFiles: true, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var normalizedFiles = files
            .Where(file => FileHeaders.ContainsKey(file.SourceFile))
            .GroupBy(file => file.SourceFile, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Last())
            .OrderBy(file => file.SourceFile, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedFiles.Count == 0)
        {
            throw new StlApiException("import_sessions.no_supported_files", "No supported Compliance Core CSV files were found.", 400);
        }

        var combinedHashBuilder = new StringBuilder();
        foreach (var file in normalizedFiles)
        {
            var hash = Sha256(file.Content);
            combinedHashBuilder.Append(file.SourceFile).Append(':').Append(hash).Append(';');
            db.ImportSessionSourceFiles.Add(new ImportSessionSourceFile
            {
                ImportSessionSourceFileId = Guid.NewGuid(),
                TenantId = tenantId,
                ImportSessionId = importSessionId,
                SourceFile = file.SourceFile,
                OriginalFilename = file.OriginalFilename,
                Content = file.Content,
                FileHash = hash,
                ByteLength = file.ByteLength,
                ValidationStatus = ImportRowValidationStatuses.Pending,
                ValidationErrorsJson = "[]",
                CreatedAt = now
            });
        }

        session.UploadedByPersonId = actorPersonId ?? session.UploadedByPersonId;
        session.SourceFilename = normalizedFiles.Count == 1
            ? normalizedFiles[0].OriginalFilename
            : session.SourceFilename.Length > 0
                ? session.SourceFilename
                : "compliancecore-csv-bundle";
        session.SourceHash = Sha256(combinedHashBuilder.ToString());
        session.Status = ImportSessionStatuses.Uploaded;
        session.ValidationStatus = ImportSessionValidationStatuses.NotValidated;
        session.MappingStatus = ImportSessionMappingStatuses.NotStarted;
        session.CommitStatus = ImportSessionCommitStatuses.NotCommitted;
        session.ValidatedAt = null;
        session.MappedAt = null;
        session.CommittedAt = null;
        session.RejectedAt = null;

        await db.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(
            "import_session.uploaded",
            tenantId,
            actorPersonId,
            "import_session",
            importSessionId.ToString(),
            "success",
            cancellationToken: cancellationToken);

        var sourceFiles = await db.ImportSessionSourceFiles
            .AsNoTracking()
            .Where(file => file.ImportSessionId == importSessionId)
            .OrderBy(file => file.SourceFile)
            .ToListAsync(cancellationToken);

        return new ImportUploadResponse(ToResponse(session), sourceFiles.Select(ToResponse).ToList());
    }

    public async Task<ImportParseResponse> ParseAsync(
        Guid tenantId,
        Guid importSessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await LoadSessionAsync(tenantId, importSessionId, cancellationToken);
        var files = await db.ImportSessionSourceFiles
            .Where(file => file.ImportSessionId == importSessionId)
            .OrderBy(file => file.SourceFile)
            .ToListAsync(cancellationToken);

        if (files.Count == 0)
        {
            throw new StlApiException("import_sessions.no_files", "Upload files before parsing the import session.", 400);
        }

        await ClearSessionWorkAsync(importSessionId, includeSourceFiles: false, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var file in files)
        {
            var parseResult = ParseCsv(file.SourceFile, file.Content);
            file.ValidationStatus = parseResult.Errors.Count == 0
                ? ImportRowValidationStatuses.Pending
                : ImportRowValidationStatuses.Invalid;
            file.ValidationErrorsJson = Serialize(parseResult.Errors);

            foreach (var row in parseResult.Rows)
            {
                AddStagedRow(tenantId, importSessionId, file.SourceFile, row, now);
            }
        }

        session.Status = ImportSessionStatuses.Parsed;
        session.ValidationStatus = ImportSessionValidationStatuses.NotValidated;
        session.MappingStatus = ImportSessionMappingStatuses.NotStarted;
        session.CommitStatus = ImportSessionCommitStatuses.NotCommitted;
        await db.SaveChangesAsync(cancellationToken);

        var summaries = await BuildFileSummariesAsync(importSessionId, cancellationToken);
        return new ImportParseResponse(ToResponse(session), summaries);
    }

    public async Task<ImportValidationResultsResponse> ValidateAsync(
        Guid tenantId,
        Guid importSessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await LoadSessionAsync(tenantId, importSessionId, cancellationToken);
        var files = await db.ImportSessionSourceFiles
            .Where(file => file.ImportSessionId == importSessionId)
            .ToListAsync(cancellationToken);

        if (files.Count == 0)
        {
            throw new StlApiException("import_sessions.no_files", "Upload and parse files before validation.", 400);
        }

        var rows = await LoadTrackedRowsAsync(importSessionId, cancellationToken);
        var lookups = await BuildValidationLookupsAsync(tenantId, rows, cancellationToken);
        var duplicateKeys = rows
            .Where(row => !string.IsNullOrWhiteSpace(row.CanonicalKeyCandidate))
            .GroupBy(row => $"{row.SourceFile}:{row.CanonicalKeyCandidate}", StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var values = ToDictionary(row.NormalizedRowJson);
            var errors = ValidateRow(row.SourceFile, values, lookups);
            if (duplicateKeys.Contains($"{row.SourceFile}:{row.CanonicalKeyCandidate}"))
            {
                errors.Add($"Duplicate deterministic key '{row.CanonicalKeyCandidate}' in {row.SourceFile}.");
            }

            SetValidation(row, errors);
        }

        var invalidFileCount = 0;
        foreach (var file in files)
        {
            var errors = DeserializeList(file.ValidationErrorsJson);
            if (errors.Count > 0)
            {
                invalidFileCount++;
                file.ValidationStatus = ImportRowValidationStatuses.Invalid;
            }
            else
            {
                file.ValidationStatus = ImportRowValidationStatuses.Valid;
            }
        }

        var invalidRows = rows.Count(row => row.ValidationStatus == ImportRowValidationStatuses.Invalid);
        var passed = invalidRows == 0 && invalidFileCount == 0;
        session.ValidationStatus = passed
            ? ImportSessionValidationStatuses.Passed
            : ImportSessionValidationStatuses.Failed;
        session.Status = passed
            ? ImportSessionStatuses.ValidationPassed
            : ImportSessionStatuses.ValidationFailed;
        session.ValidatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return await GetValidationResultsAsync(tenantId, importSessionId, cancellationToken);
    }

    public async Task<ImportValidationResultsResponse> GetValidationResultsAsync(
        Guid tenantId,
        Guid importSessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await LoadSessionAsync(tenantId, importSessionId, cancellationToken);
        var rows = await LoadRowsAsync(importSessionId, cancellationToken);
        var files = await db.ImportSessionSourceFiles
            .AsNoTracking()
            .Where(file => file.ImportSessionId == importSessionId)
            .OrderBy(file => file.SourceFile)
            .ToListAsync(cancellationToken);

        return new ImportValidationResultsResponse(
            importSessionId,
            session.ValidationStatus,
            rows.Count,
            rows.Count(row => row.ValidationStatus == ImportRowValidationStatuses.Valid),
            rows.Count(row => row.ValidationStatus == ImportRowValidationStatuses.Invalid),
            files.Select(ToResponse).ToList(),
            rows
                .OrderBy(row => row.SourceFile)
                .ThenBy(row => row.RowNumber)
                .Select(row => new ImportStagedRowResultResponse(
                    row.StagedRowId,
                    row.SourceFile,
                    row.RowNumber,
                    row.CanonicalKeyCandidate,
                    row.ValidationStatus,
                    DeserializeList(row.ValidationErrorsJson)))
                .ToList());
    }

    public async Task<IReadOnlyList<MappingCandidateResponse>> GenerateMappingCandidatesAsync(
        Guid tenantId,
        Guid importSessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await LoadSessionAsync(tenantId, importSessionId, cancellationToken);
        if (session.ValidationStatus != ImportSessionValidationStatuses.Passed)
        {
            throw new StlApiException("import_sessions.validation_required", "Validation must pass before mapping candidates are generated.", 400);
        }

        await ClearCandidatesAndDecisionsAsync(importSessionId, cancellationToken);
        var stagedRows = await db.ImportStagedFactRequirements
            .AsNoTracking()
            .Where(row => row.ImportSessionId == importSessionId && row.ValidationStatus == ImportRowValidationStatuses.Valid)
            .OrderBy(row => row.RowNumber)
            .ToListAsync(cancellationToken);
        var lookup = await BuildMappingLookupAsync(tenantId, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        foreach (var row in stagedRows)
        {
            var values = ToDictionary(row.NormalizedRowJson);
            foreach (var option in BuildEvidenceOptions(importSessionId, values))
            {
                var target = FindBestTarget(option, values, lookup);
                var candidate = new ImportStagedMappingCandidate
                {
                    MappingCandidateId = Guid.NewGuid(),
                    TenantId = tenantId,
                    ImportSessionId = importSessionId,
                    StagedRowId = row.StagedRowId,
                    StagedSourceFile = row.SourceFile,
                    StagedRowNumber = row.RowNumber,
                    SourceKey = values.GetValueOrDefault("requirement_key") ?? string.Empty,
                    SourceLabel = values.GetValueOrDefault("label") ?? string.Empty,
                    EvidenceOptionId = option.EvidenceOptionId,
                    EvidenceOptionKey = option.OptionKey,
                    EvidenceOptionLabel = option.OptionLabel,
                    OptionLogicGroup = option.LogicType,
                    TargetKind = target.TargetKind,
                    TargetId = target.TargetId,
                    TargetKey = target.TargetKey,
                    TargetLabel = target.TargetLabel,
                    ConfidenceScore = target.ConfidenceScore,
                    ConfidenceBand = ToBand(target.ConfidenceScore),
                    MatchReasonsJson = Serialize(target.Reasons),
                    RiskFlagsJson = Serialize(target.Risks),
                    ProposedAction = target.ProposedAction,
                    SatisfiesRequirementIfConfirmed = target.SatisfiesRequirementIfConfirmed,
                    RequiresAdditionalSupportingEvidence = option.LogicType == EvidenceOptionLogicTypes.AllOf,
                    RequiresConfirmation = true,
                    CreatedAt = now
                };
                db.ImportStagedMappingCandidates.Add(candidate);
            }
        }

        session.Status = ImportSessionStatuses.MappingRequired;
        session.MappingStatus = ImportSessionMappingStatuses.Required;
        session.MappedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        return await GetMappingCandidatesAsync(tenantId, importSessionId, cancellationToken);
    }

    public async Task<IReadOnlyList<MappingCandidateResponse>> GetMappingCandidatesAsync(
        Guid tenantId,
        Guid importSessionId,
        CancellationToken cancellationToken = default)
    {
        await LoadSessionAsync(tenantId, importSessionId, cancellationToken);
        var candidates = await db.ImportStagedMappingCandidates
            .AsNoTracking()
            .Where(candidate => candidate.ImportSessionId == importSessionId)
            .OrderByDescending(candidate => candidate.ConfidenceScore)
            .ThenBy(candidate => candidate.StagedRowNumber)
            .ToListAsync(cancellationToken);
        return candidates.Select(ToResponse).ToList();
    }

    public async Task<WizardSummaryResponse> GetWizardSummaryAsync(
        Guid tenantId,
        Guid importSessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await LoadSessionAsync(tenantId, importSessionId, cancellationToken);
        var rows = await db.ImportStagedFactRequirements
            .AsNoTracking()
            .Where(row => row.ImportSessionId == importSessionId && row.ValidationStatus == ImportRowValidationStatuses.Valid)
            .ToListAsync(cancellationToken);
        var decisions = await db.ImportStagedMappingDecisions
            .AsNoTracking()
            .Where(decision => decision.ImportSessionId == importSessionId)
            .ToListAsync(cancellationToken);
        var candidates = await db.ImportStagedMappingCandidates
            .AsNoTracking()
            .Where(candidate => candidate.ImportSessionId == importSessionId)
            .ToListAsync(cancellationToken);
        var decidedRows = decisions.Select(decision => decision.StagedRowId).ToHashSet();

        return new WizardSummaryResponse(
            importSessionId,
            session.Status,
            session.MappingStatus,
            rows.Count,
            rows.Count(row => !decidedRows.Contains(row.StagedRowId)),
            decisions.Count(IsConfirmedDecision),
            decisions.Count(IsChangedDecision),
            decisions.Count(decision => decision.Decision == ImportMappingDecisions.Skip),
            decisions.Count(decision => decision.Decision == ImportMappingDecisions.Reject),
            rows.Count(row => row.ValidationStatus == ImportRowValidationStatuses.Invalid),
            candidates.Count(candidate => candidate.ConfidenceBand == MappingConfidenceBands.Exact && DeserializeList(candidate.RiskFlagsJson).Count == 0),
            candidates.Count(candidate => candidate.ConfidenceBand == MappingConfidenceBands.High && DeserializeList(candidate.RiskFlagsJson).Count == 0),
            candidates.Count(candidate => DeserializeList(candidate.RiskFlagsJson).Count > 0));
    }

    public async Task<WizardItemResponse?> GetNextWizardItemAsync(
        Guid tenantId,
        Guid importSessionId,
        CancellationToken cancellationToken = default)
    {
        await LoadSessionAsync(tenantId, importSessionId, cancellationToken);
        var decidedRows = await db.ImportStagedMappingDecisions
            .AsNoTracking()
            .Where(decision => decision.ImportSessionId == importSessionId)
            .Select(decision => decision.StagedRowId)
            .ToListAsync(cancellationToken);
        var decided = decidedRows.ToHashSet();
        var candidate = await db.ImportStagedMappingCandidates
            .AsNoTracking()
            .Where(candidate => candidate.ImportSessionId == importSessionId && !decided.Contains(candidate.StagedRowId))
            .OrderBy(candidate => candidate.StagedRowNumber)
            .ThenByDescending(candidate => candidate.ConfidenceScore)
            .FirstOrDefaultAsync(cancellationToken);

        return candidate is null
            ? null
            : await BuildWizardItemAsync(tenantId, importSessionId, candidate.MappingCandidateId, cancellationToken);
    }

    public async Task<WizardItemResponse> BuildWizardItemAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        await LoadSessionAsync(tenantId, importSessionId, cancellationToken);
        var candidate = await db.ImportStagedMappingCandidates
            .AsNoTracking()
            .Where(candidate => candidate.ImportSessionId == importSessionId &&
                                (candidate.MappingCandidateId == itemId || candidate.StagedRowId == itemId))
            .OrderByDescending(candidate => candidate.ConfidenceScore)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new StlApiException("import_wizard.item_not_found", "Wizard item was not found.", 404);

        return await BuildWizardItemAsync(tenantId, importSessionId, candidate, cancellationToken);
    }

    public async Task<IReadOnlyList<EvidenceOptionProposalResponse>> GetEvidenceOptionsAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var item = await BuildWizardItemAsync(tenantId, importSessionId, itemId, cancellationToken);
        return new[] { item.SuggestedEvidencePath }
            .Concat(item.OtherAcceptableEvidencePaths)
            .GroupBy(option => option.EvidenceOptionKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    public async Task<MappingDecisionResponse> ConfirmAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        Guid actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var candidate = await LoadCandidateAsync(tenantId, importSessionId, itemId, cancellationToken);
        await EnsureRowCanBeMappedAsync(importSessionId, candidate.StagedRowId, cancellationToken);
        return await UpsertDecisionAsync(
            tenantId,
            importSessionId,
            candidate,
            ImportMappingDecisions.ConfirmCandidate,
            actorPersonId,
            candidate.EvidenceOptionId,
            candidate.EvidenceOptionKey,
            candidate.TargetKind,
            candidate.TargetId,
            candidate.TargetKey,
            "{}",
            false,
            string.Empty,
            cancellationToken);
    }

    public async Task<MappingDecisionResponse> SelectEvidenceOptionAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        SelectEvidenceOptionRequest request,
        Guid actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var candidate = await LoadCandidateAsync(tenantId, importSessionId, itemId, cancellationToken);
        await EnsureRowCanBeMappedAsync(importSessionId, candidate.StagedRowId, cancellationToken);
        var optionCandidate = await db.ImportStagedMappingCandidates
            .Where(next => next.ImportSessionId == importSessionId &&
                           next.StagedRowId == candidate.StagedRowId &&
                           next.EvidenceOptionKey == request.EvidenceOptionKey)
            .OrderByDescending(next => next.ConfidenceScore)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new StlApiException("import_wizard.evidence_option_not_found", "Evidence option was not found for this item.", 404);

        return await UpsertDecisionAsync(
            tenantId,
            importSessionId,
            optionCandidate,
            ImportMappingDecisions.SelectEvidenceOption,
            actorPersonId,
            optionCandidate.EvidenceOptionId,
            optionCandidate.EvidenceOptionKey,
            optionCandidate.TargetKind,
            optionCandidate.TargetId,
            optionCandidate.TargetKey,
            "{}",
            false,
            string.Empty,
            cancellationToken);
    }

    public async Task<MappingDecisionResponse> SelectTargetAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        SelectTargetRequest request,
        Guid actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var candidate = await LoadCandidateAsync(tenantId, importSessionId, itemId, cancellationToken);
        await EnsureRowCanBeMappedAsync(importSessionId, candidate.StagedRowId, cancellationToken);
        return await UpsertDecisionAsync(
            tenantId,
            importSessionId,
            candidate,
            ImportMappingDecisions.SelectExisting,
            actorPersonId,
            candidate.EvidenceOptionId,
            candidate.EvidenceOptionKey,
            request.TargetKind,
            request.TargetId,
            request.TargetKey,
            Serialize(new Dictionary<string, string> { ["targetLabel"] = request.TargetLabel }),
            false,
            string.Empty,
            cancellationToken);
    }

    public async Task<MappingDecisionResponse> CreateTargetAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        CreateTargetRequest request,
        Guid actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var candidate = await LoadCandidateAsync(tenantId, importSessionId, itemId, cancellationToken);
        await EnsureRowCanBeMappedAsync(importSessionId, candidate.StagedRowId, cancellationToken);
        var payload = new Dictionary<string, string>(request.Payload, StringComparer.OrdinalIgnoreCase);
        var targetKey = payload.GetValueOrDefault("stableKey")
            ?? payload.GetValueOrDefault("targetKey")
            ?? payload.GetValueOrDefault("key")
            ?? candidate.TargetKey;

        return await UpsertDecisionAsync(
            tenantId,
            importSessionId,
            candidate,
            ImportMappingDecisions.CreateNew,
            actorPersonId,
            candidate.EvidenceOptionId,
            candidate.EvidenceOptionKey,
            request.TargetKind,
            string.Empty,
            targetKey,
            Serialize(payload),
            false,
            string.Empty,
            cancellationToken);
    }

    public async Task<MappingDecisionResponse> MarkNoDocumentRequiredAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        Guid actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var candidate = await LoadCandidateAsync(tenantId, importSessionId, itemId, cancellationToken);
        var row = await db.ImportStagedFactRequirements
            .AsNoTracking()
            .FirstAsync(row => row.ImportSessionId == importSessionId && row.StagedRowId == candidate.StagedRowId, cancellationToken);
        var values = ToDictionary(row.NormalizedRowJson);
        if (!BuildEvidenceOptions(importSessionId, values).Any(option => option.TargetKind == EvidenceOptionTargetKinds.NoDocumentRequired) &&
            !IsNoDocumentAllowed(values.GetValueOrDefault("evidence_kind") ?? string.Empty))
        {
            throw new StlApiException(
                "import_wizard.no_document_not_allowed",
                "No-document-required is not allowed for this evidence kind.",
                400);
        }

        return await SpecialDecisionAsync(
            tenantId,
            importSessionId,
            itemId,
            ImportMappingDecisions.NoDocumentRequired,
            actorPersonId,
            MappingTargetKinds.NoDocumentRequired,
            "no_document_required",
            cancellationToken);
    }

    public Task<MappingDecisionResponse> AddSupportingEvidenceAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        SupportingEvidenceRequest request,
        Guid actorPersonId,
        CancellationToken cancellationToken = default) =>
        SpecialDecisionAsync(
            tenantId,
            importSessionId,
            itemId,
            ImportMappingDecisions.AddSupportingEvidence,
            actorPersonId,
            request.TargetKind,
            request.TargetKey,
            cancellationToken,
            request.Payload ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

    public Task<MappingDecisionResponse> MapAsNormalEvidenceAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        Guid actorPersonId,
        CancellationToken cancellationToken = default) =>
        SetEvidencePurposeAsync(
            tenantId,
            importSessionId,
            itemId,
            ImportMappingDecisions.MapAsNormalEvidence,
            ImportEvidenceMappingPurposes.NormalRequirement,
            actorPersonId,
            cancellationToken);

    public Task<MappingDecisionResponse> MapAsExceptionProofAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        ExceptionProofMappingRequest request,
        Guid actorPersonId,
        CancellationToken cancellationToken = default) =>
        SetExceptionProofDecisionAsync(
            tenantId,
            importSessionId,
            itemId,
            request,
            ImportMappingDecisions.MapAsExceptionProof,
            ImportEvidenceMappingPurposes.ExceptionProof,
            actorPersonId,
            cancellationToken);

    public Task<MappingDecisionResponse> MapAsExemptionProofAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        ExceptionProofMappingRequest request,
        Guid actorPersonId,
        CancellationToken cancellationToken = default) =>
        SetExceptionProofDecisionAsync(
            tenantId,
            importSessionId,
            itemId,
            request,
            ImportMappingDecisions.MapAsExemptionProof,
            ImportEvidenceMappingPurposes.ExemptionProof,
            actorPersonId,
            cancellationToken);

    public Task<MappingDecisionResponse> MapAsSpecialPermitApprovalProofAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        ExceptionProofMappingRequest request,
        Guid actorPersonId,
        CancellationToken cancellationToken = default) =>
        SetExceptionProofDecisionAsync(
            tenantId,
            importSessionId,
            itemId,
            request,
            ImportMappingDecisions.MapAsSpecialPermitApprovalProof,
            ImportEvidenceMappingPurposes.WaiverVarianceSpecialPermitProof,
            actorPersonId,
            cancellationToken);

    public Task<MappingDecisionResponse> CreateExceptionExemptionAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        ExceptionProofMappingRequest request,
        Guid actorPersonId,
        CancellationToken cancellationToken = default) =>
        SetExceptionProofDecisionAsync(
            tenantId,
            importSessionId,
            itemId,
            request,
            ImportMappingDecisions.CreateNewExceptionExemptionRecord,
            ImportEvidenceMappingPurposes.ChangesApplicability,
            actorPersonId,
            cancellationToken);

    public Task<MappingDecisionResponse> SelectExceptionExemptionAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        ExceptionProofMappingRequest request,
        Guid actorPersonId,
        CancellationToken cancellationToken = default) =>
        SetExceptionProofDecisionAsync(
            tenantId,
            importSessionId,
            itemId,
            request,
            ImportMappingDecisions.SelectExistingExceptionExemptionRecord,
            ImportEvidenceMappingPurposes.ChangesApplicability,
            actorPersonId,
            cancellationToken);

    public Task<MappingDecisionResponse> MarkExceptionNotApplicableAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        Guid actorPersonId,
        CancellationToken cancellationToken = default) =>
        SetEvidencePurposeAsync(
            tenantId,
            importSessionId,
            itemId,
            ImportMappingDecisions.MarkExceptionNotApplicable,
            ImportEvidenceMappingPurposes.NormalRequirement,
            actorPersonId,
            cancellationToken);

    public Task<MappingDecisionResponse> NotApplicableAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        Guid actorPersonId,
        CancellationToken cancellationToken = default) =>
        SpecialDecisionAsync(
            tenantId,
            importSessionId,
            itemId,
            ImportMappingDecisions.NotApplicable,
            actorPersonId,
            "not_applicable",
            "not_applicable",
            cancellationToken);

    public Task<MappingDecisionResponse> ReferenceOnlyAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        Guid actorPersonId,
        CancellationToken cancellationToken = default) =>
        SpecialDecisionAsync(
            tenantId,
            importSessionId,
            itemId,
            ImportMappingDecisions.ReferenceOnly,
            actorPersonId,
            "reference_only",
            "reference_only",
            cancellationToken);

    public Task<MappingDecisionResponse> SkipAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        Guid actorPersonId,
        CancellationToken cancellationToken = default) =>
        SpecialDecisionAsync(
            tenantId,
            importSessionId,
            itemId,
            ImportMappingDecisions.Skip,
            actorPersonId,
            "skip",
            "skip",
            cancellationToken);

    public Task<MappingDecisionResponse> RejectAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        Guid actorPersonId,
        CancellationToken cancellationToken = default) =>
        SpecialDecisionAsync(
            tenantId,
            importSessionId,
            itemId,
            ImportMappingDecisions.Reject,
            actorPersonId,
            "reject",
            "reject",
            cancellationToken);

    public async Task<MappingDecisionResponse> ForceMapAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        ForceMapRequest request,
        Guid actorPersonId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.OverrideReason) || !request.RiskAcknowledged)
        {
            throw new StlApiException(
                "import_wizard.override_reason_required",
                "Force-map requires an override reason and risk acknowledgement.",
                400);
        }

        if (string.IsNullOrWhiteSpace(request.TargetKey))
        {
            throw new StlApiException(
                "import_wizard.override_blocked",
                "Force-map is blocked because the selected target is missing a required stable key.",
                400);
        }

        var candidate = await LoadCandidateAsync(tenantId, importSessionId, itemId, cancellationToken);
        await EnsureRowCanBeMappedAsync(importSessionId, candidate.StagedRowId, cancellationToken);
        if (IsIncompatibleTarget(request.TargetKind, candidate.SourceLabel, candidate.TargetKind))
        {
            throw new StlApiException(
                "import_wizard.override_blocked",
                "Force-map is blocked because the selected target is incompatible with the source product or domain.",
                400);
        }

        var decision = await UpsertDecisionAsync(
            tenantId,
            importSessionId,
            candidate,
            ImportMappingDecisions.ForceMap,
            actorPersonId,
            candidate.EvidenceOptionId,
            candidate.EvidenceOptionKey,
            request.TargetKind,
            request.TargetId,
            request.TargetKey,
            Serialize(new Dictionary<string, string> { ["targetLabel"] = request.TargetLabel }),
            true,
            request.OverrideReason.Trim(),
            cancellationToken);

        await auditService.WriteAsync(
            "import_mapping.force_mapped",
            tenantId,
            actorPersonId,
            "import_session",
            importSessionId.ToString(),
            "success",
            "override_used",
            cancellationToken);

        return decision;
    }

    public async Task<IReadOnlyList<MappingDecisionResponse>> BulkConfirmAsync(
        Guid tenantId,
        Guid importSessionId,
        BulkConfirmMappingsRequest request,
        Guid actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var band = request.ConfidenceBand.Trim().ToLowerInvariant();
        if (band is not MappingConfidenceBands.Exact and not MappingConfidenceBands.High)
        {
            throw new StlApiException(
                "import_wizard.bulk_confirm_not_allowed",
                "Only exact or high-confidence no-risk mappings can be bulk confirmed.",
                400);
        }

        if (band == MappingConfidenceBands.High && !request.SummaryConfirmed)
        {
            throw new StlApiException(
                "import_wizard.high_confidence_summary_required",
                "High-confidence bulk confirmation requires summary confirmation.",
                400);
        }

        await LoadSessionAsync(tenantId, importSessionId, cancellationToken);
        var decided = await db.ImportStagedMappingDecisions
            .AsNoTracking()
            .Where(decision => decision.ImportSessionId == importSessionId)
            .Select(decision => decision.StagedRowId)
            .ToListAsync(cancellationToken);
        var decidedRows = decided.ToHashSet();

        var candidates = await db.ImportStagedMappingCandidates
            .Where(candidate => candidate.ImportSessionId == importSessionId &&
                                candidate.ConfidenceBand == band &&
                                !decidedRows.Contains(candidate.StagedRowId))
            .ToListAsync(cancellationToken);

        var selected = candidates
            .Where(candidate => DeserializeList(candidate.RiskFlagsJson).Count == 0)
            .GroupBy(candidate => candidate.StagedRowId)
            .Select(group => group.OrderByDescending(candidate => candidate.ConfidenceScore).First())
            .ToList();

        var responses = new List<MappingDecisionResponse>();
        foreach (var candidate in selected)
        {
            await EnsureRowCanBeMappedAsync(importSessionId, candidate.StagedRowId, cancellationToken);
            responses.Add(await UpsertDecisionAsync(
                tenantId,
                importSessionId,
                candidate,
                ImportMappingDecisions.ConfirmCandidate,
                actorPersonId,
                candidate.EvidenceOptionId,
                candidate.EvidenceOptionKey,
                candidate.TargetKind,
                candidate.TargetId,
                candidate.TargetKey,
                "{}",
                false,
                string.Empty,
                cancellationToken,
                save: false));
        }

        await UpdateMappingStatusAsync(importSessionId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return responses;
    }

    public async Task<CommitPreviewResponse> GetCommitPreviewAsync(
        Guid tenantId,
        Guid importSessionId,
        CancellationToken cancellationToken = default)
    {
        await LoadSessionAsync(tenantId, importSessionId, cancellationToken);
        var rows = await db.ImportStagedFactRequirements
            .AsNoTracking()
            .Where(row => row.ImportSessionId == importSessionId && row.ValidationStatus == ImportRowValidationStatuses.Valid)
            .ToListAsync(cancellationToken);
        var decisions = await db.ImportStagedMappingDecisions
            .AsNoTracking()
            .Where(decision => decision.ImportSessionId == importSessionId)
            .ToListAsync(cancellationToken);
        var candidates = await db.ImportStagedMappingCandidates
            .AsNoTracking()
            .Where(candidate => candidate.ImportSessionId == importSessionId)
            .ToListAsync(cancellationToken);
        var invalidRows = await LoadRowsAsync(importSessionId, cancellationToken);
        var blockers = BuildCommitBlockers(rows, decisions, invalidRows);
        var actions = BuildPreviewActions(decisions, candidates);

        var preview = new CommitPreviewResponse(
            importSessionId,
            decisions.Count,
            CountTargets(actions, MappingTargetKinds.ExistingDocumentRecord, MappingTargetKinds.ExistingDocumentType),
            CountTargets(actions, MappingTargetKinds.NewDocumentRecord, MappingTargetKinds.NewDocumentType),
            CountTargets(actions, MappingTargetKinds.ExistingMaterial),
            CountTargets(actions, MappingTargetKinds.NewMaterial),
            CountTargets(actions, MappingTargetKinds.ExistingPart),
            CountTargets(actions, MappingTargetKinds.NewPart),
            CountTargets(actions, MappingTargetKinds.ExistingSystem, MappingTargetKinds.ExistingAsset),
            CountTargets(actions, MappingTargetKinds.NewSystem, MappingTargetKinds.NewAsset, MappingTargetKinds.NewEvidenceReference),
            rows.Count(row => HasCommittableDecision(row.StagedRowId, decisions)),
            rows.Count(row => HasCommittableDecision(row.StagedRowId, decisions)),
            rows.Count(row => HasCommittableDecision(row.StagedRowId, decisions)),
            rows.SelectMany(row => BuildEvidenceOptions(importSessionId, ToDictionary(row.NormalizedRowJson))).Count(),
            decisions.Count(IsEvidenceReferenceDecision),
            decisions.Count(IsExceptionProofDecision),
            decisions.Count(IsExceptionRecordDecision),
            decisions.Count(decision => decision.OverrideUsed),
            decisions.Count(decision => decision.Decision == ImportMappingDecisions.Skip),
            decisions.Count(decision => decision.Decision == ImportMappingDecisions.Reject),
            blockers,
            actions);

        var session = await db.ImportSessions.FirstAsync(session => session.ImportSessionId == importSessionId, cancellationToken);
        session.CommitStatus = ImportSessionCommitStatuses.Previewed;
        await db.SaveChangesAsync(cancellationToken);
        return preview;
    }

    public async Task<ImportCompletionReportResponse> CommitAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var validation = await ValidateAsync(tenantId, importSessionId, cancellationToken);
        if (validation.InvalidRows > 0 || validation.Files.Any(file => file.ValidationErrors.Count > 0))
        {
            throw new StlApiException("import_sessions.validation_failed", "Invalid staged rows block commit.", 400);
        }

        var preview = await GetCommitPreviewAsync(tenantId, importSessionId, cancellationToken);
        if (preview.UnresolvedBlockers.Count > 0)
        {
            throw new StlApiException("import_sessions.unresolved_blockers", string.Join(" ", preview.UnresolvedBlockers), 400);
        }

        if (db.Database.IsRelational())
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var report = await CommitCoreAsync(tenantId, importSessionId, actorPersonId, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return report;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        return await CommitCoreAsync(tenantId, importSessionId, actorPersonId, cancellationToken);
    }

    public async Task<ImportSessionResponse> RejectSessionAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var session = await LoadSessionAsync(tenantId, importSessionId, cancellationToken);
        session.Status = ImportSessionStatuses.Rejected;
        session.RejectedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(
            "import_session.rejected",
            tenantId,
            actorPersonId,
            "import_session",
            importSessionId.ToString(),
            "success",
            cancellationToken: cancellationToken);
        return ToResponse(session);
    }

    public async Task<IReadOnlyList<ComplianceCoreAuditEvent>> GetAuditLogAsync(
        Guid tenantId,
        Guid importSessionId,
        CancellationToken cancellationToken = default)
    {
        await LoadSessionAsync(tenantId, importSessionId, cancellationToken);
        return await db.AuditEvents
            .AsNoTracking()
            .Where(audit => audit.TenantId == tenantId &&
                            audit.TargetType == "import_session" &&
                            audit.TargetId == importSessionId.ToString())
            .OrderBy(audit => audit.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ImportSessionResponse> GetSessionAsync(
        Guid tenantId,
        Guid importSessionId,
        CancellationToken cancellationToken = default) =>
        ToResponse(await LoadSessionAsync(tenantId, importSessionId, cancellationToken));

    public async Task<IReadOnlyList<ImportSessionSourceFileResponse>> GetSourceFilesAsync(
        Guid tenantId,
        Guid importSessionId,
        CancellationToken cancellationToken = default)
    {
        await LoadSessionAsync(tenantId, importSessionId, cancellationToken);
        var files = await db.ImportSessionSourceFiles
            .AsNoTracking()
            .Where(file => file.ImportSessionId == importSessionId)
            .OrderBy(file => file.SourceFile)
            .ToListAsync(cancellationToken);
        return files.Select(ToResponse).ToList();
    }

    private async Task<ImportCompletionReportResponse> CommitCoreAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid actorPersonId,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var session = await db.ImportSessions.FirstAsync(session => session.ImportSessionId == importSessionId, cancellationToken);
        var decisions = await db.ImportStagedMappingDecisions
            .Where(decision => decision.ImportSessionId == importSessionId)
            .ToListAsync(cancellationToken);
        var candidates = await db.ImportStagedMappingCandidates
            .AsNoTracking()
            .Where(candidate => candidate.ImportSessionId == importSessionId)
            .ToListAsync(cancellationToken);
        var decisionByRow = decisions
            .GroupBy(decision => decision.StagedRowId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(decision => decision.DecidedAt).First());
        var created = 0;
        var updated = 0;
        var warnings = new List<string>();
        var errors = new List<string>();

        await UpsertControlledVocabularyAsync(tenantId, importSessionId, now, cancellationToken);
        await UpsertVocabularyAliasesAsync(tenantId, importSessionId, now, cancellationToken);
        await UpsertComplianceKeysAsync(tenantId, importSessionId, now, cancellationToken);
        await UpsertMaterialKeysAsync(tenantId, importSessionId, now, cancellationToken);
        await UpsertRulePacksAsync(tenantId, importSessionId, now, cancellationToken);
        await UpsertRuleRequirementsAsync(tenantId, importSessionId, now, cancellationToken);
        var factCounts = await UpsertFactRequirementsAsync(tenantId, importSessionId, decisionByRow, now, cancellationToken);
        created += factCounts.Created;
        updated += factCounts.Updated;
        await UpsertRegulatoryMappingsAsync(tenantId, importSessionId, now, cancellationToken);
        await UpsertSdsReferencesAsync(tenantId, importSessionId, now, cancellationToken);
        var exceptionCounts = await UpsertExceptionExemptionsAsync(tenantId, importSessionId, now, cancellationToken);
        created += exceptionCounts.Created;
        updated += exceptionCounts.Updated;

        var committedRows = await db.ImportStagedFactRequirements
            .AsNoTracking()
            .Where(row => row.ImportSessionId == importSessionId && row.ValidationStatus == ImportRowValidationStatuses.Valid)
            .ToListAsync(cancellationToken);

        foreach (var row in committedRows)
        {
            if (!decisionByRow.TryGetValue(row.StagedRowId, out var decision) || !IsCommittable(decision))
            {
                continue;
            }

            var values = ToDictionary(row.NormalizedRowJson);
            await UpsertEvidenceOptionsAsync(tenantId, importSessionId, values, now, cancellationToken);
            var candidate = candidates.FirstOrDefault(candidate => candidate.MappingCandidateId == decision.MappingCandidateId)
                ?? candidates.Where(candidate => candidate.StagedRowId == row.StagedRowId)
                    .OrderByDescending(candidate => candidate.ConfidenceScore)
                    .FirstOrDefault();
            if (candidate is not null)
            {
                await UpsertExceptionExemptionFromDecisionAsync(tenantId, values, decision, now, cancellationToken);
                await CreateEvidenceReferenceAndObjectMirrorAsync(
                    tenantId,
                    importSessionId,
                    row,
                    values,
                    decision,
                    candidate,
                    now,
                    cancellationToken);
            }

            db.AuditTraces.Add(new AuditTrace
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AuditTraceId = $"import:{importSessionId:N}:{row.StagedRowId:N}",
                PackKey = values.GetValueOrDefault("pack_key") ?? string.Empty,
                FactKey = values.GetValueOrDefault("fact_key") ?? string.Empty,
                CitationKey = values.GetValueOrDefault("citation_key") ?? string.Empty,
                SubjectKind = "import_mapping",
                SubjectId = row.StagedRowId.ToString(),
                EvaluatedValue = decision.Decision,
                ExpectedValue = "confirmed_mapping_decision",
                Operator = "exists",
                Result = "mapping_committed",
                FailureSeverity = values.GetValueOrDefault("failure_severity") ?? string.Empty,
                AutomaticFailureFlag = ParseBool(values.GetValueOrDefault("automatic_failure_flag")),
                OverrideUsed = decision.OverrideUsed,
                OverridePersonId = decision.OverrideUsed ? decision.DecidedByPersonId : null,
                OverrideReason = decision.OverrideReason,
                RemediationRequired = ParseBool(values.GetValueOrDefault("remediation_required"), defaultValue: true),
                ClaimedExceptionExemptionKey = decision.ExceptionExemptionKey,
                ClaimedExceptionExemptionType = ExceptionTypeFromDecision(decision),
                ExceptionExemptionLegalBasis = values.GetValueOrDefault("citation_key") ?? string.Empty,
                ExceptionExemptionProofKey = decision.SelectedTargetKey,
                ExceptionExemptionScopeResult = string.IsNullOrWhiteSpace(decision.ExceptionExemptionKey) ? "not_claimed" : "scoped_for_import_review",
                ExceptionExemptionEffectiveResult = string.IsNullOrWhiteSpace(decision.ExceptionExemptionKey) ? "not_claimed" : "pending_evidence_effective_check",
                ResultBeforeException = "normal_requirement_pending",
                ResultAfterException = decision.EvidenceMappingPurpose,
                FinalComplianceResult = decision.OverrideUsed ? "allowed_with_override" : "mapping_committed",
                ExceptionExemptionApplied = IsExceptionProofDecision(decision),
                ExceptionExemptionProofRequired = IsExceptionProofDecision(decision),
                ExceptionExemptionProofValid = IsExceptionProofDecision(decision) && !decision.OverrideUsed,
                EvaluatedAt = now
            });
        }

        session.Status = ImportSessionStatuses.Committed;
        session.CommitStatus = ImportSessionCommitStatuses.Committed;
        session.MappingStatus = ImportSessionMappingStatuses.Confirmed;
        session.CommittedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "import_session.committed",
            tenantId,
            actorPersonId,
            "import_session",
            importSessionId.ToString(),
            "success",
            cancellationToken: cancellationToken);

        var skipped = decisions.Count(decision => decision.Decision == ImportMappingDecisions.Skip);
        var rejected = decisions.Count(decision => decision.Decision == ImportMappingDecisions.Reject);
        var overrides = decisions.Count(decision => decision.OverrideUsed);
        var evidenceMappings = decisions.Count(IsEvidenceReferenceDecision);
        var newRefs = decisions.Count(decision => decision.Decision == ImportMappingDecisions.CreateNew ||
                                                 decision.SelectedTargetKind.StartsWith("new_", StringComparison.OrdinalIgnoreCase));
        var existingRefs = decisions.Count(decision => decision.SelectedTargetKind.StartsWith("existing_", StringComparison.OrdinalIgnoreCase));

        return new ImportCompletionReportResponse(
            importSessionId,
            session.Status,
            created,
            updated,
            skipped,
            rejected,
            overrides,
            evidenceMappings,
            newRefs,
            existingRefs,
            warnings,
            errors,
            $"import_session:{importSessionId}");
    }

    private async Task UpsertControlledVocabularyAsync(
        Guid tenantId,
        Guid importSessionId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var rows = await db.ImportStagedControlledVocabulary
            .AsNoTracking()
            .Where(row => row.ImportSessionId == importSessionId && row.ValidationStatus == ImportRowValidationStatuses.Valid)
            .ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            var values = ToDictionary(row.NormalizedRowJson);
            var key = values.GetValueOrDefault("term_key") ?? string.Empty;
            var existing = await db.VocabularyTerms.FirstOrDefaultAsync(
                term => term.TenantId == tenantId && term.TermKey == key,
                cancellationToken);
            if (existing is null)
            {
                db.VocabularyTerms.Add(new VocabularyTerm
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    TermKey = key,
                    VocabularyTypeKey = values.GetValueOrDefault("vocabulary_type_key") ?? string.Empty,
                    Label = values.GetValueOrDefault("label") ?? key,
                    Description = values.GetValueOrDefault("description") ?? string.Empty,
                    IsActive = ParseBool(values.GetValueOrDefault("active"), true),
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                existing.VocabularyTypeKey = values.GetValueOrDefault("vocabulary_type_key") ?? existing.VocabularyTypeKey;
                existing.Label = values.GetValueOrDefault("label") ?? existing.Label;
                existing.Description = values.GetValueOrDefault("description") ?? existing.Description;
                existing.IsActive = ParseBool(values.GetValueOrDefault("active"), true);
                existing.UpdatedAt = now;
            }
        }
    }

    private async Task UpsertVocabularyAliasesAsync(
        Guid tenantId,
        Guid importSessionId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var rows = await db.ImportStagedVocabularyAliases
            .AsNoTracking()
            .Where(row => row.ImportSessionId == importSessionId && row.ValidationStatus == ImportRowValidationStatuses.Valid)
            .ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            var values = ToDictionary(row.NormalizedRowJson);
            var termKey = values.GetValueOrDefault("term_key") ?? string.Empty;
            var aliasText = values.GetValueOrDefault("alias_text") ?? string.Empty;
            var term = await db.VocabularyTerms.FirstOrDefaultAsync(
                term => term.TenantId == tenantId && term.TermKey == termKey,
                cancellationToken);
            if (term is null)
            {
                continue;
            }

            var existing = await db.VocabularyAliases.FirstOrDefaultAsync(
                alias => alias.TenantId == tenantId &&
                         alias.VocabularyTermId == term.Id &&
                         alias.AliasText == aliasText,
                cancellationToken);
            if (existing is null)
            {
                db.VocabularyAliases.Add(new VocabularyAlias
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    VocabularyTermId = term.Id,
                    AliasText = aliasText,
                    IsActive = ParseBool(values.GetValueOrDefault("active"), true),
                    CreatedAt = now
                });
            }
            else
            {
                existing.IsActive = ParseBool(values.GetValueOrDefault("active"), true);
            }
        }
    }

    private async Task UpsertComplianceKeysAsync(Guid tenantId, Guid importSessionId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var rows = await db.ImportStagedComplianceKeys
            .AsNoTracking()
            .Where(row => row.ImportSessionId == importSessionId && row.ValidationStatus == ImportRowValidationStatuses.Valid)
            .ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            var values = ToDictionary(row.NormalizedRowJson);
            var key = values.GetValueOrDefault("key") ?? string.Empty;
            var existing = await db.ComplianceKeys.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Key == key, cancellationToken);
            if (existing is null)
            {
                db.ComplianceKeys.Add(new ComplianceKey
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Key = key,
                    Label = values.GetValueOrDefault("label") ?? key,
                    Category = values.GetValueOrDefault("category") ?? string.Empty,
                    Description = values.GetValueOrDefault("description") ?? string.Empty,
                    IsActive = ParseBool(values.GetValueOrDefault("active"), true),
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                existing.Label = values.GetValueOrDefault("label") ?? existing.Label;
                existing.Category = values.GetValueOrDefault("category") ?? existing.Category;
                existing.Description = values.GetValueOrDefault("description") ?? existing.Description;
                existing.IsActive = ParseBool(values.GetValueOrDefault("active"), true);
                existing.UpdatedAt = now;
            }
        }
    }

    private async Task UpsertMaterialKeysAsync(Guid tenantId, Guid importSessionId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var rows = await db.ImportStagedMaterialKeys
            .AsNoTracking()
            .Where(row => row.ImportSessionId == importSessionId && row.ValidationStatus == ImportRowValidationStatuses.Valid)
            .ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            var values = ToDictionary(row.NormalizedRowJson);
            var key = values.GetValueOrDefault("key") ?? string.Empty;
            var existing = await db.MaterialKeys.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Key == key, cancellationToken);
            if (existing is null)
            {
                db.MaterialKeys.Add(new MaterialKey
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Key = key,
                    Label = values.GetValueOrDefault("label") ?? key,
                    Category = values.GetValueOrDefault("category") ?? string.Empty,
                    Description = values.GetValueOrDefault("description") ?? string.Empty,
                    IsActive = ParseBool(values.GetValueOrDefault("active"), true),
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                existing.Label = values.GetValueOrDefault("label") ?? existing.Label;
                existing.Category = values.GetValueOrDefault("category") ?? existing.Category;
                existing.Description = values.GetValueOrDefault("description") ?? existing.Description;
                existing.IsActive = ParseBool(values.GetValueOrDefault("active"), true);
                existing.UpdatedAt = now;
            }
        }
    }

    private async Task UpsertRulePacksAsync(Guid tenantId, Guid importSessionId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var rows = await db.ImportStagedRulePacks
            .AsNoTracking()
            .Where(row => row.ImportSessionId == importSessionId && row.ValidationStatus == ImportRowValidationStatuses.Valid)
            .ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            var values = ToDictionary(row.NormalizedRowJson);
            var packKey = values.GetValueOrDefault("pack_key") ?? string.Empty;
            var version = ParseInt(values.GetValueOrDefault("version_number"), 1);
            var programKey = values.GetValueOrDefault("program_key") ?? string.Empty;
            var program = await db.RegulatoryPrograms.FirstOrDefaultAsync(
                program => program.TenantId == tenantId && program.ProgramKey == programKey,
                cancellationToken);
            if (program is null)
            {
                continue;
            }

            var existing = await db.RulePacks.FirstOrDefaultAsync(
                pack => pack.TenantId == tenantId && pack.PackKey == packKey && pack.VersionNumber == version,
                cancellationToken);
            if (existing is null)
            {
                db.RulePacks.Add(new RulePack
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    RegulatoryProgramId = program.Id,
                    PackKey = packKey,
                    Label = values.GetValueOrDefault("label") ?? packKey,
                    Description = values.GetValueOrDefault("description") ?? string.Empty,
                    VersionNumber = version,
                    Status = values.GetValueOrDefault("status") ?? RulePackStatuses.Draft,
                    IsActive = ParseBool(values.GetValueOrDefault("active"), true),
                    RuleContentJson = values.GetValueOrDefault("rule_content_json"),
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                existing.RegulatoryProgramId = program.Id;
                existing.Label = values.GetValueOrDefault("label") ?? existing.Label;
                existing.Description = values.GetValueOrDefault("description") ?? existing.Description;
                existing.Status = values.GetValueOrDefault("status") ?? existing.Status;
                existing.IsActive = ParseBool(values.GetValueOrDefault("active"), true);
                existing.RuleContentJson = values.GetValueOrDefault("rule_content_json");
                existing.UpdatedAt = now;
            }
        }
    }

    private async Task UpsertRuleRequirementsAsync(Guid tenantId, Guid importSessionId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var rows = await db.ImportStagedRuleRequirements
            .AsNoTracking()
            .Where(row => row.ImportSessionId == importSessionId && row.ValidationStatus == ImportRowValidationStatuses.Valid)
            .ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            var values = ToDictionary(row.NormalizedRowJson);
            var citationKey = values.GetValueOrDefault("citation_key") ?? string.Empty;
            var programKey = values.GetValueOrDefault("program_key") ?? string.Empty;
            var program = await db.RegulatoryPrograms.FirstOrDefaultAsync(program => program.TenantId == tenantId && program.ProgramKey == programKey, cancellationToken);
            if (program is null)
            {
                continue;
            }

            var packKey = values.GetValueOrDefault("pack_key") ?? string.Empty;
            var packVersion = ParseInt(values.GetValueOrDefault("pack_version"), 1);
            var pack = string.IsNullOrWhiteSpace(packKey)
                ? null
                : await db.RulePacks.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.PackKey == packKey && item.VersionNumber == packVersion, cancellationToken);
            var existing = await db.RegulatoryCitations.FirstOrDefaultAsync(citation => citation.TenantId == tenantId && citation.CitationKey == citationKey, cancellationToken);
            if (existing is null)
            {
                db.RegulatoryCitations.Add(new RegulatoryCitation
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    RegulatoryProgramId = program.Id,
                    RulePackId = pack?.Id,
                    CitationKey = citationKey,
                    Label = values.GetValueOrDefault("label") ?? citationKey,
                    SourceReference = values.GetValueOrDefault("source_reference") ?? string.Empty,
                    Description = values.GetValueOrDefault("description") ?? string.Empty,
                    VersionNumber = 1,
                    IsActive = ParseBool(values.GetValueOrDefault("active"), true),
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                existing.RegulatoryProgramId = program.Id;
                existing.RulePackId = pack?.Id;
                existing.Label = values.GetValueOrDefault("label") ?? existing.Label;
                existing.SourceReference = values.GetValueOrDefault("source_reference") ?? existing.SourceReference;
                existing.Description = values.GetValueOrDefault("description") ?? existing.Description;
                existing.IsActive = ParseBool(values.GetValueOrDefault("active"), true);
                existing.UpdatedAt = now;
            }
        }
    }

    private async Task<(int Created, int Updated)> UpsertFactRequirementsAsync(
        Guid tenantId,
        Guid importSessionId,
        IReadOnlyDictionary<Guid, ImportStagedMappingDecision> decisionByRow,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var rows = await db.ImportStagedFactRequirements
            .AsNoTracking()
            .Where(row => row.ImportSessionId == importSessionId && row.ValidationStatus == ImportRowValidationStatuses.Valid)
            .ToListAsync(cancellationToken);
        var created = 0;
        var updated = 0;
        foreach (var row in rows)
        {
            if (!decisionByRow.TryGetValue(row.StagedRowId, out var decision) || !IsCommittable(decision))
            {
                continue;
            }

            var values = ToDictionary(row.NormalizedRowJson);
            var factKey = values.GetValueOrDefault("fact_key") ?? string.Empty;
            var requirementKey = values.GetValueOrDefault("requirement_key") ?? string.Empty;
            var fact = await db.FactDefinitions.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.FactKey == factKey, cancellationToken);
            if (fact is null)
            {
                fact = new FactDefinition
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    FactKey = factKey,
                    Label = values.GetValueOrDefault("label") ?? factKey,
                    Description = values.GetValueOrDefault("description") ?? string.Empty,
                    ValueType = values.GetValueOrDefault("value_type") ?? FactValueTypes.String,
                    IsActive = ParseBool(values.GetValueOrDefault("active"), true),
                    CreatedAt = now,
                    UpdatedAt = now
                };
                db.FactDefinitions.Add(fact);
            }
            else
            {
                fact.Label = string.IsNullOrWhiteSpace(fact.Label) ? values.GetValueOrDefault("label") ?? fact.Label : fact.Label;
                fact.Description = string.IsNullOrWhiteSpace(fact.Description) ? values.GetValueOrDefault("description") ?? fact.Description : fact.Description;
                fact.ValueType = values.GetValueOrDefault("value_type") ?? fact.ValueType;
                fact.IsActive = fact.IsActive || ParseBool(values.GetValueOrDefault("active"), true);
                fact.UpdatedAt = now;
            }

            var pack = await FindRulePackAsync(tenantId, values.GetValueOrDefault("pack_key"), values.GetValueOrDefault("pack_version"), cancellationToken);
            var citation = await FindCitationAsync(tenantId, values.GetValueOrDefault("citation_key"), values.GetValueOrDefault("citation_version"), cancellationToken);
            var existing = await db.FactRequirements.FirstOrDefaultAsync(
                item => item.TenantId == tenantId && item.RequirementKey == requirementKey,
                cancellationToken);
            if (existing is null)
            {
                db.FactRequirements.Add(new FactRequirement
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    FactDefinitionId = fact.Id,
                    RulePackId = pack?.Id,
                    CitationId = citation?.Id,
                    RequirementKey = requirementKey,
                    Label = values.GetValueOrDefault("label") ?? requirementKey,
                    Description = values.GetValueOrDefault("description") ?? string.Empty,
                    ApplicabilityKey = values.GetValueOrDefault("applicability_key") ?? string.Empty,
                    SourceProduct = FactRequirementContractRules.NormalizeProducts(values.GetValueOrDefault("source_product") ?? string.Empty),
                    SourceEntity = values.GetValueOrDefault("source_entity") ?? string.Empty,
                    SourceFieldOrRecordType = values.GetValueOrDefault("source_field_or_record_type") ?? string.Empty,
                    ValueType = values.GetValueOrDefault("value_type") ?? FactValueTypes.String,
                    Operator = values.GetValueOrDefault("operator") ?? FactRequirementOperators.Equal,
                    ExpectedValue = values.GetValueOrDefault("expected_value") ?? string.Empty,
                    EvidenceKind = values.GetValueOrDefault("evidence_kind") ?? FactRequirementEvidenceKinds.ProductRecord,
                    RequiredDocumentType = values.GetValueOrDefault("required_document_type") ?? string.Empty,
                    RetentionPeriod = values.GetValueOrDefault("retention_period") ?? string.Empty,
                    AuditQuestion = values.GetValueOrDefault("audit_question") ?? string.Empty,
                    FailureSeverity = values.GetValueOrDefault("failure_severity") ?? FactRequirementFailureSeverities.Major,
                    AutomaticFailureFlag = ParseBool(values.GetValueOrDefault("automatic_failure_flag")),
                    OverrideAllowed = ParseBool(values.GetValueOrDefault("override_allowed"), true),
                    OverridePermission = values.GetValueOrDefault("override_permission") ?? string.Empty,
                    RemediationRequired = ParseBool(values.GetValueOrDefault("remediation_required"), true),
                    ExternallyAssertable = false,
                    IsRequired = ParseBool(values.GetValueOrDefault("is_required"), true),
                    IsActive = ParseBool(values.GetValueOrDefault("active"), true),
                    CreatedAt = now,
                    UpdatedAt = now
                });
                created++;
            }
            else
            {
                existing.FactDefinitionId = fact.Id;
                existing.RulePackId = pack?.Id;
                existing.CitationId = citation?.Id;
                existing.Label = values.GetValueOrDefault("label") ?? existing.Label;
                existing.Description = values.GetValueOrDefault("description") ?? existing.Description;
                existing.ApplicabilityKey = values.GetValueOrDefault("applicability_key") ?? existing.ApplicabilityKey;
                existing.SourceProduct = FactRequirementContractRules.NormalizeProducts(values.GetValueOrDefault("source_product") ?? existing.SourceProduct);
                existing.SourceEntity = values.GetValueOrDefault("source_entity") ?? existing.SourceEntity;
                existing.SourceFieldOrRecordType = values.GetValueOrDefault("source_field_or_record_type") ?? existing.SourceFieldOrRecordType;
                existing.ValueType = values.GetValueOrDefault("value_type") ?? existing.ValueType;
                existing.Operator = values.GetValueOrDefault("operator") ?? existing.Operator;
                existing.ExpectedValue = values.GetValueOrDefault("expected_value") ?? existing.ExpectedValue;
                existing.EvidenceKind = values.GetValueOrDefault("evidence_kind") ?? existing.EvidenceKind;
                existing.RequiredDocumentType = values.GetValueOrDefault("required_document_type") ?? existing.RequiredDocumentType;
                existing.RetentionPeriod = values.GetValueOrDefault("retention_period") ?? existing.RetentionPeriod;
                existing.AuditQuestion = values.GetValueOrDefault("audit_question") ?? existing.AuditQuestion;
                existing.FailureSeverity = values.GetValueOrDefault("failure_severity") ?? existing.FailureSeverity;
                existing.AutomaticFailureFlag = ParseBool(values.GetValueOrDefault("automatic_failure_flag"));
                existing.OverrideAllowed = ParseBool(values.GetValueOrDefault("override_allowed"), true);
                existing.OverridePermission = values.GetValueOrDefault("override_permission") ?? existing.OverridePermission;
                existing.RemediationRequired = ParseBool(values.GetValueOrDefault("remediation_required"), true);
                existing.IsRequired = ParseBool(values.GetValueOrDefault("is_required"), true);
                existing.IsActive = ParseBool(values.GetValueOrDefault("active"), true);
                existing.UpdatedAt = now;
                updated++;
            }
        }

        return (created, updated);
    }

    private async Task UpsertRegulatoryMappingsAsync(Guid tenantId, Guid importSessionId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var rows = await db.ImportStagedRegulatoryMappings
            .AsNoTracking()
            .Where(row => row.ImportSessionId == importSessionId && row.ValidationStatus == ImportRowValidationStatuses.Valid)
            .ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            var values = ToDictionary(row.NormalizedRowJson);
            var mappingKey = values.GetValueOrDefault("mapping_key") ?? string.Empty;
            var program = await db.RegulatoryPrograms.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.ProgramKey == values.GetValueOrDefault("program_key"), cancellationToken);
            if (program is null)
            {
                continue;
            }

            var pack = await FindRulePackAsync(tenantId, values.GetValueOrDefault("pack_key"), values.GetValueOrDefault("pack_version"), cancellationToken);
            var citation = await FindCitationAsync(tenantId, values.GetValueOrDefault("citation_key"), null, cancellationToken);
            var fact = await db.FactDefinitions.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.FactKey == values.GetValueOrDefault("fact_key"), cancellationToken);
            var complianceKey = await db.ComplianceKeys.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Key == values.GetValueOrDefault("compliance_key"), cancellationToken);
            var materialKey = await db.MaterialKeys.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Key == values.GetValueOrDefault("material_key"), cancellationToken);
            var existing = await db.RegulatoryMappings.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.MappingKey == mappingKey, cancellationToken);
            if (existing is null)
            {
                db.RegulatoryMappings.Add(new RegulatoryMapping
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    MappingKey = mappingKey,
                    Label = values.GetValueOrDefault("label") ?? mappingKey,
                    Description = values.GetValueOrDefault("description") ?? string.Empty,
                    TargetKind = values.GetValueOrDefault("target_kind") ?? string.Empty,
                    RegulatoryProgramId = program.Id,
                    RulePackId = pack?.Id,
                    CitationId = citation?.Id,
                    FactDefinitionId = fact?.Id,
                    ComplianceKeyId = complianceKey?.Id,
                    MaterialKeyId = materialKey?.Id,
                    IsActive = ParseBool(values.GetValueOrDefault("active"), true),
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                existing.Label = values.GetValueOrDefault("label") ?? existing.Label;
                existing.Description = values.GetValueOrDefault("description") ?? existing.Description;
                existing.TargetKind = values.GetValueOrDefault("target_kind") ?? existing.TargetKind;
                existing.RegulatoryProgramId = program.Id;
                existing.RulePackId = pack?.Id;
                existing.CitationId = citation?.Id;
                existing.FactDefinitionId = fact?.Id;
                existing.ComplianceKeyId = complianceKey?.Id;
                existing.MaterialKeyId = materialKey?.Id;
                existing.IsActive = ParseBool(values.GetValueOrDefault("active"), true);
                existing.UpdatedAt = now;
            }
        }
    }

    private async Task UpsertSdsReferencesAsync(Guid tenantId, Guid importSessionId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var rows = await db.ImportStagedSdsReferences
            .AsNoTracking()
            .Where(row => row.ImportSessionId == importSessionId && row.ValidationStatus == ImportRowValidationStatuses.Valid)
            .ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            var values = ToDictionary(row.NormalizedRowJson);
            var sdsKey = values.GetValueOrDefault("sds_key") ?? string.Empty;
            var material = await db.MaterialKeys.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Key == values.GetValueOrDefault("material_key"), cancellationToken);
            var existing = await db.SdsReferences.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.SdsKey == sdsKey, cancellationToken);
            if (existing is null)
            {
                db.SdsReferences.Add(new SdsReference
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    SdsKey = sdsKey,
                    MaterialKeyId = material?.Id,
                    ProductName = values.GetValueOrDefault("product_name") ?? string.Empty,
                    Manufacturer = values.GetValueOrDefault("manufacturer") ?? string.Empty,
                    DocumentUrl = values.GetValueOrDefault("document_url") ?? string.Empty,
                    RevisionDate = ParseDate(values.GetValueOrDefault("revision_date")),
                    IsActive = ParseBool(values.GetValueOrDefault("active"), true),
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                existing.MaterialKeyId = material?.Id;
                existing.ProductName = values.GetValueOrDefault("product_name") ?? existing.ProductName;
                existing.Manufacturer = values.GetValueOrDefault("manufacturer") ?? existing.Manufacturer;
                existing.DocumentUrl = values.GetValueOrDefault("document_url") ?? existing.DocumentUrl;
                existing.RevisionDate = ParseDate(values.GetValueOrDefault("revision_date"));
                existing.IsActive = ParseBool(values.GetValueOrDefault("active"), true);
                existing.UpdatedAt = now;
            }
        }
    }

    private async Task<(int Created, int Updated)> UpsertExceptionExemptionsAsync(
        Guid tenantId,
        Guid importSessionId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var rows = await db.ImportStagedExceptionExemptions
            .AsNoTracking()
            .Where(row => row.ImportSessionId == importSessionId && row.ValidationStatus == ImportRowValidationStatuses.Valid)
            .ToListAsync(cancellationToken);
        var created = 0;
        var updated = 0;
        foreach (var row in rows)
        {
            var values = ToDictionary(row.NormalizedRowJson);
            var key = NormalizeKey(values.GetValueOrDefault("key"));
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            var evidenceGroup = await FindEvidenceOptionGroupAsync(
                tenantId,
                values.GetValueOrDefault("required_evidence_option_group_key"),
                cancellationToken);
            var existing = await db.ComplianceExceptionExemptions.FirstOrDefaultAsync(
                item => item.TenantId == tenantId && item.Key == key,
                cancellationToken);
            if (existing is null)
            {
                db.ComplianceExceptionExemptions.Add(new ComplianceExceptionExemption
                {
                    ExceptionExemptionId = Guid.NewGuid(),
                    TenantId = tenantId,
                    Key = key,
                    Label = values.GetValueOrDefault("label") ?? key,
                    Type = (values.GetValueOrDefault("type") ?? ComplianceExceptionExemptionTypes.RegulatoryException).ToLowerInvariant(),
                    GoverningBody = values.GetValueOrDefault("governing_body") ?? string.Empty,
                    ProgramKey = values.GetValueOrDefault("program_key") ?? string.Empty,
                    PackKey = values.GetValueOrDefault("pack_key") ?? string.Empty,
                    CitationKey = values.GetValueOrDefault("citation_key") ?? string.Empty,
                    ApplicabilityKey = values.GetValueOrDefault("applicability_key") ?? string.Empty,
                    AppliesToSubjectKind = values.GetValueOrDefault("applies_to_subject_kind") ?? string.Empty,
                    AppliesToSourceProduct = values.GetValueOrDefault("applies_to_source_product") ?? string.Empty,
                    AppliesToSourceEntity = values.GetValueOrDefault("applies_to_source_entity") ?? string.Empty,
                    EffectType = (values.GetValueOrDefault("effect_type") ?? ComplianceExceptionExemptionEffectTypes.MakesRequirementNotApplicable).ToLowerInvariant(),
                    ConditionLogicJson = string.IsNullOrWhiteSpace(values.GetValueOrDefault("condition_logic_json"))
                        ? "{}"
                        : values.GetValueOrDefault("condition_logic_json")!,
                    RequiredEvidenceOptionGroupId = evidenceGroup?.EvidenceOptionGroupId,
                    IssuingAuthority = values.GetValueOrDefault("issuing_authority") ?? string.Empty,
                    AuthorizationNumber = values.GetValueOrDefault("authorization_number") ?? string.Empty,
                    EffectiveAt = ParseDateTimeOffset(values.GetValueOrDefault("effective_at")),
                    ExpiresAt = ParseDateTimeOffset(values.GetValueOrDefault("expires_at")),
                    Active = ParseBool(values.GetValueOrDefault("active"), true),
                    Description = values.GetValueOrDefault("description") ?? string.Empty,
                    CreatedAt = now,
                    UpdatedAt = now
                });
                created++;
            }
            else
            {
                existing.Label = values.GetValueOrDefault("label") ?? existing.Label;
                existing.Type = (values.GetValueOrDefault("type") ?? existing.Type).ToLowerInvariant();
                existing.GoverningBody = values.GetValueOrDefault("governing_body") ?? existing.GoverningBody;
                existing.ProgramKey = values.GetValueOrDefault("program_key") ?? existing.ProgramKey;
                existing.PackKey = values.GetValueOrDefault("pack_key") ?? existing.PackKey;
                existing.CitationKey = values.GetValueOrDefault("citation_key") ?? existing.CitationKey;
                existing.ApplicabilityKey = values.GetValueOrDefault("applicability_key") ?? existing.ApplicabilityKey;
                existing.AppliesToSubjectKind = values.GetValueOrDefault("applies_to_subject_kind") ?? existing.AppliesToSubjectKind;
                existing.AppliesToSourceProduct = values.GetValueOrDefault("applies_to_source_product") ?? existing.AppliesToSourceProduct;
                existing.AppliesToSourceEntity = values.GetValueOrDefault("applies_to_source_entity") ?? existing.AppliesToSourceEntity;
                existing.EffectType = (values.GetValueOrDefault("effect_type") ?? existing.EffectType).ToLowerInvariant();
                existing.ConditionLogicJson = string.IsNullOrWhiteSpace(values.GetValueOrDefault("condition_logic_json"))
                    ? existing.ConditionLogicJson
                    : values.GetValueOrDefault("condition_logic_json")!;
                existing.RequiredEvidenceOptionGroupId = evidenceGroup?.EvidenceOptionGroupId ?? existing.RequiredEvidenceOptionGroupId;
                existing.IssuingAuthority = values.GetValueOrDefault("issuing_authority") ?? existing.IssuingAuthority;
                existing.AuthorizationNumber = values.GetValueOrDefault("authorization_number") ?? existing.AuthorizationNumber;
                existing.EffectiveAt = ParseDateTimeOffset(values.GetValueOrDefault("effective_at")) ?? existing.EffectiveAt;
                existing.ExpiresAt = ParseDateTimeOffset(values.GetValueOrDefault("expires_at")) ?? existing.ExpiresAt;
                existing.Active = ParseBool(values.GetValueOrDefault("active"), true);
                existing.Description = values.GetValueOrDefault("description") ?? existing.Description;
                existing.UpdatedAt = now;
                updated++;
            }
        }

        return (created, updated);
    }

    private async Task<ComplianceEvidenceOptionGroup?> FindEvidenceOptionGroupAsync(
        Guid tenantId,
        string? key,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return await db.ComplianceEvidenceOptionGroups.FirstOrDefaultAsync(
            group => group.TenantId == tenantId &&
                     (group.RequirementKey == key || group.FactKey == key || group.EvidenceOptionGroupId.ToString() == key),
            cancellationToken);
    }

    private async Task UpsertEvidenceOptionsAsync(
        Guid tenantId,
        Guid importSessionId,
        IReadOnlyDictionary<string, string> values,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var requirementKey = values.GetValueOrDefault("requirement_key") ?? string.Empty;
        var factKey = values.GetValueOrDefault("fact_key") ?? string.Empty;
        var group = await db.ComplianceEvidenceOptionGroups.FirstOrDefaultAsync(
            item => item.TenantId == tenantId && item.RequirementKey == requirementKey && item.FactKey == factKey,
            cancellationToken);
        var options = BuildEvidenceOptions(importSessionId, values).ToList();
        var first = options.First();
        if (group is null)
        {
            group = new ComplianceEvidenceOptionGroup
            {
                EvidenceOptionGroupId = DeterministicGuid($"{tenantId}:{requirementKey}:{factKey}:group"),
                TenantId = tenantId,
                RequirementKey = requirementKey,
                FactKey = factKey,
                PackKey = values.GetValueOrDefault("pack_key") ?? string.Empty,
                CitationKey = values.GetValueOrDefault("citation_key") ?? string.Empty,
                LogicType = first.LogicType,
                ApplicabilityKey = values.GetValueOrDefault("applicability_key") ?? string.Empty,
                Label = values.GetValueOrDefault("label") ?? requirementKey,
                Description = values.GetValueOrDefault("description") ?? string.Empty,
                Active = ParseBool(values.GetValueOrDefault("active"), true),
                CreatedAt = now,
                UpdatedAt = now
            };
            db.ComplianceEvidenceOptionGroups.Add(group);
        }
        else
        {
            group.PackKey = values.GetValueOrDefault("pack_key") ?? group.PackKey;
            group.CitationKey = values.GetValueOrDefault("citation_key") ?? group.CitationKey;
            group.LogicType = first.LogicType;
            group.ApplicabilityKey = values.GetValueOrDefault("applicability_key") ?? group.ApplicabilityKey;
            group.Label = values.GetValueOrDefault("label") ?? group.Label;
            group.Description = values.GetValueOrDefault("description") ?? group.Description;
            group.Active = ParseBool(values.GetValueOrDefault("active"), true);
            group.UpdatedAt = now;
        }

        foreach (var option in options)
        {
            var existing = await db.ComplianceEvidenceOptions.FirstOrDefaultAsync(
                item => item.TenantId == tenantId && item.OptionKey == option.OptionKey,
                cancellationToken);
            if (existing is null)
            {
                db.ComplianceEvidenceOptions.Add(new ComplianceEvidenceOption
                {
                    EvidenceOptionId = option.EvidenceOptionId,
                    TenantId = tenantId,
                    EvidenceOptionGroupId = group.EvidenceOptionGroupId,
                    OptionKey = option.OptionKey,
                    OptionLabel = option.OptionLabel,
                    EvidenceKind = option.EvidenceKind,
                    TargetKind = option.TargetKind,
                    SourceProduct = option.SourceProduct,
                    SourceEntity = option.SourceEntity,
                    SourceFieldOrRecordType = option.SourceFieldOrRecordType,
                    DocumentTypeKey = option.DocumentTypeKey,
                    MaterialKey = option.MaterialKey,
                    PartKey = option.PartKey,
                    SystemKey = option.SystemKey,
                    AssetKind = option.AssetKind,
                    ExternalRegistryKey = option.ExternalRegistryKey,
                    FactKey = option.FactKey,
                    Required = option.Required,
                    Priority = option.Priority,
                    ConfidenceHint = option.ConfidenceHint,
                    Active = true,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                existing.EvidenceOptionGroupId = group.EvidenceOptionGroupId;
                existing.OptionLabel = option.OptionLabel;
                existing.EvidenceKind = option.EvidenceKind;
                existing.TargetKind = option.TargetKind;
                existing.SourceProduct = option.SourceProduct;
                existing.SourceEntity = option.SourceEntity;
                existing.SourceFieldOrRecordType = option.SourceFieldOrRecordType;
                existing.DocumentTypeKey = option.DocumentTypeKey;
                existing.MaterialKey = option.MaterialKey;
                existing.PartKey = option.PartKey;
                existing.SystemKey = option.SystemKey;
                existing.AssetKind = option.AssetKind;
                existing.ExternalRegistryKey = option.ExternalRegistryKey;
                existing.FactKey = option.FactKey;
                existing.Required = option.Required;
                existing.Priority = option.Priority;
                existing.ConfidenceHint = option.ConfidenceHint;
                existing.Active = true;
                existing.UpdatedAt = now;
            }
        }
    }

    private async Task CreateEvidenceReferenceAndObjectMirrorAsync(
        Guid tenantId,
        Guid importSessionId,
        ImportStagedFactRequirement row,
        IReadOnlyDictionary<string, string> values,
        ImportStagedMappingDecision decision,
        ImportStagedMappingCandidate candidate,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (decision.SelectedTargetKind == MappingTargetKinds.NoDocumentRequired ||
            decision.Decision is ImportMappingDecisions.NotApplicable or ImportMappingDecisions.ReferenceOnly)
        {
            return;
        }

        var targetKey = string.IsNullOrWhiteSpace(decision.SelectedTargetKey) ? candidate.TargetKey : decision.SelectedTargetKey;
        var evidenceId = $"import:{importSessionId:N}:{row.StagedRowId:N}:{NormalizeKey(targetKey)}";
        var purposeNote = BuildEvidencePurposeNote(decision);
        var existingEvidence = await db.EvidenceReferences.FirstOrDefaultAsync(
            evidence => evidence.TenantId == tenantId && evidence.EvidenceId == evidenceId,
            cancellationToken);
        if (existingEvidence is null)
        {
            db.EvidenceReferences.Add(new EvidenceReference
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EvidenceId = evidenceId,
                FactKey = values.GetValueOrDefault("fact_key") ?? string.Empty,
                SourceProduct = values.GetValueOrDefault("source_product") ?? string.Empty,
                SourceEntity = values.GetValueOrDefault("source_entity") ?? string.Empty,
                SourceRecordId = string.IsNullOrWhiteSpace(decision.SelectedTargetId) ? targetKey : decision.SelectedTargetId,
                SourceField = values.GetValueOrDefault("source_field_or_record_type") ?? string.Empty,
                DocumentType = values.GetValueOrDefault("required_document_type") ?? string.Empty,
                DocumentUrl = string.Empty,
                StorageKey = string.Empty,
                FileHash = row.RowHash,
                CapturedAt = now,
                EffectiveAt = now,
                CreatedByPersonId = decision.DecidedByPersonId,
                ReviewedByPersonId = decision.DecidedByPersonId,
                ReviewStatus = "confirmed",
                Notes = $"Created from import session {importSessionId}. {purposeNote}"
            });
        }

        await UpsertReferenceMirrorAsync(tenantId, decision, candidate, values, now, cancellationToken);
    }

    private async Task UpsertExceptionExemptionFromDecisionAsync(
        Guid tenantId,
        IReadOnlyDictionary<string, string> values,
        ImportStagedMappingDecision decision,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (decision.Decision != ImportMappingDecisions.CreateNewExceptionExemptionRecord ||
            string.IsNullOrWhiteSpace(decision.ExceptionExemptionKey))
        {
            return;
        }

        var payload = ToDictionary(decision.CreateNewPayloadJson);
        var key = NormalizeKey(decision.ExceptionExemptionKey);
        var existing = await db.ComplianceExceptionExemptions.FirstOrDefaultAsync(
            item => item.TenantId == tenantId && item.Key == key,
            cancellationToken);
        var type = NormalizeExceptionType(payload.GetValueOrDefault("type"), decision.SelectedTargetKind);
        var effectType = NormalizeExceptionEffect(payload.GetValueOrDefault("effectType"));
        if (existing is null)
        {
            db.ComplianceExceptionExemptions.Add(new ComplianceExceptionExemption
            {
                ExceptionExemptionId = Guid.NewGuid(),
                TenantId = tenantId,
                Key = key,
                Label = payload.GetValueOrDefault("label") ?? decision.SelectedTargetKey,
                Type = type,
                GoverningBody = payload.GetValueOrDefault("governingBody") ?? string.Empty,
                ProgramKey = values.GetValueOrDefault("program_key") ?? string.Empty,
                PackKey = values.GetValueOrDefault("pack_key") ?? string.Empty,
                CitationKey = values.GetValueOrDefault("citation_key") ?? string.Empty,
                ApplicabilityKey = values.GetValueOrDefault("applicability_key") ?? string.Empty,
                AppliesToSubjectKind = values.GetValueOrDefault("source_entity") ?? string.Empty,
                AppliesToSourceProduct = values.GetValueOrDefault("source_product") ?? string.Empty,
                AppliesToSourceEntity = values.GetValueOrDefault("source_entity") ?? string.Empty,
                EffectType = effectType,
                ConditionLogicJson = "{}",
                IssuingAuthority = payload.GetValueOrDefault("issuingAuthority") ?? string.Empty,
                AuthorizationNumber = payload.GetValueOrDefault("authorizationNumber") ?? string.Empty,
                EffectiveAt = ParseDateTimeOffset(payload.GetValueOrDefault("effectiveAt")),
                ExpiresAt = ParseDateTimeOffset(payload.GetValueOrDefault("expiresAt")),
                Active = true,
                Description = payload.GetValueOrDefault("description") ?? $"Created from import mapping decision {decision.MappingDecisionId}.",
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            existing.Label = payload.GetValueOrDefault("label") ?? existing.Label;
            existing.Type = type;
            existing.EffectType = effectType;
            existing.PackKey = values.GetValueOrDefault("pack_key") ?? existing.PackKey;
            existing.CitationKey = values.GetValueOrDefault("citation_key") ?? existing.CitationKey;
            existing.ApplicabilityKey = values.GetValueOrDefault("applicability_key") ?? existing.ApplicabilityKey;
            existing.AppliesToSourceProduct = values.GetValueOrDefault("source_product") ?? existing.AppliesToSourceProduct;
            existing.AppliesToSourceEntity = values.GetValueOrDefault("source_entity") ?? existing.AppliesToSourceEntity;
            existing.UpdatedAt = now;
        }
    }

    private static string BuildEvidencePurposeNote(ImportStagedMappingDecision decision)
    {
        var basePurpose = decision.EvidenceMappingPurpose switch
        {
            ImportEvidenceMappingPurposes.ExceptionProof => "Evidence proves a regulatory exception.",
            ImportEvidenceMappingPurposes.ExemptionProof => "Evidence proves an exemption, waiver, or variance.",
            ImportEvidenceMappingPurposes.WaiverVarianceSpecialPermitProof => "Evidence proves a waiver, variance, special permit, or approval.",
            ImportEvidenceMappingPurposes.AlternateEvidencePath => "Evidence satisfies an alternate evidence path.",
            ImportEvidenceMappingPurposes.ChangesApplicability => "Evidence changes applicability through legal relief.",
            ImportEvidenceMappingPurposes.ChangesRequiredEvidence => "Evidence changes required evidence through legal relief.",
            ImportEvidenceMappingPurposes.ChangesExpectedValue => "Evidence changes expected value through legal relief.",
            _ => "Evidence satisfies the normal requirement."
        };
        return string.IsNullOrWhiteSpace(decision.ExceptionExemptionKey)
            ? basePurpose
            : $"{basePurpose} exception_exemption_key={decision.ExceptionExemptionKey}.";
    }

    private static string NormalizeExceptionType(string? type, string targetKind)
    {
        if (!string.IsNullOrWhiteSpace(type) && ComplianceExceptionExemptionTypes.All.Contains(type))
        {
            return type.Trim().ToLowerInvariant();
        }

        return targetKind switch
        {
            MappingTargetKinds.Waiver => ComplianceExceptionExemptionTypes.Waiver,
            MappingTargetKinds.Variance => ComplianceExceptionExemptionTypes.Variance,
            MappingTargetKinds.SpecialPermit => ComplianceExceptionExemptionTypes.SpecialPermit,
            MappingTargetKinds.Approval => ComplianceExceptionExemptionTypes.Approval,
            MappingTargetKinds.AlternateCompliancePath => ComplianceExceptionExemptionTypes.AlternateCompliancePath,
            MappingTargetKinds.ConditionalExclusion => ComplianceExceptionExemptionTypes.ConditionalExclusion,
            _ => ComplianceExceptionExemptionTypes.RegulatoryExemption
        };
    }

    private static string NormalizeExceptionEffect(string? effectType) =>
        !string.IsNullOrWhiteSpace(effectType) && ComplianceExceptionExemptionEffectTypes.All.Contains(effectType)
            ? effectType.Trim().ToLowerInvariant()
            : ComplianceExceptionExemptionEffectTypes.AllowsAlternateEvidence;

    private static string ExceptionTypeFromDecision(ImportStagedMappingDecision decision) =>
        NormalizeExceptionType(string.Empty, decision.SelectedTargetKind);

    private async Task UpsertReferenceMirrorAsync(
        Guid tenantId,
        ImportStagedMappingDecision decision,
        ImportStagedMappingCandidate candidate,
        IReadOnlyDictionary<string, string> values,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var targetKind = string.IsNullOrWhiteSpace(decision.SelectedTargetKind) ? candidate.TargetKind : decision.SelectedTargetKind;
        var targetKey = string.IsNullOrWhiteSpace(decision.SelectedTargetKey) ? candidate.TargetKey : decision.SelectedTargetKey;
        var targetId = string.IsNullOrWhiteSpace(decision.SelectedTargetId) ? candidate.TargetId : decision.SelectedTargetId;
        var sourceProduct = values.GetValueOrDefault("source_product") ?? string.Empty;
        var label = !string.IsNullOrWhiteSpace(candidate.TargetLabel)
            ? candidate.TargetLabel
            : values.GetValueOrDefault("label") ?? targetKey;

        switch (targetKind)
        {
            case MappingTargetKinds.ExistingDocumentRecord:
            case MappingTargetKinds.NewDocumentRecord:
            case MappingTargetKinds.ExistingDocumentType:
            case MappingTargetKinds.NewDocumentType:
                await UpsertReferenceAsync(db.DocumentReferences, tenantId, sourceProduct, "document", targetId, targetKey, label, now, cancellationToken);
                break;
            case MappingTargetKinds.ExistingMaterial:
            case MappingTargetKinds.NewMaterial:
                await UpsertReferenceAsync(db.MaterialReferences, tenantId, sourceProduct, "material", targetId, targetKey, label, now, cancellationToken);
                break;
            case MappingTargetKinds.ExistingPart:
            case MappingTargetKinds.NewPart:
                await UpsertReferenceAsync(db.PartReferences, tenantId, sourceProduct, "part", targetId, targetKey, label, now, cancellationToken);
                break;
            case MappingTargetKinds.ExistingSystem:
            case MappingTargetKinds.NewSystem:
                await UpsertReferenceAsync(db.SystemReferences, tenantId, sourceProduct, "system", targetId, targetKey, label, now, cancellationToken);
                break;
            case MappingTargetKinds.ExistingAsset:
            case MappingTargetKinds.NewAsset:
                await UpsertReferenceAsync(db.AssetReferences, tenantId, sourceProduct, "asset", targetId, targetKey, label, now, cancellationToken);
                break;
            default:
                await UpsertReferenceAsync(db.ExternalObjectReferences, tenantId, sourceProduct, targetKind, targetId, targetKey, label, now, cancellationToken);
                break;
        }
    }

    private static async Task UpsertReferenceAsync<TEntity>(
        DbSet<TEntity> set,
        Guid tenantId,
        string sourceProduct,
        string objectKind,
        string externalRecordId,
        string stableKey,
        string label,
        DateTimeOffset now,
        CancellationToken cancellationToken)
        where TEntity : ProductObjectReferenceBase, new()
    {
        if (string.IsNullOrWhiteSpace(stableKey))
        {
            stableKey = externalRecordId;
        }

        var existing = await set.FirstOrDefaultAsync(
            reference => reference.TenantId == tenantId && reference.StableKey == stableKey,
            cancellationToken);
        if (existing is null)
        {
            set.Add(new TEntity
            {
                ReferenceId = Guid.NewGuid(),
                TenantId = tenantId,
                SourceProduct = sourceProduct,
                ObjectKind = objectKind,
                ExternalRecordId = externalRecordId,
                StableKey = stableKey,
                Label = label,
                Description = string.Empty,
                Active = true,
                LastSeenAt = now,
                MetadataJson = "{}",
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            existing.SourceProduct = sourceProduct;
            existing.ObjectKind = objectKind;
            existing.ExternalRecordId = externalRecordId;
            existing.Label = label;
            existing.Active = true;
            existing.LastSeenAt = now;
            existing.UpdatedAt = now;
        }
    }

    private async Task<MappingDecisionResponse> SpecialDecisionAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        string decision,
        Guid actorPersonId,
        string targetKind,
        string targetKey,
        CancellationToken cancellationToken,
        IReadOnlyDictionary<string, string>? payload = null)
    {
        var candidate = await LoadCandidateAsync(tenantId, importSessionId, itemId, cancellationToken);
        if (decision is not ImportMappingDecisions.Skip and not ImportMappingDecisions.Reject)
        {
            await EnsureRowCanBeMappedAsync(importSessionId, candidate.StagedRowId, cancellationToken);
        }

        return await UpsertDecisionAsync(
            tenantId,
            importSessionId,
            candidate,
            decision,
            actorPersonId,
            candidate.EvidenceOptionId,
            candidate.EvidenceOptionKey,
            targetKind,
            targetKey,
            targetKey,
            payload is null ? "{}" : Serialize(payload),
            false,
            string.Empty,
            cancellationToken);
    }

    private async Task<MappingDecisionResponse> SetEvidencePurposeAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        string decision,
        string evidenceMappingPurpose,
        Guid actorPersonId,
        CancellationToken cancellationToken)
    {
        var candidate = await LoadCandidateAsync(tenantId, importSessionId, itemId, cancellationToken);
        await EnsureRowCanBeMappedAsync(importSessionId, candidate.StagedRowId, cancellationToken);
        return await UpsertDecisionAsync(
            tenantId,
            importSessionId,
            candidate,
            decision,
            actorPersonId,
            candidate.EvidenceOptionId,
            candidate.EvidenceOptionKey,
            candidate.TargetKind,
            candidate.TargetId,
            candidate.TargetKey,
            "{}",
            false,
            string.Empty,
            cancellationToken,
            evidenceMappingPurpose: evidenceMappingPurpose);
    }

    private async Task<MappingDecisionResponse> SetExceptionProofDecisionAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        ExceptionProofMappingRequest request,
        string decision,
        string evidenceMappingPurpose,
        Guid actorPersonId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ExceptionExemptionKey))
        {
            throw new StlApiException("import_wizard.exception_key_required", "Exception/exemption proof requires a selected or created exception/exemption key.", 400);
        }

        var candidate = await LoadCandidateAsync(tenantId, importSessionId, itemId, cancellationToken);
        await EnsureRowCanBeMappedAsync(importSessionId, candidate.StagedRowId, cancellationToken);
        var payload = new Dictionary<string, string>(request.Payload ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase)
        {
            ["targetLabel"] = request.TargetLabel,
            ["exceptionExemptionKey"] = request.ExceptionExemptionKey.Trim()
        };

        return await UpsertDecisionAsync(
            tenantId,
            importSessionId,
            candidate,
            decision,
            actorPersonId,
            candidate.EvidenceOptionId,
            candidate.EvidenceOptionKey,
            request.TargetKind,
            request.TargetKey,
            request.TargetKey,
            Serialize(payload),
            false,
            string.Empty,
            cancellationToken,
            evidenceMappingPurpose: evidenceMappingPurpose,
            exceptionExemptionKey: request.ExceptionExemptionKey.Trim(),
            residualRequirements: request.ResidualRequirements ?? []);
    }

    private async Task<MappingDecisionResponse> UpsertDecisionAsync(
        Guid tenantId,
        Guid importSessionId,
        ImportStagedMappingCandidate candidate,
        string decision,
        Guid actorPersonId,
        Guid? evidenceOptionId,
        string evidenceOptionKey,
        string targetKind,
        string targetId,
        string targetKey,
        string createNewPayloadJson,
        bool overrideUsed,
        string overrideReason,
        CancellationToken cancellationToken,
        bool save = true,
        string evidenceMappingPurpose = ImportEvidenceMappingPurposes.NormalRequirement,
        string exceptionExemptionKey = "",
        IReadOnlyList<string>? residualRequirements = null)
    {
        var existing = await db.ImportStagedMappingDecisions.FirstOrDefaultAsync(
            item => item.ImportSessionId == importSessionId && item.StagedRowId == candidate.StagedRowId,
            cancellationToken);
        var now = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            existing = new ImportStagedMappingDecision
            {
                MappingDecisionId = Guid.NewGuid(),
                TenantId = tenantId,
                ImportSessionId = importSessionId,
                StagedRowId = candidate.StagedRowId
            };
            db.ImportStagedMappingDecisions.Add(existing);
        }

        existing.MappingCandidateId = candidate.MappingCandidateId;
        existing.Decision = decision;
        existing.SelectedEvidenceOptionId = evidenceOptionId;
        existing.SelectedEvidenceOptionKey = evidenceOptionKey;
        existing.SelectedTargetKind = targetKind;
        existing.SelectedTargetId = targetId;
        existing.SelectedTargetKey = targetKey;
        existing.CreateNewPayloadJson = createNewPayloadJson;
        existing.EvidenceMappingPurpose = evidenceMappingPurpose;
        existing.ExceptionExemptionKey = exceptionExemptionKey;
        existing.ResidualRequirementsJson = Serialize(residualRequirements ?? []);
        existing.OverrideUsed = overrideUsed;
        existing.OverrideReason = overrideReason;
        existing.DecidedByPersonId = actorPersonId;
        existing.DecidedAt = now;

        if (save)
        {
            await UpdateMappingStatusAsync(importSessionId, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        return ToResponse(existing);
    }

    private async Task UpdateMappingStatusAsync(Guid importSessionId, CancellationToken cancellationToken)
    {
        var session = await db.ImportSessions.FirstAsync(session => session.ImportSessionId == importSessionId, cancellationToken);
        var requiredRowCount = await db.ImportStagedFactRequirements.CountAsync(
            row => row.ImportSessionId == importSessionId && row.ValidationStatus == ImportRowValidationStatuses.Valid,
            cancellationToken);
        var decisionCount = await db.ImportStagedMappingDecisions.CountAsync(
            decision => decision.ImportSessionId == importSessionId,
            cancellationToken);
        if (requiredRowCount == 0)
        {
            session.MappingStatus = ImportSessionMappingStatuses.NotStarted;
            return;
        }

        session.MappingStatus = decisionCount >= requiredRowCount
            ? ImportSessionMappingStatuses.Confirmed
            : ImportSessionMappingStatuses.PartiallyConfirmed;
        session.Status = session.MappingStatus == ImportSessionMappingStatuses.Confirmed
            ? ImportSessionStatuses.MappingConfirmed
            : ImportSessionStatuses.PartiallyConfirmed;
    }

    private async Task<WizardItemResponse> BuildWizardItemAsync(
        Guid tenantId,
        Guid importSessionId,
        ImportStagedMappingCandidate candidate,
        CancellationToken cancellationToken)
    {
        var row = await db.ImportStagedFactRequirements
            .AsNoTracking()
            .FirstOrDefaultAsync(row => row.ImportSessionId == importSessionId && row.StagedRowId == candidate.StagedRowId, cancellationToken)
            ?? throw new StlApiException("import_wizard.item_not_found", "Wizard item source row was not found.", 404);
        var values = ToDictionary(row.NormalizedRowJson);
        var options = await db.ImportStagedMappingCandidates
            .AsNoTracking()
            .Where(item => item.ImportSessionId == importSessionId && item.StagedRowId == row.StagedRowId)
            .OrderByDescending(item => item.ConfidenceScore)
            .ToListAsync(cancellationToken);
        var selected = options.FirstOrDefault(item => item.MappingCandidateId == candidate.MappingCandidateId)
            ?? options.First();
        var decision = await db.ImportStagedMappingDecisions
            .AsNoTracking()
            .Where(item => item.ImportSessionId == importSessionId && item.StagedRowId == row.StagedRowId)
            .OrderByDescending(item => item.DecidedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var optionResponses = options
            .GroupBy(item => item.EvidenceOptionKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => ToEvidenceOptionResponse(group.OrderByDescending(item => item.ConfidenceScore).First(), values))
            .ToList();
        var suggested = ToEvidenceOptionResponse(selected, values);
        var prompt = optionResponses.Count > 1
            ? "Here is the requirement. Compliance Core found several acceptable ways to satisfy it. Which evidence path should this use, and is it normal evidence or exception/exemption proof?"
            : "Here is the requirement. Compliance Core found the suggested evidence path and target. Is this normal evidence, or proof of an exception/exemption/alternate compliance path?";

        return new WizardItemResponse(
            selected.MappingCandidateId,
            row.StagedRowId,
            decision?.Decision ?? "pending",
            values.GetValueOrDefault("requirement_key") ?? string.Empty,
            values.GetValueOrDefault("fact_key") ?? string.Empty,
            values.GetValueOrDefault("label") ?? string.Empty,
            values.GetValueOrDefault("audit_question") ?? string.Empty,
            values.GetValueOrDefault("citation_key") ?? string.Empty,
            values.GetValueOrDefault("pack_key") ?? string.Empty,
            values.GetValueOrDefault("applicability_key") ?? string.Empty,
            values.GetValueOrDefault("evidence_kind") ?? string.Empty,
            selected.OptionLogicGroup,
            suggested,
            optionResponses.Where(option => option.EvidenceOptionKey != suggested.EvidenceOptionKey).ToList(),
            values.GetValueOrDefault("source_product") ?? string.Empty,
            values.GetValueOrDefault("source_entity") ?? string.Empty,
            values.GetValueOrDefault("source_field_or_record_type") ?? string.Empty,
            selected.TargetLabel,
            selected.TargetKind,
            selected.ConfidenceScore,
            selected.ConfidenceBand,
            DeserializeList(selected.MatchReasonsJson),
            DeserializeList(selected.RiskFlagsJson),
            prompt,
            BuildConfirmationEffect(selected),
            ParseBool(values.GetValueOrDefault("override_allowed"), true),
            ParseBool(values.GetValueOrDefault("remediation_required"), true),
            "Does this evidence satisfy the normal requirement, or is it proof of an exception/exemption/alternate compliance path?",
            values,
            BuildTargetRecord(selected));
    }

    private static EvidenceOptionProposalResponse ToEvidenceOptionResponse(
        ImportStagedMappingCandidate candidate,
        IReadOnlyDictionary<string, string> row)
    {
        var option = BuildEvidenceOptions(candidate.ImportSessionId, row)
            .FirstOrDefault(option => option.OptionKey == candidate.EvidenceOptionKey);
        if (option is null)
        {
            return new EvidenceOptionProposalResponse(
                candidate.EvidenceOptionId ?? Guid.Empty,
                candidate.EvidenceOptionKey,
                candidate.EvidenceOptionLabel,
                candidate.OptionLogicGroup,
                row.GetValueOrDefault("evidence_kind") ?? string.Empty,
                candidate.TargetKind,
                row.GetValueOrDefault("source_product") ?? string.Empty,
                row.GetValueOrDefault("source_entity") ?? string.Empty,
                row.GetValueOrDefault("source_field_or_record_type") ?? string.Empty,
                row.GetValueOrDefault("required_document_type") ?? string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                row.GetValueOrDefault("fact_key") ?? string.Empty,
                true,
                1,
                null);
        }

        return option.ToResponse();
    }

    private static IReadOnlyDictionary<string, string> BuildTargetRecord(ImportStagedMappingCandidate candidate) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["targetKind"] = candidate.TargetKind,
            ["targetId"] = candidate.TargetId,
            ["targetKey"] = candidate.TargetKey,
            ["targetLabel"] = candidate.TargetLabel,
            ["confidenceBand"] = candidate.ConfidenceBand
        };

    private static string BuildConfirmationEffect(ImportStagedMappingCandidate candidate)
    {
        if (candidate.TargetKind.StartsWith("new_", StringComparison.OrdinalIgnoreCase))
        {
            return $"Compliance Core will create a local reference for {candidate.TargetKey} and attach it to the selected evidence option during final commit.";
        }

        if (candidate.TargetKind.StartsWith("existing_", StringComparison.OrdinalIgnoreCase))
        {
            return $"Compliance Core will map this evidence option to existing target {candidate.TargetKey} during final commit.";
        }

        return $"Compliance Core will record decision {candidate.ProposedAction} for final commit.";
    }

    private static List<CommitPreviewActionResponse> BuildPreviewActions(
        IReadOnlyList<ImportStagedMappingDecision> decisions,
        IReadOnlyList<ImportStagedMappingCandidate> candidates)
    {
        var byId = candidates.ToDictionary(candidate => candidate.MappingCandidateId);
        return decisions
            .OrderBy(decision => decision.DecidedAt)
            .Select(decision =>
            {
                byId.TryGetValue(decision.MappingCandidateId ?? Guid.Empty, out var candidate);
                var sourceKey = candidate?.SourceKey ?? decision.StagedRowId.ToString();
                var targetKind = string.IsNullOrWhiteSpace(decision.SelectedTargetKind)
                    ? candidate?.TargetKind ?? string.Empty
                    : decision.SelectedTargetKind;
                var targetKey = string.IsNullOrWhiteSpace(decision.SelectedTargetKey)
                    ? candidate?.TargetKey ?? string.Empty
                    : decision.SelectedTargetKey;
                return new CommitPreviewActionResponse(
                    decision.Decision,
                    sourceKey,
                    targetKind,
                    targetKey,
                    $"{decision.Decision} {sourceKey} -> {targetKind}:{targetKey}",
                    decision.EvidenceMappingPurpose,
                    decision.ExceptionExemptionKey,
                    DeserializeList(decision.ResidualRequirementsJson),
                    decision.OverrideUsed);
            })
            .ToList();
    }

    private static IReadOnlyList<string> BuildCommitBlockers(
        IReadOnlyList<ImportStagedFactRequirement> factRows,
        IReadOnlyList<ImportStagedMappingDecision> decisions,
        IReadOnlyList<ImportStagedRowBase> allRows)
    {
        var blockers = new List<string>();
        var invalidRows = allRows.Count(row => row.ValidationStatus == ImportRowValidationStatuses.Invalid);
        if (invalidRows > 0)
        {
            blockers.Add($"{invalidRows} staged rows have hard validation failures.");
        }

        var decidedRows = decisions.Select(decision => decision.StagedRowId).ToHashSet();
        var missingDecisionCount = factRows.Count(row => !decidedRows.Contains(row.StagedRowId));
        if (missingDecisionCount > 0)
        {
            blockers.Add($"{missingDecisionCount} required mapping items do not have decisions.");
        }

        return blockers;
    }

    private static bool HasCommittableDecision(Guid stagedRowId, IReadOnlyList<ImportStagedMappingDecision> decisions) =>
        decisions.Any(decision => decision.StagedRowId == stagedRowId && IsCommittable(decision));

    private static bool IsCommittable(ImportStagedMappingDecision decision) =>
        decision.Decision is not ImportMappingDecisions.Skip
            and not ImportMappingDecisions.Reject
            and not ImportMappingDecisions.NotApplicable
            and not ImportMappingDecisions.ReferenceOnly;

    private static bool IsEvidenceReferenceDecision(ImportStagedMappingDecision decision) =>
        IsCommittable(decision) && decision.SelectedTargetKind != MappingTargetKinds.NoDocumentRequired;

    private static bool IsExceptionProofDecision(ImportStagedMappingDecision decision) =>
        IsCommittable(decision) &&
        decision.EvidenceMappingPurpose is ImportEvidenceMappingPurposes.ExceptionProof
            or ImportEvidenceMappingPurposes.ExemptionProof
            or ImportEvidenceMappingPurposes.WaiverVarianceSpecialPermitProof
            or ImportEvidenceMappingPurposes.AlternateEvidencePath
            or ImportEvidenceMappingPurposes.ChangesApplicability
            or ImportEvidenceMappingPurposes.ChangesRequiredEvidence
            or ImportEvidenceMappingPurposes.ChangesExpectedValue;

    private static bool IsExceptionRecordDecision(ImportStagedMappingDecision decision) =>
        decision.Decision == ImportMappingDecisions.CreateNewExceptionExemptionRecord ||
        decision.SelectedTargetKind is MappingTargetKinds.ExceptionExemption
            or MappingTargetKinds.Waiver
            or MappingTargetKinds.Variance
            or MappingTargetKinds.SpecialPermit
            or MappingTargetKinds.Approval
            or MappingTargetKinds.AlternateCompliancePath
            or MappingTargetKinds.ConditionalExclusion;

    private static bool IsConfirmedDecision(ImportStagedMappingDecision decision) =>
        decision.Decision is ImportMappingDecisions.ConfirmCandidate
            or ImportMappingDecisions.NoDocumentRequired
            or ImportMappingDecisions.ForceMap
            or ImportMappingDecisions.MapAsNormalEvidence
            or ImportMappingDecisions.MapAsExceptionProof
            or ImportMappingDecisions.MapAsExemptionProof
            or ImportMappingDecisions.MapAsSpecialPermitApprovalProof
            or ImportMappingDecisions.SelectExistingExceptionExemptionRecord
            or ImportMappingDecisions.MarkExceptionNotApplicable;

    private static bool IsChangedDecision(ImportStagedMappingDecision decision) =>
        decision.Decision is ImportMappingDecisions.SelectEvidenceOption
            or ImportMappingDecisions.SelectExisting
            or ImportMappingDecisions.CreateNew
            or ImportMappingDecisions.CreateNewExceptionExemptionRecord
            or ImportMappingDecisions.AddSupportingEvidence
            or ImportMappingDecisions.Split
            or ImportMappingDecisions.Merge;

    private static int CountTargets(IReadOnlyList<CommitPreviewActionResponse> actions, params string[] targetKinds) =>
        actions.Count(action => targetKinds.Contains(action.TargetKind, StringComparer.OrdinalIgnoreCase));

    private async Task<ImportStagedMappingCandidate> LoadCandidateAsync(
        Guid tenantId,
        Guid importSessionId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        await LoadSessionAsync(tenantId, importSessionId, cancellationToken);
        return await db.ImportStagedMappingCandidates.FirstOrDefaultAsync(
            candidate => candidate.ImportSessionId == importSessionId &&
                         (candidate.MappingCandidateId == itemId || candidate.StagedRowId == itemId),
            cancellationToken)
            ?? throw new StlApiException("import_wizard.item_not_found", "Wizard item was not found.", 404);
    }

    private async Task EnsureRowCanBeMappedAsync(Guid importSessionId, Guid stagedRowId, CancellationToken cancellationToken)
    {
        var row = await db.ImportStagedFactRequirements
            .AsNoTracking()
            .FirstOrDefaultAsync(row => row.ImportSessionId == importSessionId && row.StagedRowId == stagedRowId, cancellationToken)
            ?? throw new StlApiException("import_wizard.row_not_found", "Staged row was not found.", 404);
        if (row.ValidationStatus == ImportRowValidationStatuses.Invalid)
        {
            throw new StlApiException("import_wizard.hard_validation_failed", "Hard validation failures must be fixed before mapping.", 400);
        }
    }

    private async Task<ImportSession> LoadSessionAsync(Guid tenantId, Guid importSessionId, CancellationToken cancellationToken) =>
        await db.ImportSessions.FirstOrDefaultAsync(
            session => session.TenantId == tenantId && session.ImportSessionId == importSessionId,
            cancellationToken)
        ?? throw new StlApiException("import_sessions.not_found", "Import session was not found.", 404);

    private async Task ClearSessionWorkAsync(Guid importSessionId, bool includeSourceFiles, CancellationToken cancellationToken)
    {
        await ClearCandidatesAndDecisionsAsync(importSessionId, cancellationToken);
        db.ImportStagedControlledVocabulary.RemoveRange(await db.ImportStagedControlledVocabulary.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        db.ImportStagedVocabularyAliases.RemoveRange(await db.ImportStagedVocabularyAliases.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        db.ImportStagedComplianceKeys.RemoveRange(await db.ImportStagedComplianceKeys.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        db.ImportStagedMaterialKeys.RemoveRange(await db.ImportStagedMaterialKeys.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        db.ImportStagedRulePacks.RemoveRange(await db.ImportStagedRulePacks.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        db.ImportStagedRuleRequirements.RemoveRange(await db.ImportStagedRuleRequirements.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        db.ImportStagedFactRequirements.RemoveRange(await db.ImportStagedFactRequirements.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        db.ImportStagedRegulatoryMappings.RemoveRange(await db.ImportStagedRegulatoryMappings.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        db.ImportStagedSdsReferences.RemoveRange(await db.ImportStagedSdsReferences.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        db.ImportStagedEvidenceReferences.RemoveRange(await db.ImportStagedEvidenceReferences.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        db.ImportStagedExceptionExemptions.RemoveRange(await db.ImportStagedExceptionExemptions.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        if (includeSourceFiles)
        {
            db.ImportSessionSourceFiles.RemoveRange(await db.ImportSessionSourceFiles.Where(file => file.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ClearCandidatesAndDecisionsAsync(Guid importSessionId, CancellationToken cancellationToken)
    {
        db.ImportStagedMappingDecisions.RemoveRange(await db.ImportStagedMappingDecisions.Where(decision => decision.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        db.ImportStagedMappingCandidates.RemoveRange(await db.ImportStagedMappingCandidates.Where(candidate => candidate.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<ImportStagedRowBase>> LoadTrackedRowsAsync(Guid importSessionId, CancellationToken cancellationToken)
    {
        var rows = new List<ImportStagedRowBase>();
        rows.AddRange(await db.ImportStagedControlledVocabulary.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        rows.AddRange(await db.ImportStagedVocabularyAliases.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        rows.AddRange(await db.ImportStagedComplianceKeys.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        rows.AddRange(await db.ImportStagedMaterialKeys.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        rows.AddRange(await db.ImportStagedRulePacks.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        rows.AddRange(await db.ImportStagedRuleRequirements.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        rows.AddRange(await db.ImportStagedFactRequirements.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        rows.AddRange(await db.ImportStagedRegulatoryMappings.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        rows.AddRange(await db.ImportStagedSdsReferences.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        rows.AddRange(await db.ImportStagedEvidenceReferences.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        rows.AddRange(await db.ImportStagedExceptionExemptions.Where(row => row.ImportSessionId == importSessionId).ToListAsync(cancellationToken));
        return rows;
    }

    private async Task<List<ImportStagedRowBase>> LoadRowsAsync(Guid importSessionId, CancellationToken cancellationToken)
    {
        var rows = await LoadTrackedRowsAsync(importSessionId, cancellationToken);
        return rows
            .OrderBy(row => row.SourceFile, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.RowNumber)
            .ToList();
    }

    private async Task<IReadOnlyList<ImportStagedFileSummaryResponse>> BuildFileSummariesAsync(
        Guid importSessionId,
        CancellationToken cancellationToken)
    {
        var rows = await LoadRowsAsync(importSessionId, cancellationToken);
        var files = await db.ImportSessionSourceFiles
            .AsNoTracking()
            .Where(file => file.ImportSessionId == importSessionId)
            .OrderBy(file => file.SourceFile)
            .ToListAsync(cancellationToken);
        return files.Select(file => new ImportStagedFileSummaryResponse(
                file.SourceFile,
                rows.Count(row => string.Equals(row.SourceFile, file.SourceFile, StringComparison.OrdinalIgnoreCase)),
                file.ValidationStatus,
                DeserializeList(file.ValidationErrorsJson)))
            .ToList();
    }

    private void AddStagedRow(Guid tenantId, Guid importSessionId, string sourceFile, ParsedCsvRow row, DateTimeOffset now)
    {
        var normalizedJson = Serialize(row.Values);
        var staged = CreateStagedRow(sourceFile);
        staged.StagedRowId = Guid.NewGuid();
        staged.TenantId = tenantId;
        staged.ImportSessionId = importSessionId;
        staged.SourceFile = sourceFile;
        staged.RowNumber = row.RowNumber;
        staged.RawRowJson = Serialize(row.Values);
        staged.NormalizedRowJson = normalizedJson;
        staged.RowHash = Sha256(normalizedJson);
        staged.ValidationStatus = ImportRowValidationStatuses.Pending;
        staged.ValidationErrorsJson = "[]";
        staged.CanonicalKeyCandidate = GetCanonicalKeyCandidate(sourceFile, row.Values);
        staged.CreatedAt = now;

        switch (staged)
        {
            case ImportStagedControlledVocabulary item:
                db.ImportStagedControlledVocabulary.Add(item);
                break;
            case ImportStagedVocabularyAlias item:
                db.ImportStagedVocabularyAliases.Add(item);
                break;
            case ImportStagedComplianceKey item:
                db.ImportStagedComplianceKeys.Add(item);
                break;
            case ImportStagedMaterialKey item:
                db.ImportStagedMaterialKeys.Add(item);
                break;
            case ImportStagedRulePack item:
                db.ImportStagedRulePacks.Add(item);
                break;
            case ImportStagedRuleRequirement item:
                db.ImportStagedRuleRequirements.Add(item);
                break;
            case ImportStagedFactRequirement item:
                db.ImportStagedFactRequirements.Add(item);
                break;
            case ImportStagedRegulatoryMapping item:
                db.ImportStagedRegulatoryMappings.Add(item);
                break;
            case ImportStagedSdsReference item:
                db.ImportStagedSdsReferences.Add(item);
                break;
            case ImportStagedEvidenceReference item:
                db.ImportStagedEvidenceReferences.Add(item);
                break;
            case ImportStagedExceptionExemption item:
                db.ImportStagedExceptionExemptions.Add(item);
                break;
        }
    }

    private static ImportStagedRowBase CreateStagedRow(string sourceFile) =>
        sourceFile.ToLowerInvariant() switch
        {
            CsvBundleFiles.ControlledVocabulary => new ImportStagedControlledVocabulary(),
            CsvBundleFiles.VocabularyAliases => new ImportStagedVocabularyAlias(),
            CsvBundleFiles.ComplianceKeys => new ImportStagedComplianceKey(),
            CsvBundleFiles.MaterialKeys => new ImportStagedMaterialKey(),
            CsvBundleFiles.RulePacks => new ImportStagedRulePack(),
            CsvBundleFiles.RuleRequirements => new ImportStagedRuleRequirement(),
            CsvBundleFiles.RuleFactRequirements => new ImportStagedFactRequirement(),
            CsvBundleFiles.RegulatoryMappings => new ImportStagedRegulatoryMapping(),
            CsvBundleFiles.SdsReferences => new ImportStagedSdsReference(),
            CsvBundleFiles.ExceptionExemptions => new ImportStagedExceptionExemption(),
            EvidenceReferencesFile => new ImportStagedEvidenceReference(),
            _ => throw new StlApiException("import_sessions.unknown_file", $"Unknown CSV file '{sourceFile}'.", 400)
        };

    private static string GetCanonicalKeyCandidate(string sourceFile, IReadOnlyDictionary<string, string> values) =>
        sourceFile.ToLowerInvariant() switch
        {
            CsvBundleFiles.ControlledVocabulary => values.GetValueOrDefault("term_key") ?? string.Empty,
            CsvBundleFiles.VocabularyAliases => $"{values.GetValueOrDefault("term_key")}|{values.GetValueOrDefault("alias_text")}",
            CsvBundleFiles.ComplianceKeys => values.GetValueOrDefault("key") ?? string.Empty,
            CsvBundleFiles.MaterialKeys => values.GetValueOrDefault("key") ?? string.Empty,
            CsvBundleFiles.RulePacks => $"{values.GetValueOrDefault("pack_key")}:v{values.GetValueOrDefault("version_number")}",
            CsvBundleFiles.RuleRequirements => values.GetValueOrDefault("citation_key") ?? string.Empty,
            CsvBundleFiles.RuleFactRequirements => values.GetValueOrDefault("requirement_key") ?? string.Empty,
            CsvBundleFiles.RegulatoryMappings => values.GetValueOrDefault("mapping_key") ?? string.Empty,
            CsvBundleFiles.SdsReferences => values.GetValueOrDefault("sds_key") ?? string.Empty,
            CsvBundleFiles.ExceptionExemptions => values.GetValueOrDefault("key") ?? string.Empty,
            EvidenceReferencesFile => values.GetValueOrDefault("evidence_id") ?? string.Empty,
            _ => string.Empty
        };

    private static ParsedCsvFile ParseCsv(string sourceFile, string content)
    {
        if (!FileHeaders.TryGetValue(sourceFile, out var expectedHeaders))
        {
            return new ParsedCsvFile([], [$"Unsupported source file '{sourceFile}'."]);
        }

        var errors = new List<string>();
        var rows = new List<ParsedCsvRow>();
        var lines = content
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
        var headerLineIndex = Array.FindIndex(lines, line => !string.IsNullOrWhiteSpace(line));
        if (headerLineIndex < 0)
        {
            return new ParsedCsvFile([], ["CSV file is empty."]);
        }

        var headers = CsvText.ParseRow(lines[headerLineIndex]).Select(header => header.Trim()).ToList();
        if (headers.Count != expectedHeaders.Count ||
            !headers.SequenceEqual(expectedHeaders, StringComparer.OrdinalIgnoreCase))
        {
            return new ParsedCsvFile(
                [],
                [$"Header must be: {string.Join(",", expectedHeaders)}"]);
        }

        for (var index = headerLineIndex + 1; index < lines.Length; index++)
        {
            var line = lines[index];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var fields = CsvText.ParseRow(line);
            if (fields.Count != expectedHeaders.Count)
            {
                errors.Add($"Line {index + 1}: expected {expectedHeaders.Count} columns but found {fields.Count}.");
                continue;
            }

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var fieldIndex = 0; fieldIndex < expectedHeaders.Count; fieldIndex++)
            {
                values[expectedHeaders[fieldIndex]] = fields[fieldIndex].Trim();
            }

            rows.Add(new ParsedCsvRow(index + 1, values));
        }

        return new ParsedCsvFile(rows, errors);
    }

    private static List<string> ValidateRow(
        string sourceFile,
        IReadOnlyDictionary<string, string> values,
        ValidationLookups lookups)
    {
        var errors = new List<string>();
        foreach (var header in FileHeaders[sourceFile])
        {
            if (!values.ContainsKey(header))
            {
                errors.Add($"Column '{header}' is missing.");
            }
        }

        foreach (var column in BooleanColumns)
        {
            if (values.TryGetValue(column, out var value) &&
                !string.IsNullOrWhiteSpace(value) &&
                !bool.TryParse(value, out _))
            {
                errors.Add($"Column '{column}' must be true or false.");
            }
        }

        switch (sourceFile.ToLowerInvariant())
        {
            case CsvBundleFiles.ControlledVocabulary:
                Require(values, errors, "term_key", "vocabulary_type_key", "label");
                if (!string.IsNullOrWhiteSpace(values.GetValueOrDefault("vocabulary_type_key")) &&
                    !lookups.VocabularyTypes.Contains(values.GetValueOrDefault("vocabulary_type_key")!))
                {
                    errors.Add($"Unknown vocabulary type '{values.GetValueOrDefault("vocabulary_type_key")}'.");
                }

                break;
            case CsvBundleFiles.VocabularyAliases:
                Require(values, errors, "term_key", "alias_text");
                if (!lookups.VocabularyTerms.Contains(values.GetValueOrDefault("term_key") ?? string.Empty))
                {
                    errors.Add($"Vocabulary term '{values.GetValueOrDefault("term_key")}' was not found in this session or canonically.");
                }

                break;
            case CsvBundleFiles.ComplianceKeys:
            case CsvBundleFiles.MaterialKeys:
                Require(values, errors, "key", "label", "category");
                break;
            case CsvBundleFiles.RulePacks:
                Require(values, errors, "pack_key", "program_key", "version_number", "label", "status");
                if (!RulePackStatuses.All.Contains(values.GetValueOrDefault("status") ?? string.Empty))
                {
                    errors.Add($"Unsupported rule pack status '{values.GetValueOrDefault("status")}'.");
                }

                if (!lookups.ProgramKeys.Contains(values.GetValueOrDefault("program_key") ?? string.Empty))
                {
                    errors.Add($"Regulatory program '{values.GetValueOrDefault("program_key")}' was not found canonically.");
                }

                break;
            case CsvBundleFiles.RuleRequirements:
                Require(values, errors, "citation_key", "program_key", "label", "source_reference");
                if (!lookups.ProgramKeys.Contains(values.GetValueOrDefault("program_key") ?? string.Empty))
                {
                    errors.Add($"Regulatory program '{values.GetValueOrDefault("program_key")}' was not found canonically.");
                }

                if (!string.IsNullOrWhiteSpace(values.GetValueOrDefault("pack_key")) &&
                    !lookups.PackKeys.Contains(PackKey(values.GetValueOrDefault("pack_key"), values.GetValueOrDefault("pack_version"))))
                {
                    errors.Add($"Rule pack '{values.GetValueOrDefault("pack_key")}' was not found in this session or canonically.");
                }

                if (!string.IsNullOrWhiteSpace(values.GetValueOrDefault("supersedes_citation_key")) &&
                    !lookups.CitationKeys.Contains(values.GetValueOrDefault("supersedes_citation_key") ?? string.Empty))
                {
                    errors.Add("Citation replacement lacks valid supersession metadata.");
                }

                break;
            case CsvBundleFiles.RuleFactRequirements:
                ValidateFactRequirementRow(values, lookups, errors);
                break;
            case CsvBundleFiles.RegulatoryMappings:
                Require(values, errors, "mapping_key", "target_kind", "program_key", "label");
                var targetKind = values.GetValueOrDefault("target_kind") ?? string.Empty;
                if (targetKind is not "compliance_key" and not "material_key")
                {
                    errors.Add("target_kind must be compliance_key or material_key.");
                }

                if (!lookups.ProgramKeys.Contains(values.GetValueOrDefault("program_key") ?? string.Empty))
                {
                    errors.Add($"Regulatory program '{values.GetValueOrDefault("program_key")}' was not found canonically.");
                }

                if (targetKind == "compliance_key" && !lookups.ComplianceKeys.Contains(values.GetValueOrDefault("compliance_key") ?? string.Empty))
                {
                    errors.Add($"Compliance key '{values.GetValueOrDefault("compliance_key")}' was not found in this session or canonically.");
                }

                if (targetKind == "material_key" && !lookups.MaterialKeys.Contains(values.GetValueOrDefault("material_key") ?? string.Empty))
                {
                    errors.Add($"Material key '{values.GetValueOrDefault("material_key")}' was not found in this session or canonically.");
                }

                break;
            case CsvBundleFiles.SdsReferences:
                Require(values, errors, "sds_key", "material_key", "product_name", "manufacturer");
                if (!lookups.MaterialKeys.Contains(values.GetValueOrDefault("material_key") ?? string.Empty))
                {
                    errors.Add($"Material key '{values.GetValueOrDefault("material_key")}' was not found in this session or canonically.");
                }

                if (!string.IsNullOrWhiteSpace(values.GetValueOrDefault("revision_date")) &&
                    DateOnly.TryParse(values.GetValueOrDefault("revision_date"), CultureInfo.InvariantCulture, DateTimeStyles.None, out _) == false)
                {
                    errors.Add("revision_date must be a valid date.");
                }

                break;
            case CsvBundleFiles.ExceptionExemptions:
                ValidateExceptionExemptionRow(values, lookups, errors);
                break;
            case EvidenceReferencesFile:
                Require(values, errors, "evidence_id", "fact_key", "source_product", "source_entity", "source_record_id", "file_hash");
                break;
        }

        return errors;
    }

    private static void ValidateFactRequirementRow(
        IReadOnlyDictionary<string, string> values,
        ValidationLookups lookups,
        List<string> errors)
    {
        var automaticFailure = ParseBool(values.GetValueOrDefault("automatic_failure_flag"));
        var overrideAllowed = ParseBool(values.GetValueOrDefault("override_allowed"), true);
        var remediation = ParseBool(values.GetValueOrDefault("remediation_required"), true);
        var isRequired = ParseBool(values.GetValueOrDefault("is_required"), true);
        var contract = new FactRequirementContractInput(
            values.GetValueOrDefault("requirement_key") ?? string.Empty,
            values.GetValueOrDefault("fact_key") ?? string.Empty,
            values.GetValueOrDefault("applicability_key") ?? string.Empty,
            values.GetValueOrDefault("source_product") ?? string.Empty,
            values.GetValueOrDefault("source_entity") ?? string.Empty,
            values.GetValueOrDefault("source_field_or_record_type") ?? string.Empty,
            (values.GetValueOrDefault("value_type") ?? string.Empty).ToLowerInvariant(),
            (values.GetValueOrDefault("operator") ?? string.Empty).ToLowerInvariant(),
            values.GetValueOrDefault("expected_value") ?? string.Empty,
            (values.GetValueOrDefault("evidence_kind") ?? string.Empty).ToLowerInvariant(),
            values.GetValueOrDefault("required_document_type") ?? string.Empty,
            values.GetValueOrDefault("retention_period") ?? string.Empty,
            values.GetValueOrDefault("audit_question") ?? string.Empty,
            (values.GetValueOrDefault("failure_severity") ?? string.Empty).ToLowerInvariant(),
            automaticFailure,
            overrideAllowed,
            values.GetValueOrDefault("override_permission") ?? string.Empty,
            remediation,
            isRequired);

        errors.AddRange(FactRequirementContractRules.Validate(contract, strictAuditMetadata: true));
        if (!string.IsNullOrWhiteSpace(values.GetValueOrDefault("pack_key")) &&
            !lookups.PackKeys.Contains(PackKey(values.GetValueOrDefault("pack_key"), values.GetValueOrDefault("pack_version"))))
        {
            errors.Add($"Rule pack '{values.GetValueOrDefault("pack_key")}' was not found in this session or canonically.");
        }

        if (!string.IsNullOrWhiteSpace(values.GetValueOrDefault("citation_key")) &&
            !lookups.CitationKeys.Contains(values.GetValueOrDefault("citation_key") ?? string.Empty))
        {
            errors.Add($"Citation '{values.GetValueOrDefault("citation_key")}' was not found in this session or canonically.");
        }

        if (!KnownSourceEntities.Contains(values.GetValueOrDefault("source_entity") ?? string.Empty))
        {
            // Unknown source entities are queued for controlled-vocabulary confirmation by the mapping wizard.
        }

        if (string.Equals(contract.EvidenceKind, FactRequirementEvidenceKinds.DerivedFact, StringComparison.OrdinalIgnoreCase))
        {
            foreach (var component in FactRequirementContractRules.SplitCsv(contract.ExpectedValue))
            {
                if (!lookups.FactKeys.Contains(component))
                {
                    errors.Add($"Derived fact component '{component}' was not found.");
                }
            }
        }
    }

    private static void ValidateExceptionExemptionRow(
        IReadOnlyDictionary<string, string> values,
        ValidationLookups lookups,
        List<string> errors)
    {
        Require(values, errors, "key", "label", "type", "effect_type");
        var type = values.GetValueOrDefault("type") ?? string.Empty;
        if (!ComplianceExceptionExemptionTypes.All.Contains(type))
        {
            errors.Add($"Unsupported exception/exemption type '{type}'.");
        }

        var effectType = values.GetValueOrDefault("effect_type") ?? string.Empty;
        if (!ComplianceExceptionExemptionEffectTypes.All.Contains(effectType))
        {
            errors.Add($"Unsupported exception/exemption effect_type '{effectType}'.");
        }

        if (!string.IsNullOrWhiteSpace(values.GetValueOrDefault("program_key")) &&
            !lookups.ProgramKeys.Contains(values.GetValueOrDefault("program_key") ?? string.Empty))
        {
            errors.Add($"Regulatory program '{values.GetValueOrDefault("program_key")}' was not found canonically.");
        }

        if (!string.IsNullOrWhiteSpace(values.GetValueOrDefault("pack_key")) &&
            !lookups.PackKeys.Any(key => key.StartsWith($"{values.GetValueOrDefault("pack_key")}:v", StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add($"Rule pack '{values.GetValueOrDefault("pack_key")}' was not found in this session or canonically.");
        }

        if (!string.IsNullOrWhiteSpace(values.GetValueOrDefault("citation_key")) &&
            !lookups.CitationKeys.Contains(values.GetValueOrDefault("citation_key") ?? string.Empty))
        {
            errors.Add($"Citation '{values.GetValueOrDefault("citation_key")}' was not found in this session or canonically.");
        }

        foreach (var column in new[] { "condition_logic_json" })
        {
            var json = values.GetValueOrDefault(column);
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    using var document = JsonDocument.Parse(json);
                    if (document.RootElement.ValueKind != JsonValueKind.Object)
                    {
                        errors.Add($"{column} must be a JSON object.");
                    }
                }
                catch (JsonException)
                {
                    errors.Add($"{column} must be valid JSON.");
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(values.GetValueOrDefault("effective_at")) &&
            !DateTimeOffset.TryParse(values.GetValueOrDefault("effective_at"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _))
        {
            errors.Add("effective_at must be a valid date/time.");
        }

        if (!string.IsNullOrWhiteSpace(values.GetValueOrDefault("expires_at")) &&
            !DateTimeOffset.TryParse(values.GetValueOrDefault("expires_at"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _))
        {
            errors.Add("expires_at must be a valid date/time.");
        }
    }

    private async Task<ValidationLookups> BuildValidationLookupsAsync(
        Guid tenantId,
        IReadOnlyList<ImportStagedRowBase> rows,
        CancellationToken cancellationToken)
    {
        var dictionaries = rows.Select(row => (row.SourceFile, Values: ToDictionary(row.NormalizedRowJson))).ToList();
        var vocabularyTypes = await db.VocabularyTypes.AsNoTracking().Select(type => type.TypeKey).ToListAsync(cancellationToken);
        var vocabularyTerms = await db.VocabularyTerms.AsNoTracking().Where(term => term.TenantId == tenantId).Select(term => term.TermKey).ToListAsync(cancellationToken);
        var complianceKeys = await db.ComplianceKeys.AsNoTracking().Where(key => key.TenantId == tenantId).Select(key => key.Key).ToListAsync(cancellationToken);
        var materialKeys = await db.MaterialKeys.AsNoTracking().Where(key => key.TenantId == tenantId).Select(key => key.Key).ToListAsync(cancellationToken);
        var programs = await db.RegulatoryPrograms.AsNoTracking().Where(program => program.TenantId == tenantId).Select(program => program.ProgramKey).ToListAsync(cancellationToken);
        var packs = await db.RulePacks.AsNoTracking().Where(pack => pack.TenantId == tenantId).Select(pack => $"{pack.PackKey}:v{pack.VersionNumber}").ToListAsync(cancellationToken);
        var citations = await db.RegulatoryCitations.AsNoTracking().Where(citation => citation.TenantId == tenantId).Select(citation => citation.CitationKey).ToListAsync(cancellationToken);
        var facts = await db.FactDefinitions.AsNoTracking().Where(fact => fact.TenantId == tenantId).Select(fact => fact.FactKey).ToListAsync(cancellationToken);

        return new ValidationLookups(
            vocabularyTypes.ToHashSet(StringComparer.OrdinalIgnoreCase),
            vocabularyTerms
                .Concat(dictionaries.Where(row => row.SourceFile == CsvBundleFiles.ControlledVocabulary).Select(row => row.Values.GetValueOrDefault("term_key") ?? string.Empty))
                .ToHashSet(StringComparer.OrdinalIgnoreCase),
            complianceKeys
                .Concat(dictionaries.Where(row => row.SourceFile == CsvBundleFiles.ComplianceKeys).Select(row => row.Values.GetValueOrDefault("key") ?? string.Empty))
                .ToHashSet(StringComparer.OrdinalIgnoreCase),
            materialKeys
                .Concat(dictionaries.Where(row => row.SourceFile == CsvBundleFiles.MaterialKeys).Select(row => row.Values.GetValueOrDefault("key") ?? string.Empty))
                .ToHashSet(StringComparer.OrdinalIgnoreCase),
            programs.ToHashSet(StringComparer.OrdinalIgnoreCase),
            packs
                .Concat(dictionaries.Where(row => row.SourceFile == CsvBundleFiles.RulePacks).Select(row => PackKey(row.Values.GetValueOrDefault("pack_key"), row.Values.GetValueOrDefault("version_number"))))
                .ToHashSet(StringComparer.OrdinalIgnoreCase),
            citations
                .Concat(dictionaries.Where(row => row.SourceFile == CsvBundleFiles.RuleRequirements).Select(row => row.Values.GetValueOrDefault("citation_key") ?? string.Empty))
                .ToHashSet(StringComparer.OrdinalIgnoreCase),
            facts
                .Concat(dictionaries.Where(row => row.SourceFile == CsvBundleFiles.RuleFactRequirements).Select(row => row.Values.GetValueOrDefault("fact_key") ?? string.Empty))
                .ToHashSet(StringComparer.OrdinalIgnoreCase));
    }

    private async Task<MappingLookup> BuildMappingLookupAsync(Guid tenantId, CancellationToken cancellationToken) =>
        new(
            await db.FactDefinitions.AsNoTracking().Where(item => item.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.ComplianceKeys.AsNoTracking().Where(item => item.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.MaterialKeys.AsNoTracking().Where(item => item.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.RegulatoryCitations.AsNoTracking().Where(item => item.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.VocabularyTerms.AsNoTracking().Where(item => item.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.VocabularyAliases.AsNoTracking().Where(item => item.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.EvidenceReferences.AsNoTracking().Where(item => item.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.DocumentReferences.AsNoTracking().Where(item => item.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.MaterialReferences.AsNoTracking().Where(item => item.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.PartReferences.AsNoTracking().Where(item => item.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.SystemReferences.AsNoTracking().Where(item => item.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.AssetReferences.AsNoTracking().Where(item => item.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.ComplianceExceptionExemptions.AsNoTracking().Where(item => item.TenantId == tenantId).ToListAsync(cancellationToken));

    private static TargetCandidate FindBestTarget(
        GeneratedEvidenceOption option,
        IReadOnlyDictionary<string, string> row,
        MappingLookup lookup)
    {
        var reasons = new List<string>();
        var risks = new List<string>();
        var sourceProduct = row.GetValueOrDefault("source_product") ?? string.Empty;
        var label = row.GetValueOrDefault("label") ?? option.OptionLabel;

        if (option.TargetKind == EvidenceOptionTargetKinds.NoDocumentRequired)
        {
            return new TargetCandidate(
                MappingTargetKinds.NoDocumentRequired,
                string.Empty,
                "no_document_required",
                "No document required",
                1.0m,
                [.. reasons, "Evidence option explicitly allows no document required."],
                risks,
                MappingProposedActions.NoDocumentRequired,
                true);
        }

        if (option.TargetKind is EvidenceOptionTargetKinds.Fact or EvidenceOptionTargetKinds.DerivedFact)
        {
            var fact = lookup.FactDefinitions.FirstOrDefault(fact => fact.FactKey == option.FactKey);
            if (fact is not null)
            {
                reasons.Add("Exact match on fact_key.");
                return new TargetCandidate(MappingTargetKinds.ExistingFactDefinition, fact.Id.ToString(), fact.FactKey, fact.Label, 1.0m, reasons, risks, MappingProposedActions.MapExisting, true);
            }

            return NewTarget(MappingTargetKinds.NewFactDefinition, option.FactKey, label, "No existing fact definition matched.");
        }

        if (option.TargetKind == EvidenceOptionTargetKinds.Material)
        {
            var materialKey = FirstNonEmpty(option.MaterialKey, row.GetValueOrDefault("material_key"), option.DocumentTypeKey);
            var material = lookup.MaterialKeys.FirstOrDefault(item => item.Key == materialKey);
            if (material is not null)
            {
                reasons.Add("Exact match on material_key.");
                return new TargetCandidate(MappingTargetKinds.ExistingMaterial, material.Id.ToString(), material.Key, material.Label, 1.0m, reasons, risks, MappingProposedActions.MapExisting, true);
            }

            var materialRef = FindReference(lookup.MaterialReferences, materialKey, label, reasons, risks);
            if (materialRef is not null)
            {
                return materialRef with { TargetKind = MappingTargetKinds.ExistingMaterial };
            }

            AddOwnershipRisk(EvidenceOptionTargetKinds.Material, sourceProduct, risks);
            return NewTarget(MappingTargetKinds.NewMaterial, materialKey, label, "No existing material matched.", risks);
        }

        if (option.TargetKind == EvidenceOptionTargetKinds.Part)
        {
            var part = FindReference(lookup.PartReferences, option.PartKey, label, reasons, risks);
            if (part is not null)
            {
                return part with { TargetKind = MappingTargetKinds.ExistingPart };
            }

            return NewTarget(MappingTargetKinds.NewPart, option.PartKey, label, "No existing part matched.", risks);
        }

        if (option.TargetKind == EvidenceOptionTargetKinds.System)
        {
            var system = FindReference(lookup.SystemReferences, option.SystemKey, label, reasons, risks);
            if (system is not null)
            {
                return system with { TargetKind = MappingTargetKinds.ExistingSystem };
            }

            AddOwnershipRisk(EvidenceOptionTargetKinds.System, sourceProduct, risks);
            return NewTarget(MappingTargetKinds.NewSystem, option.SystemKey, label, "No existing system matched.", risks);
        }

        if (option.TargetKind == EvidenceOptionTargetKinds.Asset)
        {
            var asset = FindReference(lookup.AssetReferences, option.AssetKind, label, reasons, risks);
            if (asset is not null)
            {
                return asset with { TargetKind = MappingTargetKinds.ExistingAsset };
            }

            AddOwnershipRisk(EvidenceOptionTargetKinds.Asset, sourceProduct, risks);
            return NewTarget(MappingTargetKinds.NewAsset, option.AssetKind, label, "No existing asset matched.", risks);
        }

        if (option.TargetKind == EvidenceOptionTargetKinds.ExternalRegistry)
        {
            reasons.Add("Evidence option targets an external registry reference.");
            return new TargetCandidate(MappingTargetKinds.ExternalRegistry, string.Empty, option.ExternalRegistryKey, option.OptionLabel, 0.90m, reasons, risks, MappingProposedActions.ReferenceOnly, true);
        }

        if (IsExceptionTargetKind(option.TargetKind))
        {
            var exceptionKey = FirstNonEmpty(option.DocumentTypeKey, option.OptionKey, row.GetValueOrDefault("required_document_type"));
            var existingException = lookup.ExceptionExemptions.FirstOrDefault(item =>
                string.Equals(item.Key, exceptionKey, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(item.Label, option.OptionLabel, StringComparison.OrdinalIgnoreCase));
            if (existingException is not null)
            {
                reasons.Add("Exact match on exception/exemption key or label.");
                if (existingException.ExpiresAt is not null && existingException.ExpiresAt < DateTimeOffset.UtcNow)
                {
                    risks.Add("Risk: exception/exemption record is expired.");
                }

                return new TargetCandidate(MappingTargetKinds.ExceptionExemption, existingException.ExceptionExemptionId.ToString(), existingException.Key, existingException.Label, 0.98m, reasons, risks, MappingProposedActions.MapExisting, true);
            }

            reasons.Add("Evidence path represents exception, exemption, special permit, approval, or alternate compliance proof.");
            return NewTarget(MapExceptionTargetKind(option.TargetKind), exceptionKey, option.OptionLabel, "No existing exception/exemption record matched.", risks);
        }

        if (option.TargetKind == EvidenceOptionTargetKinds.ProductRecord)
        {
            reasons.Add($"Source product {sourceProduct} matches owner of product records.");
            return new TargetCandidate(MappingTargetKinds.ProductRecord, string.Empty, option.SourceFieldOrRecordType, option.OptionLabel, 0.92m, reasons, risks, MappingProposedActions.ReferenceOnly, true);
        }

        var documentKey = FirstNonEmpty(option.DocumentTypeKey, row.GetValueOrDefault("required_document_type"), option.OptionKey);
        var evidence = lookup.EvidenceReferences.FirstOrDefault(item =>
            string.Equals(item.DocumentType, documentKey, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(item.EvidenceId, documentKey, StringComparison.OrdinalIgnoreCase));
        if (evidence is not null)
        {
            reasons.Add("Required document type matches existing evidence reference.");
            return new TargetCandidate(MappingTargetKinds.ExistingEvidenceReference, evidence.Id.ToString(), evidence.EvidenceId, evidence.DocumentType, 0.98m, reasons, risks, MappingProposedActions.MapExisting, true);
        }

        var document = FindReference(lookup.DocumentReferences, documentKey, label, reasons, risks);
        if (document is not null)
        {
            return document with { TargetKind = option.TargetKind == EvidenceOptionTargetKinds.DocumentRecord ? MappingTargetKinds.ExistingDocumentRecord : MappingTargetKinds.ExistingDocumentType };
        }

        return NewTarget(
            option.TargetKind == EvidenceOptionTargetKinds.DocumentRecord ? MappingTargetKinds.NewDocumentRecord : MappingTargetKinds.NewDocumentType,
            documentKey,
            Labelize(documentKey),
            "No existing document target matched.",
            risks);
    }

    private static TargetCandidate? FindReference<TReference>(
        IReadOnlyList<TReference> references,
        string key,
        string label,
        List<string> reasons,
        List<string> risks)
        where TReference : ProductObjectReferenceBase
    {
        var exact = references.FirstOrDefault(item =>
            string.Equals(item.StableKey, key, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(item.ExternalRecordId, key, StringComparison.OrdinalIgnoreCase));
        if (exact is not null)
        {
            if (!exact.Active)
            {
                risks.Add("Risk: target record is inactive.");
            }

            reasons.Add("Exact match on stable key.");
            return new TargetCandidate("existing", exact.ReferenceId.ToString(), exact.StableKey, exact.Label, 0.99m, reasons.ToList(), risks.ToList(), MappingProposedActions.MapExisting, true);
        }

        var similar = references
            .Select(item => new { Reference = item, Score = Similarity(label, item.Label) })
            .Where(item => item.Score >= 0.70m)
            .OrderByDescending(item => item.Score)
            .ToList();
        if (similar.Count > 1)
        {
            risks.Add("Risk: multiple existing targets have similar names.");
        }

        var best = similar.FirstOrDefault();
        if (best is null)
        {
            return null;
        }

        reasons.Add($"Label similarity {(best.Score * 100m):0}% after normalization.");
        return new TargetCandidate("existing", best.Reference.ReferenceId.ToString(), best.Reference.StableKey, best.Reference.Label, best.Score, reasons.ToList(), risks.ToList(), MappingProposedActions.MapExisting, true);
    }

    private static TargetCandidate NewTarget(string targetKind, string targetKey, string targetLabel, string reason, IReadOnlyList<string>? risks = null) =>
        new(
            targetKind,
            string.Empty,
            string.IsNullOrWhiteSpace(targetKey) ? NormalizeKey(targetLabel) : targetKey,
            string.IsNullOrWhiteSpace(targetLabel) ? Labelize(targetKey) : targetLabel,
            0.30m,
            [reason, "No exact, alias, or high-similarity existing target was found."],
            risks ?? [],
            MappingProposedActions.CreateNew,
            true);

    private static void AddOwnershipRisk(string targetKind, string sourceProduct, List<string> risks)
    {
        if (targetKind == EvidenceOptionTargetKinds.Material &&
            !string.Equals(sourceProduct, ComplianceCoreProductKeys.SupplyArr, StringComparison.OrdinalIgnoreCase))
        {
            risks.Add("Risk: this would map non-SupplyArr evidence to a SupplyArr-owned material object.");
        }

        if ((targetKind == EvidenceOptionTargetKinds.Asset || targetKind == EvidenceOptionTargetKinds.System) &&
            !string.Equals(sourceProduct, ComplianceCoreProductKeys.MaintainArr, StringComparison.OrdinalIgnoreCase))
        {
            risks.Add("Risk: this would map evidence to a MaintainArr-owned asset or system object.");
        }
    }

    private static bool IsIncompatibleTarget(string selectedTargetKind, string sourceProduct, string suggestedTargetKind) =>
        selectedTargetKind.Contains("material", StringComparison.OrdinalIgnoreCase) &&
        !suggestedTargetKind.Contains("material", StringComparison.OrdinalIgnoreCase) &&
        sourceProduct.Contains(ComplianceCoreProductKeys.MaintainArr, StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<GeneratedEvidenceOption> BuildEvidenceOptions(
        Guid importSessionId,
        IReadOnlyDictionary<string, string> values)
    {
        var requirementKey = values.GetValueOrDefault("requirement_key") ?? string.Empty;
        var factKey = values.GetValueOrDefault("fact_key") ?? string.Empty;
        var evidenceKind = (values.GetValueOrDefault("evidence_kind") ?? FactRequirementEvidenceKinds.ProductRecord).ToLowerInvariant();
        var operatorValue = (values.GetValueOrDefault("operator") ?? string.Empty).ToLowerInvariant();
        var rawDocumentType = values.GetValueOrDefault("required_document_type") ?? string.Empty;
        var logicType = evidenceKind == FactRequirementEvidenceKinds.DerivedFact
            ? EvidenceOptionLogicTypes.Derived
            : operatorValue == FactRequirementOperators.AllTrue
                ? EvidenceOptionLogicTypes.AllOf
                : EvidenceOptionLogicTypes.AnyOf;

        foreach (var candidatePrefix in EvidenceOptionLogicTypes.All)
        {
            var prefix = $"{candidatePrefix}:";
            if (rawDocumentType.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                logicType = candidatePrefix;
                rawDocumentType = rawDocumentType[prefix.Length..];
                break;
            }
        }

        var optionTokens = rawDocumentType
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToList();
        if (optionTokens.Count == 0)
        {
            optionTokens.Add(IsNoDocumentAllowed(evidenceKind) ? EvidenceOptionTargetKinds.NoDocumentRequired : evidenceKind);
        }

        var options = new List<GeneratedEvidenceOption>();
        for (var index = 0; index < optionTokens.Count; index++)
        {
            var token = optionTokens[index];
            var targetKind = TargetKindFromEvidenceKind(evidenceKind, token);
            var optionKey = $"{requirementKey}.{NormalizeKey(token)}.{index + 1}";
            options.Add(new GeneratedEvidenceOption(
                DeterministicGuid($"{importSessionId}:{optionKey}"),
                optionKey,
                Labelize(token),
                logicType,
                evidenceKind,
                targetKind,
                values.GetValueOrDefault("source_product") ?? string.Empty,
                values.GetValueOrDefault("source_entity") ?? string.Empty,
                values.GetValueOrDefault("source_field_or_record_type") ?? string.Empty,
                targetKind is EvidenceOptionTargetKinds.DocumentType or EvidenceOptionTargetKinds.DocumentRecord ? token : string.Empty,
                targetKind == EvidenceOptionTargetKinds.Material ? token : string.Empty,
                targetKind == EvidenceOptionTargetKinds.Part ? token : string.Empty,
                targetKind == EvidenceOptionTargetKinds.System ? token : string.Empty,
                targetKind == EvidenceOptionTargetKinds.Asset ? token : string.Empty,
                targetKind == EvidenceOptionTargetKinds.ExternalRegistry ? token : string.Empty,
                factKey,
                true,
                index + 1,
                null));
        }

        return options;
    }

    private static string TargetKindFromEvidenceKind(string evidenceKind, string token)
    {
        if (string.Equals(token, EvidenceOptionTargetKinds.NoDocumentRequired, StringComparison.OrdinalIgnoreCase))
        {
            return EvidenceOptionTargetKinds.NoDocumentRequired;
        }

        return evidenceKind switch
        {
            FactRequirementEvidenceKinds.DocumentRecord => EvidenceOptionTargetKinds.DocumentRecord,
            FactRequirementEvidenceKinds.SystemFact => EvidenceOptionTargetKinds.Fact,
            FactRequirementEvidenceKinds.DerivedFact => EvidenceOptionTargetKinds.DerivedFact,
            FactRequirementEvidenceKinds.ExternalRegistry => EvidenceOptionTargetKinds.ExternalRegistry,
            FactRequirementEvidenceKinds.InspectionRecord => EvidenceOptionTargetKinds.ProductRecord,
            FactRequirementEvidenceKinds.ProductRecord when !string.IsNullOrWhiteSpace(token) &&
                                                        IsExceptionToken(token) =>
                token.ToLowerInvariant(),
            FactRequirementEvidenceKinds.ProductRecord when !string.IsNullOrWhiteSpace(token) &&
                                                        !string.Equals(token, FactRequirementEvidenceKinds.ProductRecord, StringComparison.OrdinalIgnoreCase) =>
                EvidenceOptionTargetKinds.DocumentType,
            _ => EvidenceOptionTargetKinds.ProductRecord
        };
    }

    private static bool IsExceptionToken(string token) =>
        string.Equals(token, EvidenceOptionTargetKinds.ExceptionExemption, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(token, EvidenceOptionTargetKinds.Waiver, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(token, EvidenceOptionTargetKinds.Variance, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(token, EvidenceOptionTargetKinds.SpecialPermit, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(token, EvidenceOptionTargetKinds.Approval, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(token, EvidenceOptionTargetKinds.AlternateCompliancePath, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(token, EvidenceOptionTargetKinds.ConditionalExclusion, StringComparison.OrdinalIgnoreCase);

    private static bool IsExceptionTargetKind(string targetKind) =>
        IsExceptionToken(targetKind);

    private static string MapExceptionTargetKind(string targetKind) =>
        targetKind switch
        {
            EvidenceOptionTargetKinds.Waiver => MappingTargetKinds.Waiver,
            EvidenceOptionTargetKinds.Variance => MappingTargetKinds.Variance,
            EvidenceOptionTargetKinds.SpecialPermit => MappingTargetKinds.SpecialPermit,
            EvidenceOptionTargetKinds.Approval => MappingTargetKinds.Approval,
            EvidenceOptionTargetKinds.AlternateCompliancePath => MappingTargetKinds.AlternateCompliancePath,
            EvidenceOptionTargetKinds.ConditionalExclusion => MappingTargetKinds.ConditionalExclusion,
            _ => MappingTargetKinds.ExceptionExemption
        };

    private static bool IsNoDocumentAllowed(string evidenceKind) =>
        evidenceKind is FactRequirementEvidenceKinds.SystemFact
            or FactRequirementEvidenceKinds.DerivedFact
            or FactRequirementEvidenceKinds.ProductRecord
            or FactRequirementEvidenceKinds.ExternalRegistry;

    private static void Require(IReadOnlyDictionary<string, string> values, List<string> errors, params string[] columns)
    {
        foreach (var column in columns)
        {
            if (string.IsNullOrWhiteSpace(values.GetValueOrDefault(column)))
            {
                errors.Add($"Column '{column}' is required.");
            }
        }
    }

    private static void SetValidation(ImportStagedRowBase row, IReadOnlyList<string> errors)
    {
        row.ValidationStatus = errors.Count == 0 ? ImportRowValidationStatuses.Valid : ImportRowValidationStatuses.Invalid;
        row.ValidationErrorsJson = Serialize(errors);
    }

    private async Task<RulePack?> FindRulePackAsync(Guid tenantId, string? packKey, string? version, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(packKey))
        {
            return null;
        }

        var parsedVersion = ParseInt(version, 1);
        return await db.RulePacks.FirstOrDefaultAsync(
            pack => pack.TenantId == tenantId && pack.PackKey == packKey && pack.VersionNumber == parsedVersion,
            cancellationToken);
    }

    private async Task<RegulatoryCitation?> FindCitationAsync(Guid tenantId, string? citationKey, string? version, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(citationKey))
        {
            return null;
        }

        var parsedVersion = ParseInt(version, 1);
        return await db.RegulatoryCitations.FirstOrDefaultAsync(
            citation => citation.TenantId == tenantId && citation.CitationKey == citationKey && citation.VersionNumber == parsedVersion,
            cancellationToken);
    }

    private static string PackKey(string? packKey, string? version) =>
        $"{packKey ?? string.Empty}:v{ParseInt(version, 1)}";

    private static Dictionary<string, string> ToDictionary(string json) =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions)
        ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions);

    private static IReadOnlyList<string> DeserializeList(string json) =>
        JsonSerializer.Deserialize<IReadOnlyList<string>>(json, JsonOptions) ?? [];

    private static string Sha256(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static Guid DeterministicGuid(string seed)
    {
        Span<byte> bytes = stackalloc byte[16];
        MD5.HashData(Encoding.UTF8.GetBytes(seed), bytes);
        return new Guid(bytes);
    }

    private static string ToBand(decimal score) =>
        score >= 0.98m ? MappingConfidenceBands.Exact :
        score >= 0.90m ? MappingConfidenceBands.High :
        score >= 0.70m ? MappingConfidenceBands.Medium :
        score >= 0.40m ? MappingConfidenceBands.Low :
        MappingConfidenceBands.NoMatch;

    private static decimal Similarity(string left, string right)
    {
        var a = NormalizeKey(left);
        var b = NormalizeKey(right);
        if (a.Length == 0 || b.Length == 0)
        {
            return 0m;
        }

        if (a == b)
        {
            return 1m;
        }

        var aTokens = a.Split('_', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var bTokens = b.Split('_', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var intersection = aTokens.Intersect(bTokens, StringComparer.OrdinalIgnoreCase).Count();
        var union = aTokens.Union(bTokens, StringComparer.OrdinalIgnoreCase).Count();
        return union == 0 ? 0m : Math.Round((decimal)intersection / union, 3);
    }

    private static string NormalizeKey(string? value)
    {
        var builder = new StringBuilder();
        var previousUnderscore = false;
        foreach (var character in (value ?? string.Empty).Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousUnderscore = false;
                continue;
            }

            if (!previousUnderscore)
            {
                builder.Append('_');
                previousUnderscore = true;
            }
        }

        return builder.ToString().Trim('_');
    }

    private static string Labelize(string? value)
    {
        var normalized = NormalizeKey(value).Replace('_', ' ');
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(normalized);
    }

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    private static bool ParseBool(string? value, bool defaultValue = false) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : bool.TryParse(value, out var parsed) && parsed;

    private static int ParseInt(string? value, int defaultValue) =>
        string.IsNullOrWhiteSpace(value) || !int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? defaultValue
            : parsed;

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null;

    private static DateTimeOffset? ParseDateTimeOffset(string? value) =>
        DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed) ? parsed : null;

    private static ImportSessionResponse ToResponse(ImportSession session) =>
        new(
            session.ImportSessionId,
            session.TenantId,
            session.UploadedByPersonId,
            session.SourceFilename,
            session.SourceHash,
            session.ImportType,
            session.Status,
            session.ValidationStatus,
            session.MappingStatus,
            session.CommitStatus,
            session.CreatedAt,
            session.ValidatedAt,
            session.MappedAt,
            session.CommittedAt,
            session.RejectedAt,
            session.Notes);

    private static ImportSessionSourceFileResponse ToResponse(ImportSessionSourceFile file) =>
        new(
            file.ImportSessionSourceFileId,
            file.SourceFile,
            file.OriginalFilename,
            file.FileHash,
            file.ByteLength,
            file.ValidationStatus,
            DeserializeList(file.ValidationErrorsJson));

    private static MappingCandidateResponse ToResponse(ImportStagedMappingCandidate candidate) =>
        new(
            candidate.MappingCandidateId,
            candidate.StagedRowId,
            candidate.StagedSourceFile,
            candidate.StagedRowNumber,
            candidate.SourceKey,
            candidate.SourceLabel,
            candidate.EvidenceOptionId,
            candidate.EvidenceOptionKey,
            candidate.EvidenceOptionLabel,
            candidate.OptionLogicGroup,
            candidate.TargetKind,
            candidate.TargetId,
            candidate.TargetKey,
            candidate.TargetLabel,
            candidate.ConfidenceScore,
            candidate.ConfidenceBand,
            DeserializeList(candidate.MatchReasonsJson),
            DeserializeList(candidate.RiskFlagsJson),
            candidate.ProposedAction,
            candidate.SatisfiesRequirementIfConfirmed,
            candidate.RequiresAdditionalSupportingEvidence,
            candidate.RequiresConfirmation);

    private static MappingDecisionResponse ToResponse(ImportStagedMappingDecision decision) =>
        new(
            decision.MappingDecisionId,
            decision.ImportSessionId,
            decision.StagedRowId,
            decision.MappingCandidateId,
            decision.Decision,
            decision.SelectedEvidenceOptionId,
            decision.SelectedEvidenceOptionKey,
            decision.SelectedTargetKind,
            decision.SelectedTargetId,
            decision.SelectedTargetKey,
            decision.EvidenceMappingPurpose,
            decision.ExceptionExemptionKey,
            DeserializeList(decision.ResidualRequirementsJson),
            decision.OverrideUsed,
            decision.OverrideReason,
            decision.DecidedByPersonId,
            decision.DecidedAt);

    private sealed record ParsedCsvFile(
        IReadOnlyList<ParsedCsvRow> Rows,
        IReadOnlyList<string> Errors);

    private sealed record ParsedCsvRow(
        int RowNumber,
        IReadOnlyDictionary<string, string> Values);

    private sealed record ValidationLookups(
        IReadOnlySet<string> VocabularyTypes,
        IReadOnlySet<string> VocabularyTerms,
        IReadOnlySet<string> ComplianceKeys,
        IReadOnlySet<string> MaterialKeys,
        IReadOnlySet<string> ProgramKeys,
        IReadOnlySet<string> PackKeys,
        IReadOnlySet<string> CitationKeys,
        IReadOnlySet<string> FactKeys);

    private sealed record MappingLookup(
        IReadOnlyList<FactDefinition> FactDefinitions,
        IReadOnlyList<ComplianceKey> ComplianceKeys,
        IReadOnlyList<MaterialKey> MaterialKeys,
        IReadOnlyList<RegulatoryCitation> Citations,
        IReadOnlyList<VocabularyTerm> VocabularyTerms,
        IReadOnlyList<VocabularyAlias> VocabularyAliases,
        IReadOnlyList<EvidenceReference> EvidenceReferences,
        IReadOnlyList<DocumentReference> DocumentReferences,
        IReadOnlyList<MaterialReference> MaterialReferences,
        IReadOnlyList<PartReference> PartReferences,
        IReadOnlyList<SystemReference> SystemReferences,
        IReadOnlyList<AssetReference> AssetReferences,
        IReadOnlyList<ComplianceExceptionExemption> ExceptionExemptions);

    private sealed record TargetCandidate(
        string TargetKind,
        string TargetId,
        string TargetKey,
        string TargetLabel,
        decimal ConfidenceScore,
        IReadOnlyList<string> Reasons,
        IReadOnlyList<string> Risks,
        string ProposedAction,
        bool SatisfiesRequirementIfConfirmed);

    private sealed record GeneratedEvidenceOption(
        Guid EvidenceOptionId,
        string OptionKey,
        string OptionLabel,
        string LogicType,
        string EvidenceKind,
        string TargetKind,
        string SourceProduct,
        string SourceEntity,
        string SourceFieldOrRecordType,
        string DocumentTypeKey,
        string MaterialKey,
        string PartKey,
        string SystemKey,
        string AssetKind,
        string ExternalRegistryKey,
        string FactKey,
        bool Required,
        int Priority,
        decimal? ConfidenceHint)
    {
        public EvidenceOptionProposalResponse ToResponse() =>
            new(
                EvidenceOptionId,
                OptionKey,
                OptionLabel,
                LogicType,
                EvidenceKind,
                TargetKind,
                SourceProduct,
                SourceEntity,
                SourceFieldOrRecordType,
                DocumentTypeKey,
                MaterialKey,
                PartKey,
                SystemKey,
                AssetKind,
                ExternalRegistryKey,
                FactKey,
                Required,
                Priority,
                ConfidenceHint);
    }
}
