using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Hosting;

namespace STLCompliance.Shared.Print;

public static class StlPrintRuntimeServiceCollectionExtensions
{
    public static IServiceCollection AddStlPrintRuntime(this IServiceCollection services)
    {
        services.AddScoped<IPrintTemplateCatalog, StlDefaultPrintTemplateCatalog>();
        services.AddScoped<IPrintPermissionEvaluator, StlDefaultPrintPermissionEvaluator>();
        services.AddScoped<IPrintExportAuditWriter, NullPrintExportAuditWriter>();
        services.AddScoped<StlPrintLogService>();
        return services;
    }
}

public sealed class StlDefaultPrintTemplateCatalog(ProductDescriptor product) : IPrintTemplateCatalog
{
    public IReadOnlyList<StlPrintTemplateDescriptor> ListTemplates() =>
    [
        BuildTemplate(
            StlPrintDocumentStatuses.WorkingCopy,
            StlPrintFormats.HtmlPrint,
            "Current page working copy",
            "Browser-printable working copy for the current page.",
            requiresOfficialIssue: false,
            requiresArchive: false,
            requiresReprintReason: false),
        BuildTemplate(
            StlPrintDocumentStatuses.Draft,
            StlPrintFormats.HtmlPrint,
            "Current page draft preview",
            "Browser-printable draft preview for the current page.",
            requiresOfficialIssue: false,
            requiresArchive: false,
            requiresReprintReason: false),
        BuildTemplate(
            StlPrintDocumentStatuses.Official,
            StlPrintFormats.Pdf,
            "Current page official PDF",
            "Server-generated official PDF for the current page.",
            requiresOfficialIssue: true,
            requiresArchive: false,
            requiresReprintReason: false),
        BuildTemplate(
            StlPrintDocumentStatuses.Copy,
            StlPrintFormats.Pdf,
            "Current page copy PDF",
            "Server-generated copy PDF for the current page.",
            requiresOfficialIssue: true,
            requiresArchive: false,
            requiresReprintReason: true),
        BuildTemplate(
            StlPrintDocumentStatuses.Redacted,
            StlPrintFormats.Pdf,
            "Current page redacted PDF",
            "Server-generated redacted PDF for the current page.",
            requiresOfficialIssue: true,
            requiresArchive: false,
            requiresReprintReason: true)
    ];

    public StlPrintTemplateDescriptor? GetTemplate(string templateKey) =>
        ListTemplates().FirstOrDefault(template =>
            string.Equals(template.TemplateKey, templateKey, StringComparison.OrdinalIgnoreCase));

    private StlPrintTemplateDescriptor BuildTemplate(
        string documentStatus,
        string format,
        string name,
        string description,
        bool requiresOfficialIssue,
        bool requiresArchive,
        bool requiresReprintReason)
    {
        var fileStem = documentStatus.Replace('_', '-');
        return new StlPrintTemplateDescriptor(
            TemplateKey: $"{product.ProductKey}.current_page.{documentStatus}",
            ProductKey: product.ProductKey,
            Name: name,
            Description: description,
            Version: "1",
            DataContractVersion: "1",
            Format: format,
            PaperSize: format == StlPrintFormats.BadgePdf ? "badge" : "letter",
            Orientation: "portrait",
            DocumentStatus: documentStatus,
            IsSystemTemplate: true,
            TenantOverrideAllowed: false,
            RequiresArchive: requiresArchive,
            RequiresOfficialIssue: requiresOfficialIssue,
            RequiresReprintReason: requiresReprintReason,
            RetentionClass: requiresOfficialIssue ? "official_output" : null,
            DefaultFileNamePattern: $"{product.ProductKey}-{fileStem}-{{sourceDisplayRef}}");
    }
}

public sealed class StlDefaultPrintPermissionEvaluator(ProductDescriptor product) : IPrintPermissionEvaluator
{
    public void EnsureTemplateCatalogRead(ClaimsPrincipal principal) =>
        EnsurePermissionSet(
            principal,
            [$"{product.ProductKey}.print.preview"],
            fallbackAllowed: !HasExplicitPermissionClaims(principal));

    public void EnsureHistoryRead(ClaimsPrincipal principal) =>
        EnsurePermissionSet(
            principal,
            [$"{product.ProductKey}.print.history.view"],
            fallbackAllowed: !HasExplicitPermissionClaims(principal));

