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

    public async Task<ComplianceReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        bool? attentionOnly,
        string? partyType,
        Guid? externalPartyId,
        string? reviewStatus,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var documents = await LoadDocumentsAsync(tenantId, partyType, externalPartyId, cancellationToken);
        var mapped = documents
            .Select(x => MapDocumentSummary(x, now))
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
                    || string.Equals(x.ReviewStatus, PartyComplianceDocumentReviewStatuses.Pending, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var partySummaries = BuildPartySummaries(documents, mapped);
        var totals = new ComplianceReportTotalsResponse(
            partySummaries.Count,
            mapped.Count,
            mapped.Count(x => x.IsExpired),
            mapped.Count(x => x.IsExpiringSoon),
            mapped.Count(x => string.Equals(x.ReviewStatus, PartyComplianceDocumentReviewStatuses.Pending, StringComparison.OrdinalIgnoreCase)),
            mapped.Count(x => string.Equals(x.EffectiveStatus, PartyComplianceDocumentReviewStatuses.Approved, StringComparison.OrdinalIgnoreCase)),
            mapped.Count(x => string.Equals(x.ReviewStatus, PartyComplianceDocumentReviewStatuses.Rejected, StringComparison.OrdinalIgnoreCase)));

        return new ComplianceReportSummaryResponse(
            now,
            totals,
            partySummaries,
            mapped.OrderByDescending(x => x.IsExpired)
                .ThenByDescending(x => x.IsExpiringSoon)
                .ThenByDescending(x => x.UpdatedAt)
                .ToList());
    }

    public async Task<CompliancePartyDetailResponse> GetPartyDetailAsync(
        Guid tenantId,
        Guid externalPartyId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var documents = await LoadDocumentsAsync(tenantId, null, externalPartyId, cancellationToken);
        if (documents.Count == 0)
        {
            var party = await db.ExternalParties
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == externalPartyId, cancellationToken);
            if (party is null)
            {
                throw new StlApiException("parties.not_found", "External party was not found.", 404);
            }

            var emptySummary = new CompliancePartySummaryItemResponse(
                party.Id,
                party.PartyKey,
                party.DisplayName,
                party.PartyType,
                party.ApprovalStatus,
                "none",
                0,
                0,
                0,
                0);
            return new CompliancePartyDetailResponse(emptySummary, []);
        }

        var mapped = documents.Select(x => MapDocumentSummary(x, now)).ToList();
        var partySummary = BuildPartySummaries(documents, mapped).First();
        var detailDocuments = documents
            .OrderByDescending(x => x.ExpiresAt ?? DateTimeOffset.MinValue)
            .ThenByDescending(x => x.UpdatedAt)
            .Take(DetailListLimit)
            .Select(x => MapDocumentDetail(x, now))
            .ToList();

        return new CompliancePartyDetailResponse(partySummary, detailDocuments);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportSummaryCsvAsync(
        Guid tenantId,
        bool? attentionOnly,
        string? partyType,
        Guid? externalPartyId,
        string? reviewStatus,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(
            tenantId,
            attentionOnly,
            partyType,
            externalPartyId,
            reviewStatus,
            cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(
            "partyKey,partyDisplayName,partyType,documentKey,documentTypeKey,title,version,reviewStatus,effectiveStatus,isExpired,isExpiringSoon,expiresAt,updatedAt");

        foreach (var doc in summary.Documents)
        {
            builder.Append(CsvEscape(doc.PartyKey));
            builder.Append(',');
            builder.Append(CsvEscape(doc.PartyDisplayName));
            builder.Append(',');
            builder.Append(CsvEscape(doc.PartyType));
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
        var vendorParties = await db.ExternalParties
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && (x.PartyType == "vendor" || x.PartyType == "supplier")
                && x.Status == "active")
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        foreach (var party in vendorParties)
        {
            var approvedTypeKeys = await db.PartyComplianceDocuments
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId
                    && x.ExternalPartyId == party.Id
                    && x.ReviewStatus == PartyComplianceDocumentReviewStatuses.Approved
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
                    party.Id,
                    party.PartyKey,
                    null,
                    null,
                    null,
                    $"Missing required documents: {string.Join(", ", missing)}.",
                    party.UpdatedAt));
            }
        }

        var expiringDocuments = await db.PartyComplianceDocuments
            .AsNoTracking()
            .Include(x => x.ExternalParty)
            .Where(x => x.TenantId == tenantId
                && x.ReviewStatus == PartyComplianceDocumentReviewStatuses.Approved
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
                document.ExternalPartyId,
                document.ExternalParty.PartyKey,
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
                request.VendorPartyId,
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
                row.request.VendorPartyId,
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

    private async Task<List<PartyComplianceDocument>> LoadDocumentsAsync(
        Guid tenantId,
        string? partyType,
        Guid? externalPartyId,
        CancellationToken cancellationToken)
    {
        var query = db.PartyComplianceDocuments
            .AsNoTracking()
            .Include(x => x.ExternalParty)
            .Where(x => x.TenantId == tenantId);

        if (externalPartyId is not null)
        {
            query = query.Where(x => x.ExternalPartyId == externalPartyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(partyType))
        {
            var normalized = partyType.Trim().ToLowerInvariant();
            query = query.Where(x => x.ExternalParty.PartyType == normalized);
        }

        return await query.ToListAsync(cancellationToken);
    }

    private static List<CompliancePartySummaryItemResponse> BuildPartySummaries(
        IReadOnlyList<PartyComplianceDocument> sourceDocuments,
        IReadOnlyList<ComplianceDocumentSummaryItemResponse> documents)
    {
        return documents
            .GroupBy(x => x.ExternalPartyId)
            .Select(group =>
            {
                var first = group.First();
                var party = sourceDocuments.First(x => x.ExternalPartyId == first.ExternalPartyId).ExternalParty;
                var expired = group.Count(x => x.IsExpired);
                var expiringSoon = group.Count(x => x.IsExpiringSoon);
                var reviewPending = group.Count(x =>
                    string.Equals(x.ReviewStatus, PartyComplianceDocumentReviewStatuses.Pending, StringComparison.OrdinalIgnoreCase));
                return new CompliancePartySummaryItemResponse(
                    first.ExternalPartyId,
                    first.PartyKey,
                    first.PartyDisplayName,
                    first.PartyType,
                    party.ApprovalStatus,
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

    private static string ResolveCompliancePosture(int expired, int expiringSoon, int reviewPending)
    {
        if (expired > 0)
        {
            return PartyComplianceDocumentReviewStatuses.Expired;
        }

        if (expiringSoon > 0)
        {
            return "expiring_soon";
        }

        if (reviewPending > 0)
        {
            return PartyComplianceDocumentReviewStatuses.Pending;
        }

        return PartyComplianceDocumentReviewStatuses.Approved;
    }

    private static ComplianceDocumentSummaryItemResponse MapDocumentSummary(
        PartyComplianceDocument document,
        DateTimeOffset now)
    {
        var isExpired = IsExpired(document.ExpiresAt, now);
        var isExpiringSoon = !isExpired && IsExpiringSoon(document.ExpiresAt, now);
        var effectiveStatus = ResolveEffectiveStatus(document.ReviewStatus, isExpired);

        return new ComplianceDocumentSummaryItemResponse(
            document.Id,
            document.ExternalPartyId,
            document.ExternalParty.PartyKey,
            document.ExternalParty.DisplayName,
            document.ExternalParty.PartyType,
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

    private static ComplianceDocumentDetailItemResponse MapDocumentDetail(
        PartyComplianceDocument document,
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

    private static string ResolveEffectiveStatus(string reviewStatus, bool isExpired) =>
        isExpired ? PartyComplianceDocumentReviewStatuses.Expired : reviewStatus.Trim().ToLowerInvariant();

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
}
