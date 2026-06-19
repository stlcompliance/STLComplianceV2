using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class EmploymentApplicationService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly IReadOnlySet<string> AllowedControls = ValueSet("text", "email", "phone", "textarea", "date", "select", "multi_select", "number", "yes_no");
    private static readonly IReadOnlySet<string> AllowedMappingModes = ValueSet("create", "eventual", "unmapped");

    public async Task<IReadOnlyList<EmploymentApplicationTemplateResponse>> ListTemplatesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await EnsureDefaultTemplateAsync(tenantId, cancellationToken);

        var templates = await db.EmploymentApplicationTemplates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.TemplateKey)
            .ThenByDescending(x => x.Version)
            .ThenByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        return templates.Select(MapTemplateResponse).ToArray();
    }

    public async Task<EmploymentApplicationTemplateResponse> GetTemplateAsync(
        Guid tenantId,
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var template = await LoadTemplateAsync(tenantId, templateId, cancellationToken);
        return MapTemplateResponse(template);
    }

    public async Task<EmploymentApplicationTemplateResponse> CreateTemplateAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        EmploymentApplicationTemplateCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.TemplateName, request.Title, request.SubmitLabel, request.Fields);
        request = NormalizeRequest(request);

        var templateKey = NormalizeTemplateKey(request.TemplateKey);
        var templateName = NormalizeTemplateName(request.TemplateName);
        if (await db.EmploymentApplicationTemplates.AnyAsync(x => x.TenantId == tenantId && x.TemplateKey == templateKey, cancellationToken))
        {
            throw new StlApiException("employment_application.template.duplicate_key", "A template with this key already exists.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new EmploymentApplicationTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TemplateKey = templateKey,
            TemplateName = templateName,
            Title = request.Title.Trim(),
            Subtitle = request.Subtitle.Trim(),
            SubmitLabel = request.SubmitLabel.Trim(),
            Version = 1,
            Status = EmploymentApplicationTemplateStatuses.Draft,
            PublicToken = GenerateToken(),
            PublicLinkExpiresAt = request.PublicLinkExpiresAt ?? now.AddDays(90),
            TemplateJson = SerializeRequest(ToUpsertRequest(request)),
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByPersonId = actorPersonId,
            UpdatedByPersonId = actorPersonId,
        };

        db.EmploymentApplicationTemplates.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteWithMetadataAsync(
            "staffarr.employment_application_template.create",
            tenantId,
            actorUserId,
            "employment_application_template",
            entity.Id.ToString(),
            "success",
            JsonSerializer.Serialize(new { template = MapTemplateResponse(entity) }, JsonOptions),
            cancellationToken: cancellationToken);

        return MapTemplateResponse(entity);
    }

    public async Task<EmploymentApplicationTemplateResponse> UpdateTemplateAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid templateId,
        EmploymentApplicationTemplateUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.TemplateName, request.Title, request.SubmitLabel, request.Fields);
        request = NormalizeRequest(request);

        var template = await LoadTemplateAsync(tenantId, templateId, cancellationToken);
        if (!string.Equals(template.Status, EmploymentApplicationTemplateStatuses.Draft, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("employment_application.template.locked", "Only draft templates can be edited. Clone a new version to make changes.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var before = MapTemplateResponse(template);
        template.TemplateName = NormalizeTemplateName(request.TemplateName);
        template.Title = request.Title.Trim();
        template.Subtitle = request.Subtitle.Trim();
        template.SubmitLabel = request.SubmitLabel.Trim();
        template.PublicLinkExpiresAt = request.PublicLinkExpiresAt ?? template.PublicLinkExpiresAt ?? now.AddDays(90);
        template.TemplateJson = SerializeRequest(request);
        template.UpdatedByPersonId = actorPersonId;
        template.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        var after = MapTemplateResponse(template);
        await audit.WriteWithMetadataAsync(
            "staffarr.employment_application_template.update",
            tenantId,
            actorUserId,
            "employment_application_template",
            template.Id.ToString(),
            "success",
            JsonSerializer.Serialize(new { before, after }, JsonOptions),
            cancellationToken: cancellationToken);

        return after;
    }

    public async Task<EmploymentApplicationTemplateResponse> PublishTemplateAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var template = await LoadTemplateAsync(tenantId, templateId, cancellationToken);
        if (!string.Equals(template.Status, EmploymentApplicationTemplateStatuses.Draft, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("employment_application.template.locked", "Only draft templates can be published.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var otherPublishedTemplates = await db.EmploymentApplicationTemplates
            .Where(x => x.TenantId == tenantId && x.TemplateKey == template.TemplateKey && x.Id != template.Id && x.Status == EmploymentApplicationTemplateStatuses.Published)
            .ToListAsync(cancellationToken);

        foreach (var published in otherPublishedTemplates)
        {
            published.Status = EmploymentApplicationTemplateStatuses.Retired;
            published.RetiredAt = now;
            published.RetiredByPersonId = actorPersonId;
            published.UpdatedByPersonId = actorPersonId;
            published.UpdatedAt = now;
        }

        template.Status = EmploymentApplicationTemplateStatuses.Published;
        template.PublishedAt = template.PublishedAt ?? now;
        template.PublishedByPersonId = template.PublishedByPersonId ?? actorPersonId;
        template.UpdatedByPersonId = actorPersonId;
        template.UpdatedAt = now;
        template.PublicToken = string.IsNullOrWhiteSpace(template.PublicToken) ? GenerateToken() : template.PublicToken;
        template.PublicLinkExpiresAt ??= now.AddDays(90);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteWithMetadataAsync(
            "staffarr.employment_application_template.publish",
            tenantId,
            actorUserId,
            "employment_application_template",
            template.Id.ToString(),
            "success",
            JsonSerializer.Serialize(new { template = MapTemplateResponse(template) }, JsonOptions),
            cancellationToken: cancellationToken);

        return MapTemplateResponse(template);
    }

    public async Task<EmploymentApplicationTemplateResponse> CloneTemplateAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var source = await GetTemplateAsync(tenantId, templateId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var nextVersion = await GetNextVersionAsync(tenantId, source.TemplateKey, cancellationToken);

        var clone = new EmploymentApplicationTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TemplateKey = source.TemplateKey,
            TemplateName = source.TemplateName,
            Title = source.Title,
            Subtitle = source.Subtitle,
            SubmitLabel = source.SubmitLabel,
            Version = nextVersion,
            Status = EmploymentApplicationTemplateStatuses.Draft,
            PublicToken = GenerateToken(),
            PublicLinkExpiresAt = source.PublicLinkExpiresAt ?? now.AddDays(90),
            TemplateJson = SerializeRequest(new EmploymentApplicationTemplateUpsertRequest(
                source.TemplateName,
                source.Title,
                source.Subtitle,
                source.SubmitLabel,
                source.PublicLinkExpiresAt,
                source.Fields.Select(MapFieldRequest).ToArray())),
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByPersonId = actorPersonId,
            UpdatedByPersonId = actorPersonId,
        };

        db.EmploymentApplicationTemplates.Add(clone);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteWithMetadataAsync(
            "staffarr.employment_application_template.clone",
            tenantId,
            actorUserId,
            "employment_application_template",
            clone.Id.ToString(),
            "success",
            JsonSerializer.Serialize(new { sourceTemplateId = templateId, clonedTemplateId = clone.Id }, JsonOptions),
            cancellationToken: cancellationToken);

        return MapTemplateResponse(clone);
    }

    public async Task<PublicEmploymentApplicationResponse> GetPublicTemplateAsync(
        string publicToken,
        CancellationToken cancellationToken = default)
    {
        var template = await db.EmploymentApplicationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PublicToken == NormalizeToken(publicToken), cancellationToken);

        if (!IsPubliclyAccessible(template))
        {
            throw new StlApiException("employment_application.not_found", "Public employment application was not found.", 404);
        }

        return MapPublicResponse(template!);
    }

    public async Task<EmploymentApplicationSubmissionResponse> SubmitPublicAsync(
        string publicToken,
        SubmitEmploymentApplicationRequest request,
        string? sourceIpAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var template = await db.EmploymentApplicationTemplates
            .FirstOrDefaultAsync(x => x.PublicToken == NormalizeToken(publicToken), cancellationToken);

        if (!IsPubliclyAccessible(template))
        {
            throw new StlApiException("employment_application.not_found", "Public employment application was not found.", 404);
        }

        var publicTemplate = template!;
        var publicRequest = DeserializeRequest(publicTemplate.TemplateJson);
        var normalizedAnswers = NormalizeAnswers(request.Answers);
        var mapping = BuildMapping(publicRequest, normalizedAnswers);

        var now = DateTimeOffset.UtcNow;
        var submission = new EmploymentApplicationSubmission
        {
            Id = Guid.NewGuid(),
            TenantId = publicTemplate.TenantId,
            EmploymentApplicationTemplateId = publicTemplate.Id,
            TemplateKey = publicTemplate.TemplateKey,
            TemplateVersion = publicTemplate.Version,
            Status = "received",
            ApplicantDisplayName = BuildApplicantName(mapping.CreateRequest, mapping.CreateRequest.PrimaryEmail),
            ApplicantEmail = mapping.CreateRequest.PrimaryEmail,
            RawAnswersJson = JsonSerializer.Serialize(normalizedAnswers, JsonOptions),
            CreateRequestJson = JsonSerializer.Serialize(mapping.CreateRequest, JsonOptions),
            EventualProfileJson = JsonSerializer.Serialize(mapping.EventualProfileValues, JsonOptions),
            SourceIpAddress = NormalizeOptionalText(sourceIpAddress, 64),
            UserAgent = NormalizeOptionalText(userAgent, 256),
            SubmittedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.EmploymentApplicationSubmissions.Add(submission);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            await audit.WriteWithMetadataAsync(
                "staffarr.employment_application.submit",
                publicTemplate.TenantId,
                null,
                "employment_application_submission",
                submission.Id.ToString(),
                "success",
                JsonSerializer.Serialize(new
                {
                    templateId = publicTemplate.Id,
                    templateKey = publicTemplate.TemplateKey,
                    templateVersion = publicTemplate.Version,
                    submissionId = submission.Id,
                    createRequest = mapping.CreateRequest,
                    eventualProfileValues = mapping.EventualProfileValues,
                }, JsonOptions),
                cancellationToken: cancellationToken);
        }
        catch
        {
            // Keep public intake resilient even if audit emission blips.
        }

        return MapSubmissionResponse(submission, mapping.CreateValues, mapping.EventualProfileValues);
    }

    public async Task<IReadOnlyList<EmploymentApplicationSubmissionListItemResponse>> ListSubmissionsAsync(
        Guid tenantId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = Math.Clamp(limit <= 0 ? 20 : limit, 1, 100);

        return await db.EmploymentApplicationSubmissions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.SubmittedAt)
            .Take(normalizedLimit)
            .Select(x => new EmploymentApplicationSubmissionListItemResponse(
                x.Id,
                x.CreatedPersonId,
                x.CreatedCandidateId,
                x.Status,
                x.ApplicantDisplayName,
                x.ApplicantEmail,
                x.TemplateKey,
                x.TemplateVersion,
                x.SubmittedAt))
            .ToListAsync(cancellationToken);
    }

    private async Task EnsureDefaultTemplateAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var exists = await db.EmploymentApplicationTemplates.AnyAsync(x => x.TenantId == tenantId, cancellationToken);
        if (exists)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var request = CreateDefaultRequest(now.AddDays(90));
        db.EmploymentApplicationTemplates.Add(new EmploymentApplicationTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TemplateKey = "employment-application",
            TemplateName = "Employment application",
            Title = request.Title.Trim(),
            Subtitle = request.Subtitle.Trim(),
            SubmitLabel = request.SubmitLabel.Trim(),
            Version = 1,
            Status = EmploymentApplicationTemplateStatuses.Draft,
            PublicToken = GenerateToken(),
            PublicLinkExpiresAt = request.PublicLinkExpiresAt,
            TemplateJson = SerializeRequest(request),
            CreatedAt = now,
            UpdatedAt = now,
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<EmploymentApplicationTemplate> LoadTemplateAsync(
        Guid tenantId,
        Guid templateId,
        CancellationToken cancellationToken)
    {
        var template = await db.EmploymentApplicationTemplates
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == templateId, cancellationToken);

        if (template is null)
        {
            throw new StlApiException("employment_application.template.not_found", "Employment application template was not found.", 404);
        }

        return template;
    }

    private async Task<int> GetNextVersionAsync(
        Guid tenantId,
        string templateKey,
        CancellationToken cancellationToken)
    {
        var currentMax = await db.EmploymentApplicationTemplates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TemplateKey == templateKey)
            .Select(x => (int?)x.Version)
            .MaxAsync(cancellationToken);

        return (currentMax ?? 0) + 1;
    }

    private static EmploymentApplicationTemplateResponse MapTemplateResponse(EmploymentApplicationTemplate template)
    {
        var request = DeserializeRequest(template.TemplateJson);
        return new EmploymentApplicationTemplateResponse(
            template.Id,
            template.TemplateKey,
            template.TemplateName,
            request.Title.Trim(),
            request.Subtitle.Trim(),
            request.SubmitLabel.Trim(),
            template.Version,
            template.Status,
            template.PublicToken,
            template.PublicLinkExpiresAt,
            request.Fields.Select(MapFieldResponse).ToArray(),
            template.CreatedAt,
            template.UpdatedAt,
            template.PublishedAt,
            template.RetiredAt);
    }

    private static PublicEmploymentApplicationResponse MapPublicResponse(EmploymentApplicationTemplate template)
    {
        var request = DeserializeRequest(template.TemplateJson);
        return new PublicEmploymentApplicationResponse(
            template.Id,
            template.TemplateKey,
            template.TemplateName,
            request.Title.Trim(),
            request.Subtitle.Trim(),
            request.SubmitLabel.Trim(),
            template.Version,
            request.Fields.Select(MapFieldResponse).ToArray(),
            template.PublicLinkExpiresAt ?? DateTimeOffset.UtcNow.AddDays(90),
            template.CreatedAt,
            template.UpdatedAt);
    }

    private static EmploymentApplicationTemplateFieldResponse MapFieldResponse(EmploymentApplicationFieldRequest field) =>
        new(
            field.FieldKey.Trim(),
            field.Label.Trim(),
            field.Control.Trim().ToLowerInvariant(),
            field.Required,
            field.MappingMode.Trim().ToLowerInvariant(),
            string.IsNullOrWhiteSpace(field.TargetFieldKey) ? null : field.TargetFieldKey.Trim(),
            string.IsNullOrWhiteSpace(field.HelpText) ? null : field.HelpText.Trim(),
            string.IsNullOrWhiteSpace(field.Placeholder) ? null : field.Placeholder.Trim(),
            field.Options ?? []);

    private static EmploymentApplicationFieldRequest MapFieldRequest(EmploymentApplicationTemplateFieldResponse field) =>
        new(
            field.FieldKey,
            field.Label,
            field.Control,
            field.Required,
            field.MappingMode,
            field.TargetFieldKey,
            field.HelpText,
            field.Placeholder,
            field.Options);

    private static EmploymentApplicationTemplateUpsertRequest ToUpsertRequest(EmploymentApplicationTemplateCreateRequest request) =>
        new(
            request.TemplateName,
            request.Title,
            request.Subtitle,
            request.SubmitLabel,
            request.PublicLinkExpiresAt,
            request.Fields);

    private static EmploymentApplicationTemplateCreateRequest NormalizeRequest(EmploymentApplicationTemplateCreateRequest request) =>
        new(
            NormalizeTemplateKey(request.TemplateKey),
            request.TemplateName.Trim(),
            request.Title.Trim(),
            request.Subtitle.Trim(),
            request.SubmitLabel.Trim(),
            request.PublicLinkExpiresAt,
            request.Fields.Select(field =>
            {
                var normalizedFieldKey = NormalizeFieldKey(field.FieldKey);
                var normalizedTargetFieldKey = string.IsNullOrWhiteSpace(field.TargetFieldKey) ? null : NormalizeFieldKey(field.TargetFieldKey);
                return new EmploymentApplicationFieldRequest(
                    normalizedFieldKey,
                    field.Label.Trim(),
                    field.Control.Trim().ToLowerInvariant(),
                    field.Required,
                    field.MappingMode.Trim().ToLowerInvariant(),
                    normalizedTargetFieldKey,
                    string.IsNullOrWhiteSpace(field.HelpText) ? null : field.HelpText.Trim(),
                    string.IsNullOrWhiteSpace(field.Placeholder) ? null : field.Placeholder.Trim(),
                    field.Options?.Select(option => new EmploymentApplicationFieldOptionRequest(option.Value.Trim(), option.Label.Trim())).ToArray() ?? []);
            }).ToArray());

    private static EmploymentApplicationTemplateUpsertRequest CreateDefaultRequest(DateTimeOffset publicLinkExpiresAt) =>
        new(
            "Employment application",
            "Employment application",
            "Tell us a little about yourself so we can build your applicant profile in StaffArr.",
            "Submit application",
            publicLinkExpiresAt,
            [
                new EmploymentApplicationFieldRequest("legal_first_name", "Legal first name", "text", true, "create", "legal_first_name", "Matches the person record.", "First name"),
                new EmploymentApplicationFieldRequest("legal_last_name", "Legal last name", "text", true, "create", "legal_last_name", "Matches the person record.", "Last name"),
                new EmploymentApplicationFieldRequest("primary_email", "Email", "email", true, "create", "primary_email", "This is the login/contact email.", "name@example.com"),
                new EmploymentApplicationFieldRequest("primary_phone", "Phone", "phone", false, "create", "primary_phone", null, "(555) 123-4567"),
                new EmploymentApplicationFieldRequest(
                    "work_relationship_type",
                    "Work relationship",
                    "select",
                    true,
                    "create",
                    "work_relationship_type",
                    "Defaults to employee if left blank.",
                    null,
                    StaffArrControlledFieldCatalog.WorkRelationshipOptions
                        .Where(option => option.Value is "employee" or "contractor" or "temp" or "vendor_worker")
                        .Select(option => new EmploymentApplicationFieldOptionRequest(option.Value, option.Label))
                        .ToArray()),
                new EmploymentApplicationFieldRequest(
                    "employment_type",
                    "Employment type",
                    "select",
                    false,
                    "create",
                    "employment_type",
                    "Optional classification for downstream profile setup.",
                    null,
                    StaffArrControlledFieldCatalog.EmploymentTypeOptions
                        .Select(option => new EmploymentApplicationFieldOptionRequest(option.Value, option.Label))
                        .ToArray()),
                new EmploymentApplicationFieldRequest("expected_start_date", "Desired start date", "date", false, "create", "expected_start_date", null, null),
                new EmploymentApplicationFieldRequest("preferred_name", "Preferred name", "text", false, "eventual", "preferred_name", "Queued for profile review after submission.", "What should we call you?"),
                new EmploymentApplicationFieldRequest("pronouns", "Pronouns", "text", false, "eventual", "pronouns", null, "they/them"),
                new EmploymentApplicationFieldRequest("job_title", "Position applying for", "text", false, "eventual", "job_title", "Stored for eventual profile review.", "Role title"),
                new EmploymentApplicationFieldRequest("application_notes", "Anything else we should know?", "textarea", false, "eventual", null, null, "Tell us about your experience, availability, or anything we should review."),
            ]);

    private static EmploymentApplicationSubmissionResponse MapSubmissionResponse(
        EmploymentApplicationSubmission submission,
        IReadOnlyDictionary<string, string?> createValues,
        IReadOnlyDictionary<string, string?> eventualProfileValues) =>
        new(
            submission.Id,
            submission.CreatedPersonId,
            submission.CreatedCandidateId,
            submission.Status,
            submission.ApplicantDisplayName,
            submission.ApplicantEmail,
            submission.TemplateKey,
            submission.TemplateVersion,
            submission.SubmittedAt,
            createValues,
            eventualProfileValues);

    private static void ValidateRequest(
        string templateName,
        string title,
        string submitLabel,
        IReadOnlyList<EmploymentApplicationFieldRequest> fields)
    {
        if (string.IsNullOrWhiteSpace(templateName))
        {
            throw new StlApiException("employment_application.validation", "Template name is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new StlApiException("employment_application.validation", "Title is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(submitLabel))
        {
            throw new StlApiException("employment_application.validation", "Submit label is required.", 400);
        }

        if (fields.Count == 0)
        {
            throw new StlApiException("employment_application.validation", "At least one field is required.", 400);
        }

        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in fields)
        {
            var normalizedFieldKey = NormalizeFieldKey(field.FieldKey);
            if (!keys.Add(normalizedFieldKey))
            {
                throw new StlApiException("employment_application.validation", $"Duplicate field key '{field.FieldKey}'.", 400);
            }

            if (!AllowedControls.Contains(field.Control.Trim().ToLowerInvariant()))
            {
                throw new StlApiException("employment_application.validation", $"Control '{field.Control}' is not supported.", 400);
            }

            if (!AllowedMappingModes.Contains(field.MappingMode.Trim().ToLowerInvariant()))
            {
                throw new StlApiException("employment_application.validation", $"Mapping mode '{field.MappingMode}' is not supported.", 400);
            }

            if (field.TargetFieldKey is not null)
            {
                _ = NormalizeFieldKey(field.TargetFieldKey);
            }

            var normalizedControl = field.Control.Trim().ToLowerInvariant();
            if ((normalizedControl is "select" or "multi_select") && (field.Options is null || field.Options.Count == 0))
            {
                throw new StlApiException("employment_application.validation", $"Select field '{field.FieldKey}' requires options.", 400);
            }
        }
    }

    private static string GenerateToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();

    private static bool IsPubliclyAccessible(EmploymentApplicationTemplate? template) =>
        template is not null
        && string.Equals(template.Status, EmploymentApplicationTemplateStatuses.Published, StringComparison.OrdinalIgnoreCase)
        && template.PublicLinkExpiresAt is DateTimeOffset expiresAt
        && expiresAt > DateTimeOffset.UtcNow;

    private static string NormalizeTemplateKey(string templateKey) =>
        NormalizeToken(templateKey).Replace(" ", "-");

    private static string NormalizeTemplateName(string templateName) =>
        string.IsNullOrWhiteSpace(templateName) ? string.Empty : templateName.Trim();

    private static EmploymentApplicationTemplateUpsertRequest NormalizeRequest(EmploymentApplicationTemplateUpsertRequest request) =>
        new(
            request.TemplateName.Trim(),
            request.Title.Trim(),
            request.Subtitle.Trim(),
            request.SubmitLabel.Trim(),
            request.PublicLinkExpiresAt,
            request.Fields.Select(field =>
            {
                var normalizedFieldKey = NormalizeFieldKey(field.FieldKey);
                var normalizedTargetFieldKey = string.IsNullOrWhiteSpace(field.TargetFieldKey) ? null : NormalizeFieldKey(field.TargetFieldKey);
                return new EmploymentApplicationFieldRequest(
                    normalizedFieldKey,
                    field.Label.Trim(),
                    field.Control.Trim().ToLowerInvariant(),
                    field.Required,
                    field.MappingMode.Trim().ToLowerInvariant(),
                    normalizedTargetFieldKey,
                    string.IsNullOrWhiteSpace(field.HelpText) ? null : field.HelpText.Trim(),
                    string.IsNullOrWhiteSpace(field.Placeholder) ? null : field.Placeholder.Trim(),
                    field.Options?.Select(option => new EmploymentApplicationFieldOptionRequest(option.Value.Trim(), option.Label.Trim())).ToArray() ?? []);
            }).ToArray());

    private static string NormalizeToken(string token) =>
        string.IsNullOrWhiteSpace(token) ? string.Empty : token.Trim().ToLowerInvariant();

    private static string NormalizeFieldKey(string fieldKey)
    {
        if (string.IsNullOrWhiteSpace(fieldKey))
        {
            throw new StlApiException("employment_application.validation", "Field key is required.", 400);
        }

        var trimmed = fieldKey.Trim();
        var builder = new System.Text.StringBuilder(trimmed.Length + 8);
        var previousWasSeparator = false;

        foreach (var current in trimmed)
        {
            if (char.IsLetterOrDigit(current))
            {
                if (char.IsUpper(current) && builder.Length > 0 && !previousWasSeparator)
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(current));
                previousWasSeparator = false;
                continue;
            }

            if (builder.Length > 0 && !previousWasSeparator)
            {
                builder.Append('_');
                previousWasSeparator = true;
            }
        }

        var normalized = builder.ToString().Trim('_');
        if (normalized.Length == 0)
        {
            throw new StlApiException("employment_application.validation", "Field key is required.", 400);
        }

        if (normalized.Length > 64)
        {
            throw new StlApiException("employment_application.validation", "Field key must be 64 characters or fewer.", 400);
        }

        return normalized;
    }

    private static string SerializeRequest(EmploymentApplicationTemplateUpsertRequest request) =>
        JsonSerializer.Serialize(request, JsonOptions);

    private static EmploymentApplicationTemplateUpsertRequest DeserializeRequest(string json) =>
        NormalizeRequest(
            JsonSerializer.Deserialize<EmploymentApplicationTemplateUpsertRequest>(json, JsonOptions)
            ?? CreateDefaultRequest(DateTimeOffset.UtcNow.AddDays(90)));

    private static Dictionary<string, string?> NormalizeAnswers(IReadOnlyDictionary<string, string?> answers) =>
        answers
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Key))
            .ToDictionary(entry => entry.Key.Trim(), entry => NormalizeOptionalText(entry.Value, 2048), StringComparer.OrdinalIgnoreCase);

    private static MappingResult BuildMapping(
        EmploymentApplicationTemplateUpsertRequest request,
        IReadOnlyDictionary<string, string?> answers)
    {
        var createValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var eventualProfileValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in request.Fields)
        {
            if (!answers.TryGetValue(field.FieldKey, out var value) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var targetFieldKey = field.TargetFieldKey?.Trim();
            var mappingMode = field.MappingMode.Trim().ToLowerInvariant();
            if (mappingMode == "create" && !string.IsNullOrWhiteSpace(targetFieldKey))
            {
                createValues[targetFieldKey!] = value;
            }
            else if (mappingMode == "eventual")
            {
                eventualProfileValues[targetFieldKey ?? field.FieldKey.Trim()] = value;
            }
            else
            {
                eventualProfileValues[field.FieldKey.Trim()] = value;
            }
        }

        var createRequest = new CreateStaffPersonRequest(
            PrimaryEmail: GetRequiredValue(createValues, "primary_email", "primary_email")!,
            LegalFirstName: GetOptionalValue(createValues, "legal_first_name"),
            LegalMiddleName: GetOptionalValue(createValues, "legal_middle_name"),
            LegalLastName: GetOptionalValue(createValues, "legal_last_name"),
            PreferredName: GetOptionalValue(createValues, "preferred_name"),
            Pronouns: GetOptionalValue(createValues, "pronouns"),
            GivenName: GetOptionalValue(createValues, "given_name") ?? GetOptionalValue(createValues, "legal_first_name"),
            FamilyName: GetOptionalValue(createValues, "family_name") ?? GetOptionalValue(createValues, "legal_last_name"),
            EmploymentStatus: "applicant",
            WorkRelationshipType: GetOptionalValue(createValues, "work_relationship_type") ?? "employee",
            EmploymentType: GetOptionalValue(createValues, "employment_type"),
            WorkerCategory: GetOptionalValue(createValues, "worker_category"),
            FlsaStatus: GetOptionalValue(createValues, "flsa_status"),
            PositionNumber: GetOptionalValue(createValues, "position_number"),
            CurrentEmploymentAction: GetOptionalValue(createValues, "current_employment_action"),
            CurrentEmploymentActionAt: ParseDateTimeOffset(GetOptionalValue(createValues, "current_employment_action_at")),
            LeaveStatus: GetOptionalValue(createValues, "leave_status"),
            EligibleForRehire: ParseLooseBoolean(GetOptionalValue(createValues, "eligible_for_rehire")) ?? true,
            AlternateEmail: GetOptionalValue(createValues, "alternate_email"),
            PrimaryPhone: GetOptionalValue(createValues, "primary_phone"),
            AlternatePhone: GetOptionalValue(createValues, "alternate_phone"),
            WorkPhone: GetOptionalValue(createValues, "work_phone"),
            StartDate: ParseDateTimeOffset(GetOptionalValue(createValues, "start_date") ?? GetOptionalValue(createValues, "expected_start_date")),
            ExpectedStartDate: ParseDateTimeOffset(GetOptionalValue(createValues, "expected_start_date")),
            PrimaryOrgUnitId: null,
            SiteOrgUnitId: null,
            DepartmentOrgUnitId: null,
            TeamOrgUnitId: null,
            PositionOrgUnitId: null,
            ManagerPersonId: null,
            JobTitle: GetOptionalValue(createValues, "job_title"),
            HomeBaseLocationId: null,
            CanLogin: ParseLooseBoolean(GetOptionalValue(createValues, "can_login")) ?? false);

        return new MappingResult(
            createRequest,
            createValues,
            eventualProfileValues);
    }

    private static string BuildApplicantName(CreateStaffPersonRequest request, string? fallbackEmail)
    {
        var parts = new[] { request.LegalFirstName, request.LegalLastName }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .ToArray();

        var name = string.Join(' ', parts);
        return string.IsNullOrWhiteSpace(name) ? fallbackEmail ?? "Applicant" : name;
    }

    private static string? GetOptionalValue(IReadOnlyDictionary<string, string?> values, string key) =>
        values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value!.Trim() : null;

    private static string? GetRequiredValue(IReadOnlyDictionary<string, string?> values, string key, string fieldName)
    {
        var value = GetOptionalValue(values, key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("employment_application.validation", $"Field '{fieldName}' is required.", 400);
        }

        return value;
    }

    private static DateTimeOffset? ParseDateTimeOffset(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value, out var parsedOffset))
        {
            return parsedOffset;
        }

        return DateTime.TryParse(value, out var parsed) ? new DateTimeOffset(DateTime.SpecifyKind(parsed, DateTimeKind.Utc)) : null;
    }

    private static bool? ParseLooseBoolean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (bool.TryParse(value, out var parsedBoolean))
        {
            return parsedBoolean;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "yes" or "y" or "1" or "on" => true,
            "no" or "n" or "0" or "off" => false,
            _ => null,
        };
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

    private static IReadOnlySet<string> ValueSet(params string[] values) =>
        new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);

    private sealed record MappingResult(
        CreateStaffPersonRequest CreateRequest,
        IReadOnlyDictionary<string, string?> CreateValues,
        IReadOnlyDictionary<string, string?> EventualProfileValues);
}