    public void EnsureActionAllowed(ClaimsPrincipal principal, string action, string documentStatus)
    {
        var normalizedAction = StlPrintActions.Normalize(action, nameof(action));
        var normalizedStatus = StlPrintDocumentStatuses.Normalize(documentStatus);
        EnsurePermissionSet(
            principal,
            BuildRequiredPermissions(normalizedAction, normalizedStatus),
            fallbackAllowed: CanFallbackToEntitlementOnly(principal, normalizedAction, normalizedStatus));
    }

    private void EnsurePermissionSet(
        ClaimsPrincipal principal,
        IReadOnlyCollection<string> requiredPermissions,
        bool fallbackAllowed)
    {
        EnsureAuthenticatedAndTenantScoped(principal);

        if (CanBypassPermissionChecks(principal))
        {
            return;
        }

        if (fallbackAllowed)
        {
            return;
        }

        if (requiredPermissions.All(permission => HasPermission(principal, permission)))
        {
            return;
        }

        throw new StlApiException(
            "print.forbidden",
            $"Print access requires {string.Join(", ", requiredPermissions)}.",
            403,
            new
            {
                productKey = product.ProductKey,
                requiredPermissions
            });
    }

    private static void EnsureAuthenticatedAndTenantScoped(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }
        
        _ = principal.GetTenantId();
    }

    private bool CanBypassPermissionChecks(ClaimsPrincipal principal) =>
        principal.IsPlatformAdmin()
        || string.Equals(principal.GetTenantRoleKey(), "tenant_admin", StringComparison.OrdinalIgnoreCase);

    private static bool HasExplicitPermissionClaims(ClaimsPrincipal principal) =>
        principal.Claims.Any(claim =>
            string.Equals(claim.Type, "permissions", StringComparison.OrdinalIgnoreCase)
            || string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase));

    private static bool CanFallbackToEntitlementOnly(
        ClaimsPrincipal principal,
        string action,
        string documentStatus)
    {
        if (HasExplicitPermissionClaims(principal))
        {
            return false;
        }

        return action is StlPrintActions.Preview or StlPrintActions.BrowserPrint
            && documentStatus is StlPrintDocumentStatuses.WorkingCopy or StlPrintDocumentStatuses.Draft;
    }

    private static bool HasPermission(ClaimsPrincipal principal, string permissionKey) =>
        principal.Claims.Any(claim =>
            (string.Equals(claim.Type, "permissions", StringComparison.OrdinalIgnoreCase)
             || string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase)
             || string.Equals(claim.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase))
            && claim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(value => string.Equals(value, permissionKey, StringComparison.OrdinalIgnoreCase)));

    private IReadOnlyList<string> BuildRequiredPermissions(string action, string documentStatus)
    {
        var permissions = new List<string>();

        switch (action)
        {
            case StlPrintActions.Preview:
                permissions.Add($"{product.ProductKey}.print.preview");
                break;
            case StlPrintActions.BrowserPrint:
                permissions.Add($"{product.ProductKey}.print.execute");
                break;
            case StlPrintActions.DownloadPdf:
                permissions.Add($"{product.ProductKey}.print.download");
                break;
            case StlPrintActions.DownloadLabelPdf:
                permissions.Add($"{product.ProductKey}.print.download");
                permissions.Add($"{product.ProductKey}.print.labels");
                break;
            case StlPrintActions.DownloadPacket:
                permissions.Add($"{product.ProductKey}.print.download");
                permissions.Add($"{product.ProductKey}.print.packets");
                break;
            case StlPrintActions.ArchiveOfficial:
                permissions.Add($"{product.ProductKey}.print.archive");
                break;
            case StlPrintActions.Send:
                permissions.Add($"{product.ProductKey}.print.execute");
                break;
            case StlPrintActions.Reprint:
                permissions.Add($"{product.ProductKey}.print.reprint");
                break;
        }

        if (documentStatus == StlPrintDocumentStatuses.Official)
        {
            permissions.Add($"{product.ProductKey}.print.official");
        }

        if (documentStatus == StlPrintDocumentStatuses.Redacted)
        {
            permissions.Add($"{product.ProductKey}.print.redacted");
        }

        return permissions.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}

