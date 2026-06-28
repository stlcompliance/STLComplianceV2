using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainArrTenantSettingsService(
    TrainArrDbContext db,
    ITrainArrAuditService audit)
{
    public const int CurrentSchemaVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly HashSet<string> AssignmentPriorities = new(StringComparer.OrdinalIgnoreCase)
    {
        "low",
        "normal",
        "high",
        "critical"
    };

    private static readonly HashSet<string> ProgramVersionPolicies = new(StringComparer.OrdinalIgnoreCase)
    {
        "none",
        "new_assignments_only",
        "incomplete_assignments_only",
        "expired_or_incomplete",
        "all_active_assignments"
    };

    private static readonly HashSet<string> CompletionModes = new(StringComparer.OrdinalIgnoreCase)
    {
        "self",
        "trainer",
        "manager",
        "evaluator",
        "blended"
    };

    private static readonly HashSet<string> CompletionEditPolicies = new(StringComparer.OrdinalIgnoreCase)
    {
        "locked",
        "admin_correction_only",
        "trainer_correction_allowed",
        "manager_correction_allowed"
    };

    private static readonly HashSet<string> AllowedEvidenceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "pdf",
        "image",
        "video",
        "external_url",
        "signature",
        "form",
        "completion_certificate",
        "evaluation_sheet",
        "signoff_form",
        "practical_demo",
        "attendance_roster",
        "quiz_result"
    };

    private static readonly HashSet<string> WorkBlockModes = new(StringComparer.OrdinalIgnoreCase)
    {
        "none",
        "warn",
        "manager_override_required",
        "hard_block"
    };

    private static readonly HashSet<string> ExternalRecordConfidenceValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "low",
        "medium",
        "high",
        "verified"
    };

    private static readonly HashSet<string> EvaluatorConflictPolicies = new(StringComparer.OrdinalIgnoreCase)
    {
        "allow",
        "warn",
        "block",
        "admin_override"
    };

    private static readonly HashSet<string> TrainerRosterSources = new(StringComparer.OrdinalIgnoreCase)
    {
        "staffarr_role",
        "trainarr_qualification",
        "both"
    };

    private static readonly HashSet<string> CitationDisplayModes = new(StringComparer.OrdinalIgnoreCase)
    {
        "hidden",
        "admin_only",
        "trainer_and_admin",
        "all_users"
    };

    public async Task<TrainArrTenantSettingsResponse> GetOrCreateAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TrainArrTenantSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            entity = CreateEntity(tenantId, updatedByPersonId: null);
            db.TrainArrTenantSettings.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
        }

        return MapResponse(entity);
    }

    public TrainArrTenantSettingsDefaultsResponse GetDefaults() =>
        new("trainarr", "tenant", CurrentSchemaVersion, CreateDefaultPayload());

    public async Task<TrainArrTenantSettingsPayload> LoadPayloadAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TrainArrTenantSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return entity is null ? CreateDefaultPayload() : DeserializePayload(entity);
    }

    public async Task<TrainArrTenantSettingsResponse> PutAsync(
        Guid tenantId,
        Guid actorPersonId,
        UpdateTrainArrTenantSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeAndValidate(request.Settings);
        var entity = await db.TrainArrTenantSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = CreateEntity(tenantId, actorPersonId);
            db.TrainArrTenantSettings.Add(entity);
        }
        else if (request.RowVersion.HasValue && request.RowVersion.Value != entity.RowVersion)
        {
            throw new StlApiException(
                "trainarr_tenant_settings.concurrency_conflict",
                "TrainArr tenant settings were updated by another administrator. Reload before saving.",
                409);
        }

        entity.SettingsJson = SerializePayload(normalized);
        entity.SchemaVersion = CurrentSchemaVersion;
        entity.UpdatedAt = now;
        entity.UpdatedByPersonId = actorPersonId;
        entity.RowVersion++;

        await db.SaveChangesAsync(cancellationToken);
        EnqueueSettingsUpdatedEvent(entity, actorPersonId, now);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "trainarr.tenant_settings.update",
            tenantId,
            actorPersonId,
            "trainarr_tenant_settings",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    public async Task<TrainArrTenantSettingsResponse> PatchAsync(
        Guid tenantId,
        Guid actorPersonId,
        PatchTrainArrTenantSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var current = await db.TrainArrTenantSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var currentPayload = current is null ? CreateDefaultPayload() : DeserializePayload(current);
        var currentNode = JsonSerializer.SerializeToNode(currentPayload, JsonOptions)
            ?? throw new StlApiException(
                "trainarr_tenant_settings.patch_failed",
                "Could not prepare current TrainArr tenant settings for patching.",
                400);
        var patchNode = JsonNode.Parse(request.Settings.GetRawText());
        if (patchNode is not JsonObject patchObject)
        {
            throw new StlApiException(
                "trainarr_tenant_settings.patch_invalid",
                "Patch settings must be a JSON object containing TrainArr tenant setting groups.",
                400);
        }

        MergeInto((JsonObject)currentNode, patchObject);
        var patched = currentNode.Deserialize<TrainArrTenantSettingsPayload>(JsonOptions)
            ?? throw new StlApiException(
                "trainarr_tenant_settings.patch_invalid",
                "Patch did not produce a valid TrainArr tenant settings payload.",
                400);

        return await PutAsync(
            tenantId,
            actorPersonId,
            new UpdateTrainArrTenantSettingsRequest(patched, request.RowVersion),
            cancellationToken);
    }

    public static TrainArrTenantSettingsPayload NormalizeAndValidate(
        TrainArrTenantSettingsPayload payload)
    {
        if (payload is null)
        {
            throw Validation("settings", "TrainArr tenant settings payload is required.");
        }

        var assignment = payload.Assignment;
        RequireNonNegative(assignment.DefaultAssignmentDueDays, "assignment.defaultAssignmentDueDays");
        RequireNonNegative(assignment.AssignmentGracePeriodDays, "assignment.assignmentGracePeriodDays");
        var assignmentPriority = RequireEnum(
            assignment.AssignmentPriorityDefault,
            AssignmentPriorities,
            "assignment.assignmentPriorityDefault");

        var versioning = payload.ProgramVersioning;
        var programPolicy = RequireEnum(
            versioning.ProgramVersionChangePolicy,
            ProgramVersionPolicies,
            "programVersioning.programVersionChangePolicy");

        var certifications = payload.Certifications;
        if (certifications.DefaultCertificateValidityDays is int validityDays)
        {
            RequirePositive(validityDays, "certifications.defaultCertificateValidityDays");
        }

        RequireNonNegative(certifications.DefaultRenewalWindowDays, "certifications.defaultRenewalWindowDays");
        var defaultExpirationWarnings = NormalizePositiveDayArray(
            certifications.DefaultExpirationWarningDays,
            "certifications.defaultExpirationWarningDays");
        var certificateNumberFormat = RequireTrimmed(
            certifications.CertificateNumberFormat,
            "certifications.certificateNumberFormat",
            minLength: 4,
            maxLength: 128);
        if (!certificateNumberFormat.Contains("{sequence}", StringComparison.OrdinalIgnoreCase))
        {
            throw Validation(
                "certifications.certificateNumberFormat",
                "Certificate number format must include a {sequence} token.");
        }

        var certificateDisplayNameFormat = NormalizeOptional(
            certifications.CertificateDisplayNameFormat,
            "certifications.certificateDisplayNameFormat",
            maxLength: 128);

        var completion = payload.CompletionSignoff;
        var completionMode = RequireEnum(
            completion.DefaultCompletionMode,
            CompletionModes,
            "completionSignoff.defaultCompletionMode");
        RequireNonNegative(completion.BackdatedCompletionMaxDays, "completionSignoff.backdatedCompletionMaxDays");
        var completionEditPolicy = RequireEnum(
            completion.CompletionEditPolicy,
            CompletionEditPolicies,
            "completionSignoff.completionEditPolicy");

        var evaluations = payload.Evaluations;
        if (evaluations.DefaultPassingScorePercent is < 0 or > 100)
        {
            throw Validation(
                "evaluations.defaultPassingScorePercent",
                "Default passing score percent must be between 0 and 100.");
        }

        RequireNonNegative(evaluations.MaxRetakeAttempts, "evaluations.maxRetakeAttempts");
        RequireNonNegative(evaluations.RetakeCooldownHours, "evaluations.retakeCooldownHours");

        var remediation = payload.Remediation;
        RequireNonNegative(remediation.IncidentRetrainingDefaultDueDays, "remediation.incidentRetrainingDefaultDueDays");
        RequirePositive(remediation.RepeatIncidentEscalationThreshold, "remediation.repeatIncidentEscalationThreshold");
        RequirePositive(remediation.RepeatIncidentLookbackDays, "remediation.repeatIncidentLookbackDays");

        var evidence = payload.EvidenceRecords;
        var evidenceTypes = NormalizeEvidenceTypes(evidence.AllowedEvidenceTypes);
        RequirePositive(evidence.MaxEvidenceFileSizeMb, "evidenceRecords.maxEvidenceFileSizeMb");
        RequireNonNegative(evidence.EvidenceRetentionYears, "evidenceRecords.evidenceRetentionYears");
        if (!evidence.AllowExternalEvidenceUrl
            && evidenceTypes.Contains("external_url", StringComparer.OrdinalIgnoreCase))
        {
            throw Validation(
                "evidenceRecords.allowedEvidenceTypes",
                "Remove external_url from allowed evidence types or enable external evidence URLs.");
        }

        var notifications = payload.Notifications;
        var dueSoonDays = NormalizePositiveDayArray(
            notifications.DueSoonReminderDays,
            "notifications.dueSoonReminderDays");
        RequirePositive(notifications.OverdueReminderCadenceDays, "notifications.overdueReminderCadenceDays");
        var certificateWarningDays = NormalizePositiveDayArray(
            notifications.CertificateExpirationWarningDays,
            "notifications.certificateExpirationWarningDays");

        var enforcement = payload.Enforcement;
        var workBlockMode = RequireEnum(
            enforcement.DefaultWorkBlockMode,
            WorkBlockModes,
            "enforcement.defaultWorkBlockMode");
        if (enforcement.AllowManagerOverrideOfBlock)
        {
            RequirePositive(enforcement.OverrideDurationHours, "enforcement.overrideDurationHours");
        }
        else
        {
            RequireNonNegative(enforcement.OverrideDurationHours, "enforcement.overrideDurationHours");
        }

        var externalTraining = payload.ExternalTraining;
        var trustedProviderIds = NormalizeTrustedProviderIds(externalTraining.TrustedProviderIds);
        var confidence = RequireEnum(
            externalTraining.ExternalRecordConfidenceDefault,
            ExternalRecordConfidenceValues,
            "externalTraining.externalRecordConfidenceDefault");

        var trainerEvaluator = payload.TrainersEvaluators;
        RequireNonNegative(
            trainerEvaluator.TrainerQualificationRequiredDays,
            "trainersEvaluators.trainerQualificationRequiredDays");
        var conflictPolicy = RequireEnum(
            trainerEvaluator.EvaluatorConflictPolicy,
            EvaluatorConflictPolicies,
            "trainersEvaluators.evaluatorConflictPolicy");
        var rosterSource = RequireEnum(
            trainerEvaluator.TrainerRosterSource,
            TrainerRosterSources,
            "trainersEvaluators.trainerRosterSource");

        var complianceCore = payload.ComplianceCore;
        if (complianceCore.RequireComplianceCoreProgramMapping
            && !complianceCore.AllowUnmappedInternalPrograms)
        {
            throw Validation(
                "complianceCore.allowUnmappedInternalPrograms",
                "Allow unmapped internal programs when Compliance Core mappings are required so internal-only training can still publish.");
        }

        var citationDisplayMode = RequireEnum(
            complianceCore.CitationDisplayMode,
            CitationDisplayModes,
            "complianceCore.citationDisplayMode");

        var auditCorrection = payload.AuditCorrection;
        RequireNonNegative(auditCorrection.AuditEventRetentionYears, "auditCorrection.auditEventRetentionYears");

        return payload with
        {
            Assignment = assignment with { AssignmentPriorityDefault = assignmentPriority },
            ProgramVersioning = versioning with { ProgramVersionChangePolicy = programPolicy },
            Certifications = certifications with
            {
                DefaultExpirationWarningDays = defaultExpirationWarnings,
                CertificateNumberFormat = certificateNumberFormat,
                CertificateDisplayNameFormat = certificateDisplayNameFormat
            },
            CompletionSignoff = completion with
            {
                DefaultCompletionMode = completionMode,
                CompletionEditPolicy = completionEditPolicy
            },
            EvidenceRecords = evidence with { AllowedEvidenceTypes = evidenceTypes },
            Notifications = notifications with
            {
                DueSoonReminderDays = dueSoonDays,
                CertificateExpirationWarningDays = certificateWarningDays
            },
            Enforcement = enforcement with { DefaultWorkBlockMode = workBlockMode },
            ExternalTraining = externalTraining with
            {
                TrustedProviderIds = trustedProviderIds,
                ExternalRecordConfidenceDefault = confidence
            },
            TrainersEvaluators = trainerEvaluator with
            {
                EvaluatorConflictPolicy = conflictPolicy,
                TrainerRosterSource = rosterSource
            },
            ComplianceCore = complianceCore with { CitationDisplayMode = citationDisplayMode }
        };
    }

    public static TrainArrTenantSettingsPayload CreateDefaultPayload() =>
        new(
            new TrainArrAssignmentSettings(
                true,
                true,
                true,
                true,
                true,
                true,
                false,
                14,
                3,
                "normal"),
            new TrainArrProgramVersioningSettings(
                "expired_or_incomplete",
                true,
                false,
                true,
                true,
                true),
            new TrainArrCertificationLifecycleSettings(
                365,
                60,
                [90, 60, 30, 14, 7, 1],
                true,
                true,
                true,
                "TRN-{tenantCode}-{yyyy}-{sequence}",
                true,
                null),
            new TrainArrCompletionSignoffSettings(
                "trainer",
                true,
                true,
                false,
                true,
                true,
                true,
                30,
                true,
                "admin_correction_only"),
            new TrainArrEvaluationScoringSettings(
                80,
                true,
                3,
                24,
                false,
                false,
                false,
                true,
                true),
            new TrainArrRemediationSettings(
                true,
                7,
                true,
                false,
                2,
                180,
                true,
                false),
            new TrainArrEvidenceRecordSettings(
                false,
                [
                    "pdf",
                    "image",
                    "video",
                    "external_url",
                    "signature",
                    "form",
                    "completion_certificate",
                    "evaluation_sheet",
                    "signoff_form",
                    "practical_demo",
                    "attendance_roster",
                    "quiz_result"
                ],
                25,
                7,
                true,
                false,
                false,
                true,
                true),
            new TrainArrNotificationEscalationSettings(
                true,
                true,
                [14, 7, 1],
                true,
                7,
                true,
                true,
                true,
                true,
                [90, 60, 30, 14, 7, 1]),
            new TrainArrEnforcementSettings(
                true,
                true,
                "manager_override_required",
                true,
                true,
                24,
                true),
            new TrainArrExternalTrainingSettings(
                true,
                true,
                true,
                [],
                true,
                "medium"),
            new TrainArrTrainerEvaluatorSettings(
                true,
                0,
                false,
                true,
                false,
                "warn",
                "both"),
            new TrainArrComplianceCoreSettings(
                true,
                false,
                true,
                "trainer_and_admin",
                true,
                true),
            new TrainArrAuditCorrectionSettings(
                true,
                true,
                true,
                true,
                true,
                7));

    private static TrainArrTenantSettings CreateEntity(Guid tenantId, Guid? updatedByPersonId)
    {
        var now = DateTimeOffset.UtcNow;
        return new TrainArrTenantSettings
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SettingsJson = SerializePayload(CreateDefaultPayload()),
            SchemaVersion = CurrentSchemaVersion,
            CreatedAt = now,
            UpdatedAt = now,
            UpdatedByPersonId = updatedByPersonId,
            RowVersion = 1
        };
    }

    private static TrainArrTenantSettingsResponse MapResponse(TrainArrTenantSettings entity) =>
        new(
            "trainarr",
            "tenant",
            entity.SchemaVersion,
            DeserializePayload(entity),
            entity.UpdatedByPersonId.HasValue ? "TrainArr administrator" : null,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.RowVersion);

    private static TrainArrTenantSettingsPayload DeserializePayload(TrainArrTenantSettings entity)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<TrainArrTenantSettingsPayload>(
                entity.SettingsJson,
                JsonOptions);
            return payload is null ? CreateDefaultPayload() : NormalizeAndValidate(payload);
        }
        catch (JsonException)
        {
            throw new StlApiException(
                "trainarr_tenant_settings.payload_invalid",
                "Stored TrainArr tenant settings payload is invalid.",
                500);
        }
    }

    private static string SerializePayload(TrainArrTenantSettingsPayload payload) =>
        JsonSerializer.Serialize(payload, JsonOptions);

    private void EnqueueSettingsUpdatedEvent(
        TrainArrTenantSettings entity,
        Guid actorPersonId,
        DateTimeOffset occurredAt)
    {
        var payload = new TrainingDomainEventPayload(
            actorPersonId,
            "trainarr_tenant_settings",
            entity.Id,
            "TrainArr tenant settings updated.",
            occurredAt);

        db.TrainingDomainEvents.Add(new TrainingDomainEvent
        {
            Id = Guid.NewGuid(),
            TenantId = entity.TenantId,
            EventKind = TrainingDomainEventKinds.TenantSettingsUpdated,
            IdempotencyKey = $"{TrainingDomainEventKinds.TenantSettingsUpdated}:{entity.Id:D}:{entity.RowVersion}",
            StaffarrPersonId = actorPersonId,
            RelatedEntityType = payload.RelatedEntityType,
            RelatedEntityId = payload.RelatedEntityId,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOptions),
            ProcessingStatus = TrainingDomainEventStatuses.Pending,
            AttemptCount = 0,
            CreatedAt = occurredAt,
            UpdatedAt = occurredAt,
        });
    }

    private static IReadOnlyList<int> NormalizePositiveDayArray(
        IReadOnlyList<int> values,
        string field)
    {
        if (values is null || values.Count == 0)
        {
            throw Validation(field, "At least one reminder day is required.");
        }

        if (values.Any(value => value <= 0))
        {
            throw Validation(field, "Reminder days must be positive integers.");
        }

        return values
            .Distinct()
            .OrderByDescending(value => value)
            .ToList();
    }

    private static IReadOnlyList<string> NormalizeEvidenceTypes(IReadOnlyList<string> values)
    {
        if (values is null || values.Count == 0)
        {
            throw Validation("evidenceRecords.allowedEvidenceTypes", "At least one evidence type is required.");
        }

        var normalized = values
            .Select(value => value.Trim().ToLowerInvariant())
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value)
            .ToList();

        var invalid = normalized
            .Where(value => !AllowedEvidenceTypes.Contains(value))
            .ToList();
        if (invalid.Count > 0)
        {
            throw Validation(
                "evidenceRecords.allowedEvidenceTypes",
                $"Allowed evidence types must be one of: {string.Join(", ", AllowedEvidenceTypes.OrderBy(x => x))}.");
        }

        return normalized;
    }

    private static IReadOnlyList<string> NormalizeTrustedProviderIds(IReadOnlyList<string> values)
    {
        if (values is null)
        {
            return [];
        }

        var normalized = values
            .Select(value => value.Trim())
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value)
            .ToList();

        if (normalized.Any(value => value.Length > 128))
        {
            throw Validation(
                "externalTraining.trustedProviderIds",
                "Trusted provider references must be 128 characters or fewer.");
        }

        return normalized;
    }

    private static string RequireEnum(string value, HashSet<string> allowed, string field)
    {
        var normalized = RequireTrimmed(value, field, minLength: 1, maxLength: 64).ToLowerInvariant();
        if (!allowed.Contains(normalized))
        {
            throw Validation(field, $"Value must be one of: {string.Join(", ", allowed.OrderBy(x => x))}.");
        }

        return normalized;
    }

    private static string RequireTrimmed(string value, string field, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw Validation(field, "Value is required.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length < minLength || trimmed.Length > maxLength)
        {
            throw Validation(field, $"Value must be between {minLength} and {maxLength} characters.");
        }

        return trimmed;
    }

    private static string? NormalizeOptional(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw Validation(field, $"Value must be {maxLength} characters or fewer.");
        }

        return trimmed;
    }

    private static void RequirePositive(int value, string field)
    {
        if (value <= 0)
        {
            throw Validation(field, "Value must be greater than 0.");
        }
    }

    private static void RequireNonNegative(int value, string field)
    {
        if (value < 0)
        {
            throw Validation(field, "Value must be zero or greater.");
        }
    }

    private static StlApiException Validation(string field, string message) =>
        new("trainarr_tenant_settings.validation", $"{field}: {message}", 400);

    private static void MergeInto(JsonObject target, JsonObject patch)
    {
        foreach (var item in patch)
        {
            if (item.Value is null)
            {
                target.Remove(item.Key);
                continue;
            }

            if (target[item.Key] is JsonObject targetChild && item.Value is JsonObject patchChild)
            {
                MergeInto(targetChild, patchChild);
                continue;
            }

            target[item.Key] = item.Value.DeepClone();
        }
    }
}
