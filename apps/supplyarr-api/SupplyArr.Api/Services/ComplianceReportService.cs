using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class ComplianceReportService(SupplyArrDbContext db)
{
    private const int DetailListLimit = 50;
    private static readonly TimeSpan ExpiringSoonWindow = TimeSpan.FromDays(30);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<SupplierComplianceReportSummaryResponse> GetSupplierSummaryAsync(
        Guid tenantId,
        bool? attentionOnly,
        Guid? supplierId,
        string? reviewStatus,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var documents = await LoadSupplierDocumentsAsync(tenantId, supplierId, cancellationToken);
        var mapped = documents
            .Select(x => MapSupplierDocumentSummary(x, now))
            .ToList();

        if (!string.IsNullOrWhiteSpace(reviewStatus))
        {
            var normalized = reviewStatus.Trim().ToLowerInvariant();
            mapped = mapped
                .Where(x => string.Equals(x.EffectiveStatus, normalized, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (attentionOnly == true)
        {
            mapped = mapped
                .Where(x => x.IsExpired || x.IsExpiringSoon
                    || string.Equals(x.ReviewStatus, SupplierComplianceDocumentReviewStatuses.Pending, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var supplierSummaries = BuildSupplierSummaries(documents, mapped);

        return new SupplierComplianceReportSummaryResponse(
            now,
            new SupplierComplianceReportTotalsResponse(
                supplierSummaries.Count,
                mapped.Count,
                mapped.Count(x => x.IsExpired),
                mapped.Count(x => x.IsExpiringSoon),
                mapped.Count(x => string.Equals(x.ReviewStatus, SupplierComplianceDocumentReviewStatuses.Pending, StringComparison.OrdinalIgnoreCase)),
                mapped.Count(x => string.Equals(x.EffectiveStatus, SupplierComplianceDocumentReviewStatuses.Approved, StringComparison.OrdinalIgnoreCase)),
                mapped.Count(x => string.Equals(x.ReviewStatus, SupplierComplianceDocumentReviewStatuses.Rejected, StringComparison.OrdinalIgnoreCase))),
            supplierSummaries,
            mapped.OrderByDescending(x => x.IsExpired)
                .ThenByDescending(x => x.IsExpiringSoon)
                .ThenByDescending(x => x.UpdatedAt)
                .ToList());
    }

    public async Task<SupplierComplianceDetailResponse> GetSupplierDetailAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var documents = await LoadSupplierDocumentsAsync(tenantId, supplierId, cancellationToken);
        if (documents.Count == 0)
        {
            var supplier = await db.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId
                        && x.Id == supplierId
                       ,
                    cancellationToken);
            if (supplier is null)
            {
                throw new StlApiException("suppliers.not_found", "Supplier was not found.", 404);
            }

            var emptySummary = new SupplierComplianceSummaryItemResponse(
                supplier.Id,
                supplier.SupplierKey,
                supplier.DisplayName,
                supplier.ParentSupplierId,
                supplier.ParentSupplier?.DisplayName,
                supplier.UnitKind,
                ParseServiceTypes(supplier.ServiceTypesJson),
                supplier.ApprovalStatus,
                "none",
                0,
                0,
                0,
                0);
            return new SupplierComplianceDetailResponse(emptySummary, []);
        }

        var mapped = documents.Select(x => MapSupplierDocumentSummary(x, now)).ToList();
        var supplierSummary = BuildSupplierSummaries(documents, mapped).First();
        var detailDocuments = documents
            .OrderByDescending(x => x.ExpiresAt ?? DateTimeOffset.MinValue)
            .ThenByDescending(x => x.UpdatedAt)
            .Take(DetailListLimit)
            .Select(x => MapDocumentDetail(x, now))
            .ToList();

        return new SupplierComplianceDetailResponse(supplierSummary, detailDocuments);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportSupplierSummaryCsvAsync(
        Guid tenantId,
        bool? attentionOnly,
        Guid? supplierId,
        string? reviewStatus,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSupplierSummaryAsync(
            tenantId,
            attentionOnly,
            supplierId,
            reviewStatus,
            cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(
            "supplierKey,supplierDisplayName,documentKey,documentTypeKey,title,version,reviewStatus,effectiveStatus,isExpired,isExpiringSoon,expiresAt,updatedAt");

        foreach (var doc in summary.Documents)
        {
            builder.Append(CsvEscape(doc.SupplierKey));
            builder.Append(',');
            builder.Append(CsvEscape(doc.SupplierDisplayName));
            builder.Append(',');
            builder.Append(CsvEscape(doc.DocumentKey));
            builder.Append(',');
            builder.Append(CsvEscape(doc.DocumentTypeKey));
            builder.Append(',');
            builder.Append(CsvEscape(doc.Title));
            builder.Append(',');
            builder.Append(doc.Version);
            builder.Append(',');
            builder.Append(CsvEscape(doc.ReviewStatus));
            builder.Append(',');
            builder.Append(CsvEscape(doc.EffectiveStatus));
            builder.Append(',');
            builder.Append(doc.IsExpired ? "true" : "false");
            builder.Append(',');
            builder.Append(doc.IsExpiringSoon ? "true" : "false");
            builder.Append(',');
            builder.Append(doc.ExpiresAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.AppendLine(doc.UpdatedAt.ToString("O"));
        }

        var fileName = $"supplyarr-compliance-{DateTime.UtcNow:yyyy-MM-dd}.csv";
        return (Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", fileName);
    }

    public async Task<IReadOnlyList<ComplianceReportAlertResponse>> ListAlertsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var alerts = new List<ComplianceReportAlertResponse>();

        var requiredDocumentTypeKeys = await LoadRequiredDocumentTypeKeysAsync(tenantId, cancellationToken);
        var suppliers = await db.Suppliers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
               
                && x.Status == "active")
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        foreach (var supplier in suppliers)
        {
            var approvedTypeKeys = await db.SupplierComplianceDocuments
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId
                    && x.SupplierId == supplier.Id
                    && x.ReviewStatus == SupplierComplianceDocumentReviewStatuses.Approved
                    && (x.ExpiresAt == null || x.ExpiresAt > now))
                .Select(x => x.DocumentTypeKey)
                .Distinct()
                .ToListAsync(cancellationToken);

            var missing = requiredDocumentTypeKeys
                .Where(required => approvedTypeKeys.All(existing =>
                    !string.Equals(existing, required, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            if (missing.Count > 0)
            {
                alerts.Add(new ComplianceReportAlertResponse(
                    "missing_required_documents",
                    "high",
                    supplier.Id,
                    supplier.SupplierKey,
                    null,
                    null,
                    null,
                    $"Missing required documents: {string.Join(", ", missing)}.",
                    supplier.UpdatedAt));
            }
        }

        var expiringDocuments = await db.SupplierComplianceDocuments
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Where(x => x.TenantId == tenantId
                && x.ReviewStatus == SupplierComplianceDocumentReviewStatuses.Approved
                && x.ExpiresAt != null
                && x.ExpiresAt >= now
                && x.ExpiresAt <= now.Add(ExpiringSoonWindow))
            .OrderBy(x => x.ExpiresAt)
            .ToListAsync(cancellationToken);

        foreach (var document in expiringDocuments)
        {
            alerts.Add(new ComplianceReportAlertResponse(
                "expiring_compliance_document",
                "medium",
                document.SupplierId,
                document.Supplier.SupplierKey,
                null,
                null,
                null,
                $"Document '{document.Title}' expires on {document.ExpiresAt:O}.",
                document.UpdatedAt));
        }

        var approvalsMissingEvidence = await db.PurchaseRequests
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.Status == PurchaseRequestStatuses.Approved
                && (x.ApprovedByUserId == null
                    || (x.IsEmergency && x.ManagerOverrideApproved && string.IsNullOrWhiteSpace(x.ManagerOverrideJustification))))
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        foreach (var request in approvalsMissingEvidence)
        {
            alerts.Add(new ComplianceReportAlertResponse(
                "purchase_approval_missing_evidence",
                "high",
                request.SupplierId,
                null,
                request.Id,
                request.RequestKey,
                null,
                "Approved purchase request is missing approval evidence.",
                request.UpdatedAt));
        }

        var emergencyExceptionRows = await (
            from exception in db.ProcurementExceptions.AsNoTracking()
            join request in db.PurchaseRequests.AsNoTracking()
                on exception.LinkedPurchaseRequestId equals request.Id
            where exception.TenantId == tenantId
                && request.TenantId == tenantId
                && request.IsEmergency
                && ProcurementExceptionStatuses.Active.Contains(exception.Status)
            orderby exception.UpdatedAt descending
            select new { exception, request })
            .ToListAsync(cancellationToken);

        foreach (var row in emergencyExceptionRows)
        {
            alerts.Add(new ComplianceReportAlertResponse(
                "emergency_purchase_exception",
                "high",
                row.request.SupplierId,
                null,
                row.request.Id,
                row.request.RequestKey,
                row.exception.Id,
                $"Emergency purchase exception '{row.exception.ExceptionKey}' is {row.exception.Status}.",
                row.exception.UpdatedAt));
        }

        return alerts
            .OrderByDescending(x => x.DetectedAt)
            .Take(200)
            .ToList();
    }

    private Task<List<SupplierComplianceDocument>> LoadSupplierDocumentsAsync(
        Guid tenantId,
        Guid? supplierId,
        CancellationToken cancellationToken)
    {
        var query = db.SupplierComplianceDocuments
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Where(x => x.TenantId == tenantId);

        if (supplierId is not null)
        {
            query = query.Where(x => x.SupplierId == supplierId.Value);
        }

        return query.ToListAsync(cancellationToken);
    }

    private static string ResolveCompliancePosture(int expired, int expiringSoon, int reviewPending)
    {
        if (expired > 0)
        {
            return SupplierComplianceDocumentReviewStatuses.Expired;
        }

        if (expiringSoon > 0)
        {
            return "expiring_soon";
        }

        if (reviewPending > 0)
        {
            return SupplierComplianceDocumentReviewStatuses.Pending;
        }

        return SupplierComplianceDocumentReviewStatuses.Approved;
    }

    private static ComplianceDocumentDetailItemResponse MapDocumentDetail(
        SupplierComplianceDocument document,
        DateTimeOffset now)
    {
        var isExpired = IsExpired(document.ExpiresAt, now);
        var isExpiringSoon = !isExpired && IsExpiringSoon(document.ExpiresAt, now);
        var effectiveStatus = ResolveEffectiveStatus(document.ReviewStatus, isExpired);

        return new ComplianceDocumentDetailItemResponse(
            document.Id,
            document.DocumentKey,
            document.DocumentTypeKey,
            document.Title,
            document.Version,
            document.ReviewStatus,
            effectiveStatus,
            isExpired,
            isExpiringSoon,
            document.ExpiresAt,
            document.EffectiveAt,
            document.FileName,
            document.ContentType,
            document.SizeBytes,
            document.Notes,
            document.ReviewedAt,
            document.CreatedAt,
            document.UpdatedAt);
    }

    private static SupplierComplianceDocumentSummaryItemResponse MapSupplierDocumentSummary(
        SupplierComplianceDocument document,
        DateTimeOffset now)
    {
        var isExpired = IsExpired(document.ExpiresAt, now);
        var isExpiringSoon = !isExpired && IsExpiringSoon(document.ExpiresAt, now);
        var effectiveStatus = ResolveEffectiveStatus(document.ReviewStatus, isExpired);

        return new SupplierComplianceDocumentSummaryItemResponse(
            document.Id,
            document.SupplierId,
            document.Supplier.SupplierKey,
            document.Supplier.DisplayName,
            document.DocumentKey,
            document.DocumentTypeKey,
            document.Title,
            document.Version,
            document.ReviewStatus,
            effectiveStatus,
            isExpired,
            isExpiringSoon,
            document.ExpiresAt,
            document.UpdatedAt);
    }

    private static List<SupplierComplianceSummaryItemResponse> BuildSupplierSummaries(
        IReadOnlyList<SupplierComplianceDocument> sourceDocuments,
        IReadOnlyList<SupplierComplianceDocumentSummaryItemResponse> documents)
    {
        return documents
            .GroupBy(x => x.SupplierId)
            .Select(group =>
            {
                var first = group.First();
                var supplier = sourceDocuments.First(x => x.SupplierId == first.SupplierId).Supplier;
                var expired = group.Count(x => x.IsExpired);
                var expiringSoon = group.Count(x => x.IsExpiringSoon);
                var reviewPending = group.Count(x =>
                    string.Equals(x.ReviewStatus, SupplierComplianceDocumentReviewStatuses.Pending, StringComparison.OrdinalIgnoreCase));
                return new SupplierComplianceSummaryItemResponse(
                    first.SupplierId,
                    first.SupplierKey,
                    first.SupplierDisplayName,
                    supplier.ParentSupplierId,
                    supplier.ParentSupplier?.DisplayName,
                    supplier.UnitKind,
                    ParseServiceTypes(supplier.ServiceTypesJson),
                    supplier.ApprovalStatus,
                    ResolveCompliancePosture(expired, expiringSoon, reviewPending),
                    group.Count(),
                    expired,
                    expiringSoon,
                    reviewPending);
            })
            .OrderByDescending(x => x.ExpiredCount)
            .ThenByDescending(x => x.ExpiringSoonCount)
            .ThenBy(x => x.DisplayName)
            .ToList();
    }

    private static string ResolveEffectiveStatus(string reviewStatus, bool isExpired) =>
        isExpired ? SupplierComplianceDocumentReviewStatuses.Expired : reviewStatus.Trim().ToLowerInvariant();

    private static bool IsExpired(DateTimeOffset? expiresAt, DateTimeOffset now) =>
        expiresAt is not null && expiresAt.Value < now;

    private static bool IsExpiringSoon(DateTimeOffset? expiresAt, DateTimeOffset now)
    {
        if (expiresAt is null)
        {
            return false;
        }

        var threshold = now.Add(ExpiringSoonWindow);
        return expiresAt.Value >= now && expiresAt.Value <= threshold;
    }

    private static string CsvEscape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }

    private async Task<IReadOnlyList<string>> LoadRequiredDocumentTypeKeysAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var settings = await db.TenantSupplierOnboardingSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (settings is null || string.IsNullOrWhiteSpace(settings.RequiredDocumentTypeKeysJson))
        {
            return SupplierOnboardingRules.DefaultRequirements.Select(x => x.DocumentTypeKey).ToList();
        }

        var parsed = JsonSerializer.Deserialize<List<string>>(settings.RequiredDocumentTypeKeysJson, JsonOptions) ?? [];
        var normalized = SupplierOnboardingRules.NormalizeRequiredTypeKeys(parsed);
        return normalized.Count == 0
            ? SupplierOnboardingRules.DefaultRequirements.Select(x => x.DocumentTypeKey).ToList()
            : normalized;
    }

    private static IReadOnlyList<string> ParseServiceTypes(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(value, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}


