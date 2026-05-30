using System.IO.Compression;
using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Csv;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class CsvImportExportService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
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
            ]
        };

    public CsvBundleManifestResponse GetManifest() =>
        new(FileHeaders.Select(pair => new CsvBundleFileDescriptor(pair.Key, pair.Value)).ToList());

    public async Task<byte[]> ExportZipAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var files = await ExportAllFilesAsync(tenantId, cancellationToken);
        await using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (fileName, content) in files)
            {
                var entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                await using var entryStream = entry.Open();
                var bytes = Encoding.UTF8.GetBytes(content);
                await entryStream.WriteAsync(bytes, cancellationToken);
            }
        }

        memory.Position = 0;
        return memory.ToArray();
    }

    public async Task<string> ExportFileAsync(
        Guid tenantId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (!FileHeaders.TryGetValue(fileName, out _))
        {
            throw new StlApiException("csv_bundle.unknown_file", "Unknown CSV bundle file.", 404);
        }

        var files = await ExportAllFilesAsync(tenantId, cancellationToken);
        return files[fileName];
    }

    public async Task<CsvImportResultResponse> ImportAsync(
        Guid tenantId,
        Guid? actorUserId,
        IReadOnlyDictionary<string, string> fileContents,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<CsvImportIssue>();
        var summaries = new List<CsvImportFileSummary>();
        ParsedCsvBundle? parsed = null;

        try
        {
            parsed = ParseBundle(fileContents);
        }
        catch (CsvParseException ex)
        {
            issues.Add(new CsvImportIssue(ex.FileName, ex.LineNumber, "csv.parse", ex.Message));
            return new CsvImportResultResponse(dryRun, false, summaries, issues);
        }

        try
        {
            return await ApplyImportAsync(tenantId, actorUserId, parsed, dryRun, summaries, issues, cancellationToken);
        }
        catch (CsvParseException ex)
        {
            issues.Add(new CsvImportIssue(ex.FileName, ex.LineNumber, "csv.parse", ex.Message));
            return new CsvImportResultResponse(dryRun, false, summaries, issues);
        }
    }

    private async Task<CsvImportResultResponse> ApplyImportAsync(
        Guid tenantId,
        Guid? actorUserId,
        ParsedCsvBundle parsed,
        bool dryRun,
        List<CsvImportFileSummary> summaries,
        List<CsvImportIssue> issues,
        CancellationToken cancellationToken)
    {
        if (db.Database.IsRelational())
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await ApplyImportCoreAsync(
                    tenantId,
                    actorUserId,
                    parsed,
                    dryRun,
                    summaries,
                    issues,
                    cancellationToken);
                if (!result.Applied)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return result;
                }

                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        return await ApplyImportCoreAsync(
            tenantId,
            actorUserId,
            parsed,
            dryRun,
            summaries,
            issues,
            cancellationToken);
    }

    private async Task<CsvImportResultResponse> ApplyImportCoreAsync(
        Guid tenantId,
        Guid? actorUserId,
        ParsedCsvBundle parsed,
        bool dryRun,
        List<CsvImportFileSummary> summaries,
        List<CsvImportIssue> issues,
        CancellationToken cancellationToken)
    {
        var context = await BuildImportContextAsync(tenantId, cancellationToken);
        summaries.Add(await ApplyVocabularyAsync(tenantId, parsed.ControlledVocabulary, context, issues, cancellationToken));
        summaries.Add(await ApplyAliasesAsync(tenantId, parsed.VocabularyAliases, context, issues, cancellationToken));
        summaries.Add(await ApplyComplianceKeysAsync(tenantId, parsed.ComplianceKeys, context, issues, cancellationToken));
        summaries.Add(await ApplyMaterialKeysAsync(tenantId, parsed.MaterialKeys, context, issues, cancellationToken));
        summaries.Add(await ApplyRulePacksAsync(tenantId, parsed.RulePacks, context, issues, cancellationToken));
        summaries.Add(await ApplyRuleRequirementsAsync(tenantId, parsed.RuleRequirements, context, issues, cancellationToken));
        summaries.Add(await ApplyRuleFactRequirementsAsync(tenantId, parsed.RuleFactRequirements, context, issues, cancellationToken));
        summaries.Add(await ApplyRegulatoryMappingsAsync(tenantId, parsed.RegulatoryMappings, context, issues, cancellationToken));
        summaries.Add(await ApplySdsReferencesAsync(tenantId, parsed.SdsReferences, context, issues, cancellationToken));

        if (issues.Count > 0 || dryRun)
        {
            db.ChangeTracker.Clear();
            return new CsvImportResultResponse(dryRun, false, summaries, issues);
        }

        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "csv_bundle.import",
            tenantId,
            actorUserId,
            "csv_bundle",
            null,
            "success",
            cancellationToken: cancellationToken);

        return new CsvImportResultResponse(false, true, summaries, issues);
    }

    private async Task<Dictionary<string, string>> ExportAllFilesAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var programs = await db.RegulatoryPrograms
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.Id, x => x.ProgramKey, cancellationToken);

        var rulePacks = await db.RulePacks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var packKeys = rulePacks.ToDictionary(x => x.Id, x => (x.PackKey, x.VersionNumber, programs.GetValueOrDefault(x.RegulatoryProgramId, string.Empty)));

        var citations = await db.RegulatoryCitations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var citationKeys = citations.ToDictionary(x => x.Id, x => x.CitationKey);

        var terms = await db.VocabularyTerms
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.TermKey)
            .ToListAsync(cancellationToken);

        var aliases = await db.VocabularyAliases
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var termKeys = terms.ToDictionary(x => x.Id, x => x.TermKey);

        var complianceKeys = await db.ComplianceKeys
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Key)
            .ToListAsync(cancellationToken);

        var materialKeys = await db.MaterialKeys
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Key)
            .ToListAsync(cancellationToken);

        var materialKeyById = materialKeys.ToDictionary(x => x.Id, x => x.Key);

        var factDefinitions = await db.FactDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.Id, x => x.FactKey, cancellationToken);

        var factRequirements = await db.FactRequirements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.RequirementKey)
            .ToListAsync(cancellationToken);

        var mappings = await db.RegulatoryMappings
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.MappingKey)
            .ToListAsync(cancellationToken);

        var sdsReferences = await db.SdsReferences
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.SdsKey)
            .ToListAsync(cancellationToken);

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [CsvBundleFiles.ControlledVocabulary] = CsvText.BuildTable(
                FileHeaders[CsvBundleFiles.ControlledVocabulary],
                terms.Select(term => new string?[]
                {
                    term.TermKey,
                    term.VocabularyTypeKey,
                    term.Label,
                    term.Description,
                    term.IsActive.ToString().ToLowerInvariant()
                })),
            [CsvBundleFiles.VocabularyAliases] = CsvText.BuildTable(
                FileHeaders[CsvBundleFiles.VocabularyAliases],
                aliases
                    .OrderBy(x => termKeys.GetValueOrDefault(x.VocabularyTermId, string.Empty))
                    .ThenBy(x => x.AliasText)
                    .Select(alias => new string?[]
                    {
                        termKeys.GetValueOrDefault(alias.VocabularyTermId, string.Empty),
                        alias.AliasText,
                        alias.IsActive.ToString().ToLowerInvariant()
                    })),
            [CsvBundleFiles.ComplianceKeys] = CsvText.BuildTable(
                FileHeaders[CsvBundleFiles.ComplianceKeys],
                complianceKeys.Select(key => new string?[]
                {
                    key.Key,
                    key.Label,
                    key.Category,
                    key.Description,
                    key.IsActive.ToString().ToLowerInvariant()
                })),
            [CsvBundleFiles.MaterialKeys] = CsvText.BuildTable(
                FileHeaders[CsvBundleFiles.MaterialKeys],
                materialKeys.Select(key => new string?[]
                {
                    key.Key,
                    key.Label,
                    key.Category,
                    key.Description,
                    key.IsActive.ToString().ToLowerInvariant()
                })),
            [CsvBundleFiles.RulePacks] = CsvText.BuildTable(
                FileHeaders[CsvBundleFiles.RulePacks],
                rulePacks
                    .OrderBy(x => x.PackKey)
                    .ThenBy(x => x.VersionNumber)
                    .Select(pack => new string?[]
                    {
                        pack.PackKey,
                        programs.GetValueOrDefault(pack.RegulatoryProgramId, string.Empty),
                        pack.VersionNumber.ToString(),
                        pack.Label,
                        pack.Description,
                        pack.Status,
                        pack.IsActive.ToString().ToLowerInvariant(),
                        pack.RuleContentJson ?? string.Empty
                    })),
            [CsvBundleFiles.RuleRequirements] = CsvText.BuildTable(
                FileHeaders[CsvBundleFiles.RuleRequirements],
                citations
                    .OrderBy(x => x.CitationKey)
                    .ThenBy(x => x.VersionNumber)
                    .Select(citation =>
                    {
                        var pack = citation.RulePackId.HasValue && packKeys.TryGetValue(citation.RulePackId.Value, out var packInfo)
                            ? packInfo
                            : (string.Empty, 0, programs.GetValueOrDefault(citation.RegulatoryProgramId, string.Empty));
                        return new string?[]
                        {
                            citation.CitationKey,
                            programs.GetValueOrDefault(citation.RegulatoryProgramId, string.Empty),
                            pack.Item1,
                            pack.Item2 > 0 ? pack.Item2.ToString() : string.Empty,
                            citation.Label,
                            citation.SourceReference,
                            citation.Description,
                            citation.IsActive.ToString().ToLowerInvariant(),
                            citation.SupersedesCitationId.HasValue
                                ? citationKeys.GetValueOrDefault(citation.SupersedesCitationId.Value, string.Empty)
                                : string.Empty
                        };
                    })),
            [CsvBundleFiles.RuleFactRequirements] = CsvText.BuildTable(
                FileHeaders[CsvBundleFiles.RuleFactRequirements],
                factRequirements.Select(requirement =>
                {
                    var pack = requirement.RulePackId.HasValue && packKeys.TryGetValue(requirement.RulePackId.Value, out var packInfo)
                        ? packInfo
                        : (string.Empty, 0, string.Empty);
                    var citation = requirement.CitationId.HasValue
                        ? citations.FirstOrDefault(x => x.Id == requirement.CitationId.Value)
                        : null;
                    return new string?[]
                    {
                        requirement.RequirementKey,
                        factDefinitions.GetValueOrDefault(requirement.FactDefinitionId, string.Empty),
                        pack.Item1,
                        pack.Item2 > 0 ? pack.Item2.ToString() : string.Empty,
                        citation?.CitationKey ?? string.Empty,
                        citation?.VersionNumber.ToString() ?? string.Empty,
                        requirement.ApplicabilityKey,
                        requirement.SourceProduct,
                        requirement.SourceEntity,
                        requirement.SourceFieldOrRecordType,
                        requirement.ValueType,
                        requirement.Operator,
                        requirement.ExpectedValue,
                        requirement.EvidenceKind,
                        requirement.RequiredDocumentType,
                        requirement.RetentionPeriod,
                        requirement.AuditQuestion,
                        requirement.FailureSeverity,
                        requirement.AutomaticFailureFlag.ToString().ToLowerInvariant(),
                        requirement.OverrideAllowed.ToString().ToLowerInvariant(),
                        requirement.OverridePermission,
                        requirement.RemediationRequired.ToString().ToLowerInvariant(),
                        requirement.Label,
                        requirement.Description,
                        requirement.IsRequired.ToString().ToLowerInvariant(),
                        requirement.IsActive.ToString().ToLowerInvariant()
                    };
                })),
            [CsvBundleFiles.RegulatoryMappings] = CsvText.BuildTable(
                FileHeaders[CsvBundleFiles.RegulatoryMappings],
                mappings.Select(mapping =>
                {
                    var pack = mapping.RulePackId.HasValue && packKeys.TryGetValue(mapping.RulePackId.Value, out var packInfo)
                        ? packInfo
                        : (string.Empty, 0, programs.GetValueOrDefault(mapping.RegulatoryProgramId, string.Empty));
                    return new string?[]
                    {
                        mapping.MappingKey,
                        mapping.TargetKind,
                        programs.GetValueOrDefault(mapping.RegulatoryProgramId, string.Empty),
                        pack.Item1,
                        pack.Item2 > 0 ? pack.Item2.ToString() : string.Empty,
                        mapping.CitationId.HasValue
                            ? citationKeys.GetValueOrDefault(mapping.CitationId.Value, string.Empty)
                            : string.Empty,
                        mapping.ComplianceKeyId.HasValue
                            ? complianceKeys.FirstOrDefault(x => x.Id == mapping.ComplianceKeyId.Value)?.Key ?? string.Empty
                            : string.Empty,
                        mapping.MaterialKeyId.HasValue
                            ? materialKeyById.GetValueOrDefault(mapping.MaterialKeyId.Value, string.Empty)
                            : string.Empty,
                        mapping.FactDefinitionId.HasValue
                            ? factDefinitions.GetValueOrDefault(mapping.FactDefinitionId.Value, string.Empty)
                            : string.Empty,
                        mapping.Label,
                        mapping.Description,
                        mapping.IsActive.ToString().ToLowerInvariant()
                    };
                })),
            [CsvBundleFiles.SdsReferences] = CsvText.BuildTable(
                FileHeaders[CsvBundleFiles.SdsReferences],
                sdsReferences.Select(reference => new string?[]
                {
                    reference.SdsKey,
                    reference.MaterialKeyId.HasValue
                        ? materialKeyById.GetValueOrDefault(reference.MaterialKeyId.Value, string.Empty)
                        : string.Empty,
                    reference.ProductName,
                    reference.Manufacturer,
                    reference.DocumentUrl,
                    reference.RevisionDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                    reference.IsActive.ToString().ToLowerInvariant()
                }))
        };
    }

    private ParsedCsvBundle ParseBundle(IReadOnlyDictionary<string, string> fileContents)
    {
        IReadOnlyList<IReadOnlyDictionary<string, string>> ReadFile(string fileName) =>
            fileContents.TryGetValue(fileName, out var content) && !string.IsNullOrWhiteSpace(content)
                ? CsvText.ParseTable(content, fileName, FileHeaders[fileName])
                : [];

        return new ParsedCsvBundle(
            ReadFile(CsvBundleFiles.ControlledVocabulary),
            ReadFile(CsvBundleFiles.VocabularyAliases),
            ReadFile(CsvBundleFiles.ComplianceKeys),
            ReadFile(CsvBundleFiles.MaterialKeys),
            ReadFile(CsvBundleFiles.RulePacks),
            ReadFile(CsvBundleFiles.RuleRequirements),
            ReadFile(CsvBundleFiles.RuleFactRequirements),
            ReadFile(CsvBundleFiles.RegulatoryMappings),
            ReadFile(CsvBundleFiles.SdsReferences));
    }

    private async Task<ImportContext> BuildImportContextAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        await db.VocabularyTypes.AsNoTracking().AnyAsync(cancellationToken);

        return new ImportContext(
            await db.VocabularyTerms.Where(x => x.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.VocabularyAliases.Where(x => x.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.ComplianceKeys.Where(x => x.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.MaterialKeys.Where(x => x.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.RegulatoryPrograms.Where(x => x.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.RulePacks.Where(x => x.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.RegulatoryCitations.Where(x => x.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.FactDefinitions.Where(x => x.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.FactRequirements.Where(x => x.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.RegulatoryMappings.Where(x => x.TenantId == tenantId).ToListAsync(cancellationToken),
            await db.SdsReferences.Where(x => x.TenantId == tenantId).ToListAsync(cancellationToken),
            (await db.VocabularyTypes.AsNoTracking().Select(x => x.TypeKey).ToListAsync(cancellationToken))
                .ToHashSet(StringComparer.OrdinalIgnoreCase));
    }

    private async Task<CsvImportFileSummary> ApplyVocabularyAsync(
        Guid tenantId,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        ImportContext context,
        List<CsvImportIssue> issues,
        CancellationToken cancellationToken)
    {
        var created = 0;
        var updated = 0;
        var deactivated = 0;
        var lineNumber = 1;

        foreach (var row in rows)
        {
            lineNumber++;
            var termKey = RequireKey(row, "term_key", CsvBundleFiles.ControlledVocabulary, lineNumber, issues);
            if (termKey is null)
            {
                continue;
            }

            var typeKey = RequireKey(row, "vocabulary_type_key", CsvBundleFiles.ControlledVocabulary, lineNumber, issues);
            if (typeKey is null || !context.VocabularyTypeKeys.Contains(typeKey))
            {
                issues.Add(new CsvImportIssue(
                    CsvBundleFiles.ControlledVocabulary,
                    lineNumber,
                    "vocabulary.type_unknown",
                    $"Unknown vocabulary type '{typeKey}'."));
                continue;
            }

            var label = RequireLabel(row, "label", CsvBundleFiles.ControlledVocabulary, lineNumber, issues);
            if (label is null)
            {
                continue;
            }

            var description = row.GetValueOrDefault("description") ?? string.Empty;
            var active = CsvText.ParseBool(row["active"], CsvBundleFiles.ControlledVocabulary, lineNumber, "active");
            var existing = context.Terms.FirstOrDefault(x => x.TermKey == termKey);
            var now = DateTimeOffset.UtcNow;

            if (existing is null)
            {
                existing = new VocabularyTerm
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    TermKey = termKey,
                    VocabularyTypeKey = typeKey,
                    Label = label,
                    Description = description,
                    IsActive = active,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                context.Terms.Add(existing);
                db.VocabularyTerms.Add(existing);
                created++;
            }
            else
            {
                existing.VocabularyTypeKey = typeKey;
                existing.Label = label;
                existing.Description = description;
                existing.IsActive = active;
                existing.UpdatedAt = now;
                updated++;
                if (!active)
                {
                    deactivated++;
                }
            }
        }

        await Task.CompletedTask;
        return new CsvImportFileSummary(CsvBundleFiles.ControlledVocabulary, rows.Count, created, updated, deactivated);
    }

    private async Task<CsvImportFileSummary> ApplyAliasesAsync(
        Guid tenantId,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        ImportContext context,
        List<CsvImportIssue> issues,
        CancellationToken cancellationToken)
    {
        var created = 0;
        var updated = 0;
        var deactivated = 0;
        var lineNumber = 1;

        foreach (var row in rows)
        {
            lineNumber++;
            var termKey = RequireKey(row, "term_key", CsvBundleFiles.VocabularyAliases, lineNumber, issues);
            if (termKey is null)
            {
                continue;
            }

            var term = context.Terms.FirstOrDefault(x => x.TermKey == termKey);
            if (term is null)
            {
                issues.Add(new CsvImportIssue(
                    CsvBundleFiles.VocabularyAliases,
                    lineNumber,
                    "vocabulary.term_not_found",
                    $"Vocabulary term '{termKey}' was not found."));
                continue;
            }

            var aliasText = RequireLabel(row, "alias_text", CsvBundleFiles.VocabularyAliases, lineNumber, issues);
            if (aliasText is null)
            {
                continue;
            }

            var active = CsvText.ParseBool(row["active"], CsvBundleFiles.VocabularyAliases, lineNumber, "active");
            var existing = context.Aliases.FirstOrDefault(
                x => x.VocabularyTermId == term.Id &&
                     string.Equals(x.AliasText, aliasText, StringComparison.OrdinalIgnoreCase));
            var now = DateTimeOffset.UtcNow;

            if (existing is null)
            {
                existing = new VocabularyAlias
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    VocabularyTermId = term.Id,
                    AliasText = aliasText,
                    IsActive = active,
                    CreatedAt = now
                };
                context.Aliases.Add(existing);
                db.VocabularyAliases.Add(existing);
                created++;
            }
            else
            {
                existing.IsActive = active;
                updated++;
                if (!active)
                {
                    deactivated++;
                }
            }
        }

        await Task.CompletedTask;
        return new CsvImportFileSummary(CsvBundleFiles.VocabularyAliases, rows.Count, created, updated, deactivated);
    }

    private async Task<CsvImportFileSummary> ApplyComplianceKeysAsync(
        Guid tenantId,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        ImportContext context,
        List<CsvImportIssue> issues,
        CancellationToken cancellationToken)
    {
        return await ApplyKeyRowsAsync(
            tenantId,
            rows,
            context.ComplianceKeys,
            (key, label, category, description, active, now) => new ComplianceKey
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Key = key,
                Label = label,
                Category = category,
                Description = description,
                IsActive = active,
                CreatedAt = now,
                UpdatedAt = now
            },
            (entity, label, category, description, active, now) =>
            {
                entity.Label = label;
                entity.Category = category;
                entity.Description = description;
                entity.IsActive = active;
                entity.UpdatedAt = now;
            },
            entity => db.ComplianceKeys.Add(entity),
            entity => entity.Key,
            CsvBundleFiles.ComplianceKeys,
            issues,
            cancellationToken);
    }

    private async Task<CsvImportFileSummary> ApplyMaterialKeysAsync(
        Guid tenantId,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        ImportContext context,
        List<CsvImportIssue> issues,
        CancellationToken cancellationToken)
    {
        return await ApplyKeyRowsAsync(
            tenantId,
            rows,
            context.MaterialKeys,
            (key, label, category, description, active, now) => new MaterialKey
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Key = key,
                Label = label,
                Category = category,
                Description = description,
                IsActive = active,
                CreatedAt = now,
                UpdatedAt = now
            },
            (entity, label, category, description, active, now) =>
            {
                entity.Label = label;
                entity.Category = category;
                entity.Description = description;
                entity.IsActive = active;
                entity.UpdatedAt = now;
            },
            entity => db.MaterialKeys.Add(entity),
            entity => entity.Key,
            CsvBundleFiles.MaterialKeys,
            issues,
            cancellationToken);
    }

    private async Task<CsvImportFileSummary> ApplyKeyRowsAsync<TEntity>(
        Guid tenantId,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        List<TEntity> entities,
        Func<string, string, string, string, bool, DateTimeOffset, TEntity> createEntity,
        Action<TEntity, string, string, string, bool, DateTimeOffset> updateEntity,
        Action<TEntity> addEntity,
        Func<TEntity, string> keySelector,
        string fileName,
        List<CsvImportIssue> issues,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var created = 0;
        var updated = 0;
        var deactivated = 0;
        var lineNumber = 1;

        foreach (var row in rows)
        {
            lineNumber++;
            var key = RequireKey(row, "key", fileName, lineNumber, issues);
            if (key is null)
            {
                continue;
            }

            var label = RequireLabel(row, "label", fileName, lineNumber, issues);
            var category = RequireKey(row, "category", fileName, lineNumber, issues);
            if (label is null || category is null)
            {
                continue;
            }

            var description = row.GetValueOrDefault("description") ?? string.Empty;
            var active = CsvText.ParseBool(row["active"], fileName, lineNumber, "active");
            var existing = entities.FirstOrDefault(x => keySelector(x) == key);
            var now = DateTimeOffset.UtcNow;

            if (existing is null)
            {
                existing = createEntity(key, label, category, description, active, now);
                entities.Add(existing);
                addEntity(existing);
                created++;
            }
            else
            {
                updateEntity(existing, label, category, description, active, now);
                updated++;
                if (!active)
                {
                    deactivated++;
                }
            }
        }

        await Task.CompletedTask;
        return new CsvImportFileSummary(fileName, rows.Count, created, updated, deactivated);
    }

    private async Task<CsvImportFileSummary> ApplyRulePacksAsync(
        Guid tenantId,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        ImportContext context,
        List<CsvImportIssue> issues,
        CancellationToken cancellationToken)
    {
        var created = 0;
        var updated = 0;
        var deactivated = 0;
        var lineNumber = 1;

        foreach (var row in rows)
        {
            lineNumber++;
            var packKey = RequireKey(row, "pack_key", CsvBundleFiles.RulePacks, lineNumber, issues);
            var programKey = RequireKey(row, "program_key", CsvBundleFiles.RulePacks, lineNumber, issues);
            if (packKey is null || programKey is null)
            {
                continue;
            }

            var program = context.Programs.FirstOrDefault(x => x.ProgramKey == programKey);
            if (program is null)
            {
                issues.Add(new CsvImportIssue(
                    CsvBundleFiles.RulePacks,
                    lineNumber,
                    "regulatory.program_not_found",
                    $"Regulatory program '{programKey}' was not found."));
                continue;
            }

            var version = CsvText.ParseInt(row["version_number"], CsvBundleFiles.RulePacks, lineNumber, "version_number");
            var label = RequireLabel(row, "label", CsvBundleFiles.RulePacks, lineNumber, issues);
            if (label is null)
            {
                continue;
            }

            var description = row.GetValueOrDefault("description") ?? string.Empty;
            var status = string.IsNullOrWhiteSpace(row.GetValueOrDefault("status"))
                ? RulePackStatuses.Draft
                : row["status"].Trim().ToLowerInvariant();
            if (!RulePackStatuses.All.Contains(status))
            {
                issues.Add(new CsvImportIssue(
                    CsvBundleFiles.RulePacks,
                    lineNumber,
                    "rule_packs.invalid_status",
                    $"Status '{status}' is not valid."));
                continue;
            }

            var active = CsvText.ParseBool(row["active"], CsvBundleFiles.RulePacks, lineNumber, "active");
            var ruleContentJson = row.GetValueOrDefault("rule_content_json");
            var existing = context.RulePacks.FirstOrDefault(x => x.PackKey == packKey && x.VersionNumber == version);
            var now = DateTimeOffset.UtcNow;

            if (existing is null)
            {
                existing = new RulePack
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    RegulatoryProgramId = program.Id,
                    PackKey = packKey,
                    VersionNumber = version,
                    Label = label,
                    Description = description,
                    Status = status,
                    RuleContentJson = string.IsNullOrWhiteSpace(ruleContentJson) ? null : ruleContentJson,
                    IsActive = active,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                context.RulePacks.Add(existing);
                db.RulePacks.Add(existing);
                created++;
            }
            else
            {
                existing.RegulatoryProgramId = program.Id;
                existing.Label = label;
                existing.Description = description;
                existing.Status = status;
                existing.RuleContentJson = string.IsNullOrWhiteSpace(ruleContentJson) ? null : ruleContentJson;
                existing.IsActive = active;
                existing.UpdatedAt = now;
                updated++;
                if (!active)
                {
                    deactivated++;
                }
            }
        }

        await Task.CompletedTask;
        return new CsvImportFileSummary(CsvBundleFiles.RulePacks, rows.Count, created, updated, deactivated);
    }

    private async Task<CsvImportFileSummary> ApplyRuleRequirementsAsync(
        Guid tenantId,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        ImportContext context,
        List<CsvImportIssue> issues,
        CancellationToken cancellationToken)
    {
        var created = 0;
        var updated = 0;
        var deactivated = 0;
        var lineNumber = 1;

        foreach (var row in rows)
        {
            lineNumber++;
            var citationKey = RequireKey(row, "citation_key", CsvBundleFiles.RuleRequirements, lineNumber, issues);
            var programKey = RequireKey(row, "program_key", CsvBundleFiles.RuleRequirements, lineNumber, issues);
            if (citationKey is null || programKey is null)
            {
                continue;
            }

            var program = context.Programs.FirstOrDefault(x => x.ProgramKey == programKey);
            if (program is null)
            {
                issues.Add(new CsvImportIssue(
                    CsvBundleFiles.RuleRequirements,
                    lineNumber,
                    "regulatory.program_not_found",
                    $"Regulatory program '{programKey}' was not found."));
                continue;
            }

            var version = 1;
            if (!string.IsNullOrWhiteSpace(row.GetValueOrDefault("pack_version")))
            {
                version = CsvText.ParseInt(row["pack_version"], CsvBundleFiles.RuleRequirements, lineNumber, "pack_version");
            }

            RulePack? rulePack = null;
            var packKey = row.GetValueOrDefault("pack_key");
            if (!string.IsNullOrWhiteSpace(packKey))
            {
                rulePack = context.RulePacks.FirstOrDefault(x => x.PackKey == packKey && x.VersionNumber == version);
                if (rulePack is null)
                {
                    issues.Add(new CsvImportIssue(
                        CsvBundleFiles.RuleRequirements,
                        lineNumber,
                        "rule_packs.not_found",
                        $"Rule pack '{packKey}' version {version} was not found."));
                    continue;
                }
            }

            var label = RequireLabel(row, "label", CsvBundleFiles.RuleRequirements, lineNumber, issues);
            if (label is null)
            {
                continue;
            }

            var sourceReference = RequireLabel(row, "source_reference", CsvBundleFiles.RuleRequirements, lineNumber, issues);
            if (sourceReference is null)
            {
                continue;
            }

            var description = row.GetValueOrDefault("description") ?? string.Empty;
            var active = CsvText.ParseBool(row["active"], CsvBundleFiles.RuleRequirements, lineNumber, "active");
            var citationVersion = 1;
            var existing = context.Citations.FirstOrDefault(x => x.CitationKey == citationKey && x.VersionNumber == citationVersion);
            var now = DateTimeOffset.UtcNow;
            Guid? supersedesId = null;
            var supersedesKey = row.GetValueOrDefault("supersedes_citation_key");
            if (!string.IsNullOrWhiteSpace(supersedesKey))
            {
                var superseded = context.Citations.FirstOrDefault(x => x.CitationKey == supersedesKey);
                if (superseded is null)
                {
                    issues.Add(new CsvImportIssue(
                        CsvBundleFiles.RuleRequirements,
                        lineNumber,
                        "citations.not_found",
                        $"Supersedes citation '{supersedesKey}' was not found."));
                    continue;
                }

                supersedesId = superseded.Id;
            }

            if (existing is null)
            {
                existing = new RegulatoryCitation
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    RegulatoryProgramId = program.Id,
                    RulePackId = rulePack?.Id,
                    CitationKey = citationKey,
                    VersionNumber = citationVersion,
                    Label = label,
                    SourceReference = sourceReference,
                    Description = description,
                    SupersedesCitationId = supersedesId,
                    IsActive = active,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                context.Citations.Add(existing);
                db.RegulatoryCitations.Add(existing);
                created++;
            }
            else
            {
                existing.RegulatoryProgramId = program.Id;
                existing.RulePackId = rulePack?.Id;
                existing.Label = label;
                existing.SourceReference = sourceReference;
                existing.Description = description;
                existing.SupersedesCitationId = supersedesId;
                existing.IsActive = active;
                existing.UpdatedAt = now;
                updated++;
                if (!active)
                {
                    deactivated++;
                }
            }
        }

        await Task.CompletedTask;
        return new CsvImportFileSummary(CsvBundleFiles.RuleRequirements, rows.Count, created, updated, deactivated);
    }

    private async Task<CsvImportFileSummary> ApplyRuleFactRequirementsAsync(
        Guid tenantId,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        ImportContext context,
        List<CsvImportIssue> issues,
        CancellationToken cancellationToken)
    {
        var created = 0;
        var updated = 0;
        var deactivated = 0;
        var lineNumber = 1;
        var seenRequirementKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var incomingFactKeys = rows
            .Select(row => row.GetValueOrDefault("fact_key"))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim().ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            lineNumber++;
            var requirementKey = RequireKey(row, "requirement_key", CsvBundleFiles.RuleFactRequirements, lineNumber, issues);
            var factKey = RequireKey(row, "fact_key", CsvBundleFiles.RuleFactRequirements, lineNumber, issues);
            if (requirementKey is null || factKey is null)
            {
                continue;
            }

            if (!seenRequirementKeys.Add(requirementKey))
            {
                issues.Add(new CsvImportIssue(
                    CsvBundleFiles.RuleFactRequirements,
                    lineNumber,
                    "fact_requirements.duplicate",
                    $"Requirement key '{requirementKey}' is duplicated in the import bundle."));
                continue;
            }

            RulePack? rulePack = null;
            var packKey = row.GetValueOrDefault("pack_key");
            if (!string.IsNullOrWhiteSpace(packKey))
            {
                var packVersion = string.IsNullOrWhiteSpace(row.GetValueOrDefault("pack_version"))
                    ? 1
                    : CsvText.ParseInt(row["pack_version"], CsvBundleFiles.RuleFactRequirements, lineNumber, "pack_version");
                rulePack = context.RulePacks.FirstOrDefault(x => x.PackKey == packKey && x.VersionNumber == packVersion);
                if (rulePack is null)
                {
                    issues.Add(new CsvImportIssue(
                        CsvBundleFiles.RuleFactRequirements,
                        lineNumber,
                        "rule_packs.not_found",
                        $"Rule pack '{packKey}' was not found."));
                    continue;
                }
            }

            RegulatoryCitation? citation = null;
            var citationKey = row.GetValueOrDefault("citation_key");
            if (!string.IsNullOrWhiteSpace(citationKey))
            {
                var citationVersion = string.IsNullOrWhiteSpace(row.GetValueOrDefault("citation_version"))
                    ? 1
                    : CsvText.ParseInt(row["citation_version"], CsvBundleFiles.RuleFactRequirements, lineNumber, "citation_version");
                citation = context.Citations.FirstOrDefault(x => x.CitationKey == citationKey && x.VersionNumber == citationVersion);
                if (citation is null)
                {
                    issues.Add(new CsvImportIssue(
                        CsvBundleFiles.RuleFactRequirements,
                        lineNumber,
                        "citations.not_found",
                        $"Citation '{citationKey}' was not found."));
                    continue;
                }
            }

            if (rulePack is null && citation is null)
            {
                issues.Add(new CsvImportIssue(
                    CsvBundleFiles.RuleFactRequirements,
                    lineNumber,
                    "fact_requirements.target_missing",
                    "Either pack_key or citation_key is required."));
                continue;
            }

            var label = RequireLabel(row, "label", CsvBundleFiles.RuleFactRequirements, lineNumber, issues);
            if (label is null)
            {
                continue;
            }

            var description = row.GetValueOrDefault("description") ?? string.Empty;
            var valueType = (row.GetValueOrDefault("value_type") ?? string.Empty).Trim().ToLowerInvariant();
            var operatorValue = (row.GetValueOrDefault("operator") ?? string.Empty).Trim().ToLowerInvariant();
            var evidenceKind = (row.GetValueOrDefault("evidence_kind") ?? string.Empty).Trim().ToLowerInvariant();
            var failureSeverity = (row.GetValueOrDefault("failure_severity") ?? string.Empty).Trim().ToLowerInvariant();
            var automaticFailureFlag = CsvText.ParseBool(
                row["automatic_failure_flag"],
                CsvBundleFiles.RuleFactRequirements,
                lineNumber,
                "automatic_failure_flag");
            var overrideAllowed = CsvText.ParseBool(
                row["override_allowed"],
                CsvBundleFiles.RuleFactRequirements,
                lineNumber,
                "override_allowed");
            var remediationRequired = CsvText.ParseBool(
                row["remediation_required"],
                CsvBundleFiles.RuleFactRequirements,
                lineNumber,
                "remediation_required");
            var isRequired = CsvText.ParseBool(row["is_required"], CsvBundleFiles.RuleFactRequirements, lineNumber, "is_required");
            var active = CsvText.ParseBool(row["active"], CsvBundleFiles.RuleFactRequirements, lineNumber, "active");

            var contract = new FactRequirementContractInput(
                requirementKey,
                factKey,
                (row.GetValueOrDefault("applicability_key") ?? string.Empty).Trim(),
                (row.GetValueOrDefault("source_product") ?? string.Empty).Trim(),
                (row.GetValueOrDefault("source_entity") ?? string.Empty).Trim(),
                (row.GetValueOrDefault("source_field_or_record_type") ?? string.Empty).Trim(),
                valueType,
                operatorValue,
                (row.GetValueOrDefault("expected_value") ?? string.Empty).Trim(),
                evidenceKind,
                (row.GetValueOrDefault("required_document_type") ?? string.Empty).Trim(),
                (row.GetValueOrDefault("retention_period") ?? string.Empty).Trim(),
                (row.GetValueOrDefault("audit_question") ?? string.Empty).Trim(),
                failureSeverity,
                automaticFailureFlag,
                overrideAllowed,
                (row.GetValueOrDefault("override_permission") ?? string.Empty).Trim(),
                remediationRequired,
                isRequired);

            var validationIssues = FactRequirementContractRules.Validate(contract, strictAuditMetadata: true);
            foreach (var component in FactRequirementContractRules.SplitCsv(contract.ExpectedValue))
            {
                if (string.Equals(contract.EvidenceKind, FactRequirementEvidenceKinds.DerivedFact, StringComparison.OrdinalIgnoreCase)
                    && !incomingFactKeys.Contains(component)
                    && !context.FactDefinitions.Any(x => string.Equals(x.FactKey, component, StringComparison.OrdinalIgnoreCase)))
                {
                    validationIssues = validationIssues.Append($"Derived fact component '{component}' was not found.").ToList();
                }
            }

            if (validationIssues.Count > 0)
            {
                issues.Add(new CsvImportIssue(
                    CsvBundleFiles.RuleFactRequirements,
                    lineNumber,
                    "fact_requirements.validation",
                    string.Join(" ", validationIssues)));
                continue;
            }

            var now = DateTimeOffset.UtcNow;
            var fact = context.FactDefinitions.FirstOrDefault(x => x.FactKey == factKey);
            if (fact is null)
            {
                fact = new FactDefinition
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    FactKey = factKey,
                    Label = label,
                    Description = description,
                    ValueType = valueType,
                    IsActive = active,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                context.FactDefinitions.Add(fact);
                db.FactDefinitions.Add(fact);
            }
            else
            {
                fact.Label = string.IsNullOrWhiteSpace(fact.Label) ? label : fact.Label;
                fact.Description = string.IsNullOrWhiteSpace(fact.Description) ? description : fact.Description;
                fact.ValueType = valueType;
                fact.IsActive = fact.IsActive || active;
                fact.UpdatedAt = now;
            }

            var existing = context.FactRequirements.FirstOrDefault(x => x.RequirementKey == requirementKey);

            if (existing is null)
            {
                existing = new FactRequirement
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    FactDefinitionId = fact.Id,
                    RulePackId = rulePack?.Id,
                    CitationId = citation?.Id,
                    RequirementKey = requirementKey,
                    Label = label,
                    Description = description,
                    ApplicabilityKey = contract.ApplicabilityKey,
                    SourceProduct = FactRequirementContractRules.NormalizeProducts(contract.SourceProduct),
                    SourceEntity = contract.SourceEntity,
                    SourceFieldOrRecordType = contract.SourceFieldOrRecordType,
                    ValueType = contract.ValueType,
                    Operator = contract.Operator,
                    ExpectedValue = contract.ExpectedValue,
                    EvidenceKind = contract.EvidenceKind,
                    RequiredDocumentType = contract.RequiredDocumentType,
                    RetentionPeriod = contract.RetentionPeriod,
                    AuditQuestion = contract.AuditQuestion,
                    FailureSeverity = contract.FailureSeverity,
                    AutomaticFailureFlag = contract.AutomaticFailureFlag,
                    OverrideAllowed = contract.OverrideAllowed,
                    OverridePermission = contract.OverridePermission,
                    RemediationRequired = contract.RemediationRequired,
                    ExternallyAssertable = false,
                    IsRequired = isRequired,
                    IsActive = active,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                context.FactRequirements.Add(existing);
                db.FactRequirements.Add(existing);
                created++;
            }
            else
            {
                existing.FactDefinitionId = fact.Id;
                existing.RulePackId = rulePack?.Id;
                existing.CitationId = citation?.Id;
                existing.Label = label;
                existing.Description = description;
                existing.ApplicabilityKey = contract.ApplicabilityKey;
                existing.SourceProduct = FactRequirementContractRules.NormalizeProducts(contract.SourceProduct);
                existing.SourceEntity = contract.SourceEntity;
                existing.SourceFieldOrRecordType = contract.SourceFieldOrRecordType;
                existing.ValueType = contract.ValueType;
                existing.Operator = contract.Operator;
                existing.ExpectedValue = contract.ExpectedValue;
                existing.EvidenceKind = contract.EvidenceKind;
                existing.RequiredDocumentType = contract.RequiredDocumentType;
                existing.RetentionPeriod = contract.RetentionPeriod;
                existing.AuditQuestion = contract.AuditQuestion;
                existing.FailureSeverity = contract.FailureSeverity;
                existing.AutomaticFailureFlag = contract.AutomaticFailureFlag;
                existing.OverrideAllowed = contract.OverrideAllowed;
                existing.OverridePermission = contract.OverridePermission;
                existing.RemediationRequired = contract.RemediationRequired;
                existing.ExternallyAssertable = false;
                existing.IsRequired = isRequired;
                existing.IsActive = active;
                existing.UpdatedAt = now;
                updated++;
                if (!active)
                {
                    deactivated++;
                }
            }
        }

        await Task.CompletedTask;
        return new CsvImportFileSummary(CsvBundleFiles.RuleFactRequirements, rows.Count, created, updated, deactivated);
    }

    private async Task<CsvImportFileSummary> ApplyRegulatoryMappingsAsync(
        Guid tenantId,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        ImportContext context,
        List<CsvImportIssue> issues,
        CancellationToken cancellationToken)
    {
        var created = 0;
        var updated = 0;
        var deactivated = 0;
        var lineNumber = 1;

        foreach (var row in rows)
        {
            lineNumber++;
            var mappingKey = RequireKey(row, "mapping_key", CsvBundleFiles.RegulatoryMappings, lineNumber, issues);
            var programKey = RequireKey(row, "program_key", CsvBundleFiles.RegulatoryMappings, lineNumber, issues);
            var targetKind = RequireKey(row, "target_kind", CsvBundleFiles.RegulatoryMappings, lineNumber, issues);
            if (mappingKey is null || programKey is null || targetKind is null)
            {
                continue;
            }

            targetKind = targetKind.ToLowerInvariant();
            if (targetKind is not "compliance_key" and not "material_key")
            {
                issues.Add(new CsvImportIssue(
                    CsvBundleFiles.RegulatoryMappings,
                    lineNumber,
                    "regulatory_mappings.invalid_target_kind",
                    "target_kind must be compliance_key or material_key."));
                continue;
            }

            var program = context.Programs.FirstOrDefault(x => x.ProgramKey == programKey);
            if (program is null)
            {
                issues.Add(new CsvImportIssue(
                    CsvBundleFiles.RegulatoryMappings,
                    lineNumber,
                    "regulatory.program_not_found",
                    $"Regulatory program '{programKey}' was not found."));
                continue;
            }

            RulePack? rulePack = null;
            var packKey = row.GetValueOrDefault("pack_key");
            if (!string.IsNullOrWhiteSpace(packKey))
            {
                var packVersion = string.IsNullOrWhiteSpace(row.GetValueOrDefault("pack_version"))
                    ? 1
                    : CsvText.ParseInt(row["pack_version"], CsvBundleFiles.RegulatoryMappings, lineNumber, "pack_version");
                rulePack = context.RulePacks.FirstOrDefault(x => x.PackKey == packKey && x.VersionNumber == packVersion);
                if (rulePack is null)
                {
                    issues.Add(new CsvImportIssue(
                        CsvBundleFiles.RegulatoryMappings,
                        lineNumber,
                        "rule_packs.not_found",
                        $"Rule pack '{packKey}' was not found."));
                    continue;
                }
            }

            RegulatoryCitation? citation = null;
            var citationKey = row.GetValueOrDefault("citation_key");
            if (!string.IsNullOrWhiteSpace(citationKey))
            {
                citation = context.Citations.FirstOrDefault(x => x.CitationKey == citationKey);
                if (citation is null)
                {
                    issues.Add(new CsvImportIssue(
                        CsvBundleFiles.RegulatoryMappings,
                        lineNumber,
                        "citations.not_found",
                        $"Citation '{citationKey}' was not found."));
                    continue;
                }
            }

            ComplianceKey? complianceKey = null;
            MaterialKey? materialKey = null;
            var complianceKeyValue = row.GetValueOrDefault("compliance_key");
            var materialKeyValue = row.GetValueOrDefault("material_key");
            if (targetKind == "compliance_key")
            {
                if (string.IsNullOrWhiteSpace(complianceKeyValue))
                {
                    issues.Add(new CsvImportIssue(
                        CsvBundleFiles.RegulatoryMappings,
                        lineNumber,
                        "regulatory_mappings.key_required",
                        "compliance_key is required for compliance_key target kind."));
                    continue;
                }

                complianceKey = context.ComplianceKeys.FirstOrDefault(x => x.Key == complianceKeyValue);
                if (complianceKey is null)
                {
                    issues.Add(new CsvImportIssue(
                        CsvBundleFiles.RegulatoryMappings,
                        lineNumber,
                        "compliance_keys.not_found",
                        $"Compliance key '{complianceKeyValue}' was not found."));
                    continue;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(materialKeyValue))
                {
                    issues.Add(new CsvImportIssue(
                        CsvBundleFiles.RegulatoryMappings,
                        lineNumber,
                        "regulatory_mappings.key_required",
                        "material_key is required for material_key target kind."));
                    continue;
                }

                materialKey = context.MaterialKeys.FirstOrDefault(x => x.Key == materialKeyValue);
                if (materialKey is null)
                {
                    issues.Add(new CsvImportIssue(
                        CsvBundleFiles.RegulatoryMappings,
                        lineNumber,
                        "material_keys.not_found",
                        $"Material key '{materialKeyValue}' was not found."));
                    continue;
                }
            }

            FactDefinition? factDefinition = null;
            var factKey = row.GetValueOrDefault("fact_key");
            if (!string.IsNullOrWhiteSpace(factKey))
            {
                factDefinition = context.FactDefinitions.FirstOrDefault(x => x.FactKey == factKey);
                if (factDefinition is null)
                {
                    issues.Add(new CsvImportIssue(
                        CsvBundleFiles.RegulatoryMappings,
                        lineNumber,
                        "facts.not_found",
                        $"Fact definition '{factKey}' was not found."));
                    continue;
                }
            }

            var label = RequireLabel(row, "label", CsvBundleFiles.RegulatoryMappings, lineNumber, issues);
            if (label is null)
            {
                continue;
            }

            var description = row.GetValueOrDefault("description") ?? string.Empty;
            var active = CsvText.ParseBool(row["active"], CsvBundleFiles.RegulatoryMappings, lineNumber, "active");
            var existing = context.Mappings.FirstOrDefault(x => x.MappingKey == mappingKey);
            var now = DateTimeOffset.UtcNow;

            if (existing is null)
            {
                existing = new RegulatoryMapping
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    MappingKey = mappingKey,
                    Label = label,
                    Description = description,
                    TargetKind = targetKind,
                    RegulatoryProgramId = program.Id,
                    RulePackId = rulePack?.Id,
                    CitationId = citation?.Id,
                    FactDefinitionId = factDefinition?.Id,
                    ComplianceKeyId = complianceKey?.Id,
                    MaterialKeyId = materialKey?.Id,
                    IsActive = active,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                context.Mappings.Add(existing);
                db.RegulatoryMappings.Add(existing);
                created++;
            }
            else
            {
                existing.Label = label;
                existing.Description = description;
                existing.TargetKind = targetKind;
                existing.RegulatoryProgramId = program.Id;
                existing.RulePackId = rulePack?.Id;
                existing.CitationId = citation?.Id;
                existing.FactDefinitionId = factDefinition?.Id;
                existing.ComplianceKeyId = complianceKey?.Id;
                existing.MaterialKeyId = materialKey?.Id;
                existing.IsActive = active;
                existing.UpdatedAt = now;
                updated++;
                if (!active)
                {
                    deactivated++;
                }
            }
        }

        await Task.CompletedTask;
        return new CsvImportFileSummary(CsvBundleFiles.RegulatoryMappings, rows.Count, created, updated, deactivated);
    }

    private async Task<CsvImportFileSummary> ApplySdsReferencesAsync(
        Guid tenantId,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        ImportContext context,
        List<CsvImportIssue> issues,
        CancellationToken cancellationToken)
    {
        var created = 0;
        var updated = 0;
        var deactivated = 0;
        var lineNumber = 1;

        foreach (var row in rows)
        {
            lineNumber++;
            var sdsKey = RequireKey(row, "sds_key", CsvBundleFiles.SdsReferences, lineNumber, issues);
            if (sdsKey is null)
            {
                continue;
            }

            MaterialKey? materialKey = null;
            var materialKeyValue = row.GetValueOrDefault("material_key");
            if (!string.IsNullOrWhiteSpace(materialKeyValue))
            {
                materialKey = context.MaterialKeys.FirstOrDefault(x => x.Key == materialKeyValue);
                if (materialKey is null)
                {
                    issues.Add(new CsvImportIssue(
                        CsvBundleFiles.SdsReferences,
                        lineNumber,
                        "material_keys.not_found",
                        $"Material key '{materialKeyValue}' was not found."));
                    continue;
                }
            }

            var productName = row.GetValueOrDefault("product_name") ?? string.Empty;
            var manufacturer = row.GetValueOrDefault("manufacturer") ?? string.Empty;
            var documentUrl = row.GetValueOrDefault("document_url") ?? string.Empty;
            var revisionDate = CsvText.ParseDateOnly(
                row.GetValueOrDefault("revision_date") ?? string.Empty,
                CsvBundleFiles.SdsReferences,
                lineNumber,
                "revision_date");
            var active = CsvText.ParseBool(row["active"], CsvBundleFiles.SdsReferences, lineNumber, "active");
            var existing = context.SdsReferences.FirstOrDefault(x => x.SdsKey == sdsKey);
            var now = DateTimeOffset.UtcNow;

            if (existing is null)
            {
                existing = new SdsReference
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    SdsKey = sdsKey,
                    MaterialKeyId = materialKey?.Id,
                    ProductName = productName,
                    Manufacturer = manufacturer,
                    DocumentUrl = documentUrl,
                    RevisionDate = revisionDate,
                    IsActive = active,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                context.SdsReferences.Add(existing);
                db.SdsReferences.Add(existing);
                created++;
            }
            else
            {
                existing.MaterialKeyId = materialKey?.Id;
                existing.ProductName = productName;
                existing.Manufacturer = manufacturer;
                existing.DocumentUrl = documentUrl;
                existing.RevisionDate = revisionDate;
                existing.IsActive = active;
                existing.UpdatedAt = now;
                updated++;
                if (!active)
                {
                    deactivated++;
                }
            }
        }

        await Task.CompletedTask;
        return new CsvImportFileSummary(CsvBundleFiles.SdsReferences, rows.Count, created, updated, deactivated);
    }

    private static string? RequireKey(
        IReadOnlyDictionary<string, string> row,
        string column,
        string fileName,
        int lineNumber,
        List<CsvImportIssue> issues)
    {
        if (!row.TryGetValue(column, out var value) || string.IsNullOrWhiteSpace(value))
        {
            issues.Add(new CsvImportIssue(fileName, lineNumber, "csv.required", $"Column '{column}' is required."));
            return null;
        }

        return value.Trim().ToLowerInvariant();
    }

    private static string? RequireLabel(
        IReadOnlyDictionary<string, string> row,
        string column,
        string fileName,
        int lineNumber,
        List<CsvImportIssue> issues)
    {
        if (!row.TryGetValue(column, out var value) || string.IsNullOrWhiteSpace(value))
        {
            issues.Add(new CsvImportIssue(fileName, lineNumber, "csv.required", $"Column '{column}' is required."));
            return null;
        }

        return value.Trim();
    }

    private sealed record ParsedCsvBundle(
        IReadOnlyList<IReadOnlyDictionary<string, string>> ControlledVocabulary,
        IReadOnlyList<IReadOnlyDictionary<string, string>> VocabularyAliases,
        IReadOnlyList<IReadOnlyDictionary<string, string>> ComplianceKeys,
        IReadOnlyList<IReadOnlyDictionary<string, string>> MaterialKeys,
        IReadOnlyList<IReadOnlyDictionary<string, string>> RulePacks,
        IReadOnlyList<IReadOnlyDictionary<string, string>> RuleRequirements,
        IReadOnlyList<IReadOnlyDictionary<string, string>> RuleFactRequirements,
        IReadOnlyList<IReadOnlyDictionary<string, string>> RegulatoryMappings,
        IReadOnlyList<IReadOnlyDictionary<string, string>> SdsReferences);

    private sealed class ImportContext(
        List<VocabularyTerm> terms,
        List<VocabularyAlias> aliases,
        List<ComplianceKey> complianceKeys,
        List<MaterialKey> materialKeys,
        List<RegulatoryProgram> programs,
        List<RulePack> rulePacks,
        List<RegulatoryCitation> citations,
        List<FactDefinition> factDefinitions,
        List<FactRequirement> factRequirements,
        List<RegulatoryMapping> mappings,
        List<SdsReference> sdsReferences,
        HashSet<string> vocabularyTypeKeys)
    {
        public List<VocabularyTerm> Terms { get; } = terms;

        public List<VocabularyAlias> Aliases { get; } = aliases;

        public List<ComplianceKey> ComplianceKeys { get; } = complianceKeys;

        public List<MaterialKey> MaterialKeys { get; } = materialKeys;

        public List<RegulatoryProgram> Programs { get; } = programs;

        public List<RulePack> RulePacks { get; } = rulePacks;

        public List<RegulatoryCitation> Citations { get; } = citations;

        public List<FactDefinition> FactDefinitions { get; } = factDefinitions;

        public List<FactRequirement> FactRequirements { get; } = factRequirements;

        public List<RegulatoryMapping> Mappings { get; } = mappings;

        public List<SdsReference> SdsReferences { get; } = sdsReferences;

        public HashSet<string> VocabularyTypeKeys { get; } = vocabularyTypeKeys;
    }
}