public sealed class StlPrintLogService(
    PlatformDbContext db,
    ProductDescriptor product,
    IPrintTemplateCatalog templateCatalog,
    IPrintPermissionEvaluator permissionEvaluator,
    IPrintExportAuditWriter auditWriter,
    IEnumerable<IPrintableProvider> providers,
    IEnumerable<ICompliancePrintAdvisor> complianceAdvisors)
{
    public StlPrintTemplateCatalogResponse GetTemplateCatalog(ClaimsPrincipal principal)
    {
        permissionEvaluator.EnsureTemplateCatalogRead(principal);
        return new StlPrintTemplateCatalogResponse(templateCatalog.ListTemplates());
    }

    public StlPrintTemplateDescriptor GetTemplate(ClaimsPrincipal principal, string templateKey)
    {
        permissionEvaluator.EnsureTemplateCatalogRead(principal);
        var template = templateCatalog.GetTemplate(templateKey.Trim());
        if (template is null)
        {
            throw new StlApiException(
                "print.template_not_found",
                $"Print template '{templateKey}' was not found for {product.DisplayName}.",
                404);
        }

        return template;
    }

    public async Task<StlPrintPreviewResponse> PreviewAsync(
        ClaimsPrincipal principal,
        StlPrintDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var resolved = ResolveDocumentRequest(request);
        permissionEvaluator.EnsureActionAllowed(principal, StlPrintActions.Preview, resolved.DocumentStatus);

        var context = CreateProviderContext(principal);
        var provider = ResolveProvider(context, resolved);
        var preview = await provider.BuildPreviewAsync(
            context,
            resolved.Request,
            resolved.Template,
            cancellationToken);
        var advice = await GetComplianceAdviceAsync(context, resolved, cancellationToken);
        var logEntry = await PersistLogAsync(
            principal,
            resolved.Request,
            preview.SourceDisplayRef,
            StlPrintActions.Preview,
            resolved.DocumentStatus,
            preview.TemplateKey,
            preview.TemplateVersion,
            resolved.Request.OptionsJson,
            null,
            null,
            null,
            cancellationToken);

        return new StlPrintPreviewResponse(
            preview.DocumentTitle,
            preview.SourceDisplayRef,
            preview.TemplateKey,
            preview.TemplateVersion,
            preview.PreviewHtml,
            preview.PreviewRoute,
            MergeMessages(preview.Warnings, advice?.Warnings),
            MergeMessages(preview.MissingRequirements, advice?.MissingRequirements),
            logEntry.Id);
    }

    public async Task<StlPrintFileResponse> GeneratePdfAsync(
        ClaimsPrincipal principal,
        StlPrintDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var resolved = ResolveDocumentRequest(request);
        var action = ResolvePdfAction(resolved.Template);
        permissionEvaluator.EnsureActionAllowed(principal, action, resolved.DocumentStatus);

        var context = CreateProviderContext(principal);
        var provider = ResolveProvider(context, resolved);
        var generated = await provider.GeneratePdfAsync(
            context,
            resolved.Request,
            resolved.Template,
            action,
            cancellationToken);
        var advice = await GetComplianceAdviceAsync(context, resolved, cancellationToken);
        var finalized = generated with
        {
            ContentHash = string.IsNullOrWhiteSpace(generated.ContentHash)
                ? ComputeContentHash(generated.Content)
                : generated.ContentHash,
            Warnings = MergeMessages(generated.Warnings, advice?.Warnings),
            MissingRequirements = MergeMessages(generated.MissingRequirements, advice?.MissingRequirements)
        };

        var logEntry = await PersistLogAsync(
            principal,
            resolved.Request,
            finalized.SourceDisplayRef,
            action,
            resolved.DocumentStatus,
            finalized.TemplateKey,
            finalized.TemplateVersion,
            resolved.Request.OptionsJson,
            finalized.FileName,
            finalized.ContentHash,
            null,
            cancellationToken);

        return new StlPrintFileResponse(logEntry.Id, finalized);
    }

    public async Task<StlPrintArchiveResponse> ArchiveOfficialAsync(
        ClaimsPrincipal principal,
        StlPrintDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var resolved = ResolveDocumentRequest(request, fallbackStatus: StlPrintDocumentStatuses.Official);
        permissionEvaluator.EnsureActionAllowed(principal, StlPrintActions.ArchiveOfficial, resolved.DocumentStatus);

        var context = CreateProviderContext(principal);
        var provider = ResolveProvider(context, resolved);
        var archived = await provider.ArchiveOfficialAsync(
            context,
            resolved.Request,
            resolved.Template,
            cancellationToken);
        var advice = await GetComplianceAdviceAsync(context, resolved, cancellationToken);

        var logEntry = await PersistLogAsync(
            principal,
            resolved.Request,
            archived.SourceDisplayRef,
            StlPrintActions.ArchiveOfficial,
            resolved.DocumentStatus,
            archived.TemplateKey,
            archived.TemplateVersion,
            resolved.Request.OptionsJson,
            archived.FileName,
            archived.ContentHash,
            archived.RecordArrDocumentId,
            cancellationToken);

        return new StlPrintArchiveResponse(
            archived.DocumentTitle,
            archived.SourceDisplayRef,
            archived.TemplateKey,
            archived.TemplateVersion,
            archived.RecordArrDocumentId,
            MergeMessages(archived.Warnings, advice?.Warnings),
            MergeMessages(archived.MissingRequirements, advice?.MissingRequirements),
            logEntry.Id);
    }

    public async Task<StlPrintActionResponse> LogBrowserPrintAsync(
        ClaimsPrincipal principal,
        StlBrowserPrintLogRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEntityType = RequireValue(request.SourceEntityType, nameof(request.SourceEntityType));
        var normalizedEntityId = RequireValue(request.SourceEntityId, nameof(request.SourceEntityId));
        var normalizedDisplayRef = RequireValue(request.SourceDisplayRef, nameof(request.SourceDisplayRef));
        var normalizedStatus = StlPrintDocumentStatuses.Normalize(request.DocumentStatus);
        var template = ResolveTemplate(request.TemplateKey, normalizedStatus);
        var normalizedVersion = string.IsNullOrWhiteSpace(request.TemplateVersion)
            ? template.Version
            : request.TemplateVersion.Trim();
        var normalizedMetadata = NormalizeJson(request.MetadataJson, "print.invalid_metadata", "Print metadataJson must be valid JSON.");

        permissionEvaluator.EnsureActionAllowed(principal, StlPrintActions.BrowserPrint, normalizedStatus);

        var logEntry = await PersistLogAsync(
            principal,
            new StlPrintDocumentRequest
            {
                SourceEntityType = normalizedEntityType,
                SourceEntityId = normalizedEntityId,
                SourceDisplayRef = normalizedDisplayRef,
                TemplateKey = template.TemplateKey,
                TemplateVersion = normalizedVersion,
                DocumentStatus = normalizedStatus
            },
            normalizedDisplayRef,
            StlPrintActions.BrowserPrint,
            normalizedStatus,
            template.TemplateKey,
            normalizedVersion,
            normalizedMetadata,
            null,
            null,
            null,
            cancellationToken);

        return ToActionResponse(logEntry);
    }

    public async Task<StlPrintActionResponse> LogReprintAsync(
        ClaimsPrincipal principal,
        StlReprintRequest request,
        CancellationToken cancellationToken)
    {
        var resolved = ResolveDocumentRequest(request);
        var sourceDisplayRef = RequireValue(request.SourceDisplayRef, nameof(request.SourceDisplayRef));
        var normalizedReason = NormalizeReprintReason(request.ReprintReason, resolved.Template, resolved.DocumentStatus);

        permissionEvaluator.EnsureActionAllowed(principal, StlPrintActions.Reprint, resolved.DocumentStatus);

        var logEntry = await PersistLogAsync(
            principal,
            resolved.Request,
            sourceDisplayRef,
            StlPrintActions.Reprint,
            resolved.DocumentStatus,
            resolved.Template.TemplateKey,
            resolved.Request.TemplateVersion ?? resolved.Template.Version,
            resolved.Request.OptionsJson,
            null,
            null,
            null,
            cancellationToken,
            normalizedReason);

        return ToActionResponse(logEntry);
    }

    public async Task<StlPrintHistoryResponse> GetHistoryAsync(
        ClaimsPrincipal principal,
        string sourceEntityType,
        string sourceEntityId,
        int? limit,
        CancellationToken cancellationToken)
    {
        permissionEvaluator.EnsureHistoryRead(principal);

        var normalizedEntityType = RequireValue(sourceEntityType, nameof(sourceEntityType));
        var normalizedEntityId = RequireValue(sourceEntityId, nameof(sourceEntityId));
        var normalizedLimit = Math.Clamp(limit ?? 25, 1, 100);
        var tenantId = principal.GetTenantId();

        var items = await db.PrintExportLogs
            .AsNoTracking()
            .Where(log =>
                log.TenantId == tenantId
                && log.ProductKey == product.ProductKey
                && log.SourceEntityType == normalizedEntityType
                && log.SourceEntityId == normalizedEntityId)
            .OrderByDescending(log => log.RequestedAtUtc)
            .Take(normalizedLimit)
            .Select(log => new StlPrintHistoryItem(
                log.Id,
                log.ProductKey,
                log.SourceEntityType,
                log.SourceEntityId,
                log.SourceDisplayRef,
                log.TemplateKey,
                log.TemplateVersion,
                log.Action,
                log.DocumentStatus,
                log.RequestedByPersonId,
                log.RequestedAtUtc,
                log.CompletedAtUtc,
                log.RecordArrDocumentId,
                log.FileName,
                log.ContentHash,
                log.ReprintReason,
                log.FailureReason,
                log.MetadataJson))
            .ToListAsync(cancellationToken);

        return new StlPrintHistoryResponse(items);
    }

    private ResolvedPrintRequest ResolveDocumentRequest(
        StlPrintDocumentRequest request,
        string fallbackStatus = StlPrintDocumentStatuses.WorkingCopy)
    {
        var normalizedEntityType = RequireValue(request.SourceEntityType, nameof(request.SourceEntityType));
        var normalizedEntityId = RequireValue(request.SourceEntityId, nameof(request.SourceEntityId));
        var normalizedStatus = StlPrintDocumentStatuses.Normalize(request.DocumentStatus, fallbackStatus);
        var template = ResolveTemplate(request.TemplateKey, normalizedStatus);
        var normalizedVersion = string.IsNullOrWhiteSpace(request.TemplateVersion)
            ? template.Version
            : request.TemplateVersion.Trim();
        var normalizedOptions = NormalizeJson(
            request.OptionsJson,
            "print.invalid_options",
            "Print optionsJson must be valid JSON.");

        return new ResolvedPrintRequest(
            new StlPrintDocumentRequest
            {
                SourceEntityType = normalizedEntityType,
                SourceEntityId = normalizedEntityId,
                SourceDisplayRef = string.IsNullOrWhiteSpace(request.SourceDisplayRef)
                    ? null
                    : request.SourceDisplayRef.Trim(),
                TemplateKey = template.TemplateKey,
                TemplateVersion = normalizedVersion,
                DocumentStatus = normalizedStatus,
                OptionsJson = normalizedOptions,
                ReprintReason = string.IsNullOrWhiteSpace(request.ReprintReason)
                    ? null
                    : request.ReprintReason.Trim()
            },
            template,
            normalizedStatus);
    }

    private StlPrintProviderContext CreateProviderContext(ClaimsPrincipal principal) =>
        new(
            product,
            principal,
            principal.GetTenantId(),
            principal.GetPersonId());

    private IPrintableProvider ResolveProvider(StlPrintProviderContext context, ResolvedPrintRequest resolved)
    {
        var provider = providers.FirstOrDefault(candidate =>
            candidate.CanHandle(context, resolved.Request, resolved.Template));
        if (provider is null)
        {
            throw new StlApiException(
                "print.provider_not_configured",
                $"No printable provider is registered for {product.DisplayName} template '{resolved.Template.TemplateKey}'.",
                501);
        }

        return provider;
    }

    private async Task<StlCompliancePrintAdvice?> GetComplianceAdviceAsync(
        StlPrintProviderContext context,
        ResolvedPrintRequest resolved,
        CancellationToken cancellationToken)
    {
        if (!complianceAdvisors.Any())
        {
            return null;
        }

        var warnings = new List<string>();
        var missingRequirements = new List<string>();

        foreach (var advisor in complianceAdvisors)
        {
            var advice = await advisor.GetAdviceAsync(
                context,
                resolved.Request,
                resolved.Template,
                cancellationToken);
            if (advice is null)
            {
                continue;
            }

            warnings.AddRange(advice.Warnings);
            missingRequirements.AddRange(advice.MissingRequirements);
        }

        return new StlCompliancePrintAdvice(
            warnings.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            missingRequirements.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private async Task<StlPrintExportLog> PersistLogAsync(
        ClaimsPrincipal principal,
        StlPrintDocumentRequest request,
        string sourceDisplayRef,
        string action,
        string documentStatus,
        string templateKey,
        string templateVersion,
        string? metadataJson,
        string? fileName,
        string? contentHash,
        string? recordArrDocumentId,
        CancellationToken cancellationToken,
        string? reprintReason = null)
    {
        var logEntry = new StlPrintExportLog
        {
            Id = Guid.NewGuid(),
            TenantId = principal.GetTenantId(),
            ProductKey = product.ProductKey,
            SourceEntityType = RequireValue(request.SourceEntityType, nameof(request.SourceEntityType)),
            SourceEntityId = RequireValue(request.SourceEntityId, nameof(request.SourceEntityId)),
            SourceDisplayRef = sourceDisplayRef,
            TemplateKey = templateKey,
            TemplateVersion = templateVersion,
            Action = action,
            DocumentStatus = documentStatus,
            RequestedByPersonId = principal.GetPersonId(),
            RequestedAtUtc = DateTimeOffset.UtcNow,
            CompletedAtUtc = DateTimeOffset.UtcNow,
            FileName = fileName,
            ContentHash = contentHash,
            RecordArrDocumentId = recordArrDocumentId,
            ReprintReason = reprintReason,
            MetadataJson = metadataJson
        };

        db.PrintExportLogs.Add(logEntry);
        await db.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(logEntry, cancellationToken);
        return logEntry;
    }

    private static string ResolvePdfAction(StlPrintTemplateDescriptor template) =>
        template.Format switch
        {
            StlPrintFormats.LabelPdf => StlPrintActions.DownloadLabelPdf,
            StlPrintFormats.PacketPdf => StlPrintActions.DownloadPacket,
            _ => StlPrintActions.DownloadPdf
        };

    private StlPrintTemplateDescriptor ResolveTemplate(string? templateKey, string documentStatus)
    {
        var resolvedTemplateKey = string.IsNullOrWhiteSpace(templateKey)
            ? $"{product.ProductKey}.current_page.{documentStatus}"
            : templateKey.Trim();
        var template = templateCatalog.GetTemplate(resolvedTemplateKey);
        if (template is null)
        {
            throw new StlApiException(
                "print.template_not_found",
                $"Print template '{resolvedTemplateKey}' was not found for {product.DisplayName}.",
                404);
        }

        return template;
    }

    private static string RequireValue(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException(
                "print.invalid_request",
                $"Print request field '{parameterName}' is required.",
                400);
        }

        return value.Trim();
    }

    private static string? NormalizeJson(string? raw, string code, string message)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(raw);
            return document.RootElement.GetRawText();
        }
        catch (JsonException)
        {
            throw new StlApiException(code, message, 400);
        }
    }

    private static string? NormalizeReprintReason(
        string? reprintReason,
        StlPrintTemplateDescriptor template,
        string documentStatus)
    {
        var normalizedReason = string.IsNullOrWhiteSpace(reprintReason) ? null : reprintReason.Trim();
        var reasonRequired = template.RequiresReprintReason
            || documentStatus is StlPrintDocumentStatuses.Official or StlPrintDocumentStatuses.Copy or StlPrintDocumentStatuses.Redacted;
        if (reasonRequired && normalizedReason is null)
        {
            throw new StlApiException(
                "print.reprint_reason_required",
                "Reprint reason is required for this document.",
                400);
        }

        return normalizedReason;
    }

    private static string ComputeContentHash(byte[] content) =>
        Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();

    private static IReadOnlyList<string> MergeMessages(
        IReadOnlyList<string> primary,
        IReadOnlyList<string>? secondary)
    {
        if (secondary is null || secondary.Count == 0)
        {
            return primary;
        }

        return primary.Concat(secondary)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static StlPrintActionResponse ToActionResponse(StlPrintExportLog logEntry) =>
        new(
            logEntry.Id,
            logEntry.ProductKey,
            logEntry.Action,
            logEntry.DocumentStatus,
            logEntry.TemplateKey,
            logEntry.TemplateVersion,
            logEntry.RequestedAtUtc);

    private sealed record ResolvedPrintRequest(
        StlPrintDocumentRequest Request,
        StlPrintTemplateDescriptor Template,
        string DocumentStatus);
}

internal sealed class NullPrintExportAuditWriter : IPrintExportAuditWriter
{
    public Task WriteAsync(StlPrintExportLog logEntry, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
