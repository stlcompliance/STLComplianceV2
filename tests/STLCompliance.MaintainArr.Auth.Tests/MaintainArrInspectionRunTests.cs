using STLCompliance.Shared.Integration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Services;
using MaintainArrRedeemRequest = MaintainArr.Api.Contracts.RedeemHandoffRequest;
using MaintainArrHandoffSessionResponse = MaintainArr.Api.Contracts.HandoffSessionResponse;
using AssetClassResponse = MaintainArr.Api.Contracts.AssetClassResponse;
using CreateAssetClassRequest = MaintainArr.Api.Contracts.CreateAssetClassRequest;
using AssetTypeResponse = MaintainArr.Api.Contracts.AssetTypeResponse;
using CreateAssetTypeRequest = MaintainArr.Api.Contracts.CreateAssetTypeRequest;
using AssetResponse = MaintainArr.Api.Contracts.AssetResponse;
using CreateAssetRequest = MaintainArr.Api.Contracts.CreateAssetRequest;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrInspectionRunTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private readonly Guid _staffarrSiteOrgUnitId = MaintainArrTestSites.DefaultStaffArrSiteOrgUnitId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"InspectionRunNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"InspectionRunMaintainArr-{Guid.NewGuid():N}";

        _nexarrFactory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(nexArrDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        await SeedNexArrAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var serviceToken = await IssueServiceTokenAsync(adminToken, "maintainarr");

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", serviceToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
            });
        });

        _maintainarrClient = _maintainarrFactory.CreateClient();
        await MaintainArrTestSites.SeedCachedStaffArrSiteAsync(_maintainarrFactory, _staffarrSiteOrgUnitId);
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Inspection_run_happy_path_passes()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, checklistItemId) = await SeedActiveTemplateWithAssetAsync(managerToken);

        var technicianToken = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_technician");

        var startRequest = Authorized(HttpMethod.Post, "/api/inspections", technicianToken);
        startRequest.Content = JsonContent.Create(new StartInspectionRunRequest(assetId, templateId));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;
        Assert.Equal("in_progress", started.Status);
        Assert.Null(started.Result);
        Assert.Single(started.ChecklistItems);

        var submitRequest = Authorized(HttpMethod.Put, $"/api/inspections/{started.InspectionRunId}/answers", technicianToken);
        submitRequest.Content = JsonContent.Create(new SubmitInspectionRunAnswersRequest([
            new InspectionRunAnswerInput(checklistItemId, "pass", null, null),
        ]));
        var submitResponse = await _maintainarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();

        var completeRequest = Authorized(HttpMethod.Post, $"/api/inspections/{started.InspectionRunId}/complete", technicianToken);
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;
        Assert.Equal("completed", completed.Status);
        Assert.Equal("passed", completed.Result);

        var managerListRequest = Authorized(HttpMethod.Get, "/api/inspections", managerToken);
        var managerListResponse = await _maintainarrClient.SendAsync(managerListRequest);
        managerListResponse.EnsureSuccessStatusCode();
        var managerRuns = (await managerListResponse.Content.ReadFromJsonAsync<List<InspectionRunSummaryResponse>>())!;
        Assert.Contains(managerRuns, x => x.InspectionRunId == started.InspectionRunId);

        var technicianListRequest = Authorized(HttpMethod.Get, "/api/inspections", technicianToken);
        var technicianListResponse = await _maintainarrClient.SendAsync(technicianListRequest);
        technicianListResponse.EnsureSuccessStatusCode();
        var technicianRuns = (await technicianListResponse.Content.ReadFromJsonAsync<List<InspectionRunSummaryResponse>>())!;
        Assert.Contains(technicianRuns, x => x.InspectionRunId == started.InspectionRunId);
    }

    [Fact]
    public async Task Inspection_run_can_pause_and_resume_with_history()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, checklistItemId) = await SeedActiveTemplateWithAssetAsync(token);

        var startRequest = Authorized(HttpMethod.Post, "/api/inspections", token);
        startRequest.Content = JsonContent.Create(new StartInspectionRunRequest(assetId, templateId));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        var pauseRequest = Authorized(HttpMethod.Post, $"/api/inspections/{started.InspectionRunId}/pause", token);
        pauseRequest.Content = JsonContent.Create(new PauseInspectionRunRequest("waiting_parts", "Need a replacement filter."));
        var pauseResponse = await _maintainarrClient.SendAsync(pauseRequest);
        pauseResponse.EnsureSuccessStatusCode();
        var paused = (await pauseResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;
        Assert.Equal("paused", paused.Status);
        Assert.Single(paused.PauseEvents);
        Assert.Equal("waiting_parts", paused.PauseEvents[0].Reason);
        Assert.Equal("Need a replacement filter.", paused.PauseEvents[0].Notes);

        var resumeRequest = Authorized(HttpMethod.Post, $"/api/inspections/{started.InspectionRunId}/resume", token);
        resumeRequest.Content = JsonContent.Create(new ResumeInspectionRunRequest("Filter available again."));
        var resumeResponse = await _maintainarrClient.SendAsync(resumeRequest);
        resumeResponse.EnsureSuccessStatusCode();
        var resumed = (await resumeResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;
        Assert.Equal("in_progress", resumed.Status);
        Assert.Single(resumed.PauseEvents);
        Assert.NotNull(resumed.PauseEvents[0].ResumedAt);
        Assert.Equal("Need a replacement filter.", resumed.PauseEvents[0].Notes);
    }

    [Fact]
    public async Task Inspection_run_v1_alias_happy_path_passes()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, checklistItemId) = await SeedActiveTemplateWithAssetAsync(managerToken);

        var technicianToken = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_technician");

        var startRequest = Authorized(HttpMethod.Post, "/api/v1/inspections", technicianToken);
        startRequest.Content = JsonContent.Create(new StartInspectionRunRequest(assetId, templateId));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        var submitRequest = Authorized(HttpMethod.Put, $"/api/v1/inspections/{started.InspectionRunId}/answers", technicianToken);
        submitRequest.Content = JsonContent.Create(new SubmitInspectionRunAnswersRequest([
            new InspectionRunAnswerInput(checklistItemId, "pass", null, null),
        ]));
        var submitResponse = await _maintainarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();

        var completeRequest = Authorized(HttpMethod.Post, $"/api/v1/inspections/{started.InspectionRunId}/complete", technicianToken);
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;
        Assert.Equal("completed", completed.Status);
        Assert.Equal("passed", completed.Result);
    }

    [Fact]
    public async Task Inspection_run_v1_inspection_runs_alias_happy_path_passes()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, checklistItemId) = await SeedActiveTemplateWithAssetAsync(managerToken);
        var technicianToken = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_technician");

        var startRequest = Authorized(HttpMethod.Post, "/api/v1/inspection-runs", technicianToken);
        startRequest.Content = JsonContent.Create(new StartInspectionRunRequest(assetId, templateId));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        var submitRequest = Authorized(HttpMethod.Put, $"/api/v1/inspection-runs/{started.InspectionRunId}/answers", technicianToken);
        submitRequest.Content = JsonContent.Create(new SubmitInspectionRunAnswersRequest([
            new InspectionRunAnswerInput(checklistItemId, "pass", null, null),
        ]));
        (await _maintainarrClient.SendAsync(submitRequest)).EnsureSuccessStatusCode();

        var completeRequest = Authorized(HttpMethod.Post, $"/api/v1/inspection-runs/{started.InspectionRunId}/complete", technicianToken);
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Inspection_run_fail_answer_marks_run_failed()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, checklistItemId) = await SeedActiveTemplateWithAssetAsync(token);

        var startRequest = Authorized(HttpMethod.Post, "/api/inspections", token);
        startRequest.Content = JsonContent.Create(new StartInspectionRunRequest(assetId, templateId));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        var submitRequest = Authorized(HttpMethod.Put, $"/api/inspections/{started.InspectionRunId}/answers", token);
        submitRequest.Content = JsonContent.Create(new SubmitInspectionRunAnswersRequest([
            new InspectionRunAnswerInput(checklistItemId, "fail", null, null),
        ]));
        await _maintainarrClient.SendAsync(submitRequest);

        var completeRequest = Authorized(HttpMethod.Post, $"/api/inspections/{started.InspectionRunId}/complete", token);
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;
        Assert.Equal("failed", completed.Result);
    }

    [Fact]
    public async Task Inspection_run_accepts_yes_no_checklist_items()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, checklistItemId) = await SeedYesNoTemplateWithAssetAsync(token);

        var startRequest = Authorized(HttpMethod.Post, "/api/inspections", token);
        startRequest.Content = JsonContent.Create(new StartInspectionRunRequest(assetId, templateId));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        var submitRequest = Authorized(HttpMethod.Put, $"/api/inspections/{started.InspectionRunId}/answers", token);
        submitRequest.Content = JsonContent.Create(new SubmitInspectionRunAnswersRequest([
            new InspectionRunAnswerInput(checklistItemId, null, null, "yes"),
        ]));
        var submitResponse = await _maintainarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();

        var completeRequest = Authorized(HttpMethod.Post, $"/api/inspections/{started.InspectionRunId}/complete", token);
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        Assert.Equal("completed", completed.Status);
        Assert.Equal("passed", completed.Result);
        Assert.Contains(completed.ChecklistItems, x => x.ChecklistItemId == checklistItemId && x.ItemType == "yes_no");
        Assert.Contains(completed.Answers, x => x.ChecklistItemId == checklistItemId && x.TextValue == "yes");
    }

    [Fact]
    public async Task Inspection_run_accepts_select_and_multi_select_checklist_items()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, selectItemId, multiSelectItemId) = await SeedSelectableTemplateWithAssetAsync(token);

        var startRequest = Authorized(HttpMethod.Post, "/api/inspections", token);
        startRequest.Content = JsonContent.Create(new StartInspectionRunRequest(assetId, templateId));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        Assert.Contains(started.ChecklistItems, x => x.ChecklistItemId == selectItemId && x.ControlledOptions.SequenceEqual(["Open", "Closed"]));
        Assert.Contains(started.ChecklistItems, x => x.ChecklistItemId == multiSelectItemId && x.ControlledOptions.SequenceEqual(["Left", "Right", "Center"]));

        var submitRequest = Authorized(HttpMethod.Put, $"/api/inspections/{started.InspectionRunId}/answers", token);
        submitRequest.Content = JsonContent.Create(new SubmitInspectionRunAnswersRequest([
            new InspectionRunAnswerInput(selectItemId, null, null, null, ["Closed"]),
            new InspectionRunAnswerInput(multiSelectItemId, null, null, null, ["Left", "Center"]),
        ]));
        var submitResponse = await _maintainarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();

        var completeRequest = Authorized(HttpMethod.Post, $"/api/inspections/{started.InspectionRunId}/complete", token);
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        Assert.Equal("completed", completed.Status);
        Assert.Equal("passed", completed.Result);
        Assert.Contains(completed.Answers, x => x.ChecklistItemId == selectItemId && x.SelectedOptions.SequenceEqual(["Closed"]));
        Assert.Contains(completed.Answers, x => x.ChecklistItemId == multiSelectItemId && x.SelectedOptions.SequenceEqual(["Left", "Center"]));
    }

    [Fact]
    public async Task Inspection_run_accepts_meter_reading_checklist_items()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, meterItemId) = await SeedMeterReadingTemplateWithAssetAsync(token);

        var startRequest = Authorized(HttpMethod.Post, "/api/inspections", token);
        startRequest.Content = JsonContent.Create(new StartInspectionRunRequest(assetId, templateId));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        Assert.Contains(started.ChecklistItems, x =>
            x.ChecklistItemId == meterItemId &&
            x.ItemType == "meter_reading" &&
            x.UnitOfMeasure == "hours" &&
            x.AcceptableRangeMin == 100 &&
            x.AcceptableRangeMax == 200);

        var submitRequest = Authorized(HttpMethod.Put, $"/api/inspections/{started.InspectionRunId}/answers", token);
        submitRequest.Content = JsonContent.Create(new SubmitInspectionRunAnswersRequest([
            new InspectionRunAnswerInput(meterItemId, null, 150m, null),
        ]));
        var submitResponse = await _maintainarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();

        var completeRequest = Authorized(HttpMethod.Post, $"/api/inspections/{started.InspectionRunId}/complete", token);
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        Assert.Equal("completed", completed.Status);
        Assert.Equal("passed", completed.Result);
        Assert.Contains(completed.Answers, x =>
            x.ChecklistItemId == meterItemId &&
            x.NumericValue == 150m &&
            x.UnitOfMeasure == "hours");
    }

    [Fact]
    public async Task Inspection_run_accepts_photo_and_signature_evidence_without_text_answers()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, photoItemId, signatureItemId) = await SeedEvidenceOnlyTemplateWithAssetAsync(token);

        var startRequest = Authorized(HttpMethod.Post, "/api/inspections", token);
        startRequest.Content = JsonContent.Create(new StartInspectionRunRequest(assetId, templateId));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        var submitRequest = Authorized(HttpMethod.Put, $"/api/inspections/{started.InspectionRunId}/answers", token);
        submitRequest.Content = JsonContent.Create(new SubmitInspectionRunAnswersRequest([]));
        var submitResponse = await _maintainarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();

        var photoUploadRequest = Authorized(HttpMethod.Post, $"/api/inspections/{started.InspectionRunId}/evidence", token);
        photoUploadRequest.Content = JsonContent.Create(new CreateMaintainArrEvidenceRequest(
            "inspection_photo",
            "data-plate.jpg",
            "image/jpeg",
            Convert.ToBase64String(new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 }),
            "Photo evidence",
            photoItemId));
        var photoUploadResponse = await _maintainarrClient.SendAsync(photoUploadRequest);
        photoUploadResponse.EnsureSuccessStatusCode();

        var signatureUploadRequest = Authorized(HttpMethod.Post, $"/api/inspections/{started.InspectionRunId}/evidence", token);
        signatureUploadRequest.Content = JsonContent.Create(new CreateMaintainArrEvidenceRequest(
            "inspection_signature",
            "signed-slip.png",
            "image/png",
            Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47 }),
            "Signature evidence",
            signatureItemId));
        var signatureUploadResponse = await _maintainarrClient.SendAsync(signatureUploadRequest);
        signatureUploadResponse.EnsureSuccessStatusCode();

        var listEvidenceRequest = Authorized(HttpMethod.Get, $"/api/inspections/{started.InspectionRunId}/evidence", token);
        var listEvidenceResponse = await _maintainarrClient.SendAsync(listEvidenceRequest);
        listEvidenceResponse.EnsureSuccessStatusCode();
        var evidence = (await listEvidenceResponse.Content.ReadFromJsonAsync<List<InspectionRunEvidenceResponse>>())!;
        Assert.Contains(evidence, entry => entry.ChecklistItemId == photoItemId && entry.FileName == "data-plate.jpg");
        Assert.Contains(evidence, entry => entry.ChecklistItemId == signatureItemId && entry.FileName == "signed-slip.png");

        var completeRequest = Authorized(HttpMethod.Post, $"/api/inspections/{started.InspectionRunId}/complete", token);
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        Assert.Equal("completed", completed.Status);
        Assert.Equal("passed", completed.Result);
        Assert.Empty(completed.Answers);
        Assert.Contains(completed.ChecklistItems, x => x.ChecklistItemId == photoItemId && x.ItemType == "photo");
        Assert.Contains(completed.ChecklistItems, x => x.ChecklistItemId == signatureItemId && x.ItemType == "signature");
    }

    [Fact]
    public async Task Technician_cannot_view_other_users_inspection_run()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, checklistItemId) = await SeedActiveTemplateWithAssetAsync(managerToken);

        var technicianToken = CreateMaintainArrAccessToken(
            ["maintainarr"],
            "maintainarr_technician",
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        var otherTechnicianToken = CreateMaintainArrAccessToken(
            ["maintainarr"],
            "maintainarr_technician",
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

        var startRequest = Authorized(HttpMethod.Post, "/api/inspections", technicianToken);
        startRequest.Content = JsonContent.Create(new StartInspectionRunRequest(assetId, templateId));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        var submitRequest = Authorized(HttpMethod.Put, $"/api/inspections/{started.InspectionRunId}/answers", technicianToken);
        submitRequest.Content = JsonContent.Create(new SubmitInspectionRunAnswersRequest([
            new InspectionRunAnswerInput(checklistItemId, "pass", null, null),
        ]));
        await _maintainarrClient.SendAsync(submitRequest);

        var peekRequest = Authorized(HttpMethod.Get, $"/api/inspections/{started.InspectionRunId}", otherTechnicianToken);
        var peekResponse = await _maintainarrClient.SendAsync(peekRequest);
        Assert.Equal(HttpStatusCode.Forbidden, peekResponse.StatusCode);

        var managerPeekRequest = Authorized(HttpMethod.Get, $"/api/inspections/{started.InspectionRunId}", managerToken);
        var managerPeekResponse = await _maintainarrClient.SendAsync(managerPeekRequest);
        managerPeekResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Start_run_requires_active_template()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createTemplateRequest = Authorized(HttpMethod.Post, "/api/inspection-templates", token);
        createTemplateRequest.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            "draft-only",
            "Draft Only",
            string.Empty));
        var createTemplateResponse = await _maintainarrClient.SendAsync(createTemplateRequest);
        createTemplateResponse.EnsureSuccessStatusCode();
        var template = (await createTemplateResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            "RUN-DRAFT-1",
            "Draft Test Asset",
            string.Empty,
            _staffarrSiteOrgUnitId.ToString("D")));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;

        var startRequest = Authorized(HttpMethod.Post, "/api/inspections", token);
        startRequest.Content = JsonContent.Create(new StartInspectionRunRequest(asset.AssetId, template.InspectionTemplateId));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        Assert.Equal(HttpStatusCode.BadRequest, startResponse.StatusCode);
    }

    private async Task<(Guid AssetId, Guid TemplateId, Guid ChecklistItemId)> SeedActiveTemplateWithAssetAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createTemplateRequest = Authorized(HttpMethod.Post, "/api/inspection-templates", token);
        createTemplateRequest.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            "pre-trip",
            "Pre-Trip",
            "Daily pre-trip inspection"));
        var createTemplateResponse = await _maintainarrClient.SendAsync(createTemplateRequest);
        createTemplateResponse.EnsureSuccessStatusCode();
        var template = (await createTemplateResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;

        var createItemRequest = Authorized(
            HttpMethod.Post,
            $"/api/inspection-templates/{template.InspectionTemplateId}/checklist-items",
            token);
        createItemRequest.Content = JsonContent.Create(new CreateInspectionChecklistItemRequest(
            "brakes-ok",
            "Brakes operate correctly",
            null,
            "pass_fail",
            true,
            10,
            null));
        var createItemResponse = await _maintainarrClient.SendAsync(createItemRequest);
        createItemResponse.EnsureSuccessStatusCode();
        var item = (await createItemResponse.Content.ReadFromJsonAsync<InspectionChecklistItemResponse>())!;

        var replaceAssetTypesRequest = Authorized(
            HttpMethod.Put,
            $"/api/inspection-templates/{template.InspectionTemplateId}/asset-types",
            token);
        replaceAssetTypesRequest.Content = JsonContent.Create(new ReplaceInspectionTemplateAssetTypesRequest([assetTypeId]));
        await _maintainarrClient.SendAsync(replaceAssetTypesRequest);

        var activateRequest = Authorized(
            HttpMethod.Patch,
            $"/api/inspection-templates/{template.InspectionTemplateId}/status",
            token);
        activateRequest.Content = JsonContent.Create(new UpdateInspectionTemplateStatusRequest("active"));
        await _maintainarrClient.SendAsync(activateRequest);

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            "RUN-ASSET-1",
            "Inspection Test Asset",
            string.Empty,
            _staffarrSiteOrgUnitId.ToString("D")));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;

        return (asset.AssetId, template.InspectionTemplateId, item.ChecklistItemId);
    }

    private async Task<(Guid AssetId, Guid TemplateId, Guid ChecklistItemId)> SeedYesNoTemplateWithAssetAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createTemplateRequest = Authorized(HttpMethod.Post, "/api/inspection-templates", token);
        createTemplateRequest.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            "yes-no",
            "Yes No",
            "Yes/no inspection template"));
        var createTemplateResponse = await _maintainarrClient.SendAsync(createTemplateRequest);
        createTemplateResponse.EnsureSuccessStatusCode();
        var template = (await createTemplateResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;

        var createItemRequest = Authorized(
            HttpMethod.Post,
            $"/api/inspection-templates/{template.InspectionTemplateId}/checklist-items",
            token);
        createItemRequest.Content = JsonContent.Create(new CreateInspectionChecklistItemRequest(
            "seat-belt-fastened",
            "Seat belt fastened",
            null,
            "yes_no",
            true,
            10,
            null));
        var createItemResponse = await _maintainarrClient.SendAsync(createItemRequest);
        createItemResponse.EnsureSuccessStatusCode();
        var item = (await createItemResponse.Content.ReadFromJsonAsync<InspectionChecklistItemResponse>())!;

        var replaceAssetTypesRequest = Authorized(
            HttpMethod.Put,
            $"/api/inspection-templates/{template.InspectionTemplateId}/asset-types",
            token);
        replaceAssetTypesRequest.Content = JsonContent.Create(new ReplaceInspectionTemplateAssetTypesRequest([assetTypeId]));
        var replaceAssetTypesResponse = await _maintainarrClient.SendAsync(replaceAssetTypesRequest);
        replaceAssetTypesResponse.EnsureSuccessStatusCode();

        var activateRequest = Authorized(
            HttpMethod.Patch,
            $"/api/inspection-templates/{template.InspectionTemplateId}/status",
            token);
        activateRequest.Content = JsonContent.Create(new UpdateInspectionTemplateStatusRequest("active"));
        var activateResponse = await _maintainarrClient.SendAsync(activateRequest);
        activateResponse.EnsureSuccessStatusCode();

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            "RUN-YESNO-1",
            "Yes/No Test Asset",
            string.Empty,
            _staffarrSiteOrgUnitId.ToString("D")));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;

        return (asset.AssetId, template.InspectionTemplateId, item.ChecklistItemId);
    }

    private async Task<(Guid AssetId, Guid TemplateId, Guid SelectItemId, Guid MultiSelectItemId)> SeedSelectableTemplateWithAssetAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createTemplateRequest = Authorized(HttpMethod.Post, "/api/inspection-templates", token);
        createTemplateRequest.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            "choice-template",
            "Choice Template",
            "Selectable inspection template"));
        var createTemplateResponse = await _maintainarrClient.SendAsync(createTemplateRequest);
        createTemplateResponse.EnsureSuccessStatusCode();
        var template = (await createTemplateResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;

        var createSelectItemRequest = Authorized(
            HttpMethod.Post,
            $"/api/inspection-templates/{template.InspectionTemplateId}/checklist-items",
            token);
        createSelectItemRequest.Content = JsonContent.Create(new CreateInspectionChecklistItemRequest(
            "cab-door-position",
            "Cab door position",
            null,
            "select",
            true,
            10,
            null,
            ["Open", "Closed"]));
        var createSelectItemResponse = await _maintainarrClient.SendAsync(createSelectItemRequest);
        createSelectItemResponse.EnsureSuccessStatusCode();
        var selectItem = (await createSelectItemResponse.Content.ReadFromJsonAsync<InspectionChecklistItemResponse>())!;

        var createMultiSelectItemRequest = Authorized(
            HttpMethod.Post,
            $"/api/inspection-templates/{template.InspectionTemplateId}/checklist-items",
            token);
        createMultiSelectItemRequest.Content = JsonContent.Create(new CreateInspectionChecklistItemRequest(
            "light-status",
            "Cab lights are functioning",
            null,
            "multi_select",
            true,
            20,
            null,
            ["Left", "Right", "Center"]));
        var createMultiSelectItemResponse = await _maintainarrClient.SendAsync(createMultiSelectItemRequest);
        createMultiSelectItemResponse.EnsureSuccessStatusCode();
        var multiSelectItem = (await createMultiSelectItemResponse.Content.ReadFromJsonAsync<InspectionChecklistItemResponse>())!;

        var replaceAssetTypesRequest = Authorized(
            HttpMethod.Put,
            $"/api/inspection-templates/{template.InspectionTemplateId}/asset-types",
            token);
        replaceAssetTypesRequest.Content = JsonContent.Create(new ReplaceInspectionTemplateAssetTypesRequest([assetTypeId]));
        var replaceAssetTypesResponse = await _maintainarrClient.SendAsync(replaceAssetTypesRequest);
        replaceAssetTypesResponse.EnsureSuccessStatusCode();

        var activateRequest = Authorized(
            HttpMethod.Patch,
            $"/api/inspection-templates/{template.InspectionTemplateId}/status",
            token);
        activateRequest.Content = JsonContent.Create(new UpdateInspectionTemplateStatusRequest("active"));
        var activateResponse = await _maintainarrClient.SendAsync(activateRequest);
        activateResponse.EnsureSuccessStatusCode();

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            "RUN-CHOICE-1",
            "Choice Test Asset",
            string.Empty,
            _staffarrSiteOrgUnitId.ToString("D")));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;

        return (asset.AssetId, template.InspectionTemplateId, selectItem.ChecklistItemId, multiSelectItem.ChecklistItemId);
    }

    private async Task<(Guid AssetId, Guid TemplateId, Guid MeterItemId)> SeedMeterReadingTemplateWithAssetAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createTemplateRequest = Authorized(HttpMethod.Post, "/api/inspection-templates", token);
        createTemplateRequest.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            "meter-readings",
            "Meter Readings",
            "Meter reading inspection template"));
        var createTemplateResponse = await _maintainarrClient.SendAsync(createTemplateRequest);
        createTemplateResponse.EnsureSuccessStatusCode();
        var template = (await createTemplateResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;

        var createMeterItemRequest = Authorized(
            HttpMethod.Post,
            $"/api/inspection-templates/{template.InspectionTemplateId}/checklist-items",
            token);
        createMeterItemRequest.Content = JsonContent.Create(new CreateInspectionChecklistItemRequest(
            "engine-hours",
            "Engine hour meter",
            null,
            "meter_reading",
            true,
            10,
            null,
            null,
            100m,
            200m,
            "hours"));
        var createMeterItemResponse = await _maintainarrClient.SendAsync(createMeterItemRequest);
        createMeterItemResponse.EnsureSuccessStatusCode();
        var meterItem = (await createMeterItemResponse.Content.ReadFromJsonAsync<InspectionChecklistItemResponse>())!;

        var replaceAssetTypesRequest = Authorized(
            HttpMethod.Put,
            $"/api/inspection-templates/{template.InspectionTemplateId}/asset-types",
            token);
        replaceAssetTypesRequest.Content = JsonContent.Create(new ReplaceInspectionTemplateAssetTypesRequest([assetTypeId]));
        var replaceAssetTypesResponse = await _maintainarrClient.SendAsync(replaceAssetTypesRequest);
        replaceAssetTypesResponse.EnsureSuccessStatusCode();

        var activateRequest = Authorized(
            HttpMethod.Patch,
            $"/api/inspection-templates/{template.InspectionTemplateId}/status",
            token);
        activateRequest.Content = JsonContent.Create(new UpdateInspectionTemplateStatusRequest("active"));
        var activateResponse = await _maintainarrClient.SendAsync(activateRequest);
        activateResponse.EnsureSuccessStatusCode();

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            "RUN-METER-1",
            "Meter Test Asset",
            string.Empty,
            _staffarrSiteOrgUnitId.ToString("D")));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;

        return (asset.AssetId, template.InspectionTemplateId, meterItem.ChecklistItemId);
    }

    private async Task<(Guid AssetId, Guid TemplateId, Guid PhotoItemId, Guid SignatureItemId)> SeedEvidenceOnlyTemplateWithAssetAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createTemplateRequest = Authorized(HttpMethod.Post, "/api/inspection-templates", token);
        createTemplateRequest.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            "evidence-only",
            "Evidence Only",
            "Photo and signature inspection template"));
        var createTemplateResponse = await _maintainarrClient.SendAsync(createTemplateRequest);
        createTemplateResponse.EnsureSuccessStatusCode();
        var template = (await createTemplateResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;

        var createPhotoItemRequest = Authorized(
            HttpMethod.Post,
            $"/api/inspection-templates/{template.InspectionTemplateId}/checklist-items",
            token);
        createPhotoItemRequest.Content = JsonContent.Create(new CreateInspectionChecklistItemRequest(
            "data-plate-photo",
            "Upload a photo of the data plate",
            null,
            "photo",
            true,
            10,
            null));
        var createPhotoItemResponse = await _maintainarrClient.SendAsync(createPhotoItemRequest);
        createPhotoItemResponse.EnsureSuccessStatusCode();
        var photoItem = (await createPhotoItemResponse.Content.ReadFromJsonAsync<InspectionChecklistItemResponse>())!;

        var createSignatureItemRequest = Authorized(
            HttpMethod.Post,
            $"/api/inspection-templates/{template.InspectionTemplateId}/checklist-items",
            token);
        createSignatureItemRequest.Content = JsonContent.Create(new CreateInspectionChecklistItemRequest(
            "signed-approval",
            "Capture a signature for approval",
            null,
            "signature",
            true,
            20,
            null));
        var createSignatureItemResponse = await _maintainarrClient.SendAsync(createSignatureItemRequest);
        createSignatureItemResponse.EnsureSuccessStatusCode();
        var signatureItem = (await createSignatureItemResponse.Content.ReadFromJsonAsync<InspectionChecklistItemResponse>())!;

        var replaceAssetTypesRequest = Authorized(
            HttpMethod.Put,
            $"/api/inspection-templates/{template.InspectionTemplateId}/asset-types",
            token);
        replaceAssetTypesRequest.Content = JsonContent.Create(new ReplaceInspectionTemplateAssetTypesRequest([assetTypeId]));
        var replaceAssetTypesResponse = await _maintainarrClient.SendAsync(replaceAssetTypesRequest);
        replaceAssetTypesResponse.EnsureSuccessStatusCode();

        var activateRequest = Authorized(
            HttpMethod.Patch,
            $"/api/inspection-templates/{template.InspectionTemplateId}/status",
            token);
        activateRequest.Content = JsonContent.Create(new UpdateInspectionTemplateStatusRequest("active"));
        var activateResponse = await _maintainarrClient.SendAsync(activateRequest);
        activateResponse.EnsureSuccessStatusCode();

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            "RUN-EVIDENCE-1",
            "Evidence Test Asset",
            string.Empty,
            _staffarrSiteOrgUnitId.ToString("D")));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;

        return (asset.AssetId, template.InspectionTemplateId, photoItem.ChecklistItemId, signatureItem.ChecklistItemId);
    }

    private async Task<Guid> SeedAssetTypeAsync(string token)
    {
        var createClassRequest = Authorized(HttpMethod.Post, "/api/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            "vehicles",
            "Vehicles",
            string.Empty));
        var createClassResponse = await _maintainarrClient.SendAsync(createClassRequest);
        createClassResponse.EnsureSuccessStatusCode();
        var assetClass = (await createClassResponse.Content.ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            "forklift",
            "Forklift",
            string.Empty));
        var createTypeResponse = await _maintainarrClient.SendAsync(createTypeRequest);
        createTypeResponse.EnsureSuccessStatusCode();
        var assetType = (await createTypeResponse.Content.ReadFromJsonAsync<AssetTypeResponse>())!;
        return assetType.AssetTypeId;
    }

    private async Task<string> RedeemMaintainArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _maintainarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new MaintainArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<MaintainArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<string> CreateHandoffAsync()
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        request.Content = JsonContent.Create(new NexArr.Api.Contracts.CreateHandoffRequest(
            "maintainarr",
            "http://localhost:5178/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<NexArr.Api.Contracts.HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.RegisterServiceClientRequest(
            $"{productKey}-inspection-run-test",
            $"{productKey} Inspection Run Test",
            productKey,
            [productKey]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "launch.redeem",
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private string CreateMaintainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin",
        Guid? userIdOverride = null)
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<MaintainArrTokenService>();
        var userId = userIdOverride
            ?? (tenantRoleKey == "maintainarr_technician"
                ? Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                : PlatformSeeder.DemoAdminUserId);
        var (token, _) = tokenService.CreateAccessToken(
            userId,
            userId,
            tenantRoleKey == "maintainarr_technician" ? "tech@example.com" : PlatformSeeder.DemoAdminEmail,
            tenantRoleKey == "maintainarr_technician" ? "Demo Technician" : "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return token;
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new NexArr.Api.Contracts.LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var login = (await response.Content.ReadFromJsonAsync<NexArr.Api.Contracts.AuthTokenResponse>())!;
        return login.AccessToken;
    }

    private async Task SeedNexArrAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
            .ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
