using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace LedgArr.Api.Services;

public sealed class StaffArrPayrollClientOptions
{
    public const string SectionName = "StaffArr";
    public string BaseUrl { get; set; } = "http://localhost:5102";
    public string? ServiceToken { get; set; }
}

public sealed class PayrollIntegrationClient(HttpClient httpClient, IOptions<StaffArrPayrollClientOptions> options)
{
    public async Task<StaffArrPayrollReadySnapshotResponse> GetPayrollReadySnapshotAsync(Guid tenantId, DateOnly? periodStartDate, DateOnly? periodEndDate, CancellationToken cancellationToken)
    {
        var uri = $"/api/v1/integrations/timekeeping/payroll-ready-snapshot?tenantId={tenantId}";
        if (periodStartDate.HasValue)
        {
            uri += $"&periodStartDate={periodStartDate.Value:yyyy-MM-dd}";
        }

        if (periodEndDate.HasValue)
        {
            uri += $"&periodEndDate={periodEndDate.Value:yyyy-MM-dd}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        if (!string.IsNullOrWhiteSpace(options.Value.ServiceToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ServiceToken);
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<StaffArrPayrollReadySnapshotResponse>(cancellationToken: cancellationToken))
            ?? throw new InvalidOperationException("StaffArr payroll-ready snapshot response was empty.");
    }
}

public sealed record StaffArrPayrollReadySnapshotResponse(
    Guid SnapshotId,
    Guid TenantId,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<StaffArrPayrollReadyTimesheetResponse> Timesheets);

public sealed record StaffArrPayrollReadyTimesheetResponse(
    Guid TimesheetPeriodId,
    Guid PersonId,
    string WorkerNumber,
    string? DefaultLegalEntityRef,
    string PayrollCalendarRef,
    DateOnly PeriodStartDate,
    DateOnly PeriodEndDate,
    string Status,
    DateTimeOffset? PayrollReadyAt,
    string SnapshotHash,
    IReadOnlyList<StaffArrPayrollReadyTimeEntryResponse> Entries);

public sealed record StaffArrPayrollReadyTimeEntryResponse(
    Guid TimeEntryId,
    DateOnly EntryDate,
    int DurationMinutes,
    string Classification,
    string PayCode,
    string PayCodeDisplayName,
    string SourceProductKey,
    string? SourceRef,
    IReadOnlyList<StaffArrLaborAllocationResponse> Allocations);

public sealed record StaffArrLaborAllocationResponse(
    Guid Id,
    decimal AllocationPercent,
    int AllocationMinutes,
    string ProductKey,
    string CostObjectType,
    string CostObjectRef,
    string LegalEntityRef,
    string SiteRef,
    string DepartmentRef,
    string? CustomerRef,
    string? OrderRef,
    string? AssetRef,
    string? WorkOrderRef,
    string? TripRef,
    string? RouteRef,
    string? WarehouseTaskRef,
    string? TrainingSessionRef,
    string? QualityCaseRef,
    string? ProjectRef,
    string? GlDimensionSnapshot);

public sealed record GenericCsvPayrollExportResponse(
    string FileName,
    string ContentType,
    string Csv);
