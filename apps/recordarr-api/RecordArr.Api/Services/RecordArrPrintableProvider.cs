using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using RecordArr.Api.Data;
using RecordArr.Api.Models;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Print;

namespace RecordArr.Api.Services;

public sealed class RecordArrPrintableProvider(
    RecordArrStore store,
    IPdfRenderer pdfRenderer,
    IRecordArchiveClient archiveClient) : IPrintableProvider
{
    private static readonly HashSet<string> SupportedTemplates = new(StringComparer.OrdinalIgnoreCase)
    {
        "recordarr.document.original",
        "recordarr.document.copy",
        "recordarr.document.redacted_copy",
        "recordarr.document.cover_sheet",
        "recordarr.document.index",
        "recordarr.record.packet",
        "recordarr.retention.audit_packet",
        "recordarr.evidence.binder",
        "recordarr.document.history",
        "recordarr.document.chain_of_custody",
    };

    public bool CanHandle(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template) =>
        string.Equals(context.Product.ProductKey, "recordarr", StringComparison.OrdinalIgnoreCase)
        && string.Equals(template.ProductKey, "recordarr", StringComparison.OrdinalIgnoreCase)
        && SupportedTemplates.Contains(template.TemplateKey)
        && string.Equals(request.SourceEntityType, "record", StringComparison.OrdinalIgnoreCase);

    public Task<StlPrintPreviewResult> BuildPreviewAsync(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template,
        CancellationToken cancellationToken)
    {
        var model = BuildModel(context, request, template);
        var html = RenderHtml(model, template, includeLongSections: true);

        return Task.FromResult(
            new StlPrintPreviewResult(
                model.DocumentTitle,
                model.SourceDisplayRef,
                template.TemplateKey,
                request.TemplateVersion ?? template.Version,
                html,
                PreviewRoute: null,
                model.Warnings,
                model.MissingRequirements));
    }

    public async Task<StlGeneratedPrintFile> GeneratePdfAsync(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template,
        string action,
        CancellationToken cancellationToken)
    {
        var generated = await BuildGeneratedFileAsync(
            context,
            request,
            template,
            cancellationToken);

        return generated;
    }

    public async Task<StlPrintArchiveResult> ArchiveOfficialAsync(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template,
        CancellationToken cancellationToken)
    {
        var model = BuildModel(context, request, template);
        var file = await BuildGeneratedFileAsync(
            context,
            request,
            template,
            cancellationToken,
            model);

        var archiveRequest = new StlRecordArchiveRequest(
            context.TenantId,
            context.Product.ProductKey,
            request.SourceEntityType ?? "record",
            request.SourceEntityId ?? string.Empty,
            model.SourceDisplayRef,
            model.DocumentClass,
            model.DocumentType,
            model.DocumentSubtype,
            model.DocumentTitle,
            template.TemplateKey,
            request.TemplateVersion ?? template.Version,
            DateTimeOffset.UtcNow,
            context.RequestedByPersonId,
            template.RetentionClass,
            file.ContentHash ?? string.Empty,
            file.FileName,
            file.Content);

        var receipt = await archiveClient.ArchiveAsync(archiveRequest, cancellationToken);

        return new StlPrintArchiveResult(
            model.DocumentTitle,
            model.SourceDisplayRef,
            template.TemplateKey,
            request.TemplateVersion ?? template.Version,
            receipt.RecordArrDocumentId,
            receipt.FileName ?? file.FileName,
            receipt.ContentHash ?? file.ContentHash,
            model.Warnings,
            model.MissingRequirements);
    }

    private async Task<StlGeneratedPrintFile> BuildGeneratedFileAsync(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template,
        CancellationToken cancellationToken,
        RecordArrPrintModel? prebuiltModel = null)
    {
        var model = prebuiltModel ?? BuildModel(context, request, template);
        var html = RenderHtml(model, template, includeLongSections: true);
        var renderable = new StlRenderablePrintDocument(
            model.DocumentTitle,
            model.SourceDisplayRef,
            template.TemplateKey,
            request.TemplateVersion ?? template.Version,
            html,
            BuildFileName(model.RecordNumber, template.TemplateKey),
            "application/pdf",
            model.Warnings,
            model.MissingRequirements);

        return await pdfRenderer.RenderPdfAsync(renderable, cancellationToken);
    }

    private RecordArrPrintModel BuildModel(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template)
    {
        var recordId = request.SourceEntityId ?? throw new StlApiException(
            "print.invalid_request",
            "Record print requests require a sourceEntityId.",
            400);
        var record = store.GetRecord(context.Principal, recordId);
        if (record is null)
        {
            throw new StlApiException(
                "print.source_not_found",
                $"Record '{recordId}' was not found or is not available for printing.",
                404);
        }

        var retention = store.GetRetentionStatus(context.TenantId.ToString(), record.RecordId);
        var files = store.GetFiles(context.Principal, record.RecordId);
        var packages = store.GetPackages(context.TenantId.ToString())
            .Where(package => package.RecordRefs.Contains(record.RecordId, StringComparer.OrdinalIgnoreCase))
            .OrderByDescending(package => package.CreatedAt)
            .ToArray();
        var controlledDocuments = store.GetControlledDocuments(context.TenantId.ToString())
            .Where(document => string.Equals(document.RecordId, record.RecordId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(document => document.DocumentNumber)
            .ToArray();
        var legalHold = store.GetLegalHolds(context.TenantId.ToString())
            .FirstOrDefault(hold =>
                string.Equals(hold.Status, "active", StringComparison.OrdinalIgnoreCase)
                && hold.RecordRefs.Contains(record.RecordId, StringComparer.OrdinalIgnoreCase));
        var redactions = store.GetRedactions(context.TenantId.ToString())
            .Where(redaction =>
                string.Equals(redaction.SourceRecordId, record.RecordId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(redaction.RedactedRecordId, record.RecordId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(redaction => redaction.RedactedAt)
            .ToArray();
        var links = store.GetRecordLinks(record.RecordId);
        var comments = store.GetRecordComments(record.RecordId);
        var accessLogs = store.GetAccessLogs(context.TenantId.ToString(), record.RecordId)
            .OrderByDescending(log => log.OccurredAt)
            .Take(12)
            .ToArray();
        var options = ParseOptions(request.OptionsJson);
        var generatedByDisplayName = ResolveGeneratedByDisplayName(context.Principal);
        var tenantDisplayName = ResolveOptionValue(options, "tenantDisplayName") ?? "Current tenant workspace";
        var sourceDisplayRef = string.IsNullOrWhiteSpace(request.SourceDisplayRef)
            ? record.RecordNumber
            : request.SourceDisplayRef.Trim();
        var templateVersion = request.TemplateVersion ?? template.Version;
        var isRedactedCopy = string.Equals(template.DocumentStatus, StlPrintDocumentStatuses.Redacted, StringComparison.OrdinalIgnoreCase)
            || string.Equals(template.TemplateKey, "recordarr.document.redacted_copy", StringComparison.OrdinalIgnoreCase);

        var fileSummaries = files.Select(file =>
            isRedactedCopy
                ? $"Protected file on record ({file.MimeType}, {FormatSize(file.SizeBytes)})"
                : $"{file.OriginalFilename} ({file.MimeType}, {FormatSize(file.SizeBytes)})")
            .ToArray();
        var packageSummaries = packages.Select(package =>
            new RecordArrPrintPackageSummary(
                package.PackageNumber,
                package.Title,
                HumanizeToken(package.Status),
                package.SourceProduct,
                package.CreatedAt))
            .ToArray();
        var controlledDocumentSummaries = controlledDocuments.Select(document =>
            $"{document.DocumentNumber}: {document.Title} ({HumanizeToken(document.Status)})")
            .ToArray();
        var linkSummaries = links.Select(link => BuildLinkSummary(context.Principal, link)).ToArray();
        var accessSummaries = accessLogs.Select(log =>
            new RecordArrPrintEventSummary(
                HumanizeToken(log.Action),
                BuildAccessEventDetail(log),
                ResolveActorLabel(context, log.ActorPersonId, log.ActorServiceClientId),
                log.OccurredAt))
            .ToArray();
        var auditSummaries = record.AuditTrail
            .OrderByDescending(entry => entry.OccurredAt)
            .Take(10)
            .Select(entry =>
                new RecordArrPrintEventSummary(
                    HumanizeToken(entry.Action),
                    entry.Details,
                    ResolveActorLabel(context, entry.ActorPersonId, actorServiceClientId: null),
                    entry.OccurredAt))
            .ToArray();
        var redactionSummaries = redactions.Select(redaction =>
            new RecordArrPrintRedactionSummary(
                HumanizeToken(redaction.RedactionReason),
                HumanizeToken(redaction.Status),
                redaction.RedactedAt,
                redaction.RedactionRules.Count == 0
                    ? "Protected fields withheld"
                    : string.Join(", ", redaction.RedactionRules.Select(HumanizeToken))))
            .ToArray();

        var warnings = new List<string>();
        var missingRequirements = new List<string>();

        if (legalHold is not null)
        {
            warnings.Add($"Legal hold {legalHold.HoldNumber} is active for this record.");
        }

        if (isRedactedCopy && redactionSummaries.Length == 0)
        {
            warnings.Add("No explicit redaction event is on file for this record; sensitive fields are still withheld in the generated copy.");
        }

        if (template.TemplateKey is "recordarr.record.packet" or "recordarr.evidence.binder" or "recordarr.retention.audit_packet")
        {
            if (files.Count == 0)
            {
                missingRequirements.Add("No file renditions are attached to this record.");
            }

            if (packages.Length == 0)
            {
                missingRequirements.Add("No related record package is linked to this record.");
            }
        }

        if (template.TemplateKey is "recordarr.document.history" or "recordarr.document.chain_of_custody" or "recordarr.record.packet")
        {
            if (accessSummaries.Length == 0)
            {
                missingRequirements.Add("No access trail entries are available for this record.");
            }
        }

        var safeDescription = isRedactedCopy
            ? "Sensitive source description withheld for the redacted copy."
            : record.Description;
        var safeSourceDisplayName = isRedactedCopy
            ? "Protected source reference"
            : record.SourceObjectDisplayName;

        return new RecordArrPrintModel(
            TenantDisplayName: tenantDisplayName,
            ProductDisplayName: context.Product.DisplayName,
            SourceDisplayRef: sourceDisplayRef,
            DocumentTitle: ResolveDocumentTitle(template.TemplateKey, record.Title),
            TemplateKey: template.TemplateKey,
            TemplateVersion: templateVersion,
            DocumentStatus: template.DocumentStatus,
            GeneratedAtUtc: DateTimeOffset.UtcNow,
            GeneratedByDisplayName: generatedByDisplayName,
            RecordNumber: record.RecordNumber,
            RecordTitle: record.Title,
            Description: safeDescription,
            RecordType: record.RecordType,
            DocumentClass: record.DocumentClass,
            DocumentType: record.DocumentType,
            DocumentSubtype: record.DocumentSubtype,
            Classification: record.Classification,
            SourceProduct: record.SourceProduct,
            SourceDisplayName: safeSourceDisplayName,
            CurrentFileName: isRedactedCopy ? "Protected file attached to record" : record.CurrentFileName,
            VersionNumber: record.VersionNumber,
            UploadedAt: record.UploadedAt,
            EffectiveAt: record.EffectiveAt,
            ExpiresAt: record.ExpiresAt,
            RetentionStatus: retention?.Status ?? "unassigned",
            RetentionPolicyRef: retention?.RetentionPolicyRef,
            ActiveLegalHoldLabel: legalHold?.HoldNumber,
            Tags: record.Tags.Select(HumanizeToken).ToArray(),
            FileSummaries: fileSummaries,
            PackageSummaries: packageSummaries,
            ControlledDocumentSummaries: controlledDocumentSummaries,
            LinkSummaries: linkSummaries,
            AccessEvents: accessSummaries,
            AuditEvents: auditSummaries,
            Redactions: redactionSummaries,
            CommentCount: comments.Count,
            Warnings: warnings.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            MissingRequirements: missingRequirements.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static string RenderHtml(
        RecordArrPrintModel model,
        StlPrintTemplateDescriptor template,
        bool includeLongSections)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<article class=\"recordarr-print-document\">");
        builder.AppendLine($"<header><h1>{Encode(model.DocumentTitle)}</h1>");
        builder.AppendLine(
            $"<p>{Encode(model.TenantDisplayName)} · {Encode(model.ProductDisplayName)} · {Encode(model.SourceDisplayRef)}</p>");
        builder.AppendLine(
            $"<p>{Encode(ResolveStatusBanner(model.DocumentStatus))} · Template {Encode(model.TemplateKey)} v{Encode(model.TemplateVersion)} · Generated {Encode(FormatDateTime(model.GeneratedAtUtc))} by {Encode(model.GeneratedByDisplayName)}</p></header>");

        AppendSection(
            builder,
            "Record summary",
            [
                $"Record number: {model.RecordNumber}",
                $"Title: {model.RecordTitle}",
                $"Record type: {HumanizeToken(model.RecordType)}",
                $"Document path: {HumanizeToken(model.DocumentClass)} / {HumanizeToken(model.DocumentType)} / {HumanizeToken(model.DocumentSubtype)}",
                $"Classification: {HumanizeToken(model.Classification)}",
                $"Source: {HumanizeToken(model.SourceProduct)} · {model.SourceDisplayName}",
                $"Current file: {model.CurrentFileName}",
                $"Version: v{model.VersionNumber}",
                $"Uploaded: {FormatDateTime(model.UploadedAt)}",
                $"Effective: {FormatDateTime(model.EffectiveAt)}",
                $"Expires: {FormatDateTime(model.ExpiresAt)}",
            ]);

        if (!string.IsNullOrWhiteSpace(model.Description))
        {
            AppendSection(builder, "Document notes", [model.Description]);
        }

        AppendSection(
            builder,
            "Governance",
            [
                $"Retention status: {HumanizeToken(model.RetentionStatus)}",
                $"Retention policy: {model.RetentionPolicyRef ?? "Not assigned"}",
                $"Legal hold: {model.ActiveLegalHoldLabel ?? "None active"}",
                $"Comment count: {model.CommentCount}",
                $"Tags: {(model.Tags.Count == 0 ? "None" : string.Join(", ", model.Tags))}",
            ]);

        if (template.TemplateKey is "recordarr.document.original" or "recordarr.document.copy" or "recordarr.document.redacted_copy" or "recordarr.document.cover_sheet" or "recordarr.document.index")
        {
            AppendOptionalSection(builder, "File inventory", model.FileSummaries);
            AppendOptionalSection(builder, "Links", model.LinkSummaries);
        }

        if (template.TemplateKey is "recordarr.record.packet" or "recordarr.evidence.binder" or "recordarr.retention.audit_packet")
        {
            AppendOptionalSection(builder, "File inventory", model.FileSummaries);
            AppendOptionalSection(
                builder,
                "Packages",
                model.PackageSummaries.Select(summary =>
                    $"{summary.PackageNumber}: {summary.Title} ({summary.Status}, {HumanizeToken(summary.SourceProduct)}) · {FormatDateTime(summary.CreatedAt)}"));
            AppendOptionalSection(builder, "Controlled documents", model.ControlledDocumentSummaries);
        }

        if (template.TemplateKey is "recordarr.document.history" or "recordarr.document.chain_of_custody" or "recordarr.record.packet" or "recordarr.retention.audit_packet")
        {
            AppendOptionalSection(
                builder,
                "Access trail",
                model.AccessEvents.Select(eventSummary =>
                    $"{FormatDateTime(eventSummary.OccurredAt)} · {eventSummary.Title} · {eventSummary.Detail} · {eventSummary.ActorLabel}"));
            AppendOptionalSection(
                builder,
                "Audit trail",
                model.AuditEvents.Select(eventSummary =>
                    $"{FormatDateTime(eventSummary.OccurredAt)} · {eventSummary.Title} · {eventSummary.Detail} · {eventSummary.ActorLabel}"));
        }

        if (includeLongSections || template.TemplateKey is "recordarr.document.redacted_copy" or "recordarr.evidence.binder")
        {
            AppendOptionalSection(
                builder,
                "Redactions",
                model.Redactions.Select(redaction =>
                    $"{FormatDateTime(redaction.RedactedAt)} · {redaction.Reason} · {redaction.Status} · {redaction.RuleSummary}"));
        }

        AppendOptionalSection(builder, "Warnings", model.Warnings);
        AppendOptionalSection(builder, "Missing requirements", model.MissingRequirements);
        builder.AppendLine("</article>");
        return builder.ToString();
    }

    private static void AppendSection(StringBuilder builder, string title, IEnumerable<string> lines)
    {
        builder.AppendLine($"<section><h2>{Encode(title)}</h2><ul>");
        foreach (var line in lines.Where(line => !string.IsNullOrWhiteSpace(line)))
        {
            builder.AppendLine($"<li>{Encode(line)}</li>");
        }

        builder.AppendLine("</ul></section>");
    }

    private static void AppendOptionalSection(
        StringBuilder builder,
        string title,
        IEnumerable<string> lines)
    {
        var normalized = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();
        if (normalized.Length == 0)
        {
            return;
        }

        AppendSection(builder, title, normalized);
    }

    private static string BuildLinkSummary(ClaimsPrincipal principal, RecordArrRecordLinkResponse link)
    {
        if (!string.IsNullOrWhiteSpace(link.LinkedRecordId))
        {
            return $"{HumanizeToken(link.LinkType)} link to another authorized record.";
        }

        return $"{HumanizeToken(link.LinkType)} link recorded.";
    }

    private static string BuildAccessEventDetail(RecordArrAccessLogResponse log)
    {
        var detailParts = new List<string> { HumanizeToken(log.Result) };
        if (!string.IsNullOrWhiteSpace(log.ReasonCode))
        {
            detailParts.Add(HumanizeToken(log.ReasonCode));
        }

        if (!string.IsNullOrWhiteSpace(log.ExternalShareId))
        {
            detailParts.Add("external share");
        }

        return string.Join(", ", detailParts);
    }

    private static string ResolveGeneratedByDisplayName(ClaimsPrincipal principal)
    {
        var displayName = principal.Identity?.Name
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Name)
            ?? principal.FindFirstValue(ClaimTypes.Name);
        return string.IsNullOrWhiteSpace(displayName) ? "Authorized user" : displayName.Trim();
    }

    private static string ResolveActorLabel(
        StlPrintProviderContext context,
        string? actorPersonId,
        string? actorServiceClientId)
    {
        if (!string.IsNullOrWhiteSpace(actorServiceClientId))
        {
            return "System service";
        }

        if (!string.IsNullOrWhiteSpace(actorPersonId) &&
            Guid.TryParse(actorPersonId, out var actorGuid) &&
            actorGuid == context.RequestedByPersonId)
        {
            return ResolveGeneratedByDisplayName(context.Principal);
        }

        if (!string.IsNullOrWhiteSpace(actorPersonId))
        {
            return "Authorized user";
        }

        return "System";
    }

    private static string ResolveDocumentTitle(string templateKey, string recordTitle) =>
        templateKey switch
        {
            "recordarr.document.original" => $"{recordTitle} official original",
            "recordarr.document.copy" => $"{recordTitle} copy",
            "recordarr.document.redacted_copy" => $"{recordTitle} redacted copy",
            "recordarr.document.cover_sheet" => $"{recordTitle} cover sheet",
            "recordarr.document.index" => $"{recordTitle} document index",
            "recordarr.record.packet" => $"{recordTitle} record packet",
            "recordarr.retention.audit_packet" => $"{recordTitle} retention audit packet",
            "recordarr.evidence.binder" => $"{recordTitle} evidence binder",
            "recordarr.document.history" => $"{recordTitle} document history",
            "recordarr.document.chain_of_custody" => $"{recordTitle} chain of custody",
            _ => recordTitle,
        };

    private static string ResolveStatusBanner(string documentStatus) =>
        documentStatus switch
        {
            StlPrintDocumentStatuses.Official => "Official original",
            StlPrintDocumentStatuses.Copy => "Copy",
            StlPrintDocumentStatuses.Redacted => "Redacted copy",
            StlPrintDocumentStatuses.Draft => "Draft",
            _ => "Working copy",
        };

    private static Dictionary<string, JsonElement>? ParseOptions(string? optionsJson)
    {
        if (string.IsNullOrWhiteSpace(optionsJson))
        {
            return null;
        }

        using var document = JsonDocument.Parse(optionsJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return document.RootElement.EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.OrdinalIgnoreCase);
    }

    private static string? ResolveOptionValue(Dictionary<string, JsonElement>? options, string key)
    {
        if (options is null || !options.TryGetValue(key, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static string BuildFileName(string recordNumber, string templateKey)
    {
        var stem = Regex.Replace(recordNumber.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        var suffix = templateKey[(templateKey.LastIndexOf('.') + 1)..].Replace('_', '-');
        return $"{stem}-{suffix}.pdf";
    }

    private static string HumanizeToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "n/a";
        }

        var normalized = value.Trim().Replace('_', ' ').Replace('-', ' ');
        normalized = Regex.Replace(normalized, "\\s+", " ");
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(normalized.ToLowerInvariant());
    }

    private static string FormatDateTime(DateTimeOffset? value) =>
        value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture) : "Not set";

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024d:0.#} KB";
        }

        return $"{bytes / (1024d * 1024d):0.#} MB";
    }

    private static string Encode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);

    private sealed record RecordArrPrintModel(
        string TenantDisplayName,
        string ProductDisplayName,
        string SourceDisplayRef,
        string DocumentTitle,
        string TemplateKey,
        string TemplateVersion,
        string DocumentStatus,
        DateTimeOffset GeneratedAtUtc,
        string GeneratedByDisplayName,
        string RecordNumber,
        string RecordTitle,
        string Description,
        string RecordType,
        string DocumentClass,
        string DocumentType,
        string DocumentSubtype,
        string Classification,
        string SourceProduct,
        string SourceDisplayName,
        string CurrentFileName,
        int VersionNumber,
        DateTimeOffset UploadedAt,
        DateTimeOffset? EffectiveAt,
        DateTimeOffset? ExpiresAt,
        string RetentionStatus,
        string? RetentionPolicyRef,
        string? ActiveLegalHoldLabel,
        IReadOnlyList<string> Tags,
        IReadOnlyList<string> FileSummaries,
        IReadOnlyList<RecordArrPrintPackageSummary> PackageSummaries,
        IReadOnlyList<string> ControlledDocumentSummaries,
        IReadOnlyList<string> LinkSummaries,
        IReadOnlyList<RecordArrPrintEventSummary> AccessEvents,
        IReadOnlyList<RecordArrPrintEventSummary> AuditEvents,
        IReadOnlyList<RecordArrPrintRedactionSummary> Redactions,
        int CommentCount,
        IReadOnlyList<string> Warnings,
        IReadOnlyList<string> MissingRequirements);

    private sealed record RecordArrPrintPackageSummary(
        string PackageNumber,
        string Title,
        string Status,
        string SourceProduct,
        DateTimeOffset CreatedAt);

    private sealed record RecordArrPrintEventSummary(
        string Title,
        string Detail,
        string ActorLabel,
        DateTimeOffset OccurredAt);

    private sealed record RecordArrPrintRedactionSummary(
        string Reason,
        string Status,
        DateTimeOffset RedactedAt,
        string RuleSummary);
}
