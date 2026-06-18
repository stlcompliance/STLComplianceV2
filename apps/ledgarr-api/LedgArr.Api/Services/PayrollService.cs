using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LedgArr.Api.Data;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace LedgArr.Api.Services;

public sealed class PayrollService(LedgArrDbContext db, PayrollIntegrationClient payrollIntegrationClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<PayrollCalendar>> ListCalendarsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        return await db.PayrollCalendars.Where(x => x.TenantId == tenantId).OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<PayrollCalendar> UpsertCalendarAsync(ClaimsPrincipal principal, Guid? id, CreatePayrollCalendarRequest request, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        _ = await db.FinancialLegalEntities.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.LegalEntityId, cancellationToken)
            ?? throw new StlApiException("ledgarr.payroll.legal_entity_not_found", "Financial legal entity was not found.", 404);

        var entity = id.HasValue
            ? await db.PayrollCalendars.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id.Value, cancellationToken)
            : null;
        if (id.HasValue && entity is null)
        {
            throw new StlApiException("ledgarr.payroll.calendar_not_found", "Payroll calendar was not found.", 404);
        }

        entity ??= new PayrollCalendar { TenantId = tenantId, CreatedAt = DateTimeOffset.UtcNow };
        entity.LegalEntityId = request.LegalEntityId;
        entity.Name = Require(request.Name, "Payroll calendar name is required.", 128);
        entity.Frequency = NormalizeEnum(request.Frequency, ["weekly", "biweekly", "semimonthly", "monthly", "custom"], "Payroll frequency");
        entity.PeriodStartDate = request.PeriodStartDate;
        entity.PeriodEndDate = request.PeriodEndDate;
        entity.PayDate = request.PayDate;
        entity.CutoffDate = request.CutoffDate;
        entity.Timezone = Require(request.Timezone, "Timezone is required.", 64);
        entity.Status = NormalizeEnum(request.Status, ["active", "inactive"], "Payroll calendar status");
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        if (db.Entry(entity).State == EntityState.Detached)
        {
            db.PayrollCalendars.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<IReadOnlyList<PayrollCodeMapping>> ListCodeMappingsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        return await db.PayrollCodeMappings.Where(x => x.TenantId == tenantId).OrderBy(x => x.StaffArrPayCodeRef).ToListAsync(cancellationToken);
    }

    public async Task<PayrollCodeMapping> UpsertCodeMappingAsync(ClaimsPrincipal principal, Guid? id, CreatePayrollCodeMappingRequest request, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        var entity = id.HasValue
            ? await db.PayrollCodeMappings.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id.Value, cancellationToken)
            : null;
        if (id.HasValue && entity is null)
        {
            throw new StlApiException("ledgarr.payroll.code_mapping_not_found", "Payroll code mapping was not found.", 404);
        }

        entity ??= new PayrollCodeMapping { TenantId = tenantId };
        entity.LegalEntityId = request.LegalEntityId;
        entity.StaffArrPayCodeRef = Require(request.StaffArrPayCodeRef, "StaffArr pay code ref is required.", 64).ToUpperInvariant();
        entity.PayrollProviderRef = Optional(request.PayrollProviderRef, 64);
        entity.ProviderEarningCode = Require(request.ProviderEarningCode, "Provider earning code is required.", 64);
        entity.ProviderDeductionCode = Optional(request.ProviderDeductionCode, 64);
        entity.GlAccountRef = Require(request.GlAccountRef, "GL account ref is required.", 64);
        entity.CostCenterRef = Optional(request.CostCenterRef, 128);
        entity.DepartmentRef = Optional(request.DepartmentRef, 128);
        entity.TaxableTreatmentSnapshot = Optional(request.TaxableTreatmentSnapshot, 1024);
        entity.Active = request.Active;
        entity.EffectiveStartDate = request.EffectiveStartDate;
        entity.EffectiveEndDate = request.EffectiveEndDate;
        if (db.Entry(entity).State == EntityState.Detached)
        {
            db.PayrollCodeMappings.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<IReadOnlyList<PayrollBatch>> ListBatchesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        return await db.PayrollBatches.Where(x => x.TenantId == tenantId).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<PayrollBatch> CreateBatchAsync(ClaimsPrincipal principal, CreatePayrollBatchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        var calendar = await db.PayrollCalendars.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.PayrollCalendarId, cancellationToken)
            ?? throw new StlApiException("ledgarr.payroll.calendar_not_found", "Payroll calendar was not found.", 404);
        var batch = new PayrollBatch
        {
            TenantId = tenantId,
            LegalEntityId = request.LegalEntityId,
            PayrollCalendarId = request.PayrollCalendarId,
            PeriodStartDate = request.PeriodStartDate,
            PeriodEndDate = request.PeriodEndDate,
            PayDate = request.PayDate ?? calendar.PayDate,
            ExportProvider = NormalizeEnum(request.ExportProvider ?? "generic_csv", ["adp", "paychex", "ukg", "workday", "gusto", "quickbooks_payroll", "paylocity", "generic_csv", "generic_sftp", "generic_webhook"], "Export provider"),
        };

        db.PayrollBatches.Add(batch);
        await db.SaveChangesAsync(cancellationToken);
        return batch;
    }

    public async Task<PayrollBatch?> GetBatchAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        return await db.PayrollBatches.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken);
    }

    public async Task<PayrollBatch> CollectTimeAsync(ClaimsPrincipal principal, Guid batchId, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        var batch = await db.PayrollBatches.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == batchId, cancellationToken)
            ?? throw new StlApiException("ledgarr.payroll.batch_not_found", "Payroll batch was not found.", 404);

        var snapshot = await payrollIntegrationClient.GetPayrollReadySnapshotAsync(tenantId, batch.PeriodStartDate, batch.PeriodEndDate, cancellationToken);
        batch.SourceStaffArrSnapshotId = snapshot.SnapshotId;
        batch.SourceSnapshotHash = ComputeHash(JsonSerializer.Serialize(snapshot, JsonOptions));
        batch.Status = "collecting_time";
        batch.UpdatedAt = DateTimeOffset.UtcNow;

        var existingLines = await db.PayrollBatchLines.Where(x => x.TenantId == tenantId && x.PayrollBatchId == batchId).ToListAsync(cancellationToken);
        db.PayrollBatchLines.RemoveRange(existingLines);

        var legalEntity = await db.FinancialLegalEntities.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == batch.LegalEntityId, cancellationToken)
            ?? throw new StlApiException("ledgarr.payroll.legal_entity_not_found", "Financial legal entity was not found.", 404);

        var lineDrafts = new List<PayrollBatchLine>();
        foreach (var timesheet in snapshot.Timesheets.Where(x => x.DefaultLegalEntityRef == legalEntity.EntityCode || x.DefaultLegalEntityRef == legalEntity.Id.ToString()))
        {
            foreach (var entry in timesheet.Entries)
            {
                var mapping = await db.PayrollCodeMappings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.LegalEntityId == batch.LegalEntityId && x.StaffArrPayCodeRef == entry.PayCode && x.Active, cancellationToken);

                lineDrafts.Add(new PayrollBatchLine
                {
                    TenantId = tenantId,
                    PayrollBatchId = batchId,
                    PersonId = timesheet.PersonId,
                    WorkerNumber = timesheet.WorkerNumber,
                    LegalEntityId = batch.LegalEntityId,
                    PayrollCalendarId = batch.PayrollCalendarId,
                    PayCodeRef = entry.PayCode,
                    ProviderEarningCode = mapping?.ProviderEarningCode ?? string.Empty,
                    DurationMinutes = entry.DurationMinutes,
                    AllocationSnapshot = JsonSerializer.Serialize(entry.Allocations, JsonOptions),
                    SourceTimesheetPeriodRef = timesheet.TimesheetPeriodId.ToString(),
                    SourceTimeEntryRefs = JsonSerializer.Serialize(new[] { entry.TimeEntryId }, JsonOptions),
                    ValidationStatus = mapping is null ? "missing_code_mapping" : "pending",
                });
            }
        }

        db.PayrollBatchLines.AddRange(lineDrafts);
        batch.TotalWorkers = lineDrafts.Select(x => x.PersonId).Distinct().Count();
        batch.TotalHours = lineDrafts.Sum(x => x.DurationMinutes) / 60m;
        batch.TotalGrossEstimate = lineDrafts.Sum(x => x.GrossEstimate ?? 0m);
        await db.SaveChangesAsync(cancellationToken);
        return batch;
    }

    public async Task<PayrollBatch> ValidateBatchAsync(ClaimsPrincipal principal, Guid batchId, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        var batch = await db.PayrollBatches.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == batchId, cancellationToken)
            ?? throw new StlApiException("ledgarr.payroll.batch_not_found", "Payroll batch was not found.", 404);
        var lines = await db.PayrollBatchLines.Where(x => x.TenantId == tenantId && x.PayrollBatchId == batchId).ToListAsync(cancellationToken);

        var hasFailures = false;
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line.ProviderEarningCode))
            {
                line.ValidationStatus = "missing_provider_earning_code";
                hasFailures = true;
                continue;
            }

            if (await db.PayrollExportPackets.AnyAsync(
                    x => x.TenantId == tenantId && x.PayloadHash.Contains(line.SourceTimesheetPeriodRef, StringComparison.Ordinal),
                    cancellationToken))
            {
                line.ValidationStatus = "duplicate_exported_time_entry";
                hasFailures = true;
                continue;
            }

            line.ValidationStatus = "valid";
        }

        batch.Status = hasFailures ? "validation_failed" : "ready_for_review";
        batch.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return batch;
    }

    public async Task<PayrollBatch> ApproveForExportAsync(ClaimsPrincipal principal, Guid batchId, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        var batch = await db.PayrollBatches.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == batchId, cancellationToken)
            ?? throw new StlApiException("ledgarr.payroll.batch_not_found", "Payroll batch was not found.", 404);
        batch.Status = "approved_for_export";
        batch.ApprovedAt = DateTimeOffset.UtcNow;
        batch.ApprovedByPersonId = principal.GetUserId();
        batch.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return batch;
    }

    public async Task<PayrollExportPacket> ExportBatchAsync(ClaimsPrincipal principal, Guid batchId, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        var batch = await db.PayrollBatches.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == batchId, cancellationToken)
            ?? throw new StlApiException("ledgarr.payroll.batch_not_found", "Payroll batch was not found.", 404);
        var lines = await db.PayrollBatchLines.Where(x => x.TenantId == tenantId && x.PayrollBatchId == batchId).OrderBy(x => x.WorkerNumber).ToListAsync(cancellationToken);

        var payload = JsonSerializer.Serialize(new
        {
            batch.Id,
            batch.LegalEntityId,
            batch.PeriodStartDate,
            batch.PeriodEndDate,
            Lines = lines.Select(x => new { x.WorkerNumber, x.PayCodeRef, x.ProviderEarningCode, x.DurationMinutes, x.SourceTimesheetPeriodRef, x.SourceTimeEntryRefs }),
        }, JsonOptions);
        var hash = ComputeHash(payload);

        var existing = await db.PayrollExportPackets.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PayrollBatchId == batchId && x.PayloadHash == hash, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var packet = new PayrollExportPacket
        {
            TenantId = tenantId,
            PayrollBatchId = batchId,
            ProviderKey = batch.ExportProvider,
            ExportFormat = batch.ExportProvider == "generic_csv" ? "csv" : "json",
            FileRef = $"payroll/{batchId}/{hash}.{(batch.ExportProvider == "generic_csv" ? "csv" : "json")}",
            PayloadHash = hash,
            ExportedByPersonId = principal.GetUserId(),
            ProviderResponseStatus = "exported",
            ProviderResponseRef = $"export:{batch.ExportProvider}:{batchId}",
            ReplayProtectionKey = hash,
        };

        db.PayrollExportPackets.Add(packet);
        batch.Status = "exported";
        batch.ExportedAt = DateTimeOffset.UtcNow;
        batch.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return packet;
    }

    public async Task<PayrollBatch> MarkProviderStatusAsync(ClaimsPrincipal principal, Guid batchId, string status, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        var batch = await db.PayrollBatches.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == batchId, cancellationToken)
            ?? throw new StlApiException("ledgarr.payroll.batch_not_found", "Payroll batch was not found.", 404);
        batch.Status = status;
        batch.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return batch;
    }

    public async Task<IReadOnlyList<PayrollJournalSnapshot>> PostJournalAsync(ClaimsPrincipal principal, Guid batchId, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        var batch = await db.PayrollBatches.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == batchId, cancellationToken)
            ?? throw new StlApiException("ledgarr.payroll.batch_not_found", "Payroll batch was not found.", 404);
        var lines = await db.PayrollBatchLines.Where(x => x.TenantId == tenantId && x.PayrollBatchId == batchId && x.ValidationStatus == "valid").ToListAsync(cancellationToken);
        var mappings = await db.PayrollCodeMappings.Where(x => x.TenantId == tenantId && x.LegalEntityId == batch.LegalEntityId).ToDictionaryAsync(x => x.StaffArrPayCodeRef, cancellationToken);

        var existing = await db.PayrollJournalSnapshots.Where(x => x.TenantId == tenantId && x.PayrollBatchId == batchId).ToListAsync(cancellationToken);
        db.PayrollJournalSnapshots.RemoveRange(existing);

        var snapshots = new List<PayrollJournalSnapshot>();
        foreach (var line in lines)
        {
            if (!mappings.TryGetValue(line.PayCodeRef, out var mapping))
            {
                continue;
            }

            snapshots.Add(new PayrollJournalSnapshot
            {
                TenantId = tenantId,
                PayrollBatchId = batchId,
                LegalEntityId = batch.LegalEntityId,
                GlAccountRef = mapping.GlAccountRef,
                CostCenterRef = mapping.CostCenterRef,
                DepartmentRef = mapping.DepartmentRef,
                ProductKey = ExtractProductKey(line.AllocationSnapshot),
                CostObjectType = ExtractCostObjectType(line.AllocationSnapshot),
                CostObjectRef = ExtractCostObjectRef(line.AllocationSnapshot),
                DebitAmount = line.GrossEstimate ?? line.DurationMinutes / 60m,
                CreditAmount = 0m,
                Currency = "USD",
                SourcePayrollBatchLineRefs = JsonSerializer.Serialize(new[] { line.Id }, JsonOptions),
                Status = "posted",
            });
        }

        db.PayrollJournalSnapshots.AddRange(snapshots);
        batch.Status = "posted";
        batch.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return snapshots;
    }

    public async Task<PayrollBatch> CloseBatchAsync(ClaimsPrincipal principal, Guid batchId, CancellationToken cancellationToken) =>
        await MarkProviderStatusAsync(principal, batchId, "closed", cancellationToken);

    public async Task<PayrollBatch> ReopenBatchAsync(ClaimsPrincipal principal, Guid batchId, string? correctionReason, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        var batch = await db.PayrollBatches.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == batchId, cancellationToken)
            ?? throw new StlApiException("ledgarr.payroll.batch_not_found", "Payroll batch was not found.", 404);
        if (batch.ExportedAt.HasValue && string.IsNullOrWhiteSpace(correctionReason))
        {
            throw new StlApiException("ledgarr.payroll.reopen_requires_reason", "Reopening an exported payroll batch requires a correction reason.", 409);
        }

        batch.Status = "reopened";
        batch.CorrectionReason = correctionReason;
        batch.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return batch;
    }

    public async Task<IReadOnlyList<PayrollBatchLine>> ListBatchLinesAsync(ClaimsPrincipal principal, Guid batchId, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        return await db.PayrollBatchLines.Where(x => x.TenantId == tenantId && x.PayrollBatchId == batchId).OrderBy(x => x.WorkerNumber).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PayrollExportPacket>> ListExportPacketsAsync(ClaimsPrincipal principal, Guid batchId, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        return await db.PayrollExportPackets.Where(x => x.TenantId == tenantId && x.PayrollBatchId == batchId).OrderByDescending(x => x.ExportedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PayrollJournalSnapshot>> ListJournalSnapshotsAsync(ClaimsPrincipal principal, Guid batchId, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        return await db.PayrollJournalSnapshots.Where(x => x.TenantId == tenantId && x.PayrollBatchId == batchId).OrderBy(x => x.GlAccountRef).ToListAsync(cancellationToken);
    }

    public async Task<GenericCsvPayrollExportResponse> BuildGenericCsvExportAsync(ClaimsPrincipal principal, Guid batchId, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        var batch = await db.PayrollBatches.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == batchId, cancellationToken)
            ?? throw new StlApiException("ledgarr.payroll.batch_not_found", "Payroll batch was not found.", 404);
        var lines = await ListBatchLinesAsync(principal, batchId, cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine("worker_number,pay_code,provider_earning_code,duration_minutes,source_timesheet_period_ref");
        foreach (var line in lines)
        {
            builder.AppendLine($"{Escape(line.WorkerNumber)},{Escape(line.PayCodeRef)},{Escape(line.ProviderEarningCode)},{line.DurationMinutes},{Escape(line.SourceTimesheetPeriodRef)}");
        }

        return new GenericCsvPayrollExportResponse($"payroll-batch-{batchId}.csv", "text/csv", builder.ToString());
    }

    private Guid EnsureEntitled(ClaimsPrincipal principal)
    {
        if (!principal.HasProductEntitlement("ledgarr"))
        {
            throw new StlApiException("ledgarr.not_entitled", "Active LedgArr entitlement is required.", 403);
        }

        return principal.GetTenantId();
    }

    private static string Require(string? value, string message, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("ledgarr.payroll.validation", message, 400);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException("ledgarr.payroll.validation", $"{message.TrimEnd('.')} must be {maxLength} characters or less.", 400);
        }

        return normalized;
    }

    private static string? Optional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException("ledgarr.payroll.validation", $"Value must be {maxLength} characters or less.", 400);
        }

        return normalized;
    }

    private static string NormalizeEnum(string value, IReadOnlyCollection<string> allowed, string fieldName)
    {
        var normalized = Require(value, $"{fieldName} is required.", 64).ToLowerInvariant();
        if (!allowed.Contains(normalized))
        {
            throw new StlApiException("ledgarr.payroll.validation", $"{fieldName} is invalid.", 400);
        }

        return normalized;
    }

    private static string ComputeHash(string payload)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string Escape(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

    private static string ExtractProductKey(string allocationSnapshot) => ExtractAllocationField(allocationSnapshot, "ProductKey") ?? "staffarr";

    private static string ExtractCostObjectType(string allocationSnapshot) => ExtractAllocationField(allocationSnapshot, "CostObjectType") ?? "timesheet";

    private static string ExtractCostObjectRef(string allocationSnapshot) => ExtractAllocationField(allocationSnapshot, "CostObjectRef") ?? "unassigned";

    private static string? ExtractAllocationField(string allocationSnapshot, string key)
    {
        try
        {
            using var document = JsonDocument.Parse(allocationSnapshot);
            var first = document.RootElement.ValueKind == JsonValueKind.Array && document.RootElement.GetArrayLength() > 0
                ? document.RootElement[0]
                : document.RootElement;
            if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty(key, out var property))
            {
                return property.GetString();
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }
}

public sealed record CreatePayrollCalendarRequest(
    Guid LegalEntityId,
    string Name,
    string Frequency,
    DateOnly PeriodStartDate,
    DateOnly PeriodEndDate,
    DateOnly PayDate,
    DateOnly CutoffDate,
    string Timezone,
    string Status);

public sealed record CreatePayrollCodeMappingRequest(
    Guid LegalEntityId,
    string StaffArrPayCodeRef,
    string? PayrollProviderRef,
    string ProviderEarningCode,
    string? ProviderDeductionCode,
    string GlAccountRef,
    string? CostCenterRef,
    string? DepartmentRef,
    string? TaxableTreatmentSnapshot,
    bool Active,
    DateOnly EffectiveStartDate,
    DateOnly? EffectiveEndDate);

public sealed record CreatePayrollBatchRequest(
    Guid LegalEntityId,
    Guid PayrollCalendarId,
    DateOnly PeriodStartDate,
    DateOnly PeriodEndDate,
    DateOnly? PayDate,
    string? ExportProvider);

public sealed record ReopenPayrollBatchRequest(string? CorrectionReason);
