using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Options;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Http;

namespace NexArr.Api.Services;

public sealed class FieldCompanionProductClient(
    IHttpClientFactory httpClientFactory,
    IOptions<FieldCompanionProductUrlsOptions> options)
{
    public async Task<FieldInboxProductSlice> FetchFieldInboxAsync(
        string productKey,
        bool available,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (!available)
        {
            return new FieldInboxProductSlice(
                productKey,
                Available: false,
                Fetched: false,
                ErrorCode: "not_available",
                ErrorMessage: FieldCompanionDeniedReasonCatalog.ToPlainMessage("not_available"),
                Items: []);
        }

        var baseUrl = ResolveBaseUrl(productKey);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return new FieldInboxProductSlice(
                productKey,
                Available: true,
                Fetched: false,
                ErrorCode: "product_url_missing",
                ErrorMessage: FieldCompanionDeniedReasonCatalog.ToPlainMessage("product_url_missing"),
                Items: []);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl.TrimEnd('/')}/api/field-inbox");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return new FieldInboxProductSlice(
                    productKey,
                    Available: true,
                    Fetched: false,
                    ErrorCode: $"upstream_{(int)response.StatusCode}",
                    ErrorMessage: string.IsNullOrWhiteSpace(body)
                        ? FieldCompanionDeniedReasonCatalog.ToPlainMessage(
                            "upstream_unreachable",
                            $"{productKey} field inbox request failed.")
                        : body,
                    Items: []);
            }

            var inbox = await response.Content.ReadFromJsonAsync<FieldInboxResponse>(cancellationToken);
            return new FieldInboxProductSlice(
                productKey,
                Available: true,
                Fetched: true,
                ErrorCode: null,
                ErrorMessage: null,
                Items: inbox?.Items ?? []);
        }
        catch (HttpRequestException ex)
        {
            return new FieldInboxProductSlice(
                productKey,
                Available: true,
                Fetched: false,
                ErrorCode: "upstream_unreachable",
                ErrorMessage: FieldCompanionDeniedReasonCatalog.ToPlainMessage(
                    "upstream_unreachable",
                    ex.Message),
                Items: []);
        }
    }

    public async Task<FieldCompanionFieldEvidenceResponse> SubmitTrainArrAssignmentEvidenceAsync(
        string accessToken,
        Guid assignmentId,
        string evidenceTypeKey,
        string fileName,
        string contentType,
        string contentBase64,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("trainarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "fieldcompanion.field_evidence.product_url_missing",
                "TrainArr API URL is not configured for fieldcompanion evidence capture.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl.TrimEnd('/')}/api/training-assignments/{assignmentId:D}/evidence");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new TrainArrCreateEvidenceUpstreamRequest(
            evidenceTypeKey,
            fileName,
            contentType,
            contentBase64,
            notes));

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "fieldcompanion.field_evidence.upstream_failed",
                string.IsNullOrWhiteSpace(body) ? "TrainArr evidence upload failed." : body,
                (int)response.StatusCode);
        }

        var created = await response.Content.ReadFromJsonAsync<TrainArrEvidenceUpstreamResponse>(cancellationToken)
            ?? throw new StlApiException(
                "fieldcompanion.field_evidence.upstream_invalid",
                "TrainArr returned an empty evidence response.",
                502);

        return new FieldCompanionFieldEvidenceResponse(
            $"trainarr:assignment:{assignmentId:D}",
            "trainarr",
            created.EvidenceId,
            created.EvidenceTypeKey,
            created.FileName,
            created.ContentType,
            created.SizeBytes,
            created.Notes,
            created.CreatedAt);
    }

    public async Task<FieldCompanionFieldDvirResponse> SubmitRoutArrTripDvirAsync(
        string accessToken,
        Guid tripId,
        string phase,
        string result,
        long? odometerReading,
        string? defectNotes,
        string? vehicleRefKey,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("routarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "fieldcompanion.field_dvir.product_url_missing",
                "RoutArr API URL is not configured for fieldcompanion DVIR capture.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl.TrimEnd('/')}/api/trips/{tripId:D}/dvir");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new RoutArrSubmitTripDvirUpstreamRequest(
            phase,
            vehicleRefKey,
            result,
            odometerReading,
            defectNotes));

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "fieldcompanion.field_dvir.upstream_failed",
                string.IsNullOrWhiteSpace(body) ? "RoutArr DVIR submission failed." : body,
                (int)response.StatusCode);
        }

        var created = await response.Content.ReadFromJsonAsync<RoutArrTripDvirUpstreamResponse>(cancellationToken)
            ?? throw new StlApiException(
                "fieldcompanion.field_dvir.upstream_invalid",
                "RoutArr returned an empty DVIR response.",
                502);

        return new FieldCompanionFieldDvirResponse(
            $"routarr:trip:{tripId:D}",
            "routarr",
            created.DvirId,
            created.TripId,
            created.Phase,
            created.Result,
            created.OdometerReading,
            created.DefectNotes,
            created.SubmittedAt);
    }

    public async Task<StaffArrFieldCompanionClockStatusUpstreamResponse> GetStaffArrFieldCompanionClockStatusAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("staffarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "fieldcompanion.clock.product_url_missing",
                "StaffArr API URL is not configured for fieldcompanion clock actions.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{baseUrl.TrimEnd('/')}/api/v1/timekeeping/fieldcompanion/clock");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "fieldcompanion.clock.upstream_failed",
                string.IsNullOrWhiteSpace(body) ? "StaffArr clock status load failed." : body,
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<StaffArrFieldCompanionClockStatusUpstreamResponse>(cancellationToken)
            ?? throw new StlApiException(
                "fieldcompanion.clock.upstream_invalid",
                "StaffArr returned an empty clock status response.",
                502);
    }

    public async Task<StaffArrFieldCompanionClockSubmissionUpstreamResponse> SubmitStaffArrFieldCompanionClockEventAsync(
        string accessToken,
        StaffArrSubmitFieldCompanionClockEventUpstreamRequest body,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("staffarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "fieldcompanion.clock.product_url_missing",
                "StaffArr API URL is not configured for fieldcompanion clock actions.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl.TrimEnd('/')}/api/v1/timekeeping/fieldcompanion/clock");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(body);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var bodyText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "fieldcompanion.clock.upstream_failed",
                string.IsNullOrWhiteSpace(bodyText) ? "StaffArr clock submission failed." : bodyText,
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<StaffArrFieldCompanionClockSubmissionUpstreamResponse>(cancellationToken)
            ?? throw new StlApiException(
                "fieldcompanion.clock.upstream_invalid",
                "StaffArr returned an empty clock submission response.",
                502);
    }

    public async Task<MaintainArrInspectionRunUpstreamResponse> GetMaintainArrInspectionRunAsync(
        string accessToken,
        Guid inspectionRunId,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("maintainarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "fieldcompanion.field_inspection.product_url_missing",
                "MaintainArr API URL is not configured for fieldcompanion inspection capture.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{baseUrl.TrimEnd('/')}/api/inspections/{inspectionRunId:D}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "fieldcompanion.field_inspection.upstream_failed",
                string.IsNullOrWhiteSpace(body) ? "MaintainArr inspection load failed." : body,
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<MaintainArrInspectionRunUpstreamResponse>(cancellationToken)
            ?? throw new StlApiException(
                "fieldcompanion.field_inspection.upstream_invalid",
                "MaintainArr returned an empty inspection response.",
                502);
    }

    public async Task<MaintainArrInspectionRunUpstreamResponse> SubmitMaintainArrInspectionAnswersAsync(
        string accessToken,
        Guid inspectionRunId,
        IReadOnlyList<FieldCompanionFieldInspectionAnswerInput> answers,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("maintainarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "fieldcompanion.field_inspection.product_url_missing",
                "MaintainArr API URL is not configured for fieldcompanion inspection capture.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"{baseUrl.TrimEnd('/')}/api/inspections/{inspectionRunId:D}/answers");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new MaintainArrSubmitInspectionAnswersUpstreamRequest(
            answers.Select(answer => new MaintainArrInspectionAnswerUpstreamInput(
                answer.ChecklistItemId,
                answer.PassFailValue,
                answer.NumericValue,
                answer.TextValue)).ToList()));

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "fieldcompanion.field_inspection.upstream_failed",
                string.IsNullOrWhiteSpace(body) ? "MaintainArr inspection answer submission failed." : body,
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<MaintainArrInspectionRunUpstreamResponse>(cancellationToken)
            ?? throw new StlApiException(
                "fieldcompanion.field_inspection.upstream_invalid",
                "MaintainArr returned an empty inspection response.",
                502);
    }

    public async Task<MaintainArrInspectionRunUpstreamResponse> CompleteMaintainArrInspectionRunAsync(
        string accessToken,
        Guid inspectionRunId,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("maintainarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "fieldcompanion.field_inspection.product_url_missing",
                "MaintainArr API URL is not configured for fieldcompanion inspection capture.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl.TrimEnd('/')}/api/inspections/{inspectionRunId:D}/complete");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "fieldcompanion.field_inspection.upstream_failed",
                string.IsNullOrWhiteSpace(body) ? "MaintainArr inspection completion failed." : body,
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<MaintainArrInspectionRunUpstreamResponse>(cancellationToken)
            ?? throw new StlApiException(
                "fieldcompanion.field_inspection.upstream_invalid",
                "MaintainArr returned an empty inspection response.",
                502);
    }

    public async Task<MaintainArrWorkOrderDetailUpstreamResponse> GetMaintainArrWorkOrderAsync(
        string accessToken,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("maintainarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "fieldcompanion.field_work_order.product_url_missing",
                "MaintainArr API URL is not configured for fieldcompanion work order capture.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{baseUrl.TrimEnd('/')}/api/work-orders/{workOrderId:D}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "fieldcompanion.field_work_order.upstream_failed",
                string.IsNullOrWhiteSpace(body) ? "MaintainArr work order load failed." : body,
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<MaintainArrWorkOrderDetailUpstreamResponse>(cancellationToken)
            ?? throw new StlApiException(
                "fieldcompanion.field_work_order.upstream_invalid",
                "MaintainArr returned an empty work order response.",
                502);
    }

    public async Task<IReadOnlyList<MaintainArrWorkOrderTaskLineUpstreamResponse>> ListMaintainArrWorkOrderTasksAsync(
        string accessToken,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("maintainarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "fieldcompanion.field_work_order.product_url_missing",
                "MaintainArr API URL is not configured for fieldcompanion work order capture.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{baseUrl.TrimEnd('/')}/api/work-orders/{workOrderId:D}/tasks");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "fieldcompanion.field_work_order.upstream_failed",
                string.IsNullOrWhiteSpace(body) ? "MaintainArr work order task load failed." : body,
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<List<MaintainArrWorkOrderTaskLineUpstreamResponse>>(cancellationToken)
            ?? [];
    }

    public async Task<IReadOnlyList<MaintainArrWorkOrderLaborEntryUpstreamResponse>> ListMaintainArrWorkOrderLaborAsync(
        string accessToken,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("maintainarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "fieldcompanion.field_work_order.product_url_missing",
                "MaintainArr API URL is not configured for fieldcompanion work order capture.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{baseUrl.TrimEnd('/')}/api/work-orders/{workOrderId:D}/labor");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "fieldcompanion.field_work_order.upstream_failed",
                string.IsNullOrWhiteSpace(body) ? "MaintainArr work order labor load failed." : body,
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<List<MaintainArrWorkOrderLaborEntryUpstreamResponse>>(cancellationToken)
            ?? [];
    }

    public async Task<MaintainArrWorkOrderDetailUpstreamResponse> UpdateMaintainArrWorkOrderStatusAsync(
        string accessToken,
        Guid workOrderId,
        string status,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("maintainarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "fieldcompanion.field_work_order.product_url_missing",
                "MaintainArr API URL is not configured for fieldcompanion work order capture.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{baseUrl.TrimEnd('/')}/api/work-orders/{workOrderId:D}/status");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new MaintainArrUpdateWorkOrderStatusUpstreamRequest(status));

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "fieldcompanion.field_work_order.upstream_failed",
                string.IsNullOrWhiteSpace(body) ? "MaintainArr work order status update failed." : body,
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<MaintainArrWorkOrderDetailUpstreamResponse>(cancellationToken)
            ?? throw new StlApiException(
                "fieldcompanion.field_work_order.upstream_invalid",
                "MaintainArr returned an empty work order response.",
                502);
    }

    public async Task<MaintainArrWorkOrderLaborEntryUpstreamResponse> LogMaintainArrWorkOrderLaborAsync(
        string accessToken,
        Guid workOrderId,
        string personId,
        decimal hoursWorked,
        string laborTypeKey,
        Guid? workOrderTaskLineId,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("maintainarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "fieldcompanion.field_work_order.product_url_missing",
                "MaintainArr API URL is not configured for fieldcompanion work order capture.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl.TrimEnd('/')}/api/work-orders/{workOrderId:D}/labor");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new MaintainArrCreateWorkOrderLaborUpstreamRequest(
            personId,
            hoursWorked,
            laborTypeKey,
            workOrderTaskLineId,
            notes));

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "fieldcompanion.field_work_order.upstream_failed",
                string.IsNullOrWhiteSpace(body) ? "MaintainArr work order labor logging failed." : body,
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<MaintainArrWorkOrderLaborEntryUpstreamResponse>(cancellationToken)
            ?? throw new StlApiException(
                "fieldcompanion.field_work_order.upstream_invalid",
                "MaintainArr returned an empty labor response.",
                502);
    }

    public async Task<string> ResolveLoadArrReceivingSessionIdAsync(
        string accessToken,
        string taskKey,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("loadarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "fieldcompanion.field_receiving.product_url_missing",
                "LoadArr API URL is not configured for fieldcompanion receiving capture.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{baseUrl.TrimEnd('/')}/api/field-inbox");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "fieldcompanion.field_receiving.upstream_failed",
                string.IsNullOrWhiteSpace(body) ? "LoadArr field inbox lookup failed." : body,
                (int)response.StatusCode);
        }

        var inbox = await response.Content.ReadFromJsonAsync<FieldInboxResponse>(cancellationToken)
            ?? throw new StlApiException(
                "fieldcompanion.field_receiving.upstream_invalid",
                "LoadArr returned an empty field inbox response.",
                502);

        var match = inbox.Items.FirstOrDefault(item =>
            string.Equals(item.TaskKey, taskKey, StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            throw new StlApiException(
                FieldCompanionFieldValidationReasonCodes.NotInInbox,
                FieldCompanionDeniedReasonCatalog.ToPlainMessage(FieldCompanionFieldValidationReasonCodes.NotInInbox),
                404);
        }

        return ExtractLoadArrReceivingSessionId(match.DeepLinkPath);
    }

    public async Task<LoadArrReceivingSessionUpstreamResponse> GetLoadArrReceivingSessionAsync(
        string accessToken,
        string receivingSessionId,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("loadarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "fieldcompanion.field_receiving.product_url_missing",
                "LoadArr API URL is not configured for fieldcompanion receiving capture.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{baseUrl.TrimEnd('/')}/api/v1/receiving/{Uri.EscapeDataString(receivingSessionId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "fieldcompanion.field_receiving.upstream_failed",
                string.IsNullOrWhiteSpace(body) ? "LoadArr receiving session load failed." : body,
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<LoadArrReceivingSessionUpstreamResponse>(cancellationToken)
            ?? throw new StlApiException(
                "fieldcompanion.field_receiving.upstream_invalid",
                "LoadArr returned an empty receiving session response.",
                502);
    }

    public async Task<LoadArrReceivingCompletionUpstreamResponse> CompleteLoadArrReceivingSessionAsync(
        string accessToken,
        string receivingSessionId,
        CompleteLoadArrReceivingSessionUpstreamRequest body,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("loadarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "fieldcompanion.field_receiving.product_url_missing",
                "LoadArr API URL is not configured for fieldcompanion receiving capture.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(FieldCompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl.TrimEnd('/')}/api/v1/receiving/{Uri.EscapeDataString(receivingSessionId)}/complete");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(body);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var bodyText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "fieldcompanion.field_receiving.upstream_failed",
                string.IsNullOrWhiteSpace(bodyText) ? "LoadArr receiving completion failed." : bodyText,
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<LoadArrReceivingCompletionUpstreamResponse>(cancellationToken)
            ?? throw new StlApiException(
                "fieldcompanion.field_receiving.upstream_invalid",
                "LoadArr returned an empty receiving completion response.",
                502);
    }

    private string ResolveBaseUrl(string productKey)
    {
        var urls = options.Value;
        return productKey switch
        {
            "staffarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.StaffArrBaseUrl),
            "trainarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.TrainArrBaseUrl),
            "maintainarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.MaintainArrBaseUrl),
            "routarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.RoutArrBaseUrl),
            "supplyarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.SupplyArrBaseUrl),
            "loadarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.LoadArrBaseUrl),
            _ => string.Empty,
        };
    }

    private static string ExtractLoadArrReceivingSessionId(string deepLinkPath)
    {
        if (string.IsNullOrWhiteSpace(deepLinkPath))
        {
            throw new StlApiException(
                "fieldcompanion.field_receiving.upstream_invalid",
                "LoadArr field inbox did not include a receiving deep link.",
                502);
        }

        var normalized = deepLinkPath.Trim();
        var pathOnly = normalized.Split('?', 2)[0].TrimEnd('/');
        foreach (var prefix in new[] { "/work/receiving/", "/receiving/" })
        {
            if (!pathOnly.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var remainder = pathOnly[prefix.Length..].Trim('/');
            var sessionId = remainder.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                return sessionId;
            }
        }

        throw new StlApiException(
            "fieldcompanion.field_receiving.upstream_invalid",
            "LoadArr field inbox deep link did not include a receiving session id.",
            502);
    }

    private sealed record TrainArrCreateEvidenceUpstreamRequest(
        string EvidenceTypeKey,
        string FileName,
        string ContentType,
        string ContentBase64,
        string? Notes);

    private sealed record TrainArrEvidenceUpstreamResponse(
        Guid EvidenceId,
        Guid TrainingAssignmentId,
        string EvidenceTypeKey,
        string FileName,
        string ContentType,
        long SizeBytes,
        string? Notes,
        Guid UploadedByUserId,
        DateTimeOffset CreatedAt);

    private sealed record RoutArrSubmitTripDvirUpstreamRequest(
        string Phase,
        string? VehicleRefKey,
        string Result,
        long? OdometerReading,
        string? DefectNotes);

    private sealed record RoutArrTripDvirUpstreamResponse(
        Guid DvirId,
        Guid TripId,
        string Phase,
        string VehicleRefKey,
        string Result,
        long? OdometerReading,
        string DefectNotes,
        string SubmittedByPersonId,
        DateTimeOffset SubmittedAt);

    private sealed record MaintainArrSubmitInspectionAnswersUpstreamRequest(
        IReadOnlyList<MaintainArrInspectionAnswerUpstreamInput> Answers);

    private sealed record MaintainArrInspectionAnswerUpstreamInput(
        Guid ChecklistItemId,
        string? PassFailValue,
        decimal? NumericValue,
        string? TextValue);
}
